using System.Collections.Generic;
using System.Linq;
using IngameScript.Helper.Log;
using Color = VRageMath.Color;

namespace IngameScript.Helper
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class TimedLog
    {
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private Dictionary<LogLevel, string> _logColour = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Trace, $"{ColorToHexARGB(Color.Gray)}" },
            { LogLevel.Debug, $"{ColorToHexARGB(Color.DarkSeaGreen)}"},
            { LogLevel.Info, $"{ColorToHexARGB(Color.White)}" },
            { LogLevel.Warning, $"{ColorToHexARGB(Color.Gold)}" },
            { LogLevel.Error, $"{ColorToHexARGB(Color.Red)}" },
            { LogLevel.Critical, $"{ColorToHexARGB(Color.DarkRed)}" },
            { LogLevel.Highlight, $"{ColorToHexARGB(Color.Aquamarine)}"}
        };
        /// <summary>
        /// ColorToHexARGB method.
        /// </summary>
        /// <param name="color">The color parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// ColorToHexARGB method.
        /// </summary>
        /// <param name="color">The color parameter.</param>
        /// <returns>The result of the operation.</returns>
        private static string ColorToHexARGB(Color color)
        {
            return $"[color=#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}]";
        }

        private static string _footer = "[/color]\n";
    
        private class Entry
        {
            internal readonly string Text;
            internal readonly double Timestamp; // seconds since epoch
            internal readonly LogLevel Level;
        /// <summary>
        /// Entry method.
        /// </summary>
        /// <param name="text">The text parameter.</param>
        /// <param name="timestamp">The timestamp parameter.</param>
        /// <param name="level">The level parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Entry method.
        /// </summary>
        /// <param name="text">The text parameter.</param>
        /// <param name="timestamp">The timestamp parameter.</param>
        /// <param name="level">The level parameter.</param>
        /// <returns>The result of the operation.</returns>
            public Entry(string text, double timestamp, LogLevel level)
            {
                Text = text;
                Timestamp = timestamp;
                Level = level;
            }
            
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly double _lifespan; // seconds

        public TimedLog(double lifespanSeconds)
        {
            _lifespan = lifespanSeconds;
        }

        public void Add(string message, LogLevel level)
        {
            if (level < Config.General.LogLevel) return;
            double now = (System.DateTime.UtcNow - new System.DateTime(1970,1,1)).TotalSeconds;
            _entries.Add(new Entry(message, now, level));
        }

        public void Update()
        {
            double now = (System.DateTime.UtcNow - new System.DateTime(1970,1,1)).TotalSeconds;
            _entries.RemoveAll(e => now - e.Timestamp > _lifespan);
        }

        public List<string> GetEntries()
        {
            return _entries.Select(e => e.Text).ToList();
        }

        public override string ToString()
        {
            string value = "";
            foreach (var entry in _entries)
            {
                value += _logColour[entry.Level] + entry.Text + _footer;
            }

            return value;
        }
    }
}