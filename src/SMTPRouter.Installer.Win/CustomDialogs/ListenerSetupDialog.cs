using System;
using System.Diagnostics;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.Forms;

namespace WixSharpSetup
{
    public partial class ListenerSetupDialog : ManagedForm, IManagedDialog
    {
        public ListenerSetupDialog()
        {
            //NOTE: If this assembly is compiled for v4.0.30319 runtime, it may not be compatible with the MSI hosted CLR.
            //The incompatibility is particularly possible for the Embedded UI scenarios. 
            //The safest way to avoid the problem is to compile the assembly for v3.5 Target Framework.WixSharp Setup
            InitializeComponent();
        }

        void ListenerSetupDialog_Load(object sender, EventArgs e)
        {
            banner.Image = Runtime.Session.GetResourceBitmap("WixUI_Bmp_Banner");
            Text = "[ProductName] Setup";

            //resolve all Control.Text cases with embedded MSI properties (e.g. 'ProductName') and *.wxl file entries
            base.Localize();
        }

        void ButtonBack_Click(object sender, EventArgs e)
        {
            Shell.GoPrev();
        }

        void ButtonNext_Click(object sender, EventArgs e)
        {
            Shell.GoNext();
        }

        void ButtonCancel_Click(object sender, EventArgs e)
        {
            Shell.Cancel();
        }

        private void ListenerSetupDialog_Shown(object sender, EventArgs e)
        {
            var listOfFeatures = string.Empty;

            foreach (var f in MsiRuntime.Session.Features)
            {
                listOfFeatures += f;
                listOfFeatures += Environment.NewLine;

            }


            MessageBox.Show("List of features\n\n" + listOfFeatures);


            MessageBox.Show("Add Local\n\n" + MsiRuntime.Session.Property("ADDLOCAL"));
            
            MessageBox.Show("Add Features\n\n" + MsiRuntime.Session["ADDFEATURES"]);
            

            //Shell.GoNext();

        }
    }
}