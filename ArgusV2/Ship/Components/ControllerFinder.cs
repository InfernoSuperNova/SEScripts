using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components
{
    public class ControllerFinder
    {
        
        private List<IMyShipController> _cockpits = new List<IMyShipController>();

        private IMyShipController _lastCockpit;
        
        public ControllerFinder(List<IMyTerminalBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var cockpit = block as IMyShipController;
                if (cockpit != null) _cockpits.Add(cockpit);
            }
        }
        
        public ControllerFinder(IMyShipController cockpit)
        {
            _cockpits.Add(cockpit);
        }

        public ControllerFinder(IMyBlockGroup group)
        {
            var blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);
            foreach (var block in blocks)
            {
                var cockpit = block as IMyShipController;
                if (cockpit != null) _cockpits.Add(cockpit);
            }
        }
        
        public ControllerFinder(IMyGridTerminalSystem grid)
        {
            grid.GetBlocksOfType(_cockpits);
        }

        public IMyShipController Get()
        {
            if (_cockpits.Count == 0) return null;
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