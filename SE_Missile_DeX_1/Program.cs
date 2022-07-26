using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Numerics;
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
        const double commonSimSpeed = 0.016d;
        static int currentSplitSecond, tickCounter, tickCounterRayCast = 0, tickDelayRayCast = 0, cameraCounter = 0, cameraMisshotCounter = 0;
        bool firstRun = true, isTargetLocked = false;
        List<IMyCameraBlock> cameras = new List<IMyCameraBlock>();
        IMyTextSurface LCDMain;
        IMyCockpit mainCockpit;
        MyDetectedEntityInfo lastEntityInfo, previousEntityInfo;
        List<IMyTerminalBlock> triggerBlocks = new List<IMyTerminalBlock>();
        List<IMyShipMergeBlock> suspensionMerges = new List<IMyShipMergeBlock>();
        List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
        List<MyMissile> missiles = new List<MyMissile>();

                
        public Program()
        {            
            tickCounter = 0;
            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits);
            foreach (var cockpit in cockpits) if (cockpit.CanControlShip) mainCockpit = cockpit;
            LCDMain = GridTerminalSystem.GetBlockWithName("LCDMain") as IMyTextSurface;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & (UpdateType.Update1)) != 0) currentSplitSecond = (int)(1 / 0.016d);
            else if ((updateSource & (UpdateType.Update10)) != 0) currentSplitSecond = (int)(1 / 0.16d);

            if ((updateSource & (UpdateType.Terminal | UpdateType.Trigger)) != 0)
            {
                if (argument == "Fire")
                {
                    foreach (MyMissile missile in missiles)
                    {
                        if (missile.CurrentStatus == MyMissile.statusMissile.ReadyToStart)
                        {
                            missile.Launch();
                            break;
                        }
                        
                        //missile.MergeMissile.ApplyAction("OnOff_Off");
                        //foreach (IMyThrust truster in missile.GetMainThrusts) truster.ThrustOverridePercentage = 1;
                    }
                }
                else if (argument == "Lock")
                {
                    isTargetLocked = false;
                    if (cameras.Count != 0)
                    {
                        foreach (var camera in cameras)
                        {
                            if (camera.CanScan(5000))
                            {
                                MyDetectedEntityInfo entityInfo = camera.Raycast(5000);
                                if (!entityInfo.IsEmpty() && (entityInfo.Type == MyDetectedEntityType.LargeGrid || entityInfo.Type == MyDetectedEntityType.SmallGrid) 
                                    && entityInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies)
                                {
                                    lastEntityInfo = entityInfo;
                                    previousEntityInfo = entityInfo;
                                    isTargetLocked = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if ((updateSource & (UpdateType.Update1 | UpdateType.Update10)) != 0)
            {
                if (firstRun)
                {
                    // Инициализируем подвески после рекомпиляции
                    suspensionMerges.Clear();
                    GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(suspensionMerges, merge => (merge.CustomName.Contains("[suspension:") || merge.CustomData == "") 
                    && !merge.CustomName.ToLower().Contains("[t]"));
                    int index = 0;
                    suspensionMerges.Sort((n, m) => n.Position.CompareTo(m.Position));
                    foreach (IMyShipMergeBlock merge in suspensionMerges)
                    {
                        merge.CustomName = "[suspension:" + index + "]";
                        merge.CustomData = merge.Position.ToString();
                        index++;
                    }
                    // Удаляем ракеты из памяти
                    terminalBlocks.Clear();
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(terminalBlocks, block => block.CustomName.ToLower().Contains("[m]"));
                    foreach (IMyTerminalBlock block in terminalBlocks)
                    {
                        if (block.CustomName.ToLower().Contains("[t]"))
                        {
                            block.CustomName = "[M][T]";
                            block.CustomData = "[M][T]";
                        }
                        else
                        {
                            block.CustomName = "[M]";
                            block.CustomData = "[M]";
                        }
                    }
                    missiles.Clear();
                }

                // Удаляем мёртвую подвеску каждый тик
                foreach (IMyShipMergeBlock merge in suspensionMerges)
                {
                    if (merge == null)
                    {
                        suspensionMerges.Remove(merge);
                        suspensionMerges.Clear();
                        GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(suspensionMerges, m => m.CustomName.ToLower().Contains("[suspension:"));
                        suspensionMerges.Sort((n, m) => n.Position.CompareTo(m.Position));
                    }
                }

                // Операции 1 раз в секунду 
                if (tickCounter >= currentSplitSecond * 1)
                {
                    // Ищем новые блоки ракет
                    terminalBlocks.Clear();
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(terminalBlocks, block => block.CustomName.ToLower().Contains("[m]") && !block.CustomName.ToLower().Contains("[inited]"));
                    LCDMain.WriteText("1", false);
                    foreach (IMyTerminalBlock block in terminalBlocks)
                    {
                        
                        LCDMain.WriteText("2" + suspensionMerges.Count, false);
                        int minDistance = 10000;
                        int indexSusp = 0;
                        for (int i = 0; i < suspensionMerges.Count; i++)
                        {
                            LCDMain.WriteText("3", false);
                            if (block.Position.RectangularDistance(suspensionMerges[i].Position) < minDistance)
                            {
                                LCDMain.WriteText("4", false);
                                minDistance = block.Position.RectangularDistance(suspensionMerges[i].Position);
                                indexSusp = i;
                            }
                            
                        }
                        if (block.IsSameConstructAs(suspensionMerges[indexSusp]))
                        {
                            bool isMissileInited = false;
                            foreach (var missile in missiles)
                                if (missile.MergeSusp == suspensionMerges[indexSusp])
                                    if ((int)missile.CurrentStatus < 2)
                                        isMissileInited = true;

                            if (!isMissileInited)
                            {
                                MyMissile missile = new MyMissile(indexSusp, suspensionMerges[indexSusp]);
                                missiles.Add(missile);
                            }

                            foreach (MyMissile missile in missiles)
                            {
                                LCDMain.WriteText("5", false);
                                if (missile.MergeSusp == suspensionMerges[indexSusp])
                                {
                                    LCDMain.WriteText("6", false);
                                    if ((int)missile.CurrentStatus < 2)
                                    {
                                        LCDMain.WriteText("7" + missiles.Count, false);
                                        isMissileInited = true;
                                        missile.AddPart(block);
                                    }
                                    
                                }
                            }

                            
                        }
                    }
                    LCDMain.WriteText("8", false);
                    // Проверяем ракеты на целостность (Меняем режим готовности)
                    for (int i = missiles.Count-1; i >= 0; i--)
                    {
                        missiles[i].RefreshStatus();
                        if (missiles[i].CurrentStatus == MyMissile.statusMissile.Losed)
                            missiles.Remove(missiles[i]);
                    }

                    LCDMain.WriteText("9", false);

                    tickCounter = 0;
                }
                else tickCounter++;

                LCDMain.WriteText("10", false);

                IMyLargeGatlingTurret myTurret = GridTerminalSystem.GetBlockWithName("BoardPro2") as IMyLargeGatlingTurret;
                if (myTurret.HasTarget)
                {
                    LCDMain.WriteText("10-1", false);
                    for (int i = missiles.Count - 1; i >= 0; i--)
                    {
                        LCDMain.WriteText("10-2", false);
                        //missiles[i].RefreshStatus();
                        //if (missiles[i].currentStatus == statusMissile.Losed)
                        //    missiles.Remove(missiles[i]);
                        
                        if (missiles[i].CurrentStatus == MyMissile.statusMissile.Launched)
                        {
                            LCDMain.WriteText("10-3", false);
                            
                            missiles[i].CorrectPath(myTurret.GetTargetedEntity().HitPosition.GetValueOrDefault(myTurret.GetTargetedEntity().Position), myTurret.GetTargetedEntity().Velocity);
                        }
                            
                    }
                    //foreach (MyMissile missile in missiles)
                    {
                        
                    }
                    
                }
                LCDMain.WriteText("11", false);
                            
                if (firstRun)
                {
                    cameras.Clear();
                    GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(cameras, camera => camera.CustomName.ToLower().Contains("[c]"));
                    foreach (var camera in cameras)
                        camera.EnableRaycast = true;                    
                }

                string tmpStr = "";
                tmpStr += cameras.Count + "\n";

                if (isTargetLocked && (cameras.Count != 0) && (tickCounterRayCast >= tickDelayRayCast))
                {
                    Vector3D shotCameraPosition = lastEntityInfo.Position + (lastEntityInfo.Velocity / 60 * tickCounterRayCast)
                            + ((lastEntityInfo.Velocity - previousEntityInfo.Velocity) / 60 * tickCounterRayCast);

                    shotCameraPosition = (Vector3D.Normalize(shotCameraPosition - cameras[cameraCounter].GetPosition())
                        * ((shotCameraPosition - cameras[cameraCounter].GetPosition()).Length() + 70)) + cameras[cameraCounter].GetPosition();

                    if (cameras[cameraCounter].CanScan(shotCameraPosition))
                    {
                        previousEntityInfo = lastEntityInfo;//(lastEntityInfo.Velocity / 60 * tickCounterRayCast);
                        lastEntityInfo = cameras[cameraCounter].Raycast(shotCameraPosition);

                        if (lastEntityInfo.IsEmpty())
                            cameraMisshotCounter++;

                        tickDelayRayCast = (int)(shotCameraPosition - cameras[cameraCounter].GetPosition()).Length() / 2 / cameras.Count / 16;
                    }
                    else
                    {
                        cameraMisshotCounter++;
                    }

                    if (cameraMisshotCounter >= 10)
                    {
                        cameraMisshotCounter = 0;
                        isTargetLocked = false;
                    }

                    cameraCounter++;
                    if (cameraCounter >= cameras.Count) cameraCounter = 0;

                    tickCounterRayCast = 0;
                }
                else tickCounterRayCast++;

                if (isTargetLocked)
                {
                    foreach (var missile in missiles)
                    {
                        missile.CorrectPath(lastEntityInfo.Position + (lastEntityInfo.Velocity / 60 * tickDelayRayCast)
                                + ((lastEntityInfo.Velocity - previousEntityInfo.Velocity) / 60 * tickDelayRayCast)
                                , lastEntityInfo.Velocity/*, mainCockpit.GetNaturalGravity()*/);
                    }
                    tmpStr += "Цель захвачена!\n";
                }
                else
                {
                    foreach (var missile in missiles)
                    {
                        missile.CorrectPath();
                    }
                    tmpStr += "Нет цели!\n";
                }
                

                tmpStr += tickDelayRayCast + " - " + tickCounterRayCast + " - " + (lastEntityInfo.Position - cameras[0].GetPosition()).Length() + " - " + "CantRayCast\n";
                tmpStr += lastEntityInfo.Velocity.Length() + " : " + lastEntityInfo.Name + "\n";

                //if (firstRun) myCamera.Raycast(1000d);

                //IMyCockpit FighterCockpit = GridTerminalSystem.GetBlockWithName("FighterCockpit") as IMyCockpit;
                //IMyTerminalBlock BoardPro1 = GridTerminalSystem.GetBlockWithName("BoardPro1") as IMyTerminalBlock;

                //TrueSpaceEngineers.Game.Entities.Blocks.MySearchlight
                //TrueSandbox.Game.Entities.MyCockpit

                // Блок вывода на сервисный экран
                Echo(tickCounter.ToString());
                //mySuspensionMerges.Clear();
                //GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(mySuspensionMerges, merge => merge.CustomName.ToLower().Contains("[suspension:"));
                //mySuspensionMerges.Sort((n, m) => n.Position.CompareTo(m.Position));
                
                int indx = 0;
                LCDMain.WriteText("12", false);
                foreach (MyMissile missile in missiles)
                {
                    LCDMain.WriteText("12.1 - " + indx, false);
                    if (missile.GetGyros.Count > 0)
                    tmpStr += "Ракета " + indx + " : " + missile.CurrentStatus + " : " + missile.GetBatteries.Count + " : " + missile.CurrentVelocity.Length() + "\n" + 
                        " : " + missile.GetGyros[0].Yaw +
                        " : " + missile.GetGyros[0].Pitch +
                        " : " + missile.GetGyros[0].Roll + "\n";
                    LCDMain.WriteText("12.2", false);
                    indx++;
                }
                LCDMain.WriteText("13", false);
                //tmpStr += myTurret.GetType();
                if (myTurret.HasTarget)
                {
                    tmpStr += myTurret.GetTargetedEntity().EntityId + " : " + myTurret.GetTargetedEntity().Position + " : " + myTurret.GetTargetedEntity().Velocity + "\n";
                }
                LCDMain.WriteText("14", false);
                //tmpStr += myCamera.EnableRaycast.ToString() + " : " + myCamera.RaycastConeLimit + " : " + myCamera.RaycastDistanceLimit + " : " + myCamera.RaycastTimeMultiplier + "\n";
                //tmpStr += myCamera.Raycast(5000d).EntityId + " : " + myCamera.Raycast(5000d).Position + "\n";
                //tmpStr += myCamera.TimeUntilScan(5000d) + " : " + myCamera.CanScan(5000d);
                //tmpStr += FighterCockpit.GetType();
                LCDMain.WriteText(tmpStr, false);
            }
            firstRun = false;
        }
        
    }
}
