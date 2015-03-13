using Microsoft.Data.Visualization.DataProvider;
using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.VisualizationControls;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using Microsoft.Reporting.QueryDesign.Edm.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows;
using System.Windows.Interop;

namespace Microsoft.Data.Visualization.Client.Excel
{
    [ComDefaultInterface(typeof(IPlugInConnection))]
    [Guid("F15876CB-BAD6-4B1C-9E50-AB2182332639")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class PlugInCore : IPlugInConnection
    {
        private const string addDataButtonId = "AddDataButtonClicked";
        internal Microsoft.Office.Interop.Excel.Application application;
        internal bool externalAddin;
        private Dictionary<Workbook, WorkbookState> workbookStateObjects;
        private readonly WeakEventListener<PlugInCore, object, EventArgs> workbookStateOnClosing;
        private readonly WeakEventListener<PlugInCore, object, InternalErrorEventArgs> workbookStateOnError;
        private readonly WeakEventListener<PlugInCore, object, UnhandledExceptionEventArgs> domainUnhandledException;
        internal WeakEventListener<PlugInCoreExternalAddin, object, EventArgs> ribbonInvalidation;

        public PlugInCore()
        {
            this.domainUnhandledException = new WeakEventListener<PlugInCore, object, UnhandledExceptionEventArgs>(this)
            {
                OnEventAction = PlugInCore.CurrentDomainUnhandledException
            };
            AppDomain.CurrentDomain.UnhandledException += this.domainUnhandledException.OnEvent;
            this.workbookStateOnClosing = new WeakEventListener<PlugInCore, object, EventArgs>(this)
            {
                OnEventAction = PlugInCore.workbookState_OnWorkbookClosing
            };
            this.workbookStateOnError = new WeakEventListener<PlugInCore, object, InternalErrorEventArgs>(this)
            {
                OnEventAction = PlugInCore.workbookState_OnWorkbookStateError
            };
        }

        public void Connect(object _application)
        {
            if (_application is Microsoft.Office.Interop.Excel.Application)
            {
                this.application = Marshal.GetUniqueObjectForIUnknown(Marshal.GetIUnknownForObject(_application)) as Microsoft.Office.Interop.Excel.Application;
                this.workbookStateObjects = new Dictionary<Workbook, WorkbookState>();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Connected to Excel");
                if (this.application == null)
                    return;
                if (this.application.LanguageSettings == null)
                    return;
                try
                {
                    Resources.Culture = CultureInfo.GetCultureInfo(this.application.LanguageSettings.get_LanguageID(MsoAppLanguageID.msoLanguageIDUI));
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.Fail("Unable to set UI Language from Excel settings", ex);
                }
            }
            else
                VisualizationTraceSource.Current.Fail("Excel connection failed, no Application object");
        }

        public void Disconnect()
        {
            if (this.workbookStateObjects != null)
            {
                foreach (WorkbookState workbookState in this.workbookStateObjects.Values)
                    this.ReleaseWorkbookState(workbookState, true);
                this.workbookStateObjects.Clear();
                this.workbookStateObjects = null;
                this.application = null;
                AppDomain.CurrentDomain.UnhandledException -= this.domainUnhandledException.OnEvent;
                GC.Collect();
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Disconnected from Excel");
        }

        public bool IsPluginActive()
        {
            try
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Check IsPluginActive");
                Workbook activeWorkbook = this.application.ActiveWorkbook;
                WorkbookState workbookState = this.GetWorkbookState(activeWorkbook);
                return workbookState != null && workbookState.IsInitialized && workbookState.CurrentTour != null;
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(ex);
                return false;
            }
        }

        public void ExploreButtonClicked()
        {
            WorkbookState workbookState = null;
            ManageToursViewModel manageToursViewModel = null;
            try
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Start Exploration mode");
                Workbook activeWorkbook = this.application.ActiveWorkbook;
                workbookState = this.GetWorkbookState(activeWorkbook);
                if (workbookState == null)
                    return;
                if (!workbookState.IsInitialized)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Initializing WorkbookState for workbook: {0}", (object)activeWorkbook.Name);
                    if (!workbookState.Initialize(activeWorkbook))
                        return;
                }
                if (workbookState.Tours.Count == 0)
                {
                    workbookState.Explore(true);
                }
                else
                {
                    manageToursViewModel = new ManageToursViewModel(workbookState);
                    ManageToursWindow manageToursWindow = new ManageToursWindow();
                    manageToursWindow.FlowDirection = Resources.Culture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    manageToursWindow.DataContext = manageToursViewModel;
                    manageToursWindow.ShowInTaskbar = false;
                    WindowInteropHelper windowInteropHelper1 = new WindowInteropHelper(manageToursWindow);
                    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
                    windowInteropHelper1.Owner = mainWindowHandle;
                    bool? nullable;
                    try
                    {
                        nullable = manageToursWindow.ShowDialog();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return;
                    }
                    catch (SEHException ex1)
                    {
                        VisualizationTraceSource.Current.Fail("ShowDialog failed, retrying without owner window", ex1);
                        windowInteropHelper1.Owner = IntPtr.Zero;
                        WindowInteropHelper windowInteropHelper2 = new WindowInteropHelper(manageToursWindow);
                        try
                        {
                            nullable = manageToursWindow.ShowDialog();
                        }
                        catch (InvalidOperationException ex2)
                        {
                            return;
                        }
                    }
                    if (nullable.HasValue && nullable.Value || workbookState.ToursAvilable)
                        return;
                    workbookState.DeleteAllData(activeWorkbook);
                    this.workbookStateObjects.Remove(activeWorkbook);
                }
            }
            catch (ModelWrapper.InvalidModelData ex)
            {
                MessageBox.Show(ex.Message, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            catch (TourDeserializationException ex)
            {
                if (manageToursViewModel != null)
                    manageToursViewModel.CloseAction(false);
                this.HandleException(workbookState, ex);
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(ex);
                this.HandleException(workbookState, ex);
            }
        }

        public void AddDataButtonClicked()
        {
            WorkbookState workbookStateAtFault = null;
            try
            {
                Workbook activeWorkbook = this.application.ActiveWorkbook;
                workbookStateAtFault = this.GetWorkbookState(activeWorkbook);
                if (workbookStateAtFault == null)
                    return;
                if (!workbookStateAtFault.IsInitialized)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Initializing WorkbookState for workbook: {0}", (object)activeWorkbook.Name);
                    if (!workbookStateAtFault.Initialize(activeWorkbook))
                        return;
                }
                workbookStateAtFault.Explore(false);
            }
            catch (ModelWrapper.InvalidModelData ex)
            {
                MessageBox.Show(ex.Message, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(ex);
                this.HandleException(workbookStateAtFault, ex);
            }
        }

        internal WorkbookState GetWorkbookState(Workbook workbook)
        {
            WorkbookState workbookState = null;
            if (workbook != null && !this.workbookStateObjects.TryGetValue(workbook, out workbookState))
            {
                CustomXMLParts customXmlParts = workbook.CustomXMLParts.SelectByNamespace("http://microsoft.data.visualization.Client.Excel/1.0");
                if (customXmlParts.Count == 0)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "No existing Excel Visualization XMLPart was found");
                    workbookState = new WorkbookState();
                }
                else
                {
                    if (customXmlParts.Count > 1)
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0} CustomXMLPart with namespace {1} were found, using the first one ", (object)customXmlParts.Count, (object)"http://microsoft.data.visualization.Client.Excel/1.0");
                    CustomXMLPart xmlPart = customXmlParts[1];
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Existing Excel Visualization XMLPart was found, deserializing it");
                    workbookState = WorkbookState.CreateFromXMLPart(workbook, xmlPart);
                    if (workbookState == null)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "GetWorkbookState(): returning null because CreateFromXMLPart returned null");
                        return null;
                    }
                }
                workbookState.OnWorkbookStateError += this.workbookStateOnError.OnEvent;
                workbookState.OnWorkbookClosing += this.workbookStateOnClosing.OnEvent;
                if (this.ribbonInvalidation != null)
                {
                    workbookState.OnVisualizationXMLPartChanged += this.ribbonInvalidation.OnEvent;
                    workbookState.OnHostWindowClosed += this.ribbonInvalidation.OnEvent;
                    workbookState.OnHostWindowOpened += this.ribbonInvalidation.OnEvent;
                }
                this.workbookStateObjects.Add(workbook, workbookState);
            }
            else if (workbook == null)
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "GetWorkbookState(): returning null because input param workbook is null");
            return workbookState;
        }

        private static void workbookState_OnWorkbookClosing(PlugInCore core, object sender, EventArgs e)
        {
            WorkbookState workbookState = sender as WorkbookState;
            if (workbookState == null)
                return;
            foreach (Workbook key in core.workbookStateObjects.Keys)
            {
                if (core.workbookStateObjects[key] == workbookState)
                {
                    try
                    {
                        core.workbookStateObjects.Remove(key);
                        core.ReleaseWorkbookState(workbookState, true);
                        break;
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail(ex);
                        break;
                    }
                }
            }
        }

        private static void workbookState_OnWorkbookStateError(PlugInCore core, object sender, InternalErrorEventArgs e)
        {
            WorkbookState workbookStateAtFault = sender as WorkbookState;
            core.HandleException(workbookStateAtFault, e.InternalErrorException);
        }

        private void HandleException(WorkbookState workbookStateAtFault, Exception ex)
        {
            try
            {
                PlugInCore.ShowClosingOnErrorDialog(ex, false, this.externalAddin);
                if (workbookStateAtFault == null)
                    return;
                foreach (Workbook key in this.workbookStateObjects.Keys)
                {
                    if (this.workbookStateObjects[key] == workbookStateAtFault)
                    {
                        this.workbookStateObjects.Remove(key);
                        this.ReleaseWorkbookState(workbookStateAtFault, false);
                        break;
                    }
                }
            }
            catch (Exception ex1)
            {
                VisualizationTraceSource.Current.Fail(ex1);
            }
        }

        private void ReleaseWorkbookState(WorkbookState workbookState, bool saveBeforeRelease)
        {
            try
            {
                workbookState.OnWorkbookStateError -= this.workbookStateOnError.OnEvent;
                workbookState.OnWorkbookClosing -= this.workbookStateOnClosing.OnEvent;
                if (this.ribbonInvalidation != null)
                {
                    workbookState.OnVisualizationXMLPartChanged -= this.ribbonInvalidation.OnEvent;
                    workbookState.OnHostWindowClosed -= this.ribbonInvalidation.OnEvent;
                    workbookState.OnHostWindowOpened -= this.ribbonInvalidation.OnEvent;
                }
                workbookState.UnInitialize(saveBeforeRelease);
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(ex);
            }
        }

        private static void CurrentDomainUnhandledException(PlugInCore core, object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            if (exception != null)
                VisualizationTraceSource.Current.Fail(exception);
            PlugInCore.ShowClosingOnErrorDialog(exception, true, core.externalAddin);
        }

        private static void ShowClosingOnErrorDialog(Exception ex, bool domainUE, bool externalAddin)
        {
            if (ex == null)
            {
                MessageBox.Show(Resources.ClosingOnError, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is VisualizationEngineException && ex.InnerException == null)
            {
                MessageBox.Show(Resources.EngineError, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is DirectX9ForWpfException)
            {
                MessageBox.Show(Resources.EngineError, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is COMException && ((ExternalException)ex).ErrorCode == -2144980991)
            {
                MessageBox.Show(Resources.EngineError, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is TargetInvocationException && ex.InnerException is ModelWrapper.InvalidModelData)
            {
                MessageBox.Show(ex.InnerException.Message, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is TourDeserializationException || ex is TargetInvocationException && ex.InnerException is TourDeserializationException)
            {
                MessageBox.Show(Resources.SerializationError, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is OutOfMemoryException || ex.InnerException is OutOfMemoryException)
            {
                MessageBox.Show(Environment.Is64BitProcess ? Resources.OOMError : Resources.OOMError32, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is COMException && (((ExternalException)ex).ErrorCode == -2147024882 || ((ExternalException)ex).ErrorCode == -2147024888) || ex.InnerException is COMException && (((ExternalException)ex.InnerException).ErrorCode == -2147024882 || ((ExternalException)ex.InnerException).ErrorCode == -2147024888))
            {
                MessageBox.Show(Environment.Is64BitProcess ? Resources.OOMError : Resources.OOMError32, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is EdmException || ex.InnerException is EdmException)
            {
                MessageBox.Show(Resources.ModelSuperscriptError, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else if (ex is COMException && ((ExternalException)ex).ErrorCode == -536602600)
            {
                MessageBox.Show(Resources.ProtectedWorkbookError, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
            }
            else
            {
                if (MessageBox.Show(Resources.ClosingOnErrorWithException, Resources.Product, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None) != MessageBoxResult.Yes)
                    return;
                try
                {
                    string str = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion + (Environment.Is64BitProcess ? ".64" : ".32") + (externalAddin ? "Ext" : string.Empty);
                    Process.Start("mailto:GeoFlowErrorReport@Microsoft.com?subject=" + HttpUtility.UrlPathEncode(Resources.ErrorEmailSubject + (domainUE ? " -- " : " - ") + str) + "&body=" + HttpUtility.UrlEncode(string.Format(Resources.ErrorEmailBody, ex.ToString())).Replace("+", "%20"));
                }
                catch (Exception ex1)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Can't open email client to send error details: {0}", (object)ex1);
                    MessageBox.Show(Resources.UnableToSendEmail, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
                }
            }
        }
    }
}
