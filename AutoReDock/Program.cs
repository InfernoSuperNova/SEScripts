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
    partial class Program : MyGridProgram
    {
        private IMyShipConnector _connector;


        private bool _run;
        public Program()
        {
            bool.TryParse(Storage, out _run);
            Echo("Turned " + (_run ? "on" : "off"));
            _connector = (IMyShipConnector)GridTerminalSystem.GetBlockWithName("Auto Connector");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
            Storage = _run.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) != 0)
            {
                Update();
            }
            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0) Command(argument);
        }

        private void Update()
        {
            if (_run) _connector.Connect();
        }

        private void Command(string arg)
        {
            if (arg == "toggle")
            {
                _run = !_run;
                Echo("Turned " + (_run ? "on" : "off"));
            }
            
        }
    }
}