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

        public MyControlRoom controlRoom;

        public MyArena arena;

        public Program()
        {
            Me.Enabled = true;

            for (int i = 0; i < 3; i++)
            {
                gateways[i] = new MyGateway
                {
                    FirstDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewayFirstDoorA" + (i + 1).ToString()),
                    SecondDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewaySecondDoorA" + (i + 1).ToString()),
                    VolumeSensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayVolumeSensorA" + (i + 1).ToString()),
                    FlySensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayFlySensorA" + (i + 1).ToString()),
                    Button = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("GatewayButtonA" + (i + 1).ToString()),
                    LCDAction = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("GatewayLCDActionA" + (i + 1).ToString())
                };

                gateways[i + 3] = new MyGateway
                {
                    FirstDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewayFirstDoorB" + (i + 1).ToString()),
                    SecondDoor = (IMyDoor)GridTerminalSystem.GetBlockWithName("GatewaySecondDoorB" + (i + 1).ToString()),
                    VolumeSensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayVolumeSensorB" + (i + 1).ToString()),
                    FlySensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("GatewayFlySensorB" + (i + 1).ToString()),
                    Button = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("GatewayButtonB" + (i + 1).ToString()),
                    LCDAction = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("GatewayLCDActionB" + (i + 1).ToString())
                };
            }

            controlRoom = new MyControlRoom(this)
            {
                Door = (IMyDoor)GridTerminalSystem.GetBlockWithName("ControlRoomDoor"),
                VolumeSensor = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("ControlRoomSensor"),
                Button1 = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("ControlRoomButton1"),
                ButtonExit = (IMyButtonPanel)GridTerminalSystem.GetBlockWithName("ControlRoomExit"),
                LCDControlRoom = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDControlRoom"),
            };

            if (Storage.Length > 0)
            {
                string[] storedValues = Storage.Split(';');
                arena = new MyArena(int.Parse(storedValues[0]), this);
                controlRoom.modeSelected = int.Parse(storedValues[1]);
            }
            else arena = new MyArena(this);

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
                if (argument.Contains("SensorVolume"))
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
                else if (argument.Contains("SensorFly"))
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
                else if (argument.Contains("Button1ControlRoom"))
                {
                    int action = int.Parse(argument.Substring(argument.IndexOf("_") + 1));

                    if (controlRoom.isGameStarted)
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
                else Echo("Do nothing...");

                /*
                switch (argument)
                {
                    case "ButtonA1_4":
                        Echo("asd");
                        break;
                    default: 
                        Echo("default");
                        break;
                }*/
            }

            if ((updateSource & (UpdateType.Update100 | UpdateType.Update10 | UpdateType.Update1)) != 0)
            {
                arena.RefreshState();
                /*
                if (arena.currentState == MyArena.StateGame.Ready)
                {
                    
                }

                if (arena.currentState == MyArena.StateGame.Prepare)
                {
                    
                }
                */
            }                        
        }
        /*
        public int GetMyHash(string code)
        {
            return  code.GetHashCode() ^ 77;
        }*/
    }

    
}
