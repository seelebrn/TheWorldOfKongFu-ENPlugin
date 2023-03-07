using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cadenza_sPlugin
{
    internal class Dump
    {

        public static void GenerateUIT()
        {
            var log = Main.log;
            var manager = new AssetsManager();
            manager.MonoTempGenerator = new MonoCecilTempGenerator(BepInEx.Paths.ManagedPath);
            manager.LoadClassPackage(Path.Combine(BepInEx.Paths.PluginPath, "classdata.tpk"));

            Main.log.LogInfo("Path : " + Path.Combine(BepInEx.Paths.GameRootPath, "TheWorldOfKongFu_Data", "data.unity3d"));

            var bunInst = manager.LoadBundleFile(Path.Combine(BepInEx.Paths.GameRootPath, "TheWorldOfKongFu_Data", "data.unity3d"), false);
            Main.log.LogInfo("IsCompressed ? " + bunInst.file.DataIsCompressed);
            Main.log.LogInfo("Count ? " + bunInst.file.GetAllFileNames().Count);
            for (int i = 0; i < bunInst.file.GetAllFileNames().Count; i++)
            {
                Main.log.LogInfo("Now unpacking bundle n°" + i + "....");
                var afileInst = manager.LoadAssetsFileFromBundle(bunInst, i, true);
                manager.LoadClassDatabaseFromPackage(afileInst.file.Metadata.UnityVersion);
                var afile = afileInst.file;

                foreach (var str in bunInst.file.GetAllFileNames())
                {
                    //Main.log.LogInfo("Package in bundle ! : " + str);
                }
                foreach (var texInfo in afile.GetAssetsOfType(AssetClassID.MonoBehaviour))
                {
                    var bf = manager.GetBaseField(afileInst, texInfo);
                    foreach (var x in bf)
                    {
                        var UIText = new List<string>();
                        //log.LogInfo(x.FieldName + " // " + x.TypeName);

                        if (x.TypeName.Contains("string"))
                        {
                            if(x.AsString != null)
                            {
                                if(Helpers.IsChinese(x.AsString))
                                {

                                    Helpers.AddItemToList(x.AsString, "UIText", Main.translationDict);
                                }
                            }


                        }
                        foreach(var y in x.Children)
                        {
                            if (y.TypeName.Contains("string"))
                            {
                                if (y.AsString != null)
                                {
                                    if (Helpers.IsChinese(y.AsString))
                                    {

                                        Helpers.AddItemToList(y.AsString, "UIText", Main.translationDict);
                                    }
                                }


                            }
                        }


                        //Main.log.LogInfo(bf["m_Name"].AsString.ToString());

                    }
                }
            }
        }
        public static void IterateThroughArray(AssetTypeValueField y, List<string> stringreturns)
        {
            switch (y.TypeName)
            {
                default:
                    try
                    {
                        PopulateArray(y, stringreturns);
                    }
                    catch
                    {

                    }
                    break;
                case "string":
                    if (Helpers.IsChinese(y.AsString))
                    {
                        if (y.AsString != null)
                        {
                            //Main.log.LogInfo("Original String ! :" + y.AsString);
                            string key = Helpers.CustomEscape(y.AsString);
                            if (Main.translationDict.ContainsKey(key))
                            {
                                string s = Main.translationDict[key];
                               

                                //Main.log.LogInfo("Match String    ! :" + s);
                                y.AsString = Helpers.CustomUnescape(s);

                            }
                            else
                            {
                                Main.log.LogInfo("Added to Untranslated ! :" + Helpers.CustomEscape(y.AsString));
                                //Main.log.LogInfo("IterateThroughArray : " + y.AsString);
                                stringreturns.Add(Helpers.CustomEscape(y.AsString));
                            }
                        }
                    }
                    if (y.AsString.Contains("\\u"))
                    {
                        Main.log.LogInfo($"Warning, Encoded string in {y.FieldName}, type {y.TypeName} : " + y.AsString);
                        Main.log.LogInfo("Decoded string for debugging purposes : " + Regex.Unescape(y.AsString));
                    }
                    break;
                case "Array":
                    PopulateArray(y, stringreturns);
                    break;


            }
        }
        public static void PopulateArray(AssetTypeValueField y, List<string> stringreturns)
        {
            try
            {
                foreach (var subvalue in y)
                {
                    IterateThroughArray(subvalue, stringreturns);
                }
            }
            catch (Exception ex)
            {
                Main.log.LogError(ex);
            }
        }

    }
}
