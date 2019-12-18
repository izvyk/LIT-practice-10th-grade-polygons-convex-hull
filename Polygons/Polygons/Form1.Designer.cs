namespace Polygons
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
            this.shapeMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileStripMenuSaveItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileStripMenuLoadItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shapeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorStripMenuVerticesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorStripMenuLinesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorStripMenuBackgroundItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeStripMenuVerticesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeStripMenuLinesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dynamicsButton = new System.Windows.Forms.Button();
            this.shapeMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // shapeMenuStrip
            // 
            this.shapeMenuStrip.BackColor = System.Drawing.Color.WhiteSmoke;
            this.shapeMenuStrip.Font = new System.Drawing.Font("Source Sans Pro Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.shapeMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.shapeMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.shapeToolStripMenuItem,
            this.colorToolStripMenuItem,
            this.resizeToolStripMenuItem});
            this.shapeMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.shapeMenuStrip.Name = "shapeMenuStrip";
            this.shapeMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.shapeMenuStrip.Size = new System.Drawing.Size(800, 27);
            this.shapeMenuStrip.TabIndex = 0;
            this.shapeMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileStripMenuSaveItem,
            this.fileStripMenuLoadItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(47, 23);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // fileStripMenuSaveItem
            // 
            this.fileStripMenuSaveItem.Name = "fileStripMenuSaveItem";
            this.fileStripMenuSaveItem.Size = new System.Drawing.Size(124, 26);
            this.fileStripMenuSaveItem.Text = "Save";
            this.fileStripMenuSaveItem.Click += new System.EventHandler(this.FileStripMenuSave_Click);
            // 
            // fileStripMenuLoadItem
            // 
            this.fileStripMenuLoadItem.Name = "fileStripMenuLoadItem";
            this.fileStripMenuLoadItem.Size = new System.Drawing.Size(124, 26);
            this.fileStripMenuLoadItem.Text = "Load";
            this.fileStripMenuLoadItem.Click += new System.EventHandler(this.FileStripMenuLoad_Click);
            // 
            // shapeToolStripMenuItem
            // 
            this.shapeToolStripMenuItem.CheckOnClick = true;
            this.shapeToolStripMenuItem.Name = "shapeToolStripMenuItem";
            this.shapeToolStripMenuItem.Size = new System.Drawing.Size(63, 23);
            this.shapeToolStripMenuItem.Text = "Shape";
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorStripMenuVerticesItem,
            this.colorStripMenuLinesItem,
            this.colorStripMenuBackgroundItem});
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(58, 23);
            this.colorToolStripMenuItem.Text = "Color";
            // 
            // colorStripMenuVerticesItem
            // 
            this.colorStripMenuVerticesItem.Name = "colorStripMenuVerticesItem";
            this.colorStripMenuVerticesItem.Size = new System.Drawing.Size(170, 26);
            this.colorStripMenuVerticesItem.Text = "Vertices";
            this.colorStripMenuVerticesItem.Click += new System.EventHandler(this.ColorStripMenuVertices_Click);
            // 
            // colorStripMenuLinesItem
            // 
            this.colorStripMenuLinesItem.Name = "colorStripMenuLinesItem";
            this.colorStripMenuLinesItem.Size = new System.Drawing.Size(170, 26);
            this.colorStripMenuLinesItem.Text = "Lines";
            this.colorStripMenuLinesItem.Click += new System.EventHandler(this.ColorStripMenuLines_Click);
            // 
            // colorStripMenuBackgroundItem
            // 
            this.colorStripMenuBackgroundItem.Name = "colorStripMenuBackgroundItem";
            this.colorStripMenuBackgroundItem.Size = new System.Drawing.Size(170, 26);
            this.colorStripMenuBackgroundItem.Text = "Background";
            this.colorStripMenuBackgroundItem.Click += new System.EventHandler(this.ColorStripMenuBackground_Click);
            // 
            // resizeToolStripMenuItem
            // 
            this.resizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resizeStripMenuVerticesItem,
            this.resizeStripMenuLinesItem});
            this.resizeToolStripMenuItem.Name = "resizeToolStripMenuItem";
            this.resizeToolStripMenuItem.Size = new System.Drawing.Size(65, 23);
            this.resizeToolStripMenuItem.Text = "Resize";
            // 
            // resizeStripMenuVerticesItem
            // 
            this.resizeStripMenuVerticesItem.Name = "resizeStripMenuVerticesItem";
            this.resizeStripMenuVerticesItem.Size = new System.Drawing.Size(144, 26);
            this.resizeStripMenuVerticesItem.Text = "Vertices";
            this.resizeStripMenuVerticesItem.Click += new System.EventHandler(this.ResizeStripMenuVertices_Click);
            // 
            // resizeStripMenuLinesItem
            // 
            this.resizeStripMenuLinesItem.Name = "resizeStripMenuLinesItem";
            this.resizeStripMenuLinesItem.Size = new System.Drawing.Size(144, 26);
            this.resizeStripMenuLinesItem.Text = "Lines";
            this.resizeStripMenuLinesItem.Click += new System.EventHandler(this.ResizeStripMenuLines_Click);
            // 
            // dynamicsButton
            // 
            this.dynamicsButton.Location = new System.Drawing.Point(262, 0);
            this.dynamicsButton.Name = "dynamicsButton";
            this.dynamicsButton.Size = new System.Drawing.Size(28, 28);
            this.dynamicsButton.TabIndex = 5;
            this.dynamicsButton.UseVisualStyleBackColor = true;
            this.dynamicsButton.Click += new System.EventHandler(this.DynamicsButton_Click);
            this.dynamicsButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DynamicsButton_Paint);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dynamicsButton);
            this.Controls.Add(this.shapeMenuStrip);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MainMenuStrip = this.shapeMenuStrip;
            this.Name = "Form1";
            this.Text = "Polygons";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            this.shapeMenuStrip.ResumeLayout(false);
            this.shapeMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button dynamicsButton;
        private System.Windows.Forms.MenuStrip shapeMenuStrip;
        // shape menu
        private System.Windows.Forms.ToolStripMenuItem shapeToolStripMenuItem;
        // color menu
        private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorStripMenuVerticesItem;
        private System.Windows.Forms.ToolStripMenuItem colorStripMenuLinesItem;
        private System.Windows.Forms.ToolStripMenuItem colorStripMenuBackgroundItem;
        // resize menu
        private System.Windows.Forms.ToolStripMenuItem resizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resizeStripMenuVerticesItem;
        private System.Windows.Forms.ToolStripMenuItem resizeStripMenuLinesItem;
        // file menu
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileStripMenuSaveItem;
        private System.Windows.Forms.ToolStripMenuItem fileStripMenuLoadItem;
    }
}

