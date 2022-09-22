using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class MyArena
    {
        public IMySensorBlock PrepareZoneSensorA;
        public IMySensorBlock PrepareZoneSensorB;
        public IMySensorBlock ArenaMainSensor;
        public IMyDoor PrepareZoneDoorA;
        public IMyDoor PrepareZoneDoorB;
        public IMyDoor ArenaDoorA;
        public IMyDoor ArenaDoorB;
        public IMyLargeInteriorTurret PrepareZoneTurretA1;
        public IMyLargeInteriorTurret PrepareZoneTurretA2;
        public IMyLargeInteriorTurret PrepareZoneTurretB1;
        public IMyLargeInteriorTurret PrepareZoneTurretB2;
        public IMyLargeInteriorTurret ArenaTurret1;
        public IMyLargeInteriorTurret ArenaTurret2;
        public IMyLargeInteriorTurret ArenaTurret3;
        public IMyLargeInteriorTurret ArenaTurret4;
        public IMyTextSurface LCDIntro;

        List<MyDetectedEntityInfo> teamA = new List<MyDetectedEntityInfo>();
        List<MyDetectedEntityInfo> teamB = new List<MyDetectedEntityInfo>();

        public TimeSpan timerGamePrepareToOpenDoors = TimeSpan.FromSeconds(300);

        public StateGame currentState;

        public string lastMessage = "";

        public bool isArenaBeenUsed = false;

        public Program mainScript;

        public MyArena(Program script) : this(0, script) 
        {
            
        }

        public MyArena(int state, Program script)
        {
            currentState = (StateGame)state;

            mainScript = script;
        }                

        public void RefreshState()
        {
            // ===--- debug-block
            string tmpstr = "";
            foreach (var str in mainScript.storedValues)
                tmpstr += str + "\n";
            LCDIntro.WriteText(tmpstr);
            // ===--- debug-block

            switch (currentState)
            {
                case StateGame.NotReady:
                    {                       
                        currentState = StateGame.Ready;

                        mainScript.Save();
                    }
                    break;

                case StateGame.Ready:
                    {
                        mainScript.controlRoom.RefreshState();

                        if (mainScript.controlRoom.isGameCanCancel)
                        {
                            currentState = StateGame.Prepare;

                            mainScript.Save();
                        }
                    }
                    break;

                case StateGame.Prepare:
                    {
                        if (!isArenaBeenUsed && mainScript.controlRoom.isGameNeedCancel)
                        {
                            currentState = StateGame.NotReady;

                            mainScript.Save();
                        }
                        else
                        {
                            bool needResetSomeA = false;
                            bool needResetSomeB = false;
                            int countUsersAccessedA = teamA.Count;
                            int countUsersAccessedB = teamB.Count;
                            for (int i = 0; i < 3; i++)
                            {
                                if (mainScript.gateways[i].currentState == MyGateway.StateGateway.AccessAccepted)
                                    countUsersAccessedA++;

                                if (mainScript.gateways[i + 3].currentState == MyGateway.StateGateway.AccessAccepted)
                                    countUsersAccessedB++;
                            }

                            if (countUsersAccessedA >= mainScript.controlRoom.modeSelected) needResetSomeA = true;

                            if (countUsersAccessedB >= mainScript.controlRoom.modeSelected) needResetSomeB = true;

                            for (int i = 0; i < 3; i++)
                            {
                                if (needResetSomeA)
                                {
                                    if (mainScript.gateways[i].currentState == MyGateway.StateGateway.AccessAccepted)
                                    {
                                        if (countUsersAccessedA > mainScript.controlRoom.modeSelected)
                                        {
                                            mainScript.gateways[i].ResetGateway();

                                            countUsersAccessedA--;
                                        }       
                                    }
                                    else
                                    {
                                        mainScript.gateways[i].ResetGateway();
                                    }
                                }

                                mainScript.gateways[i].RefreshState();

                                if (needResetSomeB)
                                {
                                    if (mainScript.gateways[i + 3].currentState == MyGateway.StateGateway.AccessAccepted)
                                    {
                                        if (countUsersAccessedB > mainScript.controlRoom.modeSelected)
                                        {
                                            mainScript.gateways[i + 3].ResetGateway();

                                            countUsersAccessedB--;
                                        }
                                    }
                                    else
                                    {
                                        mainScript.gateways[i + 3].ResetGateway();
                                    }
                                }
                                
                                mainScript.gateways[i + 3].RefreshState();
                            }
                                                       
                            mainScript.controlRoom.RefreshState();

                            if ((teamA.Count >= mainScript.controlRoom.modeSelected) && (teamB.Count >= mainScript.controlRoom.modeSelected))
                            {
                                currentState = StateGame.Running;

                                mainScript.Save();
                            }                            
                        }
                    }
                    break;

                case StateGame.Running:
                    {                        
                        //currentState = StateGame.Prepare;

                        mainScript.Save();
                    }
                    break;
                default:
                    lastMessage = "ArenaErrorAE1";
                    break;
            }
        }

        public void AddUserToTeam(bool isTeamA)
        {
            if (!isArenaBeenUsed) isArenaBeenUsed = true;

            if (isTeamA)
            {
                teamA.Add(PrepareZoneSensorA.LastDetectedEntity);
            }
            else
            {
                teamB.Add(PrepareZoneSensorB.LastDetectedEntity);
            }
        }

        public enum StateGame
        {
            NotReady = 0,
            Ready = 1,
            Prepare = 2,
            Running = 3,
            Ending = 4
        }
    }    
}
