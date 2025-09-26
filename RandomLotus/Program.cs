
using System;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using HarmonyLib;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        
        private readonly List<IMyShipMergeBlock> _mergeBlocks;
        private readonly List<IMyWarhead> _warheads;
        private int _groupCount = 0;

        private int _warheadLaunchDelay = 4; // frames

        private string _warheadGroupTag = "W";
        public Program()
        {
            _mergeBlocks = new List<IMyShipMergeBlock>();
            _warheads = new List<IMyWarhead>();
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            DiscoverGroupCount();
            DiscoverNewBlocks();
        }

        private Random _rng = new Random();
        
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update1) != 0) RunUpdate();
            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0) RunArgument(argument);
        }

        private void RunArgument(string arg)
        {
            if (arg == "start")
            {
                _launch = true;
            }
        }
        private bool _launch = false;

        private int _frame = 0;
        private void RunUpdate()
        {
            _frame++;
            if (_launch)
            {
                if (_frame % _warheadLaunchDelay != 0) return;
                int count = _mergeBlocks.Count;
                if (count == 0)
                {
                    Echo("No warheads found");
                    return;
                }
                int index = _rng.Next(count);
                _mergeBlocks[index].Enabled = false;
                _warheads[index].IsArmed = true;
                DiscoverNewBlocks();
                Echo(_mergeBlocks.Count.ToString());
            }
        }

        private int groupCount = 0;

        private void DiscoverGroupCount()
        {
            int index = 0;
            while (true)
            {
                index++;
                IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(_warheadGroupTag + index);
                if (group == null) return;
                groupCount = index;
            }
        }
        private void DiscoverNewBlocks()
        {
            _mergeBlocks.Clear();
            _warheads.Clear();

            for (int i = 0; i <= groupCount; i++)
            {
                IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(_warheadGroupTag + i);
                if (group == null) continue;
                var warheads = new List<IMyWarhead>();
                var mergeBlocks = new List<IMyShipMergeBlock>();
                group.GetBlocksOfType(warheads);
                group.GetBlocksOfType(mergeBlocks);
                if (warheads.Count > 0 && mergeBlocks.Count > 0 && warheads[0].IsFunctional && mergeBlocks[0].IsFunctional)
                {
                    _warheads.Add(warheads[0]);
                    _mergeBlocks.Add(mergeBlocks[0]);
                }
            }
        }
    }
}