namespace IngameScript.Helper
{
    using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IngameScript.Helper
{
    public static class Dwon
    {
        public class Comment
        {
            public object Obj;
            public string Com;

            public Comment(object obj, string comment)
            {
                Obj = obj;
                Com = comment;
            }
        }

        #region Serializer

        public static string Serialize(object obj, int indent = 0)
        {
            var sb = new StringBuilder();
            Serialize(obj, sb, indent, null);
            return sb.ToString();
        }

        private static void Serialize(object obj, StringBuilder sb, int indent, string key)
        {
            string pad = new string(' ', indent);

            if (obj == null)
            {
                if (key != null) sb.AppendLine(pad + key + " = null");
                return;
            }

            Comment commentObj = obj as Comment;
            if (commentObj != null)
            {
                bool isPrimitive = IsPrimitive(commentObj.Obj);
                if (isPrimitive && key != null)
                {
                    sb.AppendLine(pad + key + " = " + ValueToString(commentObj.Obj) + "   # " + commentObj.Com);
                }
                else
                {
                    if (!string.IsNullOrEmpty(commentObj.Com))
                        sb.AppendLine(pad + "# " + commentObj.Com);
                    Serialize(commentObj.Obj, sb, indent, key);
                }
                return;
            }

            IDictionary<string, object> dict = obj as IDictionary<string, object>;
            if (dict != null)
            {
                if (key != null) sb.AppendLine(pad + key + " = [");
                foreach (var kv in dict)
                {
                    Serialize(kv.Value, sb, indent + 2, kv.Key);
                }
                if (key != null) sb.AppendLine(pad + "]");
                return;
            }

            IEnumerable<object> list = obj as IEnumerable<object>;
            if (list != null)
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
                return;
            }

            if (key != null)
            {
                sb.AppendLine(pad + key + " = " + ValueToString(obj));
            }
        }

        private static string ValueToString(object obj)
        {
            if (obj == null) return "null";
            if (obj is string) return "\"" + Escape((string)obj) + "\"";
            if (obj is bool) return ((bool)obj ? "true" : "false");
            if (obj is float) return ((float)obj).ToString("R");
            if (obj is double) return ((double)obj).ToString("R");
            if (obj is int || obj is long || obj is short || obj is byte) return obj.ToString();
            return "\"" + Escape(obj.ToString()) + "\"";
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
            return ParseValue(dwon, ref idx);
        }

        private static object ParseValue(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);

            if (idx >= s.Length) return null;

            char c = s[idx];

            // Dictionary / object
            if (c == '[')
            {
                idx++; // skip '['
                var dict = new Dictionary<string, object>();
                while (idx < s.Length)
                {
                    SkipWhitespace(s, ref idx);
                    if (idx >= s.Length) break;
                    if (s[idx] == ']') { idx++; break; }

                    string key = ParseKey(s, ref idx);
                    ExpectChar(s, ref idx, '=');
                    object value = ParseValue(s, ref idx);
                    dict[key] = value;
                }
                return dict;
            }

            // Array / list
            if (c == '{')
            {
                idx++; // skip '{'
                var list = new List<object>();
                while (idx < s.Length)
                {
                    SkipWhitespace(s, ref idx);
                    if (idx >= s.Length) break;
                    if (s[idx] == '}') { idx++; break; }
                    object value = ParseValue(s, ref idx);
                    list.Add(value);

                    SkipWhitespace(s, ref idx);
                    if (idx < s.Length && s[idx] == ',') idx++; // optional comma
                }
                return list;
            }

            // Comment
            if (c == '#')
            {
                while (idx < s.Length && s[idx] != '\n') idx++;
                return ParseValue(s, ref idx);
            }

            // Primitive or string
            string token = ParseToken(s, ref idx);
            return ParsePrimitive(token);
        }

        private static string ParseKey(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);
            int start = idx;
            while (idx < s.Length && s[idx] != '=' && s[idx] != '\n' && s[idx] != ' ' && s[idx] != '\r') idx++;
            string key = s.Substring(start, idx - start).Trim();
            return key;
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
                idx++; // skip closing quote
                // skip inline comment
                SkipInlineComment(s, ref idx);
                return str;
            }

            // Unquoted token
            int startToken = idx;
            while (idx < s.Length && s[idx] != '\n' && s[idx] != '#' && s[idx] != ',' && s[idx] != '}' && s[idx] != ']') idx++;
            string token = s.Substring(startToken, idx - startToken).Trim();
            SkipInlineComment(s, ref idx);
            return token;
        }

        private static void SkipInlineComment(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);
            if (idx < s.Length && s[idx] == '#')
            {
                while (idx < s.Length && s[idx] != '\n') idx++;
            }
        }

        private static void SkipWhitespace(string s, ref int idx)
        {
            while (idx < s.Length && (s[idx] == ' ' || s[idx] == '\t' || s[idx] == '\r' || s[idx] == '\n')) idx++;
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

            int i;
            if (int.TryParse(token, out i)) return i;

            float f;
            if (float.TryParse(token, System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out f)) return f;

            return token; // string fallback
        }

        #endregion
    }
}

}