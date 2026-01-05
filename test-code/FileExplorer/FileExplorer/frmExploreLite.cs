using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data;
using VBNET = Microsoft.VisualBasic;
using gmRTL.Core;
using gmRTL.MSComCtl;
using Microsoft.VisualBasic.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace FileExplorer
{
   public partial class frmExploreLite : System.Windows.Forms.Form
   {
      private bool _disposed = false;
      public frmExploreLite()
      {
         //
         // Required for Windows Form Designer support
         //
         InitializeComponent();

         //
         // TODO: Add any constructor code after InitializeComponent call
         //
         if (DesignMode) return;
         this.Toolbar1_Button1.Name = "up";
         this.Toolbar1_Button2.Name = "down";
         this.Toolbar1_Button4.Name = "open";
         this.Toolbar1_Button5.Name = "close";
         this.Toolbar1_Button8.Name = "info";
         Form_Load(null, null);
         Form_Resize(null, null);
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (_disposed)
         {
            return;
         }
         if (disposing)
         {
            if (components != null)
            {
               components.Dispose();
            }
         }
         _disposed = true;
         base.Dispose(disposing);
      }

      private bool mIsMoving = false;     //  Is splitter in motion?
      private const int mSplitLimit = (2000)/15;     //  Minimum width for TreeView and ListView
      private void Form_Load(object sender, EventArgs e)
      {
         System.IO.DirectoryInfo currFldr = null;
         System.Windows.Forms.TreeNode nodRoot = null;


         //  Set column widths
         lvListView.Columns[0].Width = 186;
         lvListView.Columns[1].Width = 72;
         lvListView.Columns[2].Width = 67;
         lvListView.Columns[3].Width = 79;
         lvListView.Columns[4].Width = 133;

         //  Populate level 1 of the TreeView
         DoHourglass(true);
         foreach(string drvTemp in System.IO.Directory.GetLogicalDrives())
         {
            System.IO.DirectoryInfo drv = new System.IO.DirectoryInfo(drvTemp);
            //  Proceed only if the drive is ready.
            //  This ignores an empty CD drive, for example.
            if (drv.Exists)
            {
               nodRoot = tvTreeView.Nodes.Add(drv.Root.FullName,drv.Root.FullName.Substring(0,1) + ":\\");
               nodRoot.ImageKey = "drive";
               nodRoot.SelectedImageKey = "drive";
               nodRoot.TreeView.Sort(); // = true;

               //  Get a pointer to the root folder
               currFldr = drv.Root;

               //  Populate level 2 of the TreeView
               foreach(System.IO.DirectoryInfo fldr in currFldr.GetDirectories())
               {
                  AddTVNode(nodRoot,drv.Root.FullName.Substring(0,1) + "\\" + fldr.Name,fldr.Name);
               }
            }
         }

         //  Set up the first node
         nodRoot = tvTreeView.Nodes[0];
         nodRoot.EnsureVisible();
         nodRoot.TreeView.SelectedNode = nodRoot;
         nodRoot.Expand();
         lblPath.Text = nodRoot.FullPath;
         tvTreeView_NodeClick(null,new TreeNodeMouseClickEventArgs(nodRoot,new MouseButtons(),0,0,0));

         DoHourglass(false);
         System.Windows.Forms.Application.DoEvents();
      }
      private void Form_Resize(object sender, EventArgs e)
      {
         if (this.ClientRectangle.Width == 0)
         {
            return;
         }
         if (this.Width < 200)
         {
            this.Width = 200;
         }
         if (this.Height < 200)
         {
            this.Height = 200;
         }

         int formWidth = 0;
         formWidth = this.ClientRectangle.Width;

         tvTreeView.Height = this.ClientRectangle.Height - tvTreeView.Top - sbStatusBar.Height - 1;

         lvListView.Width = formWidth - tvTreeView.Width - 8;
         lvListView.Height = ((int)(tvTreeView.Height * 0.5));

         RichTextBox1.Width = formWidth - tvTreeView.Width - 8;
         RichTextBox1.Top = lvListView.Top + lvListView.Height + 1;
         RichTextBox1.Height = ((int)(tvTreeView.Height - (lvListView.Height + 1)));

         imgSplitter.Top = tvTreeView.Top + 2;
         imgSplitter.Height = tvTreeView.Height - 4;
         SizeControls(imgSplitter.Left);

      }
      private void imgSplitter_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         //  Use the PictureBox as a marker for the new split location
         picSplitter.SetBounds(imgSplitter.Left,imgSplitter.Top,imgSplitter.Width / 4,imgSplitter.Height - 1);
         picSplitter.Visible = true;
         mIsMoving = true;
      }
      private void imgSplitter_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         float X;
         X = e.X;
         float sglPos = 0.0F;

         if (mIsMoving)
         {
            sglPos = X + imgSplitter.Left;
            if (sglPos < mSplitLimit)
            {
               picSplitter.Left = mSplitLimit;
            }
            else if (sglPos > this.Width - mSplitLimit)
            {
               picSplitter.Left = this.Width - mSplitLimit;
            }
            else
            {
               picSplitter.Left = ((int)sglPos);
            }
         }
      }
      private void imgSplitter_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         SizeControls(picSplitter.Left);
         picSplitter.Visible = false;
         mIsMoving = false;
      }
      private void lvListView_ColumnClick(object sender, ColumnClickEventArgs e)
      {
         System.Windows.Forms.ColumnHeader ColumnHeader;
         ColumnHeader = ((ListView)(sender)).Columns[e.Column];

         // Sort the columnn that the user clicked.
         if (lvListView.GetSortKey() != ColumnHeader.Index - 1)
         {
            lvListView.SetSortKey(ColumnHeader.Index - 1);
            lvListView.ListViewItemSorter = new gmRTL.MSComCtl.ListViewSorter(lvListView.SetSortOrder(System.Windows.Forms.SortOrder.Ascending), lvListView.GetSortKey());
         }
         else
         {
            if (lvListView.Sorting == System.Windows.Forms.SortOrder.Ascending)
            {
               lvListView.ListViewItemSorter = new gmRTL.MSComCtl.ListViewSorter(lvListView.SetSortOrder(System.Windows.Forms.SortOrder.Descending), lvListView.GetSortKey());
            }
            else
            {
               lvListView.ListViewItemSorter = new gmRTL.MSComCtl.ListViewSorter(lvListView.SetSortOrder(System.Windows.Forms.SortOrder.Ascending), lvListView.GetSortKey());
            }
         }
      }
      private void lvListView_ItemClick(object sender, ListViewItemSelectionChangedEventArgs e)
      {
         System.Windows.Forms.ListViewItem Item;
         Item = e.Item;
         if (!Item.Selected) return;
         System.IO.StreamReader ts = null;
         string fPath = "";
         try
         {
            gmRTL.Core.GlobalException.Clear();


            fPath = System.IO.Path.Combine(lblPath.Text,Item.Text);

            if (new System.IO.FileInfo(fPath).Length > 10000)
            {
               gmRTL.Core.ErrorHandling.RaiseGlobalException(17,String.Empty,"File too big");
            }

            ts = new System.IO.StreamReader(new System.IO.FileStream(fPath,System.IO.FileMode.Open,System.IO.FileAccess.Read,System.IO.FileShare.ReadWrite));


            RichTextBox1.Text = ts.ReadToEnd();

         }
         catch(System.Exception exc)
         {
            gmRTL.Core.GlobalException.Initialize(exc);
         }
         if (gmRTL.Core.GlobalException.Instance.Number != 0)
         {
            RichTextBox1.Text = "Error opening " + fPath + "\r\n" + "Err=" + gmRTL.Core.GlobalException.Instance.Description;
            Console.Beep();
         }
         if (!(ts == null))
         {
            ts.Close();
         }

      }
      private void mnu_FileExit_Click(object sender, EventArgs e)
      {
         this.Close();
      }
      private void RichTextBox1_KeyDown(object sender, KeyEventArgs e)
      {
         int keyCode;
         keyCode = (int)e.KeyCode;
         Console.Beep();
         keyCode = 0;
      }
      private void Toolbar1_ButtonClick(object sender, ToolStripItemClickedEventArgs e)
      {
         System.Windows.Forms.ToolStripItem Button;
         Button = (System.Windows.Forms.ToolStripItem)e.ClickedItem;


         System.Windows.Forms.ListViewItem itm = null;

         try
         {
            gmRTL.Core.GlobalException.Clear();
            if (Button.Text == "up")
            {
               tvTreeView.SelectedNode.PrevNode.TreeView.SelectedNode = tvTreeView.SelectedNode.PrevNode;
            }
            if (Button.Text == "down")
            {
               tvTreeView.SelectedNode.NextNode.TreeView.SelectedNode = tvTreeView.SelectedNode.NextNode;
            }
            if (Button.Text == "open")
            {
               tvTreeView.SelectedNode.Expand();
            }
            if (Button.Text == "close")
            {
               tvTreeView.SelectedNode.Collapse();
            }
            if (Button.Text == "tagit")
            {
               decimal minsize = 0.00M;
               minsize = -1;
               decimal cursize = 0.00M;
               int smallest = 0;
               foreach(System.Windows.Forms.ListViewItem forTemp0 in lvListView.Items)
               {
                  itm = forTemp0;
                  cursize = (itm.SubItems[1].Text.TrimB()).ToDec();
                  if (cursize < minsize || minsize < 0)
                  {
                     minsize = cursize;
                     smallest = itm.Index+1;
                  }
               }
               lvListView.FocusedItem = lvListView.Items[smallest - 1];
               itm = lvListView.SelectedItem();
               itm.Font = gmRTL.GUI.FontHelper.SetBold(itm.Font, true);
               lvListView.SelectedItem().SubItems[4].Text = "smallest";
            }
            if (Button.Text == "info")
            {
               itm = lvListView.SelectedItem();
               if (itm == null)
               {
                  RichTextBox1.Text = "Select an Item";
               }
               else
               {
                  lvListView.SelectedItem().Font = gmRTL.GUI.FontHelper.SetBold(lvListView.SelectedItem().Font, true);
                  RichTextBox1.Text = "" + "name:" + itm.Text + "\r\n" + "size:" + itm.SubItems[1].Text + "\r\n" + "ext :" + itm.SubItems[2].Text + "\r\n" + "date:" + itm.SubItems[3].Text + "\r\n" + "tag :" + itm.SubItems[4].Text;
               }
            }
         }
         catch(System.Exception exc)
         {
            gmRTL.Core.GlobalException.Initialize(exc);
         }
         if (gmRTL.Core.GlobalException.Instance.Number != 0)
         {
            RichTextBox1.Rtf = "ERROR: " + gmRTL.Core.GlobalException.Instance.Description;
            Console.Beep();
         }

      }
      //  This needs a complex migration.  The event is is not raised for the toolbar but for individual menu items.
      //  This handler would need to be attached to all ButtonMenu items in the ToolBar.
      // 
      //  Private Sub Toolbar1_ButtonMenuClick(ByVal ButtonMenu As MSComctlLib.ButtonMenu)
      //     lvListView.SelectedItem.SubItems(4) = ButtonMenu.Text
      //  End Sub
      private void tvTreeView_NodeClick(object sender, TreeNodeMouseClickEventArgs e)
      {
         System.Windows.Forms.TreeNode Node;
         Node = e.Node;
         System.IO.DirectoryInfo currFldr = null;


         currFldr = new System.IO.DirectoryInfo(Node.FullPath);
         lblPath.Text = Node.FullPath;

         DoHourglass(true);

         //  Add child nodes only if there aren't any now
         if (Node.GetNodeCount(false) == 0)
         {
            foreach(System.IO.DirectoryInfo fldr in currFldr.GetDirectories())
            {
               AddTVNode(Node,Node.Name + "\\" + fldr.Name,fldr.Name);
            }
            Node.Expand();
         }

         //  Add files to the ListView
         lvListView.Items.Clear();
         float totalFileSize = 0.0F;

         foreach(System.IO.FileInfo fl in currFldr.GetFiles())
         {
            totalFileSize = totalFileSize + fl.Length;
            AddListItem("",fl.Name,fl.Length.ToStr(),System.IO.Path.GetExtension(fl.FullName),fl.LastWriteTime);
         }

         //  Show total files and space occupied
         int fileCtr = 0;
         string message = "";

         fileCtr = currFldr.GetFiles().Length;
         message = fileCtr.ToStr() + " file" + (fileCtr == 1 ? "  " : "s  ");
         message = message + gmRTL.Core.FormatHelper.Format(totalFileSize,"###,###,##0") + " bytes";
         sbStatusBar.Items[0].Text = message;
         DoHourglass(false);

         System.Windows.Forms.Application.DoEvents();
      }
      private void AddListItem(string itemKey,string itemText,string itemSize,string itemType,DateTime itemModified)
      {

         //  Add a ListItem, then additional columns as ListSubItems.
         System.Windows.Forms.ListViewItem liListItem = null;
         liListItem = lvListView.Items.Add(itemText,"genericSmall");

         System.Windows.Forms.ListViewItem.ListViewSubItemCollection withTemp1 = null;
         withTemp1 = liListItem.SubItems;
         withTemp1.Add(itemSize);
         withTemp1.Add(itemType);
         withTemp1.Add(itemModified.ToStr());
         withTemp1.Add("no");
         liListItem.Selected = false;

      }
      private void AddTVNode(System.Windows.Forms.TreeNode ParentNode,string nodeKey,string nodeText)
      {
         //  Add a new TreeView node.
         System.Windows.Forms.TreeNode newNode = null;
         newNode = ParentNode.Nodes.Add(nodeKey,nodeText,ilMain.Images.IndexOfKey("fldrClosed"),ilMain.Images.IndexOfKey("fldrOpen"));
      }
      private static int DoHourglass_hourGlassCtr = 0;
      private void DoHourglass(bool showHourglass)
      {
         //  Turns hourglass display on/off.
         //  Keeps track of how many nested calls have been made to the hourglass function.
         // 
         //  Was the cursor already an hourglass?
         bool isHourglassPrev = false;
         //  Is the cursor an hourglass now?
         bool isHourglassNow = false;
         //  How many nested routines have turned on the hourglass?

         //  Note the current state of the cursor, then
         //  update the hourglass counter.
         isHourglassPrev = DoHourglass_hourGlassCtr > 0;
         if (showHourglass)
         {
            DoHourglass_hourGlassCtr = DoHourglass_hourGlassCtr + 1;
         }
         else
         {
            DoHourglass_hourGlassCtr = DoHourglass_hourGlassCtr - 1;
         }
         if (DoHourglass_hourGlassCtr < 0)
         {
            DoHourglass_hourGlassCtr = 0;
         }

         //  Set the cursor, but only if it's different from what it is now.
         isHourglassNow = DoHourglass_hourGlassCtr > 0;
         if (isHourglassNow != isHourglassPrev)
         {
            if (DoHourglass_hourGlassCtr > 0)
            {
               System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            }
            else
            {
               System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
         }
      }
      public void SizeControls(int X)
      {
         //  Set sizes and locations for the movable controls
         try
         {
            gmRTL.Core.GlobalException.Clear();

            // Set the TreeView's width
            if (X < 100)
            {
               X = 100;
            }
            if (X > this.Width - 100)
            {
               X = this.Width - 100;
            }
            tvTreeView.Width = X;
            imgSplitter.Left = X;

            //  Set up the ListView
            lvListView.Left = X + 5;
            lvListView.Width = ((int)(this.Width - (tvTreeView.Width + 13)));
            lblPath.Left = lvListView.Left + 2;
            lblPath.Width = lvListView.Width - 4;
            RichTextBox1.Left = lvListView.Left;
            RichTextBox1.Width = lvListView.Width;
         }
         catch(System.Exception exc)
         {
            gmRTL.Core.GlobalException.Initialize(exc);
         }
         //  trying to resize during minimize?
      }
   }
}
