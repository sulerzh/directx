namespace ConsoleApplication1
{
    using Microsoft.Data.Visualization.Engine;
    using Microsoft.Data.Visualization.Engine.Graphics;
    using Microsoft.Data.Visualization.Utilities;
    using Microsoft.Data.Visualization.VisualizationCommon;
    using Microsoft.Data.Visualization.VisualizationControls;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using System.Xml;
    using System.Xml.Serialization;

    [Serializable, XmlRoot("Visualization", Namespace = "http://microsoft.data.visualization.Client.Excel/1.0", IsNullable = false)]
    public class TestState : ITourPersist, IModelWrapper, IDataSourceFactory, IHelpViewer
    {
        private int ConcurrencyLimit = Math.Min(Environment.ProcessorCount * 4, Environment.Is64BitProcess ? 0x20 : 0x10);
        private Dispatcher excelDispatcher;
        private const string HELP_TOPIC_ID = "115344";
        private LatLonCacheState latLonCache;
        private ClientProxy proxy;
        private readonly WeakEventListener<TestState, object, EventArgs> proxyHostWindowClosed;
        private readonly WeakEventListener<TestState, object, EventArgs> proxyHostWindowOpened;
        private readonly WeakEventListener<TestState, object, InternalErrorEventArgs> proxyOnError;
        private PolygonState regionProvider;
        private object tourPartSyncObject = new object();
        private object toursListSyncObject = new object();
        private int toursPendingUpdate;

        public event EventHandler OnHostWindowClosed;

        public event EventHandler OnHostWindowOpened;

        public event EventHandler OnVisualizationXMLPartChanged;

        public event EventHandler OnWorkbookClosing;

        public event InternalErrorEventHandler OnWorkbookStateError;

        public TestState()
        {
            this.proxyOnError = new WeakEventListener<TestState, object, InternalErrorEventArgs>(this)
            {
                OnEventAction = new Action<TestState, object, InternalErrorEventArgs>(TestState.proxy_OnInternalError)
            };
            this.proxyHostWindowOpened = new WeakEventListener<TestState, object, EventArgs>(this)
            {
                OnEventAction = new Action<TestState, object, EventArgs>(TestState.proxy_OnHostWindowOpened)
            };
            this.proxyHostWindowClosed = new WeakEventListener<TestState, object, EventArgs>(this)
            {
                OnEventAction = new Action<TestState, object, EventArgs>(TestState.proxy_OnHostWindowClosed)
            };
            ServicePointManager.DefaultConnectionLimit = this.ConcurrencyLimit;
            this.excelDispatcher = Dispatcher.CurrentDispatcher;
            this.toursPendingUpdate = 0;
            this.latLonCache = null;
            this.IsInitialized = false;
        }

        public void Explore(bool setNewTour)
        {
            if (this.IsInitialized)
            {
                bool addedToModel = false;
                string modelTableName = null;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                long num = 0L;
                ClientProxy proxy = this.proxy;
                if (this.proxy != null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): Calling proxy.FetchModelMetadata()");
                    proxy.FetchModelMetadata();
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore():Completed proxy.FetchModelMetadata(); total FetchModelMetadata time = {0} ms (incl CSDL fetch, parse, Orlando sampling and classification if those steps were done by call to FetchModelMetadata)", new object[] { stopwatch.ElapsedMilliseconds });
                    num += stopwatch.ElapsedMilliseconds;
                    stopwatch.Restart();
                }
                if (this.proxy != null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): Calling proxy.ShowHostWindow()");
                    if (setNewTour)
                    {
                        proxy.ShowHostWindow(null, string.Format(Properties.Resources.VisualizationTitle, string.Empty), false, false, "new");
                    }
                    else
                    {
                        proxy.ShowHostWindow("TestState");
                    }
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore():  proxy.ShowHostWindow() time = {0} ms", new object[] { stopwatch.ElapsedMilliseconds });
                    num += stopwatch.ElapsedMilliseconds;
                    stopwatch.Restart();
                }
                //if (this.proxy != null)
                //{
                //    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore(): Calling proxy.AddLayerDefinition()");
                //    proxy.AddLayerDefinition(addedToModel, modelTableName, null);
                //    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "WorkbookState.Explore():Completed proxy.AddLayerDefinition(); total AddLayerDefinition time = {0} ms (incl CSDL fetch, parse, Orlando sampling and classification if those steps were done by call to AddLayerDefinition)", new object[] { stopwatch.ElapsedMilliseconds });
                //    num += stopwatch.ElapsedMilliseconds;
                //    stopwatch.Restart();
                //}
            }
        }

        public ModelMetadata GetTableMetadata()
        {
            return null;
        }

        public bool Initialize()
        {
            if (this.IsInitialized)
            {
                return true;
            }

            Stopwatch stopwatch = new Stopwatch();
            CancellationTokenSource source = new CancellationTokenSource();
            stopwatch.Start();
            Task<GeospatialEndpoint.EndPoint> task = GeospatialEndpoint.FetchServiceEndpointsAsync(Properties.Resources.Culture.Name, new RegionInfo(Thread.CurrentThread.CurrentCulture.LCID).TwoLetterISORegionName, source.Token);
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "WorkbookState.Initialize(): endpoint svc request send time: {0} ms", new object[] { stopwatch.ElapsedMilliseconds });

            stopwatch.Restart();
            GeospatialEndpoint.EndPoint result = task.Result;
            stopwatch.Stop();
            elapsedMilliseconds += stopwatch.ElapsedMilliseconds;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "WorkbookState.Initialize(): endpoint fetch task completion wait time {0}, elapsed time processing fetch: {1} ms (incl request send time)", new object[] { stopwatch.ElapsedMilliseconds, elapsedMilliseconds });
            if (result.OtherError && result.HasNullEndpoints)
            {
                System.Windows.MessageBox.Show(string.Format(Properties.Resources.BingGeoPolError, result.StatusCode), Properties.Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Properties.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                return false;
            }
            if (result.ServerNotReachable && result.HasNullEndpoints)
            {
                System.Windows.MessageBox.Show(string.Format(Properties.Resources.BingGeoPolServerNotReachable, result.StatusCode), Properties.Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Properties.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
            }
            bool flag = false;
            if ((result.OtherError || result.ServerNotReachable) && !result.HasNullEndpoints)
            {
                System.Windows.MessageBox.Show(
                    string.Format(
                    Properties.Resources.BingGeoPolErrorUseExistingEndpoints, 
                    result.StatusCode), 
                    Properties.Resources.Product,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation, 
                    MessageBoxResult.OK, Properties.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                flag = true;
            }
            if (result.HasNullEndpoints)
            {
               System.Windows.MessageBox.Show(Properties.Resources.BingGeoPolLocationNotSupported, Properties.Resources.Product, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Properties.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
                return false;
            }

            if (this.latLonCache == null)
            {
                this.latLonCache = new LatLonCacheState();
            }
            if (this.regionProvider == null)
            {
                this.regionProvider = new PolygonState();
            }
            this.latLonCache.Initialize(result.BingMapKey, result.Geocoding, Properties.Resources.Culture);
            GeospatialDataProviders geospatialDataProviders = new GeospatialDataProviders
            {
                LatLonProvider = this.latLonCache.LatLonCache,
                RegionProvider = this.regionProvider.RegionProvider
            };
            BingMapResourceUri mapResourceUris = new BingMapResourceUri
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
            };
            this.proxy = new ClientProxy(this, this, this, this,null, geospatialDataProviders, mapResourceUris);
            this.proxy.OnInternalError += new InternalErrorEventHandler(this.proxyOnError.OnEvent);
            this.proxy.OnHostWindowOpened += new EventHandler(this.proxyHostWindowOpened.OnEvent);
            this.proxy.OnHostWindowClosed += new EventHandler(this.proxyHostWindowClosed.OnEvent);
            this.IsInitialized = true;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            GregorianCalendar gregorianCalendar = null;
            if (!(currentCulture.DateTimeFormat.Calendar is GregorianCalendar))
            {
                foreach (Calendar calendar in currentCulture.OptionalCalendars)
                {
                    GregorianCalendar gCalendar = calendar as GregorianCalendar;
                    if (gCalendar != null)
                    {
                        gregorianCalendar = gCalendar;
                        if (gCalendar.CalendarType == GregorianCalendarTypes.Localized)
                        {
                            break;
                        }
                    }
                }
                System.Windows.MessageBox.Show(Properties.Resources.CalendarChanged, Properties.Resources.Product, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK, Properties.Resources.Culture.TextInfo.IsRightToLeft ? System.Windows.MessageBoxOptions.RtlReading : System.Windows.MessageBoxOptions.None);
            }
            return this.proxy.Initialize(Properties.Resources.Culture, this.CustomColors, gregorianCalendar);
        }

        private static void proxy_OnHostWindowClosed(TestState state, object sender, EventArgs e)
        {
            if (state.OnHostWindowClosed != null)
            {
                state.OnHostWindowClosed(state, e);
            }
        }

        private static void proxy_OnHostWindowOpened(TestState state, object sender, EventArgs e)
        {
            if (state.OnHostWindowOpened != null)
            {
                state.OnHostWindowOpened(state, e);
            }
        }

        private static void proxy_OnInternalError(TestState state, object sender, InternalErrorEventArgs e)
        {
            if (state.OnWorkbookStateError != null)
            {
                try
                {
                    state.OnWorkbookStateError(state, e);
                }
                catch (Exception)
                {
                }
            }
        }

        public string SerializeToXMLString()
        {
            StringBuilder output = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(output);
            XmlSerializer serializer = new XmlSerializer(typeof(TestState));
            try
            {
                serializer.Serialize(xmlWriter, this);
            }
            finally
            {
                xmlWriter.Close();
            }
            return output.ToString();
        }

        public void UnInitialize(bool saveBeforeUnInitialize)
        {
            try
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize(): start");
                if (this.proxy != null)
                {
                    this.proxy.CloseHostWindow(saveBeforeUnInitialize);
                    this.proxy.OnInternalError -= new InternalErrorEventHandler(this.proxyOnError.OnEvent);
                    this.proxy.OnHostWindowOpened -= new EventHandler(this.proxyHostWindowOpened.OnEvent);
                    this.proxy.OnHostWindowClosed -= new EventHandler(this.proxyHostWindowClosed.OnEvent);
                    this.proxy.Dispose();
                    this.proxy = null;
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize().DoEvents(): start");
                    System.Windows.Forms.Application.DoEvents();
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize().DoEvents(): done");
                }

                this.OnVisualizationXMLPartChanged = null;
                this.OnWorkbookStateError = null;
                this.OnHostWindowOpened = null;
                this.OnHostWindowClosed = null;
                this.latLonCache = null;
            }
            catch (Exception exception3)
            {
                VisualizationTraceSource.Current.Fail("Exception while uninitializing ClientProxy", exception3);
            }
            finally
            {
                this.IsInitialized = false;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "UnInitialize(): done");
            }
        }

        public bool ConnectionsDisabled
        {
            get
            {
                return false;
            }
        }

        [XmlArrayItem("Color", typeof(Color4F)), XmlArray("Colors")]
        public List<Color4F> CustomColors { get; set; }

        [XmlIgnore]
        public bool IsInitialized { get; private set; }

        public DataSource CreateDataSource(string name)
        {
            throw new NotImplementedException();
        }

        public void PersistTour(Tour tour)
        {
            return;
        }


        public void RefreshAll()
        {
            return;
        }

        public void ShowHelp(int topicId)
        {
            return;
        }
    }
}

