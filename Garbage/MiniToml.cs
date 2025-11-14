using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IngameScript.Helper
{
    public static class MiniToml
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
                // Check if the wrapped object is primitive
                bool isPrimitive = commentObj.Obj is string || commentObj.Obj is bool ||
                                   commentObj.Obj is int || commentObj.Obj is long || commentObj.Obj is short ||
                                   commentObj.Obj is byte || commentObj.Obj is float || commentObj.Obj is double;

                if (isPrimitive)
                {
                    // Inline comment with primitive
                    if (key != null)
                    {
                        string valueStr;
                        object v = commentObj.Obj;
                        if (v is string) valueStr = "\"" + Escape((string)v) + "\"";
                        else if (v is bool) valueStr = ((bool)v ? "true" : "false");
                        else if (v is float) valueStr = ((float)v).ToString("R");
                        else if (v is double) valueStr = ((double)v).ToString("R");
                        else valueStr = v.ToString();

                        sb.AppendLine(pad + key + " = " + valueStr + "   # " + commentObj.Com);
                    }
                }
                else
                {
                    // Comment on its own line for complex objects
                    if (!string.IsNullOrEmpty(commentObj.Com))
                        sb.AppendLine(pad + "# " + commentObj.Com);
                    Serialize(commentObj.Obj, sb, indent, key);
                }
                return;
            }

            string str = obj as string;
            if (str != null)
            {
                if (key != null) sb.AppendLine(pad + key + " = \"" + Escape(str) + "\"");
                return;
            }

            if (obj is bool)
            {
                if (key != null) sb.AppendLine(pad + key + " = " + ((bool)obj ? "true" : "false"));
                return;
            }

            if (obj is int || obj is long || obj is short || obj is byte)
            {
                if (key != null) sb.AppendLine(pad + key + " = " + obj.ToString());
                return;
            }

            if (obj is float)
            {
                if (key != null) sb.AppendLine(pad + key + " = " + ((float)obj).ToString("R"));
                return;
            }

            if (obj is double)
            {
                if (key != null) sb.AppendLine(pad + key + " = " + ((double)obj).ToString("R"));
                return;
            }

            IDictionary<string, object> dict = obj as IDictionary<string, object>;
            if (dict != null)
            {
                if (key != null) sb.AppendLine(pad + "[" + key + "]");

                foreach (KeyValuePair<string, object> kv in dict)
                {
                    Serialize(kv.Value, sb, indent + 2, kv.Key);
                }
                return;
            }

            IEnumerable<object> list = obj as IEnumerable<object>;
            if (list != null)
            {
                if (key != null)
                {
                    sb.Append(pad + key + " = [");
                    bool first = true;
                    foreach (object v in list)
                    {
                        if (!first) sb.Append(", ");
                        if (v is string) sb.Append("\"" + Escape((string)v) + "\"");
                        else if (v is bool) sb.Append((bool)v ? "true" : "false");
                        else if (v is float) sb.Append(((float)v).ToString("R"));
                        else if (v is double) sb.Append(((double)v).ToString("R"));
                        else sb.Append(v.ToString());
                        first = false;
                    }
                    sb.AppendLine("]");
                }
                else
                {
                    foreach (object v in list)
                    {
                        Serialize(v, sb, indent, null);
                    }
                }
                return;
            }

            // Fallback: treat as string
            if (key != null) sb.AppendLine(pad + key + " = \"" + Escape(obj.ToString()) + "\"");
        }

        private static string Escape(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
        
        public static IDictionary<string, object> Parse(string toml)
        {
            var result = new Dictionary<string, object>();
            IDictionary<string, object> currentTable = result;

            string[] lines = toml.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0) continue; // skip empty lines
                if (line.StartsWith("#")) continue; // skip standalone comments

                // Table header
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string tableName = line.Substring(1, line.Length - 2).Trim();
                    string[] parts = tableName.Split('.');
                    currentTable = result;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!currentTable.ContainsKey(parts[i]))
                        {
                            currentTable[parts[i]] = new Dictionary<string, object>();
                        }
                        currentTable = currentTable[parts[i]] as IDictionary<string, object>;
                    }
                    continue;
                }

                // Key = value
                int eq = line.IndexOf('=');
                if (eq < 0) continue;

                string key = line.Substring(0, eq).Trim();
                string valueRaw = line.Substring(eq + 1).Trim();

                // Check for inline comment
                string comment = null;
                int hash = valueRaw.IndexOf('#');
                if (hash >= 0)
                {
                    comment = valueRaw.Substring(hash + 1).Trim();
                    valueRaw = valueRaw.Substring(0, hash).Trim();
                }

                object value = ParseValue(valueRaw);
                if (comment != null && IsPrimitive(value))
                {
                    value = new MiniToml.Comment(value, comment);
                }

                currentTable[key] = value;
            }

            return result;
        }

        private static bool IsPrimitive(object value)
        {
            return value is string || value is bool ||
                   value is int || value is long || value is short || value is byte ||
                   value is float || value is double;
        }

        private static object ParseValue(string s)
        {
            if (s.Length == 0) return null;

            // String
            if ((s.StartsWith("\"") && s.EndsWith("\"")) || (s.StartsWith("'") && s.EndsWith("'")))
            {
                return s.Substring(1, s.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
            }

            // Boolean
            if (s == "true") return true;
            if (s == "false") return false;

            // Array
            if (s.StartsWith("[") && s.EndsWith("]"))
            {
                string inner = s.Substring(1, s.Length - 2).Trim();
                var list = new List<object>();
                if (inner.Length == 0) return list;

                string[] parts = inner.Split(',');
                foreach (string p in parts)
                {
                    list.Add(ParseValue(p.Trim()));
                }
                return list;
            }

            // Number
            double d;
            if (double.TryParse(s, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out d))
            {
                if (d % 1 == 0) return (int)d;
                return d;
            }

            // Fallback
            return s;
        }
    }
}
