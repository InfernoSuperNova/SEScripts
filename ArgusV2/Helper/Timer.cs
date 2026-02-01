using System;
using System.Collections.Generic;

namespace IngameScript
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class SimpleTimer
    {
        public int Remaining;
        public Action OnComplete;
    }

    public static class TimerManager
    {
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private static readonly Dictionary<int, SimpleTimer> Timers = new Dictionary<int, SimpleTimer>();
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private static readonly Dictionary<int, SimpleTimer> _pendingAdditions = new Dictionary<int, SimpleTimer>();
        private static int _nextId = 1;

        /// <summary>
        /// Starts a timer for the given duration and optional callback.
        /// Returns a unique timer ID that is never reused.
        /// </summary>
        /// <summary>
        /// Start method.
        /// </summary>
        /// <param name="duration">The duration parameter.</param>
        /// <param name="null">The null parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Start method.
        /// </summary>
        /// <param name="duration">The duration parameter.</param>
        /// <param name="null">The null parameter.</param>
        /// <returns>The result of the operation.</returns>
        public static int Start(int duration, Action onComplete = null)
        {
            if (duration <= 0)
            {
                if (onComplete != null) onComplete();
                return -1;
            }

            int id = _nextId++;
            var timer = new SimpleTimer
            {
                Remaining = duration,
                OnComplete = onComplete
            };

            // Queue addition so TickAll can safely insert at the start
            _pendingAdditions[id] = timer;
            return id;
        }

        public static int StartSeconds(float duration, Action onComplete = null)
        {
            return Start((int)(duration * 60), onComplete);
        }

        /// <summary>
        /// Cancels a timer immediately.
        /// </summary>
        /// <summary>
        /// Cancel method.
        /// </summary>
        /// <param name="id">The id parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Cancel method.
        /// </summary>
        /// <param name="id">The id parameter.</param>
        /// <returns>The result of the operation.</returns>
        public static void Cancel(int id)
        {
            Timers.Remove(id);
            _pendingAdditions.Remove(id);
        }

        /// <summary>
        /// Tick all active timers. Call once per frame.
        /// </summary>
        /// <summary>
        /// TickAll method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// TickAll method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public static void TickAll()
        {
            if (_pendingAdditions.Count > 0)
            {
                foreach (var kvp in _pendingAdditions)
                {
                    Timers[kvp.Key] = kvp.Value;
                }
                _pendingAdditions.Clear();
            }

            var finished = new List<int>();
            
            foreach (var kvp in Timers)
            {
                var timer = kvp.Value;
                timer.Remaining--;
                if (timer.Remaining <= 0)
                {
                    timer.OnComplete?.Invoke();
                    finished.Add(kvp.Key);
                }
            }
            
            foreach (var id in finished)
            {
                Timers.Remove(id);
            }
        }

        public static bool IsActive(int id)
        {
            return Timers.ContainsKey(id) || _pendingAdditions.ContainsKey(id);
        }

        public static int GetRemaining(int id)
        {
            SimpleTimer timer;
            if (Timers.TryGetValue(id, out timer)) return timer.Remaining;
            if (_pendingAdditions.TryGetValue(id, out timer)) return timer.Remaining;
            return -1;
        }
    }
}
