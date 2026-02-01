// File: ArgusV2/Helper/FriendlyCustomName.cs

using System;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Helper
{
    /// <summary>
    /// Manages friendly names extracted from or embedded in a block's CustomName.
    /// Follows the pattern: "[Prefix] [FriendlyName]" where the friendly name is extracted from brackets.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
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
        /// <summary>
        /// FriendlyCustomName method.
        /// </summary>
        /// <param name="block">The block parameter.</param>
        /// <param name="prefix">The prefix parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// FriendlyCustomName method.
        /// </summary>
        /// <param name="block">The block parameter.</param>
        /// <param name="prefix">The prefix parameter.</param>
        /// <returns>The result of the operation.</returns>
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
        /// <summary>
        /// Sync method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Sync method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
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
        /// <summary>
        /// SetFriendlyName method.
        /// </summary>
        /// <param name="newName">The newName parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// SetFriendlyName method.
        /// </summary>
        /// <param name="newName">The newName parameter.</param>
        /// <returns>The result of the operation.</returns>
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
        /// <summary>
        /// TryExtractFriendlyName method.
        /// </summary>
        /// <param name="extracted">The extracted parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// TryExtractFriendlyName method.
        /// </summary>
        /// <param name="extracted">The extracted parameter.</param>
        /// <returns>The result of the operation.</returns>
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
        /// <summary>
        /// UpdateBlockCustomName method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// UpdateBlockCustomName method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private void UpdateBlockCustomName()
        {
            _block.CustomName = $"{_prefix} [{_friendlyName}]";
        }
    }
}