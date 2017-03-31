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
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Authentication");
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Folders");
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Application Settings", new System.Windows.Forms.TreeNode[] {
			treeNode4,
			treeNode5});
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// treeView1
			// 
			this.treeView1.Location = new System.Drawing.Point(12, 12);
			this.treeView1.Name = "treeView1";
			treeNode4.Name = "Node1";
			treeNode4.Text = "Authentication";
			treeNode5.Name = "Node2";
			treeNode5.Text = "Folders";
			treeNode6.Name = "Node0";
			treeNode6.Text = "Application Settings";
			this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
			treeNode6});
			this.treeView1.Size = new System.Drawing.Size(149, 316);
			this.treeView1.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(362, 338);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(443, 338);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// SettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(530, 373);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.treeView1);
			this.Icon = global::ShiftChanges.Resources.SettingsIcon;
			this.Name = "SettingsForm";
			this.Text = "SettingsForm";
			this.ResumeLayout(false);

		}
	}
}
