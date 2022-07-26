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

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        List<IMyPistonBase> Pistons = new List<IMyPistonBase>();
        List<IMyProjector> Projs = new List<IMyProjector>();
        List<IMyShipWelder> WeldersAll = new List<IMyShipWelder>();
        List<IMyShipWelder> WeldersHand = new List<IMyShipWelder>();
        List<IMyShipWelder> WeldersPrep = new List<IMyShipWelder>();
        List<IMyShipGrinder> GrindersCleaner = new List<IMyShipGrinder>();
        IMyShipGrinder GrinderCutGrid;
        List<IMyMotorSuspension> Wheels = new List<IMyMotorSuspension>();
        IMyDoor DoorClientFirst, DoorClientSecond;
        bool ControlEnabled = true, ControlCooldowned = true, ReadyToStep = true, IsGridWelding = false, IsGridGrinding = false, IsGridPrepWeld = false;
        int CurrentProjIndex, CurrentStepIndex, LastPointIsWelding = 0, CountStepsIsWelding = 0;
        IMyTextPanel LCDStatus, LCDInstruction, LCDInfo1, LCDInfo2;
        IMyTextSurfaceProvider ControlPanel1, ControlPanel2, ButtonStep, ButtonStop, TerminalHand;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            LCDStatus = GridTerminalSystem.GetBlockWithName("LCDStatus") as IMyTextPanel;
            LCDInstruction = GridTerminalSystem.GetBlockWithName("LCDInstruction") as IMyTextPanel;
            LCDInfo1 = GridTerminalSystem.GetBlockWithName("LCDInfo1") as IMyTextPanel;
            LCDInfo2 = GridTerminalSystem.GetBlockWithName("LCDInfo2") as IMyTextPanel;
            ButtonStep = GridTerminalSystem.GetBlockWithName("ButtonStep") as IMyTextSurfaceProvider;
            ButtonStop = GridTerminalSystem.GetBlockWithName("ButtonStop") as IMyTextSurfaceProvider;
            ControlPanel1 = GridTerminalSystem.GetBlockWithName("ControlPanel1") as IMyTextSurfaceProvider;
            ControlPanel2 = GridTerminalSystem.GetBlockWithName("ControlPanel2") as IMyTextSurfaceProvider;
            DoorClientFirst = GridTerminalSystem.GetBlockWithName("DoorClientFirst") as IMyDoor;
            DoorClientSecond = GridTerminalSystem.GetBlockWithName("DoorClientSecond") as IMyDoor;
            GrinderCutGrid = GridTerminalSystem.GetBlockWithName("GrinderCutGrid") as IMyShipGrinder;

            CurrentProjIndex = -1;

            LCDInit();

            string[] StoredData = Storage.Split('|');
            if (StoredData.Length >= 1)
            {
                CurrentProjIndex = int.Parse(StoredData[0]);
                CurrentStepIndex = int.Parse(StoredData[1]);
                ControlEnabled = bool.Parse(StoredData[2]);
                ControlCooldowned = bool.Parse(StoredData[3]);
            }

            for (int i = 0; i <= 7; i++)
            {
                if ((GridTerminalSystem.GetBlockWithName("Proj" + i)) != null)
                    Projs.Add((IMyProjector)GridTerminalSystem.GetBlockWithName("Proj" + i));
                //Echo(Projs[i].CustomName);
            }
            for (int i = 0; i <= 2; i++)
            {
                Pistons.Add((IMyPistonBase)GridTerminalSystem.GetBlockWithName("Piston" + i));
                Echo(Pistons[i].CustomName);
            }
            for (int i = 0; i <= 2; i++)
            {
                WeldersPrep.Add((IMyShipWelder)GridTerminalSystem.GetBlockWithName("WelderPrep" + i));
                Echo(WeldersPrep[i].CustomName);
            }
            for (int i = 0; i <= 2; i++)
            {
                GrindersCleaner.Add((IMyShipGrinder)GridTerminalSystem.GetBlockWithName("GrinderCleaner" + i));
                Echo(WeldersPrep[i].CustomName);
            }


            Echo("InitClose");

        }

        public void Save()
        {
            Storage = String.Join("|", CurrentProjIndex, CurrentStepIndex, ControlEnabled, ControlCooldowned);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            /*
            List<MyItemType> ItemTypes = new List<MyItemType>();
            GridTerminalSystem.GetBlockWithName("Exch").GetInventory().GetAcceptedItems(ItemTypes);
            IMyTextPanel TempLCD = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("LCD4");
            foreach (MyItemType I in ItemTypes)
                TempLCD.WriteText(I.SubtypeId + "\n");
            */

            //GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(Pistons);

            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0)
            {
                if ((argument.Length > 8) && (argument.Remove(8) == "CallProj") && (ControlEnabled) && (ControlCooldowned))
                    {
                    CurrentProjIndex = int.Parse(argument.Remove(0, 8));
                    ControlPanelSwitchLCD();
                    LCDInit();
                    //for (int i = 0; i <= 7; i++) Projs[i].Enabled = false;
                    //CurrentProjIndex = int.Parse(argument.Remove(0, 8));
                    //Projs[CurrentProjIndex].Enabled = true;
                    //Echo(Projs[CurrentProjIndex].CustomName);
                    }

                else switch (argument)
                        {
                        case "BuildStep":
                            switch (CurrentStepIndex)
                            {
                                case 0: 
                                    BuildStep1();
                                    break;
                                case 1:
                                    BuildStep2();
                                    break;
                                case 2:
                                    BuildStep3();
                                    break;
                                case 3:
                                    BuildStep4();
                                    break;
                                default:
                                    Echo("Invalid Step");
                                    break;
                            }
                            break;

                        case "BuildStop":
                            {
                                //if (!IsGridWelding)
                                    BuildStop();
                                
                            }
                            break;

                        case "PistForw":
                            foreach (IMyPistonBase CurrentPiston in Pistons)
                                CurrentPiston.Velocity = 0.9f;
                            break;

                        case "PistBack":
                            foreach (IMyPistonBase CurrentPiston in Pistons)
                                CurrentPiston.Velocity = -0.03f;
                            break;

                        case "PistStop":
                            foreach (IMyPistonBase CurrentPiston in Pistons)
                                CurrentPiston.Velocity = 0;
                            break;

                        case "Test1":
                            break;

                        default:
                            Echo("Do nothing");
                            break;
                        }

            }

            if ((updateSource & (UpdateType.Update100)) != 0)
            {
                if (IsGridPrepWeld)
                {

                }

                if (IsGridWelding)
                {
                    /*if (Projs[CurrentProjIndex].RemainingBlocks == LastPointIsWelding)
                    {
                        if (CountStepsIsWelding >= 2)
                        {
                            foreach (IMyPistonBase CurrentPiston in Pistons)
                                CurrentPiston.Velocity = -0.05f;
                            LastPointIsWelding = 0;
                            CountStepsIsWelding = 0;
                        }
                        CountStepsIsWelding++;
                    }
                    else
                    {
                        foreach (IMyPistonBase CurrentPiston in Pistons)
                            CurrentPiston.Velocity = -0.015f;
                        LastPointIsWelding = Projs[CurrentProjIndex].RemainingBlocks;
                        CountStepsIsWelding = 0;
                    }*/

                    if (Projs[CurrentProjIndex].RemainingBlocks <= 1)
                        foreach (IMyPistonBase CurrentPiston in Pistons)
                            CurrentPiston.Velocity = -0.1f;

                    if ((Pistons[0].CurrentPosition <= 0.65) && (Pistons[1].CurrentPosition <= 0.65) && (Pistons[2].CurrentPosition <= 0.65))
                    {
                        foreach (IMyShipWelder Welder in WeldersHand)
                            Welder.Enabled = false;
                        IsGridWelding = false;
                        IsGridGrinding = true;
                        foreach (IMyShipGrinder CurrentGrinder in GrindersCleaner) CurrentGrinder.Enabled = true;
                        GrinderCutGrid.Enabled = true;
                    }
                    else 
                        foreach (IMyPistonBase CurrentPiston in Pistons) 
                            CurrentPiston.Velocity = -0.1f;


                    GridTerminalSystem.GetBlocksOfType<IMyMotorSuspension>(Wheels);
                    foreach (IMyMotorSuspension Wheel in Wheels)
                        if (!Wheel.IsAttached) Wheel.ApplyAction("Add Top Part");
                }

                if (IsGridGrinding)
                {
                    
                }

                if (!ReadyToStep)
                {
                    //List<IMyShipWelder> AllWelders = new List<IMyShipWelder>;
                    //GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(AllWelders);

                }

            }

        }

        void LCDInit()
        {
            ButtonStep.GetSurface(0).WriteText("Жми чтобы\nпродолжить");
            ButtonStop.GetSurface(0).WriteText("Жми чтобы\nсбросить");
            //Echo(TerminalHand.SurfaceCount.ToString());
            //TerminalHand.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
            //TerminalHand.GetSurface(0).Alignment = TextAlignment.CENTER;
            //TerminalHand.GetSurface(0).FontSize = 3;
            //TerminalHand.GetSurface(0).WriteText("Не забудь:\nдать доступ ВСЕМ");
        }
        void ControlPanelSwitchLCD()
        {
            switch (CurrentProjIndex)
            {
                case 0:
                    LCDInfo2.WriteText("Вертолёт");
                    break;
                case 1:
                    LCDInfo2.WriteText("Пустой проект");
                    break;
                case 2:
                    LCDInfo2.WriteText("Самолёт");
                    break;
                case 3:
                    LCDInfo2.WriteText("Пустой проект");
                    break;
                case 4:
                    LCDInfo2.WriteText("Пустой проект");
                    break;
                case 5:
                    LCDInfo2.WriteText("Пустой проект");
                    break;
                case 6:
                    LCDInfo2.WriteText("Пустой проект");
                    break;
                case 7:
                    LCDInfo2.WriteText("Пустой проект");
                    break;
                default:
                    Echo("Неверный номер проекта");
                    break;
            }
            
        }
        void BuildStop()
        {
            ControlEnabled = true;
            DoorClientFirst.Enabled = true;
            DoorClientSecond.CloseDoor();
            foreach (IMyPistonBase CurrentPiston in Pistons)
                CurrentPiston.Velocity = -0.9f;
            ReadyToStep = true;
            IsGridWelding = false;
            CurrentStepIndex = 0;
            foreach (IMyShipWelder Welder in WeldersHand)
                Welder.Enabled = false;
            LCDStatus.WriteText("Перезапуск цеха");
        }
        void BuildStep0()
        {
            IsGridWelding = false;
            CurrentStepIndex = 0;
        }
        void BuildStep1()
        {
            if ((DoorClientFirst.OpenRatio == 0) && (ReadyToStep))
            {
                CurrentStepIndex = 1;
                DoorClientFirst.Enabled = false;
                DoorClientSecond.OpenDoor();
                ControlEnabled = false;
                foreach (IMyPistonBase CurrentPiston in Pistons)
                    CurrentPiston.Velocity = 1.5f;
                for (int i = 0; i <= Projs.Count-1; i++) Projs[i].Enabled = false;
                //CurrentProjIndex = int.Parse(argument.Remove(0, 8));
                if (ReadyToStep) Projs[CurrentProjIndex].Enabled = true; else ButtonStep.GetSurface(0).WriteText("Нет проекта");
                //Echo(Projs[CurrentProjIndex].CustomName);
                
                //ReadyToStep = false;
                Save();
            }
            else
            {

            }
        }
        void BuildStep2()
        {

            List<MyItemType> AcceptedItemTypes = new List<MyItemType>();
            List<MyItemType> BuildingItemTypes = new List<MyItemType>();
            GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory().GetAcceptedItems(AcceptedItemTypes);
            IMyTextPanel LCDCompsProj2 = GridTerminalSystem.GetBlockWithName("LCDCompsProj2") as IMyTextPanel;
            StringBuilder TempStringBuilder = new StringBuilder();
            LCDCompsProj2.ReadText(TempStringBuilder);
            string[] TempString = TempStringBuilder.ToString().Split('|');
            string TempStringLCD = "";
            int[] CountComps = new int[TempString.Length / 2];
            for (int i = 0; i <= TempString.Length / 2 - 1; i++)
            {
                foreach (MyItemType temp in AcceptedItemTypes)
                {
                    //LCDInstruction.WriteText("Цикл" + i);
                    if (TempString[i * 2].Trim() == temp.SubtypeId)
                    {
                        BuildingItemTypes.Add(temp);
                        CountComps[i] = int.Parse(TempString[(i * 2) + 1].Trim());
                        TempStringLCD = TempStringLCD + temp.SubtypeId + CountComps[i] + "\n";
                        LCDInstruction.WriteText(TempStringLCD);
                        MyInventoryItem? item = GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory().FindItem(temp);
                        if ((item != null) && (item.GetValueOrDefault().Amount >= CountComps[i]))
                            GridTerminalSystem.GetBlockWithName("CargoMain1").GetInventory().TransferItemTo(GridTerminalSystem.GetBlockWithName("CargoHand2").GetInventory(),
                                item.GetValueOrDefault(), CountComps[i]);
                        else TempStringLCD = TempStringLCD + "^не хватает" + "\n";
                        break;
                    }
                    else LCDInfo1.WriteText("Ошибка получения списка компонентов");
                }
            }

            CurrentStepIndex = 2;
            GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(WeldersAll);
            WeldersHand.Clear();
            foreach (IMyShipWelder Welder in WeldersAll)
                if ((Welder.CustomName != "WelderBeaconSafe") &&
                    (Welder.CustomName != "WelderPrep0") &&
                    (Welder.CustomName != "WelderPrep1") &&
                    (Welder.CustomName != "WelderPrep2"))
                    WeldersHand.Add(Welder);
            LCDInfo1.WriteText(WeldersHand.Count.ToString());
            foreach (IMyShipWelder Welder in WeldersHand)
                Welder.Enabled = true;
            DoorClientSecond.CloseDoor();
            IsGridWelding = true;
            foreach (IMyPistonBase CurrentPiston in Pistons)
                CurrentPiston.Velocity = -0.03f;
        }
        void BuildStep3()
        {
            /*IMyRefinery Ref = (IMyRefinery)GridTerminalSystem.GetBlockWithName("Генератор поля");
            IMyShipConnector Conn1 = (IMyShipConnector)GridTerminalSystem.GetBlockWithName("Conn1");
            Ref.Enabled = false;
            Echo(Conn1.Status.ToString());
            Conn1.Connect();*/
        }
        void BuildStep4()
        {

        }






    }
}
