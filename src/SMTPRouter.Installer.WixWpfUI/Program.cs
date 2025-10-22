using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.WPF;

namespace SMTPRouter.Installer.WixWpfUI
{
    public class Program
    {
        static void Main()
        {
            var project = new ManagedProject("MyProduct",
                              new Dir(@"%ProgramFiles%\My Company\My Product",
                                  new File("Program.cs")));

            project.GUID = new Guid("d69d837d-9fc3-4ace-9ec4-2b7d23fb27dc");

            // project.ManagedUI = ManagedUI.DefaultWpf; // all stock UI dialogs

            //custom set of UI WPF dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add<SMTPRouter.Installer.WixWpfUI.WelcomeDialog>()
                                            .Add<SMTPRouter.Installer.WixWpfUI.LicenceDialog>()
                                            .Add<SMTPRouter.Installer.WixWpfUI.FeaturesDialog>()
                                            .Add<SMTPRouter.Installer.WixWpfUI.InstallDirDialog>()
                                            .Add<SMTPRouter.Installer.WixWpfUI.ProgressDialog>()
                                            .Add<SMTPRouter.Installer.WixWpfUI.ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<SMTPRouter.Installer.WixWpfUI.MaintenanceTypeDialog>()
                                           .Add<SMTPRouter.Installer.WixWpfUI.FeaturesDialog>()
                                           .Add<SMTPRouter.Installer.WixWpfUI.ProgressDialog>()
                                           .Add<SMTPRouter.Installer.WixWpfUI.ExitDialog>();

            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            project.BuildMsi();
        }
    }
}