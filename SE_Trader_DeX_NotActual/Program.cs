using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;
using Sandbox.Game.Gui;
using VRageRender;
using SharpDX.Toolkit;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        IMyProgrammableBlock MPB;
        IMyTextPanel LCDMenu1, LCDMenu2, LCDIcon, LCDInfo;
        IMyInventory MainBuffer;
        List<MyMarketItem> MarketItemList = new List<MyMarketItem>();
        List<MyItemType> AcceptedItemsList = new List<MyItemType>();
        string[][] Menu = new string[9][];
        string SelectedItem = null;

        public Program()
        {
            Save();
            LCDMenu1 = GridTerminalSystem.GetBlockWithName("LCDMenu1") as IMyTextPanel;
            LCDMenu2 = GridTerminalSystem.GetBlockWithName("LCDMenu2") as IMyTextPanel;
            LCDIcon  = GridTerminalSystem.GetBlockWithName("LCDIcon")  as IMyTextPanel;
            LCDInfo  = GridTerminalSystem.GetBlockWithName("LCDInfo")  as IMyTextPanel;
            LCDMenu1.ContentType = ContentType.TEXT_AND_IMAGE;
            LCDMenu1.Alignment = TextAlignment.RIGHT;
            LCDMenu1.ClearImagesFromSelection();
            LCDMenu2.ContentType = ContentType.TEXT_AND_IMAGE;
            LCDMenu2.Alignment = TextAlignment.LEFT;
            LCDMenu2.ClearImagesFromSelection();

            MainBuffer = GridTerminalSystem.GetBlockWithId(long.Parse(Storage)).GetInventory() as IMyInventory;
            MainBuffer.GetAcceptedItems(AcceptedItemsList);
            MPB = GridTerminalSystem.GetBlockWithName("MPB") as IMyProgrammableBlock;
            GetCustomData();

            Menu[0][1] = "Компоненты";
            Menu[0][2] = "Слитки";
            Menu[0][3] = "Руда";
            Menu[0][4] = "Другое";
            foreach (MyMarketItem item in MarketItemList)
            {
                switch (item.TypeId)
                {
                    case "Component":
                        Menu[1][i1] = item.SubtypeId
                }
            }

            //MyItemType ItemType = AcceptedItemsList[2];
            //MainBuffer.TransferItemTo(GridTerminalSystem.GetBlockWithName("Storage").GetInventory(), GridTerminalSystem.GetBlockWithName("Storage").GetInventory().FindItem(ItemType).Value, MyFixedPoint.Round(10));

            /*
            IMyTextSurface panel = Me.GetSurface(0);
            List<String> sprites = new List<String>();
            panel.GetSprites(sprites);
            panel.WriteText("1");
            foreach (string s in sprites)
            {
                panel.WriteText(s);
            }

            panel.AddImagesToSelection(sprites);
            List<String> images = new List<String>();
            panel.GetSelectedImages(images);
            panel.ClearImagesFromSelection();
            */

        }

        public void Save()
        {
            if (GridTerminalSystem.GetBlockWithId(long.Parse(Storage)) == null)
            Storage = GridTerminalSystem.GetBlockWithName("MainBuffer").EntityId.ToString();
        }

        public void SetCustomData()
        {
            if (MainBuffer == null) Echo("MainBuffer - null");
            if (MPB == null) Echo("MPB - null");
            MPB.CustomData = "TypeId | SubtypeId | ItemLocalName | BasePrice | CurveLength | Extension |\n";
            foreach (MyItemType I in AcceptedItemsList)
            {
                string v = $"{I.TypeId} | {I.SubtypeId} | 0 | 0 | 0 | 0 |\n";
                v = v.Substring(16); 
                MPB.CustomData += v;
            }
        }

        public void GetCustomData()
        {
            string[] TempString = MPB.CustomData.Split('|');
            for (int i = 0; i < TempString.Length / 6 - 1; i++)
            {
                MyItemType TempItemType = null;
                foreach (MyItemType temp in AcceptedItemsList)
                {
                    if ((TempString[(i + 1) * 6].Trim() == temp.TypeId.Substring(16)) && (TempString[(i + 1) * 6 + 1].Trim() == temp.SubtypeId))
                    {
                        TempItemType = temp;
                        break;
                    }
                }
                if (TempItemType == null) Echo("GetCustomData Error");
                MyMarketItem tempItem = new MyMarketItem(   TempItemType,
                                                            TempString[(i + 1) * 6].Trim(), 
                                                            TempString[(i + 1) * 6 + 1].Trim(), 
                                                            TempString[(i + 1) * 6 + 2].Trim(), 
                                                            double.Parse(TempString[(i + 1) * 6 + 3].Trim()), 
                                                            double.Parse(TempString[(i + 1) * 6 + 4].Trim()), 
                                                            double.Parse(TempString[(i + 1) * 6 + 5].Trim()));
                MarketItemList.Add(tempItem);
            }


            /*
            string[] TempString = MPB.CustomData.Split('|');
            ItemTypeId = new string[TempString.Length / 6 - 1];
            ItemSubtypeId = new string[TempString.Length / 6 - 1];
            ItemLocalName = new string[TempString.Length / 6 - 1];
            ItemBasePrise = new double[TempString.Length / 6 - 1];
            ItemBaseCurve = new double[TempString.Length / 6 - 1];
            Extension = new double[TempString.Length / 6 - 1];
            for (int i = 0; i < TempString.Length / 6 - 1; i++)
            {
                ItemTypeId[i] = TempString[(i + 1) * 6].Trim();
                ItemSubtypeId[i] = TempString[(i + 1) * 6 + 1].Trim();
                ItemLocalName[i] = TempString[(i + 1) * 6 + 2].Trim();
                ItemBasePrise[i] = double.Parse(TempString[(i + 1) * 6 + 3].Trim());
                ItemBaseCurve[i] = double.Parse(TempString[(i + 1) * 6 + 4].Trim());
                Extension[i] = double.Parse(TempString[(i + 1) * 6 + 5].Trim());
            }
            Echo($"LengthArr {TempString.Length}");
            Echo($"LengthArr {TempString.Length/6-1}");
            Echo($"{ItemTypeId[210]}/{ItemSubtypeId[210]} {ItemLocalName[210]} {ItemBasePrise[210]} {ItemBaseCurve[210]} {Extension[210]}");
            */
        }

        public void PaintLCDIcon()
        {
            if (SelectedItem == null)
            {

            }
            else
            {

            }
        }

        public void PaintMenu(int k)
        {

            Echo(Storage);
        }

        public void RefreshState()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument)
            {
                case "SetCustomData":
                    SetCustomData();
                    break;
                case "GetCustomData":
                    GetCustomData();
                    break;
                case "B1":
                    PaintMenu(1);
                    break;
                default:
                    Echo("Do nothing");
                    break;
            }
            
            
            //IMyProgrammableBlock MainPB = GridTerminalSystem.GetBlockWithName("PBtest1") as IMyProgrammableBlock;
            //MainPB.CustomData = "123";

            //IMyInventory MainBuffer = GridTerminalSystem.GetBlockWithName("MainBuffer").GetInventory() as IMyInventory;
            
            /*
            IMyCargoContainer Cargo = GridTerminalSystem.GetBlockWithName("MainBuffer") as IMyCargoContainer;
            if (Cargo == null) Echo("Ooops1");
            IMyInventory MainBuffer = Cargo.GetInventory();
            if (MainBuffer == null) Echo("Ooops1");
            List<MyItemType> items = new List<MyItemType>();
            //MainBuffer.GetAcceptedItems(items);
            //foreach (MyItemType temp in items) { Echo(temp.TypeId.); }


            IMyInventoryItem item = MainBuffer.
            MainBuffer.TransferItemTo(MySpriteDrawFrame,IMyAirtightSlideDoor,)

            for (int i = 0; i < Cargo.InventoryCount; i++)
            {
                IMyInventory inv = Cargo.GetInventory(i);
                Echo(inv.ToString());
                items.Clear();
                inv.GetAcceptedItems(items); //BOOM!
                Echo($"Inventory accepts {items.Count} types");
                foreach (MyItemType temp in items)
                {
                    Echo(temp.SubtypeId);
                }



            }
            */
        }

        struct MyMarketItem
        {
            public string TypeId;
            public string SubtypeId;
            public string LocalName;
            public double BasePrise;
            public double BaseCurve;
            public double Extension;
            public MyItemType ItemType;
            public MyMarketItem(MyItemType ItemType, string TypeId, string SubtypeId, string LocalName, double BasePrise, double BaseCurve, double Extension)
            {
                this.TypeId = TypeId;
                this.SubtypeId = SubtypeId;
                this.LocalName = LocalName;
                this.BasePrise = BasePrise;
                this.BaseCurve = BaseCurve;
                this.Extension = Extension;
                this.ItemType = ItemType;
            }

        }




    }
}
