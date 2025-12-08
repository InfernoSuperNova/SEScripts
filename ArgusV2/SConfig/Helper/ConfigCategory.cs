using System;
using System.Collections.Generic;

namespace IngameScript.SConfig.Helper
{
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