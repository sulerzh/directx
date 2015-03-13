using Microsoft.Data.Visualization.DataProvider;
using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.VisualizationControls;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Client.Excel
{
    [XmlRoot("Visualization", IsNullable = false, Namespace = "http://microsoft.data.visualization.Client.Excel/1.0")]
    [Serializable]
    public class WorkbookState : ITourPersist, IModelWrapper, IDataSourceFactory, IHelpViewer, ICustomMapProvider
    {
        private object tourPartSyncObject = new object();
        private object toursListSyncObject = new object();
        private int ConcurrencyLimit = Math.Min(Environment.ProcessorCount * 4, Environment.Is64BitProcess ? 32 : 16);
        private const string HELP_TOPIC_ID = "115344";
        private WorkbookLatLonCacheState latLonCache;
        private WorkbookPolygonState regionProvider;
        private WorkbookCustomMapListState customMapList;
        private readonly WeakEventListener<WorkbookState, object, EventArgs> proxyHostWindowClosed;
        private readonly WeakEventListener<WorkbookState, object, EventArgs> proxyHostWindowOpened;
        private readonly WeakEventListener<WorkbookState, object, InternalErrorEventArgs> proxyOnError;
        private CustomXMLPart xmlPart;
        private Workbook workbook;
        private ModelWrapper model;
        private ClientProxy proxy;
        private Dispatcher excelDispatcher;
        private int toursPendingUpdate;
        private ADODB.Connection modelConnection;

        [XmlArrayItem("Tour", typeof(ExcelTour))]
        [XmlArray("Tours")]
        public ObservableCollectionEx<ExcelTour> Tours { get; set; }

        [XmlArrayItem("Color", typeof(Color4F))]
        [XmlArray("Colors")]
        public List<Color4F> CustomColors { get; set; }

        [XmlIgnore]
        public bool IsInitialized { get; private set; }

        public bool ToursAvilable
        {
            get
            {
                if (this.Tours.IsEmpty)
                    return this.toursPendingUpdate != 0;
                return true;
            }
        }

        public ExcelTour CurrentTour
        {
            get
            {
                ClientProxy clientProxy = this.proxy;
                if (clientProxy == null)
                    return null;
                int currentTourHandle = clientProxy.CurrentTourHandle;
                if (currentTourHandle != 0)
                {
                    lock (this.toursListSyncObject)
                    {
                        foreach (ExcelTour item_0 in this.Tours)
                        {
                            if (item_0.TourHandle == currentTourHandle)
                                return item_0;
                        }
                    }
                }
                return null;
            }
        }

        public bool ConnectionsDisabled
        {
            get
            {
                return this.workbook.ConnectionsDisabled;
            }
        }

        public ICustomMapCollection MapCollection
        {
            get
            {
                return this.GetAndEnsureCustomMapList();
            }
        }

        public event EventHandler OnVisualizationXMLPartChanged;

        public event InternalErrorEventHandler OnWorkbookStateError;

        public event EventHandler OnHostWindowOpened;

        public event EventHandler OnHostWindowClosed;

        public event EventHandler OnWorkbookClosing;

        public WorkbookState()
        {
            this.proxyOnError = new WeakEventListener<WorkbookState, object, InternalErrorEventArgs>(this)
            {
                OnEventAction = WorkbookState.proxy_OnInternalError
            };
            this.proxyHostWindowOpened = new WeakEventListener<WorkbookState, object, EventArgs>(this)
            {
                OnEventAction = WorkbookState.proxy_OnHostWindowOpened
            };
            this.proxyHostWindowClosed = new WeakEventListener<WorkbookState, object, EventArgs>(this)
            {
                OnEventAction = WorkbookState.proxy_OnHostWindowClosed
            };
            ServicePointManager.DefaultConnectionLimit = this.ConcurrencyLimit;
            this.excelDispatcher = Dispatcher.CurrentDispatcher;
            this.Tours = new ObservableCollectionEx<ExcelTour>();
            this.toursPendingUpdate = 0;
            this.xmlPart = null;
            this.latLonCache = null;
            this.customMapList = null;
            this.IsInitialized = false;
        }

        public bool Initialize(Workbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException("workbook");
            if (this.IsInitialized)
                return true;
            this.workbook = workbook;
            Stopwatch stopwatch = new Stopwatch();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            stopwatch.Start();
            Task<GeospatialEndpoint.EndPoint> task = GeospatialEndpoint.FetchServiceEndpointsAsync(Resources.Culture.Name, new RegionInfo(Thread.CurrentThread.CurrentCulture.LCID).TwoLetterISORegionName, cancellationTokenSource.Token);
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "WorkbookState.Initialize(): endpoint svc request send time: {0} ms", stopwatch.ElapsedMilliseconds);
            try
            {
                this.CacheADOConnection();
                this.model = new ModelWrapper(workbook, this.modelConnection);
            }
            catch (ModelWrapper.InvalidModelData ex)
            {
                if (ex.InnerException is COMException)
                {
                    cancellationTokenSource.Cancel();
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "WorkbookState.Initialize(): Caught InvalidModelData exception wrapping a COMException when getting model connection or initializing model wrapper - returning; ex={0}", (ex).ToString());
                    MessageBox.Show(ex.Message, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                    return false;
                }
                cancellationTokenSource.Cancel();
                throw;
            }
            catch (COMException ex)
            {
                cancellationTokenSource.Cancel();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "WorkbookState.Initialize(): Caught COM exception when getting model connection or initializing model wrapper - returning; ex={0}", (ex).ToString());
                MessageBox.Show(Resources.RetryAction, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                return false;
            }
            catch (Exception ex)
            {
                cancellationTokenSource.Cancel();
                throw;
            }
            stopwatch.Restart();
            GeospatialEndpoint.EndPoint result = task.Result;
            stopwatch.Stop();
            long num1 = elapsedMilliseconds + stopwatch.ElapsedMilliseconds;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "WorkbookState.Initialize(): endpoint fetch task completion wait time {0}, elapsed time processing fetch: {1} ms (incl request send time)", stopwatch.ElapsedMilliseconds, num1);
            if (result.OtherError && result.HasNullEndpoints)
            {
                MessageBox.Show(string.Format(Resources.BingGeoPolError, result.StatusCode), Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                return false;
            }
            if (result.ServerNotReachable && result.HasNullEndpoints)
            {
                MessageBox.Show(string.Format(Resources.BingGeoPolServerNotReachable, result.StatusCode), Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
            }
            bool flag = false;
            if ((result.OtherError || result.ServerNotReachable) && !result.HasNullEndpoints)
            {
                MessageBox.Show(string.Format(Resources.BingGeoPolErrorUseExistingEndpoints, result.StatusCode), Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                flag = true;
            }
            if (result.HasNullEndpoints)
            {
                MessageBox.Show(Resources.BingGeoPolLocationNotSupported, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                return false;
            }
            //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "AfterSave").AddEventHandler(this.workbook, new WorkbookEvents_AfterSaveEventHandler(this.workbook_AfterSave));
            //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "BeforeSave").AddEventHandler(this.workbook, new WorkbookEvents_BeforeSaveEventHandler(this.workbook_BeforeSave));
            //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "BeforeClose").AddEventHandler(this.workbook, new WorkbookEvents_BeforeCloseEventHandler(this.workbook_BeforeClose));
            //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "ModelChange").AddEventHandler(this.workbook, new WorkbookEvents_ModelChangeEventHandler(this.workbook_ModelChange));
            if (this.latLonCache == null)
                this.latLonCache = new WorkbookLatLonCacheState();
            if (this.regionProvider == null)
                this.regionProvider = new WorkbookPolygonState();
            if (this.customMapList == null)
                this.customMapList = WorkbookCustomMapListState.CreateFromXmlPart(workbook);
            this.latLonCache.Initialize(result.BingMapKey, result.Geocoding, Resources.Culture);
            this.proxy = new ClientProxy(this, this, this, this, this, new GeospatialDataProviders()
            {
                LatLonProvider = this.latLonCache.LatLonCache,
                RegionProvider = this.regionProvider.RegionProvider
            }, new BingMapResourceUri()
            {
                BingLogoAerialUrl = result.BingLogoAerial,
                BingLogoUrl = result.BingLogo,
                CombinedLogoAerialUrl = result.CombinedLogoAerial,
                CombinedLogoUrl = result.CombinedLogo,
                AerialWithLabelsTilesUri = result.AerialWithLabels,
                AerialWithoutLabelsTilesUri = result.AerialWithoutLabels,
                RoadWithLabelsTilesUri = result.RoadWithLabels,
                RoadWithoutLabelsTilesUri = result.RoadWithoutLabels,
                UrlsFromSettings = flag
            });
            this.proxy.OnInternalError += this.proxyOnError.OnEvent;
            this.proxy.OnHostWindowOpened += this.proxyHostWindowOpened.OnEvent;
            this.proxy.OnHostWindowClosed += this.proxyHostWindowClosed.OnEvent;
            this.IsInitialized = true;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            GregorianCalendar gregorianCalendar1 = null;
            if (!(currentCulture.DateTimeFormat.Calendar is GregorianCalendar))
            {
                foreach (Calendar calendar in currentCulture.OptionalCalendars)
                {
                    GregorianCalendar gregorianCalendar2 = calendar as GregorianCalendar;
                    if (gregorianCalendar2 != null)
                    {
                        gregorianCalendar1 = gregorianCalendar2;
                        if (gregorianCalendar2.CalendarType == GregorianCalendarTypes.Localized)
                            break;
                    }
                }
            }
            return this.proxy.Initialize(Resources.Culture, this.CustomColors, gregorianCalendar1);
        }

        private void CacheADOConnection()
        {
            if (this.modelConnection != null)
                return;
            Stopwatch stopwatch = Stopwatch.StartNew();
            ADODB.Connection connection = null;
            int retryTimes = 0;
            bool flag = false;
            do
            {
                try
                {
                    connection = this.workbook.Model.DataModelConnection.ModelConnection.ADOConnection as ADODB.Connection;
                    flag = true;
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == -2147417846 || ex.ErrorCode == -2146827284 || ex.ErrorCode == -2147217842)
                    {
                        if (++retryTimes >= 5)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "ModelWrapper_ctor(): {0} retries calling get_ADOConnection failed: giving up", 5);
                            throw;
                        }
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "ModelWrapper_ctor(): {0} of {1} retries calling get_ADOConnection failed, will retry: Exception: {2}", retryTimes, 5, (ex).ToString());
                    }
                    else
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "ModelWrapper_ctor(): {0} of {1} retries calling get_ADOConnection failed, giving up: Exception: {2}", (retryTimes + 1), 5, (ex).ToString());
                        throw;
                    }
                }
                if (flag)
                    break;
            }
            while (retryTimes < 5);
            this.modelConnection = connection;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Model connection retrieval time: {0} ms, retryCount={1}", stopwatch.ElapsedMilliseconds, retryTimes);
        }

        private static void proxy_OnHostWindowClosed(WorkbookState state, object sender, EventArgs e)
        {
            if (state.OnHostWindowClosed == null)
                return;
            state.OnHostWindowClosed(state, e);
        }

        private static void proxy_OnHostWindowOpened(WorkbookState state, object sender, EventArgs e)
        {
            if (state.OnHostWindowOpened == null)
                return;
            state.OnHostWindowOpened(state, e);
        }

        private static void proxy_OnInternalError(WorkbookState state, object sender, InternalErrorEventArgs e)
        {
            if (state.OnWorkbookStateError == null)
                return;
            try
            {
                state.OnWorkbookStateError(state, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void workbook_BeforeClose(ref bool Cancel)
        {
            if (this.OnWorkbookClosing == null)
                return;
            this.OnWorkbookClosing(this, null);
        }

        private void workbook_BeforeSave(bool SaveAsUI, ref bool Cancel)
        {
            if (this.proxy == null)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.BeforeSave(): starting");
            DispatcherFrame frame = new DispatcherFrame();
            this.excelDispatcher.BeginInvoke((System.Action)(() =>
            {
                try
                {
                    this.proxy.SaveHostWindow();
                }
                catch (Exception ex)
                {
                    frame.Continue = false;
                    throw;
                }
            }));
            this.excelDispatcher.BeginInvoke(DispatcherPriority.Background, (System.Action)(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.BeforeSave(): done");
        }

        private void workbook_AfterSave(bool Success)
        {
            if (this.proxy == null)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.AfterSave(): starting");
            this.proxy.SetHostWindowTitle(string.Format(Resources.VisualizationTitle, this.workbook.Name));
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.AfterSave(): done");
        }

        private void workbook_ModelChange(ModelChanges changes)
        {
            List<string> tablesWithUpdatedData = new List<string>();
            if (changes.TablesModified != null)
            {
                for (int index = 0; index < changes.TablesModified.Count; ++index)
                {
                    tablesWithUpdatedData.Add(changes.TablesModified.Item((index + 1)));
                }
            }
            this.proxy.ModelChanged(tablesWithUpdatedData);
        }

        private string GetNewTourName()
        {
            int num = 1;
            bool flag = false;
            try
            {
                lock (this.toursListSyncObject)
                {
                    while (!flag)
                    {
                        flag = true;
                        foreach (ExcelTour item_0 in this.Tours)
                        {
                            if (item_0.Name.Equals(string.Format(Resources.DefaultTourName, num)))
                            {
                                ++num;
                                flag = false;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return string.Format(Resources.DefaultTourName, num);
        }

        public void Explore(bool setNewTour)
        {
            if (!this.IsInitialized)
                return;
            bool addedToModel = false;
            string modelTableName = null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long num1 = 0L;
            try
            {
                ModelWrapper modelWrapper = this.model;
                if (modelWrapper != null)
                {
                    if (modelWrapper.ExpandAndValidateSelection())
                    {
                        IModelData dataFromSelection = modelWrapper.GetModelDataFromSelection();
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): GetModelDataFromSelection() time = {0} ms, selection is{1} null", stopwatch.ElapsedMilliseconds, dataFromSelection == null ? string.Empty : " not");
                        num1 += stopwatch.ElapsedMilliseconds;
                        stopwatch.Restart();
                        if (dataFromSelection != null)
                        {
                            addedToModel = modelWrapper.AddToModel(dataFromSelection, out modelTableName);
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): AddToModel() time = {0} ms  (incl CSDL fetch, parse, Orlando sampling and classification if those steps were done by call to AddToModel via ModelRefresh notification)", stopwatch.ElapsedMilliseconds);
                            num1 += stopwatch.ElapsedMilliseconds;
                            stopwatch.Restart();
                        }
                    }
                }
            }
            catch (ModelWrapper.InvalidModelData ex)
            {
                if (ex.InnerException is COMException)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "WorkbookState.Explore(): Caught InvalidModelData exception wrapping a COMException when  when expanding selection or adding to model - returning; ex={0}", ex);
                    MessageBox.Show(ex.Message, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                    return;
                }
                throw;
            }
            catch (COMException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "WorkbookState.Explore(): Caught COM exception when expanding selection or adding to model - returning; ex={0}", ex);
                MessageBox.Show(Resources.RetryAction, Resources.Product, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                return;
            }
            ClientProxy clientProxy = this.proxy;
            if (this.proxy != null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): Calling proxy.FetchModelMetadata()");
                clientProxy.FetchModelMetadata();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore():Completed proxy.FetchModelMetadata(); total FetchModelMetadata time = {0} ms (incl CSDL fetch, parse, Orlando sampling and classification if those steps were done by call to FetchModelMetadata)", stopwatch.ElapsedMilliseconds);
                num1 += stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
            }
            if (this.proxy != null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): Calling proxy.ShowHostWindow()");
                Workbook workbook = this.workbook;
                if (setNewTour)
                    clientProxy.ShowHostWindow(null, string.Format(Resources.VisualizationTitle, workbook == null ? string.Empty : workbook.Name), false, false, this.GetNewTourName());
                else
                    clientProxy.ShowHostWindow(string.Format(Resources.VisualizationTitle, workbook == null ? string.Empty : workbook.Name));
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore():  proxy.ShowHostWindow() time = {0} ms", stopwatch.ElapsedMilliseconds);
                num1 += stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
            }
            if (this.proxy == null)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): Calling proxy.AddLayerDefinition()");
            clientProxy.AddLayerDefinition(addedToModel, modelTableName, (string)null);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore():Completed proxy.AddLayerDefinition(); total AddLayerDefinition time = {0} ms (incl CSDL fetch, parse, Orlando sampling and classification if those steps were done by call to AddLayerDefinition)", stopwatch.ElapsedMilliseconds);
            long num3 = num1 + stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
        }

        public void SetCurrentTour(ExcelTour excelTour, bool playTour = false)
        {
            if (!this.IsInitialized)
                return;
            Tour tour = null;
            if (excelTour == null)
                return;
            bool currentTour = false;
            lock (this.tourPartSyncObject)
            {
                if (excelTour.IsOpened)
                {
                    currentTour = true;
                }
                else
                {
                    CustomXMLPart customXmlPart = this.workbook.CustomXMLParts.SelectByID(excelTour.TourVersionId);
                    if (customXmlPart != null)
                    {
                        StringReader sr = new StringReader(customXmlPart.DocumentElement.XML);
                        XmlReader xr = XmlReader.Create(sr);
                        try
                        {
                            tour = Tour.ReadXml(xr);
                            excelTour.TourHandle = tour.Handle;
                            tour.Name = excelTour.Name;
                        }
                        catch (Exception ex)
                        {
                            VisualizationTraceSource.Current.Fail(string.Format("Exception while loading the tour with id {0}.", excelTour.TourVersionId), ex);
                            throw new TourDeserializationException(Resources.SerializationError, ex);
                        }
                        finally
                        {
                            xr.Close();
                            sr.Close();
                        }
                    }
                    else
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Tour with id {0} not found in the workbook.", excelTour.TourVersionId);
                        throw new TourDeserializationException(Resources.TourNotFound);
                    }
                }
            }
            this.proxy.ShowHostWindow(tour, string.Format(Resources.VisualizationTitle, this.workbook.Name), playTour, currentTour, "");
        }

        public ModelMetadata GetTableMetadata()
        {
            return this.model.GetModelTables();
        }

        public void RefreshAll()
        {
            this.workbook.RefreshAll();
        }

        public DataSource CreateDataSource(string name)
        {
            return new ExcelGeoDataSource(name, () => this.modelConnection);
        }

        public void PersistTour(Tour tour)
        {
            if (!this.IsInitialized)
                return;
            if (tour == null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "PersistTour(): tour=null, returning");
            }
            else
            {
                StringBuilder output = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(output);
                string tourXml;
                try
                {
                    tour.WriteXml(writer);
                    tourXml = (output).ToString();
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.Fail("PersistTour failed, returning false: ", ex);
                    return;
                }
                finally
                {
                    writer.Close();
                }
                Interlocked.Increment(ref this.toursPendingUpdate);
                ExcelTour excelTourToSave = new ExcelTour();
                excelTourToSave.Name = tour.Name;
                excelTourToSave.Description = tour.Description;
                excelTourToSave.TourHandle = tour.Handle;
                excelTourToSave.WorkbookState = this;
                if (tour.Scenes.Count != 0)
                    excelTourToSave.Base64Image = tour.Scenes[0].Frame.Base64Image;
                this.excelDispatcher.BeginInvoke((System.Action)(() =>
                {
                    int tourHandle = excelTourToSave.TourHandle;
                    string Id1 = string.Empty;
                    bool flag = false;
                    string Id2 = null;
                    try
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: updating tour list and deleting old tour xml", Id1, tourHandle);
                        ExcelTour excelTour = null;
                        lock (this.toursListSyncObject)
                        {
                            foreach (ExcelTour item_0 in this.Tours)
                            {
                                if (item_0.TourHandle == tourHandle)
                                {
                                    excelTour = item_0;
                                    break;
                                }
                            }
                            if (excelTour == null)
                            {
                                excelTour = excelTourToSave;
                                this.Tours.Add(excelTourToSave);
                            }
                            else
                            {
                                excelTour.Name = excelTourToSave.Name;
                                excelTour.Description = excelTourToSave.Description;
                                excelTour.Base64Image = excelTourToSave.Base64Image;
                                excelTour.XmlVersion = excelTourToSave.XmlVersion;
                                excelTour.MinimumCompatibleXmlVersion = excelTourToSave.MinimumCompatibleXmlVersion;
                                excelTourToSave.WorkbookState = null;
                            }
                            Interlocked.Decrement(ref this.toursPendingUpdate);
                            Id1 = excelTour.TourVersionId;
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: old Tour Id found (it is empty if there is no old tour)", Id1, tourHandle);
                        }
                        if (this.IsInitialized)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: adding new tour xml", Id1, tourHandle);
                            CustomXMLPart customXmlPart;
                            try
                            {
                                customXmlPart = this.workbook.CustomXMLParts.Add(tourXml, Missing.Value);
                            }
                            catch (NullReferenceException ex)
                            {
                                VisualizationTraceSource.Current.Fail("PersistTour(): adding tour xml, NullRefEx - throwing OOM, exception={0}", ex);
                                throw new OutOfMemoryException("PersistTour tour list xml failed", ex);
                            }
                            Id2 = customXmlPart.Id;
                            flag = true;
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: added new tour xml, newTourId='{2}'", Id1, tourHandle, Id2);
                            excelTour.TourVersionId = Id2;
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: updating workbook xml", Id1, tourHandle);
                            this.UpdateTourList(this.workbook);
                            flag = false;
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: updated workbook xml", Id1, tourHandle);
                            if (!string.IsNullOrEmpty(Id1))
                            {
                                lock (this.tourPartSyncObject)
                                {
                                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: deleting old tour xml", Id1, tourHandle);
                                    CustomXMLPart local_9 = this.workbook.CustomXMLParts.SelectByID(Id1);
                                    if (local_9 != null)
                                    {
                                        local_9.Delete();
                                    }
                                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: deleted old tour xml", Id1, tourHandle);
                                }
                            }
                        }
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: updating tour list and deleting old tour xml - done", Id1, tourHandle);
                    }
                    catch (Exception ex1)
                    {
                        VisualizationTraceSource.Current.Fail(string.Format("PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: Exception when updating tour list and deleting old tour xml", Id1, tourHandle), ex1);
                        if (flag)
                        {
                            try
                            {
                                lock (this.tourPartSyncObject)
                                {
                                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: deleting new tour xml, newTourId='{2}'", Id1, tourHandle, Id2);
                                    CustomXMLPart local_12 = this.workbook.CustomXMLParts.SelectByID(Id2);
                                    if (local_12 != null)
                                    {
                                        local_12.Delete();
                                    }
                                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: deleted new tour xml, newTourId='{2}'", Id1, tourHandle, Id2);
                                }
                            }
                            catch (Exception ex2)
                            {
                                VisualizationTraceSource.Current.Fail(string.Format("PersistTour(oldTourId='{0}', tourHandle={1})/BeginInvoke: Exception when deleting newTourXml, newTourId='{2}' in exception handler of updating tour list and deleting old tour xml", Id1, tourHandle, Id2), ex2);
                            }
                        }
                        if (this.OnWorkbookStateError == null)
                            return;
                        this.OnWorkbookStateError(this, new InternalErrorEventArgs(ex1));
                    }
                }));
            }
        }

        public void ShowHelp(int topicId)
        {
            if (this.workbook == null)
                return;
            this.workbook.Application.Assistance.ShowHelp("115344", "");
        }

        public void OpenTour(ExcelTour tour)
        {
            if (!this.IsInitialized || tour == null)
                return;
            if (tour.IsOpened)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Open Tour with Id: {0} - already currently open", tour.TourVersionId);
                this.proxy.ShowHostWindow(string.Format(Resources.VisualizationTitle, this.workbook.Name));
            }
            else
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Open Tour with Id: {0}", tour.TourVersionId);
                this.SetCurrentTour(tour, false);
            }
        }

        public void PlayTour(ExcelTour tour)
        {
            if (!this.IsInitialized)
                return;
            if (tour != null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Play Tour with Id: {0}", tour.TourVersionId);
                this.SetCurrentTour(tour, true);
            }
            else
                this.Explore(true);
        }

        public void DeleteTour(ExcelTour tour)
        {
            if (!this.IsInitialized || tour == null)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Delete Tour with Id: {0}", tour.TourVersionId);
            lock (this.toursListSyncObject)
                this.Tours.Remove(tour);
            lock (this.tourPartSyncObject)
            {
                CustomXMLPart local_0 = this.workbook.CustomXMLParts.SelectByID(tour.TourVersionId);
                if (local_0 != null)
                {
                    local_0.Delete();
                }
            }
            if (this.proxy != null && tour.TourHandle == this.proxy.CurrentTourHandle)
                this.proxy.CloseHostWindow(false);
            this.UpdateTourList(this.workbook);
        }

        public void DuplicateTour(ExcelTour tour)
        {
            if (!this.IsInitialized || tour == null || string.IsNullOrEmpty(tour.TourVersionId))
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Duplicate Tour with Id: {0}", tour.TourVersionId);
            lock (this.tourPartSyncObject)
            {
                CustomXMLPart local_0 = this.workbook.CustomXMLParts.SelectByID(tour.TourVersionId);
                if (local_0 != null)
                {
                    CustomXMLPart local_1 = this.workbook.CustomXMLParts.Add(local_0.XML, Missing.Value);
                    if (local_1 != null)
                    {
                        ExcelTour local_2 = new ExcelTour();
                        local_2.WorkbookState = this;
                        local_2.TourVersionId = local_1.Id;
                        local_2.XmlVersion = tour.XmlVersion;
                        local_2.MinimumCompatibleXmlVersion = tour.MinimumCompatibleXmlVersion;
                        local_2.Name = !string.IsNullOrEmpty(tour.Name) ? string.Format(Resources.TourDuplicateName, tour.Name) : string.Format(Resources.TourDuplicateName, Resources.NoTourName);
                        if (!string.IsNullOrEmpty(tour.Description))
                            local_2.Description = string.Copy(tour.Description);
                        if (!string.IsNullOrEmpty(tour.Base64Image))
                            local_2.Base64Image = string.Copy(tour.Base64Image);
                        lock (this.toursListSyncObject)
                            this.Tours.Insert(this.Tours.IndexOf(tour) + 1, local_2);
                        this.UpdateTourList(this.workbook);
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.DuplicateTour(): Added New Tour with Id: {0}", local_2.TourVersionId);
                    }
                    else
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "WorkbookState.DuplicateTour()CustomXMLParts.Add returned tourId = null for duplicated; original tour with Id {0} not duplicated", tour.TourVersionId);
                }
                else
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Tour with id {0} not found in the workbook.", tour.TourVersionId);
                    throw new TourDeserializationException(Resources.TourNotFound);
                }
            }
        }

        private void UpdateTourList(Workbook workbook)
        {
            if (workbook == null)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UpdateTourList(): starting");
            string XML = this.SerializeToXMLString();

            CustomXMLPart customXmlPart;
            try
            {
                customXmlPart = workbook.CustomXMLParts.Add(XML, Missing.Value);
            }
            catch (NullReferenceException ex)
            {
                VisualizationTraceSource.Current.Fail("UpdateTourList(): adding new xml, NullRefEx - throwing OOM, exception={0}", ex);
                throw new OutOfMemoryException("Updating tour list xml failed", ex);
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == -2147467259)
                {
                    VisualizationTraceSource.Current.Fail("UpdateTourList(): adding new xml, COMException with E_FAIL - assuming ex.Message is 'Not enough storage is available to complete this operation' - throwing OOM, exception={0}", ex);
                    throw new OutOfMemoryException("Updating tour list xml failed", ex);
                }
                throw;
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UpdateTourList(): added new xml");
            if (this.xmlPart != null)
            {
                try
                {
                    this.xmlPart.Delete();
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == -536602620)
                        VisualizationTraceSource.Current.Fail("UpdateTourList(): xmlPart.delete, COMException with 0xE0041804 - ignoring, exception={0}", ex);
                    else
                        throw;
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UpdateTourList(): removed old xml");
            }
            this.xmlPart = customXmlPart;
            if (this.latLonCache != null)
                this.latLonCache.Save(workbook);
            if (this.regionProvider != null)
                this.regionProvider.Save(workbook);
            if (this.customMapList != null)
                this.customMapList.Save(workbook);
            if (this.OnVisualizationXMLPartChanged != null)
                this.OnVisualizationXMLPartChanged(this, EventArgs.Empty);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UpdateTourList(): done");
        }

        public void DeleteAllData(Workbook workbook)
        {
            if (!this.IsInitialized)
                return;
            lock (this.toursListSyncObject)
            {
                foreach (ExcelTour item_0 in this.Tours)
                {
                    lock (this.tourPartSyncObject)
                    {
                        CustomXMLPart local_1 = workbook.CustomXMLParts.SelectByID(item_0.TourVersionId);
                        if (local_1 != null)
                        {
                            local_1.Delete();
                        }
                    }
                }
                this.Tours.Clear();
            }
            if (this.xmlPart != null)
            {
                this.xmlPart.Delete();
                this.xmlPart = null;
            }
            if (this.latLonCache != null)
                this.latLonCache.Delete();
            if (this.regionProvider != null)
                this.regionProvider.Delete();
            if (this.customMapList != null)
            {
                this.customMapList.DeleteAllMaps();
                this.customMapList.Delete();
            }
            if (this.OnVisualizationXMLPartChanged != null)
                this.OnVisualizationXMLPartChanged(this, EventArgs.Empty);
            this.UnInitialize(false);
        }

        public void UnInitialize(bool saveBeforeUnInitialize)
        {
            try
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize(): start");
                if (this.proxy != null)
                {
                    this.proxy.CloseHostWindow(saveBeforeUnInitialize);
                    this.proxy.OnInternalError -= this.proxyOnError.OnEvent;
                    this.proxy.OnHostWindowOpened -= this.proxyHostWindowOpened.OnEvent;
                    this.proxy.OnHostWindowClosed -= this.proxyHostWindowClosed.OnEvent;
                    this.proxy.Dispose();
                    this.proxy = null;
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize().DoEvents(): start");
                    System.Windows.Forms.Application.DoEvents();
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize().DoEvents(): done");
                }
                if (this.excelDispatcher != null)
                {
                    if (this.Tours != null)
                    {
                        try
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize().CleanTours: start");
                            this.excelDispatcher.BeginInvoke((System.Action)(() => this.Tours.Clear()));
                            System.Windows.Forms.Application.DoEvents();
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize().CleanTours: done");
                        }
                        catch (Exception ex)
                        {
                            VisualizationTraceSource.Current.Fail("Exception while clearing tours", ex);
                            if (this.OnWorkbookStateError != null)
                                this.OnWorkbookStateError(this, new InternalErrorEventArgs(ex));
                        }
                    }
                }
                if (this.regionProvider != null)
                {
                    this.regionProvider.Dispose();
                    this.regionProvider = null;
                }
                if (this.modelConnection != null)
                {
                    try
                    {
                        this.modelConnection.Close();
                        this.modelConnection = null;
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "UnInitialize(): error closing the model connection, ex={0}", ex);
                    }
                }
                if (this.workbook != null)
                {
                    //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "AfterSave").RemoveEventHandler(this.workbook, new WorkbookEvents_AfterSaveEventHandler(this.workbook_AfterSave));
                    //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "BeforeSave").RemoveEventHandler(this.workbook, new WorkbookEvents_BeforeSaveEventHandler(this.workbook_BeforeSave));
                    //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "BeforeClose").RemoveEventHandler(this.workbook, new WorkbookEvents_BeforeCloseEventHandler(this.workbook_BeforeClose));
                    //new ComAwareEventInfo(typeof(WorkbookEvents_Event), "ModelChange").RemoveEventHandler(this.workbook, new WorkbookEvents_ModelChangeEventHandler(this.workbook_ModelChange));
                    this.workbook = null;

                }
                this.OnVisualizationXMLPartChanged = null;
                this.OnWorkbookStateError = null;
                this.OnHostWindowOpened = null;
                this.OnHostWindowClosed = null;
                this.model = null;
                this.latLonCache = null;
                this.customMapList = null;
                this.xmlPart = null;
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail("Exception while uninitializing ClientProxy", ex);
            }
            finally
            {
                this.IsInitialized = false;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize(): done");
            }
        }

        public string SerializeToXMLString()
        {
            StringBuilder output = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(output);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(WorkbookState));
            try
            {
                xmlSerializer.Serialize(xmlWriter, this);
            }
            finally
            {
                xmlWriter.Close();
            }
            return (output).ToString();
        }

        public static WorkbookState CreateFromXMLPart(Workbook workbook, CustomXMLPart xmlPart)
        {
            string s = xmlPart == null || xmlPart.DocumentElement == null ? null : xmlPart.DocumentElement.XML;
            WorkbookState workbookState = null;
            if (!string.IsNullOrEmpty(s))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(WorkbookState));
                StringReader stringReader = new StringReader(s);
                try
                {
                    workbookState = (WorkbookState)xmlSerializer.Deserialize(stringReader);
                    workbookState.xmlPart = xmlPart;
                    if (workbookState.CustomColors == null)
                        workbookState.CustomColors = new List<Color4F>();
                    if (workbookState.Tours == null)
                        workbookState.Tours = new ObservableCollectionEx<ExcelTour>();
                    foreach (ExcelTour excelTour in workbookState.Tours)
                    {
                        excelTour.WorkbookState = workbookState;
                        if (excelTour.XmlVersion == 0)
                            excelTour.XmlVersion = 1;
                        if (excelTour.MinimumCompatibleXmlVersion == 0)
                            excelTour.MinimumCompatibleXmlVersion = excelTour.XmlVersion;
                        Guid result;
                        if (excelTour.TourId == null || !Guid.TryParse(excelTour.TourId, out result) || result == Guid.Empty)
                            excelTour.TourId = Guid.NewGuid().ToString();
                    }
                    workbookState.latLonCache = WorkbookLatLonCacheState.CreateFromXmlPart(workbook);
                    workbookState.regionProvider = WorkbookPolygonState.CreateFromXmlPart(workbook);
                    workbookState.customMapList = WorkbookCustomMapListState.CreateFromXmlPart(workbook);
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.Fail("CreateFromXMLPart(): Exception while loading the tour list or one of the caches, returning null", ex);
                    workbookState = null;
                }
                finally
                {
                    stringReader.Close();
                }
            }
            return workbookState;
        }

        protected ICustomMapCollection GetAndEnsureCustomMapList()
        {
            if (this.customMapList == null)
                this.customMapList = WorkbookCustomMapListState.CreateFromXmlPart(this.workbook);
            return this.customMapList.MapList;
        }
    }
}
