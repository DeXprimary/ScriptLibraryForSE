using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.Immutable;
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
        // info:
        // Refinery/synthesis
        // DP = MyObjectBuilder_Refinery/DeuteriumProcessor
        // Ingot/catalyticGold
        // Ingot/catalyticPlat
        // Ingot/catalyticSilv

        bool enableSortingDP = false, enableSortingSynthesis = false;
        string collectCommonRefinery = "Refinery/Blast Furnace" + "Refinery/refinery_improved" + "Refinery/SharedRefineryT1" + "Refinery/LargeRefinery"
            + "Refinery/refinery_black" + "Refinery/FactoryRefinery";
        int maxMethods, maxInstructions, stepForCalcRuntime;
        double maxRuntime;
        string[] listOre, listIngot, listComponent, listAmmoMagazine, listPhysicalGunObject, listBottle, listAvalableItems, myStorage;
        float[] totalOre, totalIngot, totalComponent, totalAmmoMagazine, totalPhysicalGunObject, totalBottle;
        int currentStep = 0, runIcon = 0, counterSortCargo = 0, counterSortRefinery = 0, counterSortSpecial = 0, counterSortOther = 0, counterItemsInSpecial = 0;
        int currentStepAutocraft = 0;
        bool runIconDirect = true, firstRun = true;
        List<MyInventoryItem> itemsToMove = new List<MyInventoryItem>();
        List<IMyCargoContainer> listCargo = new List<IMyCargoContainer>();
        List<IMyTerminalBlock> listOther = new List<IMyTerminalBlock>();
        List<IMyRefinery> listRefinery = new List<IMyRefinery>();
        List<IMyTerminalBlock> listSpecial = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> tmpListBlock = new List<IMyTerminalBlock>();
        List<IMyAssembler> listAssembler = new List<IMyAssembler>();
        List<MyItemType> tempListAvailableItem = new List<MyItemType>();
        List<IMyTerminalBlock> lcd = new List<IMyTerminalBlock>();
        Color commonColor = new Color(50, 100, 150);
        IMyTextSurface LCDMain, autoCraft;

        public Program()
        {
            if (Storage == "") Save();
            myStorage = Storage.Split('|');
            if (myStorage[0].Contains("True")) enableSortingDP = true;
            else enableSortingDP = false;
            if (myStorage[1].Contains("True")) enableSortingSynthesis = true;
            else enableSortingSynthesis = false;
            Me.CustomData = "Краткое руководство пользователя:\n\n";
            Me.CustomData += "1. Контейнеры сортируются по принципу заполнения от более приоритет-\n";
            Me.CustomData += "ного к менее приоритетному. При этом учитываются применямые тэги в \n";
            Me.CustomData += "названии контейнеров. Сначала заполняются контейнеры с тэгом повышен-\n";
            Me.CustomData += "ной приоритетности: соответственно от !p1 до !p9. Затем заполняются\n";
            Me.CustomData += "контейнеры с любым набором тэгов: !ore, !ingot, !component, !ammo,\n";
            Me.CustomData += "!tools, !bottle. Только потом заполняются все остальные контейнеры.\n";
            Me.CustomData += "2. Почти все значимые заводы/синтезы управляются данным скриптом.\n";
            Me.CustomData += "3. Для более точной настройки контейнеров или др.блоков \"Свои данные\".\n";
            Me.CustomData += "4. С тегом !hide конкретный блок будет игнорироваться скриптом.\n";
            Me.CustomData += "5. С тегом !special конт. или завод можно настроить под собств. нужды.\n";
            Me.CustomData += "6. Добавь тэг !autocraft в назв. LCD панели для настр. автокрафта.\n";
            Me.CustomData += "============================================================\n\n";
            Me.CustomData += "Некоторые начальные установки:\n";
            //Me.CustomData += "Активировать сортировку дейтериевых процессоров: false.\n";
            //Me.CustomData += "Активировать сортировку синтезов: false.\n";
            Me.CustomData += "Активировать сортировку дейтериевых процессоров: " + enableSortingDP + ".\n";
            Me.CustomData += "Активировать сортировку синтезов: " + enableSortingSynthesis + ".\n";
            Me.Enabled = true;
            Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
            Me.GetSurface(0).FontSize = 1;
            Me.GetSurface(0).Alignment = TextAlignment.LEFT;
            Me.GetSurface(1).ContentType = ContentType.TEXT_AND_IMAGE;
            Me.GetSurface(1).FontSize = 5.5f;
            Me.GetSurface(1).Alignment = TextAlignment.CENTER;
            Me.GetSurface(1).Font = "Green";
            Me.GetSurface(1).TextPadding = 0f;
            InitListItems();
            maxMethods = 0; maxInstructions = 0; maxRuntime = 0; stepForCalcRuntime = 0;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            LCDMain = GridTerminalSystem.GetBlockWithName("LCDMain") as IMyTextSurface;
            /*
            List<MyItemType> templist = new List<MyItemType>();
            GridTerminalSystem.GetBlockWithName("[dp]").GetInventory(0).GetAcceptedItems(templist);
            GridTerminalSystem.GetBlockWithName("[dp]").CustomData = GridTerminalSystem.GetBlockWithName("[dp]").BlockDefinition.ToString() + "\n";
            GridTerminalSystem.GetBlockWithName("[dp]").CustomData += "0:\n";
            foreach (MyItemType item in templist)
                GridTerminalSystem.GetBlockWithName("[dp]").CustomData += item.TypeId.Remove(0, 16) + "/" + item.SubtypeId + "\n";
            templist.Clear();
            GridTerminalSystem.GetBlockWithName("[dp]").GetInventory(1).GetAcceptedItems(templist);
            GridTerminalSystem.GetBlockWithName("[dp]").CustomData += "1:\n";
            foreach (MyItemType item in templist)
                GridTerminalSystem.GetBlockWithName("[dp]").CustomData += item.TypeId.Remove(0, 16) + "/" + item.SubtypeId + "\n";
            */
        }

        public void Save()
        {
            Storage = enableSortingDP.ToString() + "|" + enableSortingSynthesis.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & (UpdateType.Terminal | UpdateType.Trigger | UpdateType.Script)) != 0)
            {
                
            }

            if ((updateSource & (UpdateType.Update100 | UpdateType.Update10 | UpdateType.Update1)) != 0)
            {
                //Echo("asd");
                Me.GetSurface(1).WriteText("....................\n");
                for (int i = 0; i < runIcon; i++)
                    Me.GetSurface(1).WriteText(".", true);
                Me.GetSurface(1).WriteText("[]", true);
                for (int i = 0; i < 18-runIcon; i++)
                    Me.GetSurface(1).WriteText(".", true);
                if (runIconDirect)
                    if (runIcon < 18) runIcon++; else runIconDirect = false;
                else
                    if (runIcon > 0) runIcon--; else runIconDirect = true;

                
                switch (currentStep)
                {
                    case (0):
                        //=- Сортировка контейнеров
                        listCargo.Clear();
                        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(listCargo, cargo => (cargo.IsSameConstructAs(Me)) & (!cargo.CustomName.Contains("!hide")) & (!cargo.CustomName.Contains("!special")));

                        if (listCargo.Count > 0)
                        {
                            listCargo.Sort(SortCargoByPryority);

                            if (counterSortCargo < listCargo.Count - 1) counterSortCargo++;
                            else
                            {
                                counterSortCargo = 0;
                                currentStep = 2;
                            }
                            SortCargo(listCargo[counterSortCargo]);
                        }

                        //=- Сортировка заводов
                        listRefinery.Clear();
                        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(listRefinery, refinery => (refinery.IsSameConstructAs(Me)) & (!refinery.CustomName.Contains("!hide")) & (!refinery.CustomName.Contains("!special")));
                        if ((listRefinery.Count > 0) & (listCargo.Count > 0))
                        {
                            if (counterSortRefinery < listRefinery.Count - 1) counterSortRefinery++;
                            else
                            {
                                counterSortRefinery = 0;
                                //currentStep = 2;
                                if (Me.CustomData.Substring(Me.CustomData.IndexOf("процессоров: "), 18).Contains("true") |
                                    Me.CustomData.Substring(Me.CustomData.IndexOf("процессоров: "), 18).Contains("True"))
                                    enableSortingDP = true;
                                else
                                    enableSortingDP = false;
                                if (Me.CustomData.Substring(Me.CustomData.IndexOf("синтезов: "), 15).Contains("true") |
                                    Me.CustomData.Substring(Me.CustomData.IndexOf("синтезов: "), 15).Contains("True"))
                                    enableSortingSynthesis = true;
                                else
                                    enableSortingSynthesis = false;
                                Save();
                            }
                            SortRefinery(listRefinery[counterSortRefinery]);
                        }
                        else
                        {
                            currentStep = 2;
                        }
                        break;
                    case (1):
                        
                        break;
                    case (2):
                        //=- Сортировка специальных блоков
                        listSpecial.Clear();
                        GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(listSpecial, special => (special.CustomName.Contains("!special") & !special.CustomName.Contains("!hide")));
                        if ((listSpecial.Count > 0) & (listCargo.Count > 0))
                        {
                            if (counterSortSpecial < listSpecial.Count - 1) counterSortSpecial++;
                            else
                            {
                                counterSortSpecial = 0;
                                currentStep = 3;
                            }

                            SortSpecial(listSpecial[counterSortSpecial]);
                        }
                        else
                        {
                            currentStep = 3;
                        }
                        break;
                    case (3):
                        //=- Сортировка других блоков
                        listOther.Clear();
                        GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(listOther, other => (other.IsSameConstructAs(Me)) & (!other.CustomName.Contains("!hide")) &
                        (!other.CustomName.Contains("!special")) & (other.HasInventory) & (other.BlockDefinition.TypeIdString.Contains("SurvivalKit") |
                        other.BlockDefinition.TypeIdString.Contains("Cockpit") | other.BlockDefinition.TypeIdString.Contains("CryoChamber") |
                        other.BlockDefinition.TypeIdString.Contains("ShipConnector") | other.BlockDefinition.TypeIdString.Contains("Assembler")));
                        if ((listOther.Count > 0) & (listCargo.Count > 0))
                        {
                            if (counterSortOther < listOther.Count - 1) counterSortOther++;
                            else
                            {
                                counterSortOther = 0;
                                currentStep = 4;
                            }

                            SortOther(listOther[counterSortOther]);
                        }
                        else
                        {
                            currentStep = 4;
                        }
                        break;
                    case (4):
                        //=- Обновление ЛСД Автокрафта
                        if (!firstRun) refreshAutocraftLCD();
                        
                        currentStep = 5;
                        break;

                    case (5):
                        
                        /*
                        for (int i = 0; i < 10; i++)
                        {
                            totalComponent[i] = totalItemMonitor(listComponent[i]);
                            AutoCraft(i);
                        }

                        LCDMain = GridTerminalSystem.GetBlockWithName("LCDMain") as IMyTextSurface;
                        LCDMain.WriteText("", false);
                        for (int i = 0; i < totalComponent.Count(); i++)
                            LCDMain.WriteText(listComponent[i] + " = " + totalComponent[i] + "\n", true);
                        */
                        currentStep = 6;
                        break;

                    case (6):
                        //=- Подсчёт др.предметов

                        //ItemMonitor(listOre);
                        //ItemMonitor(listIngot);
                        //ItemMonitor(listAmmoMagazine);
                        //ItemMonitor(listPhysicalGunObject);
                        //ItemMonitor(listBottle);
                        currentStep = 0;
                        break;
                }

                //=- Подсчёт компонентов и автокрафт
                if (currentStepAutocraft < listComponent.Length)
                {
                    totalComponent[currentStepAutocraft] = totalItemMonitor(listComponent[currentStepAutocraft]);
                    AutoCraft(currentStepAutocraft);
                    currentStepAutocraft++;
                }
                else
                {
                    currentStepAutocraft = 0;
                    firstRun = false;
                } 
                    

                    //=- Вывод динамических характеристик скрипта
                    //if (Runtime.CurrentCallChainDepth > maxMethods) maxMethods = Runtime.CurrentCallChainDepth;
                if (Runtime.CurrentInstructionCount > maxInstructions) maxInstructions = Runtime.CurrentInstructionCount;
                if (stepForCalcRuntime > 1) { if (Runtime.LastRunTimeMs > maxRuntime) maxRuntime = Runtime.LastRunTimeMs; } else stepForCalcRuntime++;
                //Me.GetSurface(0).WriteText("Методов (допуст./макс./текущ.):\n" + Runtime.MaxCallChainDepth.ToString() + " / " + maxMethods + " / "+ Runtime.CurrentCallChainDepth.ToString() + "\n");
                Me.GetSurface(0).WriteText("Инструкций (допуст./макс./текущ.):\n" + Runtime.MaxInstructionCount.ToString() + " / " + maxInstructions + " / " + Runtime.CurrentInstructionCount.ToString() + "\n", false);
                Me.GetSurface(0).WriteText("Время исполенения (макс./текущ.):\n" + maxRuntime + " / " + Runtime.LastRunTimeMs.ToString() + "\n", true);
                Me.GetSurface(0).WriteText("\nВ первую очередь открой \"Свои данные\"\nв программируемом блоке!!!", true);

                //=- Вывод данных в Эхо
                Echo("Обработан контейнер: " + (counterSortCargo + 1) + "/" + listCargo.Count);
                Echo("Обработан рефайнери: " + (counterSortRefinery + 1) + "/" + listRefinery.Count);
                Echo("Обработан спец.блок: " + (counterSortSpecial + 1) + "/" + listSpecial.Count);
                Echo("Обработан др.блок: " + (counterSortOther + 1) + "/" + listOther.Count);
                Echo("Автокрафтинг: " + (currentStepAutocraft + 1) + "/" + listComponent.Length);
            }
        }

        void refreshAutocraftLCD()
        {
            int lengthString = 22;
            string msgCustomData = "Добавь \"+\" перед компонентом для автокрафта:\n";
            string msgTextSurface = "Удали \"+\" перед назв. для отмены автокрафта:\n";
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(lcd, _lcd => _lcd.CustomName.Contains("!autocraft"));
            if (lcd[0] != null)
            {
                if (autoCraft == null) autoCraft = lcd[0] as IMyTextSurface;
                autoCraft.ContentType = ContentType.TEXT_AND_IMAGE;
                autoCraft.FontSize = 0.6f;
                autoCraft.TextPadding = 0;
                autoCraft.Font = "Monospace";
                autoCraft.FontColor = commonColor;
            }

            if (autoCraft != null)
            {
                if (!lcd[0].CustomData.Contains(msgCustomData) || !autoCraft.GetText().Contains(msgTextSurface))
                {
                    lcd[0].CustomData = msgCustomData;
                    autoCraft.WriteText(msgTextSurface, false);
                    for (int i = 0; i < listComponent.Length; i++)
                    {
                        string tmpString = listComponent[i].Substring(10);
                        string listNoBPItems = "ExplosivesZoneChipEnjector_milEnjector_pirTritiumAlloy1TritiumAlloy2TritiumAlloy3Accelerometer1Accelerometer2Accelerometer3" +
                            "structure_nanofiberstructure_compositestructure_titanthermal_energy_storagecentrifuge_superstrongcentrifuge_improvedservo_heavy_dutyservo_frictionless" +
                            "servo_improvedquantum_emitterMagnetronComponentmoney_lowmoney_highAdminComponentferrotitaniumnanofibercompositesdeuterium_catalyst" +
                            "DeuteriumNullCoreDeuteriumCapacitorstrongGridmotor_heavy_dutymotor_frictionlessmotor_improveddampergeneratorstrongPlatecontroller" +
                            "stabilizeroptical_fibercontrol_unitPowerCell_pirPowerCell_engPowerCell_milbiomaterialenergy_coreion_generator_improved_small" +
                            "ion_generator_improved_largeion_generator_heavy_duty_smallion_generator_heavy_duty_largesupercharger_improved_small" +
                            "supercharger_improved_largesupercharger_heavy_duty_smallsupercharger_heavy_duty_largeturbine_heavy_duty_smallturbine_heavy_duty_large" +
                            "turbine_improved_smallturbine_improved_largemodule_thrust";
                        if (!listNoBPItems.Contains(tmpString))
                        {
                            if (listComponent[i].Substring(10).Length < lengthString)
                                for (int j = 0; j < (lengthString - listComponent[i].Substring(10).Length); j++)
                                    tmpString += ".";
                            else tmpString = tmpString.Substring(0, lengthString);
                            autoCraft.WriteText("+" + tmpString + ".[" + totalComponent[i] + "] " + totalComponent[i] + "\n", true);
                        }
                        else
                        {
                            lcd[0].CustomData += tmpString + "\n";
                        }
                    }
                }
                else
                {
                    string resultString = autoCraft.GetText();
                    string reservString = resultString;

                    //=- Чистим текстовую панель
                    string[] tmpStrSurf = resultString.Trim('\n').Split('\n');
                    for (int i = 1; i < tmpStrSurf.Length; i++)
                    {
                        if (!tmpStrSurf[i].Contains("+"))
                        {
                            lcd[0].CustomData += tmpStrSurf[i].Substring(0, tmpStrSurf[i].IndexOf('.')) + "\n";
                            tmpStrSurf[i] = "";

                            string toWriteText = "";
                            for (int k = 0; k < tmpStrSurf.Length; k++)
                                if (tmpStrSurf[k].Length != 0)
                                    toWriteText += tmpStrSurf[k] + "\n";
                            resultString = toWriteText;

                        }
                        else
                        {
                            //string nameItem = tmpStrSurf[i].Substring(tmpStrSurf[i].IndexOf("+") + 1, tmpStrSurf[i].IndexOf('.') - tmpStrSurf[i].IndexOf("+") - 1);
                            /*
                            for (int j = 0; j < listComponent.Length; j++)
                            {
                                if (listComponent[j].Substring(10) == nameItem)
                                {
                                    tmpStrSurf[i] = tmpStrSurf[i].Substring(0, tmpStrSurf[i].IndexOf(']') + 2) + (int)Math.Round(totalComponent[j], 0);
                                    break;
                                }
                            }
                            */
                        }
                    }

                    reservString = resultString;

                    for (int j = 0; j < listComponent.Length; j++)
                    {
                        reservString = resultString;
                        if (resultString.Contains(listComponent[j].Substring(10)))
                        {
                            
                            string fullString = resultString;
                            resultString = resultString.Substring(0, resultString.IndexOf(listComponent[j].Substring(10)));
                            string tempString = fullString.Substring(fullString.IndexOf(listComponent[j].Substring(10)));
                            if (listComponent[j].Substring(10) == tempString.Substring(0, tempString.IndexOf(".")))
                            {
                                //LCDMain.WriteText("3-" + j + listComponent[j].Substring(10), false);
                                resultString += tempString.Substring(0, tempString.IndexOf("]") + 2) + totalComponent[j];
                                tempString = tempString.Substring(tempString.IndexOf("\n"));
                                resultString += tempString;
                                //tmpStrSurf[i] = tmpStrSurf[i].Substring(0, tmpStrSurf[i].IndexOf(']') + 2) + (int)Math.Round(totalComponent[j], 0);
                                //break;
                            }
                            else
                            {
                                
                                resultString = reservString;
                            }
                        }
                    }
                    






                    //=- Чистим свои данные
                    string[] tmpStrData = lcd[0].CustomData.Trim('\n').Split('\n');
                    for (int i = 1; i < tmpStrData.Length; i++)
                    {
                        if (tmpStrData[i].Contains("+"))
                        {
                            int? indexCurComp = null;
                            for (int j = 0; j < listComponent.Length; j++)
                            {
                                if (tmpStrData[i].Substring(1) == listComponent[j].Substring(10))
                                {
                                    indexCurComp = j;
                                    break;
                                }
                            }
                            if (indexCurComp.HasValue)
                            {
                                string tmpString = listComponent[indexCurComp.Value].Substring(10);
                                if (listComponent[indexCurComp.Value].Substring(10).Length < lengthString)
                                    for (int j = 0; j < (lengthString - listComponent[indexCurComp.Value].Substring(10).Length); j++)
                                        tmpString += ".";
                                else tmpString = tmpString.Substring(0, lengthString);
                                resultString += "+" + tmpString + ".[" + totalComponent[indexCurComp.Value] + "] " + totalComponent[indexCurComp.Value] + "\n";
                                //autoCraft.WriteText("+" + tmpString + ".[" + totalComponent[indexCurComp.Value] + "] " + totalComponent[indexCurComp.Value] + "\n", true);
                                tmpStrData[i] = "";
                                lcd[0].CustomData = tmpStrData[0] + "\n";
                                for (int j = 1; j < tmpStrData.Length; j++)
                                    if (tmpStrData[j].Length != 0)
                                        lcd[0].CustomData += tmpStrData[j] + "\n";
                            }
                        }
                    }

                    autoCraft.WriteText(resultString, false);
                }


                    
            }
        }

        void AutoCraft(int index)
        {
            //int lengthString = 22;
            
            
            //GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(lcd, _lcd => _lcd.CustomName.Contains("!autocraft"));
            //if (lcd[0] != null)
            //    autoCraft = lcd[0] as IMyTextSurface;

            if (autoCraft != null)
            {
                
                {
                    if (autoCraft.GetText().Contains("+" + listComponent[index].Substring(10)))
                    {
                        string[] arrToAutocraft = autoCraft.GetText().Trim('\n').Split('\n');
                        for (int i = 1; i < arrToAutocraft.Length; i++)
                        {
                            
                            string nameItem;
                            nameItem = arrToAutocraft[i].Substring(arrToAutocraft[i].IndexOf("+") + 1, arrToAutocraft[i].IndexOf('.') - arrToAutocraft[i].IndexOf("+") - 1);
                            if (listComponent[index].Substring(10) == nameItem)
                            {
                                decimal needCount;
                                decimal.TryParse(arrToAutocraft[i].Substring(arrToAutocraft[i].IndexOf("[") + 1, arrToAutocraft[i].IndexOf(']') - arrToAutocraft[i].IndexOf("[") - 1), out needCount);
                                //decimal totalCount;
                                //decimal.TryParse(arrToAutocraft[i].Substring(arrToAutocraft[i].IndexOf("]") + 2), out totalCount);
                                //string tmpstr = arrToAutocraft[i].Substring(0, arrToAutocraft[i].IndexOf(']') + 2) + totalComponent[index];
                                //arrToAutocraft[i] = "";
                                //arrToAutocraft[i] = tmpstr;
                                
                                if (needCount - (decimal)totalComponent[index] > 0)
                                {
                                    AddQueue(nameItem, needCount - (decimal)totalComponent[index]);

                                }
                                else
                                {
                                    ClearQueue(nameItem);
                                }
                            }
                        }
                        //autoCraft.WriteText("", false);
                        //autoCraft.WriteText(arrToAutocraft[0] + "\n", false);
                        //for (int j = 1; j < arrToAutocraft.Length; j++)
                        //    autoCraft.WriteText(arrToAutocraft[j] + "\n", true);
                    }
                }
            }
        }

        void AddQueue(string nameItem, decimal countItem)
        {
            listAssembler.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(listAssembler, asm => !asm.CustomName.Contains("!hide"));
            int assemblersCanProd = 0;
            //int indexPos = 0;
            bool isFirstUse = true;
            //LCDMain.WriteText("", false);
            if (listAssembler.Count != 0)
            {
                decimal countNowProd = 0;
                int[] indexPos = new int[listAssembler.Count];

                int i = 0;
                foreach (IMyAssembler assembler in listAssembler)
                {
                    
                    List<MyProductionItem> prodItems = new List<MyProductionItem>();
                    assembler.GetQueue(prodItems);
                    int counter = 0;
                    foreach (MyProductionItem prodItem in prodItems)
                    {
                        
                        if ((prodItem.BlueprintId.ToString().Substring(36) == nameItem) || (prodItem.BlueprintId.ToString().Contains(nameItem) && prodItem.BlueprintId.ToString().Contains("Component")))
                        {
                            countNowProd += (decimal)prodItem.Amount;
                            indexPos[i] = counter;
                            //LCDMain.WriteText(prodItem.ItemId.ToString() + "/" + indexPos.ToString() + "\n", true);
                        }
                        counter++;
                    }
                    i++;
                }

                decimal countToAdd = countItem - countNowProd;
                //if (indexPos != 0) indexPos -= 1;
                
                if (countToAdd > 0)
                {
                    foreach (IMyAssembler assembler in listAssembler)
                        if (assembler.CanUseBlueprint(MyDefinitionId.Parse("BlueprintDefinition/" + nameItem)) || assembler.CanUseBlueprint(MyDefinitionId.Parse("BlueprintDefinition/" + nameItem + "Component")))
                            assemblersCanProd++;
                    i = 0;
                    foreach (IMyAssembler assembler in listAssembler)
                    {
                        if (assembler.CanUseBlueprint(MyDefinitionId.Parse("BlueprintDefinition/" + nameItem)))
                        {
                            if (isFirstUse)
                                assembler.InsertQueueItem(indexPos[i], MyDefinitionId.Parse("BlueprintDefinition/" + nameItem), Math.Floor(countToAdd) % assemblersCanProd);
                            if (countToAdd != 1)
                                assembler.InsertQueueItem(indexPos[i], MyDefinitionId.Parse("BlueprintDefinition/" + nameItem), Math.Floor(countToAdd / assemblersCanProd));
                            isFirstUse = false;
                        }
                        else if (assembler.CanUseBlueprint(MyDefinitionId.Parse("BlueprintDefinition/" + nameItem + "Component")))
                        {
                            if (isFirstUse)
                                assembler.InsertQueueItem(indexPos[i], MyDefinitionId.Parse("BlueprintDefinition/" + nameItem + "Component"), Math.Floor(countToAdd) % assemblersCanProd);
                            if (countToAdd != 1)
                                assembler.InsertQueueItem(indexPos[i], MyDefinitionId.Parse("BlueprintDefinition/" + nameItem + "Component"), Math.Floor(countToAdd / assemblersCanProd));
                            isFirstUse = false;
                        }
                        i++;
                    }
                        
                }
                else if (countToAdd < 0) ClearQueue(nameItem);
            }
        }

        void ClearQueue(string nameItem)
        {
            listAssembler.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(listAssembler, asm => !asm.CustomName.Contains("!hide"));
            if (listAssembler.Count != 0)
            {
                foreach (IMyAssembler assembler in listAssembler)
                {
                    List<MyProductionItem> prodItems = new List<MyProductionItem>();
                    assembler.GetQueue(prodItems);
                    for (int i = 0; i < prodItems.Count; i++)
                        if ((prodItems[i].BlueprintId.ToString().Substring(36) == nameItem) || (prodItems[i].BlueprintId.ToString().Contains(nameItem) && prodItems[i].BlueprintId.ToString().Contains("Component")))
                        {
                            assembler.RemoveQueueItem(i, 1000000d);
                            break;
                        }

                }
            }
                
        }
        float totalItemMonitor(string idItem)
        {
            tmpListBlock.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(tmpListBlock, block => (block.IsSameConstructAs(Me)) && (!block.CustomName.Contains("!hide")) && (block.HasInventory)
            && (block.BlockDefinition.ToString().Contains("Refinery") || block.BlockDefinition.ToString().Contains("CargoContainer") || block.CustomName.Contains("!special") ||
            block.BlockDefinition.ToString().Contains("Assembler")));

            float itemCount = 0;

            foreach (IMyTerminalBlock block in tmpListBlock)
            {
                if (block.GetInventory(0).ItemCount != 0)
                {
                        itemCount += (float)block.GetInventory(0).GetItemAmount(MyItemType.Parse(idItem));
                }
                if (block.InventoryCount > 1)
                {
                    if (block.GetInventory(1).ItemCount != 0)
                    {
                            itemCount += (float)block.GetInventory(1).GetItemAmount(MyItemType.Parse(idItem));
                    }
                }
            }

            return itemCount;
        }

        void SortOther(IMyTerminalBlock currentOther)
        {
            itemsToMove.Clear();
            if (currentOther.BlockDefinition.TypeIdString.Contains("Assembler"))
            {
                currentOther.GetInventory(1).GetItems(itemsToMove);
                foreach (MyInventoryItem item in itemsToMove)
                    TransferItemToCargo(currentOther, 1, item);
            }
            else
            {
                currentOther.GetInventory(0).GetItems(itemsToMove);
                foreach (MyInventoryItem item in itemsToMove)
                    TransferItemToCargo(currentOther, 0, item);
            }
            
        }

        void SortSpecial(IMyTerminalBlock currentSpecial)
        {
            //=- check custom data
            string msg = "Что-бы сбросить список - удали эту строку.\n";
            if (!currentSpecial.CustomData.Contains(msg))
            {
                tempListAvailableItem.Clear();
                currentSpecial.GetInventory(0).GetAcceptedItems(tempListAvailableItem);
                currentSpecial.CustomData = msg;
                foreach (MyItemType i in tempListAvailableItem)
                {
                    if (i.TypeId.Contains("Ore"))
                        currentSpecial.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "=0\n";
                }
                foreach (MyItemType i in tempListAvailableItem)
                {
                    if (i.TypeId.Contains("Ingot"))
                        currentSpecial.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "=0\n";
                }
                foreach (MyItemType i in tempListAvailableItem)
                {
                    if (i.TypeId.Contains("Component"))
                        currentSpecial.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "=0\n";
                }
                foreach (MyItemType i in tempListAvailableItem)
                {
                    if (i.TypeId.Contains("AmmoMagazine"))
                        currentSpecial.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "=0\n";
                }
                foreach (MyItemType i in tempListAvailableItem)
                {
                    if (i.TypeId.Contains("PhysicalGunObject"))
                        currentSpecial.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "=0\n";
                }
                foreach (MyItemType i in tempListAvailableItem)
                {
                    if (i.TypeId.Contains("OxygenContainerObject") | i.TypeId.Contains("GasContainerObject"))
                        currentSpecial.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "=0\n";
                }
            }
            else
            {
                string[] tempString = currentSpecial.CustomData.Split('\n');

                //=- Move item from special
                itemsToMove.Clear();
                currentSpecial.GetInventory(0).GetItems(itemsToMove);
                if (itemsToMove.Count() > 0)
                {
                    if (counterItemsInSpecial < itemsToMove.Count-1) counterItemsInSpecial++;
                    else counterItemsInSpecial = 0;
                    if (!currentSpecial.CustomData.Contains(itemsToMove[counterItemsInSpecial].Type.TypeId.Remove(0, 16) + "/" + itemsToMove[counterItemsInSpecial].Type.SubtypeId))
                    {
                        TransferItemToCargo(currentSpecial, 0, itemsToMove[counterItemsInSpecial]);
                    }
                    else
                    {
                        for (int i = 1; i < tempString.Length; i++)
                        {
                            float resParse = 0;
                            if (tempString[i].Contains(itemsToMove[counterItemsInSpecial].Type.TypeId.Remove(0, 16) + "/" + itemsToMove[counterItemsInSpecial].Type.SubtypeId + "="))
                            {
                                float.TryParse(tempString[i].Substring(tempString[i].IndexOf('=') + 1, (tempString[i].Length) - (tempString[i].IndexOf('=') + 1)).Trim(), out resParse);
                                if (currentSpecial.GetInventory(0).GetItemAmount(itemsToMove[counterItemsInSpecial].Type) > (MyFixedPoint)resParse)
                                {
                                    TransferItemToCargo(currentSpecial, 0, itemsToMove[counterItemsInSpecial], (float)currentSpecial.GetInventory(0).GetItemAmount(itemsToMove[counterItemsInSpecial].Type) - resParse);
                                    break;
                                }
                            }
                        }
                    }
                }

                //=- Move item to special
                bool isMoved = false;
                for (int i = 1; i < tempString.Length; i++)
                {
                    float resParse = 0;
                    float.TryParse(tempString[i].Substring(tempString[i].IndexOf('=') + 1, (tempString[i].Length) - (tempString[i].IndexOf('=') + 1)).Trim(), out resParse);
                    if (resParse != 0)
                    {
                        if (currentSpecial.GetInventory(0).GetItemAmount(MyItemType.Parse(tempString[i].Remove(tempString[i].IndexOf('='), tempString[i].Length - tempString[i].IndexOf('=')))) < (MyFixedPoint)resParse)
                        {
                            foreach (IMyCargoContainer cargo in listCargo)
                            {
                                if (cargo.GetInventory(0).FindItem(MyItemType.Parse(tempString[i].Remove(tempString[i].IndexOf('='), tempString[i].Length - tempString[i].IndexOf('=')))) != null)
                                {
                                    //=- download to special
                                    currentSpecial.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem
                                        (MyItemType.Parse(tempString[i].Remove(tempString[i].IndexOf('='), tempString[i].Length - tempString[i].IndexOf('=')))).GetValueOrDefault(),
                                        (MyFixedPoint)resParse - currentSpecial.GetInventory(0).GetItemAmount(MyItemType.Parse(tempString[i].Remove(tempString[i].IndexOf('='), tempString[i].Length - tempString[i].IndexOf('=')))));
                                    isMoved = true;
                                }
                            }
                            if (isMoved) break;
                        } 
                    }
                }
            }
        }

        void SortRefinery(IMyRefinery currentRefinery)
        {
            //=- sorting for Synthesis
            if (currentRefinery.BlockDefinition.ToString().Contains("Refinery/synthesis") & enableSortingSynthesis)
            {
                currentRefinery.UseConveyorSystem = false;
                //=- check custom data
                tempListAvailableItem.Clear();
                currentRefinery.GetInventory(0).GetAcceptedItems(tempListAvailableItem, item => !(item.SubtypeId.Contains("Deuterium")));
                foreach (MyItemType itemType in tempListAvailableItem)
                {
                    if (!currentRefinery.CustomData.Contains(itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId))
                    {
                        currentRefinery.CustomData = "Что-бы не варить убери знак \"+\".\n";
                        foreach (MyItemType i in tempListAvailableItem)
                            currentRefinery.CustomData += "+" + i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "\n";
                    }
                }

                string[] tempString = currentRefinery.CustomData.Split('\n');
                bool isNeedToBlast = false;
                for (int i = 1; i < tempString.Length; i++)
                {
                    if (tempString[i].Contains("+Ingot/"))
                    {
                        isNeedToBlast = true;
                        //=- upload ore to refinery
                        foreach (IMyCargoContainer cargo in listCargo)
                        {
                            if (cargo.GetInventory(0).FindItem(MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) != null)
                            {
                                /*
                                itemsToMove.Clear();
                                currentRefinery.GetInventory(0).GetItems(itemsToMove, item =>
                                (!item.Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) & (!item.Type.SubtypeId.Contains("Deuterium")));
                                foreach (MyInventoryItem item in itemsToMove)
                                    TransferItemToCargo(currentRefinery, 0, item);
                                */

                                if ((tempString[i].Contains("+Ingot/Thorium")) &
                                    (!currentRefinery.GetInventory(0).ContainItems(80, MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))))
                                    currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                        (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 20);
                                if ((tempString[i].Contains("+Ingot/Uranium")) &
                                    (!currentRefinery.GetInventory(0).ContainItems(15000, MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))))
                                    currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                        (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 2000);
                                if ((tempString[i].Contains("+Ingot/Magnesium")) &
                                    (!currentRefinery.GetInventory(0).ContainItems(15000, MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))))
                                    currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                        (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 2000);
                            }

                            if ((cargo.GetInventory(0).FindItem(MyItemType.MakeIngot("Deuterium")) != null) &
                                (!currentRefinery.GetInventory(0).ContainItems(10000, MyItemType.MakeIngot("Deuterium"))) &
                                (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))))
                            {
                                currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                    ("Deuterium")).GetValueOrDefault(), 2000);
                            }
                        }
                        if (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))
                            break;
                    }
                }
                //=- clear inventory 0 if not ore to blast
                if (!isNeedToBlast)
                {
                    itemsToMove.Clear();
                    currentRefinery.GetInventory(0).GetItems(itemsToMove);
                    foreach (MyInventoryItem item in itemsToMove)
                        TransferItemToCargo(currentRefinery, 0, item);

                }
                //=- clear inventory 1
                itemsToMove.Clear();
                currentRefinery.GetInventory(1).GetItems(itemsToMove);
                foreach (MyInventoryItem item in itemsToMove)
                    TransferItemToCargo(currentRefinery, 1, item);

            }


            //=- sorting for DP
            if (currentRefinery.BlockDefinition.ToString().Contains("Refinery/DeuteriumProcessor") & enableSortingDP)
            {
                //=- check deuterium position
                currentRefinery.UseConveyorSystem = false;
                if (currentRefinery.GetInventory(0).GetItemAt(0).HasValue)
                    if (currentRefinery.GetInventory(0).GetItemAt(0).Value.Type.SubtypeId.Contains("Deuterium") &
                        currentRefinery.GetInventory(0).GetItemAt(0).Value.Type.TypeId.Contains("Ingot"))
                        TransferItemToCargo(currentRefinery, 0, currentRefinery.GetInventory(0).GetItemAt(0).Value);

                //=- check custom data
                tempListAvailableItem.Clear();
                currentRefinery.GetInventory(0).GetAcceptedItems(tempListAvailableItem, item => !(item.TypeId.Contains("Ingot") & item.SubtypeId.Contains("Deuterium")));
                foreach (MyItemType itemType in tempListAvailableItem)
                {
                    if (!currentRefinery.CustomData.Contains(itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId))
                    {
                        currentRefinery.CustomData = "Что-бы не варить убери знак \"+\". Порядок строк соотв. приоритетности.\n";
                        foreach (MyItemType i in tempListAvailableItem)
                            if (!i.SubtypeId.Contains("Gold") & !i.SubtypeId.Contains("Platinum") & !i.SubtypeId.Contains("Silver"))
                                currentRefinery.CustomData += "+" + i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "\n";
                            else
                                currentRefinery.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "\n";
                    }
                }

                string[] tempString = currentRefinery.CustomData.Split('\n');
                bool isNeedToBlast = false;
                for (int i = 1; i < tempString.Length; i++)
                {
                    if (tempString[i].Contains("+Ingot/") | tempString[i].Contains("+Ore/"))
                    {
                        isNeedToBlast = true;
                        //=- upload ore to refinery

                        if (tempString[i].Contains("+Ingot/"))
                        {
                            foreach (IMyCargoContainer cargo in listCargo)
                            {
                                if (cargo.GetInventory(0).FindItem(MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) != null)
                                {
                                    //=- download from refinery
                                    itemsToMove.Clear();
                                    currentRefinery.GetInventory(0).GetItems(itemsToMove, item =>
                                    (!item.Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) &
                                    !(item.Type.TypeId.Contains("Ingot") & item.Type.SubtypeId.Contains("Deuterium")));
                                    foreach (MyInventoryItem item in itemsToMove)
                                        TransferItemToCargo(currentRefinery, 0, item);

                                    //=- upload to refinery
                                    if ((tempString[i].Contains("+Ingot/Gold")) & (!currentRefinery.GetInventory(0).ContainItems(5000, MyItemType.MakeIngot("Gold"))))
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 500);
                                    if ((tempString[i].Contains("+Ingot/Platinum")) & (!currentRefinery.GetInventory(0).ContainItems(5000, MyItemType.MakeIngot("Platinum"))))
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 500);
                                    if ((tempString[i].Contains("+Ingot/Silver")) & (!currentRefinery.GetInventory(0).ContainItems(5000, MyItemType.MakeIngot("Silver"))))
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 500);
                                }
                                if ((cargo.GetInventory(0).FindItem(MyItemType.MakeIngot("Deuterium")) != null) &
                                    (!currentRefinery.GetInventory(0).ContainItems(3000, MyItemType.MakeIngot("Deuterium"))) &
                                    (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))))
                                {
                                    currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                        ("Deuterium")).GetValueOrDefault(), 500);
                                }
                            }
                        }
                        else
                        {
                            foreach (IMyCargoContainer cargo in listCargo)
                            {
                                if (cargo.GetInventory(0).FindItem(MyItemType.MakeOre(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) != null)
                                {
                                    //=- download from refinery
                                    itemsToMove.Clear();
                                    currentRefinery.GetInventory(0).GetItems(itemsToMove, item =>
                                    !item.Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)));
                                    foreach (MyInventoryItem item in itemsToMove)
                                        TransferItemToCargo(currentRefinery, 0, item);

                                    //=- upload to refinery
                                    currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem
                                        (MyItemType.MakeOre(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault());
                                }
                            }
                        }

                        if (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))
                            break;
                    }
                }

                //=- clear inventory 0 if not ore to blast
                if (!isNeedToBlast)
                {
                    itemsToMove.Clear();
                    currentRefinery.GetInventory(0).GetItems(itemsToMove);
                    foreach (MyInventoryItem item in itemsToMove)
                        TransferItemToCargo(currentRefinery, 0, item);
                }

                //=- clear inventory 1
                itemsToMove.Clear();
                currentRefinery.GetInventory(1).GetItems(itemsToMove);
                foreach (MyInventoryItem item in itemsToMove)
                    TransferItemToCargo(currentRefinery, 1, item);
            }


            //=- sorting for medium and high refs
            if (currentRefinery.BlockDefinition.ToString().Contains("Refinery/Refinery_Module_L1") | currentRefinery.BlockDefinition.ToString().Contains("Refinery/Refinery_Module_L2"))
            {
                if (!currentRefinery.CustomName.Contains("!auto"))
                {
                    //=- check catalitic position
                    currentRefinery.UseConveyorSystem = false;
                    if (currentRefinery.GetInventory(0).GetItemAt(0).HasValue)
                        if (currentRefinery.GetInventory(0).GetItemAt(0).Value.Type.SubtypeId.Contains("catalytic"))
                            TransferItemToCargo(currentRefinery, 0, currentRefinery.GetInventory(0).GetItemAt(0).Value);

                    //=- check custom data
                    tempListAvailableItem.Clear();
                    currentRefinery.GetInventory(0).GetAcceptedItems(tempListAvailableItem, item => !item.SubtypeId.Contains("catalyticGold") & !item.SubtypeId.Contains("catalyticPlat"));
                    foreach (MyItemType itemType in tempListAvailableItem)
                    {
                        if (!currentRefinery.CustomData.Contains(itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId))
                        {
                            currentRefinery.CustomData = "Что-бы не варить убери знак \"+\". Порядок строк соотв. приоритетности.\n";
                            foreach (MyItemType i in tempListAvailableItem)
                                currentRefinery.CustomData += "+" + i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "\n";
                        }
                    }

                    string[] tempString = currentRefinery.CustomData.Split('\n');
                    bool isNeedToBlast = false;
                    for (int i = 1; i < tempString.Length; i++)
                    {
                        if (tempString[i].Contains("+Ingot/"))
                        {
                            isNeedToBlast = true;
                            //=- upload ore to refinery
                            foreach (IMyCargoContainer cargo in listCargo)
                            {
                                if (cargo.GetInventory(0).FindItem(MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) != null)
                                {
                                    itemsToMove.Clear();
                                    currentRefinery.GetInventory(0).GetItems(itemsToMove, item =>
                                    (!item.Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) &
                                    (!item.Type.SubtypeId.Contains("catalyticGold")) & (!item.Type.SubtypeId.Contains("catalyticPlat")));
                                    foreach (MyInventoryItem item in itemsToMove)
                                        TransferItemToCargo(currentRefinery, 0, item);

                                    if ((tempString[i].Contains("+Ingot/Duranium")) &
                                        (!currentRefinery.GetInventory(0).ContainItems(6000, MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))))
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 1000);
                                    if ((tempString[i].Contains("+Ingot/Tritium")) &
                                        (!currentRefinery.GetInventory(0).ContainItems(3750, MyItemType.MakeIngot(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))))
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 500);
                                }
                                if (currentRefinery.BlockDefinition.ToString().Contains("Refinery/Refinery_Module_L1"))
                                {
                                    if ((cargo.GetInventory(0).FindItem(MyItemType.MakeIngot("catalyticPlat")) != null) &
                                    (!currentRefinery.GetInventory(0).ContainItems(3750, MyItemType.MakeIngot("catalyticPlat"))) &
                                    (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))))
                                    {
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            ("catalyticPlat")).GetValueOrDefault(), 500);
                                    }
                                }
                                else
                                {
                                    if ((cargo.GetInventory(0).FindItem(MyItemType.MakeIngot("catalyticGold")) != null) &
                                    (!currentRefinery.GetInventory(0).ContainItems(3750, MyItemType.MakeIngot("catalyticGold"))) &
                                    (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))))
                                    {
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            ("catalyticGold")).GetValueOrDefault(), 500);
                                    }
                                }
                            }
                            if (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)))
                                break;
                        }
                    }
                    //=- clear inventory 0 if not ore to blast
                    if (!isNeedToBlast)
                    {
                        itemsToMove.Clear();
                        currentRefinery.GetInventory(0).GetItems(itemsToMove);
                        foreach (MyInventoryItem item in itemsToMove)
                            TransferItemToCargo(currentRefinery, 0, item);

                    }
                    //=- clear inventory 1
                    itemsToMove.Clear();
                    currentRefinery.GetInventory(1).GetItems(itemsToMove);
                    foreach (MyInventoryItem item in itemsToMove)
                        TransferItemToCargo(currentRefinery, 1, item);

                }
            }
            //=- sorting for common refinery
            if (collectCommonRefinery.Contains(currentRefinery.BlockDefinition.ToString().Remove(0, 16)))
            {
                if (!currentRefinery.CustomName.Contains("!auto"))
                {
                    //=- check catalitic position
                    currentRefinery.UseConveyorSystem = false;
                    if (currentRefinery.GetInventory(0).GetItemAt(0).HasValue)
                        if (currentRefinery.GetInventory(0).GetItemAt(0).Value.Type.SubtypeId.Contains("catalytic"))
                            TransferItemToCargo(currentRefinery, 0, currentRefinery.GetInventory(0).GetItemAt(0).Value);

                    //=- check custom data
                    tempListAvailableItem.Clear();
                    currentRefinery.GetInventory(0).GetAcceptedItems(tempListAvailableItem, item => !item.TypeId.Contains("Ingot"));
                    foreach (MyItemType itemType in tempListAvailableItem)
                    {
                        if (!currentRefinery.CustomData.Contains(itemType.TypeId.Remove(0,16) + "/" + itemType.SubtypeId))
                        {
                            currentRefinery.CustomData = "Что-бы не варить убери знак \"+\". Порядок строк соотв. приоритетности.\n";
                            foreach (MyItemType i in tempListAvailableItem)
                                if (!i.SubtypeId.Contains("Stone"))
                                    currentRefinery.CustomData += "+" + i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "\n";
                                else
                                    currentRefinery.CustomData += i.TypeId.Remove(0, 16) + "/" + i.SubtypeId + "\n";

                        }
                    }
                    //=- sorting ores
                    string[] tempString = currentRefinery.CustomData.Split('\n');
                    bool isNeedToBlast = false;
                    for (int i = 1; i < tempString.Length; i++)
                    {
                        if (tempString[i].Contains("+Ore/"))
                        {
                            isNeedToBlast = true;
                            //=- upload ore to refinery
                            if (tempString[i].Contains("+Ore/Duranium") | tempString[i].Contains("+Ore/Tritium"))
                            {
                                foreach (IMyCargoContainer cargo in listCargo)
                                {
                                    if (cargo.GetInventory(0).FindItem(MyItemType.MakeOre(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) != null)
                                    {
                                        //=- download from refinery
                                        itemsToMove.Clear();
                                        currentRefinery.GetInventory(0).GetItems(itemsToMove, item =>
                                        (!item.Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) &
                                        (!item.Type.SubtypeId.Contains("catalyticSilv")));
                                        foreach (MyInventoryItem item in itemsToMove)
                                            TransferItemToCargo(currentRefinery, 0, item);

                                        //=- upload to refinery
                                        if ((tempString[i].Contains("+Ore/Duranium")) & (!currentRefinery.GetInventory(0).ContainItems(50000, MyItemType.MakeOre("Duranium"))))
                                            currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeOre
                                                (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 5000);
                                        if ((tempString[i].Contains("+Ore/Tritium")) & (!currentRefinery.GetInventory(0).ContainItems(13000, MyItemType.MakeOre("Tritium"))))
                                            currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeOre
                                                (tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault(), 1000);
                                    }
                                    if ((cargo.GetInventory(0).FindItem(MyItemType.MakeIngot("catalyticSilv")) != null) &
                                        (!currentRefinery.GetInventory(0).ContainItems(15000, MyItemType.MakeIngot("catalyticSilv"))) & 
                                        (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))))
                                    {
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem(MyItemType.MakeIngot
                                            ("catalyticSilv")).GetValueOrDefault(), 1000);
                                    }
                                }
                            }
                            else
                            {
                                foreach (IMyCargoContainer cargo in listCargo)
                                {
                                    if (cargo.GetInventory(0).FindItem(MyItemType.MakeOre(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) != null)
                                    {
                                        //=- download from refinery
                                        itemsToMove.Clear();
                                        currentRefinery.GetInventory(0).GetItems(itemsToMove, item =>
                                        !item.Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1)));
                                        foreach (MyInventoryItem item in itemsToMove)
                                            TransferItemToCargo(currentRefinery, 0, item);

                                        //=- upload to refinery
                                        currentRefinery.GetInventory(0).TransferItemFrom(cargo.GetInventory(0), cargo.GetInventory(0).FindItem
                                            (MyItemType.MakeOre(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))).GetValueOrDefault());
                                    }
                                }
                            }
                            if (currentRefinery.GetInventory(0).GetItemAt(0).GetValueOrDefault().Type.SubtypeId.Contains(tempString[i].Remove(0, tempString[i].IndexOf('/') + 1))) 
                                break;
                        }
                    }
                    //=- clear inventory 0 if not ore to blast
                    if (!isNeedToBlast)
                    {
                        itemsToMove.Clear();
                        currentRefinery.GetInventory(0).GetItems(itemsToMove);
                        foreach (MyInventoryItem item in itemsToMove)
                            TransferItemToCargo(currentRefinery, 0, item);
                    }

                    //=- clear inventory 1
                    itemsToMove.Clear();
                    currentRefinery.GetInventory(1).GetItems(itemsToMove);
                    foreach (MyInventoryItem item in itemsToMove)
                        TransferItemToCargo(currentRefinery, 1, item);
                }
            }
        }
        
        void TransferItemToCargo(IMyTerminalBlock block, int inventoryIndex, MyInventoryItem item, float _amount = 0)
        {
            foreach (IMyCargoContainer cargo in listCargo)
            {
                if (cargo.CustomData.Contains("+" + item.Type.TypeId.Remove(0, 16) + "/" + item.Type.SubtypeId + "\n") |
                    (!cargo.CustomName.Contains("!ore") & !cargo.CustomName.Contains("!ingot") & !cargo.CustomName.Contains("!component") &
                    !cargo.CustomName.Contains("!ammo") & !cargo.CustomName.Contains("!tool") & !cargo.CustomName.Contains("!bottle")))
                {
                    if (cargo.GetInventory(0).MaxVolume - cargo.GetInventory(0).CurrentVolume > 1)
                    {
                        if (_amount == 0f)
                        {
                            block.GetInventory(inventoryIndex).TransferItemTo(cargo.GetInventory(0), item);
                            break;
                        }
                        else
                        {
                            MyFixedPoint amount = (MyFixedPoint)_amount;
                            block.GetInventory(inventoryIndex).TransferItemTo(cargo.GetInventory(0), item, amount);
                            break;
                        }
                    }
                }
            }

            /*
            if (_amount == 0)
            {
                
            }
            else
            {
                foreach (IMyCargoContainer cargo in listCargo)
                {
                    if (cargo.CustomData.Contains("+" + item.Type.TypeId.Remove(0, 16) + "/" + item.Type.SubtypeId) |
                        (!cargo.CustomName.Contains("!ore") & !cargo.CustomName.Contains("!ingot") & !cargo.CustomName.Contains("!component") &
                        !cargo.CustomName.Contains("!ammo") & !cargo.CustomName.Contains("!tool") & !cargo.CustomName.Contains("!bottle")))
                    {
                        if (cargo.GetInventory(0).MaxVolume - cargo.GetInventory(0).CurrentVolume > 1)
                        {
                            
                        }
                    }
                }

            }
            */
        }

        int SortCargoByPryority(IMyCargoContainer x, IMyCargoContainer y)
        {
            if (!x.CustomName.Contains("!p1") & !x.CustomName.Contains("!p2") & !x.CustomName.Contains("!p3") & !x.CustomName.Contains("!p4") & !x.CustomName.Contains("!p5")
                 & !x.CustomName.Contains("!p6") & !x.CustomName.Contains("!p7") & !x.CustomName.Contains("!p8") & !x.CustomName.Contains("!p9"))
            {
                if (!y.CustomName.Contains("!p1") & !y.CustomName.Contains("!p2") & !y.CustomName.Contains("!p3") & !y.CustomName.Contains("!p4") & !y.CustomName.Contains("!p5")
                 & !y.CustomName.Contains("!p6") & !y.CustomName.Contains("!p7") & !y.CustomName.Contains("!p8") & !y.CustomName.Contains("!p9"))
                {
                    if (!x.CustomName.Contains("!ore") & !x.CustomName.Contains("!ingot") & !x.CustomName.Contains("!component") 
                        & !x.CustomName.Contains("!ammo") & !x.CustomName.Contains("!tool") & !x.CustomName.Contains("!bottle"))
                    {
                        if (!y.CustomName.Contains("!ore") & !y.CustomName.Contains("!ingot") & !y.CustomName.Contains("!component")
                            & !y.CustomName.Contains("!ammo") & !y.CustomName.Contains("!tool") & !y.CustomName.Contains("!bottle"))
                            return 0;
                        else
                            return 1;
                    }
                    else
                    {
                        if (!y.CustomName.Contains("!ore") & !y.CustomName.Contains("!ingot") & !y.CustomName.Contains("!component")
                            & !y.CustomName.Contains("!ammo") & !y.CustomName.Contains("!tool") & !y.CustomName.Contains("!bottle"))
                            return -1;
                        else
                            return 0;
                    }
                }
                else
                    return 1;
            }
            else
            {
                if (!y.CustomName.Contains("!p1") & !y.CustomName.Contains("!p2") & !y.CustomName.Contains("!p3") & !y.CustomName.Contains("!p4") & !y.CustomName.Contains("!p5")
                 & !y.CustomName.Contains("!p6") & !y.CustomName.Contains("!p7") & !y.CustomName.Contains("!p8") & !y.CustomName.Contains("!p9"))
                    return -1;
                else
                {
                    int sortRes = x.CustomName.Substring(x.CustomName.IndexOf("!p"), 3).CompareTo(y.CustomName.Substring(y.CustomName.IndexOf("!p"), 3));
                    if (sortRes == 0)
                    {
                        if (!x.CustomName.Contains("!ore") & !x.CustomName.Contains("!ingot") & !x.CustomName.Contains("!component")
                        & !x.CustomName.Contains("!ammo") & !x.CustomName.Contains("!tool") & !x.CustomName.Contains("!bottle"))
                        {
                            if (!y.CustomName.Contains("!ore") & !y.CustomName.Contains("!ingot") & !y.CustomName.Contains("!component")
                                & !y.CustomName.Contains("!ammo") & !y.CustomName.Contains("!tool") & !y.CustomName.Contains("!bottle"))
                                return 0;
                            else
                                return 1;
                        }
                        else
                        {
                            if (!y.CustomName.Contains("!ore") & !y.CustomName.Contains("!ingot") & !y.CustomName.Contains("!component")
                                & !y.CustomName.Contains("!ammo") & !y.CustomName.Contains("!tool") & !y.CustomName.Contains("!bottle"))
                                return -1;
                            else
                                return 0;
                        }
                    }
                    else
                    {
                        return sortRes;
                    }
                } 
                    
            }
        }

        void InitListItems()
        {
            List<IMyCargoContainer> tempList = new List<IMyCargoContainer>();
            List<MyItemType> itemTypes = new List<MyItemType>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(tempList);
            tempList[0].GetInventory(0).GetAcceptedItems(itemTypes);
            string listOreTemp = "", listIngotTemp = "", listComponentTemp = "", listAmmoMagazineTemp = "", listPhysicalGunObjectTemp = "", listBottleTemp = "";
            //LCDMain = GridTerminalSystem.GetBlockWithName("LCDMain") as IMyTextSurface;
            //LCDMain.WriteText("", false);
            foreach (MyItemType itemType in itemTypes)
            {
                
                //LCDMain.WriteText(itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId + "\n", true);

                if (itemType.TypeId.Contains("Ore")) listOreTemp += itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId + "\n";
                if (itemType.TypeId.Contains("Ingot")) listIngotTemp += itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId + "\n";
                if (itemType.TypeId.Contains("Component")) listComponentTemp += itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId + "\n";
                if (itemType.TypeId.Contains("AmmoMagazine")) listAmmoMagazineTemp += itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId + "\n";
                if (itemType.TypeId.Contains("PhysicalGunObject")) listPhysicalGunObjectTemp += itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId + "\n";
                if (itemType.TypeId.Contains("OxygenContainerObject") || (itemType.TypeId.Contains("GasContainerObject"))) listBottleTemp += itemType.TypeId.Remove(0, 16) + "/" + itemType.SubtypeId + "\n";
            }

            listAvalableItems = (listOreTemp + listIngotTemp + listComponentTemp + listAmmoMagazineTemp +
                listPhysicalGunObjectTemp + listBottleTemp).Trim('\n').Split('\n');

            listOre = listOreTemp.Trim('\n').Split('\n');
            totalOre = new float[listOre.Length];
            listIngot = listIngotTemp.Trim('\n').Split('\n');
            totalIngot = new float[listIngot.Length];
            listComponent = listComponentTemp.Trim('\n').Split('\n');
            totalComponent = new float[listComponent.Length];
            listAmmoMagazine = listAmmoMagazineTemp.Trim('\n').Split('\n');
            totalAmmoMagazine = new float[listAmmoMagazine.Length];
            listPhysicalGunObject = listPhysicalGunObjectTemp.Trim('\n').Split('\n');
            totalPhysicalGunObject = new float[listPhysicalGunObject.Length];
            listBottle = listBottleTemp.Trim('\n').Split('\n');
            totalBottle = new float[listBottle.Length];
        }

        void FixDataCargo(IMyCargoContainer currentCargo)
        {
            currentCargo.CustomData = "";
            if (currentCargo.CustomName.Contains("!ore"))
            {
                currentCargo.CustomData += "~-------===(Ore)===-------\n";
                for (int i = 0; i < listOre.Length; i++) currentCargo.CustomData += "+" + listOre[i] + "\n";
            }
            if (currentCargo.CustomName.Contains("!ingot"))
            {
                currentCargo.CustomData += "~-------===(Ingot)===-------\n";
                for (int i = 0; i < listIngot.Length; i++) currentCargo.CustomData += "+" + listIngot[i] + "\n";
            }
            if (currentCargo.CustomName.Contains("!component"))
            {
                currentCargo.CustomData += "~-------===(Component)===-------\n";
                for (int i = 0; i < listComponent.Length; i++) currentCargo.CustomData += "+" + listComponent[i] + "\n";
            }
            if (currentCargo.CustomName.Contains("!ammo"))
            {
                currentCargo.CustomData += "~-------===(AmmoMagazine)===-------\n";
                for (int i = 0; i < listAmmoMagazine.Length; i++) currentCargo.CustomData += "+" + listAmmoMagazine[i] + "\n";
            }
            if (currentCargo.CustomName.Contains("!tool"))
            {
                currentCargo.CustomData += "~-------===(PhysicalGunObject)===-------\n";
                for (int i = 0; i < listPhysicalGunObject.Length; i++) currentCargo.CustomData += "+" + listPhysicalGunObject[i] + "\n";
            }
            if (currentCargo.CustomName.Contains("!bottle"))
            {
                currentCargo.CustomData += "~-------===(OxygenContainerObject) (GasContainerObject)===-------\n";
                for (int i = 0; i < listBottle.Length; i++) currentCargo.CustomData += "+" + listBottle[i] + "\n";
            }

        }

        void RefreshCargo(IMyCargoContainer currentCargo)
        {
            if (!currentCargo.CustomName.Contains("!hide"))
            {
                if (currentCargo.CustomName.Contains("!fix"))
                {
                    FixDataCargo(currentCargo);
                    currentCargo.SetCustomName(currentCargo.CustomName.Replace("!fix", ""));
                }

                EditDataCargo(currentCargo, "!ore", "-------===(Ore)===-------", listOre);
                EditDataCargo(currentCargo, "!ingot", "-------===(Ingot)===-------", listIngot);
                EditDataCargo(currentCargo, "!component", "-------===(Component)===-------", listComponent);
                EditDataCargo(currentCargo, "!ammo", "-------===(AmmoMagazine)===-------", listAmmoMagazine);
                EditDataCargo(currentCargo, "!tool", "-------===(PhysicalGunObject)===-------", listPhysicalGunObject);
                EditDataCargo(currentCargo, "!bottle", "-------===(OxygenContainerObject) (GasContainerObject)===-------", listBottle);

            }

        }

        void EditDataCargo(IMyCargoContainer currentCargo, string typeTag, string nameHeader, string[] list)
        {
            if (currentCargo.CustomName.Contains(typeTag))
            {
                if (!currentCargo.CustomData.Contains(nameHeader))
                {
                    currentCargo.CustomData += "~" + nameHeader + "\n";
                    for (int j = 0; j < list.Length; j++)
                        currentCargo.CustomData += "+" + list[j] + "\n";
                }
                else
                    for (int i = 0; i < list.Length; i++)
                    {
                        if (!currentCargo.CustomData.Contains(list[i]))
                        {
                            string[] tempString;
                            if (currentCargo.CustomData.Contains("~"))
                            {
                                tempString = currentCargo.CustomData.Split('~');
                                currentCargo.CustomData = "";
                                for (int k = 0; k < tempString.Length; k++)
                                {
                                    if (tempString[k].Contains(nameHeader))
                                    {
                                        tempString[k] = nameHeader + "\n";
                                        for (int j = 0; j < list.Length; j++)
                                            tempString[k] += "+" + list[j] + "\n";
                                    }
                                    if (k == 0) currentCargo.CustomData = tempString[k];
                                    else if (tempString[k] != "") currentCargo.CustomData += "~" + tempString[k];
                                }
                            }
                            else FixDataCargo(currentCargo);
                        }
                    }
            }
            else
            {
                string[] tempString;
                if (currentCargo.CustomData.Contains("~"))
                {
                    tempString = currentCargo.CustomData.Split('~');
                    currentCargo.CustomData = "";
                    for (int k = 0; k < tempString.Length; k++)
                    {
                        if (tempString[k].Contains(nameHeader))
                        {
                            tempString[k] = "";
                        }
                        if (k == 0) currentCargo.CustomData = tempString[k];
                        else if (tempString[k] != "") currentCargo.CustomData += "~" + tempString[k];
                    }
                }
                else FixDataCargo(currentCargo);
            }
        }

        void SortCargo(IMyCargoContainer currentCargo)
        {
            RefreshCargo(currentCargo);

            //listCargo[counterSortCargo].GetInventory(0).GetItems(items, item => (item.Type.TypeId.Contains("Ore") & ));

            itemsToMove.Clear();
            currentCargo.GetInventory(0).GetItems(itemsToMove);

            string notTurnItems = "";
            bool isItemMoved = false;
            foreach (MyInventoryItem item in itemsToMove)
            {
                if (!notTurnItems.Contains(item.Type.TypeId.Remove(0, 16) + "/" + item.Type.SubtypeId))
                {
                    foreach (IMyCargoContainer cargo in listCargo)
                    {
                        if (cargo.CustomData.Contains("+" + item.Type.TypeId.Remove(0, 16) + "/" + item.Type.SubtypeId + "\n") |
                            (!cargo.CustomName.Contains("!ore") & !cargo.CustomName.Contains("!ingot") & !cargo.CustomName.Contains("!component") &
                            !cargo.CustomName.Contains("!ammo") & !cargo.CustomName.Contains("!tool") & !cargo.CustomName.Contains("!bottle")))
                        {
                            if (currentCargo.EntityId != cargo.EntityId)
                            {
                                //if (!cargo.GetInventory(0).IsFull)
                                if (cargo.GetInventory(0).MaxVolume - cargo.GetInventory(0).CurrentVolume > 1)
                                {
                                    currentCargo.GetInventory(0).TransferItemTo(cargo.GetInventory(0), item);
                                    isItemMoved = true;
                                    break;
                                }
                            }
                            else break;
                        }
                    }
                    //if (!isItemMoved) notTurnItems += item.Type.TypeId.Remove(0, 16) + "/" + item.Type.SubtypeId; 
                    notTurnItems += item.Type.TypeId.Remove(0, 16) + "/" + item.Type.SubtypeId + "\n";
                    
                }
                
            }
        }
    }
}
