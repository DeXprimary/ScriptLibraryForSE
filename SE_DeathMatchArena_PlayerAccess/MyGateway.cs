using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace IngameScript
{
    public class MyGateway
    {
        public IMyDoor FirstDoor;
        public IMyDoor SecondDoor;
        public IMySensorBlock VolumeSensor;
        public IMySensorBlock FlySensor;
        public IMyButtonPanel Button;
        public IMyTextSurface LCDAction;

        public StateGateway currentState = StateGateway.NotReady;
        public bool isGatewayReseted = true;

        public TimeSpan timerPlayerCheckHydrogen = TimeSpan.FromSeconds(10);
        public DateTime? timeStampStartHydrogenChecking;
        public bool isHydrogenChecked = false;

        public string enteredCode = "";
        public int lengthOfCode = 5;
        public bool isTokenVerifed = false;

        public bool isNeedReset = false;

        public Program mainScript;

        public MyGateway(Program script)
        {
            mainScript = script;
        }

        public void RefreshState()
        {
            switch (currentState)
            {
                case StateGateway.NotReady:
                    {
                        if (FirstDoor.OpenRatio == 1 && SecondDoor.OpenRatio == 0 && !isNeedReset)
                        {
                            if (isGatewayReseted)
                            {
                                currentState = StateGateway.Ready;

                                isGatewayReseted = false;
                            }
                            else ResetGateway();
                        }
                        else
                        {
                            ResetGateway();
                        }
                    }
                    break;

                case StateGateway.Ready:
                    {
                        LCDAction.WriteText("СВОБОДНЫЙ ШЛЮЗ");

                        if (VolumeSensor.IsActive && mainScript.arena.currentState <= MyArena.StateGame.ClosingDoors)
                        {
                            if (CheckSoloUser())
                            {
                                if (FirstDoor.OpenRatio == 0)
                                {
                                    currentState = StateGateway.UserInside;
                                }
                                else FirstDoor.CloseDoor();
                            }
                            else currentState = StateGateway.Error;
                        }
                    }
                    break;

                case StateGateway.UserInside:
                    {
                        if (VolumeSensor.IsActive && mainScript.arena.currentState <= MyArena.StateGame.ClosingDoors)
                        {
                            if (timeStampStartHydrogenChecking.HasValue)
                            {
                                if (isHydrogenChecked)
                                {
                                    timeStampStartHydrogenChecking = null;
                                }
                                else RefreshHydrogenChecking();
                            }

                            if (enteredCode.Length >= lengthOfCode)
                            {
                                if ((enteredCode.GetHashCode() ^ 77) == int.Parse(mainScript.Me.CustomData))
                                {
                                    isTokenVerifed = true;
                                }
                                enteredCode = "";
                            }

                            if (isTokenVerifed && isHydrogenChecked)
                            {
                                currentState = StateGateway.AccessAccepted;
                            }

                            if (!isTokenVerifed)
                            {
                                string str = "ВВЕДИТЕ ТОКЕН: ";

                                foreach (var ch in enteredCode) str += "*";

                                LCDAction.WriteText(str);
                            }
                            else if (!FlySensor.IsActive)
                            {
                                LCDAction.WriteText("ИСПОЛЬЗ. ДЖЕТПАК");
                            }
                            else if (!isHydrogenChecked)
                            {
                                string str = "ОСТАЛОСЬ: ";

                                str += (int)((timerPlayerCheckHydrogen - (DateTime.Now - timeStampStartHydrogenChecking.Value)).TotalSeconds);

                                str += " СЕК.";

                                LCDAction.WriteText(str);
                            }
                            else
                            {
                                LCDAction.WriteText("ДОСТУП РАЗРЕШЁН");
                            }
                        }
                        else ResetGateway();
                    }
                    break;

                case StateGateway.AccessAccepted:
                    {
                        SecondDoor.OpenDoor();
                    }
                    break;

                default:
                    {
                        LCDAction.WriteText("ОШИБКА! E1");

                        ResetGateway();
                    }
                    break;
            }
                        
        }

        public void SensorVolume(bool On)
        {
            if (On)
            {
                if (currentState >= StateGateway.Ready && mainScript.arena.currentState <= MyArena.StateGame.ClosingDoors)
                {
                    FirstDoor.CloseDoor();
                }                    
            }
            else
            {
                ResetGateway();
            }
        }

        private void RefreshHydrogenChecking()
        {            
            if (timerPlayerCheckHydrogen < (DateTime.Now - timeStampStartHydrogenChecking.Value))
            {
                isHydrogenChecked = true;
            }
        }

        public void ResetGateway()
        {
            LCDAction.WriteText("ОЖИДАНИЕ...");

            enteredCode = "";

            isNeedReset = false;

            isTokenVerifed = false;

            isHydrogenChecked = false;

            currentState = StateGateway.NotReady;

            if (SecondDoor.OpenRatio != 0)
            {
                SecondDoor.CloseDoor();
            }
            else
            {
                FirstDoor.OpenDoor();

                if (!VolumeSensor.IsActive)
                {                    
                    isGatewayReseted = true;
                }                
            }                
        }

        private bool CheckSoloUser()
        {
            List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();

            VolumeSensor.DetectedEntities(entities);
                        
            if (entities.Count > 1) return false;

            else return true;
        }

        public enum StateGateway
        {
            Error = 0,
            NotReady = 1,
            Ready = 2,
            UserInside = 3,
            AccessAccepted = 4
        }
    }        
}
