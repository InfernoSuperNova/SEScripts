using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class MissilePattern
    {
        private Dictionary<string, int> _blockPattern;
        private Dictionary<string, int> _currentPattern;
        private bool _hasPattern;

        private bool _patternValid;
        

        public MissilePattern()
        {
            _blockPattern = new Dictionary<string, int>();
            _currentPattern = new Dictionary<string, int>();
            _hasPattern = false;
        }

        /// <summary>
        /// Saves the current block configuration as the expected pattern
        /// </summary>
        /// <summary>
        /// SavePattern method.
        /// </summary>
        /// <param name="blocks">The blocks parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// SavePattern method.
        /// </summary>
        /// <param name="blocks">The blocks parameter.</param>
        /// <returns>The result of the operation.</returns>
        public void SavePattern(List<IMyCubeBlock> blocks)
        {
            _patternValid = MissileEssentials.IsValidMissile(blocks);
            
            if (blocks == null || blocks.Count == 0)
            {
                _blockPattern.Clear();
                _hasPattern = false;
                return;
            }

            CountBlockTypes(blocks, _blockPattern);
            _hasPattern = true;
        }

        /// <summary>
        /// Compares the current block list against the saved pattern
        /// </summary>
        /// <summary>
        /// MatchesPattern method.
        /// </summary>
        /// <param name="blocks">The blocks parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// MatchesPattern method.
        /// </summary>
        /// <param name="blocks">The blocks parameter.</param>
        /// <returns>The result of the operation.</returns>
        public bool MatchesPattern(List<IMyCubeBlock> blocks)
        {
            if (!_hasPattern)
                return false;

            if (blocks == null || blocks.Count == 0)
                return false;

            CountBlockTypes(blocks, _currentPattern);

            bool matches = ComparePatternsEqual(_currentPattern, _blockPattern);

            _currentPattern.Clear();

            return matches;
        }

        private void CountBlockTypes(List<IMyCubeBlock> blocks, Dictionary<string, int> pattern)
        {
            pattern.Clear();

            foreach (var block in blocks)
            {
                if (block == null) continue;

                string blockType = block.BlockDefinition.SubtypeName;

                if (pattern.ContainsKey(blockType))
                {
                    pattern[blockType]++;
                }
                else
                {
                    pattern[blockType] = 1;
                }
            }
        }

        private bool ComparePatternsEqual(Dictionary<string, int> pattern1, Dictionary<string, int> pattern2)
        {
            if (pattern1.Count != pattern2.Count)
                return false;

            foreach (var kvp in pattern2)
            {
                if (!pattern1.ContainsKey(kvp.Key))
                    return false;

                if (pattern1[kvp.Key] != kvp.Value)
                    return false;
            }

            return true;
        }

        public bool HasPattern => _hasPattern;
        
        public bool IsValid => _hasPattern && _patternValid;

        public void ClearPattern()
        {
            _blockPattern.Clear();
            _hasPattern = false;
        }
    }
}