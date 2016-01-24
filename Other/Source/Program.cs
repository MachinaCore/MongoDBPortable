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


namespace Launcher
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Check for Launcher ini file
            Launcher.ReadLauncherIni();


            string pathvar = Environment.GetEnvironmentVariable("PATH");

            foreach (var section in Globals.Launcher["Environment"])
            {
                string sectionComment = null;
                if (section.Comment != null)
                {
                    sectionComment = section.Comment.ToString();
                }
                string sectionString = section.StringValue + sectionComment;

                sectionString = sectionString.Replace("%PATH%", pathvar);
                sectionString = sectionString.Replace("%PAL:AppDir%", Globals.AppPath + "\\App");
                sectionString = sectionString.Replace("%PAL:DataDir%", Globals.DataPath);

                if (section.Name == "PGSQL")
                    Globals.PostgreSQLEnvironment["PGSQL"] = sectionString;

                if (section.Name == "PGDATA")
                    Globals.PostgreSQLEnvironment["PGDATA"] = sectionString;

                if (section.Name == "PGLOG")
                    Globals.PostgreSQLEnvironment["PGLOG"] = sectionString;

                if (section.Name == "PGLOCALEDIR")
                    Globals.PostgreSQLEnvironment["PGLOCALEDIR"] = sectionString;

                if (section.Name == "PGDATABASE")
                    Globals.PostgreSQLEnvironment["PGDATABASE"] = sectionString;

                if (section.Name == "PGPORT")
                    Globals.PostgreSQLEnvironment["PGPORT"] = sectionString;

                if (section.Name == "PGUSER")
                    Globals.PostgreSQLEnvironment["PGUSER"] = sectionString;


                Environment.SetEnvironmentVariable(section.Name, sectionString);
                //Console.WriteLine(section.Name);
                //Console.WriteLine(section.StringValue.Replace("%PAL:AppDir%", Globals.AppPath));
                //Console.WriteLine(sectionString);
            }

            if (args != null)
            {
                if (args.Length != 0)
                {
                    for (int i = 0; i < args.Length; i++) // Loop through array
                    {
                        string argument = args[i];
                        if (argument == "stop")
                        {
                            Console.WriteLine("Stopping PostgreSQL"); // Check for null array
                            using (Process process = new Process())
                            {
                                process.StartInfo.FileName = Globals.PostgreSQLEnvironment["PGSQL"] + "\\bin\\pg_ctl.exe";
                                process.StartInfo.Arguments = (" stop --pgdata " + Globals.PostgreSQLEnvironment["PGDATA"]
                                                                + " -l " + Globals.PostgreSQLEnvironment["PGLOG"]
                                                                + " --mode=smart -W"
                                                                );
                                process.Start();
                            }
                            return;
                        }
                        if (argument == "restart")
                        {
                            Console.WriteLine("Restarting PostgreSQL"); // Check for null array
                            using (Process process = new Process())
                            {
                                process.StartInfo.FileName = Globals.PostgreSQLEnvironment["PGSQL"] + "\\bin\\pg_ctl.exe";
                                process.StartInfo.Arguments = (" stop --pgdata " + Globals.PostgreSQLEnvironment["PGDATA"]
                                                                + " -l " + Globals.PostgreSQLEnvironment["PGLOG"]
                                                                + " --mode=smart -W"
                                                                );
                                process.StartInfo.CreateNoWindow = false;
                                process.StartInfo.UseShellExecute = false;
                                process.Start();
                                process.WaitForExit();
                                Thread.Sleep(3000);
                            }
                        }
                    }
                }
            }


            //Check if running or definied Handler
            if (Classes.AlreadyRunning.Handler())
            {
                return;
            }

            //Check if logfile exist -> Rename
            if (File.Exists(Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", ".old.log")))
            {
                File.Delete(Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", ".old.log"));
            }
            if (File.Exists(Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", ".log")))
            {
                File.Move(Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", ".log"), Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", ".old.log"));
            }


            try
            {
                Globals.streamWriter = new StreamWriter(Globals.AppPath + "\\" + Globals.ExeFileName.Replace(".exe", "") + ".log", true);
            }
            catch
            {
                Console.WriteLine("Cant access log file -> Ignoring");
            }

            bool newInstance = true;
            bool newInstallation = false;

            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!Directory.Exists(Globals.PostgreSQLEnvironment["PGDATA"]))
                {
                    newInstallation = true;
                    //Creating Database
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = Globals.PostgreSQLEnvironment["PGSQL"] + "\\bin\\initdb.exe";
                        process.StartInfo.Arguments = (" -U " + Globals.PostgreSQLEnvironment["PGUSER"]
                                                        + " -A trust"
                                                        + " -E utf8"
                                                        + " --locale=C"
                                                        );
                        process.StartInfo.RedirectStandardError = true;
                        //process.StartInfo.RedirectStandardInput = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                        process.OutputDataReceived += CaptureOutput;
                        process.ErrorDataReceived += CaptureError;

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        Console.WriteLine("Database creation ready");
                    }
                }

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = Globals.PostgreSQLEnvironment["PGSQL"] + "\\bin\\pg_ctl.exe";
                    process.StartInfo.Arguments = (" -D " + Globals.PostgreSQLEnvironment["PGDATA"]
                                                    + " -l " + Globals.PostgreSQLEnvironment["PGLOG"]
                                                    + " -w start"
                                                    );
                    process.StartInfo.RedirectStandardError = true;
                    //process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                    process.OutputDataReceived += CaptureOutput;
                    process.ErrorDataReceived += CaptureError;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();


                    process.WaitForExit();

                    Console.WriteLine("Starting PostgreSQL");
                }
            }
        }

        static void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data, ConsoleColor.Green);
        }


        static void CaptureError(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data, ConsoleColor.Red);
        }

        static void ShowOutput(string data, ConsoleColor color)
        {
            if (data != null)
            {
                try
                {
                    Globals.streamWriter.WriteLine("[{0}][{1}] {2} ", String.Format("{0:yyyyMMdd HH:mm:ss}", DateTime.Now), "Info", data);
                    Globals.streamWriter.Flush();
                    ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine("Received: {0}", data);
                    Console.ForegroundColor = oldColor;
                }
                catch
                {
                    Console.WriteLine("Cant access log file -> Ignoring");
                }

            }
        }

    }



    public class Globals
    {
        public static Dictionary<string, string> PostgreSQLEnvironment = new Dictionary<string, string>();
        public static string ExeFile = Assembly.GetExecutingAssembly().Location;
        public static string ExeFileName = Path.GetFileName(ExeFile);
        public static string AppPath = Path.GetDirectoryName(Globals.ExeFile);
        public static string BasePath = Directory.GetParent(Path.GetDirectoryName(Globals.ExeFile)).FullName;
        public static string DataPath = Globals.AppPath + "\\Data";
        public static Configuration AppInfo = Configuration.LoadFromFile(AppPath + "\\App\\AppInfo\\AppInfo.ini");
        public static Configuration Launcher = null;
        public static StreamWriter streamWriter = null;
    }
}
