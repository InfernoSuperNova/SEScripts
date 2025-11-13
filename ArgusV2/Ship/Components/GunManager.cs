using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components
{
    public class GunManager
    {
        private List<Gun> _guns = new List<Gun>();
        
        public GunManager(List<IMyTerminalBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var gun = block as IMyUserControllableGun;
                if (gun != null) _guns.Add(new Gun(gun));
            }
        }

        public void EarlyUpdate(int frame)
        {
            foreach (var gun in _guns) gun.EarlyUpdate(frame);
        }

        public void LateUpdate(int frame)
        {
            foreach (var gun in _guns) gun.LateUpdate(frame);
        }

        public void FireAll()
        {
            foreach (var gun in _guns) gun.Fire();
        }

        public void CancelAll()
        {
            foreach (var gun in _guns) gun.Cancel();
        }
        
    }
}