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
        //------------НАЧАЛО--------------
        /*
        Mor_All - SpaceEngener's - Trading system
        V1.0
        for TerraNowa Discord: https://spaceengineers.world/

        */

        //Добавлен фильтр коннекторов TRADE
        //Автоматическое включение всех коннекторов
        //Система защиты от читеров 
        //Лог на Отключение Безопасной Зоны если такая есть.
        //Автоматическое включение Респавна.
        //Вытаскивание из своих коннекторов того что там могло заваляться.

        //argument=="c" Убирает с магаза заказ
        //argument=="cc" - Перезапуск системы (почти как рекомпеляция)
        //if(argument=="o"|argument=="щ") - Обновить ассортимент в Торговом автомате (Не Магазин)
        //if(argument=="oo"|argument=="щщ") - Обновить ассортимент в Магазине (Не Торговый автомат)

        //Добавлено автоматическое закрытие дверей каждые 5 сек

        List<IMyFunctionalBlock> Mfb = new List<IMyFunctionalBlock>();

        int zamedlenie = 0;

        List<IMyTerminalBlock> Base_Door_list = new List<IMyTerminalBlock>();                           //Двери
        List<IMyTerminalBlock> Base_Door_all_list = new List<IMyTerminalBlock>();                       //Все двери

        void KonnectorOn()
        {
            GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(Mfb);
            foreach (var b in Mfb)
            {
                if (b is IMyShipConnector)
                {
                    TerminalPropertyExtensions.SetValueBool(b, "OnOff", true);
                }

                if (TerminalPropertyExtensions.GetValueBool(b, "ShowInToolbarConfig")) { TerminalPropertyExtensions.SetValueBool(b, "ShowInToolbarConfig", false); }

            }
        }
        IMyTerminalBlock rr;

        void GetSafCS()
        {
            List<IMySafeZoneBlock> BloksGroup = new List<IMySafeZoneBlock>();
            GridTerminalSystem.GetBlocksOfType(BloksGroup);
            if (BloksGroup.Count != 0)
            {
                rr = BloksGroup[0] as IMyTerminalBlock;
            }
        }

        bool gpd = false;

        void GetSafC()
        {
            if (!gpd & rr is IMySafeZoneBlock)
            {
                if (!TerminalPropertyExtensions.GetValueBool(rr, "SafeZoneCreate"))
                {
                    rr.CustomData += $"Detect Disabled zone in {System.DateTime.Now}\n{logTrade}\n";
                    gpd = true;
                }
                else { GetSafCS(); gpd = false; }
            }
        }

        void TeleportS()
        {
            List<IMyTerminalBlock> isTe = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(isTe, t => t.BlockDefinition.TypeIdString.Contains("SurvivalKit") || t.BlockDefinition.TypeIdString.Contains("MedicalRoom"));
            if (isTe.Count != 0)
            {
                foreach (var rrr in isTe)
                {
                    rr = rrr as IMyTerminalBlock;
                    if (rr.IsFunctional & TerminalPropertyExtensions.GetValueBool(rr, "OnOff") == false)
                    {
                        Echo($"{rr.CustomName} Is enabled");
                        TerminalPropertyExtensions.SetValueBool(rr, "OnOff", true);
                    }
                }
            }
        }

        string LogTest = "";
        IMyStoreBlock OreIngBS;
        IMyTextSurface m, mc;
        string ffiic = "";
        int ii = 0;
        MyStoreItemDataSimple TEST;
        IMyCubeBlock ddd;
        List<IMyStoreBlock> StorBlList = new List<IMyStoreBlock>();
        List<IMyStoreBlock> VendingMList = new List<IMyStoreBlock>();
        List<MyStoreQueryItem> MSQI = new List<MyStoreQueryItem>();
        List<MyStoreItemDataSimple> msIdsSell = new List<MyStoreItemDataSimple>();
        List<IMyShipConnector> ConnListMe = new List<IMyShipConnector>();
        List<IMyShipConnector> ConnListElse = new List<IMyShipConnector>();
        Dictionary<int, List<System.Single>> buysel = new Dictionary<int, List<System.Single>>();
        Dictionary<int, List<MyStoreItemDataSimple>> MagazGazSIDS = new Dictionary<int, List<MyStoreItemDataSimple>>();

        List<System.Single> aa = new List<System.Single>();
        string[] TokenAtAr(string Text, char Delimiter = '\n')
        {
            string[] TokenList = Text.Split(Delimiter);
            for (int ii = 0; ii < TokenList.Length; ii++)
            {
                if (TokenList[ii] == "") { Echo("Fatality\nПроверь CustomData пустых строк недолжно быть!!!"); Me.Enabled = !Me.Enabled; }
            }
            return TokenList;
        }

        void IniMag()
        {
            List<IMyTerminalBlock> mmV = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(mmV, b => b.BlockDefinition.TypeIdString.Contains("VendingMachine") & b.CustomData != "" & b.GetOwnerFactionTag() == Me.GetOwnerFactionTag());
            List<IMyTerminalBlock> mmm = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(mmm, b => b.BlockDefinition.TypeIdString.Contains("StoreBlock") & b.CustomData != "" & b.GetOwnerFactionTag() == Me.GetOwnerFactionTag());//,b=>b.CubeGrid==Me.CubeGrid
            if (StorBlList.Count != 0)
            {
                StorBlList.Clear();
                MagazGazSIDS = new Dictionary<int, List<MyStoreItemDataSimple>>();
                buysel = new Dictionary<int, List<System.Single>>();
            }
            if (mmV.Count != 0)
            {
                if (VendingMList.Count != 0) { VendingMList.Clear(); }
                for (int y = 0; y < mmV.Count; y++) { VendingMList.Add(mmV[y] as IMyStoreBlock); }
            }
            if (mmm.Count != 0)
            {
                for (int i = 0; i < mmm.Count; i++)
                {
                    IMyCubeBlock CBS = mmm[i] as IMyCubeBlock;
                    if (CBS.IsWorking & mmm[i].CustomData != "" & mmm[i].GetOwnerFactionTag() == Me.GetOwnerFactionTag())
                    {

                        MagazGazSIDS.Add(StorBlList.Count, msIdsSell);
                        buysel.Add(StorBlList.Count, aa);
                        StorBlList.Add(mmm[i] as IMyStoreBlock); OreIngBS = mmm[i] as IMyStoreBlock;
                    }

                    LogTest += $"|{buysel.Count}|{MagazGazSIDS.Count}|\n";
                }
            }
        }

        public Program()
        {
            KonnectorOn();
            Me.Enabled = true;
            GetSafCS();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(ConnListMe);
            m = Me.GetSurface(0);
            m.FontColor = Color.Gold; m.ContentType = ContentType.TEXT_AND_IMAGE; m.FontSize = 1; m.TextPadding = 0;//m.Alignment= TextAlignment.CENTER;
            m.WriteText($"Торговых Автоматов ={StorBlList.Count}\nsell-{ii}\nores-{MSQI.Count}\n**", false);
            mc = Me.GetSurface(1);
            mc.ContentType = ContentType.TEXT_AND_IMAGE;
            mc.TextPadding = 1;
            mc.FontSize = 2.0f;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;


            GridTerminalSystem.GetBlocksOfType<IMyDoor>(Base_Door_all_list, filterThis);
            Base_Door_list = Base_Door_all_list.FindAll(x => x.BlockDefinition.SubtypeId.Contains("LargeBlockSlideDoor"));                              //Двери
            Base_Door_list.AddList(Base_Door_all_list.FindAll(x => x.BlockDefinition.TypeIdString.Contains("MyObjectBuilder_Door")));                   //Двери
        }
        void Offa() { Me.Enabled = !Me.Enabled; }
        void ShipTradingKon()
        {

            string ga = ""; string gsa = ""; string tet = "";
            //Вытаскивание из своих коннекторов того что там могло заваляться.
            foreach (IMyShipConnector dd in ConnListMe)
            {
                if (!tomagazOre)
                {
                    TerminalPropertyExtensions.SetValueBool(dd, "OnOff", true);
                }
                IMyCubeBlock fsa = dd as IMyCubeBlock;
                if (Cargos.Count != 0)
                {
                    IMyInventory dds = fsa.GetInventory();
                    IMyInventory ddc = Cargos[0].GetInventory();
                    if (dds.ItemCount > 0 & dds.IsConnectedTo(ddc))
                    {
                        MyInventoryItem ffd = (MyInventoryItem)dds.GetItemAt(0);
                        dds.TransferItemTo(ddc, ffd, ffd.Amount);
                    }
                }

                ga += $"{dd.CustomName} = Trading:{TerminalPropertyExtensions.GetValueBool(dd, "Trading")} On/{dd.Enabled} \n";
                if (dd.OtherConnector is IMyCubeBlock)
                {
                    ddd = dd;
                    IMyCubeBlock foc = dd.OtherConnector as IMyCubeBlock;
                    IMyShipConnector ds = foc as IMyShipConnector;
                    TerminalPropertyExtensions.SetValueBool(ds, "Trading", true);
                    if (ConnListElse.Count == 0) { ConnListElse.Add(ds); }
                    else
                    {
                        bool errorE = false;
                        foreach (var dqa in ConnListElse)
                        {
                            if (dqa.Name == ds.Name) { errorE = true; }
                        }
                        if (!errorE) { ConnListElse.Add(ds); }
                    }
                    gsa += $"Else={ConnListElse.Count} {tet}\n{ds.Status}\n";

                    IMyTerminalBlock da; da = foc as IMyTerminalBlock;
                    gsa += $"{fsa.GetOwnerFactionTag()}~{foc.GetOwnerFactionTag()}\n{dd.CustomName}= Trading:{TerminalPropertyExtensions.GetValueBool(dd, "Trading")}\nGridStat {dd.OtherConnector.Status == MyShipConnectorStatus.Connectable}|{TerminalPropertyExtensions.GetValueBool(dd.OtherConnector, "Trading")}\nGridName:{foc.CubeGrid.CustomName}\nOwner:{foc.OwnerId == fsa.OwnerId}\n";
                    if (!TerminalPropertyExtensions.GetValueBool(dd, "Trading"))
                    {
                        TerminalPropertyExtensions.SetValueBool(dd, "Trading", true);

                    }

                }
            }

            if (ConnListElse.Count != 0)
            {
                if (Runtime.UpdateFrequency != UpdateFrequency.Update100)
                { Runtime.UpdateFrequency = UpdateFrequency.Update100; }
                for (int g = 0; g < ConnListElse.Count; g++)
                {
                    if (ConnListElse[g].Status == MyShipConnectorStatus.Unconnected)
                    {
                        if (TerminalPropertyExtensions.GetValueBool(ConnListElse[g], "Trading"))
                        {
                            TerminalPropertyExtensions.SetValueBool(ConnListElse[g], "Trading", false);
                        }

                    }
                    if (ConnListElse[g].Status == MyShipConnectorStatus.Connectable)
                    {
                        if (!TerminalPropertyExtensions.GetValueBool(ConnListElse[g], "Trading"))
                        {
                            TerminalPropertyExtensions.SetValueBool(ConnListElse[g], "Trading", true);
                        }
                    }
                    tet += $"OC={ConnListElse[g].Status}||";
                }

            }

            if (gsa != "")
            {
                m.WriteText(gsa, false);
            }
            mc.WriteText(ga, false);
            logTrade = gsa + ga;
        }
        string logTrade = "";
        void ReleaseC()
        {
            if (ConnListElse.Count != 0)
            {
                int ifd = ConnListElse.Count;
                foreach (var ss in ConnListElse)
                {
                    if (ss.Status == MyShipConnectorStatus.Connected & !TerminalPropertyExtensions.GetValueBool(ss, "Trading") || !TerminalPropertyExtensions.GetValueBool(ss, "Trading"))
                    {
                        TerminalPropertyExtensions.SetValueBool(ss, "OnOff", false);
                    }
                    if (ss.Status == MyShipConnectorStatus.Unconnected & TerminalPropertyExtensions.GetValueBool(ss, "Trading") || !TerminalPropertyExtensions.GetValueBool(ss, "Trading"))
                    {
                        TerminalPropertyExtensions.SetValueBool(ss, "Trading", false); ifd -= 1;
                    }
                }
                if (ifd <= 0) { ConnListElse.Clear(); }
            }
            else { if (Runtime.UpdateFrequency != UpdateFrequency.Update10) { Runtime.UpdateFrequency = UpdateFrequency.Update10; } }
        }

        int counter1 = 0;
        string s_report = "";
        bool b_doReport = false;
        void PutMagazMAGAZ(int i, int k)
        {
            if (aa.Count - 1 < i) { ggg++; totmagO = 0; return; }
            if (k == StorBlList.Count | ggg >= StorBlList.Count)
            {
                tomagazOre = false;
                Echo("EROR4"); totmagO = 0; Offa(); return;
            }

            if (StorBlList.Count < ggg) { Echo("EROR5"); tomagazOre = false; Offa(); return; }

            msIdsSell = MagazGazSIDS[k];
            if (MSQI.Count < 30 | aa.Count <= i)
            {
                if (aa.Count > 0 & MSQI.Count < 30)
                {
                    MyStoreQueryItem vmas = new MyStoreQueryItem();
                    if (aa[i] < 0.5f)
                    {
                        StorBlList[k].InsertOrder(msIdsSell[i], out vmas.Id);
                    }
                    if (aa[i] > 0.5f)
                    {
                        StorBlList[k].InsertOffer(msIdsSell[i], out vmas.Id);
                    }
                }
            }
            StorBlList[k].GetPlayerStoreItems(MSQI);
            ddds = $"{(100 / ((float)(msIdsSell.Count) / (float)MSQI.Count)):F0}\n";
            if (aa.Count <= i | MSQI.Count == 30) { ggg++; totmagO = 0; return; } else { totmagO++; }
            if (b_doReport == false) { b_doReport = true; }
        }

        public void Main(string argument)
        {
            counter1++;
            zamedlenie++;
            if (counter1 > 1)
            {
                int ddh1 = System.DateTime.Now.Hour;
                if (ddh1 > ddh) { THN(); }
                ReleaseC();
            }
            if (counter1 > 2)
            {
                GetSafC();
                if (go)
                {

                    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(ConnListMe, b => b.CustomData == "TRADE");
                    foreach (var gew in ConnListMe)
                    {
                        TerminalPropertyExtensions.SetValueBool(gew, "OnOff", false);
                    }
                    TeleportS();
                    go = false;
                    IninCAR(); tomagazOre = true; totmagO = 0; ggg = 0;
                }
                else { ShipTradingKon(); }
                counter1 = 0;
            }
            // сборщики хлама
            if (argument == "T") { Me.CustomData = s_report; }
            if (argument == "pp") { Me.CustomData = qqq1 + ffiic + LogTest; }
            if (argument == "p")
            {
                string qqq1 = "";
                for (int po = 0; po < msIdsSell.Count; po++)
                {
                    qqq1 += $"{msIdsSell[po].ItemId}***{msIdsSell[po].Amount}***{msIdsSell[po].PricePerUnit}\n";
                }

                LogTest += qqq1;
            }
            string rt = $"V1={totmagO}***V2={MSQI.Count}\nC={cgg}|{Clearmag}***\nКомпонентов {msIdsSell.Count}";
            Echo($"{(totmag != msIdsSell.Count & tomagaz & msIdsSell.Count > 0 & StorBlList.Count > 0)}{rt}");
            Echo($"inProgress ={ddds}%\n{dddss}\n{dddBs}\n{Runtime.UpdateFrequency}\nCurentC={ConnListElse.Count}\n{ddh}|{System.DateTime.Now.Hour}");


            if (argument == "oo" | argument == "щщ") { IninCAR(); tomagazOre = true; totmagO = 0; ggg = 0; }
            if (tomagazOre & buysel.Count > ggg)
            {
                aa = buysel[ggg];
                MSQI.Clear();
                PutMagazMAGAZ(totmagO, ggg);

            }
            else { tomagazOre = false; }
            if (argument == "o" | argument == "щ") { isNotE = true; rewq = 0; rewqw = 0; PutVendingMSell(); }
            if (isNotE)
            {
                PutsVendingStore(rewq, rewqw);
                rewq++;
            }

            if (argument == "c")
            {// Убирает с магаза заказ
                Clearmag = true; cgg = StorBlList.Count - 1;
            }
            if (Clearmag & cgg >= 0)
            {
                MSQI.Clear();
                StorBlList[cgg].GetPlayerStoreItems(MSQI);
                if (MSQI.Count > 0)
                {
                    StorBlList[cgg].CancelStoreItem(MSQI[0].Id);
                }
                else
                {
                    if (cgg >= 0) { cgg--; }
                    else
                    {
                        if (MSQI.Count == 0) { Clearmag = false; cgg = StorBlList.Count - 1; }
                        Echo("NOITEMS");
                    }
                }
            }
            else { Clearmag = false; }
            if (argument == "cc")
            {
                List<IMyTerminalBlock> mmm = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(mmm, b => b.BlockDefinition.TypeIdString.Contains("StoreBlock") & b.CustomData != "");
                if (StorBlList.Count != 0)
                {
                    StorBlList.Clear();
                    foreach (IMyStoreBlock ssc in mmm)
                    {
                        MSQI.Clear();
                        ssc.GetPlayerStoreItems(MSQI);
                        while (MSQI.Count != 0)
                        {
                            ssc.CancelStoreItem(MSQI[0].Id);
                            MSQI.Clear();
                            ssc.GetPlayerStoreItems(MSQI);
                        }
                    }
                }
            }

            //Закрытие дверей
            if ((zamedlenie % 30) == 0) { List<IMyDoor> Base_door = Base_Door_list.ConvertAll(x => (IMyDoor)x); Base_door.ForEach(door => door.CloseDoor()); }
        }

        bool tomagaz = false; bool Clearmag = false; int totmag = 0;
        int totmagO = 0; string qqq1 = ""; int ggg = 0; int cgg = 0;
        string dddss = ""; string dddBs = ""; bool tomagazOre = false; string ddds = "";
        List<IMyTerminalBlock> Cargos = new List<IMyTerminalBlock>();

        void IniCargoss()
        {
            ffiic = "";
            List<IMyTerminalBlock> Qtargets = new List<IMyTerminalBlock>();
            //Добавлен фильтр для контейнеров
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Qtargets,
       k => k.HasInventory & k.BlockDefinition.TypeIdString.ToLower().Contains("cargoco") & k.CustomData == "TRADE");
            //&k.CustomName.ToLower().Contains(ggf)
            if (Cargos.Count != 0) { Cargos.Clear(); msIdsSell.Clear(); }
            foreach (var b in Qtargets) { Cargos.Add(b); }

        }

        //Фильтр поиска блоков в одном гриде
        bool filterThis(IMyTerminalBlock block)
        {
            return block.CubeGrid == Me.CubeGrid;
        }

        List<MyStoreItemDataSimple> MagazVend = new List<MyStoreItemDataSimple>();
        void PutVendingMSell()
        {
            MagazVend.Clear();
            IMyTerminalBlock oor;
            for (int j = 0; j < VendingMList.Count; j++)
            {
                MSQI.Clear();
                VendingMList[j].GetPlayerStoreItems(MSQI);
                while (MSQI.Count != 0)
                {
                    VendingMList[j].CancelStoreItem(MSQI[0].Id);
                    MSQI.Clear();
                    VendingMList[j].GetPlayerStoreItems(MSQI);
                }
                oor = VendingMList[j] as IMyTerminalBlock;
                string[] gaa = TokenAtAr(VendingMList[j].CustomData);
                IMyInventory gase = oor.GetInventory(0);
                if (gase.ItemCount > 0)
                {

                    for (int i = 0; i < gaa.Length; i++)
                    {
                        int iio = 0;
                        MyItemType oo;
                        string gs = TokenAt(gaa[i], 1);
                        oo = MyItemType.Parse(gs);
                        string we = TokenAt(gaa[i], 0);
                        int dws = (int)float.Parse(TokenAt(gaa[i], 2), System.Globalization.CultureInfo.InvariantCulture);
                        iio = (int)gase.GetItemAmount(oo);
                        if (iio != 0)
                        {
                            TEST = new MyStoreItemDataSimple(MyDefinitionId.Parse(gs), (int)iio, dws);
                            MagazVend.Add(TEST);
                        }
                        //Только 1 Вендинг.
                    }
                }
            }
        }

        int rewq = 0;
        int rewqw = 0;

        bool isNotE = false;

        void PutsVendingStore(int dd, int vv)
        {
            if (VendingMList.Count > vv)
            {
                if (dd < MagazVend.Count)
                {
                    MyStoreQueryItem vmas = new MyStoreQueryItem();
                    VendingMList[vv].InsertOffer(MagazVend[dd], out vmas.Id);
                }
                else { rewqw++; }
            }
            else { isNotE = false; }
        }

        void IninCAR()
        {
            if (Cargos.Count == 0) { IniCargoss(); }
            if (Cargos.Count != 0)
            {
                if (b_doReport == true) { b_doReport = false; s_report = ""; }
                IniMag();
                //new Vending

                for (int df = 0; df < StorBlList.Count; df++)
                {
                    //Clear
                    var ddf = StorBlList[df];
                    MagazGazSIDS[df] = new List<MyStoreItemDataSimple>();
                    buysel[df] = new List<System.Single>();
                    MSQI.Clear();
                    ddf.GetPlayerStoreItems(MSQI);
                    while (MSQI.Count != 0)
                    {
                        ddf.CancelStoreItem(MSQI[0].Id);
                        MSQI.Clear();
                        ddf.GetPlayerStoreItems(MSQI);
                    }
                    string[] gaa = TokenAtAr(StorBlList[df].CustomData);
                    for (int j = 0; j < gaa.Length; j++)
                    {
                        int iio = 0;
                        MyItemType oo;
                        string gs = TokenAt(gaa[j], 1);
                        oo = MyItemType.Parse(gs);
                        string we = TokenAt(gaa[j], 0);
                        int dws = (int)float.Parse(TokenAt(gaa[j], 2), System.Globalization.CultureInfo.InvariantCulture);
                        int dwb = (int)float.Parse(TokenAt(gaa[j], 3), System.Globalization.CultureInfo.InvariantCulture);
                        int dww = (int)float.Parse(TokenAt(gaa[j], 4), System.Globalization.CultureInfo.InvariantCulture);
                        if (dws >= 1)
                        {
                            if (we.Contains("sel"))
                            {
                                //50%на продажу
                                for (int h = 0; h < Cargos.Count; h++)
                                {
                                    //
                                    IMyCubeBlock weq = Cargos[h] as IMyCubeBlock;
                                    if (Me.GetOwnerFactionTag() == weq.GetOwnerFactionTag())
                                    {
                                        IMyInventory gase = Cargos[h].GetInventory(0);
                                        iio += (int)gase.GetItemAmount(oo);
                                    }
                                }
                                if (iio < dwb & b_doReport == false) { if (s_report == "") { s_report += $"{gs}*{we}*{dww}"; } else { s_report += $"\n{gs}*{we}*{dww}"; } }
                                if (iio != 0)
                                {
                                    buysel[df].Add(1f);
                                    TEST = new MyStoreItemDataSimple(MyDefinitionId.Parse(gs), dww, dws);
                                    MagazGazSIDS[df].Add(TEST);
                                }
                            }
                        }
                        if (we.Contains("buy"))
                        {
                            if (dwb > 0)
                            {
                                buysel[df].Add(0f);
                                TEST = new MyStoreItemDataSimple(MyDefinitionId.Parse(gs), dww, dwb);
                                MagazGazSIDS[df].Add(TEST);
                            }
                        }
                    }
                }
            }
        }

        static readonly string oB = "MyObjectBuilder_";

        string TokenAt(string Text, int Index, char Delimiter = '*')
        {
            string[] TokenList = Text.Split(Delimiter);
            if (Index == 1)
            {
                if (!TokenList[Index].Contains(oB)) { TokenList[Index] = $"{oB}{TokenList[Index]}"; }
            }
            return TokenList[Index];
        }

        void THN()
        {
            ddh = System.DateTime.Now.Hour;
            go = true;
        }

        int ddh = -1;
        bool go = false;

        //------------КОНЕЦ--------------
    }
}
