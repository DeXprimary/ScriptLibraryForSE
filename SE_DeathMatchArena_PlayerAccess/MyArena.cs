using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.ObjectBuilders;

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
        public IMyLargeInteriorTurret ArenaTurret5;
        public IMyLargeInteriorTurret ArenaTurret6;
        public IMyTextSurface LCDMainIntro;
        public IMyTextSurface LCDArenaEntranceA1;
        public IMyTextSurface LCDArenaEntranceA2;
        public IMyTextSurface LCDArenaEntranceB1;
        public IMyTextSurface LCDArenaEntranceB2;
        public IMyTextSurface LCDArenaState1;
        public IMyTextSurface LCDArenaState2;
        public IMyTextSurface LCDArenaBoardA;
        public IMyTextSurface LCDArenaBoardB;
        public IMyTextSurface LCDRespZoneA;
        public IMyTextSurface LCDRespZoneB;
        public IMyTerminalBlock DummyA;
        public IMyTerminalBlock DummyB;
        public IMyStoreBlock PrepareZoneStoreA;
        public IMyStoreBlock PrepareZoneStoreB;
        public IMyCargoContainer CargoMain;
        public IMyShipConnector ArenaConnectorA;
        public IMyShipConnector ArenaConnectorB;

        public List<long> teamA = new List<long>();
        public List<long> teamB = new List<long>();

        public DateTime? timeStampPrepare;
        public TimeSpan timeSpanPrepare = TimeSpan.FromSeconds(120);
        public TimeSpan timeSpanPrepareScaled;
        public DateTime? timeStampRunning;
        public TimeSpan timeSpanRunning = TimeSpan.FromSeconds(30);
        public DateTime? timeStampClosePrepareZone;
        public TimeSpan timeSpanClosePrepareZone = TimeSpan.FromSeconds(30);
        public DateTime? timeStampBattle;
        public TimeSpan timeSpanBattle = TimeSpan.FromSeconds(600);
        public DateTime? timeStampShowWinner;
        public TimeSpan timeSpanShowWinner = TimeSpan.FromSeconds(15);

        public StateGame currentState;

        public bool isArenaBeenUsed = false;

        private string winner = "";
        
        public Program mainScript;

        public MyArena(IMySensorBlock sensorA, IMySensorBlock sensorB, Program script) : this(0, sensorA, sensorB, script) 
        {
            
        }

        public MyArena(int state, IMySensorBlock sensorA, IMySensorBlock sensorB, Program script)
        {
            currentState = (StateGame)state;

            mainScript = script;

            PrepareZoneSensorA = sensorA;

            PrepareZoneSensorB = sensorB;

            var entities = new List<MyDetectedEntityInfo>();

            switch (currentState)
            {
                case StateGame.NotReady:
                    break;
                case StateGame.Ready:
                    break;
                case StateGame.Prepare:
                    timeStampPrepare = DateTime.Now;
                    timeSpanPrepareScaled = TimeSpan.FromSeconds((int)(timeSpanPrepare.TotalSeconds) * ((mainScript.controlRoom.modeSelected - 1) / 3 + 1));
                    entities.Clear();
                    PrepareZoneSensorA.DetectedEntities(entities);
                    foreach (var entity in entities) { teamA.Add(entity.EntityId); }
                    PrepareZoneSensorB.DetectedEntities(entities);
                    foreach (var entity in entities) { teamB.Add(entity.EntityId); }
                    break;
                case StateGame.Running:
                    timeStampRunning = DateTime.Now;
                    break;
                case StateGame.ClosingDoors:
                    break;
                case StateGame.Battling:
                    currentState = StateGame.Ending;
                    //timeStampClosePrepareZone = DateTime.Now;
                    //timeStampBattle = DateTime.Now;
                    break;
                case StateGame.Ending:
                    timeStampShowWinner = DateTime.Now;
                    break;
            }
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
                        isArenaBeenUsed = false;

                        mainScript.controlRoom.isGameCanCancel = false;

                        if (!timeStampShowWinner.HasValue || (timeSpanShowWinner < (DateTime.Now - timeStampShowWinner.Value)))
                        {
                            LCDArenaState1.WriteText("СТАТУС АРЕНЫ:\n\nВ ОЖИДАНИИ\nНОВЫХ СМЕЛЬЧАКОВ");
                            LCDArenaState2.WriteText("СТАТУС АРЕНЫ:\n\nВ ОЖИДАНИИ\nНОВЫХ СМЕЛЬЧАКОВ");
                            LCDArenaBoardA.WriteText("");
                            LCDArenaBoardB.WriteText("");
                            LCDRespZoneA.WriteText("");
                            LCDRespZoneB.WriteText("");
                            timeStampShowWinner = null;
                            ArenaTurret1.ApplyAction("OnOff_On");
                            ArenaTurret2.ApplyAction("OnOff_On");
                            ArenaTurret3.ApplyAction("OnOff_On");
                            ArenaTurret4.ApplyAction("OnOff_On");
                            ArenaTurret5.ApplyAction("OnOff_On");
                            ArenaTurret6.ApplyAction("OnOff_On");

                            if (!ArenaMainSensor.IsActive)
                            {
                                DummyA.SetValueBool("Enable Restoration", true);
                                DummyB.SetValueBool("Enable Restoration", true);                                

                                currentState = StateGame.Ready;

                                mainScript.Save();
                            }
                        }                        
                    }
                    break;

                case StateGame.Ready:
                    {
                        mainScript.controlRoom.RefreshState();                        

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

                            timeSpanPrepareScaled = TimeSpan.FromSeconds((int)(timeSpanPrepare.TotalSeconds) * ((mainScript.controlRoom.modeSelected - 1) / 3 + 1)); 

                            ReloadStores();

                            teamA.Clear();

                            teamB.Clear();

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
                                    || (timeStampPrepare.HasValue && timeSpanPrepareScaled < DateTime.Now - timeStampPrepare.Value))
                            {                               
                                timeStampRunning = DateTime.Now;

                                timeStampPrepare = null;

                                currentState = StateGame.Running;

                                mainScript.Save();
                            }                            
                        }



                        LCDRespZoneA.WriteText("Состав команды: " + teamA.Count + "/" + mainScript.controlRoom.modeSelected.ToString());
                        LCDRespZoneB.WriteText("Состав команды: " + teamB.Count + "/" + mainScript.controlRoom.modeSelected.ToString());

                        LCDArenaState1.WriteText("СТАТУС АРЕНЫ:\n\nСБОР УЧАСТНИКОВ\nНА МАТЧ " 
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString() 
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanPrepareScaled - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));
                        LCDArenaState2.WriteText("СТАТУС АРЕНЫ:\n\nСБОР УЧАСТНИКОВ\nНА МАТЧ "
                            + mainScript.controlRoom.modeSelected.ToString() + "x" + mainScript.controlRoom.modeSelected.ToString()
                            + "\n" + "ОСТАЛОСЬ: " + Math.Truncate((timeSpanPrepareScaled - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));

                        LCDArenaEntranceA2.WriteText("ИГРОКОВ - " + teamA.Count + "/" + mainScript.controlRoom.modeSelected.ToString() 
                            + "\nБОЙ ЧЕРЕЗ - " + Math.Truncate((timeSpanPrepareScaled - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));
                        LCDArenaEntranceB2.WriteText("ИГРОКОВ - " + teamB.Count + "/" + mainScript.controlRoom.modeSelected.ToString()
                            + "\nБОЙ ЧЕРЕЗ - " + Math.Truncate((timeSpanPrepareScaled - (DateTime.Now - timeStampPrepare.Value)).TotalSeconds));
                    }
                    break;

                case StateGame.Running:
                    {
                        RefreshGateways();

                        LCDRespZoneA.WriteText("Подготовка к матчу: " + Math.Truncate((timeSpanRunning - (DateTime.Now - timeStampRunning.Value)).TotalSeconds));
                        LCDRespZoneB.WriteText("Подготовка к матчу: " + Math.Truncate((timeSpanRunning - (DateTime.Now - timeStampRunning.Value)).TotalSeconds));

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
                            foreach (var gateway in mainScript.gateways) gateway.ResetGateway();

                            PrepareZoneDoorA.OpenDoor();
                            PrepareZoneDoorB.OpenDoor();
                            ArenaDoorA.OpenDoor();
                            ArenaDoorB.OpenDoor();
                            ArenaTurret1.ApplyAction("OnOff_Off");
                            ArenaTurret2.ApplyAction("OnOff_Off");
                            ArenaTurret3.ApplyAction("OnOff_Off");
                            ArenaTurret4.ApplyAction("OnOff_Off");
                            ArenaTurret5.ApplyAction("OnOff_Off");
                            ArenaTurret6.ApplyAction("OnOff_Off");

                            DummyA.SetValueBool("Enable Restoration", false);
                            DummyB.SetValueBool("Enable Restoration", false);

                            LCDRespZoneA.WriteText("Идёт бой");
                            LCDRespZoneB.WriteText("Идёт бой");

                            LCDArenaEntranceA2.WriteText("");
                            LCDArenaEntranceB2.WriteText("");

                            timeStampRunning = null;

                            timeStampClosePrepareZone = DateTime.Now;

                            timeStampBattle = DateTime.Now;

                            currentState = StateGame.Battling;

                            mainScript.Save();
                        }
                    }
                    break;

                case StateGame.ClosingDoors:
                    {                        
                                                       
                    }
                    break;

                case StateGame.Battling:
                    {
                        if (timeStampClosePrepareZone.HasValue)
                        {
                            if (timeSpanClosePrepareZone < (DateTime.Now - timeStampClosePrepareZone.Value))
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
                            }
                            else
                            {
                                RefreshGateways();

                                LCDArenaEntranceA1.WriteText("ВЫХОД НА АРЕНУ\nЗАКРОЕТСЯ ЧЕРЕЗ:\n"
                                    + Math.Truncate((timeSpanClosePrepareZone - (DateTime.Now - timeStampClosePrepareZone.Value)).TotalSeconds));
                                LCDArenaEntranceB1.WriteText("ВЫХОД НА АРЕНУ\nЗАКРОЕТСЯ ЧЕРЕЗ:\n"
                                    + Math.Truncate((timeSpanClosePrepareZone - (DateTime.Now - timeStampClosePrepareZone.Value)).TotalSeconds));
                            }
                        }

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

                        if (timeStampClosePrepareZone.HasValue)
                        {
                            sizeTeamA = teamA.Count;
                            sizeTeamB = teamB.Count;
                        }
                        else
                        {
                            foreach (var entity in entities)
                            {
                                if (teamA.Contains(entity.EntityId)) sizeTeamA++;
                                if (teamB.Contains(entity.EntityId)) sizeTeamB++;
                            }
                        }

                        string strA = "КОМАНДА \"А\"\n" + sizeTeamA + "/" + mainScript.controlRoom.modeSelected 
                            + "\nВРЕМЯ МАТЧА\n" + Math.Truncate((DateTime.Now - timeStampBattle.Value).TotalSeconds) 
                            + "/" + Math.Truncate(timeSpanBattle.TotalSeconds);
                        string strB = "КОМАНДА \"B\"\n" + sizeTeamB + "/" + mainScript.controlRoom.modeSelected
                            + "\nВРЕМЯ МАТЧА\n" + Math.Truncate((DateTime.Now - timeStampBattle.Value).TotalSeconds)
                            + "/" + Math.Truncate(timeSpanBattle.TotalSeconds);

                        string msgToResp = "";

                        if (!timeStampClosePrepareZone.HasValue)
                        {
                            if ((sizeTeamA == 0) || (sizeTeamB == 0)
                            || (timeStampBattle.HasValue && timeSpanBattle < (DateTime.Now - timeStampBattle.Value)))
                            {
                                if (sizeTeamA == sizeTeamB)
                                {
                                    strA = "НИЧЬЯ!!!\n\nДРУЖБА ПОБЕДИЛА!!!";
                                    msgToResp = "Ничья!";
                                }
                                else if (sizeTeamA < sizeTeamB)
                                {
                                    strA = "КОМАНДА \"B\"\n\nОДЕРЖАЛА ПОБЕДУ!!!";
                                    msgToResp = "Команда \"B\" победитель!";
                                }
                                else if (sizeTeamA > sizeTeamB)
                                {
                                    strA = "КОМАНДА \"A\"\n\nОДЕРЖАЛА ПОБЕДУ!!!";
                                    msgToResp = "Команда \"A\" победитель!";
                                }
                                else
                                {

                                }

                                strB = strA;
                                winner = strA;

                                LCDRespZoneA.WriteText(msgToResp);
                                LCDRespZoneB.WriteText(msgToResp);

                                LCDArenaState1.WriteText(winner);
                                LCDArenaState2.WriteText(winner);

                                timeStampBattle = null;

                                timeStampShowWinner = DateTime.Now;

                                currentState = StateGame.Ending;

                                mainScript.Save();
                            }
                        }

                        LCDArenaBoardA.WriteText(strA);
                        LCDArenaBoardB.WriteText(strB);
                    }
                    break;

                case StateGame.Ending:
                    {
                        currentState = StateGame.NotReady;

                        mainScript.Save();
                    }
                    break;

                default:
                    mainScript.Echo("ArenaErrorAE1");
                    break;
            }
        }

        public void ReloadStores()
        {
            List<MyStoreQueryItem> itemsA = new List<MyStoreQueryItem>();
            PrepareZoneStoreA.GetPlayerStoreItems(itemsA);
            foreach (var item in itemsA) PrepareZoneStoreA.CancelStoreItem(item.Id);

            List<MyStoreQueryItem> itemsB = new List<MyStoreQueryItem>();
            PrepareZoneStoreB.GetPlayerStoreItems(itemsB);
            foreach (var item in itemsB) PrepareZoneStoreB.CancelStoreItem(item.Id);

            long insertedId;            

            var AutomaticRifleItem = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AutomaticRifleItem"), mainScript.controlRoom.modeSelected, 3000);

            var AutomaticRifleGun_Mag_20rd = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_AmmoMagazine/AutomaticRifleGun_Mag_20rd"), mainScript.controlRoom.modeSelected * 10, 25000);

            var PreciseAutomaticRifleItem = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/PreciseAutomaticRifleItem"), mainScript.controlRoom.modeSelected, 50000);

            var PreciseAutomaticRifleGun_Mag_5rd = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_AmmoMagazine/PreciseAutomaticRifleGun_Mag_5rd"), mainScript.controlRoom.modeSelected * 10, 100000);

            var RapidFireAutomaticRifleItem = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/RapidFireAutomaticRifleItem"), mainScript.controlRoom.modeSelected, 100000);

            var RapidFireAutomaticRifleGun_Mag_50rd = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_AmmoMagazine/RapidFireAutomaticRifleGun_Mag_50rd"), mainScript.controlRoom.modeSelected * 10, 200000);

            var WelderItem = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/WelderItem"), mainScript.controlRoom.modeSelected, 26000);

            var AngleGrinderItem = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AngleGrinderItem"), mainScript.controlRoom.modeSelected, 26000);

            var HandDrillItem = new MyStoreItemDataSimple(
                MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/HandDrillItem"), mainScript.controlRoom.modeSelected, 26000);

            PrepareZoneStoreA.InsertOffer(AutomaticRifleItem, out insertedId);
            PrepareZoneStoreA.InsertOffer(AutomaticRifleGun_Mag_20rd, out insertedId);
            PrepareZoneStoreA.InsertOffer(PreciseAutomaticRifleItem, out insertedId);
            PrepareZoneStoreA.InsertOffer(PreciseAutomaticRifleGun_Mag_5rd, out insertedId);
            PrepareZoneStoreA.InsertOffer(RapidFireAutomaticRifleItem, out insertedId);
            PrepareZoneStoreA.InsertOffer(RapidFireAutomaticRifleGun_Mag_50rd, out insertedId);

            PrepareZoneStoreB.InsertOffer(AutomaticRifleItem, out insertedId);
            PrepareZoneStoreB.InsertOffer(AutomaticRifleGun_Mag_20rd, out insertedId);
            PrepareZoneStoreB.InsertOffer(PreciseAutomaticRifleItem, out insertedId);
            PrepareZoneStoreB.InsertOffer(PreciseAutomaticRifleGun_Mag_5rd, out insertedId);
            PrepareZoneStoreB.InsertOffer(RapidFireAutomaticRifleItem, out insertedId);
            PrepareZoneStoreB.InsertOffer(RapidFireAutomaticRifleGun_Mag_50rd, out insertedId);

            PrepareZoneStoreA.InsertOrder(WelderItem, out insertedId);
            PrepareZoneStoreA.InsertOrder(AngleGrinderItem, out insertedId);
            PrepareZoneStoreA.InsertOrder(HandDrillItem, out insertedId);

            PrepareZoneStoreB.InsertOrder(WelderItem, out insertedId);
            PrepareZoneStoreB.InsertOrder(AngleGrinderItem, out insertedId);
            PrepareZoneStoreB.InsertOrder(HandDrillItem, out insertedId);            
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

        public void GiveBonus(bool isSideA)
        {
            for (int i = 0; i < mainScript.controlRoom.modeSelected * 2; i++)
            {
                if (isSideA)
                {
                    var medkit = CargoMain.GetInventory(0).FindItem(MyItemType.Parse("MyObjectBuilder_ConsumableItem/Medkit"));
                    if (medkit.HasValue) CargoMain.GetInventory(0).TransferItemTo(ArenaConnectorB.GetInventory(0), 0, 100, false, 1);
                    //if (medkit.HasValue) CargoMain.GetInventory(0).TransferItemTo(ArenaConnectorB.GetInventory(0), medkit.Value, 1);
                }
                else
                {
                    var medkit = CargoMain.GetInventory(0).FindItem(MyItemType.Parse("MyObjectBuilder_ConsumableItem/Medkit"));
                    if (medkit.HasValue) CargoMain.GetInventory(0).TransferItemTo(ArenaConnectorA.GetInventory(0), 0, 100, false, 1);
                    //if (medkit.HasValue) CargoMain.GetInventory(0).TransferItemTo(ArenaConnectorA.GetInventory(0), medkit.Value, 1);
                }
            }
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
