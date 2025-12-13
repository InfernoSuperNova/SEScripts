namespace IngameScript
{
    internal enum ReloadTrackerCurrentState
    {
        Bursting,
        Reloading,
        Reloaded
    }

    internal class ReloadTracker
    {
        private readonly int _burstInterval;
        private readonly int _reloadInterval;
        private readonly int _burstCount;
        
        private int _currentFrameInSequence;
        private int _currentBurst;

        public double Velocity;
        public double Range;
        private ReloadTrackerCurrentState _state = ReloadTrackerCurrentState.Reloaded;

        public ReloadTracker(int burstInterval, int reloadInterval, int burstCount, double velocity, double range)
        {
            _burstInterval = burstInterval;
            _reloadInterval = reloadInterval;
            _burstCount = burstCount;
            Velocity = velocity;
            Range = range;
        }

        

        public ReloadTrackerCurrentState GetState()
        {
            return _state;
        }

        public bool Update(bool doFire)
        {

            
            
            switch (_state)
            {
                case ReloadTrackerCurrentState.Bursting:
                    _currentFrameInSequence--;
                    // If we are firing and the timer says that it's time for the next shot
                    if (doFire && _currentFrameInSequence <= 0)
                    {
                        // Take a shot, reset timer
                        _currentFrameInSequence = _burstInterval;
                        _currentBurst++;
                        // If that was the last shot in the clip
                        if (_currentBurst >= _burstCount)
                        {
                            // Switch to the reloading state
                            _state = ReloadTrackerCurrentState.Reloading;
                            _currentFrameInSequence = _reloadInterval;
                            _currentBurst = 0;
                        }

                        return true;
                    }
                    break;
                case ReloadTrackerCurrentState.Reloading:
                    _currentFrameInSequence--;
                    // If the reload timer is zero
                    if (_currentFrameInSequence <= 0)
                    {
                        // Switch to the reloaded state
                        _state = ReloadTrackerCurrentState.Reloaded;
                    }
                    break;
                case ReloadTrackerCurrentState.Reloaded:
                    if (doFire) _state = ReloadTrackerCurrentState.Bursting;
                    break;
                
                
            }

            return false;
        }
    }
}