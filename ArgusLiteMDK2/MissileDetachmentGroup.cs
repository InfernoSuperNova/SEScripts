using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    enum DetachmentState
    {
        DelayEnable,
        Enable,
        DelayDisable,
        Disable
    }
    public class MissileDetachmentGroup
    {
        private List<IMyThrust> _thrusters;
        private DetachmentState _state = DetachmentState.Enable;

        public MissileDetachmentGroup(List<IMyThrust> thrusters)
        {
            _thrusters = thrusters;
        }

        public void Detach()
        {
            // foreach (var thruster in _thrusters)
            // {
            //     thruster.Enabled = true;
            // }
            //
            // return;
            switch (_state)
            {
                case DetachmentState.DelayEnable:
                    _state = DetachmentState.Enable;
                    break;
                case DetachmentState.Enable:
                    foreach (var thruster in _thrusters)
                    {
                        thruster.Enabled = true;
                    }
                    _state = DetachmentState.DelayDisable;
                    break;
                case DetachmentState.DelayDisable:
                    _state = DetachmentState.Disable;
                    break;
                case DetachmentState.Disable:
                    foreach (var thruster in _thrusters)
                    {
                        thruster.Enabled = false;
                    }
                    _state = DetachmentState.DelayEnable;
                    break;
                    
            }
        }

        public void Reset()
        {
            _state = DetachmentState.Enable;
            foreach (var thruster in _thrusters)
            {
                thruster.Enabled = false;
            }
        }

    }
}