namespace IngameScript.Ship.Components.Missiles
{
    /// <summary>
    /// Encapsulates all parameters needed to launch a missile.
    /// Passed to MissileLauncher to configure how the missile should behave after launch.
    /// </summary>
    public struct LaunchRequest
    {
        /// <summary>
        /// The control mode for this launch (Automatic, Manual, ManualCiwsOverride)
        /// </summary>
        public LaunchControl LaunchControl { get; set; }

        /// <summary>
        /// The target ship for this missile. Can be null if the missile will find its own target.
        /// </summary>
        public TrackableShip Target { get; set; }

        /// <summary>
        /// How the missile should target the target ship (AABBCenter or SpecificBlock)
        /// </summary>
        public TargetingMode TargetingMode { get; set; }

        /// <summary>
        /// The behavior the missile should exhibit after launch
        /// </summary>
        public Behavior Behavior { get; set; }

        /// <summary>
        /// Whether this launch request has an explicit target assigned
        /// </summary>
        public bool HasExplicitTarget => Target != null;

        public LaunchRequest(LaunchControl launchControl, TrackableShip target = null,
            TargetingMode targetingMode = TargetingMode.AABBCenter, Behavior behavior = Behavior.DirectAttack)
        {
            LaunchControl = launchControl;
            Target = target;
            TargetingMode = targetingMode;
            Behavior = behavior;
        }
    }
}
