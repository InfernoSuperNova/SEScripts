using System;
using System.Collections.Generic;
using System.Text;
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.TruncationWrappers;

namespace IngameScript.SConfig.Helper
{
    public static class Dwon
    {
        /// <summary>
        /// Represents a field in the DWON object tree with optional before/inline comments
        /// </summary>
        public class Field
        {
            public object Obj;
            public string BeforeComment;
            public string InlineComment;

            public Field(object obj, string beforeComment = "", string inlineComment = "")
            {
                Obj = obj;
                BeforeComment = beforeComment ?? "";
                InlineComment = inlineComment ?? "";
            }
        }

        #region Serializer

        public static string Serialize(object obj, int indent = -1)
        {
            var sb = new StringBuilder();
            Serialize(obj, sb, indent, null);
            Program.LogLine("Serialized successfully", LogLevel.Debug);
            return sb.ToString();
        }

        private static void Serialize(object obj, StringBuilder sb, int indent, string key)
        {
            string pad = new string(' ', Math.Max(indent, 0));

            if (obj == null)
            {
                if (key != null) sb.AppendLine(pad + key + " = null");
                return;
            }

            Field fieldObj = obj as Field;
            if (fieldObj != null)
            {
                SerializeField(fieldObj, sb, indent, key);
                return;
            }

            IDictionary<string, object> dict = obj as IDictionary<string, object>;
            if (dict != null)
            {
                SerializeDictionary(dict, sb, indent, key, pad);
                return;
            }

            IEnumerable<object> list = obj as IEnumerable<object>;
            if (list != null)
            {
                SerializeList(list, sb, indent, key, pad);
                return;
            }

            if (key != null)
            {
                sb.AppendLine(pad + key + " = " + ValueToString(obj));
            }
        }

        private static void SerializeField(Field fieldObj, StringBuilder sb, int indent, string key)
        {
            string pad = new string(' ', Math.Max(indent, 0));
            
            // Write before comment on its own line
            if (!string.IsNullOrEmpty(fieldObj.BeforeComment))
                sb.AppendLine(pad + "    # " + fieldObj.BeforeComment);
            
            // Serialize the actual value
            if (key != null && IsPrimitive(fieldObj.Obj))
            {
                // For primitives, write inline
                sb.Append(pad + key + " = " + ValueToString(fieldObj.Obj));
                
                // Add inline comment if present
                if (!string.IsNullOrEmpty(fieldObj.InlineComment))
                    sb.Append("       # " + fieldObj.InlineComment);
                
                sb.AppendLine();
            }
            else
            {
                // For complex objects, just serialize normally
                Serialize(fieldObj.Obj, sb, indent, key);
            }
        }

        private static void SerializeDictionary(IDictionary<string, object> dict, StringBuilder sb, int indent, string key, string pad)
        {
            if (key != null) sb.AppendLine(pad + key + " = [");
            foreach (var kv in dict)
            {
                Serialize(kv.Value, sb, indent + 2, kv.Key);
            }
            if (key != null) sb.AppendLine(pad + "]");
        }

        private static void SerializeList(IEnumerable<object> list, StringBuilder sb, int indent, string key, string pad)
        {
            if (key != null)
            {
                sb.Append(pad + key + " = { ");
                bool first = true;
                foreach (object v in list)
                {
                    if (!first) sb.Append(", ");
                    sb.Append(ValueToString(v));
                    first = false;
                }
                sb.AppendLine(" }");
            }
            else
            {
                foreach (object v in list)
                    Serialize(v, sb, indent, null);
            }
        }

        private static string ValueToString(object obj)
        {
            if (obj == null) return "null";
            if (obj is AT_Vector3D) return FormatVec3((AT_Vector3D)obj);
            if (obj is string)
            {
                string str = (string)obj;
                // Check if string looks like an unquoted identifier or enum value
                if (IsUnquotedIdentifier(str))
                    return str;
                return "\"" + Escape(str) + "\"";
            }
            if (obj is bool) return ((bool)obj ? "true" : "false");
            if (obj is float) return ((float)obj).ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture);
            if (obj is double) return ((double)obj).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
            if (obj is int || obj is long || obj is short || obj is byte) return obj.ToString();
            return "\"" + Escape(obj.ToString()) + "\"";
        }

        private static bool IsUnquotedIdentifier(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            // Allow alphanumeric, underscore, pipe, colon, and spaces (for enum flags and labels)
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '|' && c != ' ' && c != ':')
                    return false;
            }
            return true;
        }

        private static string FormatVec3(AT_Vector3D v)
        {
            return string.Format("Vec3({0} {1} {2})",
                v.X.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                v.Y.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                v.Z.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        }


        private static string Escape(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static bool IsPrimitive(object value)
        {
            return value is string || value is bool ||
                   value is int || value is long || value is short || value is byte ||
                   value is float || value is double;
        }

        #endregion

        #region Parser

        public static object Parse(string dwon)
        {
            int idx = 0;
            var top = new Dictionary<string, object>();
            while (idx < dwon.Length)
            {
                string precedingComment = SkipWhitespaceAndCaptureComments(dwon, ref idx);
                if (idx >= dwon.Length) break;

                string key = ParseKey(dwon, ref idx);
                ExpectChar(dwon, ref idx, '=');
                object value = ParseValue(dwon, ref idx);
                string inlineComment = ParseInlineComment(dwon, ref idx);

                // Always wrap in Field object
                top[key] = new Field(value, precedingComment ?? "", inlineComment ?? "");
            }
            return top;
        }

        private static object ParseValue(string s, ref int idx)
        {
            SkipWhitespaceAndComments(s, ref idx);

            if (idx >= s.Length) return null;

            char c = s[idx];

            switch (c)
            {
                case '[':
                    return ParseDictionary(s, ref idx);
                case '{':
                    return ParseList(s, ref idx);
                default:
                    return ParsePrimitiveValue(s, ref idx);
            }
        }

        private static object ParseDictionary(string s, ref int idx)
        {
            idx++; // skip '['
            var dict = new Dictionary<string, object>();

            while (true)
            {
                string precedingComment = SkipWhitespaceAndCaptureComments(s, ref idx);
                if (idx >= s.Length) break;
                if (s[idx] == ']') { idx++; break; }

                string key = ParseKey(s, ref idx);
                ExpectChar(s, ref idx, '=');
                object value = ParseValue(s, ref idx);
                string inlineComment = ParseInlineComment(s, ref idx);

                // Always wrap in Field object
                dict[key] = new Field(value, precedingComment ?? "", inlineComment ?? "");
            }

            return dict;
        }

        private static object ParseList(string s, ref int idx)
        {
            idx++; // skip '{'
            var list = new List<object>();

            while (true)
            {
                string precedingComment = SkipWhitespaceAndCaptureComments(s, ref idx);
                if (idx >= s.Length) break;
                if (s[idx] == '}') { idx++; break; }

                object value = ParseValue(s, ref idx);
                string inlineComment = ParseInlineComment(s, ref idx);

                // Always wrap in Field object
                list.Add(new Field(value, precedingComment ?? "", inlineComment ?? ""));

                SkipWhitespaceAndComments(s, ref idx);
                if (idx < s.Length && s[idx] == ',') idx++; // optional comma
            }

            return list;
        }

        private static object ParsePrimitiveValue(string s, ref int idx)
        {
            string token = ParseToken(s, ref idx);
            return ParsePrimitive(token);
        }

        private static void SkipWhitespaceAndComments(string s, ref int idx)
        {
            while (idx < s.Length)
            {
                if (IsWhitespace(s[idx]))
                {
                    idx++;
                    continue;
                }

                if (s[idx] == '#')
                {
                    SkipToEndOfLine(s, ref idx);
                    continue;
                }
                break;
            }
        }

        private static string SkipWhitespaceAndCaptureComments(string s, ref int idx)
        {
            string lastComment = null;
            while (idx < s.Length)
            {
                if (IsWhitespace(s[idx]))
                {
                    idx++;
                    continue;
                }

                if (s[idx] == '#')
                {
                    lastComment = CaptureComment(s, ref idx);
                    continue;
                }
                break;
            }
            return lastComment;
        }

        private static string CaptureComment(string s, ref int idx)
        {
            idx++; // skip #
            int start = idx;
            while (idx < s.Length && !IsNewline(s[idx])) idx++;
            return s.Substring(start, idx - start).Trim();
        }

        private static void SkipToEndOfLine(string s, ref int idx)
        {
            while (idx < s.Length && s[idx] != '\n') idx++;
        }

        private static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        private static bool IsNewline(char c)
        {
            return c == '\n' || c == '\r';
        }

        private static string ParseKey(string s, ref int idx)
        {
            try
            {
                SkipWhitespace(s, ref idx);

                int start = idx;

                while (idx < s.Length)
                {
                   
                    char c = s[idx];
                    if (c == '=') break;
                    if (IsNewline(c))
                        throw new Exception("Unexpected newline while reading key");

                    idx++;
                }

                if (idx == start)
                    throw new Exception($"Empty key at index {idx}");

                return s.Substring(start, idx - start).Trim();
            }
            catch (Exception ex)
            {
                throw new Exception($"DWON: ParseKey failed with string ({s}). EX: {ex}");
            }
            
        }

        private static string ParseToken(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);
            if (idx >= s.Length) return "";

            char c = s[idx];

            // Quoted string
            if (c == '"' || c == '\'')
            {
                char quote = c;
                idx++;
                int start = idx;
                while (idx < s.Length && s[idx] != quote) idx++;
                string str = s.Substring(start, idx - start);
                if (idx < s.Length) idx++; // skip closing quote
                return str;
            }

            // Unquoted token
            int startToken = idx;
            while (idx < s.Length && s[idx] != '\n' && s[idx] != '\r' && s[idx] != '#' && s[idx] != ',' && s[idx] != '}' && s[idx] != ']') idx++;
            string token = s.Substring(startToken, idx - startToken).Trim();
            return token;
        }

        private static string ParseInlineComment(string s, ref int idx)
        {
            SkipNonNewlineWhitespace(s, ref idx);
            if (idx < s.Length && s[idx] == '#')
            {
                return CaptureComment(s, ref idx);
            }
            return null;
        }
        
        private static void SkipNonNewlineWhitespace(string s, ref int idx)
        {
            while (idx < s.Length && IsWhitespace(s[idx]) && s[idx] != '\n' && s[idx] != '\r') idx++;
        }

        private static void SkipWhitespace(string s, ref int idx)
        {
            while (idx < s.Length && IsWhitespace(s[idx])) idx++;
        }

        private static void ExpectChar(string s, ref int idx, char expected)
        {
            SkipWhitespace(s, ref idx);
            if (idx >= s.Length || s[idx] != expected) throw new Exception("Expected '" + expected + "' at index " + idx);
            idx++;
        }

        private static object ParsePrimitive(string token)
        {
            if (token.Length == 0) return null;

            if (token == "true") return true;
            if (token == "false") return false;

            object vec3 = TryParseVec3(token);
            if (vec3 != null) return vec3;

            int i;
            if (int.TryParse(token, out i)) return i;

            double f;
            if (double.TryParse(token, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out f)) return f;

            return token; // fallback to string
        }

        private static object TryParseVec3(string token)
        {
            if (!token.StartsWith("Vec3(") || !token.EndsWith(")"))
                return null;

            string inside = token.Substring(5, token.Length - 6);
            string[] parts = inside.Split(' ');

            if (parts.Length != 3)
                return null;

            double x, y, z;
            if (double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z))
            {
                return new AT_Vector3D(x, y, z);
            }

            return null;
        }



        #endregion
        
        // Helper: recursively unwrap all Dwon.Comment objects
        public static void UnwrapAllComments(Dictionary<string, object> dict)
        {
            var keys = new List<string>(dict.Keys);
            foreach (var key in keys)
            {
                dict[key] = UnwrapValue(dict[key]);
            }
        }

        private static object UnwrapValue(object val)
        {
            // Unwrap Field to get the actual value
            Field field = val as Field;
            if (field != null)
                val = field.Obj;

            // Recursively unwrap nested structures
            Dictionary<string, object> nestedDict = val as Dictionary<string, object>;
            if (nestedDict != null)
            {
                UnwrapAllComments(nestedDict);
                return nestedDict;
            }

            List<object> list = val as List<object>;
            if (list != null)
            {
                UnwrapList(list);
                return list;
            }

            return val;
        }

        private static void UnwrapList(List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Field field = list[i] as Field;
                if (field != null)
                {
                    list[i] = field.Obj;
                }
            }
        }

        // Helper: safely get field, returns Field with default value if missing
        public static Field GetField<T>(Dictionary<string, object> dict, string key, T defaultValue = default(T))
        {
            object value;
            if (!dict.TryGetValue(key, out value))
            {
                return new Field(defaultValue);
            }

            // Extract Field if present
            Field field = value as Field;
            if (field != null)
            {
                // Convert the wrapped value to the correct type
                T convertedValue = ConvertValue<T>(field.Obj, defaultValue);
                return new Field(convertedValue, field.BeforeComment, field.InlineComment);
            }
            else
            {
                // Not wrapped in Field, convert directly
                T convertedValue = ConvertValue<T>(value, defaultValue);
                return new Field(convertedValue);
            }
        }

        // Helper: safely get value, returns default if missing (legacy method for backward compatibility)
        public static T GetValue<T>(Dictionary<string, object> dict, string key, ref string comment, T defaultValue = default(T))
        {
            Field field = GetField(dict, key, defaultValue);
            comment = field.BeforeComment;
            return ConvertValue<T>(field.Obj, defaultValue);
        }
        
        private static T ConvertValue<T>(object value, T defaultValue)
        {
            if (value is T) return (T)value;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
