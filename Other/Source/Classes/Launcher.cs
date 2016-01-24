using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SharpConfig;

// Not App Specific -> Should work for any App

namespace Launcher
{
    class Launcher
    {

        public static void ReadLauncherIni()
        {
            //Check for Launcher ini file
            if (File.Exists(Globals.AppPath + "\\App\\AppInfo\\Launcher\\" + Globals.ExeFileName.Replace(".exe", ".ini")))
            {
                Globals.Launcher = Configuration.LoadFromFile(Globals.AppPath + "\\App\\AppInfo\\Launcher\\" + Globals.ExeFileName.Replace(".exe", ".ini"));
            }
            else
            {
                MessageBox.Show(Globals.AppPath + "\\App\\AppInfo\\Launcher\\" + Globals.ExeFileName.Replace(".exe", ".ini") + " not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Overwrite with values from Main Path
            if (File.Exists(Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", ".ini")))
            {
                Configuration iniFilePortableAppsLauncher = Configuration.LoadFromFile(Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", ".ini"));
                //Loop values and replace dummy with ini values
                foreach (var section in iniFilePortableAppsLauncher)
                {
                    try
                    {
                        foreach (var key in section)
                        {
                            if (key.StringValue == "true" || key.StringValue == "True")
                            {
                                Globals.Launcher[section.Name][key.Name].SetValue(true);
                            }
                            else if (key.StringValue == "false" || key.StringValue == "False")
                            {
                                Globals.Launcher[section.Name][key.Name].SetValue(false);
                            }
                            else
                            {
                                Globals.Launcher[section.Name][key.Name].SetValue(key.StringValue);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (Globals.Launcher.Contains("AppDependencies"))
            {
                ResolveAppDependencies();
            }

        }

        public static void ResolveAppDependencies() { 
            if (Globals.Launcher["AppDependencies"].Contains("AppName"))
            {
                //Check if Needed App is innstalled

                if (!Directory.Exists(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue))
                {
                    MessageBox.Show(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + " not found - Exiting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\App\\AppInfo\\Launcher\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + ".ini"))
                {
                    MessageBox.Show(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\App\\AppInfo\\Launcher\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + ".ini not found - Exiting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Configuration dependencyAppLauncher = Configuration.LoadFromFile(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\App\\AppInfo\\Launcher\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + ".ini");
                bool appIsRunning = DependencyProcessName(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\App\\AppInfo\\Launcher\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + ".ini");
                if (appIsRunning)
                {
                    return;
                }


                if (!appIsRunning && Globals.Launcher["AppDependencies"].Contains("Autostart"))
                {
                    if (Globals.Launcher["AppDependencies"]["Autostart"].BoolValue)
                    {
                        if (!File.Exists(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\App\\AppInfo\\appinfo.ini"))
                        {
                            MessageBox.Show(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\App\\AppInfo\\appinfo.ini not found - Exiting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Configuration dependencyAppInfo = Configuration.LoadFromFile(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\App\\AppInfo\\appinfo.ini");
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue + "\\" + dependencyAppInfo["Control"]["Start"].StringValue;
                            process.StartInfo.WorkingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\" + Globals.Launcher["AppDependencies"]["AppName"].StringValue;
                            process.Start();
                            Thread.Sleep(5000);
                            return;
                        }

                    }
                }
                else
                {
                    MessageBox.Show(Globals.Launcher["AppDependencies"]["AppName"].StringValue + " not running - Exiting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }


        public static bool DependencyProcessName(string launcherIni)
        {
            string processName = null;
            Configuration dependencyAppLauncher = Configuration.LoadFromFile(launcherIni);
            if (dependencyAppLauncher["Launch"].Contains("ProcessName"))
            {
                processName = dependencyAppLauncher["Launch"]["ProcessName"].StringValue;
            }
            if (dependencyAppLauncher["Launch"].Contains("ProgramExecutable"))
            {
                processName = dependencyAppLauncher["Launch"]["ProgramExecutable"].StringValue;
            }
            if (processName != null)
            {
                if (Classes.ProcessExplorer.IsExeRunning(processName))
                {
                    return true;
                }
            }

            return false;
        }


    }
}
