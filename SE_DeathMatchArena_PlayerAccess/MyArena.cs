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
        public TimeSpan timerGamePrepareToOpenDoors = TimeSpan.FromSeconds(300);

        public StateGame currentState;

        public string lastMessage = "";

        public bool isArenaUsed = false;

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

                        if (mainScript.controlRoom.isGameStarted)
                        {
                            currentState = StateGame.Prepare;

                            mainScript.Save();
                        }
                    }
                    break;

                case StateGame.Prepare:
                    {
                        if (!isArenaUsed && !mainScript.controlRoom.isGameStarted)
                        {
                            currentState = StateGame.NotReady;

                            mainScript.Save();
                        }
                        else
                        {
                            foreach (var gateway in mainScript.gateways)
                            {
                                gateway.RefreshState(int.Parse(mainScript.Me.CustomData));
                            }

                            mainScript.controlRoom.RefreshState();

                            currentState = StateGame.Prepare;

                            mainScript.Save();
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
