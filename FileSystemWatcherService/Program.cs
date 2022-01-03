using System;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace FileSystemWatcherService
{
    static class Program
    {
        static void Main(
            string [] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (args.Length == 1)
            {
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();

                    var installer = new AssemblyInstaller(assembly, null)
                    {
                        UseNewContext = true
                    };

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

                catch
                {
                    // Ignore
                }

                return;
            }

            var config = (WatchersConfig) ConfigurationManager.GetSection("watchers");

            ServiceBase.Run(new Service(config));
        }
    }
}
