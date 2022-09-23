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


        public Program()
        {
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var DummyA = (IMyTerminalBlock)GridTerminalSystem.GetBlockWithName("DummyA");
            var LCDTools3 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDTools3");
            string str = DummyA.BlockDefinition.TypeIdString + "\n"
                + DummyA.BlockDefinition.TypeIdStringAttribute + "\n"
                + DummyA.BlockDefinition.SubtypeId + "\n"
                + DummyA.BlockDefinition.SubtypeIdAttribute + "\n"
                + DummyA.BlockDefinition.TypeId + "\n"
                + DummyA.BlockDefinition.SubtypeName + "\n"
                + DummyA.GetType().ToString() + "\n";

            List<ITerminalAction> actions = new List<ITerminalAction>();
            DummyA.GetActions(actions);
            foreach (var action in actions) str += action.Id + "\n";

            List<ITerminalProperty> properties = new List<ITerminalProperty>();
            DummyA.GetProperties(properties);
            foreach (var property in properties) str += property.Id + "\n";

            LCDTools3.WriteText(str);


            IMyCargoContainer CargoSphere1 = (IMyCargoContainer)GridTerminalSystem.GetBlockWithName("CargoSphere1");

            List<MyItemType> items = new List<MyItemType>();

            CargoSphere1.GetInventory(0).GetAcceptedItems(items);

            var LCDTools1 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDTools1");

            LCDTools1.WriteText("");

            foreach (var item in items) LCDTools1.WriteText(item.TypeId + "/" + item.SubtypeId + "\n", true);

            var LCDTools2 = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("LCDTools2");

            var temp = new MyStoreItemDataSimple(MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AutomaticRifleItem"), 3, 7);

            LCDTools2.WriteText(temp.ItemId + " " + temp.Amount);

            /*
            LCD.WriteText("\uE001 ", true);
            LCD.WriteText("\uE002 ", true);
            LCD.WriteText("\uE003 ", true);
            LCD.WriteText("\uE004 ", true);
            LCD.WriteText("\uE005 ", true);
            LCD.WriteText("\uE006 ", true);
            LCD.WriteText("\uE007 ", true);
            LCD.WriteText("\uE008 \n", true);
            LCD.WriteText("\uE009 ", true);
            LCD.WriteText("\uE010 ", true);
            LCD.WriteText("\uE011 ", true);
            LCD.WriteText("\uE012 ", true);
            LCD.WriteText("\uE013 ", true);
            LCD.WriteText("\uE014 ", true);
            LCD.WriteText("\uE015 ", true);
            LCD.WriteText("\uE016 \n", true);
            LCD.WriteText("\uE017 ", true);
            LCD.WriteText("\uE018 ", true);
            LCD.WriteText("\uE019 ", true);
            LCD.WriteText("\uE020 ", true);
            LCD.WriteText("\uE021 ", true);
            LCD.WriteText("\uE022 ", true);
            LCD.WriteText("\uE023 ", true);
            LCD.WriteText("\uE024 \n", true);
            LCD.WriteText("\uE025 ", true);
            LCD.WriteText("\uE026 ", true);
            LCD.WriteText("\uE027 ", true);
            LCD.WriteText("\uE028 ", true);
            LCD.WriteText("\uE029 ", true);
            LCD.WriteText("\uE030 ", true);
            LCD.WriteText("\uE031 ", true);
            LCD.WriteText("\uE032 \n", true);
            LCD.WriteText("\uE033 ", true);
            LCD.WriteText("\uE034 ", true);
            LCD.WriteText("\uE035 ", true);
            LCD.WriteText("\uE036 ", true);
            LCD.WriteText("\uE037 ", true);
            LCD.WriteText("\uE038 ", true);
            LCD.WriteText("\uE039 ", true);
            LCD.WriteText("\uE040 \n", true);
            LCD.WriteText("\uE041 ", true);
            LCD.WriteText("\uE042 ", true);
            LCD.WriteText("\uE043 ", true);
            LCD.WriteText("\uE044 ", true);
            LCD.WriteText("\uE045 ", true);
            LCD.WriteText("\uE046 ", true);
            LCD.WriteText("\uE047 ", true);
            LCD.WriteText("\uE048 \n", true);
            LCD.WriteText("\uE049 ", true);
            LCD.WriteText("\uE050 ", true);
            LCD.WriteText("\uE051 ", true);
            LCD.WriteText("\uE052 ", true);
            LCD.WriteText("\uE053 ", true);
            LCD.WriteText("\uE054 ", true);
            LCD.WriteText("\uE055 ", true);
            LCD.WriteText("\uE056 \n", true);
            LCD.WriteText("\uE057 ", true);
            LCD.WriteText("\uE058 ", true);
            LCD.WriteText("\uE059 ", true);
            LCD.WriteText("\uE060 ", true);
            LCD.WriteText("\uE061 ", true);
            LCD.WriteText("\uE062 ", true);
            LCD.WriteText("\uE063 ", true);
            LCD.WriteText("\uE064 \n", true);
            LCD.WriteText("\uE065 ", true);
            LCD.WriteText("\uE066 ", true);
            LCD.WriteText("\uE067 ", true);
            LCD.WriteText("\uE068 ", true);
            LCD.WriteText("\uE069 ", true);
            LCD.WriteText("\uE070 ", true);
            LCD.WriteText("\uE071 ", true);
            LCD.WriteText("\uE072 \n", true);
            LCD.WriteText("\uE073 ", true);
            LCD.WriteText("\uE074 ", true);
            LCD.WriteText("\uE075 ", true);
            LCD.WriteText("\uE076 ", true);
            LCD.WriteText("\uE077 ", true);
            LCD.WriteText("\uE078 ", true);
            LCD.WriteText("\uE079 ", true);
            LCD.WriteText("\uE080 \n", true);
            LCD.WriteText("\uE081 ", true);
            LCD.WriteText("\uE082 ", true);
            LCD.WriteText("\uE083 ", true);
            LCD.WriteText("\uE084 ", true);
            LCD.WriteText("\uE085 ", true);
            LCD.WriteText("\uE086 ", true);
            LCD.WriteText("\uE087 ", true);
            LCD.WriteText("\uE088 ", true);
            LCD.WriteText("\uE089 ", true);
            LCD.WriteText("\uE090 ", true);
            LCD.WriteText("\uE091 ", true);
            LCD.WriteText("\uE092 ", true);
            LCD.WriteText("\uE093 ", true);
            LCD.WriteText("\uE094 ", true);
            LCD.WriteText("\uE095 ", true);
            LCD.WriteText("\uE096 ", true);
            LCD.WriteText("\uE097 ", true);
            LCD.WriteText("\uE098 ", true);
            LCD.WriteText("\uE099 ", true);
            LCD.WriteText("\uE100 ", true);
            LCD.WriteText("\uE101 ", true);
            LCD.WriteText("\uE102 ", true);
            LCD.WriteText("\uE103 ", true);
            LCD.WriteText("\uE104 ", true);
            LCD.WriteText("\uE105 ", true);
            LCD.WriteText("\uE106 ", true);
            LCD.WriteText("\uE107 ", true);
            LCD.WriteText("\uE108 ", true);
            LCD.WriteText("\uE109 ", true);
            LCD.WriteText("\uE110 ", true);
            LCD.WriteText("\uE111 ", true);
            LCD.WriteText("\uE112 ", true);
            LCD.WriteText("\uE113 ", true);
            LCD.WriteText("\uE114 ", true);
            LCD.WriteText("\uE115 ", true);
            LCD.WriteText("\uE116 ", true);
            LCD.WriteText("\uE117 ", true);
            LCD.WriteText("\uE118 ", true);
            LCD.WriteText("\uE119 ", true);
            LCD.WriteText("\uE120 ", true);
            LCD.WriteText("\uE121 ", true);
            LCD.WriteText("\uE122 ", true);
            LCD.WriteText("\uE123 ", true);
            LCD.WriteText("\uE124 ", true);
            LCD.WriteText("\uE125 ", true);
            LCD.WriteText("\uE126 ", true);
            LCD.WriteText("\uE127 ", true);
            LCD.WriteText("\uE128 ", true);
            LCD.WriteText("\uE129 ", true);
            LCD.WriteText("\uE130 ", true);
            LCD.WriteText("\uE131 ", true);
            LCD.WriteText("\uE132 ", true);
            LCD.WriteText("\uE133 ", true);
            LCD.WriteText("\uE134 ", true);
            LCD.WriteText("\uE135 ", true);
            LCD.WriteText("\uE136 ", true);
            LCD.WriteText("\uE137 ", true);
            LCD.WriteText("\uE138 ", true);
            LCD.WriteText("\uE139 ", true);
            LCD.WriteText("\uE140 ", true);
            LCD.WriteText("\uE141 ", true);
            LCD.WriteText("\uE142 ", true);
            LCD.WriteText("\uE143 ", true);
            LCD.WriteText("\uE144 ", true);
            LCD.WriteText("\uE145 ", true);
            LCD.WriteText("\uE146 ", true);
            LCD.WriteText("\uE147 ", true);
            LCD.WriteText("\uE148 ", true);
            LCD.WriteText("\uE149 ", true);
            LCD.WriteText("\uE150 ", true);
            LCD.WriteText("\uE151 ", true);
            LCD.WriteText("\uE152 ", true);
            LCD.WriteText("\uE153 ", true);
            LCD.WriteText("\uE154 ", true);
            LCD.WriteText("\uE155 ", true);
            LCD.WriteText("\uE156 ", true);
            LCD.WriteText("\uE157 ", true);
            LCD.WriteText("\uE158 ", true);
            LCD.WriteText("\uE159 ", true);
            LCD.WriteText("\uE160 ", true);
            LCD.WriteText("\uE161 ", true);
            LCD.WriteText("\uE162 ", true);
            LCD.WriteText("\uE163 ", true);
            LCD.WriteText("\uE164 ", true);
            LCD.WriteText("\uE165 ", true);
            LCD.WriteText("\uE166 ", true);
            LCD.WriteText("\uE167 ", true);
            LCD.WriteText("\uE168 ", true);
            LCD.WriteText("\uE169 ", true);
            LCD.WriteText("\uE170 ", true);
            LCD.WriteText("\uE171 ", true);
            LCD.WriteText("\uE172 ", true);
            LCD.WriteText("\uE173 ", true);
            LCD.WriteText("\uE174 ", true);
            LCD.WriteText("\uE175 ", true);
            LCD.WriteText("\uE176 ", true);
            LCD.WriteText("\uE177 ", true);
            LCD.WriteText("\uE178 ", true);
            LCD.WriteText("\uE179 ", true);
            LCD.WriteText("\uE180 ", true);
            LCD.WriteText("\uE181 ", true);
            LCD.WriteText("\uE182 ", true);
            LCD.WriteText("\uE183 ", true);
            LCD.WriteText("\uE184 ", true);
            LCD.WriteText("\uE185 ", true);
            LCD.WriteText("\uE186 ", true);
            LCD.WriteText("\uE187 ", true);
            LCD.WriteText("\uE188 ", true);
            LCD.WriteText("\uE189 ", true);
            LCD.WriteText("\uE190 ", true);
            LCD.WriteText("\uE191 ", true);
            LCD.WriteText("\uE192 ", true);
            LCD.WriteText("\uE193 ", true);
            LCD.WriteText("\uE194 ", true);
            LCD.WriteText("\uE195 ", true);
            LCD.WriteText("\uE196 ", true);
            LCD.WriteText("\uE197 ", true);
            LCD.WriteText("\uE198 ", true);
            LCD.WriteText("\uE199 ", true);
            LCD.WriteText("\uE200 ", true);
            LCD.WriteText("\uE201 ", true);
            LCD.WriteText("\uE202 ", true);
            LCD.WriteText("\uE203 ", true);
            LCD.WriteText("\uE204 ", true);
            LCD.WriteText("\uE205 ", true);
            LCD.WriteText("\uE206 ", true);
            LCD.WriteText("\uE207 ", true);
            LCD.WriteText("\uE208 ", true);
            LCD.WriteText("\uE209 ", true);
            LCD.WriteText("\uE210 ", true);
            LCD.WriteText("\uE211 ", true);
            LCD.WriteText("\uE212 ", true);
            LCD.WriteText("\uE213 ", true);
            LCD.WriteText("\uE214 ", true);
            LCD.WriteText("\uE215 ", true);
            LCD.WriteText("\uE216 ", true);
            LCD.WriteText("\uE217 ", true);
            LCD.WriteText("\uE218 ", true);
            LCD.WriteText("\uE219 ", true);
            LCD.WriteText("\uE220 ", true);
            LCD.WriteText("\uE221 ", true);
            LCD.WriteText("\uE222 ", true);
            LCD.WriteText("\uE223 ", true);
            LCD.WriteText("\uE224 ", true);
            LCD.WriteText("\uE225 ", true);
            LCD.WriteText("\uE226 ", true);
            LCD.WriteText("\uE227 ", true);
            LCD.WriteText("\uE228 ", true);
            LCD.WriteText("\uE229 ", true);
            LCD.WriteText("\uE230 ", true);
            LCD.WriteText("\uE231 ", true);
            LCD.WriteText("\uE232 ", true);
            LCD.WriteText("\uE233 ", true);
            LCD.WriteText("\uE234 ", true);
            LCD.WriteText("\uE235 ", true);
            LCD.WriteText("\uE236 ", true);
            LCD.WriteText("\uE237 ", true);
            LCD.WriteText("\uE238 ", true);
            LCD.WriteText("\uE239 ", true);
            LCD.WriteText("\uE240 ", true);
            LCD.WriteText("\uE241 ", true);
            LCD.WriteText("\uE242 ", true);
            LCD.WriteText("\uE243 ", true);
            LCD.WriteText("\uE244 ", true);
            LCD.WriteText("\uE245 ", true);
            LCD.WriteText("\uE246 ", true);
            LCD.WriteText("\uE247 ", true);
            LCD.WriteText("\uE248 ", true);
            LCD.WriteText("\uE249 ", true);
            LCD.WriteText("\uE250 ", true);
            LCD.WriteText("\uE251 ", true);
            LCD.WriteText("\uE252 ", true);
            LCD.WriteText("\uE253 ", true);
            LCD.WriteText("\uE254 ", true);
            LCD.WriteText("\uE255 ", true);
            LCD.WriteText("\uE256 ", true);
            LCD.WriteText("\uE257 ", true);
            LCD.WriteText("\uE258 ", true);
            LCD.WriteText("\uE259 ", true);
            LCD.WriteText("\uE260 ", true);
            LCD.WriteText("\uE261 ", true);
            LCD.WriteText("\uE262 ", true);
            LCD.WriteText("\uE263 ", true);
            LCD.WriteText("\uE264 ", true);
            LCD.WriteText("\uE265 ", true);
            LCD.WriteText("\uE266 ", true);
            LCD.WriteText("\uE267 ", true);
            LCD.WriteText("\uE268 ", true);
            LCD.WriteText("\uE269 ", true);
            LCD.WriteText("\uE270 ", true);
            LCD.WriteText("\uE271 ", true);
            LCD.WriteText("\uE272 ", true);
            LCD.WriteText("\uE273 ", true);
            LCD.WriteText("\uE274 ", true);
            LCD.WriteText("\uE275 ", true);
            LCD.WriteText("\uE276 ", true);
            LCD.WriteText("\uE277 ", true);
            LCD.WriteText("\uE278 ", true);
            LCD.WriteText("\uE279 ", true);
            LCD.WriteText("\uE280 ", true);
            LCD.WriteText("\uE281 ", true);
            LCD.WriteText("\uE282 ", true);
            LCD.WriteText("\uE283 ", true);
            LCD.WriteText("\uE284 ", true);
            LCD.WriteText("\uE285 ", true);
            LCD.WriteText("\uE286 ", true);
            LCD.WriteText("\uE287 ", true);
            LCD.WriteText("\uE288 ", true);
            LCD.WriteText("\uE289 ", true);
            LCD.WriteText("\uE290 ", true);
            LCD.WriteText("\uE291 ", true);
            LCD.WriteText("\uE292 ", true);
            LCD.WriteText("\uE293 ", true);
            LCD.WriteText("\uE294 ", true);
            LCD.WriteText("\uE295 ", true);
            LCD.WriteText("\uE296 ", true);
            LCD.WriteText("\uE297 ", true);
            LCD.WriteText("\uE298 ", true);
            LCD.WriteText("\uE299 ", true);
            LCD.WriteText("\uE300 ", true);
            */

        }
    }
}
