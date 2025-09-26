using System.Collections.Generic;

namespace IngameScript
{
    /// <summary>
    /// Used to control the attack pattern of the missile
    /// </summary>
    public enum AttackPattern
    {
        DirectAttack,
        SurroundAndWaitForAttack,
        LoiterAndPursue
    }

    // Todo: Perhaps this shouldn't be part of guidance parameters?
    /// <summary>
    /// Used to control what happens when the launch argument is recieved
    /// </summary>
    public enum LaunchType
    {
        Single,
        Alpha,
        Staggered
    }
    
    /// <summary>
    /// Contains information about the behavior of the missile
    /// </summary>
    public class KratosMissileBehavior
    {
        public static Dictionary<KratosMissileBehaviorType, KratosMissileBehavior> Behaviors =
            new Dictionary<KratosMissileBehaviorType, KratosMissileBehavior>
            {
                { KratosMissileBehaviorType.FighterMissileBehavior, new FighterMissileBehavior() },
                { KratosMissileBehaviorType.SurroundMissileBehavior, new SurroundMissileBehavior() },
                { KratosMissileBehaviorType.InterdictMissileBehavior, new InterdictMissileBehavior() },
            };
        public double MinSurroundDistance;
        public double MaxSurroundDistance;
        public double MaxAttackDistance;
        public double MinAngleGravity;
        public double MaxAngleGravity;
        public double MissileSafetyDistance = 50;
        public double MissileDivergeDistance = 300;
        public double MaxLaunchSpeed = 140;
        public double KP = 12;
        public double KI = 0;
        public double KD = 0;
        public double MinGuidanceFactor = 5;
        public double MaxGuidanceFactor = 5;
        public int MissileLaunchDelayFrames = 5;
        public int MaintainTrajectorAfterLaunchFrames = 60;
        
        public AttackPattern AttackPattern;
        public LaunchType LaunchType;
    }

/// <summary>
/// Used for indexing the behavioral classes
/// </summary>
    public enum KratosMissileBehaviorType
    {
        FighterMissileBehavior,
        SurroundMissileBehavior,
        InterdictMissileBehavior
    }
    

    public class FighterMissileBehavior : KratosMissileBehavior
    {
        public FighterMissileBehavior()
        {
            MinSurroundDistance = 500;
            MaxSurroundDistance = 500;
            MaxAttackDistance = 0;
            MinAngleGravity = 0;
            MaxAngleGravity = 180;
            AttackPattern = AttackPattern.DirectAttack;
            LaunchType = LaunchType.Single;
        }
    }

    public class SurroundMissileBehavior : KratosMissileBehavior
    {
        public SurroundMissileBehavior()
        {
            MinSurroundDistance = 1500;
            MaxSurroundDistance = 3000;
            MaxAttackDistance = 0;
            MinAngleGravity = 100;
            MaxAngleGravity = 180;
            AttackPattern = AttackPattern.SurroundAndWaitForAttack;
            LaunchType = LaunchType.Staggered;
        }
    }

    public class InterdictMissileBehavior : KratosMissileBehavior
    {
        public InterdictMissileBehavior()
        {
            MinSurroundDistance = 1500;
            MaxSurroundDistance = 11000;
            MaxAttackDistance = 5000;
            MinAngleGravity = 85;
            MaxAngleGravity = 120;
            AttackPattern = AttackPattern.LoiterAndPursue;
            LaunchType = LaunchType.Staggered;
        }
    }
}