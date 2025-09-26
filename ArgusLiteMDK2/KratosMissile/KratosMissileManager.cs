using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    public enum MissileControlMode
    {
        Loiter,
        Attack
    }

    internal class KratosMissileManager
    {
        // TODO: Clean up fields

    #region Fields
    #region Config
        // TODO: Make this part of the config
        private readonly int _missileCount = 16;
    #endregion

    #region Missile lists
        // TODO: Assign lists
        private readonly List<KratosMissile> _readyMissiles;
        private readonly Dictionary<KratosMissile, int> _pendingLaunchMissiles;
        private readonly List<KratosMissile> _launchedMissiles;
    #endregion

    #region Uncategorized
        private int _frame;
        private bool _hasTarget;
        private readonly Random _rng;
    #endregion

    #region Single target targeting
        private ArgusTargetableShip _currentTargetedShip;
    #endregion
    #region Multi target targeting
        public List<ArgusTargetableShip> _targetCandidates; 
    #endregion

        private ArgusShip _host; // TODO: Construct me

    #region Missile accquisition
        private readonly List<string> _groupNames = new List<string>();

        // Keeps track of whether missiles for each group has been accquired so duplicate entries aren't created
        private readonly Dictionary<string, bool> _accquiredMissiles = new Dictionary<string, bool>();
    #endregion

        private TargetableBlockCategory _targetCategory = TargetableBlockCategory.Default;

        private readonly Dictionary<string, MissileDetachmentGroup> _detachmentGroups =
            new Dictionary<string, MissileDetachmentGroup>();

        private KratosMissileBehavior _kratosMissileBehavior =
            KratosMissileBehavior.Behaviors[KratosMissileBehaviorType.SurroundMissileBehavior];

        private bool _attack;

        // TODO: Clean up constructor
    #endregion

    #region Constructor
        public KratosMissileManager(IMyGridTerminalSystem gridTerminalSystem, ArgusShip host)
        {
            _host = host;
            _rng = new Random();
            _targetCandidates = new List<ArgusTargetableShip>();
            _readyMissiles = new List<KratosMissile>();
            _pendingLaunchMissiles = new Dictionary<KratosMissile, int>();
            _launchedMissiles = new List<KratosMissile>();
            for (var i = 1; i <= _missileCount; i++)
            {
                var missileGroupName = "M" + i;
                var detachmentGroupName = "DM" + i;

                _groupNames.Add(missileGroupName);

                _accquiredMissiles.Add(missileGroupName, false);
                var missileGroup = gridTerminalSystem.GetBlockGroupWithName(missileGroupName);
                if (missileGroup != null)
                {
                    var gyros = new List<IMyGyro>();
                    missileGroup.GetBlocksOfType(gyros);
                    foreach (var gyro in gyros) gyro.CustomData = "";
                }

                var detachmentGroup = gridTerminalSystem.GetBlockGroupWithName(detachmentGroupName);
                if (detachmentGroup != null)
                {
                    var thrusters = new List<IMyThrust>();
                    detachmentGroup.GetBlocksOfType(thrusters);
                    _detachmentGroups.Add(missileGroupName, new MissileDetachmentGroup(thrusters));
                }
            }
        }
    #endregion

    #region Entrypoints
        // TODO: Call from argument somewhere
        public void SetNewBehaviorType(KratosMissileBehaviorType behaviorType)
        {
            _kratosMissileBehavior = KratosMissileBehavior.Behaviors[behaviorType];


            // Fighter > Fighter: nothing
            // Fighter > Surround: stop attacking and loiter around target if valid else loiter around player
            // Figher > interdict: check for targets in range and attack else loiter

            // Surround > Fighter: attack
            // Surround > Surround: nothing
            // Surround > interdict: check for targets in range and attack else loiter

            // Interdict > Fighter: attack
            // Interdict > Surround: stop attacking and loiter around target if valid else loiter around player
            // Interdict > Interdict: nothing

            // So it's symmetrical regardless of what the switch is doing then? Cool
            // TODO: Implement specific changes when switching behaviors
            switch (behaviorType)
            {
                case KratosMissileBehaviorType.FighterMissileBehavior:
                    break;
                case KratosMissileBehaviorType.InterdictMissileBehavior:
                    break;
                case KratosMissileBehaviorType.SurroundMissileBehavior:
                    break;
            }
        }

        public void UpdateTargetedShip(ArgusTargetableShip newTargetedShip)
        {
            _currentTargetedShip = newTargetedShip;
        }
        // TODO: Call me
        public void CycleTargetCategory()
        {
            _CycleTargetCategory();
        }
    #endregion


    #region Missile targeting indices
        private void _CycleTargetCategory()
        {
            switch (_targetCategory)
            {
                case TargetableBlockCategory.Default:
                    _targetCategory = TargetableBlockCategory.Propulsion;
                    break;
                case TargetableBlockCategory.Propulsion:
                    _targetCategory = TargetableBlockCategory.Weapons;
                    break;
                case TargetableBlockCategory.Weapons:
                    _targetCategory = TargetableBlockCategory.PowerSystems;
                    break;
                case TargetableBlockCategory.PowerSystems:
                    _targetCategory = TargetableBlockCategory.Default;
                    break;
            }
        }

        // TODO: Select indexes for missiles when a new target is scanned (done?)
        // TODO: Call from Program.cs
        public void TargetJustScanned(ArgusTargetableShip newShip)
        {
            foreach (var missile in _launchedMissiles) SetTargetedBlockIndices(missile, newShip);
        }

        // TODO: Call this on new missile launch
        private void SetTargetingIndicesForNewMissile(KratosMissile missile, List<ArgusTargetableShip> ships)
        {
            foreach (var ship in ships) SetTargetedBlockIndices(missile, ship);
        }
        // ^^^^^
        // These two together should cover all bases?
        // If a missile is launched, it needs to discover all existing ships and their blocks to select a target
        // If a ship is detected, it needs to tell all the missiles to select a target


        // Should this be moved to another class?
        // Either KratosMissile or TargetableShip, which one does it "belong" to more?
        // I guess targetable ship because it's getting data from that specifically?
        // Or I could just keep it here because this is neutral ground
        private void SetTargetedBlockIndices(KratosMissile missile, ArgusTargetableShip ship)
        {
            if (ship == null)
            {
                return;
            }


            var targetedIndices = new Dictionary<TargetableBlockCategory, int>
            {
                {
                    TargetableBlockCategory.Default,
                    GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.Default)
                },
                {
                    TargetableBlockCategory.Weapons,
                    GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.Weapons)
                },
                {
                    TargetableBlockCategory.Propulsion,
                    GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.Propulsion)
                },
                {
                    TargetableBlockCategory.PowerSystems,
                    GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.PowerSystems)
                }
            };

            // TODO: Is this check redundant?
            // Add the kvp for this ship to the missile if it doesn't exist

            //if (!missile.targetedCategoryBlockIndices.ContainsKey(ship)) missile.targetedCategoryBlockIndices.Add(ship, targetedIndices);
            missile.TargetedCategoryBlockIndices[ship] = targetedIndices;
            // Update the index for the targeted block
        }


        private int GetTargetedBlockIndexForCategory(ArgusTargetableShip ship, TargetableBlockCategory cat)
        {
            var index = -1;
            switch (cat)
            {
                case TargetableBlockCategory.Default:
                    if (ship.AllBlocksCount == 0) break;
                    index = _rng.Next(0, ship.AllBlocksCount - 1);
                    break;
                case TargetableBlockCategory.Propulsion:
                    if (ship.PropulsionBlocksCount == 0) break;
                    index = _rng.Next(0, ship.PropulsionBlocksCount - 1);
                    break;
                case TargetableBlockCategory.Weapons:
                    if (ship.WeaponBlocksCount == 0) break;
                    index = _rng.Next(0, ship.WeaponBlocksCount - 1);
                    break;
                case TargetableBlockCategory.PowerSystems:
                    if (ship.PowerSystemsBlocksCount == 0) break;
                    index = _rng.Next(0, ship.PowerSystemsBlocksCount - 1);
                    break;
            }

            return index;
        }
    #endregion

        // TODO: Tackle this absolute monstrosity
        public void Update(ArgusTargetableShip primaryEnemyShip)
        {
            _frame++;

            var gravity = _host.Controller.GetNaturalGravity();
            var gravityNormal = gravity.Normalized();
            

            HandleAttackPatterns(primaryEnemyShip, gravity, gravityNormal);
            FindNewMissiles();
            HandleLaunchQueue();
        }

        private void FindNewMissiles()
        {
            if ((_frame + 32) % 9 == 0)
                foreach (var name in _groupNames)
                {
                    if (_accquiredMissiles[name]) continue;
                    var group = Program._gridTerminalSystem.GetBlockGroupWithName(name);
                    if (group == null) continue;
                    var blocks = new List<IMyTerminalBlock>();
                    group.GetBlocksOfType(blocks);
                    
                    var missile = new KratosMissile(_host, blocks, _kratosMissileBehavior,
                        _rng.NextDouble() * (_kratosMissileBehavior.MaxGuidanceFactor -
                                             _kratosMissileBehavior.MinGuidanceFactor) +
                        _kratosMissileBehavior.MinGuidanceFactor, name, false);
                    if (missile == null || !missile.ConstructedSuccessfully) continue;
                    _accquiredMissiles[name] = true;
                    _readyMissiles.Add(missile);
                }
        }


        #region Attack patterns
        
        private void HandleAttackPatterns(ArgusTargetableShip ship, Vector3D gravity, Vector3D gravityNormal)
        {
            // TODO: Set up params
            switch (_kratosMissileBehavior.AttackPattern)
            {
                case AttackPattern.DirectAttack:
                    // Guide missile to target if it has one or guide to position
                    DirectAttack(ship, gravity, gravityNormal);
                    break;

                case AttackPattern.SurroundAndWaitForAttack:
                    // Guide missile to surround the host ship if it does not have a target, or surround the target if it has one, or attack the target if it has one and attack is true
                    SurroundAndWaitForAttack(ship, gravity, gravityNormal);
                    break;

                case AttackPattern.LoiterAndPursue:
                    // Guide missile to position and intercept nearby targets
                    LoiterAndPursue(gravity, gravityNormal);
                    break;
            }
        }
        
        
        // The most basic mode. Simply loiters around the target in a sphere and automatically goes for a direct attack when a target is detected.
        private void DirectAttack(ArgusTargetableShip ship, Vector3D gravity, Vector3D gravityNormal)
        {
            
            if (ship == ArgusTargetableShip.Default)
                foreach (var missile in _launchedMissiles) // Ship is invalid, loiter around host
                {
                    missile.LoiterAroundHost();
                    missile.Target = ship;
                    missile.Update(gravity, gravityNormal);
                }
            else
                foreach (var missile in _launchedMissiles) // Ship is valid, loiter aruond target
                {
                    missile.InterceptTarget();
                    missile.Target = ship;
                    missile.Update(gravity, gravityNormal);
                }

            // TODO: get ship pos and gravity, rework update to take only what is required
        }

        // Surrounds the host ship in a tight sphere, which translates itself directly to the position of the primary enemy if detected
        private void SurroundAndWaitForAttack(ArgusTargetableShip ship, Vector3D gravity, Vector3D gravityNormal)
        {
            if (ship == ArgusTargetableShip.Default)
                foreach (var missile in _launchedMissiles) // Ship is invalid, loiter around host
                {
                    missile.LoiterAroundHost();
                    missile.Target = ship;
                    missile.Update(gravity, gravityNormal);
                    ALDebug.Echo("Loitering around host?");
                }
            else if (_attack)
                // Attack target
                foreach (var missile in _launchedMissiles) // Ship is valid and attack is true, intercept target
                {
                    missile.InterceptTarget();
                    missile.Target = ship;
                    missile.Update(gravity, gravityNormal);
                    ALDebug.Echo("Attacking target?");
                }
            else
                // Surround target
                foreach (var missile in _launchedMissiles) // Ship is valid and attack is false, loiter around target
                {
                    missile.LoiterAroundTarget();
                    missile.Target = ship;
                    missile.Update(gravity, gravityNormal);
                    ALDebug.Echo("Surrounding target?");
                }
        }


        // Takes a range of targets and figures out per missile which ship would be the best to atack
        private void LoiterAndPursue(Vector3D gravity, Vector3D gravityNormal)
        {
            // TODO: Make target finding once every 10 frames
            foreach (var missile in _launchedMissiles)
            {
                var target = GetClosestTarget(_targetCandidates, missile);
                missile.Target = target;
                if (target == ArgusTargetableShip.Default || !_attack) // No targets in range or attack is turned off
                    // We go to loiter position
                    missile.LoiterAroundHost();
                else
                    missile.InterceptTarget(); // Target in range and attack is turned on
                // TODO: get ship pos and gravity, rework update to take only what is required
                missile.Update(gravity, gravityNormal);
            }
        }
    #endregion

        private ArgusTargetableShip GetClosestTarget(List<ArgusTargetableShip> ships, KratosMissile missile)
        {
            var minDistance = double.MaxValue;
            var targetFinal = ArgusTargetableShip.Default;
            foreach (var targetCandidate in ships)
            {
                var shipDistanceSquared = (targetCandidate.Position - missile.GetPosition()).LengthSquared();

                if (!targetCandidate.DataIsCurrent || !(shipDistanceSquared <
                                                        _kratosMissileBehavior.MaxAttackDistance *
                                                        _kratosMissileBehavior.MaxAttackDistance)) continue;

                if (shipDistanceSquared < minDistance)
                {
                    minDistance = shipDistanceSquared;
                    targetFinal = targetCandidate;
                }
            }

            return targetFinal;
        }

        // Safety function on recompile, server restart, PB crash, etc.
        public void DetonateAll()
        {
            foreach (var missile in _launchedMissiles)
            {
                if (!missile.HasLaunched) return;
                missile.ForceDetonate();
            }
        }

    #region launching
        // TODO: Add safety checks for launching based on missile direction and velocity (Should this be added to the launch queue instead?)
        // In that case, what should happen when a missile comes up in the queue as invalid?
        // It could:
        // a, keep it in the queue but don't try the next missile
        // b, keep it in the queue and immediately try launch the next missile
        // c, keep it in the queue and reduce the delay for the next missile as normal
        // d, Remove it from the queue but don't try the next missile
        // e, Remove it from the queue and immediately try launch the next missile
        // f, Remove it from the queue and reduce the delay for the next missile as normal


        // upsides of a:
        // Does not present a case where missiles could fire simultaneously
        // Preserves the launch order

        // downsides of a:
        // Queue can become indefinitely stuck if it is always false, leading to no missile launches


        // upsides of b:
        // Does not present a case where missiles could fire simultaneously
        // Cannot become stuck
        // Retains rate of fire up until that point
        // Scalable with multiple waiting missiles

        // downsides of b:
        // Sacrifices the launch order
        // If one missile gets stuck, the rest could launch in quick succession one frame after the other
        // This could be mitigated by storing the initial frame delay of the current missile but that would ignore the frame delay of the next missile


        // upsides of c:
        // Cannot become stuck
        // Retains rate of fire

        // ok that's enough upsides and downsides

        // COUNTERPROPOSAL:
        // g, keep it in the launch queue and don't try the next missile, but set a timer and forcibly launch the waiting missile anyway after a set delay

        // upsides:
        // Cannot become stuck
        // Retains launch order
        // Will never allow simultaneous fire

        // downsides:
        // sacrifices launch speed
        // Might blow a hole in the side of the ship because ignoring safety checks
        public void TryLaunch(MyGridProgram program, Vector3D shipPos, Vector3D velocity, Vector3D forward)
        {
            LaunchBoobyTrap();

            var shouldReturn = false;
            var delay = 0;
            // I could skip this switch entirely and have values for shouldReturn in KratosMissileBehavior and derrived classes
            // But then it would be difficult to refactor the launch type to be separated from the missile behavior
            switch (_kratosMissileBehavior.LaunchType)
            {
                case LaunchType.Single:
                    shouldReturn = true;
                    break;
                case LaunchType.Alpha:
                    delay = 0;
                    break;
                case LaunchType.Staggered:
                    delay = _kratosMissileBehavior.MissileLaunchDelayFrames;
                    break;
            }

            
            
            for (var index = _readyMissiles.Count - 1; index >= 0; index--)
            {
                var missile = _readyMissiles[index];
                if (!missile.IsComplete()) continue;

                _pendingLaunchMissiles.Add(missile, delay);

                if (shouldReturn) return;
            }
        }

        // TODO: test this thoroughly before I am comfortable enabling it
        private void LaunchBoobyTrap()
        {
            // if (Program.TrollMode)
            // {
            //     
            //     if (rng.NextDouble() < 0.1)
            //     {
            //         BoobyTrap.DetonateAllTheWarheads(Program._gridTerminalSystem);
            //     }
            // }
        }

        // TODO: Call from update
        // Handles the launch queue
        private void HandleLaunchQueue()
        {
            var missilesLaunched = new List<KratosMissile>();

            // In the case that all are 0, all will launch on the same frame, otherwise it will subtract from a given missiles pool
            foreach (var kvp in _pendingLaunchMissiles)
            {
                var missile = kvp.Key;
                var framesRemaining = kvp.Value;
                if (framesRemaining > 0)
                {
                    _pendingLaunchMissiles[missile] = framesRemaining - 1;
                    return;
                }

                
                // TODO: add params
                var result = ActuallyLaunch(missile, _host);
                // TODO: add result handling
                switch (result)
                {
                    case 0: // success
                        missilesLaunched.Add(missile);
                        break;
                    case 1: // Cannot launch due to relative velocity
                        break;
                    case 2:
                        break;
                }
            }
            
            missilesLaunched.ForEach(x => { _pendingLaunchMissiles.Remove(x); _launchedMissiles.Add(x); });
        }


        private int ActuallyLaunch(KratosMissile missile, ArgusShip host)
        {
            var missileForward = missile.GetForward();
            if (missileForward.Dot(host.Velocity) > _kratosMissileBehavior.MaxLaunchSpeed ||
                missileForward.Cross(host.Velocity).ALengthSquared() >
                _kratosMissileBehavior.MaxLaunchSpeed * _kratosMissileBehavior.MaxLaunchSpeed) return 1;


            // TODO: this monstrosity
            //SetTargetingIndicesForNewMissile(missile, _targetCandidates); // Disabled for now
            if (_currentTargetedShip != null) SetTargetedBlockIndices(missile, _currentTargetedShip);
            var missileLaunchResult = missile.Launch();
            
            switch (missileLaunchResult)
            {
                case 1: // fuel fail
                    return 2; // fuel fail
            }
            
            _accquiredMissiles[missile.Name] = false;
            
            
            var missilePos = missile.GetThrusterPosition();
            var safetyPos = missilePos + missileForward * _kratosMissileBehavior.MissileSafetyDistance;
            var divergePos = safetyPos + (safetyPos - host.Position).Normalized() * _kratosMissileBehavior.MissileDivergeDistance;
            var surroundDir = GenerateRandomPositionOnSphere(Vector3D.Zero);
            
            var gravity = -_host.Controller.GetNaturalGravity();
            var gravityLength = gravity.Length();
            if (gravityLength > 0)
                surroundDir = GenerateGravityAttackPosition(gravity / gravityLength, _kratosMissileBehavior.MinAngleGravity, _kratosMissileBehavior.MaxAngleGravity);

            missile.SurroundDir = surroundDir;

            missile.SurroundDist =
                _rng.NextDouble() * (_kratosMissileBehavior.MaxSurroundDistance - _kratosMissileBehavior.MinSurroundDistance) + _kratosMissileBehavior.MinSurroundDistance;
            return 0;
        }
    #endregion

        private Vector3D GenerateGravityAttackPosition(Vector3D direction, double minAngle, double maxAngle)
        {
            // Convert the angle range from degrees to radians
            var minVarianceRadians = MathHelper.ToRadians(minAngle);
            var maxVarianceRadians = MathHelper.ToRadians(maxAngle);

            // Generate a random variance between the minimum and maximum angle
            var varianceRadians = minVarianceRadians + _rng.NextDouble() * (maxVarianceRadians - minVarianceRadians);

            // Generate a random point on a unit sphere
            var theta = 2 * Math.PI * _rng.NextDouble(); // Random angle between 0 and 2*PI
            var u = 2 * _rng.NextDouble() - 1; // Random value between -1 and 1
            var sqrtOneMinusUSquared = Math.Sqrt(1 - u * u);
            var x = sqrtOneMinusUSquared * Math.Cos(theta);
            var y = sqrtOneMinusUSquared * Math.Sin(theta);
            var z = u;
            var randomPointOnSphere = new Vector3D(x, y, z);

            // Scale the random point by the randomly generated variance
            var perturbation = Vector3D.Normalize(randomPointOnSphere) * varianceRadians;

            // Normalize the original direction vector
            direction.Normalize();

            // Calculate the axis and angle for rotation
            var axis = Vector3D.Cross(Vector3D.Up, direction);
            var angle = Math.Acos(Vector3D.Dot(Vector3D.Up, direction));

            // Apply the rotation to the perturbation
            var rotatedPerturbation = Vector3D.Transform(perturbation, MatrixD.CreateFromAxisAngle(axis, angle));

            // Combine the rotated perturbation with the direction vector
            var deviatedDirection = direction + rotatedPerturbation;
            deviatedDirection.Normalize();

            // Compute the final attack position
            var dir = deviatedDirection;
            return dir;
        }

        public void Attack()
        {
            _attack = !_attack;
        }

        private Vector3D GenerateRandomPositionOnSphere(Vector3D center)
        {
            // Generate random spherical coordinates
            var theta = 2 * Math.PI * _rng.NextDouble(); // Random angle in [0, 2*PI]
            var phi = Math.Acos(2 * _rng.NextDouble() - 1); // Random angle in [0, PI]

            // Convert spherical coordinates to Cartesian coordinates
            var x = Math.Sin(phi) * Math.Cos(theta);
            var y = Math.Sin(phi) * Math.Sin(theta);
            var z = Math.Cos(phi);

            // Create the random position vector
            var randomPosition = new Vector3D(x, y, z);

            // Translate the position by the center
            return center + randomPosition;
        }


    #region HUD data
        public List<MissileData> GetMissileData()
        {
            var missileData = new List<MissileData>();
            foreach (var missile in _launchedMissiles) missileData.Add(missile.GetData());
            return missileData;
        }


        public struct MissileData
        {
            public Vector3D Position;
            public Vector3D PreviousPosition;
            public Vector3D Forward;
            public Vector3D DistanceToTarget;
            public double Fuel;
            public long Id;
            public bool HasLaunched;
            public bool IsHydrogen;

            public MissileData(Vector3D position, Vector3D previousPosition, Vector3D forward,
                Vector3D distanceToTarget, double fuel, long id, bool hasLaunched, bool isHydrogen)
            {
                Position = position;
                PreviousPosition = previousPosition;
                Forward = forward;
                DistanceToTarget = distanceToTarget;
                Fuel = fuel;
                Id = id;
                HasLaunched = hasLaunched;
                IsHydrogen = isHydrogen;
            }
        }
    #endregion
    }
}