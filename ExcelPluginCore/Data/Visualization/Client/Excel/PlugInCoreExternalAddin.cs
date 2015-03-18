using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Client.Excel
{
    // 76904183
    public class PlugInCoreExternalAddin : IPlugInConnectionExternalAddin
    {
        private const string addDataButtonId = "AddDataButtonClicked";
        private IRibbonUI ribbonUI;
        private PlugInCore pluginCore;
        private Dispatcher excelDispatcher;
        private readonly WeakEventListener<PlugInCoreExternalAddin, object, EventArgs> ribbonInvalidation;

        public PlugInCoreExternalAddin()
        {
            this.pluginCore = new PlugInCore();
            this.ribbonInvalidation = new WeakEventListener<PlugInCoreExternalAddin, object, EventArgs>(this)
            {
                OnEventAction = PlugInCoreExternalAddin.InvalidateRibbon
            };
            this.pluginCore.ribbonInvalidation = this.ribbonInvalidation;
            this.pluginCore.externalAddin = true;
            this.excelDispatcher = Dispatcher.CurrentDispatcher;
        }

        public void ConnectExternalAddin(object _application, object _ribbonUI)
        {
            if (!(_ribbonUI is IRibbonUI))
                return;
            this.ribbonUI = _ribbonUI as IRibbonUI;
            this.pluginCore.Connect(_application);
        }

        public void DisconnectExternalAddin()
        {
            this.pluginCore.Disconnect();
        }

        public bool IsPluginActiveExternalAddin()
        {
            try
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Check IsPluginActiveExternalAddin");
                Workbook activeWorkbook = this.pluginCore.application.ActiveWorkbook;
                WorkbookState workbookState = this.pluginCore.GetWorkbookState(activeWorkbook);
                if (workbookState == null)
                    return false;
                if (!workbookState.IsInitialized)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "IsPluginActiveExternalAddin(): Initializing WorkbookState for workbook: {0}", (object)activeWorkbook.Name);
                    if (!workbookState.Initialize(activeWorkbook))
                        return true;
                }
                return this.pluginCore.IsPluginActive();
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(ex);
                return false;
            }
        }

        public void ExploreButtonClickedExternalAddin(object ribbonControl)
        {
            if (!this.ShouldBeEnabled())
                return;
            this.pluginCore.ExploreButtonClicked();
        }

        public void AddDataButtonClickedExternalAddin(object ribbonControl)
        {
            if (!this.GetEnabledExternalAddin(ribbonControl))
                return;
            this.pluginCore.AddDataButtonClicked();
        }

        public bool GetEnabledExternalAddin(object ribbonControl)
        {
            if (!this.ShouldBeEnabled())
                return false;
            IRibbonControl ribbonControl1 = ribbonControl as IRibbonControl;
            if (ribbonControl1 == null)
                return false;
            if (!(ribbonControl1.Id == addDataButtonId))
                return true;
            if (this.pluginCore.application != null && this.pluginCore.application.ActiveWorkbook != null)
            {
                WorkbookState workbookState = this.pluginCore.GetWorkbookState(this.pluginCore.application.ActiveWorkbook);
                if (workbookState != null && workbookState.IsInitialized)
                    return true;
            }
            return false;
        }

        private bool ShouldBeEnabled()
        {
            if (this.pluginCore.application.ActiveWorkbook != null && 
                (this.pluginCore.application.ActiveWorkbook.FileFormat == XlFileFormat.xlOpenXMLWorkbook || 
                this.pluginCore.application.ActiveWorkbook.FileFormat == XlFileFormat.xlOpenXMLWorkbook || 
                (this.pluginCore.application.ActiveWorkbook.FileFormat == XlFileFormat.xlOpenXMLWorkbookMacroEnabled || 
                this.pluginCore.application.ActiveWorkbook.FileFormat == XlFileFormat.xlOpenXMLTemplateMacroEnabled) || 
                (this.pluginCore.application.ActiveWorkbook.FileFormat == XlFileFormat.xlOpenXMLStrictWorkbook || 
                this.pluginCore.application.ActiveWorkbook.FileFormat == XlFileFormat.xlExcel12 || 
                this.pluginCore.application.ActiveWorkbook.FileFormat == XlFileFormat.xlOpenXMLTemplate)) && 
                (this.pluginCore.application.ActiveProtectedViewWindow == null && 
                !this.pluginCore.application.ActiveWorkbook.ProtectStructure && 
                !this.pluginCore.application.ActiveWorkbook.Final))
                return !this.pluginCore.application.ActiveWorkbook.MultiUserEditing;
            return false;
        }

        private static void InvalidateRibbon(PlugInCoreExternalAddin coreExternalAddin, object sender, EventArgs e)
        {
            if (coreExternalAddin.ribbonUI == null)
                return;
            coreExternalAddin.excelDispatcher.BeginInvoke((System.Action)(() => coreExternalAddin.ribbonUI.Invalidate()));
        }
    }
}
