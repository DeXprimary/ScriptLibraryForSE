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
        List<IMyCargoContainer> CargoContainers = new List<IMyCargoContainer>();
        IMyLandingGear Chassis2;
        IMySafeZoneBlock SphereMain;
        int counter = 0, SphereStartingTimer = 0, SphereStartingTimer2 = 0;
        bool IsNeedToGrind = false, IsSphereWork = false, IsSphereGrind = false;

        public Program()
        {
            Me.Enabled = true;
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(CargoContainers);
            //Chassis2 = GridTerminalSystem.GetBlockWithName("Chassis2") as IMyLandingGear;
            SphereMain = GridTerminalSystem.GetBlockWithName("SphereMain") as IMySafeZoneBlock;
            _UnicastListener = IGC.UnicastListener;
            _UnicastListener.SetMessageCallback("UnicastCallbackSphere");
            Me.GetSurface(0).WriteText(Me.GetId().ToString());
            //IsSphereGrind = SphereMain.GetValueBool("SafeZoneGrindingCb");
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

                    if (myIGCMessage.Tag == "SphereMode") SphereMode(myIGCMessage.Data.ToString());

                    //if (myIGCMessage.Tag == "CallAnswerSphere") CallAnswerSphere(myIGCMessage);

                    Me.GetSurface(1).WriteText(myIGCMessage.Tag.ToString());
                }
            }

            /*
            if ((updateSource & (UpdateType.Update1)) != 0)
            {
                Echo(IsSphereGrind.ToString());
                if (IsNeedToGrind)
                    if (ShipyardWelderMain != null)
                        if (ShipyardWelderMain.IsFunctional)
                        {
                            Echo("St2");

                            IsSphereGrind = true;
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
                        ShipyardGrinderMain.Enabled = true;
                        ShipyardWelderMain = GridTerminalSystem.GetBlockWithName("ShipyardWelderMain") as IMyShipWelder;
                    }

                //if (DoorWelder.)

                if (ShipyardWelderMain != null)
                    if (ShipyardWelderMain.IsFunctional && !IsNeedToGrind)
                    {
                        ShipyardWelderMain.Enabled = true;
                        DoorWelder.CloseDoor();
                    }
            }
            */

            switch (argument)
            {
                case "GetBlockId":
                    break;

                default:
                    Echo("Do nothing");
                    break;

            }
        }
        /*
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
        */
        void SphereMode(string SphereMode)
        {
            if (SphereMode == "SphereCollapse")
                for (int i = 0; i <= 10; i++)
                    SphereMain.ApplyAction("DecreaseSafeZoneXSlider");
            else
                for (int i = 0; i <= 10; i++)
                    SphereMain.ApplyAction("IncreaseSafeZoneXSlider");

        }







        //==============
    }
}
