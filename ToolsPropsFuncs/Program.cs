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


        public Program()
        {
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var DummyA = (IMyTerminalBlock)GridTerminalSystem.GetBlockWithName("DummyA");
            var LCDTools3 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDTools3");
            string str = DummyA.BlockDefinition.TypeIdString + "\n"
                + DummyA.BlockDefinition.TypeIdStringAttribute + "\n"
                + DummyA.BlockDefinition.SubtypeId + "\n"
                + DummyA.BlockDefinition.SubtypeIdAttribute + "\n"
                + DummyA.BlockDefinition.TypeId + "\n"
                + DummyA.BlockDefinition.SubtypeName + "\n"
                + DummyA.GetType().ToString() + "\n";

            List<ITerminalAction> actions = new List<ITerminalAction>();
            DummyA.GetActions(actions);
            foreach (var action in actions) str += action.Id + "\n";

            List<ITerminalProperty> properties = new List<ITerminalProperty>();
            DummyA.GetProperties(properties);
            foreach (var property in properties) str += property.Id + "\n";

            LCDTools3.WriteText(str);
            
        }
    }
}
