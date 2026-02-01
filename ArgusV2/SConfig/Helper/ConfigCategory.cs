using System;
using System.Collections.Generic;
using IngameScript.Helper.Log;
using IngameScript.SConfig.Database;

namespace IngameScript.SConfig.Helper
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    class ConfigCategory
    {
        private readonly Dictionary<string, object> _values;

        private ConfigCategory(Dictionary<string, object> values)
        {
            _values = values;
        }

        public static ConfigCategory From(Dictionary<string, object> root, string name)
        {
            object obj;

            if (!root.TryGetValue(name, out obj))
            {
                var dict = new Dictionary<string, object>();
                root[name] = new Dwon.Field(dict);
                return new ConfigCategory(dict);
            }

            // Extract the actual dictionary from Field if wrapped
            Dwon.Field field = obj as Dwon.Field;
            Dictionary<string, object> extractedDict = field != null
                ? (field.Obj as Dictionary<string, object>)
                : (obj as Dictionary<string, object>);

            // If extraction failed, create new and wrap
            if (extractedDict == null)
            {
                extractedDict = new Dictionary<string, object>();
                root[name] = new Dwon.Field(extractedDict);
            }
            else if (field == null)
            {
                // If it wasn't wrapped, wrap it now
                root[name] = new Dwon.Field(extractedDict);
            }

            return new ConfigCategory(extractedDict);
        }

        public void SyncEnum<E>(string key, ref E scriptField, string beforeComment = "", string inlineComment = "") where E : struct
        {
            var field = Dwon.GetField(_values, key, EnumLookup.GetName(scriptField));
        /// <summary>
        /// ToString method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// ToString method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
            var enumName = field.Obj.ToString();
        /// <summary>
        /// Log method.
        /// </summary>
        /// <param name="enumName">The enumName parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Log method.
        /// </summary>
        /// <param name="enumName">The enumName parameter.</param>
        /// <returns>The result of the operation.</returns>
            Program.Log(enumName);
            bool debug = enumName == "Info";
        /// <summary>
        /// if method.
        /// </summary>
        /// <param name="debug">The debug parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// if method.
        /// </summary>
        /// <param name="debug">The debug parameter.</param>
        /// <returns>The result of the operation.</returns>
            if (debug) Program.Log(enumName);
            if (debug) Program.Log(scriptField.GetType());
        /// <summary>
        /// TryGetValue method.
        /// </summary>
        /// <param name="enumName">The enumName parameter.</param>
        /// <param name="scriptField">The scriptField parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// TryGetValue method.
        /// </summary>
        /// <param name="enumName">The enumName parameter.</param>
        /// <param name="scriptField">The scriptField parameter.</param>
        /// <returns>The result of the operation.</returns>
            EnumLookup.TryGetValue(enumName, out scriptField);  
        /// <summary>
        /// GetName method.
        /// </summary>
        /// <param name="scriptField">The scriptField parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetName method.
        /// </summary>
        /// <param name="scriptField">The scriptField parameter.</param>
        /// <returns>The result of the operation.</returns>
            enumName = EnumLookup.GetName(scriptField);
        /// <summary>
        /// if method.
        /// </summary>
        /// <param name="debug">The debug parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// if method.
        /// </summary>
        /// <param name="debug">The debug parameter.</param>
        /// <returns>The result of the operation.</returns>
            if (debug) Program.Log(enumName);
            // Determine which comment to use: Existing > Provided > Auto-generated
            if (string.IsNullOrEmpty(field.BeforeComment))
            {
                var isFlags = EnumLookup.IsFlags(typeof(E));
                var join = isFlags ? " | " : ", ";
                var prefix = isFlags ? "BitFlags: " : "Enum: ";
                field.BeforeComment = !string.IsNullOrEmpty(beforeComment)
                    ? beforeComment
                    : $"{prefix}{string.Join(join, EnumLookup.GetNames(typeof(E)))}";
            }

            if (string.IsNullOrEmpty(field.InlineComment))
            {
                field.InlineComment = inlineComment;
            }

            // Update the value and store the reused field
            field.Obj = enumName;
            _values[key] = field;
        }


        public void Sync<T>(string key, ref T field, string beforeComment = "", string inlineComment = "")
        {
            var existingField = Dwon.GetField(_values, key, field);
            var temp = (T)existingField.Obj;

            // Use existing comments if present, otherwise use provided comments
            existingField.BeforeComment = !string.IsNullOrEmpty(existingField.BeforeComment) ? existingField.BeforeComment : beforeComment;
            existingField.InlineComment = !string.IsNullOrEmpty(existingField.InlineComment) ? existingField.InlineComment : inlineComment;

            // Update the value and store the reused field
            existingField.Obj = temp;
            _values[key] = existingField;

            field = temp;
        }
    }
}