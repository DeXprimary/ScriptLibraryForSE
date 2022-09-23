using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    public class MyControlRoom
    {
        public IMyDoor Door;
        public IMySensorBlock VolumeSensor;
        public IMyButtonPanel Button1;
        public IMyButtonPanel ButtonExit;
        public IMyTextSurface LCDControlRoom;

        public StateControlRoom currentState = StateControlRoom.NotReady;
        public bool isRoomReseted = true;

        public bool isModeSelected = false;
        public bool isModeConfirm = false;
        public int modeSelected = 0;
        public int currentPage = 0;
        public int maxCountPage = 3;

        public int lengthOfCode = 5;
        public string cancelToken = "";

        public bool isGameNeedCancel = false;
        public bool isGameCanCancel = false;

        public Program mainScript;

        public MyControlRoom(Program script)
        {
            mainScript = script;
        }

        public void RefreshState()
        {
            RefreshSurfaceButtons();

            switch (currentState)
            {
                case StateControlRoom.NotReady:
                    {
                        if (!isGameCanCancel)
                        {
                            if (Door.OpenRatio == 1 && mainScript.arena.currentState == MyArena.StateGame.Ready)
                            {
                                if (isRoomReseted)
                                {
                                    modeSelected = 0;

                                    currentState = StateControlRoom.Ready;

                                    isRoomReseted = false;

                                    isGameNeedCancel = false;
                                }
                                else ResetControlRoom();
                            }
                            else
                            {
                                ResetControlRoom();
                            }
                        }
                        else
                        {
                            string str = "Матч можно отменить\nтолько если никто\nне прошёл на арену.\n\nДля отмены введите токен:\n";

                            foreach (var ch in cancelToken) str += "*";

                            LCDControlRoom.WriteText(str);

                            if (cancelToken.Length >= lengthOfCode)
                            {
                                if ((cancelToken.GetHashCode() ^ 77) == int.Parse(mainScript.Me.CustomData))
                                {
                                    isGameNeedCancel = true;

                                    isGameCanCancel = false;
                                }
                                cancelToken = "";
                            }
                        }
                    }
                    break;

                case StateControlRoom.Ready:
                    {
                        if (VolumeSensor.IsActive)
                        {
                            if (CheckSoloUser())
                            {
                                if (Door.OpenRatio == 0)
                                {
                                    currentState = StateControlRoom.UserInside;
                                }
                                else Door.CloseDoor();
                            }
                            else currentState = StateControlRoom.Error;
                        }
                    }
                    break;

                case StateControlRoom.UserInside:
                    {
                        if (VolumeSensor.IsActive)
                        {
                            if (isModeSelected && isModeConfirm)
                            {
                                currentState = StateControlRoom.ModeSelected;

                                isGameCanCancel = true;
                            }

                            if (!isModeSelected)
                            {
                                string str = "(Cтраница " + (currentPage + 1).ToString() + ")\n\nПожалуйста,\nвыберите режим игры.\n\n";
                                str += (currentPage * 3 + 1).ToString() + "x" + (currentPage * 3 + 1).ToString() + "\n";
                                str += (currentPage * 3 + 2).ToString() + "x" + (currentPage * 3 + 2).ToString() + "\n";
                                str += (currentPage * 3 + 3).ToString() + "x" + (currentPage * 3 + 3).ToString() + "\n";

                                LCDControlRoom.WriteText(str);
                            }
                            else if (!isModeConfirm)
                            {
                                string str = "Выбран режим:\n" + modeSelected.ToString() + "x" + modeSelected.ToString() + "\n\nЧто-бы подтвердить\nнажми снова.";

                                LCDControlRoom.WriteText(str);
                            }
                            else
                            {
                                string token = GetRandomToken().ToString();

                                mainScript.Me.CustomData = (token.GetHashCode() ^ 77).ToString();

                                string str = "Сообщи этот токен\nтолько участникам матча:\n\n" + token;

                                LCDControlRoom.WriteText(str);                                
                            }
                        }
                        else ResetControlRoom();
                    }
                    break;

                case StateControlRoom.ModeSelected:
                    {
                        if (VolumeSensor.IsActive)
                        {
                            
                        }
                        else ResetControlRoom();
                    }
                    break;

                default:
                    {
                        LCDControlRoom.WriteText("ОШИБКА! CRE1");

                        if (!VolumeSensor.IsActive) currentState = StateControlRoom.NotReady;
                    }
                    break;
            }

        }

        public void RefreshSurfaceButtons()
        {
            if (isGameCanCancel)
            {
                if (currentState == StateControlRoom.NotReady)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        ((IMyTextSurfaceProvider)Button1).GetSurface(i).WriteText((i + 1).ToString());
                    }

                    ((IMyTextSurfaceProvider)Button1).GetSurface(3).WriteText("Стереть");
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ((IMyTextSurfaceProvider)Button1).GetSurface(i).WriteText("");
                    }
                }
            }
            else
            {
                if (currentState == StateControlRoom.NotReady)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ((IMyTextSurfaceProvider)Button1).GetSurface(i).WriteText("");
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        ((IMyTextSurfaceProvider)Button1).GetSurface(i).WriteText((currentPage * 3 + i + 1).ToString() + "x" + (currentPage * 3 + i + 1).ToString());
                    }

                    ((IMyTextSurfaceProvider)Button1).GetSurface(3).WriteText("Далее");
                }
            }
        }

        public void SensorVolume(bool On)
        {
            if (On)
            {
                if (currentState >= StateControlRoom.Ready)
                {
                    Door.CloseDoor();

                    //RefreshState();
                }
            }
            else
            {
                ResetControlRoom();
            }
        }

        private int GetRandomToken()
        {
            int result = 0;

            Random random = new Random();

            for (int i = 0; i < lengthOfCode; i++)
            {
                result += random.Next(1, 4) * (int)Math.Pow(10, i);
            }

            return result;
        }

        public void ChangePage()
        {
            isModeSelected = false;

            modeSelected = 0;

            if (currentPage >= maxCountPage - 1)
            {
                currentPage = 0;
            }
            else currentPage++;
        }

        public void SelectMode(int button)
        {
            int modeIndex = 3 * currentPage + button;

            if (!isModeConfirm)
            {
                if (!isModeSelected)
                {
                    modeSelected = modeIndex;
                    isModeSelected = true;
                }
                else if (modeSelected != modeIndex)
                {
                    modeSelected = modeIndex;
                }
                else isModeConfirm = true;
            }
        }

        public void ResetControlRoom()
        {
            LCDControlRoom.WriteText("ОЖИДАНИЕ...");

            cancelToken = "";

            isGameNeedCancel = false;

            isModeSelected = false;

            isModeConfirm = false;

            currentPage = 0;

            currentState = StateControlRoom.NotReady;

            Door.OpenDoor();

            if (!VolumeSensor.IsActive)
            {
                isRoomReseted = true;
            }
        }

        private bool CheckSoloUser()
        {
            List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();

            VolumeSensor.DetectedEntities(entities);

            if (entities.Count > 1) return false;

            else return true;
        }

        public enum StateControlRoom
        {
            Error = 0,
            NotReady = 1,
            Ready = 2,
            UserInside = 3,
            ModeSelected = 4
        }
    }
}
