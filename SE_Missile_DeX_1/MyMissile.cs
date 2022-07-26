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
    partial class Program
    {
        public class MyMissile
        {
            public enum statusMissile
            {
                NotReady = 0,
                ReadyToStart = 1,
                Launched = 2,
                Broken = 3,
                Losed = 4
            }
            private bool isJavPointReached = true;
            private TimeSpan safetyLaunchTime = TimeSpan.FromSeconds(3);
            private DateTime launchTimeStamp;
            private statusMissile currentStatus;
            private Vector3D previousPosition;
            private Vector3D currentVelocity;
            private Vector3D holdPosition;
            private int indexSuspOnGrid;
            private IMyBeacon beacon;
            private IMyRemoteControl remoteControl;
            private IMyShipMergeBlock mergeMissile;
            private List<IMyShipMergeBlock> triggeredMerges = new List<IMyShipMergeBlock>();
            private List<IMySmallGatlingGun> triggeredGuns = new List<IMySmallGatlingGun>();
            private List<IMyWarhead> warheads = new List<IMyWarhead>();
            private List<IMyThrust> mainThrusts = new List<IMyThrust>();
            private List<IMyGyro> gyros = new List<IMyGyro>();
            private List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            private List<IMyReactor> reactors = new List<IMyReactor>();
            private List<IMyGasTank> gastanks = new List<IMyGasTank>();
            private List<IMyCameraBlock> cameras = new List<IMyCameraBlock>();
            public Vector3D CurrentVelocity { get { return currentVelocity; } }
            public statusMissile CurrentStatus { get { return currentStatus; } }
            public IMyShipMergeBlock MergeSusp { get; }
            public IMyRemoteControl RemoteControl { get { return remoteControl; } }
            public IMyBeacon Beacon { get { return beacon; } }
            public IMyShipMergeBlock MergeMissile { get { return mergeMissile; } }
            public List<IMyWarhead> GetWarheads { get { return warheads; } }
            public List<IMyThrust> GetMainThrusts { get { return mainThrusts; } }
            public List<IMyGyro> GetGyros { get { return gyros; } }
            public List<IMyBatteryBlock> GetBatteries { get { return batteries; } }
            public List<IMyReactor> GetReactors { get { return reactors; } }
            public List<IMyGasTank> GetGastanks { get { return gastanks; } }
            public List<IMyCameraBlock> GetCameras { get { return cameras; } }

            public MyMissile(int _indexSuspOnGrid, IMyShipMergeBlock _mergeSusp)
            {
                indexSuspOnGrid = _indexSuspOnGrid;
                MergeSusp = _mergeSusp;
                currentStatus = statusMissile.NotReady;

            }

            public void AddPart(IMyTerminalBlock NewPart)
            {
                if (!NewPart.CustomName.ToLower().Contains("[inited]"))
                {
                    string nameOfType;
                    if (!NewPart.CustomName.ToLower().Contains("[t]"))
                    {
                        if (NewPart is IMyBeacon) { beacon = NewPart as IMyBeacon; nameOfType = "Beacon"; }
                        else if (NewPart is IMyRemoteControl) { remoteControl = NewPart as IMyRemoteControl; nameOfType = "Control"; }
                        else if (NewPart is IMyShipMergeBlock) { mergeMissile = NewPart as IMyShipMergeBlock; nameOfType = "Merge"; }
                        else if (NewPart is IMyWarhead) { warheads.Add(NewPart as IMyWarhead); nameOfType = "Warhead"; }
                        else if (NewPart is IMyThrust) { mainThrusts.Add(NewPart as IMyThrust); nameOfType = "Thrust"; }
                        else if (NewPart is IMyGyro) { gyros.Add(NewPart as IMyGyro); nameOfType = "Gyro"; }
                        else if (NewPart is IMyBatteryBlock) { batteries.Add(NewPart as IMyBatteryBlock); nameOfType = "Battery"; }
                        else if (NewPart is IMyReactor) { reactors.Add(NewPart as IMyReactor); nameOfType = "Reactor"; }
                        else if (NewPart is IMyGasTank) { gastanks.Add(NewPart as IMyGasTank); nameOfType = "GasTank"; }
                        else if (NewPart is IMyCameraBlock) { cameras.Add(NewPart as IMyCameraBlock); nameOfType = "Camera"; }
                        else { nameOfType = "Not_used"; }
                        NewPart.CustomName = "[M]:" + indexSuspOnGrid + " [Inited] - " + nameOfType;
                        NewPart.CustomData = "[M]\n" + "[Inited] - " + nameOfType + "\nПодвеска: " + indexSuspOnGrid;
                    }
                    else
                    {
                        if (NewPart is IMyShipMergeBlock) { triggeredMerges.Add(NewPart as IMyShipMergeBlock); nameOfType = "TriggeredMerge"; }
                        else if (NewPart is IMySmallGatlingGun) { triggeredGuns.Add(NewPart as IMySmallGatlingGun); nameOfType = "TriggeredGun"; }
                        else { nameOfType = "Not_used"; }
                        NewPart.CustomName = "[M][T]:" + indexSuspOnGrid + " [Inited] - " + nameOfType;
                        NewPart.CustomData = "[M][T]\n" + "[Inited] - " + nameOfType + "\nПодвеска: " + indexSuspOnGrid;
                    }
                }
            }

            private void RefreshList<T>(List<T> list)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] != null)
                    {
                        IMyTerminalBlock l = list[i] as IMyTerminalBlock;
                        if (l.Closed) list.Remove(list[i]);
                    }
                    else list.Remove(list[i]);
                }
            }
            
            public void RefreshStatus()
            {
                switch (currentStatus)
                {
                    case (statusMissile.NotReady):
                        if (beacon != null && mergeMissile != null)
                        {
                            if ((!beacon.Closed) && (!mergeMissile.Closed) && (warheads.Count != 0) && (gyros.Count != 0) && (mainThrusts.Count != 0))
                            {
                                currentStatus = statusMissile.ReadyToStart;
                            }
                        }
                        break;
                    case (statusMissile.ReadyToStart):
                        if (beacon != null && mergeMissile != null)
                        {
                            RefreshList<IMyWarhead>(warheads);
                            RefreshList<IMyGyro>(gyros);
                            RefreshList<IMyThrust>(mainThrusts);
                            if ((!beacon.Closed) && (!mergeMissile.Closed) && (warheads.Count != 0) && (gyros.Count != 0) && (mainThrusts.Count != 0))
                            {
                            }
                            else currentStatus = statusMissile.NotReady;
                        }
                        //else currentStatus = statusMissile.NotReady;
                        break;
                    case (statusMissile.Launched):
                        if (beacon != null && mergeMissile != null)
                        {
                            RefreshList<IMyWarhead>(warheads);
                            RefreshList<IMyGyro>(gyros);
                            RefreshList<IMyThrust>(mainThrusts);
                            if ((!beacon.Closed) && (!mergeMissile.Closed) && (warheads.Count != 0) && (gyros.Count != 0) && (mainThrusts.Count != 0))
                            {
                            }
                            else currentStatus = statusMissile.Broken;
                        }
                        else currentStatus = statusMissile.Broken;
                        break;
                    case (statusMissile.Broken):
                        currentStatus = statusMissile.Losed;
                        break;
                    case (statusMissile.Losed):
                        break;
                    default:
                        break;
                }
            }

            public void Launch()
            {
                if (currentStatus == statusMissile.ReadyToStart)
                {
                    mergeMissile.ApplyAction("OnOff_Off");
                    foreach (IMyThrust truster in mainThrusts) truster.ThrustOverridePercentage = 1F;
                    currentStatus = statusMissile.Launched;
                    launchTimeStamp = DateTime.Now;
                }
            }

            public MyDetectedEntityInfo lockTargetCamera(MyDetectedEntityInfo entityInfo)
            {
                MyDetectedEntityInfo newEntityInfo = cameras[0].Raycast(entityInfo.Position);
                return newEntityInfo;
            }

            public void CorrectPath(Vector3D? _targetPosition = null, Vector3D? _targetVelocity = null, Vector3D? _gravDirection = null)
            {
                if ((int)currentStatus == 2 && safetyLaunchTime < DateTime.Now - launchTimeStamp)
                {
                    Vector3D targetPosition;

                    if (_targetPosition.HasValue)
                    {
                        targetPosition = _targetPosition.Value;
                        holdPosition = targetPosition;
                    }
                    else
                    {
                        if (holdPosition != Vector3D.Zero)
                            targetPosition = holdPosition;
                        else targetPosition = mainThrusts[0].GetPosition();
                    }

                    Vector3D targetVelocity = _targetVelocity.GetValueOrDefault(Vector3D.Zero);

                    if (_gravDirection.HasValue)
                    {
                        targetPosition = Vector3D.Normalize(-_gravDirection.Value) * 10 + targetPosition;
                    }

                    if (!isJavPointReached && _gravDirection.GetValueOrDefault(Vector3D.Zero) != Vector3D.Zero)
                    {
                        Vector3D gravDirection = _gravDirection.GetValueOrDefault(Vector3D.Zero);
                        Vector3D targetJavPosition = Vector3D.Normalize(-gravDirection) * 1200 + targetPosition;
                        targetPosition = targetJavPosition;

                    }

                    //Определяем вектор скорости ракеты.
                    if (previousPosition != null)
                    {
                        currentVelocity = (mainThrusts[0].GetPosition() - previousPosition) * Program.currentSplitSecond;
                    }
                    else currentVelocity = Vector3D.Zero;

                    //Определяем вектор направления к цели.
                    Vector3D pathToTarget = targetPosition - mainThrusts[0].GetPosition();

                    //Определяем вектор прицеливания в точку упреждения
                    {
                        double targetOrth = targetVelocity.Dot(Vector3D.Normalize(pathToTarget));
                        Vector3D targetTang = Vector3D.Reject(targetVelocity, Vector3D.Normalize(pathToTarget));
                        Vector3D missileTang = targetTang;
                        double missileTangLength = targetTang.Length();
                        if (missileTangLength < currentVelocity.Length())
                        {
                            double missileOrthLength = Math.Sqrt(currentVelocity.Length() * currentVelocity.Length() - missileTangLength * missileTangLength);
                            Vector3D missileOrth = Vector3D.Normalize(pathToTarget) * missileOrthLength;
                            pathToTarget = missileOrth + missileTang;
                        }
                    }

                    //Определяем вектор направления с учётом гашения инерции
                    Vector3D pathCorrected;
                    if (pathToTarget.Dot(currentVelocity) < 0)
                        pathCorrected = Vector3D.Normalize(pathToTarget);
                    else pathCorrected = Vector3D.Normalize(-Vector3D.Reflect(currentVelocity, Vector3D.Normalize(pathToTarget)));


                    Vector3D axisRotation = pathCorrected.Cross(mainThrusts[0].WorldMatrix.Backward);
                    if (pathCorrected.Dot(mainThrusts[0].WorldMatrix.Backward) < 0)
                    {
                        axisRotation = Vector3D.Normalize(axisRotation);
                    }

                    //Получаем длины проекций итогового вектора на оси ракеты 
                    //double FProjection = pathCorrected.Dot(remoteControl.WorldMatrix.Forward);
                    //double LProjection = pathCorrected.Dot(remoteControl.WorldMatrix.Left);
                    //double UProjection = pathCorrected.Dot(remoteControl.WorldMatrix.Down);
                    //Вычисляем сигнал для гироскопов

                    foreach (var gyro in gyros)
                    {

                        gyro.GyroOverride = true;
                        gyro.Yaw = (float)axisRotation.Dot(gyro.WorldMatrix.Up);// + (float)mainThrusts[0].WorldMatrix.Backward.Dot(gyro.WorldMatrix.Up) * (float)mainThrusts[0].WorldMatrix.Backward.Dot(gyro.WorldMatrix.Up);
                        gyro.Pitch = (float)axisRotation.Dot(gyro.WorldMatrix.Right);// + (float)mainThrusts[0].WorldMatrix.Backward.Dot(gyro.WorldMatrix.Right) * (float)mainThrusts[0].WorldMatrix.Backward.Dot(gyro.WorldMatrix.Right);
                        gyro.Roll = (float)axisRotation.Dot(gyro.WorldMatrix.Backward);// + (float)mainThrusts[0].WorldMatrix.Backward.Dot(gyro.WorldMatrix.Backward) * (float)mainThrusts[0].WorldMatrix.Backward.Dot(gyro.WorldMatrix.Backward);
                                                                                       //gyro.Yaw = (float)Math.Atan2(LProjection, FProjection);
                                                                                       //gyro.Pitch = -(float)Math.Atan2(UProjection, FProjection);
                                                                                       //gyro.Roll = 0f;
                    }

                    if (_targetPosition.HasValue)
                    {
                        if ((targetPosition - mainThrusts[0].GetPosition()).Length() < 450 && isJavPointReached)
                        {
                            foreach (var merge in triggeredMerges) merge.Enabled = false;
                            foreach (var gun in triggeredGuns) gun.Shoot = true;
                        }

                        if ((targetPosition - mainThrusts[0].GetPosition()).Length() < 100 && isJavPointReached)
                        {

                            foreach (IMyWarhead warhead in warheads)
                            {
                                if (warhead.StartCountdown())
                                {
                                    warhead.DetonationTime = (float)(100 / (currentVelocity - targetVelocity).Length());
                                    //currentStatus = statusMissile.Losed;
                                }
                            }
                        }
                    }

                    if ((targetPosition - mainThrusts[0].GetPosition()).Length() < 450)
                        isJavPointReached = true;

                    previousPosition = mainThrusts[0].GetPosition();

                        /*
                        //if (!remoteControl.Closed && remoteControl != null)
                        {
                            targetPos = targetPos - remoteControl.GetPosition();

                            tmp = Vector3D.Normalize(targetPos);
                            tmp = -Vector3D.Reflect(remoteControl.GetShipVelocities().LinearVelocity, tmp);
                            double FProj = tmp.Dot(remoteControl.WorldMatrix.Forward);
                            double LProj = tmp.Dot(remoteControl.WorldMatrix.Left);
                            double UProj = tmp.Dot(remoteControl.WorldMatrix.Down);

                            foreach (IMyGyro gyro in gyros)
                            {

                                /*
                                if (resultVector.Length() < 100)
                                {
                                    foreach (IMyWarhead warhead in warheads)
                                    {
                                        warhead.DetonationTime = 10f;
                                        warhead.StartCountdown();
                                    }
                                }*/
                        /*
                                float Yaw = (float)Math.Atan2(LProj, FProj);
                                float Pitch = -(float)Math.Atan2(UProj, FProj);
                                float Roll = 0f;

                                gyro.GyroOverride = true;
                                gyro.Yaw = Yaw;
                                gyro.Pitch = Pitch;

                                //gyro.Yaw = (float)resultVector.Dot(gyro.WorldMatrix.Up);
                                //gyro.Pitch = (float)resultVector.Dot(gyro.WorldMatrix.Right);
                                //gyro.Roll = (float)resultVector.Dot(gyro.WorldMatrix.Backward);
                                //oldPosition = remoteControl.GetPosition();
                            /*    
                            }

                            if (targetPos.Length() < 110)
                            {

                                foreach (IMyWarhead warhead in warheads)
                                {
                                    if (warhead.StartCountdown())
                                    {
                                        warhead.DetonationTime = (float)(100 / remoteControl.GetShipVelocities().LinearVelocity.Length());
                                        currentStatus = statusMissile.Losed;
                                    }

                                }
                            }
                        }*/


                }
            }

            
        }

        //Missile test = new Missile();

    }
}
