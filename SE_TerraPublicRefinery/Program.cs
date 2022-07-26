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
        long Ref1id, Ref2id;

        public Program()
        {
            Me.Enabled = true;
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Ref1id = GridTerminalSystem.GetBlockWithName("Refinery1").GetId();
            Ref2id = GridTerminalSystem.GetBlockWithName("Refinery2").GetId();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument)
            {
                case "GlassRight":
                    GridTerminalSystem.GetBlockWithName("GlassRight1").ApplyAction("Open");
                    GridTerminalSystem.GetBlockWithName("GlassRight2").ApplyAction("Open");
                    GridTerminalSystem.GetBlockWithName("GlassRight3").ApplyAction("Open");
                    GridTerminalSystem.GetBlockWithName("GlassRight4").ApplyAction("Open");
                    break;

                case "GlassLeft":
                    GridTerminalSystem.GetBlockWithName("GlassLeft1").ApplyAction("Open");
                    GridTerminalSystem.GetBlockWithName("GlassLeft2").ApplyAction("Open");
                    GridTerminalSystem.GetBlockWithName("GlassLeft3").ApplyAction("Open");
                    GridTerminalSystem.GetBlockWithName("GlassLeft4").ApplyAction("Open");
                    break;

                case "ProjLight":
                    GridTerminalSystem.GetBlockWithName("ProjLight1").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("ProjLight2").ApplyAction("OnOff");
                    break;

                case "Automat":
                    GridTerminalSystem.GetBlockWithName("Automat1").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Automat2").ApplyAction("OnOff");
                    break;

                case "TrusterBack":
                    GridTerminalSystem.GetBlockWithName("TrusterAtmoBack1").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("TrusterAtmoBack2").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("TrusterIonBack1").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("TrusterIonBack2").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("TrusterIonBack3").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("TrusterIonBack4").ApplyAction("OnOff");
                    break;

                case "Turret":
                    GridTerminalSystem.GetBlockWithName("Turret1").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret2").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret3").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret4").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret5").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret6").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret7").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret8").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret9").ApplyAction("OnOff");
                    GridTerminalSystem.GetBlockWithName("Turret10").ApplyAction("OnOff");
                    break;

                default:
                    break;

            }

            if ((updateSource & (UpdateType.Update100)) != 0)
            {
                GridTerminalSystem.GetBlockWithName("Connector1").ApplyAction("OnOff_On");
                GridTerminalSystem.GetBlockWithName("Connector2").ApplyAction("OnOff_On");

                if (((GridTerminalSystem.GetBlockWithId(Ref1id).GetInventory(0).CurrentVolume != 0) & 
                    (GridTerminalSystem.GetBlockWithId(Ref1id).IsWorking)) || 
                    ((GridTerminalSystem.GetBlockWithId(Ref2id).GetInventory(0).CurrentVolume != 0) & 
                    (GridTerminalSystem.GetBlockWithId(Ref2id).IsWorking)))
                {
                    GridTerminalSystem.GetBlockWithName("Smoke1").ApplyAction("OnOff_On");
                    GridTerminalSystem.GetBlockWithName("Smoke2").ApplyAction("OnOff_On");
                }
                else
                {
                    GridTerminalSystem.GetBlockWithName("Smoke1").ApplyAction("OnOff_Off");
                    GridTerminalSystem.GetBlockWithName("Smoke2").ApplyAction("OnOff_Off");
                }

                if (GridTerminalSystem.GetBlockWithName("Turret1").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret2").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret3").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret4").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret5").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret6").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret7").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret8").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret9").GetValueBool("OnOff") ||
                    GridTerminalSystem.GetBlockWithName("Turret10").GetValueBool("OnOff") )
                {
                    GridTerminalSystem.GetBlockWithName("AttentionLight1").ApplyAction("OnOff_On");
                    GridTerminalSystem.GetBlockWithName("AttentionLight2").ApplyAction("OnOff_On");
                    GridTerminalSystem.GetBlockWithName("AttentionLight3").ApplyAction("OnOff_On");
                }
                else
                {
                    GridTerminalSystem.GetBlockWithName("AttentionLight1").ApplyAction("OnOff_Off");
                    GridTerminalSystem.GetBlockWithName("AttentionLight2").ApplyAction("OnOff_Off");
                    GridTerminalSystem.GetBlockWithName("AttentionLight3").ApplyAction("OnOff_Off");
                }

                GridTerminalSystem.GetBlockWithId(Ref1id).CustomName = "Refinery1";
                GridTerminalSystem.GetBlockWithId(Ref2id).CustomName = "Refinery2";

                IMyTextSurface LCDInfo1 = GridTerminalSystem.GetBlockWithName("LCDInfo1") as IMyTextSurface;
                IMyTextSurface LCDInfo2 = GridTerminalSystem.GetBlockWithName("LCDInfo2") as IMyTextSurface;

                LCDInfo1.WriteText(WriteLCD(Ref1id, 1));
                LCDInfo2.WriteText(WriteLCD(Ref2id, 2));

                /*
                Me.CustomData = "";
                List<ITerminalAction> Acts = new List<ITerminalAction>();
                GridTerminalSystem.GetBlockWithName("GlassLeft1").GetActions(Acts);
                foreach (ITerminalAction Act in Acts)
                Me.CustomData += Act.Id + "|" + Act.Name + "\n";
                */
            }



        }
        string WriteLCD(long Ref, int n)
        {
            float Total, Current;
            string StatusBar;
            List<MyInventoryItem> Items = new List<MyInventoryItem>();

            IMyRefinery Refinery = GridTerminalSystem.GetBlockWithId(Ref) as IMyRefinery;

            string WriteText = "Очистительная фабрика #" + n + ":\n\nИнвентарь с рудой ";

            Total = ((float)Refinery.GetInventory(0).MaxVolume);
            Current = ((float)Refinery.GetInventory(0).CurrentVolume);
            StatusBar = "|";
            for (int i = 0; i <= 49; i++)
                if (i < (50 * Current / Total))
                    StatusBar += "]";
                else StatusBar += ".";
            StatusBar += "|";
            WriteText += "(" + Math.Round(100 * Current / Total, 2) + "%)\n" + StatusBar + "\n";
            Refinery.GetInventory(0).GetItems(Items);
            foreach (MyInventoryItem Item in Items)
            {
                WriteText += Item.Type.SubtypeId + ": " + Math.Round((double)Item.Amount, 2) + " (кг)\n";
            }
            WriteText += "\n";

            WriteText += "Инвентарь со слитками ";
            Items.Clear();
            Total = ((float)Refinery.GetInventory(1).MaxVolume);
            Current = ((float)Refinery.GetInventory(1).CurrentVolume);
            StatusBar = "|";
            for (int i = 0; i <= 49; i++)
                if (i < (50 * Current / Total))
                    StatusBar += "]";
                else StatusBar += ".";
            StatusBar += "|";
            WriteText += "(" + Math.Round(100 * Current / Total, 2) + "%)\n" + StatusBar + "\n";
            Refinery.GetInventory(1).GetItems(Items);
            foreach (MyInventoryItem Item in Items)
            {
                WriteText += Item.Type.SubtypeId + ": " + Math.Round((double)Item.Amount, 2) + " (кг)\n";
            }

            //LCDStatus2.WriteText("Всего блоков: " + TempString[1]
            //+ "\nОсталось блоков: " + TempString[2]
            //+ "\nЗаварено: " + (100 * (Total - Remainig) / Total) + " %\n" + StatusBar);


            return WriteText;
        }









        //=============================
    }
}
