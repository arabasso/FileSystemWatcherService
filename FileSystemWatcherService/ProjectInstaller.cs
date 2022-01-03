using System.ComponentModel;

namespace FileSystemWatcherService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller :
        System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
