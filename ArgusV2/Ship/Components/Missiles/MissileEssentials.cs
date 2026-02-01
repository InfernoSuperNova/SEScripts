namespace IngameScript.Ship.Components.Missiles
{
    using System;
    using System.Collections.Generic;
    using Sandbox.ModAPI.Ingame;
    using VRage.Game.ModAPI.Ingame;

    /// <summary>
    /// Static utility class for validating missile assemblies.
    /// Validates that missiles meet all essential requirements:
    /// - Must have a gyroscope
    /// - Must have a power source (battery or reactor)
    /// - If it has a hydrogen thruster, it must have a hydrogen tank
    /// </summary>
    public static class MissileEssentials
    {
        /// <summary>
        /// Checks if a missile block list has a gyroscope
        /// </summary>
        public static bool HasGyroscope(List<IMyCubeBlock> blocks)
        {
            return blocks.Find(b => b is IMyGyro) != null;
        }

        /// <summary>
        /// Checks if a missile block list has a power source (battery or reactor)
        /// </summary>
        public static bool HasPowerSource(List<IMyCubeBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block is IMyBatteryBlock)
                    return true;
                if (block is IMyReactor)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a missile has hydrogen thrusters
        /// </summary>
        public static bool HasHydrogenThrusters(List<IMyCubeBlock> blocks)
        {
            foreach (var block in blocks)
            {
                IMyThrust thruster = block as IMyThrust;
                if (thruster != null && thruster.BlockDefinition.SubtypeId.Contains("Hydrogen"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a missile has hydrogen tanks
        /// </summary>
        public static bool HasHydrogenTanks(List<IMyCubeBlock> blocks)
        {
            foreach (var block in blocks)
            {
                IMyGasTank gasTank = block as IMyGasTank;
                if (gasTank != null && gasTank.BlockDefinition.SubtypeId.Contains("Hydrogen"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Validates that a missile assembly meets all essential requirements
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidMissile(List<IMyCubeBlock> blocks)
        {
            // Requirement 1: Must have at least one gyroscope
            if (!HasGyroscope(blocks))
                return false;

            // Requirement 2: Must have at least one power source
            if (!HasPowerSource(blocks))
                return false;

            // Requirement 3: If it has hydrogen thrusters, it must have hydrogen tanks
            if (HasHydrogenThrusters(blocks) && !HasHydrogenTanks(blocks))
                return false;

            return true;
        }
        
        /// <summary>
        /// Gets a validation error message if the missile is invalid, or null if valid
        /// </summary>
        public static string GetValidationError(List<IMyCubeBlock> blocks)
        {
            if (!HasGyroscope(blocks))
                return "Missing required gyroscope";

            if (!HasPowerSource(blocks))
                return "Missing required power source (battery or reactor)";

            if (HasHydrogenThrusters(blocks) && !HasHydrogenTanks(blocks))
                return "Hydrogen thrusters detected but no hydrogen tank found";

            return null;
        }
    }
}