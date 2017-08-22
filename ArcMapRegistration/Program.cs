using System;
using System.Diagnostics;
using DnrGps_ArcMap;
using System.Linq;

namespace ArcMapRegistration
{
    class Program
    {
        private const string AssemblyName = "DnrGps_ArcMap.dll";

        static int Main(string[] args)
        {
            //RunTests();
            return RegisterAssembly(args);
        }

        private static int RegisterAssembly(string[] args)
        {
            bool silent = IsSilentSet(args);
            bool register = IsRegisterSet(args);
            bool unregister = IsUnregisterSet(args);
            bool showHelp = IsHelpSet(args);

            if (register && unregister)
                showHelp = true;

            if (!register && !unregister && !silent && !showHelp && args.Length > 0)
                showHelp = true;

            if (showHelp)
            {
                ShowUsage();
                return 0;
            }

            if (!register && !unregister)
                register = true;

            if (register)
            {
                try
                {
                    Register();
                    if (!silent)
                    {
                        Console.WriteLine("Successfully registered " + AssemblyName);
                        PressKeyToContinue();
                    }
                    return 0;

                }
                catch (Exception e)
                {
                    if (!silent)
                    {
                        Console.WriteLine("Registration failed: {0}", e.Message);
                        PressKeyToContinue();
                    }
                    return 1;
                }
            }

            if (unregister)
            {
                try
                {
                    Unregister();
                    if (!silent)
                    {
                        Console.WriteLine("Successfully un-registered " + AssemblyName);
                        PressKeyToContinue();
                    }
                    return 0;

                }
                catch (Exception e)
                {
                    if (!silent)
                    {
                        Console.WriteLine("Un-registration failed: {0}", e.Message);
                        PressKeyToContinue();
                    }
                    return 1;
                }
            }
            //we should never get here
            return 1;
        }

        private static void PressKeyToContinue()
        {
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: " + AppDomain.CurrentDomain.FriendlyName + " [Options]");
            Console.WriteLine("Options: /register or /r - registers the adjacent " + AssemblyName);
            Console.WriteLine("         /unregister or /u  - unregisters the adjacent " + AssemblyName);
            Console.WriteLine("         /silent or /s  - run silently ");
            Console.WriteLine("         /help or /h or /?  - Show this help then exit");
            Console.WriteLine("/r is assumed if neither /r or /u are given");
            Console.WriteLine();
            PressKeyToContinue();
        }


        private static bool IsSilentSet(string[] args)
        {
            return args.Any(arg => arg.ToLower().StartsWith("/s"));
        }

        private static bool IsRegisterSet(string[] args)
        {
            return args.Any(arg => arg.ToLower().StartsWith("/r"));
        }

        private static bool IsUnregisterSet(string[] args)
        {
            return args.Any(arg => arg.ToLower().StartsWith("/u"));
        }

        private static bool IsHelpSet(string[] args)
        {
            return args.Any(arg => arg == "/?" || arg.ToLower().StartsWith("/h"));
        }


        private static void Register()
        {
            RegisterAssembly("/codebase");
        }

        private static void Unregister()
        {
            RegisterAssembly("/unregister");
        }

        private static void RegisterAssembly(string options)
        {
            string dll = System.IO.Path.Combine(AssemblyDirectory, AssemblyName);
            string sysdir = Environment.GetEnvironmentVariable("SystemRoot");
            string[] netdirs = {
                                   @"\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe",
                                   @"\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe",
                                   @"\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe",
                                   @"\Microsoft.NET\Framework64\v2.0.50727\RegAsm.exe",
                               };
            foreach (var netdir in netdirs)
            {
                string regasm = sysdir + netdir;
                if (!System.IO.File.Exists(regasm))
                    continue;
                string arguments = "\"" + dll + "\" /silent " + options;
                //Console.WriteLine("registration: " + regasm + " " + arguments);
                var process = new Process();
                var processStartInfo = new ProcessStartInfo(regasm, arguments) { UseShellExecute = false, RedirectStandardError = true };
                process.StartInfo = processStartInfo;
                process.Start();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                //Console.WriteLine("Error Stream = " + error);
                //Console.WriteLine("Exit code = " + process.ExitCode);
                if (process.ExitCode != 0)
                    throw new ApplicationException("Registration Error: " + error);
                return;
            }
            throw new ApplicationException("Registration Error: RegAsm executable not found in .Net system folders.");
        }

        private static void RegisterCOM(string options)
        {
            string dll = System.IO.Path.Combine(AssemblyDirectory, AssemblyName);
            string sysdir = Environment.GetEnvironmentVariable("CommonProgramFiles");
            string[] dirs = {
                                   @"\ArcGIS\bin\esriRegasm.exe",
                               };
            foreach (var dir in dirs)
            {
                string regasm = sysdir + dir;
                if (!System.IO.File.Exists(regasm))
                    continue;
                string arguments = "\"" + dll + "\" /p:Desktop /s " + options;
                //Console.WriteLine("registration: " + regasm + " " + arguments);
                var process = new Process();
                var processStartInfo = new ProcessStartInfo(regasm, arguments) { UseShellExecute = false, RedirectStandardError = true };
                process.StartInfo = processStartInfo;
                process.Start();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                //Console.WriteLine("Error Stream = " + error);
                //Console.WriteLine("Exit code = " + process.ExitCode);
                if (process.ExitCode != 0)
                    throw new ApplicationException("Registration Error: " + error);
                return;
            }
            throw new ApplicationException("Registration Error: RegAsm executable not found in .Net system folders.");
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return System.IO.Path.GetDirectoryName(codeBase);
            }
        }

        #region testing

        private static void RunTests()
        {
            //Test1 - unregister, and check for failure to load.
            try
            {
                Test1();
                Console.WriteLine("Test 1 failed to throw an exception");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Test 1 passed");
            }

            catch (Exception e)
            {
                Console.WriteLine("Test 1 failed: {0}", e.Message);
            }

            //Test2 - register for COM and then find component in ArcMap
            try
            {
                Test2();
                Console.WriteLine("Test 2 passed");
            }
            catch (Exception e)
            {
                Console.WriteLine("Test 2 failed: {0}", e.Message);
            }

            //Test3 - Start ArcMap, register for COM, then load component
            try
            {
                Test3();
                Console.WriteLine("Test 3 passed");
            }
            catch (Exception e)
            {
                Console.WriteLine("Test 3 failed: {0}", e.Message);
            }

            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }

        private static void Test1()
        {
            Unregister();
            ArcMapStart();
            ArcMapLoad();
        }

        private static void Test2()
        {
            Register();
            ArcMapStart();
            ArcMapLoad();
            Unregister();
        }

        private static void Test3()
        {
            ArcMapStart();
            Register();
            ArcMapLoad();
            Unregister();
        }

        private static void ArcMapStart()
        {
            if (Controller.HasOpenDocuments)
                return;
            Controller.StartNewDocument();
            if (Controller.HasOpenDocuments)
                return;
            throw new Exception("Failed to start ArcMap");
        }

        private static void ArcMapLoad()
        {
            IDnrGpsController ext = Controller.GetExtensionFromTopDocument();
            if (ext == null)
                throw new InvalidOperationException("Failed to load extension in ArcMap");
        }

        private static readonly ArcMapController Controller = new ArcMapController();

        #endregion
    }
}
