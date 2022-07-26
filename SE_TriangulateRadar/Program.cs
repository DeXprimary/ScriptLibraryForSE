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
using Numpy;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        List<Vector3D> CenterSphere = new List<Vector3D>();
        List<Ship> ShipsInArea = new List<Ship>();
        List<string[]> GridsInAreaOld = new List<string[]>();
        List<string[]> ListPonits = new List<string[]>();
        Vector3D LastShipPoint;
        int AvailableScanPoints = 0;
        bool IsWaiting = false;
        IMyRemoteControl ShipPointCheker;
        IMyTextSurface LCDRadar1, LCDRadar2, LCDRadar3, LCDRadarComplex1, LCDRadarComplex2;
        IMyCockpit CockpitMain;



        public Program()
        {
            string[] StringForInit = new string[5];
            ListPonits.Add(StringForInit);
            ListPonits.Add(StringForInit);
            ListPonits.Add(StringForInit);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            LCDRadar1 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDRadar1");
            LCDRadar2 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDRadar2");
            LCDRadar3 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDRadar3");
            LCDRadarComplex1 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDRadarComplex1");
            LCDRadarComplex2 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDRadarComplex2");
            ShipPointCheker = (IMyRemoteControl)GridTerminalSystem.GetBlockWithName("ShipPointCheker");
            

        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (IsWaiting)
            {
                ListPonits[2] = ListPonits[1];
                ListPonits[1] = ListPonits[0];
                IsWaiting = true;
            }
            
            string[] TempStringFromRadar = LCDRadar1.GetText().Trim().Split('\n');
            LCDRadarComplex1.WriteText(""); 
            LCDRadarComplex2.WriteText("");
            for (int i = 1; i <= TempStringFromRadar.Count() - 1; i++)
            {
                string GridName = TempStringFromRadar[i].Substring(1, TempStringFromRadar[i].IndexOf("> [") - 1);
                string Radius = TempStringFromRadar[i].Substring(TempStringFromRadar[i].IndexOf("> [") + 3, (TempStringFromRadar[i].IndexOf("m] <<")) - (TempStringFromRadar[i].IndexOf("> [") + 3));
                string GridOwner = TempStringFromRadar[i].Substring(TempStringFromRadar[i].IndexOf("m] <<") + 5, (TempStringFromRadar[i].IndexOf(">>")) - (TempStringFromRadar[i].IndexOf("m] <<") + 5));

                if (ShipsInArea.Count != 0)
                {
                    bool IsShipInlist = false;
                    
                    foreach (Ship TempShip in ShipsInArea)
                    {
                        if ((TempShip.NameOwner == GridOwner) & (TempShip.NameShip == GridName))
                        {
                            if ((TempShip.R1 != float.Parse(Radius)))
                            {
                                TempShip.AddVector(float.Parse(Radius), ShipPointCheker.GetPosition());
                            }
                            IsShipInlist = true;
                            if (TempShip.IsHaveResult)
                            {
                                LCDRadarComplex2.WriteText("GPS:" + TempShip.NameOwner + "(" + TempShip.NameShip + "):" + TempShip.Result.X + ":" + TempShip.Result.Y + ":" + TempShip.Result.Z + ":#FF75C9F1:" + "\n", true);
                                LCDRadarComplex2.WriteText(TempShip.V1 + "\n" + TempShip.V2 + "\n" + TempShip.V3 + "\n" + TempShip.T1 + "\n" + TempShip.T2 + "\n", true);
                            }
                                
                            if (TempShip.IsHaveError)
                                Echo("Error");
                            break;
                        }
                    }
                    
                    /*
                    for (int j = 0; j <= ShipsInAreaOld.Count - 1; j++)
                    {
                        if ((ShipsInAreaOld[j].NameOwner == GridOwner) & (ShipsInAreaOld[j].NameShip == GridName))
                        {
                            ShipsInAreaOld[0].R1 = 
                            ShipsInAreaOld[j].R3 = ShipsInAreaOld[j].R2;
                            TempShip.R2 = TempShip.R1;
                            ShipsInAreaOld[j].R1 = float.Parse(Radius);
                            TempShip.V3 = TempShip.V2;
                            TempShip.V2 = TempShip.V1;
                            TempShip.V1 = ShipPointCheker.CenterOfMass;
                            ShipsInAreaOld[j].AddVector(float.Parse(Radius), ShipPointCheker.CenterOfMass);
                            IsShipInlist = true;
                            LCDRadarComplex2.WriteText(ShipsInAreaOld.Count.ToString());
                            break;
                        }
                    }
                    */
                    if (!IsShipInlist)
                    {
                        Ship TempShip = new Ship();
                        TempShip.NameOwner = GridOwner;
                        TempShip.NameShip = GridName;
                        TempShip.R1 = float.Parse(Radius);
                        TempShip.V1 = ShipPointCheker.GetPosition();
                        ShipsInArea.Add(TempShip);
                    }
                }
                else
                {
                    Ship TempShip = new Ship();
                    TempShip.NameOwner = GridOwner;
                    TempShip.NameShip = GridName;
                    TempShip.R1 = float.Parse(Radius);
                    TempShip.V1 = ShipPointCheker.GetPosition();
                    ShipsInArea.Add(TempShip);
                }

                



                //string[] TempStringArr = new string[7];
                //TempStringArr[0] = GridOwner + "(" + GridName + ")";
                //TempStringArr[1] = Radius;
                //TempStringArr[2] = ShipPointCheker.CenterOfMass.ToString();
                //GridsInArea.Add(TempStringArr);

                //LCDRadarComplex1.WriteText(GridsInArea[i][0] + GridsInArea[i][1] + GridsInArea[i][2] + "\n", true);

                //foreach ()

                //float RadiusToGrid = float.Parse();
                //ListGridsInArea.Add
            }


            foreach (Ship TempShip in ShipsInArea)
                LCDRadarComplex1.WriteText(TempShip.NameShip + " " + TempShip.R1.ToString() + " " + TempShip.R2.ToString() + " " + TempShip.R3.ToString() + "\n", true);
            //LCDRadarComplex1.WriteText(TempStringFromRadar[i] + "\n", true);

            LastShipPoint = ShipPointCheker.CenterOfMass;

            /*
            //CockpitMain = (IMyCockpit)GridTerminalSystem.GetBlockWithName("CockpitMain");
            //CenterSphere = CockpitMain.CenterOfMass;


            //IMyBeacon TestRadar;
            //TestRadar.
            IMyTextSurface TestLCD = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("TestLCD");
            //(IMyRadioAntenna)GridTerminalSystem.GetBlockWithName("TestRadar").GetType();
            List<ITerminalProperty> temp = new List<ITerminalProperty>();
            //string TempString = "";
            GridTerminalSystem.GetBlockWithName("TestRadar").GetProperties(temp);
            foreach (ITerminalProperty Act in temp)
            {
            //    TempString += Act.Id + "\n";
            }
            //TestLCD.WriteText(TempString);
            TestLCD.WriteText(GridTerminalSystem.GetBlockWithName("TestRadar").GetPosition().ToString() + "\n" +
                              GridTerminalSystem.GetBlockWithName("TestRadar").Position.ToString() + "\n" +
                              GridTerminalSystem.GetBlockWithName("TestRadar2").GetPosition().ToString() + "\n" +
                              GridTerminalSystem.GetBlockWithName("TestRadar2").Position.ToString() + "\n" +
                              CockpitMain.CenterOfMass.X);
            */
        }


        
        public class Ship
        {
            public string NameOwner;
            public string NameShip;
            public double R1, R2, R3, dx, dy, dz, d, S1, S2, S3, VO, L;
            public Vector3D V1, V2, V3, N12, N13, V, P, O1, T1, T2, Result;
            public bool IsHaveResult = false, IsHaveError = false;
            public void AddVector(double R, Vector3D Vt)
            {
                R3 = R2; 
                R2 = R1;
                R1 = R;
                V3 = V2;
                V2 = V1;
                V1 = Vt;
                if (R3 != 0)
                {
                    N12.X = (V1.X - V2.X);
                    N12.Y = (V1.Y - V2.Y);
                    N12.Z = (V1.Z - V2.Z);
                    N13.X = (V1.X - V3.X);
                    N13.Y = (V1.Y - V3.Y);
                    N13.Z = (V1.Z - V3.Z);

                    V = Vector3D.Cross(N12, N13);

                    S1 = (V1.X * V1.X) + (V1.Y * V1.Y) + (V1.Z * V1.Z) - (R1 * R1);
                    S2 = (V2.X * V2.X) + (V2.Y * V2.Y) + (V2.Z * V2.Z) - (R2 * R2);
                    S3 = (V3.X * V3.X) + (V3.Y * V3.Y) + (V3.Z * V3.Z) - (R3 * R3);

                    O1 = V1;

                    VO = Vector3D.Dot(V, O1);

                    d = ((V1.X - V2.X) * (V1.Y - V3.Y) * (V.Z)) + 
                        ((V1.Y - V2.Y) * (V1.Z - V3.Z) * (V.X)) + 
                        ((V1.Z - V2.Z) * (V1.X - V3.X) * (V.Y)) - 
                        ((V1.Z - V2.Z) * (V1.Y - V3.Y) * (V.X)) - 
                        ((V1.X - V2.X) * (V1.Z - V3.Z) * (V.Y)) - 
                        ((V1.Y - V2.Y) * (V1.X - V3.X) * (V.Z));

                    if (d == 0) IsHaveError = true;
                    else
                    {
                        IsHaveError = false;

                        dx = (((S1 - S2) / 2) * (V1.Y - V3.Y)   * (V.Z)) +
                             ((V1.Y - V2.Y)   * (V1.Z - V3.Z)   * (VO)) +
                             ((V1.Z - V2.Z)   * ((S1 - S3) / 2) * (V.Y)) -
                             ((V1.Z - V2.Z)   * (V1.Y - V3.Y)   * (VO)) -
                             (((S1 - S2) / 2) * (V1.Z - V3.Z)   * (V.Y)) -
                             ((V1.Y - V2.Y)   * ((S1 - S3) / 2) * (V.Z));

                        dy = ((V1.X - V2.X)  * ((S1 - S3) / 2) * (V.Z)) +
                            (((S1 - S2) / 2) * (V1.Z - V3.Z)   * (V.X)) +
                            ((V1.Z - V2.Z)   * (V1.X - V3.X)   * (VO)) -
                            ((V1.Z - V2.Z)   * ((S1 - S3) / 2) * (V.X)) -
                            ((V1.X - V2.X)   * (V1.Z - V3.Z)   * (VO)) -
                            (((S1 - S2) / 2) * (V1.X - V3.X)   * (V.Z));

                        dz = ((V1.X - V2.X)  * (V1.Y - V3.Y)   * (VO)) +
                            ((V1.Y - V2.Y)   * ((S1 - S3) / 2) * (V.X)) +
                            (((S1 - S2) / 2) * (V1.X - V3.X)   * (V.Y)) -
                            (((S1 - S2) / 2) * (V1.Y - V3.Y)   * (V.X)) -
                            ((V1.X - V2.X)   * ((S1 - S3) / 2) * (V.Y)) -
                            ((V1.Y - V2.Y)   * (V1.X - V3.X)   * (VO));

                        P.X = dx / d;
                        P.Y = dy / d;
                        P.Z = dz / d;

                        L = Math.Sqrt(R1 * R1 - Vector3D.DistanceSquared(P, O1));

                        T1 = Vector3D.Add(P, Vector3D.Multiply(Vector3D.Divide(V, V.Length()), L));
                        T2 = Vector3D.Subtract(P, Vector3D.Multiply(Vector3D.Divide(V, V.Length()), L));

                        //if (T1 )

                    } 

                    //x0 = ((V2.X * V2.X) + (R1 * R1) - (R2 * R2)) / (2 * V2.X);
                    //y0 = ((V3.X * V3.X) + (V3.Y * V3.Y) + (R1 * R1) - (R3 * R3) - (2 * V3.X * x0)) / (2 * V3.Y);
                    //z0 = Math.Sqrt((R1 * R1) - (x0 * x0) - (y0 * y0));
                    //Result.X = x0;
                    //Result.Y = y0;
                    //Result.Z = z0;
                    
                    IsHaveResult = true;
                    
                }

            }

        }
        





        //=====================
    }
}
