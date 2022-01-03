
namespace FileSystemWatcherService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Designer de Componentes

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.FileSystemWatcherServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.FileSystemWatcherServiceServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // FileSystemWatcherServiceProcessInstaller
            // 
            this.FileSystemWatcherServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.FileSystemWatcherServiceProcessInstaller.Password = null;
            this.FileSystemWatcherServiceProcessInstaller.Username = null;
            // 
            // FileSystemWatcherServiceServiceInstaller
            // 
            this.FileSystemWatcherServiceServiceInstaller.Description = "Listens to the file system change notifications and raises events when a director" +
    "y, or file in a directory, changes.";
            this.FileSystemWatcherServiceServiceInstaller.DisplayName = "FileSystemWatcherService";
            this.FileSystemWatcherServiceServiceInstaller.ServiceName = "FileSystemWatcherService";
            this.FileSystemWatcherServiceServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.FileSystemWatcherServiceProcessInstaller,
            this.FileSystemWatcherServiceServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller FileSystemWatcherServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller FileSystemWatcherServiceServiceInstaller;
    }
}