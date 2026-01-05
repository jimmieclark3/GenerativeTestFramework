using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data;
using VBNET = Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace FileExplorer
{
   #region Windows Form Designer generated code
   partial class frmExploreLite
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;
      private System.Windows.Forms.RichTextBox RichTextBox1;
      private System.Windows.Forms.ToolStrip Toolbar1;
      private System.Windows.Forms.ToolStripButton Toolbar1_Button1;
      private System.Windows.Forms.ToolStripButton Toolbar1_Button2;
      private System.Windows.Forms.ToolStripSeparator Toolbar1_Button3;
      private System.Windows.Forms.ToolStripButton Toolbar1_Button4;
      private System.Windows.Forms.ToolStripButton Toolbar1_Button5;
      private System.Windows.Forms.ToolStripSeparator Toolbar1_Button6;
      private System.Windows.Forms.ToolStripDropDownButton Toolbar1_Button7;
      private System.Windows.Forms.ToolStripMenuItem Toolbar1_Button7_ButtonMenu1;
      private System.Windows.Forms.ToolStripMenuItem Toolbar1_Button7_ButtonMenu2;
      private System.Windows.Forms.ToolStripMenuItem Toolbar1_Button7_ButtonMenu3;
      private System.Windows.Forms.ToolStripButton Toolbar1_Button8;
      private System.Windows.Forms.PictureBox picSplitter;
      private System.Windows.Forms.ImageList ilSmall;
      private System.Windows.Forms.ImageList ilMain;
      private System.Windows.Forms.ListView lvListView;
      private System.Windows.Forms.ColumnHeader lvListView_ColumnHeader1;
      private System.Windows.Forms.ColumnHeader lvListView_ColumnHeader2;
      private System.Windows.Forms.ColumnHeader lvListView_ColumnHeader3;
      private System.Windows.Forms.ColumnHeader lvListView_ColumnHeader4;
      private System.Windows.Forms.ColumnHeader lvListView_ColumnHeader5;
      private System.Windows.Forms.TreeView tvTreeView;
      private System.Windows.Forms.StatusStrip sbStatusBar;
      private System.Windows.Forms.ToolStripStatusLabel sbStatusBar_Panel1;
      private System.Windows.Forms.ToolStripStatusLabel sbStatusBar_Panel2;
      private System.Windows.Forms.ToolStripStatusLabel sbStatusBar_Panel3;
      private System.Windows.Forms.ToolStripStatusLabel sbStatusBar_Panel4;
      private System.Windows.Forms.PictureBox imgSplitter;
      private System.Windows.Forms.Label lblPath;
      private System.Windows.Forms.Label Label1;
      private System.Windows.Forms.MenuStrip mainMenu1;
      private System.Windows.Forms.ToolStripMenuItem mnu_File;
      private System.Windows.Forms.ToolStripMenuItem mnu_FileExit;

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExploreLite));
         this.components = new System.ComponentModel.Container();
         this.RichTextBox1 = new System.Windows.Forms.RichTextBox();
         this.Toolbar1 = new System.Windows.Forms.ToolStrip();
         this.Toolbar1_Button1 = new System.Windows.Forms.ToolStripButton();
         this.Toolbar1_Button2 = new System.Windows.Forms.ToolStripButton();
         this.Toolbar1_Button3 = new System.Windows.Forms.ToolStripSeparator();
         this.Toolbar1_Button4 = new System.Windows.Forms.ToolStripButton();
         this.Toolbar1_Button5 = new System.Windows.Forms.ToolStripButton();
         this.Toolbar1_Button6 = new System.Windows.Forms.ToolStripSeparator();
         this.Toolbar1_Button7 = new System.Windows.Forms.ToolStripDropDownButton();
         this.Toolbar1_Button7_ButtonMenu1 = new System.Windows.Forms.ToolStripMenuItem();
         this.Toolbar1_Button7_ButtonMenu2 = new System.Windows.Forms.ToolStripMenuItem();
         this.Toolbar1_Button7_ButtonMenu3 = new System.Windows.Forms.ToolStripMenuItem();
         this.Toolbar1_Button8 = new System.Windows.Forms.ToolStripButton();
         this.picSplitter = new System.Windows.Forms.PictureBox();
         this.ilSmall = new System.Windows.Forms.ImageList(this.components);
         this.ilMain = new System.Windows.Forms.ImageList(this.components);
         this.lvListView = new System.Windows.Forms.ListView();
         this.lvListView_ColumnHeader1 = new System.Windows.Forms.ColumnHeader();
         this.lvListView_ColumnHeader2 = new System.Windows.Forms.ColumnHeader();
         this.lvListView_ColumnHeader3 = new System.Windows.Forms.ColumnHeader();
         this.lvListView_ColumnHeader4 = new System.Windows.Forms.ColumnHeader();
         this.lvListView_ColumnHeader5 = new System.Windows.Forms.ColumnHeader();
         this.tvTreeView = new System.Windows.Forms.TreeView();
         this.sbStatusBar = new System.Windows.Forms.StatusStrip();
         this.sbStatusBar_Panel1 = new System.Windows.Forms.ToolStripStatusLabel();
         this.sbStatusBar_Panel2 = new System.Windows.Forms.ToolStripStatusLabel();
         this.sbStatusBar_Panel3 = new System.Windows.Forms.ToolStripStatusLabel();
         this.sbStatusBar_Panel4 = new System.Windows.Forms.ToolStripStatusLabel();
         this.imgSplitter = new System.Windows.Forms.PictureBox();
         this.lblPath = new System.Windows.Forms.Label();
         this.Label1 = new System.Windows.Forms.Label();
         this.mainMenu1 = new System.Windows.Forms.MenuStrip();
         this.mnu_File = new System.Windows.Forms.ToolStripMenuItem();
         this.mnu_FileExit = new System.Windows.Forms.ToolStripMenuItem();
         this.SuspendLayout();
         //
         // RichTextBox1
         //
         this.RichTextBox1.BackColor = System.Drawing.SystemColors.Window;
         this.RichTextBox1.Enabled = true;
         this.RichTextBox1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.RichTextBox1.Location = new System.Drawing.Point(216, 304);
         this.RichTextBox1.Name = "RichTextBox1";
         this.RichTextBox1.Size = new System.Drawing.Size(433, 129);
         this.RichTextBox1.TabIndex = 7;
         this.RichTextBox1.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Consolas;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 \r\n\\par }\r\n";
         this.RichTextBox1.TabStop = true;
         this.RichTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RichTextBox1_KeyDown);
         //
         // Toolbar1
         //
         this.Toolbar1.Font = new System.Drawing.Font("MS Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.Toolbar1.ImageList = ilMain;
         this.Toolbar1.Location = new System.Drawing.Point(0, 24);
         this.Toolbar1.Name = "Toolbar1";
         this.Toolbar1.Size = new System.Drawing.Size(759, 40);
         this.Toolbar1.TabIndex = 6;
         this.Toolbar1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Toolbar1_ButtonClick);
         //
         // Toolbar1_Button1
         //
         this.Toolbar1_Button1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
         this.Toolbar1_Button1.ImageIndex = 6;
         this.Toolbar1_Button1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
         this.Toolbar1_Button1.Name = "up";
         this.Toolbar1_Button1.Size = new System.Drawing.Size(69, 67);
         this.Toolbar1_Button1.Text = "up";
         this.Toolbar1_Button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.Toolbar1_Button1.Visible = true;
         //
         // Toolbar1_Button2
         //
         this.Toolbar1_Button2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
         this.Toolbar1_Button2.ImageIndex = 5;
         this.Toolbar1_Button2.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
         this.Toolbar1_Button2.Name = "down";
         this.Toolbar1_Button2.Size = new System.Drawing.Size(69, 67);
         this.Toolbar1_Button2.Text = "down";
         this.Toolbar1_Button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.Toolbar1_Button2.Visible = true;
         //
         // Toolbar1_Button3
         //
         this.Toolbar1_Button3.Name = "Toolbar1_Button3";
         //
         // Toolbar1_Button4
         //
         this.Toolbar1_Button4.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
         this.Toolbar1_Button4.ImageIndex = 1;
         this.Toolbar1_Button4.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
         this.Toolbar1_Button4.Name = "open";
         this.Toolbar1_Button4.Size = new System.Drawing.Size(69, 67);
         this.Toolbar1_Button4.Text = "open";
         this.Toolbar1_Button4.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.Toolbar1_Button4.Visible = true;
         //
         // Toolbar1_Button5
         //
         this.Toolbar1_Button5.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
         this.Toolbar1_Button5.ImageIndex = 0;
         this.Toolbar1_Button5.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
         this.Toolbar1_Button5.Name = "close";
         this.Toolbar1_Button5.Size = new System.Drawing.Size(69, 67);
         this.Toolbar1_Button5.Text = "close";
         this.Toolbar1_Button5.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.Toolbar1_Button5.Visible = true;
         //
         // Toolbar1_Button6
         //
         this.Toolbar1_Button6.Name = "Toolbar1_Button6";
         //
         // Toolbar1_Button7
         //
         this.Toolbar1_Button7.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
         this.Toolbar1_Button7.ImageIndex = 4;
         this.Toolbar1_Button7.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
         this.Toolbar1_Button7.Name = "Toolbar1_Button7";
         this.Toolbar1_Button7.Size = new System.Drawing.Size(69, 67);
         this.Toolbar1_Button7.Text = "tagit";
         this.Toolbar1_Button7.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.Toolbar1_Button7.Visible = true;
         //
         // Toolbar1_Button7_ButtonMenu1
         //
         this.Toolbar1_Button7_ButtonMenu1.Name = "Toolbar1_Button7_ButtonMenu1";
         this.Toolbar1_Button7_ButtonMenu1.Text = "ahoy";
         //
         // Toolbar1_Button7_ButtonMenu2
         //
         this.Toolbar1_Button7_ButtonMenu2.Name = "Toolbar1_Button7_ButtonMenu2";
         this.Toolbar1_Button7_ButtonMenu2.Text = "there";
         //
         // Toolbar1_Button7_ButtonMenu3
         //
         this.Toolbar1_Button7_ButtonMenu3.Name = "Toolbar1_Button7_ButtonMenu3";
         this.Toolbar1_Button7_ButtonMenu3.Text = "matey";
         //
         // Toolbar1_Button8
         //
         this.Toolbar1_Button8.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
         this.Toolbar1_Button8.ImageIndex = 3;
         this.Toolbar1_Button8.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
         this.Toolbar1_Button8.Name = "info";
         this.Toolbar1_Button8.Size = new System.Drawing.Size(69, 67);
         this.Toolbar1_Button8.Text = "info";
         this.Toolbar1_Button8.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.Toolbar1_Button8.Visible = true;
         //
         // picSplitter
         //
         this.picSplitter.BackColor = System.Drawing.Color.FromArgb(128, 128, 128);
         this.picSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.picSplitter.CausesValidation = true;
         this.picSplitter.Cursor = System.Windows.Forms.Cursors.Default;
         this.picSplitter.Dock = System.Windows.Forms.DockStyle.None;
         this.picSplitter.Enabled = true;
         this.picSplitter.Font = new System.Drawing.Font("MS Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.picSplitter.ForeColor = System.Drawing.SystemColors.ControlText;
         this.picSplitter.Location = new System.Drawing.Point(664, 88);
         this.picSplitter.Name = "picSplitter";
         this.picSplitter.RightToLeft = System.Windows.Forms.RightToLeft.No;
         this.picSplitter.Size = new System.Drawing.Size(2, 345);
         this.picSplitter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
         this.picSplitter.TabIndex = 5;
         this.picSplitter.TabStop = true;
         this.picSplitter.Visible = false;
         //
         // ilSmall
         //
         this.ilSmall.ImageSize = new System.Drawing.Size(16, 16);
         this.ilSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilSmall.ImageStream")));
         this.ilSmall.Images.SetKeyName(0, "genericSmall");
         this.ilSmall.Images.SetKeyName(1, "fldrClosed");
         this.ilSmall.TransparentColor = System.Drawing.Color.Transparent;
         //
         // ilMain
         //
         this.ilMain.ImageSize = new System.Drawing.Size(16, 16);
         this.ilMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilMain.ImageStream")));
         this.ilMain.Images.SetKeyName(0, "fldrClosed");
         this.ilMain.Images.SetKeyName(1, "fldrOpen");
         this.ilMain.Images.SetKeyName(2, "drive");
         this.ilMain.Images.SetKeyName(3, "explorer");
         this.ilMain.Images.SetKeyName(4, "genericLarge");
         this.ilMain.Images.SetKeyName(5, "down");
         this.ilMain.Images.SetKeyName(6, "up");
         this.ilMain.Images.SetKeyName(7, "genericMedium");
         this.ilMain.Images.SetKeyName(8, "genericSmall");
         this.ilMain.TransparentColor = System.Drawing.Color.Transparent;
         //
         // lvListView
         //
         this.lvListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
         this.lvListView.BackColor = System.Drawing.SystemColors.Window;
         this.lvListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.lvListView.Font = new System.Drawing.Font("MS Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.lvListView.ForeColor = System.Drawing.SystemColors.WindowText;
         this.lvListView.FullRowSelect = true;
         this.lvListView.GridLines = true;
         this.lvListView.HideSelection = false;
         this.lvListView.LabelEdit = false;
         this.lvListView.LargeImageList = ilMain;
         this.lvListView.Location = new System.Drawing.Point(216, 96);
         this.lvListView.MultiSelect = true;
         this.lvListView.Name = "lvListView";
         this.lvListView.Size = new System.Drawing.Size(433, 193);
         this.lvListView.SmallImageList = ilSmall;
         this.lvListView.Sorting = System.Windows.Forms.SortOrder.None;
         this.lvListView.TabIndex = 3;
         this.lvListView.UseCompatibleStateImageBehavior = false;
         this.lvListView.View = System.Windows.Forms.View.Details;
         this.lvListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvListView_ColumnClick);
         this.lvListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvListView_ItemClick);
         //
         // lvListView_ColumnHeader1
         //
         this.lvListView_ColumnHeader1.Text = "Name";
         this.lvListView_ColumnHeader1.Width = 169;
         //
         // lvListView_ColumnHeader2
         //
         this.lvListView_ColumnHeader2.Text = "Size";
         this.lvListView_ColumnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
         this.lvListView_ColumnHeader2.Width = 169;
         //
         // lvListView_ColumnHeader3
         //
         this.lvListView_ColumnHeader3.Text = "Type";
         this.lvListView_ColumnHeader3.Width = 169;
         //
         // lvListView_ColumnHeader4
         //
         this.lvListView_ColumnHeader4.Text = "Modified";
         this.lvListView_ColumnHeader4.Width = 169;
         //
         // lvListView_ColumnHeader5
         //
         this.lvListView_ColumnHeader5.Text = "Tag";
         this.lvListView_ColumnHeader5.Width = 169;
         //
         // tvTreeView
         //
         this.tvTreeView.Enabled = true;
         this.tvTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.tvTreeView.FullRowSelect = false;
         this.tvTreeView.ImageList = ilMain;
         this.tvTreeView.ItemHeight = 16;
         this.tvTreeView.Indent = 35;
         this.tvTreeView.LabelEdit = false;
         this.tvTreeView.Location = new System.Drawing.Point(0, 96);
         this.tvTreeView.Name = "tvTreeView";
         this.tvTreeView.Size = new System.Drawing.Size(208, 344);
         this.tvTreeView.TabIndex = 0;
         this.tvTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvTreeView_NodeClick);
         //
         // sbStatusBar
         //
         this.sbStatusBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.sbStatusBar.Location = new System.Drawing.Point(0, 534);
         this.sbStatusBar.Name = "sbStatusBar";
         this.sbStatusBar.Size = new System.Drawing.Size(759, 19);
         this.sbStatusBar.TabIndex = 1;
         this.sbStatusBar.Visible = true;
         //
         // sbStatusBar_Panel1
         //
         this.sbStatusBar_Panel1.AutoSize = false;
         this.sbStatusBar_Panel1.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)
            ((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.sbStatusBar_Panel1.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
         this.sbStatusBar_Panel1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.sbStatusBar_Panel1.Name = "sbStatusBar_Panel1";
         this.sbStatusBar_Panel1.Size = new System.Drawing.Size(91, 19);
         this.sbStatusBar_Panel1.Spring = true;
         this.sbStatusBar_Panel1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
         //
         // sbStatusBar_Panel2
         //
         this.sbStatusBar_Panel2.AutoSize = false;
         this.sbStatusBar_Panel2.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)
            ((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.sbStatusBar_Panel2.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
         this.sbStatusBar_Panel2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.sbStatusBar_Panel2.Name = "sbStatusBar_Panel2";
         this.sbStatusBar_Panel2.Size = new System.Drawing.Size(56, 19);
         this.sbStatusBar_Panel2.Text = "TIME";
         this.sbStatusBar_Panel2.TextAlign = System.Drawing.ContentAlignment.TopLeft;
         //
         // sbStatusBar_Panel3
         //
         this.sbStatusBar_Panel3.AutoSize = false;
         this.sbStatusBar_Panel3.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)
            ((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.sbStatusBar_Panel3.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
         this.sbStatusBar_Panel3.Image = (System.Drawing.Image)resources.GetObject("FrxData3715.Picture");
         this.sbStatusBar_Panel3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.sbStatusBar_Panel3.Name = "sbStatusBar_Panel3";
         this.sbStatusBar_Panel3.Size = new System.Drawing.Size(100, 19);
         this.sbStatusBar_Panel3.TextAlign = System.Drawing.ContentAlignment.TopLeft;
         //
         // sbStatusBar_Panel4
         //
         this.sbStatusBar_Panel4.AutoSize = false;
         this.sbStatusBar_Panel4.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)
            ((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
               | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
         this.sbStatusBar_Panel4.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
         this.sbStatusBar_Panel4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.sbStatusBar_Panel4.Name = "sbStatusBar_Panel4";
         this.sbStatusBar_Panel4.Size = new System.Drawing.Size(100, 19);
         this.sbStatusBar_Panel4.Text = "DATE";
         this.sbStatusBar_Panel4.TextAlign = System.Drawing.ContentAlignment.TopLeft;
         //
         // imgSplitter
         //
         this.imgSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.imgSplitter.Cursor = System.Windows.Forms.Cursors.SizeWE;
         this.imgSplitter.Location = new System.Drawing.Point(208, 120);
         this.imgSplitter.Name = "imgSplitter";
         this.imgSplitter.Size = new System.Drawing.Size(10, 344);
         this.imgSplitter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imgSplitter_MouseDown);
         this.imgSplitter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imgSplitter_MouseMove);
         this.imgSplitter.MouseUp += new System.Windows.Forms.MouseEventHandler(this.imgSplitter_MouseUp);
         //
         // lblPath
         //
         this.lblPath.AutoSize = false;
         this.lblPath.BackColor = System.Drawing.SystemColors.Control;
         this.lblPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.lblPath.Cursor = System.Windows.Forms.Cursors.Default;
         this.lblPath.Enabled = true;
         this.lblPath.FlatStyle = System.Windows.Forms.FlatStyle.System;
         this.lblPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.lblPath.ForeColor = System.Drawing.SystemColors.ControlText;
         this.lblPath.Location = new System.Drawing.Point(215, 80);
         this.lblPath.Name = "lblPath";
         this.lblPath.RightToLeft = System.Windows.Forms.RightToLeft.No;
         this.lblPath.Size = new System.Drawing.Size(431, 13);
         this.lblPath.Text = "File info";
         this.lblPath.TextAlign = System.Drawing.ContentAlignment.TopLeft;
         this.lblPath.UseMnemonic = true;
         this.lblPath.Visible = true;
         //
         // Label1
         //
         this.Label1.AutoSize = false;
         this.Label1.BackColor = System.Drawing.SystemColors.Control;
         this.Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
         this.Label1.Enabled = true;
         this.Label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
         this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (System.Byte)(0));
         this.Label1.ForeColor = System.Drawing.SystemColors.ControlText;
         this.Label1.Location = new System.Drawing.Point(4, 80);
         this.Label1.Name = "Label1";
         this.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
         this.Label1.Size = new System.Drawing.Size(204, 13);
         this.Label1.Text = "Folders";
         this.Label1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
         this.Label1.UseMnemonic = true;
         this.Label1.Visible = true;
         //
         // mnu_File
         //
         this.mnu_File.Name = "mnu_File";
         this.mnu_File.Text = "&File";
         //
         // mnu_FileExit
         //
         this.mnu_FileExit.Name = "mnu_FileExit";
         this.mnu_FileExit.Text = "E&xit";
         this.mnu_FileExit.Click += new System.EventHandler(this.mnu_FileExit_Click);
         //
         // frmExploreLite
         //
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(759, 553);
         this.Controls.AddRange(new System.Windows.Forms.Control[] {
            this.RichTextBox1,
            this.Toolbar1,
            this.picSplitter,
            this.lvListView,
            this.tvTreeView,
            this.sbStatusBar,
            this.imgSplitter,
            this.lblPath,
            this.Label1
         });
         this.Toolbar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Toolbar1_Button1,
            this.Toolbar1_Button2,
            this.Toolbar1_Button3,
            this.Toolbar1_Button4,
            this.Toolbar1_Button5,
            this.Toolbar1_Button6,
            this.Toolbar1_Button7,
            this.Toolbar1_Button8
         });
         this.lvListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvListView_ColumnHeader1,
            this.lvListView_ColumnHeader2,
            this.lvListView_ColumnHeader3,
            this.lvListView_ColumnHeader4,
            this.lvListView_ColumnHeader5
         });
         this.sbStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sbStatusBar_Panel1,
            this.sbStatusBar_Panel2,
            this.sbStatusBar_Panel3,
            this.sbStatusBar_Panel4
         });
         this.Toolbar1_Button7.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Toolbar1_Button7_ButtonMenu1,
            this.Toolbar1_Button7_ButtonMenu2,
            this.Toolbar1_Button7_ButtonMenu3
         });
         this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {this.mnu_File});
         this.mnu_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu_FileExit
         });
         this.MainMenuStrip = this.mainMenu1;
         this.mainMenu1.AutoSize = false;
         this.Controls.Add(this.mainMenu1);
         this.Name = "frmExploreLite";
         this.Text = "File Explorer";
         this.Icon = (System.Drawing.Icon)resources.GetObject("FrxData0000.Icon");
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Resize += new System.EventHandler(this.Form_Resize);

         this.ResumeLayout(false);
         this.PerformLayout();
      }
   }
   #endregion
}
