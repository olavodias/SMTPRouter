using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;
using IO = System.IO;

namespace SMTPRouter.Win.Installer
{
    public class Program
    {
        static int Main()
        {
            try
            {                
                var buildConfiguration = "Release";
                var buildPlatform = "net8.0-windows";
                var buildArchitecture = "win-x64";

                // Look for source code directory
                var sourceDirectoryInfo = new IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

                while (sourceDirectoryInfo != null && !sourceDirectoryInfo.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                {
                    sourceDirectoryInfo = sourceDirectoryInfo.Parent;
                }

                if (sourceDirectoryInfo == null)
                    throw new Exception("Unable to locate source code directory");

                // Setup Installation Features
                var serviceListenerBinariesFeature = new Feature("Smtp Message Listener", "Smtp Message Listener\n\nListen to Smtp messages and store them in the file system", true, true);
                var serviceRouterBinariesFeature = new Feature("Smtp Message Router", "Smtp Message Router\n\nProcess received messages and forward them to a different Smtp Server", true, true);
                //var managementBinariesFeature = new Feature("Service Management Application", "Manage Services", true, true); // For Future

                // Setup Paths
                var listenerBinariesPath = IO.Path.Combine(sourceDirectoryInfo.FullName, "SMTPRouter.Listener", "bin", buildConfiguration, buildPlatform, "publish", buildArchitecture);
                var routerBinariesPath = IO.Path.Combine(sourceDirectoryInfo.FullName, "SMTPRouter.Router", "bin", buildConfiguration, buildPlatform, "publish", buildArchitecture);

                // Look for files on output folder
                if (IO.Directory.GetFiles(listenerBinariesPath).Length == 0)
                    throw new IO.FileNotFoundException($"No files found under the Listener Publish Folder \"{listenerBinariesPath}\"");

                if (IO.Directory.GetFiles(routerBinariesPath).Length == 0)
                    throw new IO.FileNotFoundException($"No files found under the Router Publish Folder \"{routerBinariesPath}\"");

                // Setup Project
                var project = new ManagedProject("SMTPRouter",
                                                 new InstallDir(@"%ProgramFiles%\SMTPRouter",
                                                                new Dir(@"Bin\ListenerService",
                                                                        new DirFiles(serviceListenerBinariesFeature, IO.Path.Combine(listenerBinariesPath,"*.*"))),
                                                                new File(IO.Path.Combine(listenerBinariesPath, "SMTPRouter.Listener.exe"), 
                                                                         new ServiceInstaller
                                                                         {
                                                                             Name = "SMTPRouter.Listener",
                                                                             DisplayName = "SMTPRouter.Listener",
                                                                             StartOn = null,
                                                                             StopOn = SvcEvent.InstallUninstall_Wait,
                                                                             RemoveOn = SvcEvent.Uninstall_Wait,
                                                                             DelayedAutoStart = true,
                                                                             Start = SvcStartType.auto,
                                                                             ServiceSid = ServiceSid.none,
                                                                             ProgramCommandLine = "SMTPRouter.Listener.exe",
                                                                         }),
                                                                new Dir(@"Bin\RouterService",
                                                                        new DirFiles(serviceRouterBinariesFeature, IO.Path.Combine(routerBinariesPath,"*.*")))
                                                                //new Dir(@"Bin\Management",
                                                                //        new DirFiles(managementBinariesFeature, routerBinariesPath))
                                                               )
                                                 );
                
                project.GUID = new Guid("71237eb0-8802-4f93-967a-440b1db089bb");
                project.Version = Version.Parse(System.Reflection.Assembly.GetExecutingAssembly().GetVersion());

                project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
                project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

                //custom set of standard UI dialogs
                project.ManagedUI = new ManagedUI();

                project.ManagedUI.InstallDialogs.Add(Dialogs.Welcome)
                                                .Add(Dialogs.Licence)
                                                //.Add(Dialogs.SetupType)
                                                .Add(Dialogs.Features)
                                                .Add(Dialogs.InstallDir)
                                                .Add(Dialogs.Progress)
                                                .Add(Dialogs.Exit);

                project.ManagedUI.ModifyDialogs.Add(Dialogs.MaintenanceType)
                                               .Add(Dialogs.Features)
                                               .Add(Dialogs.Progress)
                                               .Add(Dialogs.Exit);

                project.Load += Msi_Load;
                project.BeforeInstall += Msi_BeforeInstall;
                project.AfterInstall += Msi_AfterInstall;

                project.OutDir = IO.Path.Combine(sourceDirectoryInfo.FullName, "SMTPRouter.Win.Installer", "msi", buildArchitecture);

#if DEBUG
                project.PreserveTempFiles = true;
#endif

                project.BuildMsi();

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred");
                Console.WriteLine($"Message.......: {e.Message}");
                Console.WriteLine($"Stack Trace...: {e.StackTrace}");

                return 1;
            }
        }

        static void Msi_Load(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "Load");
        }

        static void Msi_BeforeInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "BeforeInstall");
        }

        static void Msi_AfterInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "AfterExecute");
        }
    }
}