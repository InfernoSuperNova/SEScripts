using System;

namespace IngameScript.Ship.Components
{
    
    public enum SensorPollFrequency
    {
        Low,        // For targets veeery far away (>10x max weapon range)
        Medium,     // For targets out of immediate gun range (>max weapon range)
        High,       // For targets out of gun range but being tracked by missiles in a non terminal phase
        Realtime    // For targets in gun range or being tracked by missiles in a terminal phase
    }
    public class SensorPolling
    {
        public static int GetFramesBetweenPolls(SensorPollFrequency freq)
        {
            switch (freq)
            {
                case SensorPollFrequency.Low:      return 600;  // 10 seconds
                case SensorPollFrequency.Medium:   return 60;   // 1 second
                case SensorPollFrequency.High:     return 10;   // every 10 frames
                case SensorPollFrequency.Realtime: return 1;    // every frame
                default: return Int32.MaxValue;                 // Fuck you
            }
        }

    }
}