﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx;
using System.IO;
using BepInEx.Logging;
using UnityEngine;
using TMPro;
using static UnityEngine.UI.CanvasScaler;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Reflection.Emit;
using UnityEngine.Experimental.PlayerLoop;

namespace Cadenza_sPlugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public static ManualLogSource log = new ManualLogSource("EN"); // The source name is shown in BepInEx log
        public const string pluginGuid = "Cadenza.IWOL.EnMod";
        public const string pluginName = "ENMod Continued";
        public const string pluginVersion = "0.5";
        public static string sourceDir = BepInEx.Paths.PluginPath;
        static public Dictionary<string, string> translationDict = new Dictionary<string, string>();
        static public Dictionary<string, string> TADict = new Dictionary<string, string>();
        static public Dictionary<string, string> HardcodedDict = new Dictionary<string, string>();
        private void Awake()
        {
            Cleaning.Clean();
            translationDict = Helpers.FileToDictionary("UIText.txt");
            TADict = Helpers.FileToDictionary("TA.txt");
            HardcodedDict = Helpers.FileToDictionary("Hardcoded.txt");
            log = Logger;
            //Dump.GenerateUIT();
            log.LogInfo("Running Harmony Patches...");
            var harmony = new Harmony("Cadenza.GAME.ENMOD");
            Harmony.CreateAndPatchAll(System.Reflection.Assembly.GetExecutingAssembly(), null);
        }
        private void Update()
        {

            
        }
    }
    [HarmonyPatch(typeof(CsvParser2), "Parse", new Type[] {typeof(string)})]
    static class Patch00
    {
        static void Postfix(string[][] __result)
        {
            if(__result != null)
            { 
            for(int i = 0; i < __result.Length; i++)
            {
                    for (int j = 0; j < __result[i].Length; j++)
                    {
                        var value = __result[i][j];
                        if(value != null)
                        { 
                            if(Helpers.IsChinese(value) || value.Contains("\\u"))
                            { 
                __result[i][j] = Helpers.AddItemToList(__result[i][j], "TA", Main.TADict);
                            }
                        }
                    }
            }
            }

        }
    }



    [HarmonyPatch(typeof(UnityEngine.UI.Text), "text", MethodType.Getter)]
    static class Patch01
    {

        static void Postfix(UnityEngine.UI.Text __instance, ref string __result)
        {
            if(Main.translationDict.ContainsKey(Helpers.CustomEscape(__result)))
            { 
            __result = Helpers.CustomUnescape(Main.translationDict[Helpers.CustomEscape(__result)]);
            }

        }
    }

    [HarmonyPatch]

    static class BattleControler_patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            //Battle UI
            yield return AccessTools.Method(typeof(BattleController), "UpdateBuffs");
            yield return AccessTools.Method(typeof(BattleController), "UpdateCharacterInfo");
            yield return AccessTools.Method(typeof(BattleController), "GetBattleObjectAfter");
            yield return AccessTools.Method(typeof(BattleController), "FlowStatus");
            yield return AccessTools.Method(typeof(BattleController), "DisplayWuGongInfo");
            yield return AccessTools.Method(typeof(BattleController), "SkillUI");
            yield return AccessTools.Method(typeof(BattleObject), "Attack");
            yield return AccessTools.Method(typeof(BattleObject), "Update");
            yield return AccessTools.Method(typeof(BattleObject), "DisplayDamageInfo");
            yield return AccessTools.Method(typeof(BattleObject), "EffectBeat");
            yield return AccessTools.Method(typeof(BattleObject), "LetUsRevenge");
            yield return AccessTools.Method(typeof(BattleObject), "RunItemSkill");
            yield return AccessTools.Method(typeof(BattleObject), "RunSkill");
            yield return AccessTools.Method(typeof(BattleObject), "RunTraitOne");
            yield return AccessTools.Method(typeof(BattleObject), "UseItemInBattle");

            //Map UI
            yield return AccessTools.Method(typeof(MapController), "DoAutoAction");
            yield return AccessTools.Method(typeof(MapController), "EventAction");
            yield return AccessTools.Method(typeof(MapController), "Explore");
            yield return AccessTools.Method(typeof(MapController), "FixIssues");
            yield return AccessTools.Method(typeof(MapController), "Update");

            //HoverWGController
            yield return AccessTools.Method(typeof(HoverWGController), "ItemHoverSub");
            yield return AccessTools.Method(typeof(HoverWGController), "SetStatus");
            yield return AccessTools.Method(typeof(HoverWGController), "ShowWuGongHover");
            yield return AccessTools.Method(typeof(HoverWGController), "SkillUI");
            //StatusSub
            yield return AccessTools.Method(typeof(StatusSub5), "Start");
            yield return AccessTools.Method(typeof(StatusSub6), "Start");
            yield return AccessTools.Method(typeof(StatusSub7), "RunItemSkillOnField");
            yield return AccessTools.Method(typeof(StatusSub7), "UpdatePackageItemDetail");
            yield return AccessTools.Method(typeof(StatusSub7), "UseSelectedItem");
            //StarterControllers
            yield return AccessTools.Method(typeof(Starter1Controller), "Start");
            yield return AccessTools.Method(typeof(Starter2Controller), "UpdateRightInfo");
            yield return AccessTools.Method(typeof(Starter3Controller), "OnButtonUp");
            yield return AccessTools.Method(typeof(Starter3Controller), "OnButtonUp");
            //StatusControllers
            yield return AccessTools.Method(typeof(Status1Controller), "Start");
            yield return AccessTools.Method(typeof(Status2Controller), "Start");
            yield return AccessTools.Method(typeof(Status2Controller), "UpdateLeftInfo");
            yield return AccessTools.Method(typeof(Status3Controller), "Start");
            yield return AccessTools.Method(typeof(Status4Controller), "Start");
            //Skills
            yield return AccessTools.Method(typeof(KongFuDetailController), "SkillUI");
            yield return AccessTools.Method(typeof(KongFuDetailController), "Start");
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {


                if (codes[i].opcode == OpCodes.Ldstr)
                {
                    if (codes[i].operand != null)
                    {
                        if (Helpers.IsChinese(codes[i].operand.ToString()))
                        {
                            codes[i].operand = Helpers.AddItemToList(codes[i].operand.ToString(), "Hardcoded", Main.HardcodedDict);
                        }
                    }
                }


            }
            return codes.AsEnumerable();


        }
    }

    public static class Helpers
    {
        public static Dictionary<string, string> FileToDictionary(string dir)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            IEnumerable<string> lines = File.ReadLines(Path.Combine(Main.sourceDir, "Translations", dir));

            foreach (string line in lines)
            {

                var arr = line.Split('¤');
                if (arr[0] != arr[1])
                {
                    var pair = new KeyValuePair<string, string>(Regex.Replace(arr[0], @"\t|\n|\r", ""), arr[1]);

                    if (!dict.ContainsKey(pair.Key))
                        dict.Add(pair.Key, pair.Value);
                    else
                        Debug.Log($"Found a duplicated line while parsing {dir}: {pair.Key}");
                }
            }

            return dict;

        }
        public static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        public static bool IsChinese(string s)
        {
            return cjkCharRegex.IsMatch(s);
        }
        public static string CustomEscape(string s)
        {
            return s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }
        public static string CustomUnescape(string s)
        {
            return s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
        }
       
        public static string AddItemToList(string str, string file, Dictionary<string,string> dict)
        {

            var list = new System.Collections.Generic.List<string>();
            var path = Path.Combine(BepInEx.Paths.PluginPath, "Dump", file + ".txt");
            if (File.Exists(path))
            {
                list = File.ReadAllLines(path).ToList();
            }
            else
            {

                list = new System.Collections.Generic.List<string>();
            }
            Main.log.LogInfo("Original string : " + str);
            if (dict.ContainsKey(Helpers.CustomEscape(str)))
            {
                Main.log.LogInfo("Found Match : " + Helpers.CustomUnescape(dict[Helpers.CustomEscape(str)]));
                return Helpers.CustomUnescape(dict[Helpers.CustomEscape(str)]);
            }
            else
            {
                Main.log.LogInfo("Writing to Untranslated... " + Helpers.CustomEscape(str));
                using (StreamWriter sw = new StreamWriter(path, append: true))
                {

                    if (!list.Contains(Helpers.CustomEscape(str)) && str != null)
                    {
                        if (Helpers.IsChinese(str))
                        {

                            sw.Write(Helpers.CustomEscape(str) + System.Environment.NewLine);
                            list.Add(Helpers.CustomEscape(str));

                        }
                    }

                }
                return str;
            }
        }
    }
}
