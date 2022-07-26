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
        //------------НАЧАЛО------------//

        //----  [КОКПИТ]  [LCD = true]  [CAMERA]  [DESIGNATOR]  [SPEED]  [URAN]  [ROCKET]  [Avto_SV]  [Avto_BAM]

        //----  "Fire_ "  "BAM"  "TRASH"  "DESIGNATOR"  "SPEED"  "LOCK"  "HUNTER"  "Avto_Fire"  "Avto_SV"  "Avto_SV_Off"  "Avto_BAM_All"

        List<string> _TAG_List = new List<string> { "A", "B", "C", "D", "E" };                         //  <=  Классы торпед менять, добавлять или удалять ТУТ
        bool TAG_privat = false, SPEED = false, Avto_SV = false, Avto_Fire = false, Avto_BAM = false;  //  <=  Настройки опций по умолчанию
        public int t_Small = 2, t_Large = 3, t_mif = 0; //  <=  Кол-во торпед идущих на цель для малой и большой сетки
        double Str_Dist = 30;                            //  <=  Дистанция прямого полёта торпеды
        double CAM_Dist = 3000;                          //  <=  Дистанция поиска цели Raycast_камерой кокпита
        double FIRE_Dist = 4000;                         //  <=  Дистанция авто запуска торпеды(если t_mif = 0)
        double auto_target_capture = 600;                //  <=  Дистанция авто наведение свободной торпеды
        float Uran = 0.5f, Uran_min = 0.9f;              //  <=  Урана в реакторах торпед и мин. кол-во в %
        int int_LCD = 0;                                 //  <=  Номер дисплея в кокпите
        bool Avto_BAM_All = false, mif = true;           //  <=  Блокисовка авто-подрыва по аргументу, и ложные цели: денамичные(true)\статичнык(false)
        string pref_s = "я"; bool pref_b = false;        //  <=  Префикс название блоков торпед и разрешение из переименовать (true\false)
        public static List<string> TAG_List = new List<string>(); // Лист собранных Классов торпед.
        bool Start = true, AvtoLCD = false, WARHEADS_Detonation = false, TRASH = false, OnOff = true, b_SV = true, b_SP = false,
            CAMERA_LOCK = false, _Avto_Fire = false, _Avto_BAM = false, _Avto_SV = false, HUNTER = false;
        public int Time_Step = 0, Step = 100, m_Mif = 0, m_0 = 0, m_1 = 0, m_2 = 0, m_3 = 0, t_0 = 0; // Параметры задержки операций и торпедная обстановка 
        int String_ID, X, Y, Z, Xmax = -1000, Xmin = 1000, Ymax = -1000, Ymin = 1000, Zmax = -1000, Zmin = 1000; // Параметры (Setka_Torped)
        Vector3 String_IDpos;          // Параметры текстовой строки (сетки торпед)
        Vector3I IDpos;                // Параметры сетки торпед
        string Info_Data, Setka_Torped = ""; // cockpit_AvtoLCD, TAG_privat_str;
        List<IMyTerminalBlock> ROCKET_Block_List = new List<IMyTerminalBlock>();              // Список блоков для сборки торпед
        IMyCockpit cockpit; IMyTextSurface LCD; IMySoundBlock Alarm; IMyCameraBlock CAMERA;   // Блоки на сетке
        List<IMyTextSurface> LCD_info = new List<IMyTextSurface>();                           // Лист дисплеев, для визуализации скрипта.
        List<IMyLargeTurretBase> List_designator = new List<IMyLargeTurretBase>();            // Список целе-указателей
        List<MyDetectedEntityInfo> List_target_new = new List<MyDetectedEntityInfo>();        // Список целей
        List<IMyInventory> Cargo = new List<IMyInventory>();                                  // Список иточника урана для торпед
        List<IMyTerminalBlock> List_BAM = new List<IMyTerminalBlock>();                       // Список блоков самоуничтожения
        List<IMyTerminalBlock> SV_List = new List<IMyTerminalBlock>();
        MyDetectedEntityInfo new_Target;      // Новая цель
        MyDetectedEntityInfo Scanpos;         // Данное сканирование Raycast_камеры кокпита
        double Global_Timestep = 0.016;       // Что-то о глобальной симуляции
        double PNGain = 1;                    // Облегчает Столкновение С Целью LOS_Delta * 9.8 * (0.5 * Усиление)
        double min_height = 600, height = 0;  // Минимальная высота и текущая высота
        Vector3D mif_poz = new Vector3D();    // Точка отсчёта ложной цели
        int iaf = 0, ias = 0, iab = 0, isp = 0, inff = 0;
        string NFF = ""; //Af = "", AS = "AS", AB = "", SP = "", 
        List<IMyThrust> Speed_Thrust = new List<IMyThrust>();   // Список трастеров для круиз-контроля (В соих данных указать "[SPEED]")
        public List<Vector3D> mif_list = new List<Vector3D>     // Список ложных целей
        {
            new Vector3D(0, 0, 0), // НЕ ТРОГАТЬ!!! (для режима "HUNTER")
            new Vector3D(300, 300, 300), new Vector3D(300, -300, 300), new Vector3D(300, -300, -300), new Vector3D(300, 300, -300), new Vector3D(300, 300, 300),
            new Vector3D(-300, 300, 300), new Vector3D(-300, -300, 300), new Vector3D(-300, -300, -300), new Vector3D(-300, 300, -300), new Vector3D(-300, 300, 300)
            /*
            new Vector3D(0, 0, 0), // ТРОГАТЬ!!! сперва летит сюда
            new Vector3D(209424.36, 285453.43, -222077.86), new Vector3D(207424.13, 285663.01, -222624.77), new Vector3D(205541.84, 285533.28, -223017.72),
            new Vector3D(203927.04, 284531.08, -222610.05), new Vector3D(203474.02, 283461.05, -221813.48), new Vector3D(203969.24, 282800.83, -220873.04),
            new Vector3D(205143.06, 283099.33, -220418.91), new Vector3D(206545.31, 283490.98, -220066.13), new Vector3D(208065.98, 283891.73, -220135.59)
            */
        };
        List<Color> _Color = new List<Color>
        {
            new Color(255, 255, 255), // Белый.
            new Color(0, 255, 0),     // Зел.
            new Color(255, 255, 0),   // Жёл.
            new Color(0, 0, 255),     // Син.
            new Color(255, 0, 0),     // Красн.
            new Color(0, 0, 0)        // Чёрный.
        };
        IMyTextSurface LCD2;

        #region Классы (НАЧАЛО)

        class MERGES //---- Класс подвески торпед
        {
            public string TAG;
            public IMyShipMergeBlock MERGE0;
            public int torpedoStatus = 0;       // статус торпеды (готова, повреждена, запущена, уничтожена)
            public Vector3I TV3I_position;      // Место торпеды в сетке торпед
            public Vector3I V3I_ID;             // 
            public double torpedo_Length = 0;   // Растояние торпедной подвески от кокпита
            public int ID;                      // Уникальный номер подвески торпеды в текстовой сторе (сетки торпед)
            public MISSILE MIS = new MISSILE(); // Подвешенная торпеда
        }
        List<MERGES> MERGES_List = new List<MERGES>();

        class MISSILE_TAG //---- Класс сортировки торпед по классам
        {
            public string TAG;
            public List<MERGES> MERGES_ROCKET_List = new List<MERGES>();
        }
        List<MISSILE_TAG> MERGES_TAG_List = new List<MISSILE_TAG>();

        class MISSILE //---- Класс торпед
        {
            //----Блоки Ракеты
            public IMyCameraBlock CAMERA;
            public IMyShipMergeBlock MERGE1;
            public IMyGyro GYRO;
            public List<IMyThrust> THRUSTERS = new List<IMyThrust>();
            public IMyTerminalBlock POWER;
            public List<IMyGasTank> TANK = new List<IMyGasTank>();
            public List<IMyWarhead> WARHEADS = new List<IMyWarhead>();
            public bool TANK_bool = false;    // Наличие топливного бака
            // Параметры Ракеты (Статичные)
            public string TAG;                // Класс торпеды [TAG]
            public int torpedoStatus = 0;     // статус торпеды (готова, повреждена, запущена, уничтожена)
            public TARGET target_info;        // Назначенная цель
            public double Missil_Dist;        // Растояние до ближайшей цели
            public double MissileAccel = 10;  // Ракетный разгон
            public double MissileMass = 0;    // Ракетная масса
            public double MissileThrust = 0;  // Ракетная тяга
            public bool IsLargeGrid = false;  // Размер сетки
            public double FuseDist = 100;     // Расстояние снятия предохранителя боеголовки
            public double FuseDistance = 1;   // Расстояние подрыва боеголовки
            //public bool mif_target = false;   // Цель ложная или нет
            public int TARGET_MIF = 1;        // Номер ложной цели
            public bool HAS_DETACHED = false;
            public bool IS_CLEAR = false;     // Прямой или управляемый полёт торпеды
            public Vector3D TARGET_PREV_POS = new Vector3D();
            public Vector3D MIS_PREV_POS = new Vector3D();
            public double PREV_Yaw = 0;
            public double PREV_Pitch = 0;
        }
        List<MISSILE> MISSILES_List = new List<MISSILE>();

        class TARGET //---- Класс Целей
        {
            public MyDetectedEntityInfo target;
            public long ID_target;            // Уникальный номер ЦЕЛИ
            public double target_Dist;        // Растояние до ближайшей торпеды
            public int target_Status = 0;     // Статус цели (первая, важнейшая, SOS, ложная, без торпеды, потерена (0.5с))
            public int target_MISSILE = 0;    // количество торпед идущих на цель
            public int MAX_target_MISSILE = 2;// Max количество торпед идущих на цель
            public int target_Tame_On = 0;    // Последнее обновление информации о целе (в тиках)
            public int target_Tame_Off = 20; // Мах время до потери цели (в тиках)
            public bool HAS_DETACHED = false;
            public bool IS_CLEAR = false;     // Размер сетки
            public Vector3D TARGET_PREV_POS = new Vector3D();
        }
        List<TARGET> TARGET_List = new List<TARGET>();
        #endregion Классы (КОНЕЦ)

        Program()
        {
            if (_TAG_List.Count > 0) // Определение и сборка классов торпед
            {
                foreach (var b in _TAG_List)
                {
                    if (b.Substring(0, 1) != "[" & b.Substring(0, 1) != "]" & b.Substring(0, 1) != " ")
                    {
                        string a = "[" + b.Substring(0, 1) + "]"; TAG_List.Add(a);
                        MISSILE_TAG NEW_MISSILE_TAG_List = new MISSILE_TAG() { TAG = a, };
                        MERGES_TAG_List.Add(NEW_MISSILE_TAG_List);
                    }
                }
            }
            List<IMyCockpit> _cockpit = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(_cockpit, a => a.CustomData.Contains("[КОКПИТ]"));
            List<IMyTextPanel> _LCD_info = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(_LCD_info, a => a.CustomData.Contains("[LCD = true]"));
            foreach (var b in _LCD_info) { LCD_info.Add(b as IMyTextSurface); }
            //GridTerminalSystem.GetBlocksOfType<IMyTextSurface>(LCD_info, a => (a as IMyTerminalBlock).CustomData.Contains("[LCD = true]"));
            if (_cockpit.Count > 0)
            {
                cockpit = _cockpit[0];
                if (cockpit.CustomData.Contains("[LCD = true]")) { LCD = cockpit.GetSurface(int_LCD); LCD_info.Add(LCD); }
                List<IMyCameraBlock> List_CAM = new List<IMyCameraBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(List_CAM, a => a.CustomData.Contains("[CAMERA]") & a.IsSameConstructAs(cockpit));
                if (List_CAM.Count > 0) { CAMERA = List_CAM[0]; CAMERA.CustomData = "[CAMERA] " + ((cockpit.Position - CAMERA.Position) * new Vector3(-1, -1, 1)); CAMERA.EnableRaycast = true; }
            }
            if (LCD_info.Count > 0) { AvtoLCD = true; foreach (var b in LCD_info) { b.ContentType = ContentType.SCRIPT; b.Script = ""; b.ScriptBackgroundColor = new Color(0, 0, 0); } }
            GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(List_designator, a => a.CustomData.Contains("[DESIGNATOR]"));
            List<IMySoundBlock> _Alarm = new List<IMySoundBlock>(); // Собирает Звук/Сигнал Тревоги
            GridTerminalSystem.GetBlocksOfType<IMySoundBlock>(_Alarm, a => a.DetailedInfo != "NoUse" & a.IsSameConstructAs(Me));
            if (_Alarm.Count > 0) { Alarm = _Alarm[0]; Alarm.SelectedSound = "SoundBlockAlert2"; Alarm.LoopPeriod = 99999; Alarm.Play(); Alarm.Enabled = false; }
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Speed_Thrust, a => a.CustomData.Contains("[SPEED]"));
            List<IMyTerminalBlock> _Cargo = new List<IMyTerminalBlock>(); // Собирает лист источников урана
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_Cargo, a => a.CustomData.Contains("[URAN]") & a != Me);
            if (_Cargo.Count > 0) foreach (var b in _Cargo) Cargo.Add(b.GetInventory()); float fff = Uran * Uran_min; Uran_min = fff;
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(List_BAM, a => a.CustomData.Contains("[Avto_BAM]") & !(a is IMyProgrammableBlock));
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(SV_List, a => a.CustomData.Contains("[Avto_SV]") & !(a is IMyProgrammableBlock));
            if (List_BAM.Count == 0) Avto_BAM = false;
            if (cockpit == null) { SPEED = false; HUNTER = false; }
            LCD2 = Me.GetSurface(0);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        } //---- Program(КОНЕЦ) 

        void Main(string arg)
        {
            Time_Step++; m_0 = 0; m_1 = 0; t_0 = 0;
            foreach (var R in MERGES_List) if (R.torpedoStatus == 1) m_0++;                // Торпед готово к пуску
            foreach (var b in MISSILES_List) if (b.torpedoStatus == 3) m_1++;              // Торпед запущенно
            foreach (var b in TARGET_List) t_0 -= b.target_MISSILE - b.MAX_target_MISSILE; // Торпед необходимо запустить
            //cockpit.GetShipSpeed()
            if (cockpit != null)
            {
                cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out height);
                if (height < min_height) { mif_poz = cockpit.GetPosition() + (Vector3D.Normalize(-cockpit.GetNaturalGravity()) * (min_height - height)); }
                else { mif_poz = cockpit.GetPosition(); }
            }
            string ini = "Инициализацмя " + 100 / Step * Time_Step;
            if (OnOff) { NFF = "On"; inff = 1; } else { NFF = "Off"; inff = 4; }
            Echo("B.O.S.  v1.1" + ini + "%");
            Echo("Fire_" + NFF);
            Echo("Торпед готово к пуску: " + m_0 + "/" + MERGES_List.Count);
            Echo("Торпед запущенно: " + m_1 + "/" + t_0);
            Echo("Топред на охране: " + (m_1 - t_0) + "/" + t_mif);
            Echo("Кол-во Designator: " + List_designator.Count);
            Echo("Иерархия классов: " + TAG_privat);
            Echo("Режим SPEED: " + SPEED + " " + Speed_Thrust.Count);
            Echo("Режим Avto_Fire: " + Avto_Fire);
            Echo("Режим Avto_SV: " + Avto_SV + " " + SV_List.Count);
            Echo("Режим Avto_BAM: " + Avto_BAM + " " + List_BAM.Count);
            Echo("Avto_BAM_All: " + Avto_BAM_All);
            Echo("Топред для малой сетки: " + t_Small);
            Echo("Топред для большой сетки: " + t_Large);
            Echo("Дист прямого полёта торпед: " + Str_Dist);
            Echo("Кол-во урана в торпедах: " + Uran);
            Echo("Торпед запущенно всего: " + m_2);
            Echo("Торпед 100% попавших в цель: " + m_3);
            LCD2.WriteText("B.O.S.  v1.1  " + ini + "%" + "  Fire_" + NFF + "\nКол-во Designator: " + List_designator.Count + "\nРежим Avto_Fire: " + Avto_Fire + "\nРежим Avto_SV: " + Avto_SV
                 + "\nТорпед готово к пуску: " + m_0 + "/" + MERGES_List.Count + "\nТорпед запущенно: " + m_1 + "/" + t_0 + "\nТопред на охране: " + (m_1 - t_0) + "/" + t_mif
                 + "\nТорпед запущенно всего: " + m_2 + "\nТорпед 100% попавших в цель: " + m_3, false);

            if (!SPEED & Avto_Fire & TARGET_List.Count == 0 & m_1 == 0) { Step = 20; Runtime.UpdateFrequency = UpdateFrequency.Update10; }
            else { Step = 100; Runtime.UpdateFrequency = UpdateFrequency.Update1; }
            //Echo("1.0");
            #region ОПЦИИ (НАЧАЛО)
            if (arg == "On") OnOff = true;                  //---- Разрешить запуск торпед
            if (arg == "Off") OnOff = false;                //---- Запретить запуск торпед
            if (arg == "BAM") WARHEADS_Detonation = true;   //---- Уничтожить все запущенные торпеды
            if (arg == "Avto_Fire") Avto_Fire = !Avto_Fire; //---- Вкл\выкл Авто запуск торпед
            if (arg == "Avto_SV") Avto_SV = !Avto_SV;       //---- Вкл\выкл Режим Авто-сварка торпед
            if (arg == "Avto_SV_Off") _Avto_SV = false;     //---- Остановить сварку
            if (arg.Contains("Fire_") & Avto_SV) { _Avto_SV = true; b_SV = true; } //---- Запустить Сварку
            if (arg == "Avto_BAM" & List_BAM.Count > 0) Avto_BAM = !Avto_BAM;      //---- Вкл\выкл режим самоуничтожения
            if (arg == "TRASH") TRASH = true;               //---- Сбросить с подвески и уничтожить все неисправные торпеды
            if (arg == "HUNTER" & cockpit != null) HUNTER = !HUNTER; //---- Наведение торпед по курсору
            if (arg == "SPEED" & cockpit != null) SPEED = !SPEED;    //---- Вкл\выкл круиз-контроль
            if (arg == "DESIGNATOR") //---- Добавление новых цели-указателей
            {
                List<IMyLargeTurretBase> List_designator_new = new List<IMyLargeTurretBase>();
                GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(List_designator_new, a => a.CustomData.Contains("[DESIGNATOR]") & !List_designator.Contains(a));
                if (List_designator_new.Count > 0) foreach (var b in List_designator_new) List_designator.Add(b); // if (!List_designator.Contains(b)) List_designator.Add(b);
            }
            if (arg == "Avto_BAM_All" & Avto_BAM_All) //---- Подрыв пусковой установки
            {
                if (List_BAM.Count > 0)
                {
                    iab = 4;
                    foreach (var b in List_BAM)
                    {
                        if (b is IMyTimerBlock) { (b as IMyFunctionalBlock).Enabled = true; (b as IMyTimerBlock).Trigger(); }
                        if (b is IMyWarhead) { (b as IMyWarhead).IsArmed = true; (b as IMyWarhead).Detonate(); }
                    }
                }
            }
            if (SPEED) //---- АВТО-Контроль скорости
            {
                double H; isp = 1; float hh = -1;
                cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out H);
                if (H >= 300)
                {
                    if (cockpit.MoveIndicator.Z != 0)
                    {
                        foreach (var b in Speed_Thrust) { b.ThrustOverridePercentage += -cockpit.MoveIndicator.Z * 0.01f; hh += b.ThrustOverridePercentage; }
                        if (hh > 0) b_SP = true; else b_SP = false;
                    }
                    if (b_SP) isp = 3;
                }
                else { foreach (var b in Speed_Thrust) b.ThrustOverridePercentage = 0; isp = 2; }
            }
            else if (Speed_Thrust.Count > 0) { foreach (var b in Speed_Thrust) b.ThrustOverridePercentage = 0; SPEED = false; }
            #endregion ОПЦИИ (КОНЕЦ)
            //Echo("2.0");
            #region Инициализация подвески ТОРПЕД (НАЧАЛО)
            if (arg == "initialization" || Start || !Me.CustomData.Contains(Setka_Torped))
            {
                List<IMyTerminalBlock> Merge_List = new List<IMyTerminalBlock>(); //---- Списка блоков для подвески торпед
                bool sort = true;
                //---- Полной инициализации
                if (arg == "initialization" & TAG_List.Count > 0)
                {
                    GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(Merge_List, b => !b.CustomData.Contains("TAG = [false]") & !b.CustomData.Contains("[ROCKET]") & !b.CustomName.Contains("[ROCKET]"));
                    foreach (var Merge in Merge_List)
                    {
                        if (Merge.CustomName.Length < 3 || !TAG_List.Contains(Merge.CustomName.Substring(0, 3)))
                        {
                            Merge.CustomName = TAG_List[0] + " Соеденитель " + ((Me.Position - Merge.Position) * new Vector3(-1, -1, 1));
                        }
                        Merge.ApplyAction("OnOff_Off");
                    }
                }
                //---- Инициализация
                if (Start & TAG_List.Count > 0)
                {
                    GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(Merge_List, b => b.CustomName.Length >= 3 & TAG_List.Contains(b.CustomName.Substring(0, 3)));
                    //---- Очистка списков подвески (кроме списка TAG_List)
                    foreach (var List_ in MERGES_TAG_List) { List_.MERGES_ROCKET_List.Clear(); }
                    MERGES_List.Clear();
                    //---- Удаление торпед со статусом < 3
                    List<MISSILE> mysor1 = new List<MISSILE>();
                    foreach (var b in MISSILES_List) { if (b.torpedoStatus < 3) mysor1.Add(b); }
                    foreach (var b in mysor1) MISSILES_List.Remove(b);
                    //---- Редактирование списка подвески торпед
                    foreach (var Merge in Merge_List)
                    {
                        Merge.ApplyAction("OnOff_On");
                        MISSILE mis = new MISSILE();
                        MISSILES_List.Add(mis);
                        MERGES NEW_MERGES = new MERGES
                        {
                            MERGE0 = Merge as IMyShipMergeBlock,
                            V3I_ID = Merge.Position,
                            TAG = Merge.CustomName.Substring(0, 3),
                            MIS = mis
                        };
                        NEW_MERGES.MIS.TAG = Merge.CustomName.Substring(0, 3);
                        MERGES_List.Add(NEW_MERGES);
                        int i = TAG_List.IndexOf("[" + NEW_MERGES.MERGE0.CustomName.Substring(1, 1) + "]"); //---- Добавляет (MISSILES) в списки_ракет_TAG 
                        if (i < MERGES_TAG_List.Count) { MERGES_TAG_List[i].MERGES_ROCKET_List.Add(NEW_MERGES); }
                    }
                    foreach (var b in MERGES_List) //---- Создание сетки подвески торпед
                    {
                        Xmax = Math.Max(Xmax, Me.Position.X - b.MERGE0.Position.X);
                        Xmin = Math.Min(Xmin, Me.Position.X - b.MERGE0.Position.X);
                        X = Xmax - Xmin + 1;
                        Ymax = Math.Max(Ymax, Me.Position.Y - b.MERGE0.Position.Y);
                        Ymin = Math.Min(Ymin, Me.Position.Y - b.MERGE0.Position.Y);
                        Y = Ymax - Ymin + 1;
                        Zmax = Math.Max(Zmax, Me.Position.Z - b.MERGE0.Position.Z);
                        Zmin = Math.Min(Zmin, Me.Position.Z - b.MERGE0.Position.Z);
                        Z = Zmax - Zmin + 1;
                    }
                    if (X < Xmax - Me.Position.X + Me.Position.X) X = Xmax - Me.Position.X + Me.Position.X;
                    string X_string = String.Empty;
                    string Y_string = String.Empty;
                    string Z_string = String.Empty;
                    int Y_format = 0, Z_format = 0, Z_sort = 0;
                    for (int i = 0; i < X; i++)
                    {
                        X_string = (X_string + " ");
                        Y_string = X_string;
                        Setka_Torped = X_string;
                    }
                    List<int> Y_Position = new List<int>();
                    foreach (var b in MERGES_List)
                    {
                        if (!Y_Position.Contains(Me.Position.Y - b.MERGE0.Position.Y))
                        {
                            Y_Position.Add(Me.Position.Y - b.MERGE0.Position.Y);
                        }
                    }
                    Y_format = Y_Position.Count;
                    Y_Position.Sort((n, m) => n.CompareTo(m));
                    for (int i = 1; i < Y_format; i++)
                    {
                        Y_string = (Y_string + "\n\n" + X_string);
                        Z_string = Y_string;
                        Setka_Torped = Y_string;
                    }
                    Z_string = Y_string;
                    List<int> Z_Position = new List<int>();
                    foreach (var b in MERGES_List)
                    {
                        if (!Z_Position.Contains(Me.Position.Z - b.MERGE0.Position.Z))
                        {
                            Z_Position.Add(Me.Position.Z - b.MERGE0.Position.Z);
                        }
                    }
                    Z_format = Z_Position.Count;
                    Z_Position.Sort((n, m) => n.CompareTo(m));
                    if (Zmax - Zmin < 4)
                    {
                        List<Vector2> Z_Vector_Sort = new List<Vector2>();
                        foreach (var b in MERGES_List)
                        {
                            Vector2 Vector_Sort1 = new Vector2(Me.Position.X - b.MERGE0.Position.X, Me.Position.Y * -1 - b.MERGE0.Position.Y * -1);
                            if (Z_Vector_Sort.Contains(Vector_Sort1))
                            {
                                Z_sort = 1;
                                break;
                            }
                            Z_Vector_Sort.Add(Vector_Sort1);
                        }
                    }
                    else Z_sort = 1;
                    if (Z_sort == 1)
                    {
                        for (int i = 1; i < Z_format; i++)
                        {
                            Z_string = (Z_string + "\n\n" + Y_string);
                            Setka_Torped = Z_string;
                        }
                    }
                    foreach (var b in MERGES_List) //---- Заполнение данных торпед и размещение на сетке подвески торпед (NEW_MISSILE)
                    {
                        int Ax = (Xmax - Me.Position.X + Me.Position.X) - (Me.Position.X - b.MERGE0.Position.X);
                        int Ay = Y_Position.IndexOf(Me.Position.Y - b.MERGE0.Position.Y);
                        int Az = Z_Position.IndexOf(Me.Position.Z - b.MERGE0.Position.Z);
                        int Apos = Ax + (Ay * (X + 2)) + (Az * (X + 2) * Z_sort);
                        Setka_Torped = Setka_Torped.Remove(Apos, 1);
                        Setka_Torped = Setka_Torped.Insert(Apos, b.MERGE0.CustomName.Substring(1, 1));
                        b.ID = Apos;
                        b.MERGE0.CustomName = b.MERGE0.CustomName.Substring(0, 3) + " Соеденитель " + ((Me.Position - b.MERGE0.Position) * new Vector3(-1, -1, 1)).ToString();
                        b.TAG = b.MERGE0.CustomName.Substring(0, 3);
                        b.MERGE0.CustomData = "NEW_MISSILE.ID = " + Apos + "\nNEW_MISSILE.TAG = " + b.MERGE0.CustomName.Substring(0, 3);
                        b.TV3I_position = new Vector3I(Ax, Ay, Az * Z_sort);
                        b.torpedo_Length = (Me.GetPosition() - b.MERGE0.GetPosition()).Length();
                    }
                    foreach (var b in MERGES_TAG_List) b.MERGES_ROCKET_List.Sort((x, y) => (y.torpedo_Length.CompareTo(x.torpedo_Length))); //---- Сортирует подвеску по растоянию до ПБ
                    String_IDpos = new Vector3(X, Y_format, Z_format * Z_sort);
                    IDpos = new Vector3I(X, Y_format, Z_format);
                    Start = false;
                    sort = false;
                }
                if (arg == "initialization" & TAG_List.Count > 0) Start = true;
                if (sort & !Start & !Me.CustomData.Contains(Setka_Torped)) //---- Определение типа подвески (подвешиваемых торпед), по символам в своих данных
                {
                    foreach (var Merge in MERGES_List)
                    {
                        if (String_ID + Merge.ID <= Me.CustomData.Length)
                        {
                            string s2 = Me.CustomData.Substring(String_ID + Merge.ID, 1);
                            if (Merge.MERGE0.CustomName.Length >= 3 & Merge.MERGE0.CustomName.Substring(1, 1) != s2 & s2 != "#")
                            {
                                if (TAG_List.Contains("[" + s2 + "]"))
                                {
                                    Merge.MERGE0.CustomName = Merge.MERGE0.CustomName.Remove(1, 1);
                                    Merge.MERGE0.CustomName = Merge.MERGE0.CustomName.Insert(1, s2);
                                    Setka_Torped = Setka_Torped.Remove(Merge.ID, 1);
                                    Setka_Torped = Setka_Torped.Insert(Merge.ID, s2);
                                    Merge.TAG = Merge.MERGE0.CustomName.Substring(0, 3);
                                    Merge.MERGE0.CustomData = "NEW_MISSILE.ID = " + Merge.ID + "\nNEW_MISSILE.TAG = " + Merge.MERGE0.CustomName.Substring(0, 3);
                                    Start = true;
                                }
                                else
                                {
                                    Merge.MERGE0.CustomName = Merge.MERGE0.CustomName.Remove(0, 3);
                                    Merge.MERGE0.CustomName = Merge.MERGE0.CustomName.Insert(0, Merge.TAG);
                                }
                            }
                            else if (Merge.MERGE0.CustomName.Length >= 3 & TAG_List.Contains(Merge.MERGE0.CustomName.Substring(0, 3)) & s2 == "#")
                            {
                                Merge.MERGE0.CustomName = Merge.MERGE0.CustomName.Remove(1, 1);
                                Merge.MERGE0.CustomName = Merge.MERGE0.CustomName.Insert(1, "#");
                                Setka_Torped = Setka_Torped.Remove(Merge.ID, 1);
                                Setka_Torped = Setka_Torped.Insert(Merge.ID, " ");
                                Merge.MERGE0.CustomData = "TAG = [false]";
                                Start = true;
                            }
                        }
                        else break;
                    }
                }
                //---- Инструкция в Своих данных Програм.Блока [КОКПИТ]  [LCD = true]  [CAMERA]  [DESIGNATOR]  [SPEED]  [URAN]  [ROCKET]  [Avto_SV]  [Avto_BAM]
                Info_Data = "                     Торпеда [BOS v1.10]"
                    + "\n   Данные текущей инициализации:"
                    + "\n   Кол-во Подвесок торпед: " + MERGES_List.Count + "\n   Параметр сетки торпед = " + String_IDpos
                    + "\n   Инициализация типов ТОРПЕД:\n              ---//---\n" + Setka_Torped + "\n              ---//---"
                    + "\n   Поменять тип торпеды можно заменив её символ,"
                    + "\nна символ соответствующего типа,"
                    + "\nа также можно скрыть межБлок символом #\n или указав в 'Cвоих данных' блока  TAG = [false]"
                    + "\n        ---//---   Info    ---//---"
                    + "\n   Для заруск торпеды, аргумент: Fire_ с символом торпеды"
                    + "\n   Полная инициализация сетки: аргумент => initialization"
                    + "\n   Инициализация блоков с символом класса торпеды в [ ] скобках,\nпроходит при рекомпиляции или смене класса"
                    + "\n   Индефикаторы блоков вносить в 'Cвои данные' блока"
                    + "\n   Кокпита: [КОКПИТ] и [LCD = true] если требуется"
                    + "\n   Камера: [CAMERA]"
                    + "\n   Дисплей: [LCD = true]"
                    + "\n   Цели-указатели: [DESIGNATOR]"
                    + "\n   Блоки торпед: [ROCKET]"
                    + "\n   Блоки Автосварки: [Avto_SV]"
                    + "\n   Источник урана для торпед: [URAN]"
                    + "\n   Двигатели функции SPEED: [SPEED]"
                    + "\n   Блоки самоуничтожения: [Avto_BAM]"
                    + "\n   Аргумент: LOCK - Захват цели камерой"
                    + "\n   Аргумент: DESIGNATOR - Добавить цели-указатели"
                    + "\n   Аргумент: SPEED - вкл/выкл круиз-контроль"
                    + "\n   Аргумент: Avto_SV - вкл/выкл режим авто-сварки"
                    + "\n   Аргумент: Avto_Fire - вкл/выкл атопуск торпед"
                    + "\n   Аргумент: Avto_BAM - вкл/выкл режим самоуничтожения"
                    + "\n   Аргумент: Avto_BAM_All - Взорвать всё (Требуется Avto_BAM_All = true)"
                    + "\n   Аргумент: BAM - Уничтожить запущенные торпеды"
                    + "\n   Аргумент: TRASH - Сбросить и взорвать неисправные торпеды"
                    + "\n   Аргумент: HUNTER - Направлять несколько(" + t_Small + ") торпед за корсором"
                    + "\n"
                    + "\n"
                    + "\n";

                String_ID = Info_Data.IndexOf(Setka_Torped);
                Me.CustomData = Info_Data;
                Time_Step = 0;
            }
            #endregion Инициализация подвески ТОРПЕД (КОНЕЦ)
            //Echo("3.0");
            #region Сборка, проверка и запуск ТОРПЕД (НАЧАЛО)
            if (!Start & Time_Step > Step || arg.Contains("Fire_") & arg.Count() > 5)
            {
                if (Time_Step > Step)
                {
                    List<IMyTerminalBlock> _ROCKET_Block_List = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_ROCKET_Block_List, b => !ROCKET_Block_List.Contains(b) & !(b is IMyProgrammableBlock) & b.CustomData.Contains("[ROCKET]") || !ROCKET_Block_List.Contains(b) & !(b is IMyProgrammableBlock) & b.CustomName.Contains("[ROCKET]"));
                    if (_ROCKET_Block_List.Count > 0) //---- Сборка торпед на подвеске и проверка на 
                    {
                        List<IMyShipMergeBlock> Merge_List = new List<IMyShipMergeBlock>();
                        GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(Merge_List, b => b.CustomName.Length >= 3 & TAG_List.Contains(b.CustomName.Substring(0, 3)));
                        foreach (var M in MERGES_List)
                        {
                            int i = 0; foreach (var b in Merge_List) if (M.V3I_ID == b.Position) { M.MERGE0 = b; i++; }
                            if (i == 0) { M.MERGE0 = null; M.torpedoStatus = 4; M.MIS.torpedoStatus = 4; ROCKET_Block_List.Clear(); }
                        }
                        foreach (var Blokc in _ROCKET_Block_List)
                        {
                            int i = -1, _sort = 0, i0 = -1; double Leht = 100; string s = "";
                            foreach (var Merge in MERGES_List)
                            {
                                i++;
                                if (Merge.MERGE0 != null) 
                                { 
                                    Leht = Math.Min(Leht, (Merge.MERGE0.GetPosition() - Blokc.GetPosition()).Length()); 
                                    if (Leht == (Merge.MERGE0.GetPosition() - Blokc.GetPosition()).Length()) 
                                        i0 = i; 
                                }
                            }
                            if (i0 != -1)
                            {
                                if (Blokc.IsSameConstructAs(MERGES_List[i0].MERGE0))
                                {
                                    if (MERGES_List[i0].torpedoStatus > 2)
                                    {
                                        MISSILE mis = new MISSILE(); MISSILES_List.Add(mis); MERGES_List[i0].MIS = mis; MERGES_List[i0].torpedoStatus = 0;
                                    }
                                    if (Blokc is IMyShipMergeBlock) { MERGES_List[i0].MIS.MERGE1 = Blokc as IMyShipMergeBlock; s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " Merge"; _sort++; }
                                    if (Blokc is IMyGyro) 
                                    { 
                                        MERGES_List[i0].MIS.GYRO = Blokc as IMyGyro; 
                                        s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " Gyro"; 
                                        _sort++; 
                                    }
                                    if (Blokc is IMyBatteryBlock) { MERGES_List[i0].MIS.POWER = Blokc as IMyBatteryBlock; s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " Battery"; _sort++; (Blokc as IMyBatteryBlock).ChargeMode = ChargeMode.Recharge; }
                                    if (Blokc is IMyReactor) { MERGES_List[i0].MIS.POWER = Blokc as IMyReactor; s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " Reactor"; _sort++; }
                                    if (Blokc is IMyGasTank) { MERGES_List[i0].MIS.TANK.Add(Blokc as IMyGasTank); s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " GasTank" + MERGES_List[i0].MIS.TANK.Count; MERGES_List[i0].MIS.TANK_bool = true; _sort++; (Blokc as IMyGasTank).Stockpile = true; }
                                    if (Blokc is IMyThrust) { MERGES_List[i0].MIS.THRUSTERS.Add(Blokc as IMyThrust); s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " Thrust " + MERGES_List[i0].MIS.THRUSTERS.Count; _sort++; }
                                    if (Blokc is IMyWarhead) { MERGES_List[i0].MIS.WARHEADS.Add(Blokc as IMyWarhead); s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " Warhead " + MERGES_List[i0].MIS.WARHEADS.Count; _sort++; }
                                    if (Blokc is IMyCameraBlock) { MERGES_List[i0].MIS.CAMERA = Blokc as IMyCameraBlock; s = "[" + MERGES_List[i0].ID + "_" + MERGES_List[i0].TAG.Substring(1, 2) + " Camera"; _sort++; (Blokc as IMyCameraBlock).EnableRaycast = true; }
                                    if (_sort != 0) { Blokc.CustomData = "[ROCKET]\n" + s; ROCKET_Block_List.Add(Blokc); if (pref_b) Blokc.CustomName = pref_s + s; }
                                    else { Blokc.CustomName = " [???] Блок не определён"; Blokc.CustomData = s; }
                                }
                            }
                        }
                    }
                }
                if (Avto_SV) //---- Авто-сварка
                {
                    int i = SV_List.Count;
                    if (_Avto_SV)
                    {
                        foreach (var b in SV_List)
                        {
                            if (b is IMyTimerBlock) { (b as IMyTimerBlock).Enabled = true; i--; if (b_SV) (b as IMyTimerBlock).StartCountdown(); b_SV = false; }
                            if (b is IMyShipWelder) { (b as IMyShipWelder).Enabled = true; i--; }
                            if (b is IMyProjector) { (b as IMyProjector).Enabled = true; i--; }
                        }
                        ias = 3;
                    }
                    else
                    {
                        foreach (var b in SV_List)
                        {
                            if (b is IMyShipWelder) { (b as IMyShipWelder).Enabled = false; i--; }
                            if (b is IMyProjector) { (b as IMyProjector).Enabled = false; i--; }
                            if (b is IMyTimerBlock) { (b as IMyTimerBlock).StopCountdown(); i--; b_SV = true; }
                        }
                        ias = 1;
                    }
                    if (i > 0) { GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(SV_List, a => a.CustomData.Contains("[Avto_SV]") & !(a is IMyProgrammableBlock)); ias = 2; }
                    if (i == SV_List.Count) ias = 4;
                    if (SV_List.Count == 0) ias = 0;
                }
                if (Avto_BAM & !_Avto_BAM) { _Avto_BAM = true; }
                foreach (var R in MERGES_List) //---- Проверка готовности торпед
                {
                    if (R.torpedoStatus < 3 & R.MERGE0 == null) { R.torpedoStatus = 4; R.MIS.torpedoStatus = 4; }
                    if (R.torpedoStatus < 3)
                    {
                        bool power = true;
                        if (R.MIS.MERGE1 != null & R.MIS.POWER != null & R.MIS.POWER is IMyReactor)
                        {
                            power = false;
                            if (R.MIS.POWER.IsFunctional)
                            {
                                IMyInventory U = R.MIS.POWER.GetInventory();
                                if ((float)U.CurrentMass < Uran_min)
                                {
                                    float L = (Uran / 1000) - (float)U.CurrentVolume;
                                    if (L + (float)U.CurrentVolume > (float)U.MaxVolume) { L = (float)(U.MaxVolume - U.CurrentVolume); }
                                    List<MyItemType> items = new List<MyItemType>();
                                    U.GetAcceptedItems(items);
                                    foreach (var b in Cargo) if (b.IsConnectedTo(R.MIS.POWER.GetInventory()) & b.ContainItems((MyFixedPoint)L, items[0])) { U.TransferItemFrom(b, (MyInventoryItem)b.FindItem(items[0]), (MyFixedPoint)L * 1000); break; }
                                }
                                else power = true;
                            }
                        }
                        else if (R.MIS.MERGE1 != null & R.MIS.POWER != null & R.MIS.POWER is IMyBatteryBlock)
                        {
                            power = true;
                        }
                        else power = false;
                        if (R.MIS.MERGE1 != null) { R.torpedoStatus = 2; }
                        if (R.MIS.MERGE1 != null & power & R.MIS.GYRO != null & R.MIS.WARHEADS.Count > 0 & R.MIS.THRUSTERS.Count > 0)
                        {
                            bool _TANK_bool = true;
                            if (R.MIS.TANK_bool)
                            {
                                int _TANK = 0; double _TANK2 = 0;
                                foreach (var b in R.MIS.TANK) if (b != null & b.IsFunctional) { _TANK++; _TANK2 = (_TANK2 + b.FilledRatio); }
                                if (_TANK == 0 || _TANK2 == 0) { R.torpedoStatus = 2; _TANK_bool = false; }
                                else if (_TANK == R.MIS.TANK.Count & _TANK2 > R.MIS.TANK.Count * 0.9) { R.torpedoStatus = 1; _TANK_bool = true; }
                                else _TANK_bool = false;
                            }
                            int _THRUSTERS = 0; foreach (var b in R.MIS.THRUSTERS) if (b != null & b.IsFunctional) _THRUSTERS++;
                            int _WARHEADS = 0; foreach (var b in R.MIS.WARHEADS) if (b != null & b.IsWorking) _WARHEADS++;
                            if (R.MIS.GYRO.IsFunctional & _THRUSTERS > 0 & _WARHEADS > 0 & _TANK_bool) R.torpedoStatus = 1;
                        }
                        if (Avto_BAM & R.torpedoStatus == 1) { _Avto_BAM = false; iab = 1; }
                    }
                    //else { R.torpedoStatus = R.MIS.torpedoStatus; } //?????????????
                }
                if (TRASH) //---- Сбросить и уничтожить неисправные торпеды на подвеске
                {
                    foreach (var b in MERGES_List)
                    {
                        if (b.torpedoStatus == 2)
                        {
                            if (b.MIS.MERGE1 != null) { b.torpedoStatus = 4; b.MIS.torpedoStatus = 4; b.MIS.MERGE1.Enabled = false; }
                            else { b.torpedoStatus = 4; b.MIS.torpedoStatus = 4; }
                            foreach (var w in b.MIS.WARHEADS) { w.DetonationTime = 20; w.StartCountdown(); }
                        }
                    }
                    TRASH = false;
                }
                if (_Avto_BAM) //---- Запуск самоуничтожения пусковой установки
                {
                    if (List_BAM.Count > 0)
                    {
                        int i = List_BAM.Count;
                        foreach (var b in List_BAM)
                        {
                            if (b is IMyTimerBlock) { (b as IMyFunctionalBlock).Enabled = true; (b as IMyTimerBlock).StartCountdown(); i--; }
                            if (b is IMyWarhead) { (b as IMyWarhead).StartCountdown(); i--; }
                        }
                        if (i != 0) iab = 2; else iab = 3;
                        if (i == List_BAM.Count) iab = 0; _Avto_BAM = false; List_BAM.Clear();
                    }
                    else Avto_BAM = false;
                }
                //---- Настройка авто-запуска торпед
                if (Avto_Fire)
                {

                    if (TARGET_List.Count > 0 & t_mif == 0)
                    {
                        int i = 0; iaf = 3;
                        foreach (var t in TARGET_List)
                        {
                            if (t.target_MISSILE < t.MAX_target_MISSILE & (t.target.Position - Me.GetPosition()).Length() < FIRE_Dist)
                            {
                                _Avto_Fire = true; i++; Runtime.UpdateFrequency = UpdateFrequency.Update1; iaf = 3;
                            }
                        }
                        if (i == 0) { _Avto_Fire = false; iaf = 1; }
                    }
                    else if (t_mif != 0 & m_1 < t_0 + t_mif)
                    {
                        _Avto_Fire = true; Runtime.UpdateFrequency = UpdateFrequency.Update1; iaf = 3;
                    }
                    else { _Avto_Fire = false; iaf = 1; }
                    if (Avto_Fire & _Avto_Fire & m_0 == 0) { iaf = 2; }
                    if (Avto_Fire & _Avto_Fire & !OnOff) iaf = 4;
                }
                else if (_Avto_Fire) { _Avto_Fire = false; }
                //---- Запуск торпед
                if (arg.Contains("Fire_") & arg.Count() > 5 & OnOff || _Avto_Fire & OnOff)
                {
                    int i = 0; _Avto_SV = true;
                    if (arg.Contains("Fire_")) i = TAG_List.IndexOf("[" + arg.Substring(5, 1) + "]");
                    int i1 = i;
                    if (_Avto_Fire) { i = MERGES_TAG_List.Count - 1; i1 = 0; }
                    if (i != -1 & i < MERGES_TAG_List.Count)
                    {
                        WARHEADS_Detonation = false; m_2++;
                        if (TAG_privat) i1 = 0;
                        for (; i1 <= i; i--)
                        {
                            foreach (var rocket in MERGES_TAG_List[i].MERGES_ROCKET_List)
                            {
                                if (rocket.torpedoStatus == 1) //---- Подготоввка и запуск торпеды
                                {
                                    iaf = 3;
                                    rocket.torpedoStatus = 3;
                                    rocket.MIS.torpedoStatus = 3;
                                    rocket.MIS.MERGE1.ApplyAction("OnOff_Off");
                                    if (!mif) rocket.MIS.TARGET_MIF = 0;
                                    foreach (var b in rocket.MIS.THRUSTERS) //---- Готовит двигатели и извлекает наибольшее направление тяги
                                    {
                                        b.ApplyAction("OnOff_On");
                                        /*
                                        Dictionary<Vector3D, double> ThrustDict = new Dictionary<Vector3D, double>();
                                        Vector3D Fwd = b.WorldMatrix.Forward;
                                        double Thrval = b.MaxEffectiveThrust;
                                        if (ThrustDict.ContainsKey(Fwd) == false)
                                        { ThrustDict.Add(Fwd, Thrval); }
                                        else
                                        { ThrustDict[Fwd] = ThrustDict[Fwd] + Thrval; }
                                        List<KeyValuePair<Vector3D, double>> ThrustList = ThrustDict.ToList();
                                        ThrustList.Sort((x, y) => y.Value.CompareTo(x.Value));
                                        Vector3D ThrForward = ThrustList[0].Key;
                                        */
                                        b.ThrustOverride = b.MaxThrust;
                                        rocket.MIS.MissileThrust += b.MaxThrust;
                                        rocket.MIS.MissileMass += b.Mass;
                                    }
                                    if (rocket.MIS.POWER is IMyBatteryBlock)
                                    {
                                        rocket.MIS.POWER.ApplyAction("OnOff_On");
                                        (rocket.MIS.POWER as IMyBatteryBlock).ChargeMode = ChargeMode.Discharge;
                                        rocket.MIS.MissileMass += rocket.MIS.POWER.Mass;
                                    }
                                    if (rocket.MIS.POWER is IMyReactor)
                                    {
                                        rocket.MIS.POWER.ApplyAction("OnOff_On");
                                        rocket.MIS.MissileMass += rocket.MIS.POWER.Mass;
                                    }
                                    if (rocket.MIS.TANK_bool) foreach (var b in rocket.MIS.TANK) b.Stockpile = false;
                                    rocket.MIS.GYRO.Enabled = true;
                                    rocket.MIS.MissileMass += rocket.MIS.GYRO.Mass; //---- Добавляет Дополнительную Массу И Устанавливает Мах возможную тягу
                                    rocket.MIS.MissileMass += rocket.MIS.MERGE1.Mass;
                                    rocket.MIS.MissileAccel = rocket.MIS.MissileThrust / rocket.MIS.MissileMass;
                                    rocket.MIS.IsLargeGrid = rocket.MIS.GYRO.CubeGrid.GridSizeEnum == MyCubeSize.Large; //---- Устанавливает Тип сетки
                                    rocket.MIS.FuseDistance = rocket.MIS.IsLargeGrid ? 16 : 7;
                                    i = -1;
                                    break;
                                }
                            }
                        }
                    }
                }
                Time_Step = 0;
            }
            #endregion Сборка, проверка и запуск ТОРПЕД (КОНЕЦ)
            //Echo("4.0");
            #region Определение статуса ТОРПЕД и ЦЕЛЕЙ (НАЧАЛО)
            if (!Start & Me.CustomData.Contains(Info_Data))
            {
                //---- Определение статуса ТОРПЕД
                foreach (var R in MISSILES_List)
                {
                    if (R.torpedoStatus == 3)
                    {
                        if (R.POWER != null & R.GYRO != null & R.THRUSTERS.Count > 0)
                        {
                            bool _POWER = true;
                            bool _TANK_bool = true;
                            int _THRUSTERS = 0; foreach (var b in R.THRUSTERS) if (b != null & b.IsFunctional) _THRUSTERS++;
                            int _WARHEADS = 0; foreach (var b in R.WARHEADS) if (b != null & b.IsFunctional) _WARHEADS++;
                            if (R.POWER.IsFunctional)
                            {
                                if (R is IMyReactor) { if ((float)R.POWER.GetInventory().CurrentMass == 0) { _POWER = false; } }
                                if (R is IMyBatteryBlock) { _POWER = true; }
                            }
                            if (R.TANK_bool)
                            {
                                int _TANK = 0; double _TANK2 = 0;
                                foreach (var b in R.TANK) if (b != null & b.IsFunctional) { _TANK++; _TANK2 += b.FilledRatio; }
                                if (_TANK != 0 & _TANK2 != 0) { _TANK_bool = true; }
                                //else { _TANK_bool = false; }
                            }
                            if (_POWER & R.GYRO.IsFunctional & _THRUSTERS > 0 & _WARHEADS > 0 & _TANK_bool) { R.torpedoStatus = 3; }
                            else R.torpedoStatus = 4;
                        }
                        else R.torpedoStatus = 4;
                        if (R.torpedoStatus == 3 & WARHEADS_Detonation) { R.torpedoStatus = 4; foreach (var b in R.WARHEADS) b.DetonationTime = 1; if (R.TARGET_MIF == 0) { R.TARGET_MIF = 1; m_Mif--; } }
                        if (R.torpedoStatus == 3 & R.IS_CLEAR) { STD_GUIDANCE(R); foreach (var b in R.WARHEADS) { b.DetonationTime = 10; } }
                        if (R.torpedoStatus == 3 & !R.IS_CLEAR) { if ((R.GYRO.GetPosition() - Me.GetPosition()).Length() > Str_Dist) { R.IS_CLEAR = true; foreach (var b in R.WARHEADS) { b.DetonationTime = 30; b.StartCountdown(); } } }
                        if (R.torpedoStatus == 4 & R.target_info != null) { R.target_info.target_MISSILE--; R.target_info = null; }
                        if (R.torpedoStatus == 4 & R.TARGET_MIF == 0) { R.TARGET_MIF = 1; m_Mif--; }
                        if (R.torpedoStatus == 3 & R.IS_CLEAR & R.target_info != null & TARGET_List.Count > 0) //---- Оновление инфо о цели
                        {
                            if (R.target_info.target_Tame_On < R.target_info.target_Tame_Off)
                            {
                                int i = 0;
                                foreach (var b in TARGET_List)
                                {
                                    i++;
                                    if (R.target_info.target.EntityId == b.target.EntityId)
                                    {
                                        R.target_info = b; i = 0; break;
                                    }
                                }
                                if (i == TARGET_List.Count) { R.target_info.target_MISSILE--; R.target_info = null; }
                            }
                            else R.target_info = null;
                        }
                    }
                }
                TRASH = false; //Echo("4.1");
                //---- Сбор целей в общий список целей
                if (List_designator.Count > 0)
                {
                    List_target_new.Clear();
                    foreach (var b in List_designator)
                    {
                        if (b != null & b.IsWorking & b.IsAimed)
                        {
                            new_Target = b.GetTargetedEntity();
                            if (new_Target.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) List_target_new.Add(new_Target);
                        }
                    }
                }
                //---- Захват и соправождение цели Raycast_камерой кокпита
                if (CAMERA != null & arg == "LOCK" || CAMERA != null & CAMERA_LOCK)
                {
                    Vector3D ThisEntity = new Vector3D();
                    MyDetectedEntityInfo Scanpos2 = new MyDetectedEntityInfo();
                    if (arg == "LOCK") { CAMERA_LOCK = !CAMERA_LOCK; ThisEntity = CAMERA.Raycast(CAM_Dist, 0, 0).Position; }
                    else if (Scanpos.EntityId == 0) { ThisEntity = CAMERA.Raycast(CAM_Dist, 0, 0).Position; }
                    else { ThisEntity = Scanpos.BoundingBox.Center; }
                    if (CAMERA.CanScan(ThisEntity))
                    {
                        Scanpos2 = CAMERA.Raycast(ThisEntity);
                        if (Scanpos2.EntityId != 0 & Scanpos.EntityId == 0 & TARGET_List.Count > 0) foreach (var b in TARGET_List) if (Scanpos2.EntityId == b.target.EntityId) b.target_Status = 0;
                        if (Scanpos.EntityId == Scanpos2.EntityId) { Scanpos = Scanpos2; List_target_new.Add(Scanpos); }
                        else if (Scanpos2.EntityId != 0 & Scanpos2.Relationship != MyRelationsBetweenPlayerAndBlock.NoOwnership & Scanpos2.Relationship != MyRelationsBetweenPlayerAndBlock.FactionShare)
                        { Scanpos = Scanpos2; }
                        else { Scanpos = new MyDetectedEntityInfo(); }
                    }
                    else { Scanpos = new MyDetectedEntityInfo(); }
                    if (!CAMERA_LOCK) { Scanpos = new MyDetectedEntityInfo(); }
                }
                //---- Работа с антенной
                //-----------------------------------------------------------------------
                //Echo("4.2");
                //---- Cоздание новой цели или обновление старых
                if (List_target_new.Count > 0)
                {
                    foreach (var b in List_target_new)
                    {
                        int i = 0;
                        TARGET target = new TARGET { ID_target = b.EntityId, target = b };
                        foreach (var _target in TARGET_List)
                        {
                            i++;
                            if (b.EntityId == _target.ID_target & b.EntityId != 0)
                            {
                                if (_target.target.Type == MyDetectedEntityType.SmallGrid) _target.MAX_target_MISSILE = t_Small;
                                else if (_target.target.Type == MyDetectedEntityType.LargeGrid) _target.MAX_target_MISSILE = t_Large;
                                _target.target = b; _target.target_Tame_On = 0; i = 0; break;
                            }
                        }
                        if (TARGET_List.Count == 0 || i != 0 & b.EntityId != 0) TARGET_List.Add(target);
                    }
                    List_target_new.Clear();
                }
                //Echo("4.3");
                //---- Удаление совсем старых целей
                List<TARGET> mysor = new List<TARGET>();
                foreach (var b in TARGET_List)
                {
                    b.target_Tame_On++;
                    if (b.target_Tame_On > b.target_Tame_Off)
                    {
                        mysor.Add(b); //Echo("4.3.0");
                        foreach (var m in MISSILES_List) if (m.target_info != null) if (m.target_info.target.EntityId == b.target.EntityId) m.target_info = null;
                    }
                }
                foreach (var b in mysor) TARGET_List.Remove(b);
                //Echo("4.3.1");
                //---- Удаление торпед со статусом 4
                List<MISSILE> mysor1 = new List<MISSILE>();
                foreach (var b in MISSILES_List) { if (b.torpedoStatus == 4) mysor1.Add(b); }
                foreach (var b in mysor1) MISSILES_List.Remove(b);
                //Echo("4.3.2");
                //---- Определение ближайшей и важнейшей цели
                foreach (var t in TARGET_List) { if (t.target_MISSILE < t.MAX_target_MISSILE) { t.target_Dist = (Me.GetPosition() - t.target.Position).Length(); } }
                TARGET_List.Sort((x, y) => x.target_Dist.CompareTo(y.target_Dist));
                TARGET_List.Sort((x, y) => x.target_MISSILE.CompareTo(y.target_MISSILE));
                TARGET_List.Sort((x, y) => x.target_Status.CompareTo(y.target_Status));
                //Echo("4.3.3");
                //---- Назначение ближайшей цели новой торпеды
                foreach (var t in TARGET_List)
                {
                    if (t.target_MISSILE < t.MAX_target_MISSILE)
                    {
                        bool b = false;
                        foreach (var m in MISSILES_List) { if (m.torpedoStatus == 3 & m.target_info == null & m.TARGET_MIF != 0) { b = true; m.Missil_Dist = (m.GYRO.GetPosition() - t.target.Position).Length(); } }
                        if (b)
                        {
                            MISSILES_List.Sort((x, y) => x.Missil_Dist.CompareTo(y.Missil_Dist));
                            foreach (var m in MISSILES_List) { if (m.torpedoStatus == 3 & m.target_info == null & t.target_MISSILE < t.MAX_target_MISSILE) { m.target_info = t; t.target_MISSILE++; } }
                        }
                    }
                }
                //Echo("4.4");
                //---- Направление торпеды по курсу
                if (HUNTER & cockpit != null & mif)
                {
                    List<MISSILE> mis_List = new List<MISSILE>();
                    double dist = 0;
                    foreach (var m in MISSILES_List)
                    {
                        if (m.torpedoStatus == 3 & m.target_info == null & m.TARGET_MIF != 0)
                        {
                            dist = dist = Math.Min(dist, (m.GYRO.GetPosition() - (cockpit.GetPosition() + cockpit.WorldMatrix.Forward + 1000)).Length());
                            m.Missil_Dist = dist;
                            mis_List.Add(m);
                        }
                    }
                    if (mis_List.Count > 0 & m_Mif < t_Small)
                    {
                        mis_List.Sort((x, y) => x.Missil_Dist.CompareTo(y.Missil_Dist));
                        mis_List[0].TARGET_MIF = 0; m_Mif++;
                    }
                }
                else if (mif & m_Mif > 0) { foreach (var m in MISSILES_List) if (m.torpedoStatus == 3 & m.TARGET_MIF == 0) m.TARGET_MIF = 1; m_Mif = 0; }

            }
            #endregion Определение статуса ТОРПЕД и ЦЕЛЕЙ (КОНЕЦ)
            //Echo("5.0");
            #region ВИЗУАЛИЗАЦИЯ (НАЧАЛО)
            if (AvtoLCD)
            {
                IMyTerminalBlock block;
                foreach (var lcd in LCD_info)
                {
                    if (cockpit != null) block = cockpit; else block = Me;
                    var frame = lcd.DrawFrame(); var sprite = new MySprite();
                    Vector2 vec = lcd.SurfaceSize; if (vec.Y < 256) vec.Y = 256;
                    int xx = 1, xx2 = 0; if (vec.X > 256 & vec.Y > 256) xx = 2; if (xx == 2) xx2 = 1;
                    if (CAMERA != null) //---- Работа камеры
                    {
                        int i = 0;
                        string a0 = "[CAMERA]";
                        if (HUNTER) a0 = "[CAMERA][H]";
                        string a2 = "";
                        if (CAMERA_LOCK) i = 1;
                        if (Scanpos.EntityId != 0 & Scanpos.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) { a2 = " Враг"; i = 4; }
                        if (Scanpos.EntityId != 0 & Scanpos.Relationship == MyRelationsBetweenPlayerAndBlock.Friends) { a2 = " Друг"; i = 3; }
                        if (Scanpos.EntityId != 0 & Scanpos.Relationship == MyRelationsBetweenPlayerAndBlock.Neutral) { a2 = " Друг"; i = 3; }
                        //Текст Спрайт         ---//---
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = a0 + a2, Position = new Vector2(vec.X / 2 - 58 * xx, 60 / xx), RotationOrScale = 0.5f * xx, Color = _Color[i], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }
                    else //---- Работа HUNTER
                    {
                        int i = 0;
                        string a0 = "[HUNTER]";
                        string a2 = "";
                        if (HUNTER) { i = 1; a2 = " On"; }
                        else { i = 0; a2 = " Off"; }
                        //Текст Спрайт         ---//---
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = a0 + a2, Position = new Vector2(vec.X / 2 - 58 * xx, 60 / xx), RotationOrScale = 0.5f * xx, Color = _Color[i], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }
                    //---- Фон
                    sprite = new MySprite() { Type = SpriteType.TEXTURE, Data = "Grid", Position = vec / 2, RotationOrScale = 0.0f, Size = vec, Color = _Color[0].Alpha(0.50f / xx), Alignment = TextAlignment.CENTER };
                    frame.Add(sprite);
                    //---- Центр
                    sprite = new MySprite() { Type = SpriteType.TEXTURE, Data = "Circle", Position = vec / 2, RotationOrScale = 0.0f, Size = new Vector2(3, 3) * xx, Color = _Color[1], Alignment = TextAlignment.CENTER };
                    frame.Add(sprite);
                    //---- OnOFF
                    sprite = new MySprite() { Type = SpriteType.TEXT, Data = "Fire_" + NFF, Position = new Vector2(vec.X - 70 - xx2 * 30, vec.Y / 2 + 10 / xx), RotationOrScale = 0.5f * xx, Color = _Color[inff], Alignment = TextAlignment.LEFT, FontId = "White" };
                    frame.Add(sprite);

                    if (MERGES_List.Count > 0) //---- Кол-во торпед
                    {
                        int i3 = 0, i2 = m_1 - t_0, i4 = 0;
                        if (m_0 <= MERGES_List.Count / 2) i3 = 4; if (m_0 > MERGES_List.Count / 2) i3 = 2; if (m_0 == MERGES_List.Count) i3 = 1;
                        if (i2 <= t_0 / 2) i4 = 4; if (i2 > t_0 / 2) i4 = 2; if (i2 >= t_0) i4 = 1; if (i2 >= (t_0 + t_Small) * 2) i4 = 3; if (i2 == 0 & t_0 == 0) i4 = 0;
                        //---- Кол-во торпед на подвеске
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = "" + m_0 + "/" + MERGES_List.Count, Position = new Vector2(20, vec.Y / 2 + 10 / xx), RotationOrScale = 0.5f * xx, Color = _Color[i3], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                        //---- Кол-во торпед запущено
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = "" + m_1 + "/" + t_0, Position = new Vector2(20, vec.Y / 2 + 43 + xx2 * 3), RotationOrScale = 0.5f * xx, Color = _Color[i4], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }
                    if (TARGET_List.Count > 0) //---- Кол-во целей
                    {
                        //---- Текст
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = "ЦЕЛИ - " + TARGET_List.Count, Position = new Vector2(18, vec.Y / 2 + 33 - xx2 * 6), RotationOrScale = 0.5f * xx, Color = _Color[4], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }
                    //---- Опции
                    if (SPEED)
                    {
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = "SPEED", Position = new Vector2(vec.X - 56 - xx2 * 30, vec.Y / 2 + 27), RotationOrScale = 0.5f * xx, Color = _Color[isp], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }
                    if (Avto_Fire)
                    {
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = "AF", Position = new Vector2(vec.X - 86 - xx2 * 36, vec.Y / 2 + 43 + xx2 * 6), RotationOrScale = 0.5f * xx, Color = _Color[iaf], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }
                    if (Avto_SV)
                    {
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = "AS", Position = new Vector2(vec.X - 66 - xx2 * 18, vec.Y / 2 + 43 + xx2 * 6), RotationOrScale = 0.5f * xx, Color = _Color[ias], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }
                    if (Avto_BAM)
                    {
                        sprite = new MySprite() { Type = SpriteType.TEXT, Data = "AB", Position = new Vector2(vec.X - 46 + xx2 * 1, vec.Y / 2 + 43 + xx2 * 6), RotationOrScale = 0.5f * xx, Color = _Color[iab], Alignment = TextAlignment.LEFT, FontId = "White" };
                        frame.Add(sprite);
                    }

                    foreach (var b in MERGES_List) //---- Отрисовка Торпедной сетки
                    {
                        int x_Pos = b.TV3I_position.X;
                        int y_Pos = b.TV3I_position.Y;
                        int z_Pos = b.TV3I_position.Z;
                        Vector2 Pos = new Vector2((vec.X / 2 - X / 2 * (xx + 2 * xx2)) + x_Pos * (xx + 2 * xx2), (vec.Y - (71 - xx2 * 38) - Y * z_Pos * 3) + y_Pos * 10);
                        //---- Отрисовка Торпед на торпедной сетки
                        sprite = new MySprite()
                        {
                            Type = SpriteType.TEXTURE,
                            Data = "Circle",
                            Position = Pos,
                            RotationOrScale = 0.0f,
                            Size = new Vector2(1, 3) * (xx + 2 * xx2),
                            Color = _Color[b.torpedoStatus],
                            Alignment = TextAlignment.CENTER,
                        };
                        frame.Add(sprite);
                    }
                    foreach (var b in MISSILES_List) //---- Отрисовка Торпед на радаре
                    {
                        if (b.torpedoStatus == 3)
                        {
                            float f0 = (float)6000 / 70 / xx;
                            double dist = (block.GetPosition() - b.GYRO.GetPosition()).Length() / f0; //дистанция до цели, в маштабе 1:120
                            Vector3D pos1 = (block.GetPosition() - b.GYRO.GetPosition()) / f0 * block.WorldMatrix.Forward;
                            Vector3D pos2 = (block.GetPosition() - b.GYRO.GetPosition()) / f0 * block.WorldMatrix.Left;
                            float Pos_x = (float)(pos1.Y + pos1.Z); // Смещение перед\зад
                            float Pos_y = (float)(pos2.Y + pos2.Z); // Смещение лево\право
                                                                    //---- Отрисовка Торпед на радаре
                            sprite = new MySprite()
                            {
                                Type = SpriteType.TEXTURE,
                                Data = "Triangle",
                                Position = new Vector2(vec.X / 2 + Pos_y, vec.Y / 2 + Pos_x),
                                RotationOrScale = 0,
                                Size = new Vector2(2, 2) * xx,
                                Color = _Color[b.torpedoStatus],
                                Alignment = TextAlignment.CENTER,
                            };
                            frame.Add(sprite);
                        }
                    }
                    foreach (var b in List_designator) //---- Отрисовка Целе-указателей (союзники)
                    {
                        if (!b.IsSameConstructAs(Me))
                        {
                            string a = "SquareSimple";
                            float f = 0;
                            float f0 = (float)6000 / 70 / xx;
                            double dist = (cockpit.GetPosition() - b.GetPosition()).Length() / f0; //дистанция до цели, в маштабе 1:120
                            Vector3D pos1 = (cockpit.GetPosition() - b.GetPosition()) / f0 * block.WorldMatrix.Forward;
                            Vector3D pos2 = (cockpit.GetPosition() - b.GetPosition()) / f0 * block.WorldMatrix.Left;
                            float Pos_x = (float)(pos1.Y + pos1.Z); // Смещение перед\зад
                            float Pos_y = (float)(pos2.Y + pos2.Z); // Смещение лево\право

                            sprite = new MySprite()
                            {
                                Type = SpriteType.TEXTURE,
                                Data = a,
                                Position = new Vector2(vec.X / 2 + Pos_y, vec.Y / 2 + Pos_x),
                                RotationOrScale = f,
                                Size = new Vector2(3, 3) * xx,
                                Color = Color.Green.Alpha(0.50f),
                                Alignment = TextAlignment.CENTER,
                            };
                            frame.Add(sprite);
                        }
                    }
                    foreach (var b in TARGET_List) //---- Отрисовка Целей на радаре
                    {
                        string a = "SquareSimple";
                        float f = 0;
                        float f0 = (float)6000 / 70 / xx;
                        double dist = (block.GetPosition() - b.target.Position).Length() / f0; //дистанция до цели, в маштабе 1:120
                        Vector3D pos1 = (block.GetPosition() - b.target.Position) / f0 * block.WorldMatrix.Forward;
                        Vector3D pos2 = (block.GetPosition() - b.target.Position) / f0 * block.WorldMatrix.Left;
                        float Pos_x = (float)(pos1.Y + pos1.Z); // Смещение перед\зад
                        float Pos_y = (float)(pos2.Y + pos2.Z); // Смещение лево\право

                        sprite = new MySprite()
                        {
                            Type = SpriteType.TEXTURE,
                            Data = a,
                            Position = new Vector2(vec.X / 2 + Pos_y, vec.Y / 2 + Pos_x), //new Vector2( 128 + Pos_y, 128 + Pos_x),
                            RotationOrScale = f,
                            Size = new Vector2(3, 3) * xx,
                            Color = Color.Red.Alpha(0.50f),
                            Alignment = TextAlignment.CENTER,
                        };
                        frame.Add(sprite);
                    }
                    frame.Dispose();
                }
            }
            #endregion ВИЗУАЛИЗАЦИЯ (КОНЕЦ)
            //Echo("END");
        }// Main(КОНЕЦ)


        //----------  Ведение ракеты к целе       (НАЧАЛО) RdavNav  #RFC#
        //----------
        void STD_GUIDANCE(MISSILE rocket)
        {

            //---- Определение координат цели
            Vector3D ENEMY_POS = new Vector3D();
            bool bam = false;
            int Tame_On = 100;
            if (rocket.target_info != null) { ENEMY_POS = rocket.target_info.target.Position; bam = true; Tame_On = rocket.target_info.target_Tame_On; }
            else //---- Назначение ложно-боевой цели
            {
                if (!mif & cockpit == null & rocket.TARGET_MIF == 0) rocket.TARGET_MIF = 1;
                if (mif) //---- Назначение денамической ложной цели
                {
                    if (cockpit != null)
                    {
                        if (rocket.TARGET_MIF == 0) { mif_list[0] = cockpit.GetPosition() + cockpit.WorldMatrix.Forward * ((rocket.GYRO.GetPosition() - Me.GetPosition()).Length() + 300); ENEMY_POS = mif_list[0]; }
                        else if (rocket.TARGET_MIF != 0 & ((mif_poz + /*cockpit.WorldMatrix.Forward * */mif_list[rocket.TARGET_MIF]) - rocket.GYRO.GetPosition()).Length() < 50)
                        {
                            if (rocket.TARGET_MIF + 1 == mif_list.Count) { rocket.TARGET_MIF = 1; }
                            else rocket.TARGET_MIF += 1;
                            ENEMY_POS = mif_poz + /*cockpit.WorldMatrix.Forward * */mif_list[rocket.TARGET_MIF];
                        }
                        else ENEMY_POS = mif_poz + /*cockpit.WorldMatrix.Forward * */mif_list[rocket.TARGET_MIF];
                    }
                    else
                    {
                        if (rocket.TARGET_MIF != 0 & ((Me.GetPosition() + mif_list[rocket.TARGET_MIF]) - rocket.GYRO.GetPosition()).Length() < 50)
                        {
                            if (rocket.TARGET_MIF + 1 == mif_list.Count) { rocket.TARGET_MIF = 1; }
                            else rocket.TARGET_MIF += 1;
                            ENEMY_POS = Me.GetPosition() + mif_list[rocket.TARGET_MIF];
                        }
                        else ENEMY_POS = Me.GetPosition() + mif_list[rocket.TARGET_MIF];
                    }
                }
                else //---- Назначение статической ложной цели
                {
                    if (rocket.TARGET_MIF == 0 & (mif_list[rocket.TARGET_MIF] - rocket.GYRO.GetPosition()).Length() < 50) rocket.TARGET_MIF = 1;
                    else if ((mif_list[rocket.TARGET_MIF] - rocket.GYRO.GetPosition()).Length() < 50)
                    {
                        if (rocket.TARGET_MIF + 1 == mif_list.Count) rocket.TARGET_MIF = 1; else rocket.TARGET_MIF += 1;
                        ENEMY_POS = mif_list[rocket.TARGET_MIF];
                    }
                    else ENEMY_POS = mif_list[rocket.TARGET_MIF];
                }
                if (TARGET_List.Count > 0) //---- Направление свободной торпеды на перехват к ближайшеё цели в радиусе "auto_target_capture"
                {
                    double dist = auto_target_capture;
                    foreach (var t in TARGET_List)
                    {
                        if ((t.target.Position - rocket.GYRO.GetPosition()).Length() < dist & t.target_Tame_On < 5 & (t.target.Position - mif_list[rocket.TARGET_MIF]).Length() < dist * 3)
                        {
                            dist = (t.target.Position - rocket.GYRO.GetPosition()).Length();
                            ENEMY_POS = t.target.Position; bam = true; Tame_On = t.target_Tame_On;
                        }
                    }
                }
            }

            //---- Обновление цели торпеды её камерой (если она есть)
            if (rocket.target_info != null & rocket.CAMERA != null)
            {
                if (rocket.CAMERA.CanScan(ENEMY_POS))
                {
                    var ThisEntity = rocket.CAMERA.Raycast(ENEMY_POS);
                    if (ThisEntity.EntityId != rocket.target_info.target.EntityId) List_target_new.Add(ThisEntity);
                }
            }

            //---- Сортировка Текущих Скоростей
            Vector3D MissilePosition = rocket.GYRO.CubeGrid.WorldVolume.Center;
            Vector3D MissilePositionPrev = rocket.MIS_PREV_POS;
            Vector3D MissileVelocity = (MissilePosition - MissilePositionPrev) / Global_Timestep;

            Vector3D TargetPosition = ENEMY_POS;
            Vector3D TargetPositionPrev = rocket.TARGET_PREV_POS;
            Vector3D TargetVelocity = (TargetPosition - rocket.TARGET_PREV_POS) / Global_Timestep;

            //---- Использует навигационную систему наведения APN RdavNav
            //-----------------------------------------------------

            //---- Настройка скорости ЛОС и системы PN
            Vector3D LOS_Old = Vector3D.Normalize(TargetPositionPrev - MissilePositionPrev);
            Vector3D LOS_New = Vector3D.Normalize(TargetPosition - MissilePosition);
            Vector3D Rel_Vel = Vector3D.Normalize(TargetVelocity - MissileVelocity);

            //---- И Цессионарии
            Vector3D am = new Vector3D(1, 0, 0); double LOS_Rate; Vector3D LOS_Delta;
            Vector3D MissileForwards = rocket.THRUSTERS[0].WorldMatrix.Backward;

            //---- Вектор/Скорость вращения
            if (LOS_Old.Length() == 0)
            { LOS_Delta = new Vector3D(0, 0, 0); LOS_Rate = 0.0; }
            else
            { LOS_Delta = LOS_New - LOS_Old; LOS_Rate = LOS_Delta.Length() / Global_Timestep; }

            //-----------------------------------------------

            //---- /Скорость Закрытия
            double Vclosing = (TargetVelocity - MissileVelocity).Length();

            //---- Если Под Действием Силы Тяжести Используется Гравитационное Ускорение
            Vector3D GravityComp = new Vector3D();
            if (cockpit != null) GravityComp = -cockpit.GetNaturalGravity();

            //---- Окончательного расчета бокового ускорения
            Vector3D LateralDirection = Vector3D.Normalize(Vector3D.Cross(Vector3D.Cross(Rel_Vel, LOS_New), Rel_Vel));
            Vector3D LateralAccelerationComponent = LateralDirection * PNGain * LOS_Rate * Vclosing + LOS_Delta * 9.8 * (0.5 * PNGain);

            //---- Если Невозможное решение (т. е. скорость поворота макса) Используйте Дрифт Для Отмены Минимальная Т
            double OversteerReqt = (LateralAccelerationComponent).Length() / rocket.MissileAccel;
            if (OversteerReqt > 0.98)
            {
                LateralAccelerationComponent = rocket.MissileAccel * Vector3D.Normalize(LateralAccelerationComponent + (OversteerReqt * Vector3D.Normalize(-MissileVelocity)) * 40);
            }

            foreach (IMyThrust thruster in rocket.THRUSTERS)
            {
                if (thruster.ThrustOverride != thruster.MaxThrust) //---- 12 неравенство приращения для экономии производительности
                { thruster.ThrustOverride = thruster.MaxThrust * 2; }
            }

            //---- Вычисляет Оставшуюся Составляющую Силы И Добавляет Вдоль ЛОС
            double RejectedAccel = Math.Sqrt(rocket.MissileAccel * rocket.MissileAccel - LateralAccelerationComponent.LengthSquared()); //---- Accel должен быть определен в зависимости от того, как вы его срежете
            if (double.IsNaN(RejectedAccel)) { RejectedAccel = 0; }
            LateralAccelerationComponent = LateralAccelerationComponent + LOS_New * RejectedAccel;

            //-----------------------------------------------

            //---- Направляющие К Цели С Помощью Гироскопов
            am = Vector3D.Normalize(LateralAccelerationComponent + GravityComp);
            double Yaw; double Pitch;
            GyroTurn6(am, 18, 0.3, rocket.THRUSTERS[0], rocket.GYRO as IMyGyro, rocket.PREV_Yaw, rocket.PREV_Pitch, out Pitch, out Yaw);

            //---- Обновления Для Следующего Тикового Раунда
            rocket.TARGET_PREV_POS = TargetPosition;
            rocket.MIS_PREV_POS = MissilePosition;
            rocket.PREV_Yaw = Yaw;
            rocket.PREV_Pitch = Pitch;

            //---- Снимает/сиавит предохранитель или подрыв
            if (rocket.WARHEADS.Count > 0)
            {
                if (bam & (TargetPosition - MissilePosition).Length() < rocket.FuseDist)
                { foreach (var item in rocket.WARHEADS) { if (Tame_On < 5 & TARGET_List.Count > 0) (item as IMyWarhead).IsArmed = true; } }
                if ((TargetPosition - MissilePosition).Length() > rocket.FuseDist)
                { foreach (var item in rocket.WARHEADS) { (item as IMyWarhead).IsArmed = false; } }
                if (bam & (TargetPosition - MissilePosition).Length() < rocket.FuseDistance)
                { if (Tame_On < 5 & TARGET_List.Count > 0) { (rocket.WARHEADS[0] as IMyWarhead).Detonate(); rocket.torpedoStatus = 4; m_3++; } }
            }

        }
        //----------  Ведение ракеты к целе       (КОНЕЦ)  RdavNav  #RFC#


        //----------  Карректировка параметров для гироскопа        (НАЧАЛО)  RdavNav  #RFC#
        //----------
        void GyroTurn6(Vector3D TARGETVECTOR, double GAIN, double DAMPINGGAIN, IMyTerminalBlock REF, IMyGyro GYRO, double YawPrev, double PitchPrev, out double NewPitch, out double NewYaw)
        {
            //---- Предустановочные коэффициенты
            NewYaw = 0;
            NewPitch = 0;


            Vector3D ShipUp = REF.WorldMatrix.Up; //---- Извлечение Вперед И Вверх
            Vector3D ShipForward = REF.WorldMatrix.Backward; //---- Назад для двигателей

            //---- Создание И Использование Обратного Кватиниона
            Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
            var InvQuat = Quaternion.Inverse(Quat_Two);

            Vector3D DirectionVector = TARGETVECTOR; //---- Целевой вектор реального Мира
            Vector3D RCReferenceFrameVector = Vector3D.Transform(DirectionVector, InvQuat); //---- Целевой Вектор В Терминах RC-Блока

            //---- Преобразование В Локальный Азимут И Высоту
            double ShipForwardAzimuth = 0; double ShipForwardElevation = 0;
            Vector3D.GetAzimuthAndElevation(RCReferenceFrameVector, out ShipForwardAzimuth, out ShipForwardElevation);

            //---- После Установления Факторов
            NewYaw = ShipForwardAzimuth;
            NewPitch = ShipForwardElevation;

            //---- Применяется Некоторое ПИД-Демпфирование
            ShipForwardAzimuth = ShipForwardAzimuth + DAMPINGGAIN * ((ShipForwardAzimuth - YawPrev) / Global_Timestep);
            ShipForwardElevation = ShipForwardElevation + DAMPINGGAIN * ((ShipForwardElevation - PitchPrev) / Global_Timestep);

            //---- Делает Ли Некоторые Вращения, Чтобы Обеспечить любую Ориентацию Гироскопа
            var REF_Matrix = MatrixD.CreateWorld(REF.GetPosition(), (Vector3)ShipForward, (Vector3)ShipUp).GetOrientation();
            var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, 0)), REF_Matrix); //---- Новообращенные В Мир
            var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(GYRO.WorldMatrix.GetOrientation())); //---- Преобразуется В Локальный Гироскоп

            //---- Логические проверки для НаН
            if (double.IsNaN(TRANS_VECT.X) || double.IsNaN(TRANS_VECT.Y) || double.IsNaN(TRANS_VECT.Z))
            { return; }

            //---- Применяется к ракете
            GYRO.Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X) * GAIN, -1000, 1000);
            GYRO.Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y)) * GAIN, -1000, 1000);
            GYRO.Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z)) * GAIN, -1000, 1000);
            GYRO.GyroOverride = true;
        }
        //----------  Корректировка параметров для гироскопа        (КОНЕЦ)   RdavNav  #RFC#


        //------------КОНЕЦ-------------//
    }
}