using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace IngameScript.Helper
{
    public static class MiniJson
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
            bool _ = false;
            return Serialize(obj, indent, out _);
        }
        
        public static string Serialize(object obj, int indent, out bool skipComma)
        {
            skipComma = false;
            bool _ = false;
            var pad = new string(' ', indent);
            var pad2 = new string(' ', indent + 2);

            if (obj == null) return "null";

            var str = obj as string;
            if (str != null) return "\"" + Escape(str) + "\"";

            if (obj is bool && (bool)obj) return "true";
            if (obj is bool && !(bool)obj) return "false";

            var dict = obj as IDictionary<string, object>;
            if (dict != null)
            {
                var sb = new StringBuilder("{");
                bool first = true;
                bool shouldSkipComma = false;
                foreach (var kv in dict)
                {
                    if (!first && !shouldSkipComma) sb.Append(",");
                    shouldSkipComma = false;
                    sb.Append("\n")
                        .Append(pad2)
                        .Append("\"").Append(Escape(kv.Key)).Append("\": ")
                        .Append(Serialize(kv.Value, indent + 2, out shouldSkipComma));
                    first = false;
                }
                sb.Append("\n").Append(pad).Append("}");
                return sb.ToString();
            }

            var commentObj = obj as Comment;
            if (commentObj != null)
            {
                bool isComplex = commentObj.Obj is IDictionary<string, object> || commentObj.Obj is IEnumerable<object>;
                if (isComplex)
                {
                    var sb = new StringBuilder();
                    sb.Append("\n").Append(pad2).Append(" // ").Append(commentObj.Com);
                    sb.Append("\n").Append(pad2).Append(Serialize(commentObj.Obj, indent + 2, out _));
                    return sb.ToString();
                }
                else
                {
                    skipComma = true;
                    return Serialize(commentObj.Obj, indent, out _) + ",   // " + commentObj.Com;
                }
            }

            var list = obj as IEnumerable<object>;
            if (list != null)
            {
                var sb = new StringBuilder("[");
                bool first = true;
                foreach (var v in list)
                {
                    if (!first) sb.Append(",");
                    sb.Append("\n")
                        .Append(pad2)
                        .Append(Serialize(v, indent + 2, out _));
                    first = false;
                }
                sb.Append("\n").Append(pad).Append("]");
                return sb.ToString();
            }

            if (obj is float) return ((float)obj).ToString("R");
            if (obj is double) return ((double)obj).ToString("R");
            if (obj is int || obj is long || obj is short || obj is byte)
                return obj.ToString();

            return "\"" + Escape(obj.ToString()) + "\"";
        }

        private static string Escape(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}