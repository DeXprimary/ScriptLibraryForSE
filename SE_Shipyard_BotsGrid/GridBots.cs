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
        IMyUnicastListener _UnicastListener;
        IMyCargoContainer CargoMain1;
        List<IMyCargoContainer> CargoContainers = new List<IMyCargoContainer>();
        IMyPistonBase ShipyardPistonMain;
        IMyShipGrinder ShipyardGrinderMain;
        IMyShipWelder ShipyardWelderMain, Bot1, Bot2, Bot3, Bot4;
        IMyConveyorSorter SorterForToolBox;
        IMyShipConnector ConnectorForToolBox;
        IMyLandingGear Chassis2;
        IMyDoor DoorWelder;
        IMySafeZoneBlock SphereMain;
        //List<MyItemType> TempItemListForToolBox = new List<MyItemType>();
        //List<IMyProjector> Bots = new List<IMyProjector>();
        int counter = 0, SphereStartingTimer = 0, SphereStartingTimer2 = 0;
        bool IsNeedToGrind = false, IsSphereWork = false, IsSphereGrind = false;

        public Program()
        {
            ShipyardPistonMain = GridTerminalSystem.GetBlockWithName("ShipyardPistonMain") as IMyPistonBase;
            ShipyardGrinderMain = GridTerminalSystem.GetBlockWithName("ShipyardGrinderMain") as IMyShipGrinder;
            ShipyardWelderMain = GridTerminalSystem.GetBlockWithName("ShipyardWelderMain") as IMyShipWelder;
            Bot1 = GridTerminalSystem.GetBlockWithName("Bot1") as IMyShipWelder;
            Bot2 = GridTerminalSystem.GetBlockWithName("Bot2") as IMyShipWelder;
            Bot3 = GridTerminalSystem.GetBlockWithName("Bot3") as IMyShipWelder;
            Bot4 = GridTerminalSystem.GetBlockWithName("Bot4") as IMyShipWelder;
            SorterForToolBox = GridTerminalSystem.GetBlockWithName("SorterForToolBox") as IMyConveyorSorter;
            ConnectorForToolBox = GridTerminalSystem.GetBlockWithName("ConnectorForToolBox") as IMyShipConnector;
            CargoMain1 = GridTerminalSystem.GetBlockWithName("CargoMain1") as IMyCargoContainer;
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(CargoContainers);
            Chassis2 = GridTerminalSystem.GetBlockWithName("Chassis2") as IMyLandingGear;
            SphereMain = GridTerminalSystem.GetBlockWithName("SphereMain") as IMySafeZoneBlock;
            DoorWelder = GridTerminalSystem.GetBlockWithName("DoorWelder") as IMyDoor;
            _UnicastListener = IGC.UnicastListener;
            _UnicastListener.SetMessageCallback("UnicastCallbackBots");
            Me.GetSurface(0).WriteText(Me.GetId().ToString());
            Me.Enabled = true;
            SorterForToolBox.Enabled = true;
            ConnectorForToolBox.Enabled = true;
            IsSphereGrind = SphereMain.GetValueBool("SafeZoneGrindingCb");
            //GrindBots();
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {

            if ((updateSource & UpdateType.IGC) > 0)
            {
                while (_UnicastListener.HasPendingMessage)
                {
                    MyIGCMessage myIGCMessage = _UnicastListener.AcceptMessage();

                    if (myIGCMessage.Tag == "GrindBots") GrindBots();

                    if (myIGCMessage.Tag == "EnableBots") EnableBots();

                    if (myIGCMessage.Tag == "OrderForToolBox") OrderForToolBox(myIGCMessage.Data.ToString());

                    if (myIGCMessage.Tag == "CallAnswerCargo") CallAnswerCargo(myIGCMessage);

                    if (myIGCMessage.Tag == "CallAnswerSphere") CallAnswerSphere(myIGCMessage);

                    Me.GetSurface(1).WriteText(myIGCMessage.Tag.ToString());
                }
            }

            if ((updateSource & (UpdateType.Update1)) != 0)
            {
                Echo(IsSphereGrind.ToString());
                if (IsNeedToGrind)
                    if (ShipyardWelderMain != null)
                        if (ShipyardWelderMain.IsFunctional)
                        {
                            Echo("St2");

                            //if (!IsSphereGrind) SphereMain.GetProperty("SafeZoneGrindingCb").AsBool().SetValue(GridTerminalSystem.GetBlockWithName("SphereMain"), true);
                            //IsSphereGrind = true;
                            ShipyardGrinderMain.Enabled = true;
                        }
                        else if (counter < 150)
                        {
                            Echo("St3");
                            Echo(ShipyardWelderMain.IsFunctional.ToString());
                            ShipyardPistonMain.Velocity = 5f;
                            counter++;
                        }
                        else
                        {
                            Echo("St4");
                            ShipyardGrinderMain.Enabled = false;

                            //if (IsSphereGrind) SphereMain.GetProperty("SafeZoneGrindingCb").AsBool().SetValue(GridTerminalSystem.GetBlockWithName("SphereMain"), false);
                            //IsSphereGrind = false;

                            IsNeedToGrind = false;
                            ShipyardPistonMain.Velocity = -5f;
                            counter = 0;
                            DoorWelder.OpenDoor();
                            ShipyardWelderMain = GridTerminalSystem.GetBlockWithName("ShipyardWelderMain") as IMyShipWelder;
                            Bot1 = GridTerminalSystem.GetBlockWithName("Bot1") as IMyShipWelder;
                            Bot2 = GridTerminalSystem.GetBlockWithName("Bot2") as IMyShipWelder;
                            Bot3 = GridTerminalSystem.GetBlockWithName("Bot3") as IMyShipWelder;
                            Bot4 = GridTerminalSystem.GetBlockWithName("Bot4") as IMyShipWelder;
                            Runtime.UpdateFrequency = UpdateFrequency.None;
                        }
                    else
                    {

                        //if (!IsSphereGrind) SphereMain.GetProperty("SafeZoneGrindingCb").AsBool().SetValue(GridTerminalSystem.GetBlockWithName("SphereMain"), true);
                        //IsSphereGrind = true;
                        ShipyardGrinderMain.Enabled = true;
                        ShipyardWelderMain = GridTerminalSystem.GetBlockWithName("ShipyardWelderMain") as IMyShipWelder;
                    }

                if (!IsNeedToGrind)
                {

                }

                if (ShipyardWelderMain != null)
                    if (ShipyardWelderMain.IsFunctional && !IsNeedToGrind)
                    {
                        ShipyardWelderMain.Enabled = true;
                        DoorWelder.CloseDoor();
                    }



            }

            switch (argument)
            {
                case "GetBlockId":
                    break;

                case "GrindBots":
                    GrindBots();
                    break;

                case "CheckUp":
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    break;

                case "CheckDown":
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    break;

                default:
                    Echo("Do nothing");
                    break;

            }
        }
        void OrderForToolBox(string CurrentOrderForToolBox)
        {
            //CurrentItemsInToolBox.Clear();
            List<MyItemType> TempItemListForToolBox = new List<MyItemType>();
            GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory(0).GetAcceptedItems(TempItemListForToolBox);
            string[] TempString = CurrentOrderForToolBox.Split('|');
            //GridTerminalSystem.GetBlockWithName("ToolBox").GetInventory(0).GetItems(CurrentItemsInToolBox);
            for (int i = 0; i <= TempString.Length / 3 - 1; i++)
            {
                foreach (MyItemType TempItem in TempItemListForToolBox)
                {
                    if ((TempString[i * 3].Trim() == TempItem.TypeId) && (TempString[i * 3 + 1].Trim() == TempItem.SubtypeId))
                    {
                        if (GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory(0).ContainItems(int.Parse(TempString[i * 3 + 2].Trim()), TempItem))
                        {
                            SorterForToolBox.AddItem(TempItem);
                            GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory(0).TransferItemTo(
                                GridTerminalSystem.GetBlockWithName("ConnectorForToolBox").GetInventory(0),
                                GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory(0).FindItem(TempItem).GetValueOrDefault(),
                                int.Parse(TempString[i * 3 + 2].Trim()));
                            SorterForToolBox.RemoveItem(TempItem);
                        }
                        else Echo("Не достаточно предметов для пополнения ToolBox");
                        break;
                    }
                }

                /*
                GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory(0).FindItem
                int CurrentItemAmount = 0;
                
                if ((int.Parse(TempString[i * 3 + 2].Trim()) > CurrentItemAmount))
                    CurrentOrderForToolBox += TempString[i * 3].Trim() + "|" + TempString[i * 3 + 1].Trim() +
                        "|" + (int.Parse(TempString[i * 3 + 2].Trim()) - CurrentItemAmount).ToString() + "|";
                */
            }
            TempItemListForToolBox.Clear();
        }
        void CallAnswerCargo(MyIGCMessage myIGCMessage)
        {
            foreach (IMyCargoContainer TempCargo in CargoContainers)
            {
                List<MyInventoryItem> InventoryItems = new List<MyInventoryItem>();
                if (TempCargo.GetId() != CargoMain1.GetId()) TempCargo.GetInventory(0).GetItems(InventoryItems);
                foreach (MyInventoryItem TempItem in InventoryItems)
                {
                    TempCargo.GetInventory(0).TransferItemTo(CargoMain1.GetInventory(0), TempItem);
                }
                InventoryItems.Clear();
            }
            if (SphereMain.GetInventory(0).FindItem(MyItemType.MakeComponent("ZoneChip")).GetValueOrDefault().Amount < 2000) CargoMain1.GetInventory(0).TransferItemTo(SphereMain.GetInventory(0), CargoMain1.GetInventory(0).FindItem(MyItemType.MakeComponent("ZoneChip")).GetValueOrDefault());
            List<MyInventoryItem> InventoryItemsCargoMain1 = new List<MyInventoryItem>();
            CargoMain1.GetInventory(0).GetItems(InventoryItemsCargoMain1);
            string TempString = "";
            foreach (MyInventoryItem TempItem in InventoryItemsCargoMain1)
            {
                TempString += TempItem.Type.SubtypeId + "|" + TempItem.Amount + "|";
            }
            IGC.SendUnicastMessage(myIGCMessage.Source, "AnswerCargo", TempString);
            InventoryItemsCargoMain1.Clear();
        }
        void CallAnswerSphere(MyIGCMessage myIGCMessage)
        {
            Echo("Активность сферы: " + SphereMain.GetValueBool("SafeZoneCreate").ToString());
            if (SphereMain.GetValueBool("SafeZoneCreate"))
            {
                Echo(IsSphereWork.ToString());
                if (IsSphereWork)
                {
                    IGC.SendUnicastMessage(myIGCMessage.Source, "AnswerSphere", "1");
                }
                else
                {
                    if (SphereStartingTimer > 20)
                    {
                        Chassis2.Lock();
                        if (!Chassis2.IsLocked) IsSphereWork = true;
                        Chassis2.Unlock();
                        SphereStartingTimer = 0;
                    }
                    else SphereStartingTimer++;
                }
            }
            else
            {
                Echo(SphereStartingTimer2.ToString());
                if (SphereStartingTimer2 > 35)
                {
                    IsSphereWork = false;
                    SphereStartingTimer2 = 0;
                }
                else SphereStartingTimer2++;
            }
        }
        void GrindBots()
        {
            IsNeedToGrind = true;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }
        void EnableBots()
        {
            if ((Bot1 != null) && (Bot2 != null) && (Bot3 != null) && (Bot4 != null))
                if (Bot1.Enabled || Bot2.Enabled || Bot3.Enabled || Bot4.Enabled)
                {
                    Bot1.Enabled = false;
                    Bot2.Enabled = false;
                    Bot3.Enabled = false;
                    Bot4.Enabled = false;
                }
                else
                {
                    Bot1.Enabled = true;
                    Bot2.Enabled = true;
                    Bot3.Enabled = true;
                    Bot4.Enabled = true;
                }
        }




        // ===================
    }
}
