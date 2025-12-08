using System;
using System.Collections.Generic;
using System.Text;
using IngameScript;

namespace IngameScript
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
            if (obj is float) return ((float)obj).ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture);
            if (obj is double) return ((double)obj).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture); 
            // ~10 decimal places, enough for SE precision
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
            var top = new Dictionary<string, object>();
            while (idx < dwon.Length)
            {
                SkipWhitespaceAndComments(dwon, ref idx);
                if (idx >= dwon.Length) break;

                string key = ParseKey(dwon, ref idx);
                Program.Log(key);
                ExpectChar(dwon, ref idx, '=');
                object value = ParseValue(dwon, ref idx);
                string comment = ParseInlineComment(dwon, ref idx);
                if (comment != null && IsPrimitive(value))
                    value = new Comment(value, comment);

                top[key] = value;
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
                // Dictionary / object
                case '[':
                {
                    idx++;
                    var dict = new Dictionary<string, object>();
                    while (true)
                    {
                        SkipWhitespaceAndComments(s, ref idx);
                        if (idx >= s.Length) break;
                        if (s[idx] == ']') { idx++; break; }

                        string key = ParseKey(s, ref idx);
                        ExpectChar(s, ref idx, '=');
                        object value = ParseValue(s, ref idx);

                        // attach inline comment if exists
                        string comment = ParseInlineComment(s, ref idx);
                        if (comment != null && IsPrimitive(value))
                            value = new Comment(value, comment);

                        dict[key] = value;
                    }
                    return dict;
                }
                // Array / list
                case '{':
                {
                    idx++;
                    var list = new List<object>();
                    while (true)
                    {
                        SkipWhitespaceAndComments(s, ref idx);
                        if (idx >= s.Length) break;
                        if (s[idx] == '}') { idx++; break; }

                        object value = ParseValue(s, ref idx);
                        string comment = ParseInlineComment(s, ref idx);
                        if (comment != null && IsPrimitive(value))
                            value = new Comment(value, comment);

                        list.Add(value);

                        SkipWhitespaceAndComments(s, ref idx);
                        if (idx < s.Length && s[idx] == ',') idx++; // optional comma
                    }
                    return list;
                }
            }

            // Primitive or string
            string token = ParseToken(s, ref idx);
            string inlineComment = ParseInlineComment(s, ref idx);
            object primitive = ParsePrimitive(token);
            if (inlineComment != null && IsPrimitive(primitive))
                primitive = new Comment(primitive, inlineComment);
            return primitive;
        }

        private static void SkipWhitespaceAndComments(string s, ref int idx)
        {
            while (idx < s.Length)
            {
                if (s[idx] == ' ' || s[idx] == '\t' || s[idx] == '\r' || s[idx] == '\n')
                {
                    idx++;
                    continue;
                }

                if (s[idx] == '#')
                {
                    while (idx < s.Length && s[idx] != '\n') idx++;
                    continue;
                }
                break;
            }
        }

        private static string ParseKey(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);

            int start = idx;

            while (idx < s.Length)
            {
                char c = s[idx];

                // key ends ONLY at '='
                if (c == '=')
                    break;

                // stop if we hit newline before seeing '=' → malformed key
                if (c == '\n' || c == '\r')
                    throw new Exception("Unexpected newline while reading key");

                idx++;
            }

            if (idx == start)
                throw new Exception($"Empty key at index {idx}");

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
            SkipWhitespace(s, ref idx);
            if (idx < s.Length && s[idx] == '#')
            {
                idx++; // skip #
                int start = idx;
                while (idx < s.Length && s[idx] != '\n' && s[idx] != '\r') idx++;
                return s.Substring(start, idx - start).Trim();
            }
            return null;
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

            double f;
            if (double.TryParse(token, System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out f)) return f;

            return token; // string fallback
        }

        #endregion
        
        // Helper: recursively unwrap all Dwon.Comment objects
        public static void UnwrapAllComments(Dictionary<string, object> dict)
        {
            var keys = new List<string>(dict.Keys);
            foreach (var key in keys)
            {
                var val = dict[key];
                var c = val as Comment;
                var nestedDict = val as Dictionary<string, object>;
                var list = val as List<object>;
                if (c != null)
                    dict[key] = c.Obj;
                
                else if (nestedDict != null)
                    UnwrapAllComments(nestedDict);
                else if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var listItem = list[i];
                        var cl = listItem as Comment;
                        list[i] = cl != null ? cl.Obj : list[i];
                    }
                        
                }
            }
        }

// Helper: safely get value, returns default if missing
        public static T GetValue<T>(Dictionary<string, object> dict, string key, ref string comment, T defaultValue = default(T))
        {
            comment = "";
            object value;
            if (dict.TryGetValue(key, out value))
            {
                // Handle value types and reference types separately
                var temp = value as Comment;
                if (temp != null)
                {
                    value = temp.Obj;
                    comment = temp.Com;
                }
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
            return defaultValue;
        }
    }
    class ConfigCategory
    {
        private readonly Dictionary<string, object> _values;

        private ConfigCategory(Dictionary<string, object> values)
        {
            _values = values;
        }

        public static ConfigCategory From(Dictionary<string, object> root, string name)
        {
            Dwon.UnwrapAllComments(root);

            Dictionary<string, object> dict;
            object obj;

            if (!root.TryGetValue(name, out obj) || (dict = obj as Dictionary<string, object>) == null)
            {
                dict = new Dictionary<string, object>();
                root[name] = dict;
            }

            return new ConfigCategory(dict);
        }

        public void SyncEnum<E>(string key, ref E field, string comment = "") where E : struct
        {
            int temp = Convert.ToInt32(field);  // enum -> int
            temp = Dwon.GetValue(_values, key, ref comment, temp);
            field = (E)Enum.ToObject(typeof(E), temp);

            if (!string.IsNullOrEmpty(comment))
                _values[key] = new Dwon.Comment(temp, comment);
            else
                _values[key] = temp;
        }


        public void Sync<T>(string key, ref T field, string comment = "")
        {
            var temp = Dwon.GetValue(_values, key, ref comment, field);

            if (!string.IsNullOrEmpty(comment))
                _values[key] = new Dwon.Comment(temp, comment);
            else
                _values[key] = temp;

            field = temp;
        }
    }
}
