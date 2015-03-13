using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Data.Visualization.Client.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;

namespace ExcelAddIn1
{
    public partial class ThisAddIn
    {
        // private UserControl1 myUserControl1;

        // private Microsoft.Office.Tools.CustomTaskPane myCustomTaskPane;

        //public bool ShowTaskPane
        //{
        //    get
        //    {
        //        if (myCustomTaskPane != null)
        //        {
        //            return myCustomTaskPane.Visible;
        //        }
        //        return false;
        //    }
        //    set
        //    {
        //        if (myCustomTaskPane != null)
        //        {
        //            myCustomTaskPane.Visible = value;
        //        }
        //    }
        //}
        
        private PlugInCoreExternalAddin addin;

        public PlugInCoreExternalAddin Addin
        {
            get { return addin; }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            this.addin = new PlugInCoreExternalAddin();
            
            //myUserControl1 = new UserControl1();
            //myCustomTaskPane = this.CustomTaskPanes.Add(myUserControl1,
            //    "New Task Pane");

            //myCustomTaskPane.DockPosition =
            //    Office.MsoCTPDockPosition.msoCTPDockPositionFloating;
            //myCustomTaskPane.Height = 500;
            //myCustomTaskPane.Width = 500;

            //myCustomTaskPane.DockPosition =
            //    Office.MsoCTPDockPosition.msoCTPDockPositionRight;
            //myCustomTaskPane.Width = 300;

            //myCustomTaskPane.Visible = true;
            //myCustomTaskPane.DockPositionChanged +=
            //    new EventHandler(myCustomTaskPane_DockPositionChanged);
        }

        //private void myCustomTaskPane_DockPositionChanged(object sender, EventArgs e)
        //{
        //    Microsoft.Office.Tools.CustomTaskPane taskPane =
        //        sender as Microsoft.Office.Tools.CustomTaskPane;

        //    if (taskPane != null)
        //    {
        //        // Adjust sizes of user control and flow panel to fit current task pane size.
        //        UserControl1 userControl = taskPane.Control as UserControl1;
        //        System.Drawing.Size paneSize = new System.Drawing.Size(taskPane.Width, taskPane.Height);
        //        userControl.Size = paneSize;
        //        userControl.FlowPanel.Size = paneSize;

        //        // Adjust flow direction of controls on the task pane. 
        //        if (taskPane.DockPosition ==
        //            Office.MsoCTPDockPosition.msoCTPDockPositionTop ||
        //            taskPane.DockPosition ==
        //            Office.MsoCTPDockPosition.msoCTPDockPositionBottom)
        //        {
        //            userControl.FlowPanel.FlowDirection =
        //                System.Windows.Forms.FlowDirection.LeftToRight;
        //        }
        //        else
        //        {
        //            userControl.FlowPanel.FlowDirection =
        //                System.Windows.Forms.FlowDirection.TopDown;
        //        }
        //    }
        //}

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            Addin.DisconnectExternalAddin();
        }

        #region VSTO 生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
