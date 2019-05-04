namespace _432project_server
{
    partial class Form1
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
            this.logs = new System.Windows.Forms.RichTextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listenButton = new System.Windows.Forms.Button();
            this.passwordBox = new System.Windows.Forms.TextBox();
            this.password = new System.Windows.Forms.Label();
            this.sendButton = new System.Windows.Forms.Button();
            this.passPanel = new System.Windows.Forms.Panel();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.labelchangepass = new System.Windows.Forms.Label();
            this.changePassBtn = new System.Windows.Forms.Button();
            this.labelnewpass = new System.Windows.Forms.Label();
            this.labeloldpass = new System.Windows.Forms.Label();
            this.newPassBox = new System.Windows.Forms.TextBox();
            this.oldPassBox = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.passPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(233, 24);
            this.logs.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(261, 316);
            this.logs.TabIndex = 1;
            this.logs.Text = "";
            // 
            // textBox2
            // 
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(102, 116);
            this.textBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(111, 20);
            this.textBox2.TabIndex = 2;
            this.textBox2.Text = "1234";
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(71, 117);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // listenButton
            // 
            this.listenButton.Enabled = false;
            this.listenButton.Location = new System.Drawing.Point(143, 140);
            this.listenButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(70, 26);
            this.listenButton.TabIndex = 5;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            this.listenButton.Click += new System.EventHandler(this.listenButton_Click);
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point(102, 27);
            this.passwordBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.Size = new System.Drawing.Size(111, 20);
            this.passwordBox.TabIndex = 6;
            this.passwordBox.TextChanged += new System.EventHandler(this.passwordBox_TextChanged);
            // 
            // password
            // 
            this.password.AutoSize = true;
            this.password.Location = new System.Drawing.Point(43, 27);
            this.password.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.password.Name = "password";
            this.password.Size = new System.Drawing.Size(56, 13);
            this.password.TabIndex = 7;
            this.password.Text = "Password:";
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(148, 51);
            this.sendButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(65, 25);
            this.sendButton.TabIndex = 8;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // passPanel
            // 
            this.passPanel.BackColor = System.Drawing.Color.SeaShell;
            this.passPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.passPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.passPanel.Controls.Add(this.cancelBtn);
            this.passPanel.Controls.Add(this.labelchangepass);
            this.passPanel.Controls.Add(this.changePassBtn);
            this.passPanel.Controls.Add(this.labelnewpass);
            this.passPanel.Controls.Add(this.labeloldpass);
            this.passPanel.Controls.Add(this.newPassBox);
            this.passPanel.Controls.Add(this.oldPassBox);
            this.passPanel.Location = new System.Drawing.Point(11, 184);
            this.passPanel.Name = "passPanel";
            this.passPanel.Size = new System.Drawing.Size(217, 156);
            this.passPanel.TabIndex = 15;
            this.passPanel.Visible = false;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(102, 117);
            this.cancelBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(48, 24);
            this.cancelBtn.TabIndex = 21;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Visible = false;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // labelchangepass
            // 
            this.labelchangepass.AutoSize = true;
            this.labelchangepass.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.labelchangepass.Location = new System.Drawing.Point(43, 23);
            this.labelchangepass.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelchangepass.Name = "labelchangepass";
            this.labelchangepass.Size = new System.Drawing.Size(122, 15);
            this.labelchangepass.TabIndex = 20;
            this.labelchangepass.Text = "Change Password";
            this.labelchangepass.Visible = false;
            // 
            // changePassBtn
            // 
            this.changePassBtn.Location = new System.Drawing.Point(154, 117);
            this.changePassBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.changePassBtn.Name = "changePassBtn";
            this.changePassBtn.Size = new System.Drawing.Size(48, 24);
            this.changePassBtn.TabIndex = 19;
            this.changePassBtn.Text = "Okay";
            this.changePassBtn.UseVisualStyleBackColor = true;
            this.changePassBtn.Visible = false;
            this.changePassBtn.Click += new System.EventHandler(this.changePassBtn_Click);
            // 
            // labelnewpass
            // 
            this.labelnewpass.AutoSize = true;
            this.labelnewpass.Location = new System.Drawing.Point(6, 95);
            this.labelnewpass.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelnewpass.Name = "labelnewpass";
            this.labelnewpass.Size = new System.Drawing.Size(81, 13);
            this.labelnewpass.TabIndex = 18;
            this.labelnewpass.Text = "New Password:";
            this.labelnewpass.Visible = false;
            // 
            // labeloldpass
            // 
            this.labeloldpass.AutoSize = true;
            this.labeloldpass.Location = new System.Drawing.Point(12, 60);
            this.labeloldpass.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labeloldpass.Name = "labeloldpass";
            this.labeloldpass.Size = new System.Drawing.Size(75, 13);
            this.labeloldpass.TabIndex = 17;
            this.labeloldpass.Text = "Old Password:";
            this.labeloldpass.Visible = false;
            // 
            // newPassBox
            // 
            this.newPassBox.Location = new System.Drawing.Point(91, 92);
            this.newPassBox.Name = "newPassBox";
            this.newPassBox.Size = new System.Drawing.Size(111, 20);
            this.newPassBox.TabIndex = 16;
            this.newPassBox.Visible = false;
            // 
            // oldPassBox
            // 
            this.oldPassBox.Location = new System.Drawing.Point(91, 57);
            this.oldPassBox.Name = "oldPassBox";
            this.oldPassBox.Size = new System.Drawing.Size(111, 20);
            this.oldPassBox.TabIndex = 15;
            this.oldPassBox.Visible = false;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkColor = System.Drawing.Color.Blue;
            this.linkLabel1.Location = new System.Drawing.Point(120, 78);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(93, 13);
            this.linkLabel1.TabIndex = 16;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Change Password";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 351);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.passPanel);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.password);
            this.Controls.Add(this.passwordBox);
            this.Controls.Add(this.listenButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.logs);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.passPanel.ResumeLayout(false);
            this.passPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.Label password;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Panel passPanel;
        private System.Windows.Forms.Label labelchangepass;
        private System.Windows.Forms.Button changePassBtn;
        private System.Windows.Forms.Label labelnewpass;
        private System.Windows.Forms.Label labeloldpass;
        private System.Windows.Forms.TextBox newPassBox;
        private System.Windows.Forms.TextBox oldPassBox;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button cancelBtn;
    }
}

