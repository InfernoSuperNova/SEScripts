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
using CoreSystems.Api;
using SharpDX.XAPO.Fx;
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
    public class PDC
    {
        public static float MaxRange = 3000;
        public static float MinRange = 400;
        public static float RangePerSecond = 10000;
        public static float RangePerFrame = RangePerSecond / 60;
        
        private enum WeaponState
        {
            Idle,
            Tracking,
            JustStoppedTracking
        }
        
        public static readonly List<PDC> PdcList = new List<PDC>();

        private readonly IMyFunctionalBlock termPdc;

        private void Echo(string message)
        {
            Program.I.Echo(message);
        }
        
        public PDC(IMyFunctionalBlock pdc)
        {
            termPdc = pdc;
            
            Action<int, bool> eventTrigger = (value, flag) =>
            {
                Echo(value.ToString());
                switch (value)
                {
                    case 0: // tracking
                        Echo("Tracking!");
                        break;
                    case 1: // firing
                        Echo("Firing!");
                        break;
                }
            };
            
            //Program.Api.MonitorEvents(pdc, 0, eventTrigger);
            Echo("Adding event monitor");
            PdcList.Add(this);
        }


        private bool needsEnabling = false;
        private int lastAmmo = 0;
        private WeaponState _state = WeaponState.Idle;


        private double lastAzimuthForwardX = 0;
        private double lastAzimuthForwardZ = 0;
        public void Update()
        {
            // if (needsEnabling)
            // {
            //     termPdc.Enabled = true;
            //     needsEnabling = false;
            // }
            
            var azimuthMatrix = Program.Api.GetWeaponAzimuthMatrix(termPdc, 0);
            var azimuthForwardX = azimuthMatrix.Forward.X;
            var azimuthForwardZ = azimuthMatrix.Forward.Z;
            Program.Api.GetHeatLevel(termPdc); // TODO: Use heat instead. Assign PDC's close to cluster impact to goalkeeper duty.
            
            
            const double expFactor = 1.0; // Quadratic curve, tweak as needed
            switch (_state)
            {
                case WeaponState.Idle:
                    var range = Program.Api.GetMaxWeaponRange(termPdc, 0);
                    // Calculate increment based on distance from MaxRange
                    double distanceToMax = MaxRange - range;
                    double factorUp = Math.Pow(distanceToMax / MaxRange, expFactor);
                    double increment = RangePerFrame * factorUp;

                    var newRange = range + increment;
                    newRange = Math.Min(newRange, MaxRange);
                    Program.Api.SetBlockTrackingRange(termPdc, (float)newRange);

                    if (azimuthForwardX != lastAzimuthForwardX || azimuthForwardZ != lastAzimuthForwardZ)
                        _state = WeaponState.Tracking;
                    break;

                case WeaponState.Tracking:
                    var range2 = Program.Api.GetMaxWeaponRange(termPdc, 0);
                    // Calculate decrement based on distance from MinRange
                    double distanceFromMin = range2 - MinRange;
                    double factorDown = Math.Pow(distanceFromMin / (MaxRange - MinRange), expFactor);
                    double decrement = RangePerFrame * factorDown;

                    var newRange2 = range2 - decrement;
                    newRange2 = Math.Max(newRange2, MinRange);
                    Program.Api.SetBlockTrackingRange(termPdc, (float)newRange2);

                    if (azimuthForwardX == lastAzimuthForwardX && azimuthForwardZ == lastAzimuthForwardZ)
                    {
                        _state = WeaponState.JustStoppedTracking;
                        termPdc.RequestEnable(false);
                    }
                    break;
                    
                    break;
                case WeaponState.JustStoppedTracking:
                    _state = WeaponState.Idle;
                    termPdc.RequestEnable(true);
                    break;
            }

            lastAzimuthForwardX = azimuthForwardX;
            lastAzimuthForwardZ = azimuthForwardZ;
        }
    }
}