using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;
using IO = System.IO;

namespace SMTPRouter.Win.Installer
{
    public class Program
    {
        static void Main()
        {
#if DEBUG
            var buildConfiguration = "Debug";
#else
            var buildConfiguration = "Release";
#endif

            buildConfiguration = "Release";
            var buildPlatform = "net8.0-windows";

            // Look for parent directory
            var directoryInfo = new IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (!directoryInfo.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
            {
                directoryInfo = directoryInfo.Parent;
            }

            if (directoryInfo == null)
                return;

            // Build Output Directory
            var servicesFeature = new Feature("Services", "Application Services", true, false);

            var listenerBinariesFeature = new Feature("Listener", "Smtp Message Listener\n\nListen to Smtp messages and store them in the file system", true, false);
            var listenerBinariesPath = IO.Path.Combine(directoryInfo.FullName, "SMTPRouter.Listener", "bin", buildConfiguration, buildPlatform, "publish", "win-x64");

            var routerBinariesFeature = new Feature("Router", "Smtp Message Router\n\nProcess received messages and forward them to a different Smtp Server", true, false);
            var routerBinariesPath = IO.Path.Combine(directoryInfo.FullName, "SMTPRouter.Router", "bin", buildConfiguration, buildPlatform, "publish", "win-x64");

            var managementBinariesFeature = new Feature("Service Manager", "Manage Services", true, true);

            servicesFeature.Add(listenerBinariesFeature);
            servicesFeature.Add(routerBinariesFeature);

            var project = new ManagedProject("SMTPRouter", new Dir(@"%ProgramFiles%\SMTPRouter", new DirFiles(listenerBinariesFeature, listenerBinariesPath),
                                                                                                 new DirFiles(routerBinariesFeature, routerBinariesPath))
                                             );

            project.GUID = new Guid("71237eb0-8802-4f93-967a-440b1db089bb");

            project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
            project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

            project.UI = WUI.WixUI_FeatureTree;
            project.DefaultFeature = servicesFeature;

            //custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add(Dialogs.Welcome)
                                            .Add(Dialogs.Licence)
                                            .Add(Dialogs.SetupType)
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


            //project.SourceBaseDir = "<input dir path>";
            project.OutDir = IO.Path.Combine(directoryInfo.FullName, "SMTPRouter.Win.Installer", "msi");

            project.BuildMsi();
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