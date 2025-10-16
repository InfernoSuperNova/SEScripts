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
        string GroupPrefix = "GCE ";

        private const float FireDelay = 3f;
        
        List<FireControlGroup> fireControlGroups;

        public int GetGroupCount()
        {
            int groupCount = 0;
            // Get all the groups on the ship
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(groups);

            // Iterate through the groups and increment the counter
            foreach (IMyBlockGroup group in groups)
            {
                groupCount++;
            }
            return groupCount;
        }

        int groupCount = 0;
        public Program()
        {
            //Get projectile speed block definitions
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            fireControlGroups = new List<FireControlGroup>();
            // Get the grid terminal system of the current programmable block

            groupCount = GetGroupCount();

            Echo("Group Count: " + groupCount);

            for (int i = 0; i <= groupCount; i++)
            {
                IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(GroupPrefix + i);
                if (group == null) { continue; }
                //Place a new fire control group into the list
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


            Echo(Runtime.LastRunTimeMs.ToString());

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
        controllers = new List<IMyTurretControlBlock>();
        fireControlGroup.GetBlocksOfType(controllers, controller => controller is IMyTurretControlBlock);
        weapons = new List<IMyUserControllableGun>();
        fireControlGroup.GetBlocksOfType(weapons, weapon => weapon is IMyUserControllableGun);
        currentWeaponIndex = 0;
        frame = 0;
        var motors = new List<IMyMotorStator>();
        
        fireControlGroup.GetBlocksOfType(motors);
        
        foreach (var motor in motors)
        {
            switch (motor.CustomName)
            {
                case "Azimuth":
                    _azimuth = motor;
                    break;
                case "Elevation":
                    _elevation = motor;
                    break;
            }
        }

        controllers[0].AzimuthRotor = _azimuth;
        controllers[0].ElevationRotor = _elevation;

        // Temp
        //controllers[0].AddTools(new List<IMyFunctionalBlock>(weapons));
        
        EquipWeapon();
            
        IntegrityCheck();
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
            passed = false;
        }

        foreach (var controller in controllers.ToList())
        {
            if (controller.Closed)
            {
                controllers.Remove(controller);
            }
        }
        if (controllers.Count <= 0)
        {
            passed = false;
        }

        return passed;
    }
}
