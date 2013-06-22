namespace ByteStorm.ReverseCryptoDrive.Gui
{
    partial class ReverseCryptoDrive
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReverseCryptoDrive));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.tnaMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unmountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxMountPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.driveLetterBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonMount = new System.Windows.Forms.Button();
            this.buttonUnmount = new System.Windows.Forms.Button();
            this.textBoxLogging = new System.Windows.Forms.TextBox();
            this.chkForce = new System.Windows.Forms.CheckBox();
            this.tnaMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.tnaMenu;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ReverseCryptoDrive";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // tnaMenu
            // 
            this.tnaMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mountToolStripMenuItem,
            this.unmountToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.tnaMenu.Name = "tnaMenu";
            this.tnaMenu.Size = new System.Drawing.Size(126, 70);
            this.tnaMenu.Text = "Actions";
            // 
            // mountToolStripMenuItem
            // 
            this.mountToolStripMenuItem.Name = "mountToolStripMenuItem";
            this.mountToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.mountToolStripMenuItem.Text = "&Mount";
            this.mountToolStripMenuItem.Click += new System.EventHandler(this.mountToolStripMenuItem_Click);
            // 
            // unmountToolStripMenuItem
            // 
            this.unmountToolStripMenuItem.Enabled = false;
            this.unmountToolStripMenuItem.Name = "unmountToolStripMenuItem";
            this.unmountToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.unmountToolStripMenuItem.Text = "&Unmount";
            this.unmountToolStripMenuItem.Click += new System.EventHandler(this.unmountToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 300);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(458, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(42, 17);
            this.toolStripStatusLabel1.Text = "Status:";
            // 
            // toolStripStatus
            // 
            this.toolStripStatus.Name = "toolStripStatus";
            this.toolStripStatus.Size = new System.Drawing.Size(26, 17);
            this.toolStripStatus.Text = "Idle";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ByteStorm.ReverseCryptoDrive.Gui.Properties.Resources.Crypto;
            this.pictureBox1.Location = new System.Drawing.Point(6, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(0, 70);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 95);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.button1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxMountPath, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.driveLetterBox, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 20);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(452, 61);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mounted Directory";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(390, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(59, 20);
            this.button1.TabIndex = 2;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // textBoxMountPath
            // 
            this.textBoxMountPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMountPath.Location = new System.Drawing.Point(103, 3);
            this.textBoxMountPath.Name = "textBoxMountPath";
            this.textBoxMountPath.Size = new System.Drawing.Size(281, 20);
            this.textBoxMountPath.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 33);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Drive Letter";
            // 
            // driveLetterBox
            // 
            this.driveLetterBox.FormattingEnabled = true;
            this.driveLetterBox.Items.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"});
            this.driveLetterBox.Location = new System.Drawing.Point(103, 31);
            this.driveLetterBox.MaxLength = 1;
            this.driveLetterBox.Name = "driveLetterBox";
            this.driveLetterBox.Size = new System.Drawing.Size(66, 21);
            this.driveLetterBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Brandish", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(69, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(394, 47);
            this.label3.TabIndex = 4;
            this.label3.Text = "ReverseCryptoDrive";
            // 
            // buttonMount
            // 
            this.buttonMount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonMount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMount.Location = new System.Drawing.Point(128, 252);
            this.buttonMount.Name = "buttonMount";
            this.buttonMount.Size = new System.Drawing.Size(107, 36);
            this.buttonMount.TabIndex = 5;
            this.buttonMount.Text = "Mount";
            this.buttonMount.UseVisualStyleBackColor = true;
            this.buttonMount.Click += new System.EventHandler(this.buttonMount_Click);
            // 
            // buttonUnmount
            // 
            this.buttonUnmount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonUnmount.Enabled = false;
            this.buttonUnmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUnmount.Location = new System.Drawing.Point(241, 252);
            this.buttonUnmount.Name = "buttonUnmount";
            this.buttonUnmount.Size = new System.Drawing.Size(107, 36);
            this.buttonUnmount.TabIndex = 6;
            this.buttonUnmount.Text = "Unmount";
            this.buttonUnmount.UseVisualStyleBackColor = true;
            this.buttonUnmount.Click += new System.EventHandler(this.buttonUnmount_Click);
            // 
            // textBoxLogging
            // 
            this.textBoxLogging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLogging.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxLogging.Location = new System.Drawing.Point(6, 175);
            this.textBoxLogging.Multiline = true;
            this.textBoxLogging.Name = "textBoxLogging";
            this.textBoxLogging.ReadOnly = true;
            this.textBoxLogging.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLogging.Size = new System.Drawing.Size(446, 65);
            this.textBoxLogging.TabIndex = 8;
            this.textBoxLogging.Text = "Log Output\r\n";
            // 
            // chkForce
            // 
            this.chkForce.AutoSize = true;
            this.chkForce.Location = new System.Drawing.Point(354, 263);
            this.chkForce.Name = "chkForce";
            this.chkForce.Size = new System.Drawing.Size(50, 17);
            this.chkForce.TabIndex = 9;
            this.chkForce.Text = "force";
            this.chkForce.UseVisualStyleBackColor = true;
            this.chkForce.CheckedChanged += new System.EventHandler(this.chkForce_CheckedChanged);
            // 
            // ReverseCryptoDrive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 322);
            this.Controls.Add(this.chkForce);
            this.Controls.Add(this.textBoxLogging);
            this.Controls.Add(this.buttonUnmount);
            this.Controls.Add(this.buttonMount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(470, 270);
            this.Name = "ReverseCryptoDrive";
            this.Text = "ReverseCryptoDrive";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReverseCryptoDrive_FormClosing);
            this.Load += new System.EventHandler(this.ReverseCryptoDrive_Load);
            this.Resize += new System.EventHandler(this.ReverseCryptoDrive_Resize);
            this.tnaMenu.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus;
        private System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBoxMountPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox driveLetterBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonMount;
        private System.Windows.Forms.Button buttonUnmount;
        private System.Windows.Forms.ContextMenuStrip tnaMenu;
        private System.Windows.Forms.ToolStripMenuItem mountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unmountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TextBox textBoxLogging;
        private System.Windows.Forms.CheckBox chkForce;
    }
}

