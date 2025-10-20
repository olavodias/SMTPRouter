namespace WixSharpSetup
{
    partial class ListenerSetupDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.banner = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.back = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRegularPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSecurePort = new System.Windows.Forms.TextBox();
            this.chkSecurePortEnable = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // banner
            // 
            this.banner.BackColor = System.Drawing.Color.White;
            this.banner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.banner.Location = new System.Drawing.Point(0, 0);
            this.banner.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.banner.Name = "banner";
            this.banner.Size = new System.Drawing.Size(501, 59);
            this.banner.TabIndex = 0;
            this.banner.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.banner);
            this.panel2.Location = new System.Drawing.Point(-5, -1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(503, 61);
            this.panel2.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(30, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(206, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Configure the Listener of SMTP Messages";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 14);
            this.label1.TabIndex = 1;
            this.label1.Text = "Listener Setup";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.back);
            this.panel1.Controls.Add(this.next);
            this.panel1.Controls.Add(this.cancel);
            this.panel1.Location = new System.Drawing.Point(-3, 308);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(503, 57);
            this.panel1.TabIndex = 9;
            // 
            // back
            // 
            this.back.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.back.Location = new System.Drawing.Point(227, 12);
            this.back.Name = "back";
            this.back.Size = new System.Drawing.Size(75, 23);
            this.back.TabIndex = 0;
            this.back.Text = "[WixUIBack]";
            this.back.UseVisualStyleBackColor = true;
            this.back.Click += new System.EventHandler(this.ButtonBack_Click);
            // 
            // next
            // 
            this.next.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.next.Location = new System.Drawing.Point(308, 12);
            this.next.Name = "next";
            this.next.Size = new System.Drawing.Size(75, 23);
            this.next.TabIndex = 0;
            this.next.Text = "[WixUINext]";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.ButtonNext_Click);
            // 
            // cancel
            // 
            this.cancel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancel.Location = new System.Drawing.Point(404, 12);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 0;
            this.cancel.Text = "[WixUICancel]";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Regular Port";
            // 
            // txtRegularPort
            // 
            this.txtRegularPort.Location = new System.Drawing.Point(132, 82);
            this.txtRegularPort.Name = "txtRegularPort";
            this.txtRegularPort.Size = new System.Drawing.Size(100, 21);
            this.txtRegularPort.TabIndex = 12;
            this.txtRegularPort.Text = "25";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Secure Port";
            // 
            // txtSecurePort
            // 
            this.txtSecurePort.Enabled = false;
            this.txtSecurePort.Location = new System.Drawing.Point(132, 125);
            this.txtSecurePort.Name = "txtSecurePort";
            this.txtSecurePort.Size = new System.Drawing.Size(100, 21);
            this.txtSecurePort.TabIndex = 14;
            this.txtSecurePort.Text = "587";
            // 
            // chkSecurePortEnable
            // 
            this.chkSecurePortEnable.AutoSize = true;
            this.chkSecurePortEnable.Location = new System.Drawing.Point(262, 128);
            this.chkSecurePortEnable.Name = "chkSecurePortEnable";
            this.chkSecurePortEnable.Size = new System.Drawing.Size(58, 17);
            this.chkSecurePortEnable.TabIndex = 15;
            this.chkSecurePortEnable.Text = "Enable";
            this.chkSecurePortEnable.UseVisualStyleBackColor = true;
            // 
            // ListenerSetupDialog
            // 
            this.ClientSize = new System.Drawing.Size(494, 361);
            this.Controls.Add(this.chkSecurePortEnable);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtSecurePort);
            this.Controls.Add(this.txtRegularPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ListenerSetupDialog";
            this.Text = "[UserNameDlg_Title]";
            this.Load += new System.EventHandler(this.ListenerSetupDialog_Load);
            this.Shown += new System.EventHandler(this.ListenerSetupDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.banner)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox banner;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button back;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRegularPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSecurePort;
        private System.Windows.Forms.CheckBox chkSecurePortEnable;
    }
}