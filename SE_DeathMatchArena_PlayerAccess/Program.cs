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
using System.Security.Cryptography;
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
    public partial class Program : MyGridProgram
    {
        //TimeSpan timerGamePrepareToAnyPlayer = TimeSpan.FromSeconds(180);
        
        public MyGateway[] gateways = new MyGateway[6];

        internal MyControlRoom controlRoom;

        public MyArena arena;

        public string[] storedValues;

        public IMyTextSurface LCDDebug1;

        public IMyDoor StaffOnlyDoor1Top;
        public IMyDoor StaffOnlyDoor2Mid;
        public IMyDoor StaffOnlyDoorAPrepareZone;
        public IMyDoor StaffOnlyDoorBPrepareZone;
        public List<IMyDoor> StaffDoors;
        
        bool isError = false;

        public Program()
        {
            Me.Enabled = true;

            StaffOnlyDoor1Top = (IMyDoor)GridTerminalSystem.GetBlockWithName("StaffOnlyDoor1Top");
            StaffOnlyDoor2Mid = (IMyDoor)GridTerminalSystem.GetBlockWithName("StaffOnlyDoor2Mid");
            StaffOnlyDoorAPrepareZone = (IMyDoor)GridTerminalSystem.GetBlockWithName("StaffOnlyDoorAPrepareZone");
            StaffOnlyDoorBPrepareZone = (IMyDoor)GridTerminalSystem.GetBlockWithName("StaffOnlyDoorBPrepareZone");
            StaffDoors = new List<IMyDoor> { StaffOnlyDoor1Top, StaffOnlyDoor2Mid, StaffOnlyDoorAPrepareZone, StaffOnlyDoorBPrepareZone };

            for (int i = 0; i < 3; i++)
            {
                gateways[i] = new MyGateway(this)
                {
                    FirstDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewayFirstDoorA" + (i + 1).ToString()),
                    SecondDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewaySecondDoorA" + (i + 1).ToString()),
                    VolumeSensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayVolumeSensorA" + (i + 1).ToString()),
                    FlySensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayFlySensorA" + (i + 1).ToString()),
                    Button = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("GatewayButtonA" + (i + 1).ToString()),
                    LCDAction = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("GatewayLCDActionA" + (i + 1).ToString())
                };

                gateways[i + 3] = new MyGateway(this)
                {
                    FirstDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewayFirstDoorB" + (i + 1).ToString()),
                    SecondDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewaySecondDoorB" + (i + 1).ToString()),
                    VolumeSensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayVolumeSensorB" + (i + 1).ToString()),
                    FlySensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayFlySensorB" + (i + 1).ToString()),
                    Button = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("GatewayButtonB" + (i + 1).ToString()),
                    LCDAction = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("GatewayLCDActionB" + (i + 1).ToString())
                };
            }

            IMyTextSurfaceProvider surface;

            foreach (var gateway in gateways)
            {
                surface = (IMyTextSurfaceProvider)gateway.Button;

                for (int i = 0; i < 3; i++)
                {
                    surface.GetSurface(i).ContentType = ContentType.TEXT_AND_IMAGE;
                    surface.GetSurface(i).FontSize = 10f;
                    surface.GetSurface(i).FontColor = new Color(100, 100, 0);
                    surface.GetSurface(i).Alignment = TextAlignment.CENTER;
                    surface.GetSurface(i).TextPadding = 2f;
                    surface.GetSurface(i).WriteText((i + 1).ToString());
                }

                surface.GetSurface(3).ContentType = ContentType.TEXT_AND_IMAGE;
                surface.GetSurface(3).FontSize = 6f;
                surface.GetSurface(3).FontColor = new Color(100, 100, 0);
                surface.GetSurface(3).Alignment = TextAlignment.CENTER;
                surface.GetSurface(3).TextPadding = 16f;
                surface.GetSurface(3).WriteText("Выход");
            }

            controlRoom = new MyControlRoom(this)
            {
                Door = (IMyDoor)GridTerminalSystem.GetBlockWithName("ControlRoomDoor"),
                VolumeSensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("ControlRoomSensor"),
                Button1 = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("ControlRoomButton1"),
                ButtonExit = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("ControlRoomExit"),
                LCDControlRoom = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDControlRoom"),
            };

            surface = (IMyTextSurfaceProvider)controlRoom.Button1;

            for (int i = 0; i < 3; i++)
            {
                surface.GetSurface(i).ContentType = ContentType.TEXT_AND_IMAGE;
                surface.GetSurface(i).FontSize = 8f;
                surface.GetSurface(i).FontColor = new Color(100, 100, 0);
                surface.GetSurface(i).Alignment = TextAlignment.CENTER;
                surface.GetSurface(i).TextPadding = 10f;
                surface.GetSurface(i).WriteText((i + 1).ToString() + "x" + (i + 1).ToString());
            }

            surface.GetSurface(3).ContentType = ContentType.TEXT_AND_IMAGE;
            surface.GetSurface(3).FontSize = 5f;
            surface.GetSurface(3).FontColor = new Color(100, 100, 0);
            surface.GetSurface(3).Alignment = TextAlignment.CENTER;
            surface.GetSurface(3).TextPadding = 25f;
            surface.GetSurface(3).WriteText("Далее");

            surface = (IMyTextSurfaceProvider)controlRoom.ButtonExit;

            surface.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
            surface.GetSurface(0).FontSize = 4.5f;
            surface.GetSurface(0).FontColor = new Color(100, 100, 0);
            surface.GetSurface(0).Alignment = TextAlignment.CENTER;
            surface.GetSurface(0).TextPadding = 12f;
            surface.GetSurface(0).WriteText("Покинуть\nкомнату");

            if (Storage.Length > 0)
            {
                storedValues = Storage.Split(';');
                arena = new MyArena(int.Parse(storedValues[0]),
                    (IMySensorBlock)GridTerminalSystem.GetBlockWithName("PrepareZoneSensorA"),
                    (IMySensorBlock)GridTerminalSystem.GetBlockWithName("PrepareZoneSensorB"), 
                    this);
                controlRoom.modeSelected = int.Parse(storedValues[1]);
            }
            else arena = new MyArena(
                (IMySensorBlock)GridTerminalSystem.GetBlockWithName("PrepareZoneSensorA"),                   
                (IMySensorBlock)GridTerminalSystem.GetBlockWithName("PrepareZoneSensorB"), 
                this);

            arena.ArenaMainSensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("ArenaMainSensor");
            arena.PrepareZoneDoorA = (IMyDoor)GridTerminalSystem.GetBlockWithName("PrepareZoneDoorA");
            arena.PrepareZoneDoorB = (IMyDoor)GridTerminalSystem.GetBlockWithName("PrepareZoneDoorB");
            arena.ArenaDoorA = (IMyDoor)GridTerminalSystem.GetBlockWithName("ArenaEntranceDoorA");
            arena.ArenaDoorB = (IMyDoor)GridTerminalSystem.GetBlockWithName("ArenaEntranceDoorB");
            arena.PrepareZoneTurretA1 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("TurretArenaEntranceA1");
            arena.PrepareZoneTurretA2 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("TurretArenaEntranceA2");
            arena.PrepareZoneTurretB1 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("TurretArenaEntranceB1");
            arena.PrepareZoneTurretB2 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("TurretArenaEntranceB2");
            arena.ArenaTurret1 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("ArenaTurret1");
            arena.ArenaTurret2 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("ArenaTurret2");
            arena.ArenaTurret3 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("ArenaTurret3");
            arena.ArenaTurret4 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("ArenaTurret4");
            arena.ArenaTurret5 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("ArenaTurret5");
            arena.ArenaTurret6 = (IMyLargeInteriorTurret)GridTerminalSystem.GetBlockWithName("ArenaTurret6");
            arena.LCDMainIntro = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDMainIntro");
            arena.LCDArenaEntranceA1 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaEntranceA1");
            arena.LCDArenaEntranceA2 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaEntranceA2");
            arena.LCDArenaEntranceB1 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaEntranceB1");
            arena.LCDArenaEntranceB2 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaEntranceB2");
            arena.LCDArenaState1 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaState1");
            arena.LCDArenaState2 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaState2");
            arena.LCDArenaBoardA = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaBoardA");
            arena.LCDArenaBoardB = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDArenaBoardB");
            arena.LCDRespZoneA = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDRespZoneA");
            arena.LCDRespZoneB = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDRespZoneB");
            arena.PrepareZoneStoreA = (IMyStoreBlock)GridTerminalSystem.GetBlockWithName("PrepareZoneStoreA");
            arena.PrepareZoneStoreB = (IMyStoreBlock)GridTerminalSystem.GetBlockWithName("PrepareZoneStoreB");
            arena.CargoMain = (IMyCargoContainer)GridTerminalSystem.GetBlockWithName("CargoMain");
            arena.ArenaConnectorA = (IMyShipConnector)GridTerminalSystem.GetBlockWithName("ArenaConnectorA");
            arena.ArenaConnectorB = (IMyShipConnector)GridTerminalSystem.GetBlockWithName("ArenaConnectorB");
            arena.DummyA = (IMyTerminalBlock)GridTerminalSystem.GetBlockWithName("DummyA");
            arena.DummyB = (IMyTerminalBlock)GridTerminalSystem.GetBlockWithName("DummyB");
            LCDDebug1 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDDebug1");

            foreach (var surf in (new List<IMyTextSurfaceProvider>() { (IMyTextSurfaceProvider)arena.PrepareZoneStoreA, (IMyTextSurfaceProvider)arena.PrepareZoneStoreB } ))
            {
                surf.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
                surf.GetSurface(0).FontSize = 2.3f;
                surf.GetSurface(0).FontColor = new Color(100, 100, 0);
                surf.GetSurface(0).Alignment = TextAlignment.CENTER;
                surf.GetSurface(0).TextPadding = 5f;
                surf.GetSurface(0).WriteText("Тут\nможно продать\nначальные\nинструменты");

                surf.GetSurface(1).ContentType = ContentType.TEXT_AND_IMAGE;
                surf.GetSurface(1).PreserveAspectRatio = true;
                surf.GetSurface(1).ChangeInterval = 1f;
                var liststr = new List<string>();
                surf.GetSurface(1).GetSelectedImages(liststr);
                if (liststr.Count == 0)
                {
                    surf.GetSurface(1).AddImageToSelection("Arrow");
                    surf.GetSurface(1).AddImageToSelection("LCD_Economy_SC_Here");
                }
            }

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save()
        {
            Storage = ((int)arena.currentState).ToString() + ";" + controlRoom.modeSelected.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & (UpdateType.Terminal | UpdateType.Trigger | UpdateType.Script)) != 0)
            {
                if (argument.Contains("Setup"))
                {
                    Storage = "0;0";
                }
                else if (argument.Contains("SensorVolumeGateway"))
                {
                    int index = int.Parse(argument.Substring(argument.IndexOf("_") - 1, 1));

                    string action = argument.Substring(argument.IndexOf("_") + 1);

                    if (action == "on")
                    {
                        gateways[index].SensorVolume(true);
                    }
                    else if (action == "off")
                    {
                        gateways[index].SensorVolume(false);
                    }
                }
                else if (argument.Contains("SensorFlyGateway"))
                {
                    int index = int.Parse(argument.Substring(argument.IndexOf("_") - 1, 1));

                    string action = argument.Substring(argument.IndexOf("_") + 1);

                    if (action == "on")
                    {
                        gateways[index].timeStampStartHydrogenChecking = DateTime.Now;
                    }
                    else if (action == "off")
                    {
                        gateways[index].timeStampStartHydrogenChecking = null;
                    }
                }
                else if (argument.Contains("ButtonGateway"))
                {
                    int index = int.Parse(argument.Substring(argument.IndexOf("_") - 1, 1));

                    int action = int.Parse(argument.Substring(argument.IndexOf("_") + 1));

                    if (action == 4)
                    {
                        gateways[index].ResetGateway();
                    }
                    else
                    {
                        gateways[index].enteredCode += action;
                    }
                }
                else if (argument.Contains("SensorVolumeControlRoom"))
                {
                    string action = argument.Substring(argument.IndexOf("_") + 1);

                    if (action == "on")
                    {
                        controlRoom.SensorVolume(true);
                    }
                    else if (action == "off")
                    {
                        controlRoom.SensorVolume(false);
                    }
                }
                else if (argument.Contains("Button1ControlRoom"))
                {
                    int action = int.Parse(argument.Substring(argument.IndexOf("_") + 1));

                    if (controlRoom.isGameCanCancel)
                    {
                        if (action == 4)
                        {
                            controlRoom.cancelToken = "";
                        }
                        else
                        {
                            controlRoom.cancelToken += action;
                        }
                    }
                    else
                    {
                        if (action == 4)
                        {
                            controlRoom.ChangePage();
                        }
                        else
                        {
                            controlRoom.SelectMode(action);
                        }
                    }                    
                }
                else if (argument.Contains("ButtonExitControlRoom"))
                {
                    controlRoom.ResetControlRoom();
                }
                else if (argument.Contains("PrepareZoneSensorA"))
                {
                    string action = argument.Substring(argument.IndexOf("_") + 1);

                    if (action == "on")
                    {
                        arena.AddUserToTeam(true);
                    }
                    else if (action == "off")
                    {
                        
                    }
                }
                else if (argument.Contains("PrepareZoneSensorB"))
                {
                    string action = argument.Substring(argument.IndexOf("_") + 1);

                    if (action == "on")
                    {
                        arena.AddUserToTeam(false);
                    }
                    else if (action == "off")
                    {

                    }
                }
                else if (argument.Contains("Dummy"))
                {
                    if (argument == "DummyA_destroy")
                    {
                        arena.GiveBonus(true);
                    }
                    else if (argument == "DummyB_destroy")
                    {
                        arena.GiveBonus(false);
                    }
                }
                else Echo("Do nothing...");
            }

            if ((updateSource & (UpdateType.Update100 | UpdateType.Update10 | UpdateType.Update1)) != 0)
            {
                foreach (var door in StaffDoors)
                    if (door.OpenRatio == 1)
                        door.CloseDoor();

                /*
                if (arena.currentState == MyArena.StateGame.Ready && controlRoom.currentState != MyControlRoom.StateControlRoom.UserInside)
                    Runtime.UpdateFrequency = UpdateFrequency.Update100;
                else Runtime.UpdateFrequency = UpdateFrequency.Update10;
                */

                if (!isError)
                {
                    try
                    {
                        arena.RefreshState();

                        foreach (var gateway in gateways) if (gateway.isNeedReset) gateway.RefreshState();
                    }
                    catch (Exception ex)
                    {
                        string str = ex.Message + "\n" + ex.StackTrace;

                        LCDDebug1.WriteText(str);
                    }
                }                                
            }                        
        }
    }    
}
