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
        IMyPistonBase ShipyardProjPiston;
        IMyUnicastListener _UnicastListener;
        IMyShipMergeBlock MergeMain;
        IMyProjector Projector;

        public Program()
        {
            Projector = GridTerminalSystem.GetBlockWithName("Projector") as IMyProjector;
            MergeMain = GridTerminalSystem.GetBlockWithName("MergeMain") as IMyShipMergeBlock;
            ShipyardProjPiston = GridTerminalSystem.GetBlockWithName("ShipyardProjPiston") as IMyPistonBase;
            _UnicastListener = IGC.UnicastListener;
            _UnicastListener.SetMessageCallback("UnicastCallbackProj");
            Me.GetSurface(0).WriteText(Me.GetId().ToString());
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            //Me.GetSurface(1).WriteText("");
            if ((updateSource & UpdateType.IGC) > 0)
            {
                while (_UnicastListener.HasPendingMessage)
                {

                    MyIGCMessage myIGCMessage = _UnicastListener.AcceptMessage();
/*
                    switch (myIGCMessage.Tag)
                    {

                        case "PistonMove":
                            if (myIGCMessage.Data.ToString() == "PistonPush")
                            {
                                MergeMain.Enabled = true;
                                ShipyardProjPiston.Velocity = 5f;
                            }
                            else if (myIGCMessage.Data.ToString() == "PistonBack")
                            {
                                MergeMain.Enabled = false;
                                ShipyardProjPiston.Velocity = -5f;
                            }
                            break;

                        case "CallStatus":
                            if ((Projector.Enabled) && (Projector.IsProjecting))
                                IGC.SendUnicastMessage(myIGCMessage.Source, "AnswerStatus",
                                Projector.Enabled.ToString() + "|" +
                                Projector.TotalBlocks.ToString() + "|" +
                                Projector.RemainingBlocks.ToString() + "|" +
                                Projector.IsProjecting.ToString());
                            break;

                        case "ProjRotate":
                            Vector3I Temp = Projector.ProjectionRotation;
                            switch (int.Parse(myIGCMessage.Data.ToString().Trim()))
                            {
                                case 1: if (Projector.ProjectionRotation.X >= 2) Temp.X = -2; else Temp.X++; break;
                                case 2: if (Projector.ProjectionRotation.X <= -2) Temp.X = 2; else Temp.X--; break;
                                case 3: if (Projector.ProjectionRotation.Y >= 2) Temp.Y = -2; else Temp.Y++; break;
                                case 4: if (Projector.ProjectionRotation.Y <= -2) Temp.Y = 2; else Temp.Y--; break;
                                case 5: if (Projector.ProjectionRotation.Z >= 2) Temp.Z = -2; else Temp.Z++; break;
                                case 6: if (Projector.ProjectionRotation.Z <= -2) Temp.Z = 2; else Temp.Z--; break;
                            }
                            Projector.ProjectionRotation = Temp;
                            break;

                        default:
                            Echo("Do nothing");
                            break;
                    }
*/                    
                    
                    if (myIGCMessage.Tag == "PistonMove")
                    {
                        if (myIGCMessage.Data.ToString() == "PistonPush")
                        {
                            MergeMain.Enabled = true;
                            ShipyardProjPiston.Velocity = 5f;
                        }
                        else if (myIGCMessage.Data.ToString() == "PistonBack")
                        {
                            MergeMain.Enabled = false;
                            ShipyardProjPiston.Velocity = -5f;
                        }
                    

                        Me.GetSurface(1).WriteText(myIGCMessage.Data.ToString());
                    }
                    
                    if (myIGCMessage.Tag.ToString() == "CallStatus")
                    {
                        if ((Projector.Enabled) && (Projector.IsProjecting) && (Projector.IsFunctional))
                            IGC.SendUnicastMessage(myIGCMessage.Source, "AnswerStatus",
                            Projector.Enabled.ToString() + "|" +
                            Projector.TotalBlocks.ToString() + "|" +
                            Projector.RemainingBlocks.ToString() + "|" +
                            Projector.IsProjecting.ToString());
                        Me.GetSurface(1).WriteText(myIGCMessage.Data.ToString());
                    }

                    if (myIGCMessage.Tag == "ProjRotate")
                    {
                        Vector3I TempRotation = Projector.ProjectionRotation;
                        Vector3I TempOffset = Projector.ProjectionOffset;
                        switch (int.Parse(myIGCMessage.Data.ToString().Trim()))
                        {
                            case 1: if (Projector.ProjectionRotation.X >= 2) TempRotation.X = -1; else TempRotation.X++; break;
                            case 2: if (Projector.ProjectionRotation.X <= -2) TempRotation.X = 1; else TempRotation.X--; break;
                            case 3: if (Projector.ProjectionRotation.Y >= 2) TempRotation.Y = -1; else TempRotation.Y++; break;
                            case 4: if (Projector.ProjectionRotation.Y <= -2) TempRotation.Y = 1; else TempRotation.Y--; break;
                            case 5: if (Projector.ProjectionRotation.Z >= 2) TempRotation.Z = -1; else TempRotation.Z++; break;
                            case 6: if (Projector.ProjectionRotation.Z <= -2) TempRotation.Z = 1; else TempRotation.Z--; break;

                            case 7: TempOffset.X--; break;
                            case 8: TempOffset.X++; break;
                            case 9: TempOffset.Y--; break;
                            case 10: TempOffset.Y++; break;
                            case 11: TempOffset.Z--; break;
                            case 12: TempOffset.Z++; break;

                        }
                        Projector.ProjectionOffset = TempOffset;
                        Projector.ProjectionRotation = TempRotation;
                        Projector.UpdateOffsetAndRotation();
                        Me.GetSurface(1).WriteText(myIGCMessage.Data.ToString());
                    }

                    if (myIGCMessage.Tag == "ProjMove")
                    {
                        //Vector3 qwe = myIGCMessage.Data.;
                        //Vector3 Temp = Projector.ProjectionOffset;
                        //if (Temp.X != 0) 
                        //
                        //Projector.ProjectionOffset = Temp;
                    }


                }
            }
        }





//===========
    }
}
