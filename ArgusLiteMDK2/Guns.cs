using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    public class Guns
    {
        private const float IdlePowerDraw = 0.002f;
        private Gun activeGun;

        private readonly Dictionary<Gun, bool> availableGuns;

        private ushort currentGunIndex = 1;
        private bool currentGunIsVolleyFiring;
        private Gun currentlyFiringGun;
        private int currentVolleyFrame;

        private int gunIndex = -1;

        public List<float> gunRechargePercentages = new List<float>();

        public List<int> gunRechargeTimes = new List<int>();

        private readonly List<Gun> guns;
        private readonly List<IMyUserControllableGun> gunsReference;
        private readonly Dictionary<MyDefinitionId, float> knownFireDelays;
        private readonly MyGridProgram program;
        private readonly float secondDifferenceToGroupGunFiring;

        private MatrixD ToWorldMatrix;
        
        public int volleyDelayFrames;

        public enum GunMode
        {
            FireWhenReady,
            Volley,
            WaitForAll
        }

        public GunMode _GunMode = GunMode.WaitForAll;

        public Guns(List<IMyUserControllableGun> guns, MyGridProgram program,
            Dictionary<MyDefinitionId, float> knownFireDelays, float framesToGroupGuns,GunMode gunMode,
            int volleyDelayFrames)

        {

            _GunMode = gunMode;
            secondDifferenceToGroupGunFiring = framesToGroupGuns / 60;
            availableGuns = new Dictionary<Gun, bool>();
            this.guns = new List<Gun>();
            gunsReference = new List<IMyUserControllableGun>();
            this.knownFireDelays = knownFireDelays;
            foreach (var gun in guns)
            {
                var isTurret = gun as IMyLargeTurretBase;
                if (isTurret == null)
                {
                    this.guns.Add(new Gun(gun, knownFireDelays, GunFinishedFiring, program, currentGunIndex));
                    currentGunIndex++;
                    gunsReference.Add(gun);
                }
            }

            foreach (var gun in this.guns) availableGuns[gun] = gun.Available;
            this.volleyDelayFrames = volleyDelayFrames;

            this.program = program;

            switch (gunMode)
            {
                case GunMode.FireWhenReady:
                    break;
                case GunMode.Volley:
                    activeGun = this.guns[0];
                    currentlyFiringGun = this.guns[0];
                    break;
                case GunMode.WaitForAll:
                    break;
            }
            

            ToWorldMatrix = program.Me.CubeGrid.WorldMatrix;
        }

        public void UpdateWeaponsList(List<IMyUserControllableGun> guns)
        {
            foreach (var gun in guns)
            {
                var isTurret = gun as IMyLargeTurretBase;
                if (isTurret == null && !gunsReference.Contains(gun))
                {
                    gunsReference.Add(gun);
                    this.guns.Add(new Gun(gun, knownFireDelays, GunFinishedFiring, program, currentGunIndex));
                    currentGunIndex++;
                }
            }

            foreach (var gun in this.guns) availableGuns[gun] = gun.Available;
        }

        private void GunFinishedFiring(Gun gun)
        {
            //if (gun != currentlyFiringGun) return;
            var newGun = GetLowestGunToFire();
            if (newGun != null) currentlyFiringGun = newGun;
        }

        public void Tick()
        {
            
            
            for (var i = guns.Count - 1; i >= 0; i--)
            {
                var gun = guns[i];
                if (gun == null || gun.Closed)
                {
                    guns.RemoveAt(i);
                    gunsReference.RemoveAt(i);
                    //gunRechargeTimes.RemoveAt(i);
                    gunRechargePercentages.RemoveAt(i);
                    continue;
                }

                gun.Tick();
            }
        }
        
        public void SlowTick(double range, double sigma)
        {

            if (guns.Count == 0) return;
            gunIndex = (gunIndex + 1) % guns.Count;

            var gun = guns[gunIndex];
            if (gun == null || gun.Closed || !program.GridTerminalSystem.CanAccess(gun.actualGun))
            {
                guns.RemoveAt(gunIndex);
                gunsReference.RemoveAt(gunIndex);
                //gunRechargeTimes.RemoveAt(gunIndex);
                gunRechargePercentages.RemoveAt(gunIndex);
                return;
            }

            gun.SlowTick();
            gun.range = range;
            gun.sigma = sigma;
            while (gunRechargePercentages.Count < gunIndex + 1) gunRechargePercentages.Add(0);
            //gunRechargeTimes[gunIndex] = gun.RemainingChargeSeconds;
            gunRechargePercentages[gunIndex] = gun.chargePercent;
        }

        public int AreAvailable()
        {
            var availableGuns = 0;
            for (var i = guns.Count - 1; i >= 0; i--)
            {
                var gun = guns[i];
                var isGunAvailable = gun.Available;
                availableGuns += isGunAvailable ? 1 : 0;
                this.availableGuns[gun] = isGunAvailable;
            }

            if (_GunMode == GunMode.Volley && currentGunIsVolleyFiring) TickActiveGun();
            return availableGuns;
        }


        private void TickActiveGun()
        {
            currentVolleyFrame++;
            if (currentVolleyFrame >= volleyDelayFrames)
            {
                currentVolleyFrame = 0;
                currentGunIsVolleyFiring = false;
                IncrementActiveGun();
            }
        }

        private void IncrementActiveGun()
        {
            var currentIndex = guns.IndexOf(activeGun);
            activeGun = guns[(currentIndex + 1) % guns.Count];
        }

        public Vector3D GetAimingReferencePos(Vector3D fallback)
        {
            if (_GunMode == GunMode.Volley)
            {
                if (currentlyFiringGun == null) return fallback;
                if (availableGuns[currentlyFiringGun])
                    return currentlyFiringGun.GetPosition();
                return fallback;
            }

            var lowestTimeToFire = GetLowestTimeToFire(); // MUST be called to update the time to fire for each gun
            var averagePos = Vector3D.Zero;
            var activeGunCount = 0;
            var chargingGunCount = 0;
            for (var i = 0; i < guns.Count; i++)
            {
                var gun = guns[i];
                if (availableGuns[gun])
                {
                    if (gun.TimeToFire - secondDifferenceToGroupGunFiring > lowestTimeToFire)
                    {
                        chargingGunCount++;
                        continue;
                    }

                    ;
                    var GunPos = gun.GetPosition();
                    averagePos += GunPos;
                    activeGunCount++;
                }
            }


            if (activeGunCount == 0) return fallback;
            averagePos /= activeGunCount;
            //averagePos = Vector3D.Transform(averagePos, ToWorldMatrix);

            return averagePos;
        }

        public float GetLowestTimeToFire()
        {
            var lowestTimeToFire = float.MaxValue;
            for (var i = 0; i < guns.Count; i++)
            {
                var gun = guns[i];
                if (gun.Available && gun.Shoot)
                {
                    var timeToFire = gun.GetTimeToFire();
                    lowestTimeToFire = Math.Min(lowestTimeToFire, timeToFire);
                }
            }

            return lowestTimeToFire;
        }

        private Gun GetLowestGunToFire() //returns the gun with the lowest time to fire
        {
            Gun lowestGun = null;
            var lowestTimeToFire = float.MaxValue;
            for (var i = 0; i < guns.Count; i++)
            {
                var gun = guns[i];
                if (gun.Available && gun.Shoot)
                {
                    var timeToFire = gun.GetTimeToFire();
                    if (timeToFire < lowestTimeToFire)
                    {
                        lowestTimeToFire = timeToFire;
                        lowestGun = gun;
                    }
                }
            }

            return lowestGun;
        }

        public void Fire()
        {

            switch (_GunMode)
            {
                case GunMode.FireWhenReady:
                    for (var i = 0; i < guns.Count; i++)
                    {
                        var gun = guns[i];
                        if (availableGuns[gun])
                        {
                            gun.Enabled = true;
                            gun.Shoot = true;
                        }
                    }
                    break;
                case GunMode.Volley:
                    if (availableGuns[activeGun])
                    {
                        activeGun.Enabled = true;
                        activeGun.Shoot = true;
                        currentGunIsVolleyFiring = true;
                    }
                    break;
                case GunMode.WaitForAll:
                    var allReady = true;
                    
                    for (var i = 0; i < guns.Count; i++)
                    {
                        var gun = guns[i];
                        if (gun.PowerDraw > 0.002f) allReady = false;
                    }

                    if (allReady)
                    {
                        for (var i = 0; i < guns.Count; i++)
                        {
                            var gun = guns[i];
                            if (availableGuns[gun])
                            {
                                gun.Enabled = true;
                                gun.Shoot = true;
                            }
                        }
                    }
                    break;
            }
        }

        public void Cancel()
        {
            for (var i = 0; i < guns.Count; i++)
            {
                var gun = guns[i];
                if (availableGuns[gun])
                {
                    gun.Shoot = false;
                    gun.Enabled = false;
                }
                else
                {
                    gun.Shoot = false;
                    gun.Enabled = true; //ensure that uncharged guns will still accumulate power
                }
            }
        }

        public void Standby()
        {
            foreach (var gun in guns)
            {
                gun.Shoot = false;
                gun.Enabled = true;
            }
        }
    }
}