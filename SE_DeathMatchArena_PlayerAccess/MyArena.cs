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
        public IMyTextSurface LCDMainIntro;
        public IMyTextSurface LCDArenaEntranceA1;
        public IMyTextSurface LCDArenaEntranceA2;
        public IMyTextSurface LCDArenaEntranceB1;
        public IMyTextSurface LCDArenaEntranceB2;
        public IMyTextSurface LCDArenaState1;
        public IMyTextSurface LCDArenaState2;
        public IMyTextSurface LCDArenaBoardA;
        public IMyTextSurface LCDArenaBoardB;
        

        public List<long> teamA = new List<long>();
        public List<long> teamB = new List<long>();

        public DateTime? timeStampPrepare;
        public TimeSpan timeSpanPrepare = TimeSpan.FromSeconds(60);
        public DateTime? timeStampRunning;
        public TimeSpan timeSpanRunning = TimeSpan.FromSeconds(30);
        public DateTime? timeStampClosePrepareZone;
        public TimeSpan timeSpanClosePrepareZone = TimeSpan.FromSeconds(30);
        public DateTime? timeStampBattle;
        public TimeSpan timeSpanBattle = TimeSpan.FromSeconds(70);
        public DateTime? timeStampShowWinner;
        public TimeSpan timeSpanShowWinner = TimeSpan.FromSeconds(30);

        public StateGame currentState;

        public string lastMessage = "";

        public bool isArenaBeenUsed = false;

        private string winner = "";
        
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
            string tmpstr = "Debug info:\n";
            foreach (var str in mainScript.Storage.Split(';'))
                tmpstr += str + "\n";
            tmpstr += (int)currentState + "\n";
            tmpstr += mainScript.controlRoom.modeSelected + "\n";
            tmpstr += mainScript.controlRoom.isGameCanCancel;
            tmpstr += mainScript.controlRoom.isGameNeedCancel;
            mainScript.LCDDebug1.WriteText(tmpstr);
            // ===--- debug-block

            switch (currentState)
            {
                case StateGame.NotReady:
                    {                        
                        LCDArenaBoardA.WriteText("");
                        LCDArenaBoardB.WriteText("");

                        currentState = StateGame.Ready;

                        mainScript.Save();
                    }
                    break;

                case StateGame.Ready:
                    {
                        mainScript.controlRoom.RefreshState();

                        if (timeStampShowWinner.HasValue && timeSpanShowWinner < (DateTime.Now - timeStampShowWinner.Value))
                        {
                            LCDArenaState1.WriteText("СТАТУС АРЕНЫ:\n\nВ ОЖИДАНИИ\nНОВЫХ СМЕЛЬЧАКОВ");
                            LCDArenaState2.WriteText("СТАТУС АРЕНЫ:\n\nВ ОЖИДАНИИ\nНОВЫХ СМЕЛЬЧАКОВ");
                            timeStampShowWinner = null;
                        }

                        if (mainScript.controlRoom.isGameCanCancel)
                        {
                            ArenaDoorA.CloseDoor();
                            ArenaDoorB.CloseDoor();
                            PrepareZoneDoorA.CloseDoor();
                            PrepareZoneDoorB.CloseDoor();
                            PrepareZoneTurretA1.ApplyAction("OnOff_Off");
                            PrepareZoneTurretA2.ApplyAction("OnOff_Off");
                            PrepareZoneTurretB1.ApplyAction("OnOff_Off");
                            PrepareZoneTurretB2.ApplyAction("OnOff_Off");

                            timeStampPrepare = DateTime.Now;

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
                            RefreshGateways();

                            if (((teamA.Count >= mainScript.controlRoom.modeSelected) && (teamB.Count >= mainScript.controlRoom.modeSelected)) 
                                    || (timeStampPrepare.HasValue && timeSpanPrepare < DateTime.Now - timeStampPrepare.Value))
                            {                               
                                timeStampRunning = DateTime.Now;

                                timeStampPrepare = null;

                                currentState = StateGame.Running;

                                mainScript.Save();
                            }                            
                        }

                        LCDArenaState1.WriteText("СТАТУС АРЕНЫ:\n\nСБОР УЧАСТНИКОВ\nНА МАТЧ " 
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString() 
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanPrepare - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));

                        LCDArenaState2.WriteText("СТАТУС АРЕНЫ:\n\nСБОР УЧАСТНИКОВ\nНА МАТЧ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanPrepare - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));

                        LCDArenaEntranceA2.WriteText("ИГРОКОВ - " + teamA.Count + "/" + mainScript.controlRoom.modeSelected.ToString() 
                            + "\nБОЙ ЧЕРЕЗ - " + Math.Truncate((timeSpanPrepare - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));

                        LCDArenaEntranceB2.WriteText("ИГРОКОВ - " + teamB.Count + "/" + mainScript.controlRoom.modeSelected.ToString()
                            + "\nБОЙ ЧЕРЕЗ - " + Math.Truncate((timeSpanPrepare - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));
                    }
                    break;

                case StateGame.Running:
                    {
                        RefreshGateways();

                        LCDArenaState1.WriteText("СТАТУС АРЕНЫ:\n\nПОДГОТОВКА\nК МАТЧУ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanRunning - (DateTime.Now - timeStampRunning.Value)).TotalSeconds));

                        LCDArenaState2.WriteText("СТАТУС АРЕНЫ:\n\nПОДГОТОВКА\nК МАТЧУ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanRunning - (DateTime.Now - timeStampRunning.Value)).TotalSeconds));

                        LCDArenaEntranceA2.WriteText("ПОДГОТОВКА К БОЮ\n" 
                            + Math.Truncate((timeSpanRunning - (DateTime.Now - timeStampRunning.Value)).TotalSeconds));

                        LCDArenaEntranceB2.WriteText("ПОДГОТОВКА К БОЮ\n"
                            + Math.Truncate((timeSpanRunning - (DateTime.Now - timeStampRunning.Value)).TotalSeconds));

                        if (timeStampRunning.HasValue && timeSpanRunning < DateTime.Now - timeStampRunning.Value)
                        {
                            PrepareZoneDoorA.OpenDoor();
                            PrepareZoneDoorB.OpenDoor();
                            ArenaDoorA.OpenDoor();
                            ArenaDoorB.OpenDoor();

                            LCDArenaEntranceA2.WriteText("");

                            LCDArenaEntranceB2.WriteText("");

                            timeStampRunning = null;

                            timeStampClosePrepareZone = DateTime.Now;

                            timeStampBattle = DateTime.Now;

                            currentState = StateGame.ClosingDoors;

                            mainScript.Save();
                        }
                    }
                    break;

                case StateGame.ClosingDoors:
                    {
                        RefreshGateways();

                        LCDArenaEntranceA1.WriteText("ВЫХОД НА АРЕНУ\nЗАКРОЕТСЯ ЧЕРЕЗ:\n"
                            + Math.Truncate((timeSpanClosePrepareZone - (DateTime.Now - timeStampClosePrepareZone.Value)).TotalSeconds));

                        LCDArenaEntranceB1.WriteText("ВЫХОД НА АРЕНУ\nЗАКРОЕТСЯ ЧЕРЕЗ:\n"
                            + Math.Truncate((timeSpanClosePrepareZone - (DateTime.Now - timeStampClosePrepareZone.Value)).TotalSeconds));

                        LCDArenaState1.WriteText("СТАТУС АРЕНЫ:\n\nПРОВОДИТСЯ\nМАТЧ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanBattle - (DateTime.Now - timeStampBattle.Value)).TotalSeconds));

                        LCDArenaState2.WriteText("СТАТУС АРЕНЫ:\n\nПРОВОДИТСЯ\nМАТЧ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanBattle - (DateTime.Now - timeStampBattle.Value)).TotalSeconds));

                        List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();
                        ArenaMainSensor.DetectedEntities(entities);
                        int sizeTeamA = 0;
                        int sizeTeamB = 0;
                        foreach (var entity in entities)
                        {
                            if (teamA.Contains(entity.EntityId)) sizeTeamA++;
                            if (teamB.Contains(entity.EntityId)) sizeTeamB++;
                        }

                        string strA = "КОМАНДА \"А\"\n" + sizeTeamA + "/" + mainScript.controlRoom.modeSelected
                            + "\nВРЕМЯ МАТЧА\n" + Math.Truncate((DateTime.Now - timeStampBattle.Value).TotalSeconds)
                            + "/" + Math.Truncate(timeSpanBattle.TotalSeconds);
                        string strB = "КОМАНДА \"B\"\n" + sizeTeamB + "/" + mainScript.controlRoom.modeSelected
                            + "\nВРЕМЯ МАТЧА\n" + Math.Truncate((DateTime.Now - timeStampBattle.Value).TotalSeconds)
                            + "/" + Math.Truncate(timeSpanBattle.TotalSeconds);

                        LCDArenaBoardA.WriteText(strA);
                        LCDArenaBoardB.WriteText(strB);

                        if (timeStampClosePrepareZone.HasValue && timeSpanClosePrepareZone < (DateTime.Now - timeStampClosePrepareZone.Value))
                        {
                            ArenaDoorA.CloseDoor();
                            ArenaDoorB.CloseDoor();
                            PrepareZoneTurretA1.ApplyAction("OnOff_On");
                            PrepareZoneTurretA2.ApplyAction("OnOff_On");
                            PrepareZoneTurretB1.ApplyAction("OnOff_On");
                            PrepareZoneTurretB2.ApplyAction("OnOff_On");

                            LCDArenaEntranceA1.WriteText("");

                            LCDArenaEntranceB1.WriteText("");

                            timeStampClosePrepareZone = null;

                            currentState = StateGame.Battling;

                            mainScript.Save();
                        }                                             
                    }
                    break;

                case StateGame.Battling:
                    {
                        LCDArenaState1.WriteText("СТАТУС АРЕНЫ:\n\nПРОВОДИТСЯ\nМАТЧ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanBattle - (DateTime.Now - timeStampBattle.Value)).TotalSeconds));

                        LCDArenaState2.WriteText("СТАТУС АРЕНЫ:\n\nПРОВОДИТСЯ\nМАТЧ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanBattle - (DateTime.Now - timeStampBattle.Value)).TotalSeconds));

                        List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();
                        ArenaMainSensor.DetectedEntities(entities);
                        int sizeTeamA = 0;
                        int sizeTeamB = 0;
                        foreach (var entity in entities)
                        {
                            if (teamA.Contains(entity.EntityId)) sizeTeamA++;
                            if (teamB.Contains(entity.EntityId)) sizeTeamB++;
                        }

                        string strA = "КОМАНДА \"А\"\n" + sizeTeamA + "/" + mainScript.controlRoom.modeSelected 
                            + "\nВРЕМЯ МАТЧА\n" + Math.Truncate((DateTime.Now - timeStampBattle.Value).TotalSeconds) 
                            + "/" + Math.Truncate(timeSpanBattle.TotalSeconds);
                        string strB = "КОМАНДА \"B\"\n" + sizeTeamB + "/" + mainScript.controlRoom.modeSelected
                            + "\nВРЕМЯ МАТЧА\n" + Math.Truncate((DateTime.Now - timeStampBattle.Value).TotalSeconds)
                            + "/" + Math.Truncate(timeSpanBattle.TotalSeconds);

                        if (/*!ArenaMainSensor.IsActive ||*/ (timeStampBattle.HasValue && timeSpanBattle < (DateTime.Now - timeStampBattle.Value)))
                        {
                            if (sizeTeamA == sizeTeamB)
                            {
                                strA = "НИЧЬЯ!!!\n\nДРУЖБА ПОБЕДИЛА!!!";
                            }
                            else if (sizeTeamA < sizeTeamB)
                            {
                                strA = "КОМАНДА \"B\"\n\nОДЕРЖАЛА ПОБЕДУ!!!";
                            }
                            else if (sizeTeamA > sizeTeamB)
                            {
                                strA = "КОМАНДА \"A\"\n\nОДЕРЖАЛА ПОБЕДУ!!!";
                            }
                            else
                            {

                            }

                            strB = strA;
                            winner = strA;

                            LCDArenaState1.WriteText(winner);
                            LCDArenaState2.WriteText(winner);

                            timeStampBattle = null;

                            timeStampShowWinner = DateTime.Now;

                            currentState = StateGame.Ending;

                            mainScript.Save();
                        }

                        LCDArenaBoardA.WriteText(strA);
                        LCDArenaBoardB.WriteText(strB);
                    }
                    break;

                case StateGame.Ending:
                    {
                        

                        teamA.Clear();

                        teamB.Clear();



                        /*
                        if (timeSpanClosePrepareZone < DateTime.Now - timeStampClosePrepareZone)
                        {
                            ArenaDoorA.CloseDoor();
                            ArenaDoorB.CloseDoor();
                            PrepareZoneTurretA1.ApplyAction("OnOff_On");
                            PrepareZoneTurretA2.ApplyAction("OnOff_On");
                            PrepareZoneTurretB1.ApplyAction("OnOff_On");
                            PrepareZoneTurretB2.ApplyAction("OnOff_On");
                        }
                        */
                    }
                    break;

                default:
                    lastMessage = "ArenaErrorAE1";
                    break;
            }
        }

        public void RefreshGateways()
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
        }

        public void AddUserToTeam(bool isTeamA)
        {
            if (!isArenaBeenUsed) isArenaBeenUsed = true;

            if (isTeamA)
            {
                teamA.Add(PrepareZoneSensorA.LastDetectedEntity.EntityId);
            }
            else
            {
                teamB.Add(PrepareZoneSensorB.LastDetectedEntity.EntityId);
            }
        }

        public enum StateGame
        {
            NotReady = 0,
            Ready = 1,
            Prepare = 2,
            Running = 3,
            ClosingDoors = 4,
            Battling = 5,
            Ending = 6
        }
    }    
}
