using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public delegate void GunFinishedFiringDelegate(Gun gun);

    public class Gun
    {
        private static readonly MyDefinitionId ElectricityId =
            new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

        public IMyUserControllableGun actualGun;
        public bool Available;
        public bool AvailablePrevious;
        public float chargePercent;
        private readonly StringBuilder chargePercentSB = new StringBuilder();
        private StringBuilder chargeTimeSB = new StringBuilder();
        private readonly StringBuilder detailedInfoSB = new StringBuilder();
        private readonly float FireDelay;
        public bool Functional;
        public bool FunctionalPrevious;
        public Vector3D gridPosition;
        private readonly GunFinishedFiringDelegate gunFinishedFiringDelegate;
        private readonly MyResourceSinkComponent gunSinkComponent;

        public char nameShorthand = ' ';
        public double PowerDraw;
        public double PowerDrawPrevious;
        public double range = 0;
        public int RemainingChargeSeconds = 0;
        public bool Shoot;

        public bool ShootPrevious;
        public double sigma = 0;
        private float TimeSpentFiring;
        public float TimeToFire;

        public Gun(IMyUserControllableGun gun, Dictionary<MyDefinitionId, float> knownFireDelays,
            GunFinishedFiringDelegate gunFinishedFiringDelegate, MyGridProgram program, ushort id)
        {
            actualGun = gun;
            gunSinkComponent = actualGun.Components.Get<MyResourceSinkComponent>();
            if (!knownFireDelays.ContainsKey(gun.BlockDefinition))
                FireDelay = 0f;
            else
                FireDelay = knownFireDelays[gun.BlockDefinition];
            this.gunFinishedFiringDelegate = gunFinishedFiringDelegate;
            gridPosition = actualGun.Position;

            nameShorthand = (char)id;


            Functional = actualGun.IsFunctional;
            PowerDraw = gunSinkComponent.CurrentInputByType(ElectricityId);
            Available = Functional && PowerDraw < 0.002f;

            AvailablePrevious = Available;
            ShootPrevious = Shoot;
            FunctionalPrevious = Functional;
            PowerDrawPrevious = PowerDraw;

            FlightDataRecorder.WeaponCreatedEvent(nameShorthand);
        }

        public bool Enabled
        {
            get { return actualGun.Enabled; }

            set
            {
                if (actualGun.Enabled != value) actualGun.Enabled = value;
            }
        }


        public bool Closed => actualGun.Closed;

        public bool IsFunctional => actualGun.IsFunctional;

        public void Tick()
        {
            if (Available && Shoot)
            {
                actualGun.ShootOnce();
                TimeSpentFiring = Math.Min(TimeSpentFiring + 1f / 60f, FireDelay);
            }
            else
            {
                if (TimeSpentFiring > 0)
                {
                    gunFinishedFiringDelegate(this);
                    Shoot = false;
                    TimeSpentFiring = 0;
                }
            }
        }

        public void SlowTick()
        {
            AvailablePrevious = Available;

            FunctionalPrevious = Functional;
            PowerDrawPrevious = PowerDraw;


            Functional = actualGun.IsFunctional;
            PowerDraw = gunSinkComponent.CurrentInputByType(ElectricityId);
            Available = Functional && PowerDraw < 0.002f;

            EvaluateData(Available, AvailablePrevious, Shoot, ShootPrevious, Functional, FunctionalPrevious, PowerDraw,
                PowerDrawPrevious);
            ShootPrevious = Shoot;
            detailedInfoSB.Clear().Append(actualGun.DetailedInfo);


            var startIndex = actualGun.DetailedInfo.IndexOf("Stored power: ") + 14;
            if (startIndex + 6 > actualGun.DetailedInfo.Length)
            {
                chargePercent = 0;
                return;
            }

            chargePercentSB.Clear().AppendSubstring(detailedInfoSB, startIndex, 6).RemoveNonNumberChars();
            chargePercent = float.Parse(chargePercentSB.ToString()) / 50000f;
        }

        private void EvaluateData(bool available, bool availablePrevious, bool shoot, bool shootPrevious,
            bool functional, bool functionalPrevious, double powerDraw, double previousPowerDraw)
        {
            if (shoot != shootPrevious)
            {
                if (shoot)
                    FlightDataRecorder.WeaponBeginChargeEvent(nameShorthand, sigma, range);
                else
                    FlightDataRecorder.WeaponCancelChargeEvent(nameShorthand, sigma, range);
            }

            if (functional != functionalPrevious)
            {
                if (functional)
                    FlightDataRecorder.WeaponRepairedEvent(nameShorthand);
                else
                    FlightDataRecorder.WeaponDamagedEvent(nameShorthand);
            }

            if (powerDraw != previousPowerDraw)
            {
                if (powerDraw < 0.002f)
                {
                    if (previousPowerDraw > 0.002f && functional && functionalPrevious)
                        FlightDataRecorder.WeaponRechargedEvent(nameShorthand);
                }
                else
                {
                    if (previousPowerDraw < 0.002f && functionalPrevious)
                        FlightDataRecorder.WeaponFireEvent(nameShorthand, sigma, range);
                }
            }
        }


        public Vector3D GetPosition()
        {
            return actualGun.GetPosition();
        }

        public float GetTimeToFire()
        {
            TimeToFire = FireDelay - TimeSpentFiring;
            return TimeToFire;
        }
    }
}