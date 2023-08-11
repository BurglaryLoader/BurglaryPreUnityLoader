using Burglary;
using Burglary.Addons.Attributes;
using Burglary.Addons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using db = UnityEngine.Debug;
using HarmonyLib;
using System.Runtime.InteropServices;
using ct = Burglary.cons.ConsoleUtils;
using UnityEngine;
using Burglary.Events;
using System.Net;
using System.Threading;
using System.Runtime.CompilerServices;

namespace BurglaryPreUnityLoader
{
    internal class Entrypoint
    {
        public const string VERSION = "v1.0.0";

        internal static void WLColNL(string text, ConsoleColor fg)
        {
            //spaghetti (but formatted!)
            ConsoleColor prevFG = Console.ForegroundColor;

            Console.ForegroundColor = fg;

            Console.WriteLine(text);

            Console.ForegroundColor = prevFG;
        }

        internal static StringWriter writer = new StringWriter();

        internal static string log_loc = Directory.GetCurrentDirectory() + "\\Burglary\\logs\\" + string.Format("{0:yyyyMMdd_HHmmss_fff}_Burglary.log", DateTime.Now);
        private static void write_log()
        {
            //WLColNL(writer.ToString(), ConsoleColor.DarkYellow);
            File.WriteAllText(log_loc, writer.ToString());
        }

        private static void nl()
        {
            Console.WriteLine();
            writer.WriteLine();
            write_log();
        }

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        private static extern int AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        public static void Main()
        {
            string log_location = Directory.GetCurrentDirectory() + "\\Burglary\\logs\\" + string.Format("{0:yyyyMMdd_HHmmss_fff}_Burglary.log", DateTime.Now);
            string logs_dir = Directory.GetCurrentDirectory() + "\\Burglary\\logs";

            try
            {
                if (!Directory.Exists(logs_dir))
                    Directory.CreateDirectory(logs_dir);

                AllocConsole();

                ct.WLColNL(ct.motd_header, ConsoleColor.Blue, writer);

                ct.WLColNL("Version \"" + VERSION + "\"", ConsoleColor.Green, writer);
                //using (WebClient c = new WebClient())
                //{
                //doing wc stuff breaks it??? weird..
                //string latest = c.DownloadString("https://raw.githubusercontent.com/BurglaryLoader/BurglaryHosting/main/current_version.txt");
                //if (VERSION != latest)
                //{
                //    ct.WCol("You are using an ", ConsoleColor.Red, writer);
                //    ct.WCol("OUTDATED", ConsoleColor.DarkRed, writer);
                //    ct.WCol(" version of Burglary. Latest: ", ConsoleColor.Red, writer);
                //    ct.WLColNL(latest, ConsoleColor.Red, writer);
                //}
                //}

                write_log();


                log_loc = log_location;

                BurglaryMain.InitBurglary(new object[] {
                    log_location,
                    writer,
                    Console.Out
                });

                ct.WLCol("Creating and setting property HarmonyInstance...", ConsoleColor.DarkBlue, writer);
                write_log();


                Rewired.ReInput.InitializedEvent += new Action(() =>
                {
                    ct.WLColNL("{SYSDBG} rewired has initialized", ConsoleColor.Cyan, writer);
                    try
                    {
                        BurglaryMain.HarmonyInstance.PatchAll(typeof(BurglaryMain).GetType().Assembly);
                    }
                    catch (Exception ex)
                    {
                        ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("ERROR!", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("message: " + ex.Message, ConsoleColor.Red, writer);
                        ct.WLColNL("stacktrace: " + ex.StackTrace, ConsoleColor.Red, writer);
                        ct.WLColNL("datadict: " + ex.Data.ToString(), ConsoleColor.Red, writer);
                        ct.WLColNL("targetsite: " + ex.TargetSite.Name, ConsoleColor.Red, writer);
                        ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                        ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                    }
                    foreach (Addon a in BurglaryMain.Addons)
                    {
                        ct.WLColNL("{SYSDBG} found addon " + a.GetType().GetCustomAttribute<AddonData>().Name, ConsoleColor.DarkCyan, writer);
                        try
                        {
                            BurglaryMain.HarmonyInstance.PatchAll(a.GetType().Assembly);
                        }
                        catch (Exception ex)
                        {
                            ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                            ct.WLColNL("ERROR!", ConsoleColor.DarkRed, writer);
                            ct.WLColNL("message: " + ex.Message, ConsoleColor.Red, writer);
                            ct.WLColNL("stacktrace: " + ex.StackTrace, ConsoleColor.Red, writer);
                            ct.WLColNL("datadict: " + ex.Data.ToString(), ConsoleColor.Red, writer);
                            ct.WLColNL("targetsite: " + ex.TargetSite.Name, ConsoleColor.Red, writer);
                            ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                            ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                            ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                        }
                    }
                    ct.WLColNL("{SYSDBG} finished patching all addons in registry and main class. final count: " + BurglaryMain.HarmonyInstance.GetPatchedMethods().ToList().Count, ConsoleColor.Cyan, writer);
                });

                ct.WLColNL("Registering action to Rewired InitializedEvent... This should apply all patches to all addons in the registry.", ConsoleColor.Yellow, writer);


                BurglaryMain.HarmonyInstance = new Harmony("Burglary");
                ct.WLColNL("Warning! Harmony has been known to interfere with specific libraries used by The Break-In. Please patch with caution...", ConsoleColor.Yellow, writer);

                write_log();

                DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());

                string addonsPath = Path.Combine(dir.FullName, "addons");


                if (!Directory.Exists(addonsPath))
                {
                    ct.WLCol("Addons directory doesn't exist! Creating...\n", ConsoleColor.Red, writer);
                    Directory.CreateDirectory(addonsPath);
                }

                //BurglaryMain.Log("Addons Path: " + addonsPath);

                dir = new DirectoryInfo(addonsPath);

                ct.WCol("Searching for Addons in \"", ConsoleColor.Cyan, writer);
                ct.WCol(dir.FullName, ConsoleColor.DarkCyan, writer);
                ct.WLCol("\"...", ConsoleColor.Cyan, writer);
                write_log();

                List<string> filePaths = Directory.GetFiles(addonsPath).ToList();

                List<Type> addons = new List<Type>(); // so addons can be organized / stuff bnal lbal bla a
                                                      //(hard to get that through filenames lol)
                foreach (string FilePath in filePaths)
                {
                    ct.WLColNL(FilePath + " found in addons.", ConsoleColor.DarkGray, writer);
                    write_log();
                    if (FilePath.EndsWith(".dll"))
                    {
                        try
                        {
                            Assembly assembly = Assembly.LoadFile(FilePath);
                            foreach (Type t in assembly.GetTypes())
                            {
                                ct.WLColNL("t " + (t.FullName), ConsoleColor.DarkGray, writer);
                                ct.WLColNL("tbase " + (t.BaseType.FullName), ConsoleColor.DarkGray, writer);
                                ct.WLColNL("tnull? " + (t == null), ConsoleColor.DarkGray, writer);
                                ct.WLColNL("tdatnull? " + (t.GetCustomAttribute<AddonData>() == null), ConsoleColor.DarkGray, writer);
                                if (t.IsClass & t.BaseType.Equals(typeof(Addon)))
                                {
                                    bool pass = true;
                                    foreach (Type addon in addons)
                                    {
                                        try
                                        {
                                            if (t.GetCustomAttribute<AddonData>().Name == addon.GetCustomAttribute<AddonData>().Name)
                                            {
                                                ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                                                ct.WLCol(t.GetCustomAttribute<AddonData>().Name + " conflicts with pre registered addon " + addon.GetCustomAttribute<AddonData>().Name, ConsoleColor.Red, writer);
                                                ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                                                pass = false;
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                                            ct.WLColNL("Error checking for conflict.", ConsoleColor.Red, writer);
                                            ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                                            ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                                            ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                                        }
                                    }
                                    if (pass)//addons.FirstOrDefault(a => t.GetCustomAttribute<AddonData>().Name == a.GetType().GetCustomAttribute<AddonData>().Name) == null)
                                    {
                                        nl();
                                        ct.WLColNL("found " + ((AddonData)t.GetCustomAttribute(typeof(AddonData))).Name,
                                            ConsoleColor.DarkGray, writer);
                                        nl();
                                        write_log();
                                        addons.Add(t);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ct.WLColNL("Something went wrong when loading " + FilePath, ConsoleColor.DarkGray, writer);
                            ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                            ct.WLColNL("ERROR!", ConsoleColor.DarkRed, writer);
                            ct.WLColNL("message: " + ex.Message, ConsoleColor.Red, writer);
                            ct.WLColNL("stacktrace: " + ex.StackTrace, ConsoleColor.Red, writer);
                            ct.WLColNL("datadict: " + ex.Data.ToString(), ConsoleColor.Red, writer);
                            ct.WLColNL("targetsite: " + ex.TargetSite.Name, ConsoleColor.Red, writer);
                            ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                            ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                            ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                            write_log();
                        }
                    }
                }
                ct.WLColNL("Search concluded. Results: " + addons.Count.ToString(), ConsoleColor.Cyan, writer);
                write_log();

                addons = addons.OrderBy(a =>
                {
                    PriorityAttribute priorityAttribute = a.GetCustomAttribute<PriorityAttribute>();
                    return priorityAttribute != null ? priorityAttribute.Priority : 0;
                }).ToList();

                foreach (Type addon in addons)
                {
                    //BurglaryMain.Log("loading addon " + addon.FullName);
                    write_log();
                    try
                    {
                        Addon a = (Addon)Activator.CreateInstance(addon);//Burglary.asm_loader.load(addon);
                        ct.PrintAttributeData(a.GetType().GetCustomAttribute<AddonData>(), ConsoleColor.Magenta, writer);
                        write_log();
                        BurglaryMain.Addons.Add(a);
                        a.OnRegister();
                        a.OnBurglaryInitialize();
                    }
                    catch (Exception ex)
                    {
                        ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("ERROR!", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("message: " + ex.Message, ConsoleColor.Red, writer);
                        ct.WLColNL("stacktrace: " + ex.StackTrace, ConsoleColor.Red, writer);
                        ct.WLColNL("datadict: " + ex.Data.ToString(), ConsoleColor.Red, writer);
                        ct.WLColNL("targetsite: " + ex.TargetSite.Name, ConsoleColor.Red, writer);
                        ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                        ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                        write_log();
                    }
                    write_log();
                    //BurglaryMain.Log("done " + addon.FullName + "/mod" + addon.GetCustomAttribute<AddonData>().Name);
                }
                nl();

                foreach (Addon addon in BurglaryMain.Addons)
                {
                    try
                    {
                        addon.OnBurglaryLoad();
                    }
                    catch (Exception ex)
                    {
                        ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("ERROR!", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("message: " + ex.Message, ConsoleColor.Red, writer);
                        ct.WLColNL("stacktrace: " + ex.StackTrace, ConsoleColor.Red, writer);
                        ct.WLColNL("datadict: " + ex.Data.ToString(), ConsoleColor.Red, writer);
                        ct.WLColNL("targetsite: " + ex.TargetSite.Name, ConsoleColor.Red, writer);
                        ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                        ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                        ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                        write_log();
                    }
                    //BurglaryMain.Log("done " + addon.FullName + "/mod" + addon.GetCustomAttribute<AddonData>().Name);
                }
                write_log();

                SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>((Scene, LoadSceneMode) =>
                {
                    GameObject burglary = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    burglary.name = "BurglaryRequired";
                    burglary.transform.position = new Vector3(99999, 99999, 9999);
                    burglary.AddComponent<BurglaryMonoBehaviour>();
                    Burglary.Events.Utils.dispatcher.sceneload(Scene);
                });

                SceneManager.sceneUnloaded += new UnityAction<Scene>((Scene) =>
                {
                    Burglary.Events.Utils.dispatcher.sceneunload(Scene);
                });

                SceneManager.activeSceneChanged += new UnityAction<Scene, Scene>((Scene, Scene2) =>
                {
                    Burglary.Events.Utils.dispatcher.scenechange(Scene, Scene2);
                });
            }
            catch (Exception ex)
            {
                ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);
                ct.WLColNL("ERROR IN PRELOADER!", ConsoleColor.DarkRed, writer);
                ct.WLColNL("message: " + ex.Message, ConsoleColor.Red, writer);
                ct.WLColNL("stacktrace: " + ex.StackTrace, ConsoleColor.Red, writer);
                ct.WLColNL("datadict: " + ex.Data.ToString(), ConsoleColor.Red, writer);
                ct.WLColNL("targetsite: " + ex.TargetSite.Name, ConsoleColor.Red, writer);
                ct.WLColNL("---------------------", ConsoleColor.DarkRed, writer);
                ct.WLColNL("RAW: " + ex.ToString(), ConsoleColor.Red, writer);
                ct.WLColNL("=====================", ConsoleColor.DarkRed, writer);

                Console.WriteLine("Quitting.");
                writer.WriteLine("Quitting.");

                write_log();

                Utils.dispatcher.burgunload();
                FreeConsole();
                writer.Dispose();
            }

            ct.write_log();
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => { write_log(); Utils.dispatcher.burgunload(); FreeConsole(); writer.Dispose(); };
        }
    }
}
