using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Tools.Ribbon;

namespace ExcelAddIn1
{
    public partial class Ribbon1
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            Globals.ThisAddIn.Addin.ConnectExternalAddin(Globals.ThisAddIn.Application, e.RibbonUI);
        }

        private void splitButton1_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.Addin.ExploreButtonClickedExternalAddin(e.Control);
            // Globals.ThisAddIn.ShowTaskPane = !Globals.ThisAddIn.ShowTaskPane;
        }

        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.Addin.AddDataButtonClickedExternalAddin(e.Control);
            
        }

        private void button1_Click_1(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.Addin.ExploreButtonClickedExternalAddin(e.Control);            
        }
    }
}
