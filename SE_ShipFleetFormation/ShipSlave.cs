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
    partial class ShipSlave : MyGridProgram
    {
        IMyRemoteControl RemoteControl;
        string BroadCastTag = "ShipMasterMatrix";
        IMyBroadcastListener _myBroadcastListener;
        MatrixD LastListenedMatrix;

        public ShipSlave()
        {
            LastListenedMatrix = MatrixD.Zero;
            RemoteControl = GridTerminalSystem.GetBlockWithName("RemoteControl") as IMyRemoteControl;
            _myBroadcastListener = IGC.RegisterBroadcastListener(BroadCastTag);
            _myBroadcastListener.SetMessageCallback(BroadCastTag);
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.IGC) > 0)
            {
                Me.GetSurface(0).WriteText("Прием");
                while (_myBroadcastListener.HasPendingMessage)
                {
                    MyIGCMessage myIGCMessage = _myBroadcastListener.AcceptMessage();
                    if (myIGCMessage.Tag == BroadCastTag)
                    {
                        if (myIGCMessage.Data is MatrixD)
                        {
                            MatrixD ShipMasterMatrix = (MatrixD)myIGCMessage.Data;
                            if (ShipMasterMatrix != LastListenedMatrix)
                            {
                                Me.GetSurface(0).WriteText(myIGCMessage.Data.ToString());
                                Vector3D CurrentCoords;
                                CurrentCoords.X = ShipMasterMatrix.M41 + 50;
                                CurrentCoords.Y = ShipMasterMatrix.M42 + 50;
                                CurrentCoords.Z = ShipMasterMatrix.M43 + 50;
                                RemoteControl.ClearWaypoints();
                                RemoteControl.AddWaypoint(CurrentCoords, "Way");
                                RemoteControl.SetAutoPilotEnabled(true);

                                LastListenedMatrix = ShipMasterMatrix;
                                //RemoteControl.WorldMatrix.M41
                            }


                        }
                    }


                }
            }


            //RemoteControl.AddWaypoint();
        }







        //==============================
    }
}
