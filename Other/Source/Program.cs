using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using SharpConfig;
using MachinaCorePortableApp;

namespace Launcher
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Check for Launcher ini file
            PortableApp.Init();

            bool folderExists = Directory.Exists(Globals.EnvironmentVariable["MONGODB_DB"]);
            if (!folderExists)
            {
                Directory.CreateDirectory(Globals.EnvironmentVariable["MONGODB_DB"]);
            }

            //Set Stop Arguments
            Globals.StopFileName = Globals.EnvironmentVariable["MONGODB_BIN"] + "\\bin\\mongo.exe";
            Globals.StopArguments = " admin --eval \"db.shutdownServer()\"";

            //Start Database
            Globals.StartFileName = Globals.EnvironmentVariable["MONGODB_BIN"] + "\\bin\\mongod.exe";
            Globals.StartArguments = " --dbpath " + Globals.EnvironmentVariable["MONGODB_DB"]
                                    + " --port " + Globals.EnvironmentVariable["MONGODB_PORT"];

            Console.WriteLine("Starting MongoDB");

            PortableApp.Run(args);
        }


    }


}
