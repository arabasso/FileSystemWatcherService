using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;

namespace FileSystemWatcherService
{
    static class Program
    {
        static void Main(
            string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (!Environment.UserInteractive)
            {
                ServiceBase.Run(new Service());

                return;
            }

            if (args.Length == 1)
            {
                var assembly = Assembly.GetExecutingAssembly();

                if (IsAdministrator())
                {
                    var installer = new AssemblyInstaller(assembly, null)
                    {
                        UseNewContext = true
                    };

                    try
                    {
                        switch (args[0].ToLower())
                        {
                            case "/install":
                                installer.Install(null);
                                break;

                            case "/uninstall":
                                installer.Uninstall(null);
                                break;
                        }

                        installer.Commit(null);
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = assembly.Location,
                    Arguments = string.Join(" ", args),
                    CreateNoWindow = true,
                    Verb = "runas",
                };

                Process.Start(startInfo)?.WaitForExit();

                return;
            }

            Console.WriteLine(@"Usage: FileSystemWatcherService.exe [OPTION]");
            Console.WriteLine();
            Console.WriteLine(@"  /install      To install service");
            Console.WriteLine(@"  /uninstall    To uninstall service");
        }

        public static bool IsAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();

            var principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
