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
        const long IDPBBots = 113556137669632624, IDPBProj = 124366902525606502, IDPBSphere = 107104917933929949;
        IMyTextSurface LCDStatus1, LCDStatus2, LCDStatus3, LCDItemInfo1, LCDItemInfo2, LCDItemInfo3, LCDItemInfo4, LCDService1;
        IMyLandingGear ChassisGrid;
        IMyShipWelder ProjWelder;
        IMyAirtightHangarDoor HangarDoorForProj;
        IMyDoor DoorCollectorStone, DoorCollectorComps, DoorWelderRoom;
        IMyCockpit CockpitProjControl;
        List<MyInventoryItem> CurrentItemsInToolBox = new List<MyInventoryItem>();
        bool IsHangarDoorOpening = false, IsProjecting = false, SafeLoop = true;
        int CounterUpdate, CounterUpdateItems, TimerAnswerSphere, TimerAnswerProj;


        public Program()
        {
            Me.Enabled = true;
            _UnicastListener = IGC.UnicastListener;
            _UnicastListener.SetMessageCallback("UnicastCallbackMain");
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            LCDStatus1 = GridTerminalSystem.GetBlockWithName("LCDStatus1") as IMyTextSurface;
            LCDStatus2 = GridTerminalSystem.GetBlockWithName("LCDStatus2") as IMyTextSurface;
            LCDStatus3 = GridTerminalSystem.GetBlockWithName("LCDStatus3") as IMyTextSurface;
            LCDItemInfo1 = GridTerminalSystem.GetBlockWithName("LCDItemInfo1") as IMyTextSurface;
            LCDItemInfo2 = GridTerminalSystem.GetBlockWithName("LCDItemInfo2") as IMyTextSurface;
            LCDItemInfo3 = GridTerminalSystem.GetBlockWithName("LCDItemInfo3") as IMyTextSurface;
            LCDItemInfo4 = GridTerminalSystem.GetBlockWithName("LCDItemInfo4") as IMyTextSurface;
            LCDService1 = GridTerminalSystem.GetBlockWithName("LCDService1") as IMyTextSurface;
            ProjWelder = GridTerminalSystem.GetBlockWithName("ProjWelder") as IMyShipWelder;
            CockpitProjControl = GridTerminalSystem.GetBlockWithName("CockpitProjControl") as IMyCockpit;
            HangarDoorForProj = GridTerminalSystem.GetBlockWithName("HangarDoorForProj") as IMyAirtightHangarDoor;
            DoorCollectorStone = GridTerminalSystem.GetBlockWithName("DoorCollectorStone") as IMyDoor;
            DoorCollectorComps = GridTerminalSystem.GetBlockWithName("DoorCollectorComps") as IMyDoor;
            DoorWelderRoom = GridTerminalSystem.GetBlockWithName("DoorWelderRoom") as IMyDoor;
            Me.GetSurface(0).WriteText(Me.GetId().ToString());
            ProjWelder.Enabled = true;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & (UpdateType.Terminal | UpdateType.Trigger | UpdateType.Script)) != 0)
            {
                if ((argument.Length > 10) && (argument.Remove(10) == "ProjRotate"))
                {
                    IGC.SendUnicastMessage(IDPBProj, "ProjRotate", argument.Remove(0, 10));

                }
                else switch (argument)
                    {
                        case ("PistonPush"):
                            PistonPush();
                            break;
                        case ("PistonBack"):
                            PistonBack();
                            break;
                        case ("SafeGridUp"):
                            SafeGridUp();
                            break;
                        case ("SafeGridDown"):
                            SafeGridDown();
                            break;
                        case ("GrindBots"):
                            GrindBots();
                            break;
                        case ("EnableBots"):
                            EnableBots();
                            break;
                        default:
                            Echo("Do nothing");
                            break;
                    }

            }

            if ((updateSource & (UpdateType.Update10)) != 0)
            {
                IMyTextSurface LCDRadar = GridTerminalSystem.GetBlockWithName("LCDRadar") as IMyTextSurface;
                LCDStatus1.WriteText(LCDRadar.GetText());

                if (CounterUpdateItems > 7)
                {
                    IGC.SendUnicastMessage(IDPBBots, "CallAnswerCargo", "CallCargo");
                    CounterUpdateItems = 0;
                }
                else CounterUpdateItems++;

                if (CounterUpdate > 50)
                {
                    string CurrentOrderForToolBox = "";
                    CurrentItemsInToolBox.Clear();
                    string[] TempString = Me.CustomData.Trim().Split('|');
                    GridTerminalSystem.GetBlockWithName("ToolBox").GetInventory(0).GetItems(CurrentItemsInToolBox);
                    for (int i = 0; i <= TempString.Length / 3 - 1; i++)
                    {
                        int CurrentItemAmount = 0;
                        foreach (MyInventoryItem TempItem in CurrentItemsInToolBox)
                        {
                            if ((TempString[i * 3].Trim() == TempItem.Type.TypeId) && (TempString[i * 3 + 1].Trim() == TempItem.Type.SubtypeId))
                            {
                                CurrentItemAmount += TempItem.Amount.ToIntSafe();
                            }
                        }
                        if ((int.Parse(TempString[i * 3 + 2].Trim()) > CurrentItemAmount))
                            CurrentOrderForToolBox += TempString[i * 3].Trim() + "|" + TempString[i * 3 + 1].Trim() +
                                "|" + (int.Parse(TempString[i * 3 + 2].Trim()) - CurrentItemAmount).ToString() + "|";
                    }
                    if (CurrentOrderForToolBox != "")
                    {
                        IGC.SendUnicastMessage(IDPBBots, "OrderForToolBox", CurrentOrderForToolBox);
                        Echo(CurrentOrderForToolBox);
                    }
                    CounterUpdate = 0;

                }
                else CounterUpdate++;

                if (TimerAnswerSphere > 60)
                {
                    SafeGridUp();
                    //Me.GetSurface(0).WriteText(Me.TryRun("SafeGridUp").ToString());
                    //SafeLoop = false;
                    TimerAnswerSphere = 0;
                }
                else
                {
                    TimerAnswerSphere++;
                }


                if (IsHangarDoorOpening)
                    if (HangarDoorForProj.OpenRatio == 1)
                    {
                        IGC.SendUnicastMessage(IDPBProj, "PistonMove", "PistonPush");
                        IsHangarDoorOpening = false;
                    }

                IGC.SendUnicastMessage(IDPBProj, "CallStatus", "Call");

                IGC.SendUnicastMessage(IDPBBots, "CallAnswerSphere", "Hello");

                if (IsProjecting)
                {
                }
            }

            if ((updateSource & UpdateType.IGC) > 0)
            {
                while (_UnicastListener.HasPendingMessage)
                {
                    MyIGCMessage myIGCMessage = _UnicastListener.AcceptMessage();

                    if (myIGCMessage.Tag.ToString() == "AnswerStatus")
                    {
                        string[] TempString = myIGCMessage.Data.ToString().Split('|');

                        if ((TempString[0].Trim() == "True") && (TempString[3].Trim() == "True"))
                        {
                            IsProjecting = true;
                            int Total = int.Parse(TempString[1]), Remainig = int.Parse(TempString[2]);
                            string StatusBar = "|";
                            for (int i = 0; i <= 29; i++)
                                if (i < (30 * (Total - Remainig) / Total))
                                    StatusBar += "]";
                                else StatusBar += ".";
                            StatusBar += "|";
                            LCDStatus2.WriteText("Всего блоков: " + TempString[1]
                            + "\nОсталось блоков: " + TempString[2]
                            + "\nЗаварено: " + (100 * (Total - Remainig) / Total) + " %\n" + StatusBar);
                        }
                        else
                        {
                            LCDStatus2.WriteText("Проектор выключен" + " " + TempString[0].Trim() + " " + TempString[3].Trim());
                            IsProjecting = false;
                        }

                        //Echo("Ответ от проектора");
                    }

                    if (myIGCMessage.Tag.ToString() == "AnswerSphere")
                    {
                        string TempString = myIGCMessage.Data.ToString().Trim();
                        if (TempString == "1")
                        {
                            SafeGridDown();
                            TimerAnswerSphere = 0;
                        }

                        Echo("Ответ от сферы");
                    }

                    if (myIGCMessage.Tag.ToString() == "AnswerCargo")
                    {
                        string[] TempStringCargo = myIGCMessage.Data.ToString().Split('|');
                        string[] TempStringMainList = LCDService1.GetText().Trim().Split('|');
                        //List<int> TempAmountForLCD = new List<int>();
                        //string[] TempLCDItemName, TempLCDItemAmount;
                        LCDItemInfo1.WriteText("");
                        LCDItemInfo2.WriteText("");
                        LCDItemInfo3.WriteText("");
                        LCDItemInfo4.WriteText("");

                        for (int i = 0; i <= TempStringMainList.Length / 3 - 1; i++)
                        {
                            //TempAmountForLCD.Add(0);
                            bool IsZero = true;
                            for (int j = 0; j <= TempStringCargo.Length / 2 - 1; j++)
                            {

                                if ((TempStringMainList[i * 3].Trim() == TempStringCargo[j * 2].Trim()) && (int.Parse(TempStringMainList[i * 3 + 2].Trim()) != 0))
                                {
                                    //TempLCDItemName[i] = TempStringMainList[i * 3 + 1].Trim() + "\n";
                                    int Total = int.Parse(TempStringMainList[i * 3 + 2].Trim()), Current = (int)float.Parse(TempStringCargo[j * 2 + 1].Trim());
                                    string StatusBar = "|";
                                    for (int k = 0; k < 40; k++)
                                        if (k < (40 * Current / Total))
                                            StatusBar += "]";
                                        else StatusBar += ".";
                                    StatusBar += "|";

                                    if (i < 42)
                                    {
                                        LCDItemInfo1.WriteText(TempStringMainList[i * 3 + 1].Trim() + "\n", true);
                                        LCDItemInfo2.WriteText(StatusBar + " " + Current + " / " + Total + "\n", true);
                                    }
                                    else
                                    {
                                        LCDItemInfo3.WriteText(TempStringMainList[i * 3 + 1].Trim() + "\n", true);
                                        LCDItemInfo4.WriteText(StatusBar + " " + Current + " / " + Total + "\n", true);
                                    }
                                    IsZero = false;
                                    break;
                                }

                            }

                            if (IsZero && (int.Parse(TempStringMainList[i * 3 + 2].Trim()) != 0))
                            {
                                int Total = int.Parse(TempStringMainList[i * 3 + 2].Trim()), Current = 0;
                                string StatusBar = "|";
                                for (int k = 0; k < 40; k++)
                                    if (k < (40 * Current / Total))
                                        StatusBar += "]";
                                    else StatusBar += ".";
                                StatusBar += "|";
                                if (i < 42)
                                {
                                    LCDItemInfo1.WriteText(TempStringMainList[i * 3 + 1].Trim() + "\n", true);
                                    LCDItemInfo2.WriteText(StatusBar + " " + Current + " / " + Total + "\n", true);
                                }
                                else
                                {
                                    LCDItemInfo3.WriteText(TempStringMainList[i * 3 + 1].Trim() + "\n", true);
                                    LCDItemInfo4.WriteText(StatusBar + " " + Current + " / " + Total + "\n", true);
                                }
                            }

                        }

                        Echo("Ответ от контейнера");
                    }

                }
            }
        }
        void SafeGridUp()
        {
            DoorWelderRoom.CloseDoor();
            //DoorCollectorStone.CloseDoor();
            DoorCollectorComps.CloseDoor();
            PistonBack();
        }
        void SafeGridDown()
        {
            DoorWelderRoom.OpenDoor();
            //DoorCollectorStone.OpenDoor();
            DoorCollectorComps.OpenDoor();
            PistonPush();
        }
        void PistonPush()
        {
            HangarDoorForProj.OpenDoor();
            IsHangarDoorOpening = true;
        }
        void PistonBack()
        {
            HangarDoorForProj.CloseDoor();
            IGC.SendUnicastMessage(IDPBProj, "PistonMove", "PistonBack");
        }
        void GrindBots()
        {
            IGC.SendUnicastMessage(IDPBBots, "GrindBots", "GrindBots");
        }
        void EnableBots()
        {
            IGC.SendUnicastMessage(IDPBBots, "EnableBots", "EnableBots");
        }





        //=========
    }
}
