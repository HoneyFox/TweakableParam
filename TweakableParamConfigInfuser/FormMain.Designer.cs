namespace TweakableParamConfigInfuser
{
	partial class FormMain
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.lblPath = new System.Windows.Forms.Label();
			this.btnSelectPath = new System.Windows.Forms.Button();
			this.chkBoxList = new System.Windows.Forms.CheckedListBox();
			this.btnApply = new System.Windows.Forms.Button();
			this.folderDlg = new System.Windows.Forms.FolderBrowserDialog();
			this.btnCheckAll = new System.Windows.Forms.Button();
			this.btnCheckNone = new System.Windows.Forms.Button();
			this.buttonOutputMMConfig = new System.Windows.Forms.Button();
			this.txtMMCfg = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// lblPath
			// 
			this.lblPath.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblPath.Location = new System.Drawing.Point(12, 9);
			this.lblPath.Name = "lblPath";
			this.lblPath.Size = new System.Drawing.Size(376, 19);
			this.lblPath.TabIndex = 0;
			this.lblPath.Text = "Select KSP Parts Path...";
			// 
			// btnSelectPath
			// 
			this.btnSelectPath.Location = new System.Drawing.Point(394, 8);
			this.btnSelectPath.Name = "btnSelectPath";
			this.btnSelectPath.Size = new System.Drawing.Size(54, 21);
			this.btnSelectPath.TabIndex = 1;
			this.btnSelectPath.Text = "...";
			this.btnSelectPath.UseVisualStyleBackColor = true;
			this.btnSelectPath.Click += new System.EventHandler(this.btnSelectPath_Click);
			// 
			// chkBoxList
			// 
			this.chkBoxList.FormattingEnabled = true;
			this.chkBoxList.Location = new System.Drawing.Point(12, 36);
			this.chkBoxList.Name = "chkBoxList";
			this.chkBoxList.ScrollAlwaysVisible = true;
			this.chkBoxList.Size = new System.Drawing.Size(435, 228);
			this.chkBoxList.TabIndex = 2;
			// 
			// btnApply
			// 
			this.btnApply.Location = new System.Drawing.Point(12, 300);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(215, 40);
			this.btnApply.TabIndex = 3;
			this.btnApply.Text = "Add TweakableParam Capability to Selected Parts";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// btnCheckAll
			// 
			this.btnCheckAll.Location = new System.Drawing.Point(12, 270);
			this.btnCheckAll.Name = "btnCheckAll";
			this.btnCheckAll.Size = new System.Drawing.Size(215, 25);
			this.btnCheckAll.TabIndex = 4;
			this.btnCheckAll.Text = "Check All";
			this.btnCheckAll.UseVisualStyleBackColor = true;
			this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
			// 
			// btnCheckNone
			// 
			this.btnCheckNone.Location = new System.Drawing.Point(233, 270);
			this.btnCheckNone.Name = "btnCheckNone";
			this.btnCheckNone.Size = new System.Drawing.Size(215, 25);
			this.btnCheckNone.TabIndex = 5;
			this.btnCheckNone.Text = "Check None";
			this.btnCheckNone.UseVisualStyleBackColor = true;
			this.btnCheckNone.Click += new System.EventHandler(this.btnCheckNone_Click);
			// 
			// buttonOutputMMConfig
			// 
			this.buttonOutputMMConfig.Location = new System.Drawing.Point(233, 300);
			this.buttonOutputMMConfig.Name = "buttonOutputMMConfig";
			this.buttonOutputMMConfig.Size = new System.Drawing.Size(215, 40);
			this.buttonOutputMMConfig.TabIndex = 6;
			this.buttonOutputMMConfig.Text = "Generate MM Config";
			this.buttonOutputMMConfig.UseVisualStyleBackColor = true;
			this.buttonOutputMMConfig.Click += new System.EventHandler(this.buttonOutputMMConfig_Click);
			// 
			// txtMMCfg
			// 
			this.txtMMCfg.AcceptsReturn = true;
			this.txtMMCfg.AcceptsTab = true;
			this.txtMMCfg.Location = new System.Drawing.Point(12, 346);
			this.txtMMCfg.Multiline = true;
			this.txtMMCfg.Name = "txtMMCfg";
			this.txtMMCfg.ReadOnly = true;
			this.txtMMCfg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtMMCfg.Size = new System.Drawing.Size(435, 145);
			this.txtMMCfg.TabIndex = 7;
			this.txtMMCfg.WordWrap = false;
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(456, 496);
			this.Controls.Add(this.txtMMCfg);
			this.Controls.Add(this.buttonOutputMMConfig);
			this.Controls.Add(this.btnCheckNone);
			this.Controls.Add(this.btnCheckAll);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.chkBoxList);
			this.Controls.Add(this.btnSelectPath);
			this.Controls.Add(this.lblPath);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormMain";
			this.Text = "TweakableParam Configuration Infuser";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblPath;
		private System.Windows.Forms.Button btnSelectPath;
		private System.Windows.Forms.CheckedListBox chkBoxList;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.FolderBrowserDialog folderDlg;
		private System.Windows.Forms.Button btnCheckAll;
		private System.Windows.Forms.Button btnCheckNone;
		private System.Windows.Forms.Button buttonOutputMMConfig;
		private System.Windows.Forms.TextBox txtMMCfg;
	}
}

