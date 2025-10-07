using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;

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

            var buildOutputDirectory = Environment.CurrentDirectory;

            var listenerBinaries = new Feature("Listener", "SMTP Message Listener", true, false);
            var routerBinaries = new Feature("Router", "SMTP Message Router", true, false);
            var managementBinaries = new Feature("Service Manager", "Manage Services", true, true);

            


            var project = new ManagedProject("SMTPRouter", 
                                             new Dir(@"%ProgramFiles%\SMTPRouter",
                                             
                                             new File("Program.cs")));

            project.GUID = new Guid("71237eb0-8802-4f93-967a-440b1db089bb");

            project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
            project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

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

            project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

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