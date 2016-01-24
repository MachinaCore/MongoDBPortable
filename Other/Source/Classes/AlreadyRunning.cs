using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Launcher.Classes
{
    class AlreadyRunning
    {
        public static bool Handler()
        {
            bool runningCheck = false;
            if (Globals.Launcher["Launch"].Contains("ProcessName"))
            {
                runningCheck = Classes.ProcessExplorer.IsExeRunning(Globals.Launcher["Launch"]["ProcessName"].StringValue);
            }
            else
            {
                runningCheck = Classes.ProcessExplorer.IsSelfRunning();
            }

            if (runningCheck)
            {
                if (Globals.Launcher["Launch"].Contains("AlreadyRunningHandler"))
                {
                    if (Globals.Launcher["Launch"]["AlreadyRunningHandler"].StringValue == "GathSystemsElectron")
                    {
                        string filePath = "";
                        if (File.Exists(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\GathSystems.com\\Data\\MainConfig.json"))
                        {
                            filePath = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\GathSystems.com\\Data\\MainConfig.json";
                        }
                        else
                        {
                            MessageBox.Show("GathSystems.com Main Application not found - Exiting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(-1);
                        }
                        JObject mainConfigFile = JObject.Parse(File.ReadAllText(filePath));
                        try
                        {
                            var client = new WebClient {Credentials = new NetworkCredential((string) mainConfigFile["webConfig"]["webUsername"], (string) mainConfigFile["webConfig"]["webPassword"])};
                            var response = client.DownloadString("http://127.0.0.1:" + (string) mainConfigFile["webConfig"]["webPort"] + "/appBrowserWindow/" + Globals.AppInfo["Details"]["AppID"].StringValue);
                        }
                        catch
                        {
                            Console.WriteLine("AlreadyRunningHandler GathSystemsElectron failed - Is GathSystems.com Main application running ?");
                        }

                        Environment.Exit(-1);
                    }
                    else
                    {
                        Console.WriteLine(Globals.ExeFileName + " already running"); // Check for null array
                        Console.WriteLine("Use " + Globals.ExeFileName + " stop"); // Check for null array
                        return true;
                    }
                }

                Console.WriteLine(Globals.ExeFileName + " already running"); // Check for null array
                Console.WriteLine("Use " + Globals.ExeFileName + " stop"); // Check for null array
                return true;
            }
        return false;
        }
    }
}
