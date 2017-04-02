/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 04/01/2017
 * Time: 01:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace ShiftChanges.Settings.UI
{
	partial class SettingsForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Authentication");
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Folders");
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Application Settings", new System.Windows.Forms.TreeNode[] {
			treeNode1,
			treeNode2});
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// treeView1
			// 
			this.treeView1.Location = new System.Drawing.Point(5, 5);
			this.treeView1.Name = "treeView1";
			treeNode1.Name = "Node1";
			treeNode1.Tag = "ShiftChanges.Settings.UI.AuthenticationSettingsPanel";
			treeNode1.Text = "Authentication";
			treeNode2.Name = "Node2";
			treeNode2.Tag = "ShiftChanges.Settings.UI.FoldersSettingsPanel";
			treeNode2.Text = "Folders";
			treeNode3.Name = "Node0";
			treeNode3.Text = "Application Settings";
			this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
			treeNode3});
			this.treeView1.Size = new System.Drawing.Size(124, 105);
			this.treeView1.TabIndex = 0;
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1AfterSelect);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(358, 116);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(439, 116);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.Button2Click);
			// 
			// SettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(518, 144);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.treeView1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = global::ShiftChanges.Resources.SettingsIcon;
			this.MaximizeBox = false;
			this.Name = "SettingsForm";
			this.Text = "Settings";
			this.ResumeLayout(false);

		}
	}
}
