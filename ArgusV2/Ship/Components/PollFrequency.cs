using System;

namespace IngameScript.Ship.Components
{
    
    public enum PollFrequency
    {
        Low,        // For targets veeery far away (>10x max weapon range)
        Medium,     // For targets out of immediate gun range (>max weapon range)
        High,       // For targets out of gun range but being tracked by missiles in a non terminal phase
        Realtime    // For targets in gun range or being tracked by missiles in a terminal phase
    }
    public class Polling
    {
        /// <summary>
        /// GetFramesBetweenPolls method.
        /// </summary>
        /// <param name="freq">The freq parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetFramesBetweenPolls method.
        /// </summary>
        /// <param name="freq">The freq parameter.</param>
        /// <returns>The result of the operation.</returns>
        public static int GetFramesBetweenPolls(PollFrequency freq)
        {
            switch (freq)
            {
                case PollFrequency.Low:      return 600;  // 10 seconds
                case PollFrequency.Medium:   return 60;   // 1 second
                case PollFrequency.High:     return 10;   // every 10 frames
                case PollFrequency.Realtime: return 1;    // every frame
                default: return Int32.MaxValue;                 // Fuck you (sanity default case, shouldn't ever actually happen but making it virtually never trigger is probably the best way to catch unintended behavior early)
            }
        }

    }
}