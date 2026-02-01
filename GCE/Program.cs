using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using SpaceEngineers.Game.Entities.Blocks;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    internal class Program : MyGridProgram
    {
        //config
        string GroupPrefix = "GCE";

        private const float FireDelay = 3f;
        
        List<FireControlGroup> fireControlGroups;

        int groupCount = 0;
        public Program()
        {
            //Get projectile speed block definitions
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            fireControlGroups = new List<FireControlGroup>();
            // Get the grid terminal system of the current programmable block
            
            var groups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(groups);

            foreach (var group in groups)
            {
                if (!group.Name.StartsWith(GroupPrefix)) continue;
                try
                {
                    fireControlGroups.Add(new FireControlGroup(group, this));
                }
                catch
                {
                }
            }
        }

        public void Save()
        {

        }
        public void Main(string argument, UpdateType updateSource)
        {


            foreach (FireControlGroup fireControlGroup in fireControlGroups.ToList())
            {

                bool passed = fireControlGroup.IntegrityCheck();
                if (!passed)
                {
                    fireControlGroups.Remove(fireControlGroup);
                    continue;
                }
                
                fireControlGroup.frame++;
                if (fireControlGroup.GetHasTarget())
                {
                    if (fireControlGroup.frame % FireDelay == 0)
                    {
                        fireControlGroup.ShootWeapon();
                    }
                    if ((fireControlGroup.frame + 1) % FireDelay == 0)
                    {
                        fireControlGroup.CycleWeapon();
                    }
                }

            }

        }
    }
}
    
public class FireControlGroup
{
    public List<IMyTurretControlBlock> controllers;
    public List<IMyUserControllableGun> weapons;
    public int frame;
    private float accumulatedAngleDeviation;
    public int currentWeaponIndex;
    private MyGridProgram program;

    private IMyMotorStator _azimuth;
    private IMyMotorStator _elevation;
    public FireControlGroup(IMyBlockGroup fireControlGroup, MyGridProgram program)

    {
        this.program = program;
        program.Echo($"[FCG] Initializing FireControlGroup: {fireControlGroup.Name}");

        controllers = new List<IMyTurretControlBlock>();
        fireControlGroup.GetBlocksOfType(controllers, controller => controller is IMyTurretControlBlock);
        program.Echo($"[FCG] Found {controllers.Count} controller(s)");

        weapons = new List<IMyUserControllableGun>();
        fireControlGroup.GetBlocksOfType(weapons, weapon => weapon is IMyUserControllableGun);
        program.Echo($"[FCG] Found {weapons.Count} weapon(s)");

        currentWeaponIndex = 0;
        frame = 0;
        var motors = new List<IMyMotorStator>();
        
        fireControlGroup.GetBlocksOfType(motors);
        program.Echo($"[FCG] Found {motors.Count} motor(s) in group");

        foreach (var motor in motors)
        {
            if (motor.CustomName.Contains("Azimuth"))
            {
                _azimuth = motor;
                program.Echo($"[FCG] Found Azimuth motor");
                continue;
            }
            if (motor.CustomName.Contains("Elevation"))
            {
                _elevation = motor;
                program.Echo($"[FCG] Found Elevation motor");
                continue;
            }
        }

        // If only azimuth is found, collect all blocks on top of azimuth
        if (_azimuth != null && _elevation == null)
        {
            program.Echo($"[FCG] Only Azimuth found, searching for Elevation on top grid");

            // Get all blocks on top of azimuth using GridTerminalSystem
            var blocksOnAzimuth = new List<IMyTerminalBlock>();
            program.GridTerminalSystem.GetBlocksOfType(blocksOnAzimuth, block => block.CubeGrid == _azimuth.TopGrid);
            program.Echo($"[FCG] Found {blocksOnAzimuth.Count} block(s) on top of Azimuth");

            // From the blocks on top of azimuth, find the elevation motor
            foreach (var block in blocksOnAzimuth)
            {
                var motorBlock = block as IMyMotorStator;
                if (motorBlock != null && motorBlock.CustomName == "Elevation")
                {
                    _elevation = motorBlock;
                    program.Echo($"[FCG] Found Elevation motor on top of Azimuth");
                    break;
                }
                
            }

            // If elevation was found, collect all blocks on top of elevation
            if (_elevation != null && _elevation.TopGrid != null)
            {
                var blocksOnElevation = new List<IMyTerminalBlock>();
                program.GridTerminalSystem.GetBlocksOfType(blocksOnElevation, block => block.CubeGrid == _elevation.TopGrid);
                program.Echo($"[FCG] Found {blocksOnElevation.Count} block(s) on top of Elevation");

                // Process blocks on top of elevation as needed
                foreach (var block in blocksOnElevation)
                {
                    var gunBlock = block as IMyUserControllableGun;
                    if (gunBlock != null)
                    {
                        weapons.Add(gunBlock);
                        //program.Echo($"[FCG] Found weapon on top of Elevation");
                    }
                }
            }
        }

        if (_azimuth == null)
            program.Echo($"[FCG] WARNING: Azimuth motor not found!");
        if (_elevation == null)
            program.Echo($"[FCG] WARNING: Elevation motor not found!");

        controllers[0].AzimuthRotor = _azimuth;
        controllers[0].ElevationRotor = _elevation;

        // Temp
        //controllers[0].AddTools(new List<IMyFunctionalBlock>(weapons));
        
        EquipWeapon();
        program.Echo($"[FCG] Equipped weapon at index {currentWeaponIndex}");

        program.Echo($"[FCG] Passed: {IntegrityCheck()}");
        program.Echo($"[FCG] FireControlGroup initialization complete");
    }
    public Vector3 GetTargetLeadPos(float projectileSpeed)
    {
        try
        {
            Vector3 targetPos = controllers[0].GetTargetedEntity().Position;
            Vector3 targetVel = controllers[0].GetTargetedEntity().Velocity;
            Vector3 ownVel = controllers[0].CubeGrid.LinearVelocity;
            Vector3 ownPos = weapons[currentWeaponIndex].GetPosition();


            Vector3 relativeVelocity = targetVel - ownVel;

            float timeToTarget = Vector3.Distance(targetPos, ownPos) / projectileSpeed;

            Vector3 predictedTargetPos = targetPos + relativeVelocity * timeToTarget;

            //program.Echo("Target pos: " + targetPos.ToString());
            //program.Echo("Target vel: " + targetVel.ToString());
            //program.Echo("Own vel: " + ownVel.ToString());
            //program.Echo("Own pos: " + ownPos.ToString());
            return predictedTargetPos;
        }
        catch
        {
            return new Vector3(0, 0, 0);
        }

    }
    public void ShootWeapon()
    {
        if (weapons.Count <= currentWeaponIndex) return;
        if (weapons[currentWeaponIndex].Shoot)
        {
            weapons[currentWeaponIndex].Shoot = false;
            weapons[currentWeaponIndex].ShootOnce();
        }


    }
    public void CycleWeapon()
    {
        currentWeaponIndex++;
        var weaponCount = weapons.Count;
        if (weaponCount == 0) return;
        if (currentWeaponIndex >= weaponCount)
        {
            currentWeaponIndex = 0;
        }
        foreach (IMyUserControllableGun weapon in weapons)
        {
            controllers[0].RemoveTool(weapon);
            weapon.Shoot = false;
        }
        controllers[0].AddTool(weapons[currentWeaponIndex]);
    }
    public void EquipWeapon()
    {
        
        var weaponCount = weapons.Count;
        if (weaponCount == 0) return;
        if (currentWeaponIndex >= weaponCount)
        {
            currentWeaponIndex = 0;
        }
        foreach (IMyUserControllableGun weapon in weapons)
        {
            controllers[0].RemoveTool(weapon);
            weapon.Shoot = false;
        }
        if (weapons[currentWeaponIndex] == null)
        {
            weapons.RemoveAt(currentWeaponIndex);
            EquipWeapon();
        }
        else
        {
            controllers[0].AddTool(weapons[currentWeaponIndex]);
        }
    }
    public bool GetHasTarget()
    {
        
        return controllers[0].HasTarget;
    }
    

    public float GetDistanceModifier(float projectileVel)
    {
        try
        {
            float minRange = 0f;
            float maxRange = controllers[0].Range;
            float distance = Vector3.Distance(GetTargetLeadPos(projectileVel), weapons[currentWeaponIndex].GetPosition());
            float distanceModifier = (distance - minRange) / (maxRange - minRange);
            return distanceModifier;
        }
        catch
        {
            return 0f;
        }
    }

    internal bool IntegrityCheck()
    {
        bool passed = true;
        foreach (var weapon in weapons.ToList())
        {
            if (weapon.Closed)
            {
                weapons.Remove(weapon);
            }
        }
        if (weapons.Count <= 0)
        {
            //program.Echo($"[FCG] WARNING: No weapons found!");
            passed = false;
        }

        foreach (var controller in controllers.ToList())
        {
            if (controller.Closed)
            {
                //program.Echo($"[FCG] WARNING: Controller closed!");
                controllers.Remove(controller);
            }
        }
        if (controllers.Count <= 0)
        {
            //program.Echo($"[FCG] WARNING: No controllers found!");
            passed = false;
        }

        return passed;
    }
}
