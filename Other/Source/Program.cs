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
using GathSystemsPortableApp;

namespace Launcher
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Check for Launcher ini file
            PortableApp.Init();

            //Set Stop Arguments
            Globals.StopFileName = Globals.EnvironmentVariable["PGSQL"] + "\\bin\\pg_ctl.exe";
            Globals.StopArguments = " stop --pgdata " + Globals.EnvironmentVariable["PGDATA"]
                                  + " -l " + Globals.EnvironmentVariable["PGLOG"]
                                  + " --mode=smart -W";


            //Create Database if not exists
            if (!Directory.Exists(Globals.EnvironmentVariable["PGDATA"]))
            {
                Globals.StartFileName = Globals.EnvironmentVariable["PGSQL"] + "\\bin\\initdb.exe";
                Globals.StartArguments = " -U " + Globals.EnvironmentVariable["PGUSER"]
                                         + " -A trust"
                                         + " -E utf8"
                                         + " --locale=C";
                Console.WriteLine("Database creation ready");
                PortableApp.Run(args);
            }

            //Start Database
            Globals.StartFileName = Globals.EnvironmentVariable["PGSQL"] + "\\bin\\pg_ctl.exe";
            Globals.StartArguments = " -D " + Globals.EnvironmentVariable["PGDATA"]
                                    + " -l " + Globals.EnvironmentVariable["PGLOG"]
                                    + " -w start";

            Console.WriteLine("Starting PostgreSQL");

            PortableApp.Run(args);





        }


    }


}
