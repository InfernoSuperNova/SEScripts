using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public class CockpitFinder
    {
        
        private List<IMyShipController> _cockpits = new List<IMyShipController>();

        private IMyShipController _lastCockpit;
        
        public CockpitFinder(List<IMyTerminalBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var cockpit = block as IMyShipController;
                if (cockpit != null) _cockpits.Add(cockpit);
            }
        }
        
        public CockpitFinder(IMyShipController cockpit)
        {
            _cockpits.Add(cockpit);
        }

        public CockpitFinder(IMyBlockGroup group)
        {
            var blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);
            foreach (var block in blocks)
            {
                var cockpit = block as IMyShipController;
                if (cockpit != null) _cockpits.Add(cockpit);
            }
        }
        
        public CockpitFinder(IMyGridTerminalSystem grid)
        {
            grid.GetBlocksOfType(_cockpits);
        }

        public IMyShipController GetCockpit()
        {
            if (_cockpits.Count == 0) return null;
            
            if (_lastCockpit != null && _lastCockpit.IsUnderControl) return _lastCockpit;
            
            foreach (var cockpit in _cockpits)
            {
                if (cockpit.IsUnderControl)
                {
                    _lastCockpit = cockpit;
                    return cockpit;
                }
            }
            if (_lastCockpit == null) _lastCockpit = _cockpits[0];
            return _lastCockpit;
 
        } 
        
        
    }
}