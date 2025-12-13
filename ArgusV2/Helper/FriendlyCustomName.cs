// File: ArgusV2/Helper/FriendlyCustomName.cs

using System;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Helper
{
    /// <summary>
    /// Manages friendly names extracted from or embedded in a block's CustomName.
    /// Follows the pattern: "[Prefix] [FriendlyName]" where the friendly name is extracted from brackets.
    /// </summary>
    public class FriendlyCustomName
    {
        private readonly IMyTerminalBlock _block;
        private readonly string _prefix;
        private string _friendlyName;

        /// <summary>
        /// Creates a new FriendlyCustomName manager.
        /// </summary>
        /// <param name="block">The block whose CustomName will be managed</param>
        /// <param name="prefix">The prefix to use when setting the CustomName (e.g., "Missile Finder")</param>
        public FriendlyCustomName(IMyTerminalBlock block, string prefix)
        {
            _block = block;
            _prefix = prefix;
            Sync();
        }

        /// <summary>
        /// Gets the friendly name extracted from the block's CustomName or generates a new one.
        /// </summary>
        public string FriendlyName => _friendlyName;

        /// <summary>
        /// Synchronizes the friendly name with the block's CustomName.
        /// Extracts existing friendly name from brackets, or generates and sets a new one.
        /// </summary>
        public void Sync()
        {
            string extracted;
            if (TryExtractFriendlyName(out extracted))
            {
                _friendlyName = extracted;
            }
            else
            {
                _friendlyName = MemorableName.Get();
                UpdateBlockCustomName();
            }
        }

        /// <summary>
        /// Sets a new friendly name and updates the block's CustomName.
        /// </summary>
        public void SetFriendlyName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Friendly name cannot be empty", nameof(newName));

            _friendlyName = newName.Trim();
            UpdateBlockCustomName();
        }

        /// <summary>
        /// Attempts to extract the friendly name from the block's CustomName.
        /// Looks for text between the first '[' and last ']'.
        /// </summary>
        private bool TryExtractFriendlyName(out string extracted)
        {
            extracted = null;

            if (string.IsNullOrEmpty(_block.CustomName) || !_block.CustomName.Contains("["))
                return false;

            int openBracket = _block.CustomName.IndexOf('[');
            int closeBracket = _block.CustomName.LastIndexOf(']');

            if (closeBracket <= openBracket)
                return false;

            extracted = _block.CustomName.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();
            return !string.IsNullOrEmpty(extracted);
        }

        /// <summary>
        /// Updates the block's CustomName with the current friendly name.
        /// Format: "[Prefix] [FriendlyName]"
        /// </summary>
        private void UpdateBlockCustomName()
        {
            _block.CustomName = $"{_prefix} [{_friendlyName}]";
        }
    }
}