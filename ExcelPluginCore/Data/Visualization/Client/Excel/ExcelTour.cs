using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.VisualizationControls;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.Client.Excel
{
    [Serializable]
    public class ExcelTour : CompositePropertyChangeNotificationBase
    {
        public const int CurrentXmlVersion = 3;
        public const int CurrentMinimumCompatibleXmlVersion = 3;
        private readonly WeakEventListener<ExcelTour, object, EventArgs> onHostWindowOpened;
        private readonly WeakEventListener<ExcelTour, object, EventArgs> onHostWindowClosed;
        private WorkbookState workbookState;
        [XmlIgnore]
        public ExcelTourOperationEventHandler OnExcelTourOperation;
        private string _Name;
        private string _Description;
        private string _Base64Image;
        [XmlIgnore]
        public Action<bool> ShowWaitState;
        [XmlIgnore]
        public Action<string> DisplayErrorMessage;
        [XmlIgnore]
        public Func<string, bool> DisplayWarningMessage;

        [XmlIgnore]
        public ICommand OpenCommand { get; set; }

        [XmlIgnore]
        public ICommand PlayCommand { get; set; }

        [XmlIgnore]
        public ICommand DeleteCommand { get; set; }

        [XmlIgnore]
        public ICommand DuplicateCommand { get; set; }

        [XmlIgnore]
        public bool IsOpened
        {
            get
            {
                if (this.WorkbookState != null)
                    return this.WorkbookState.CurrentTour == this;
                else
                    return false;
            }
        }

        [XmlIgnore]
        public bool IsWindowOpened
        {
            get
            {
                if (this.WorkbookState != null)
                    return this.WorkbookState.CurrentTour != null;
                else
                    return false;
            }
        }

        [XmlIgnore]
        public WorkbookState WorkbookState
        {
            get
            {
                return this.workbookState;
            }
            set
            {
                if (this.workbookState != null)
                {
                    this.workbookState.OnHostWindowClosed -= this.onHostWindowClosed.OnEvent;
                    this.workbookState.OnHostWindowOpened -= this.onHostWindowOpened.OnEvent;
                }
                this.workbookState = value;
                if (this.workbookState == null)
                    return;
                this.workbookState.OnHostWindowClosed += this.onHostWindowClosed.OnEvent;
                this.workbookState.OnHostWindowOpened += this.onHostWindowOpened.OnEvent;
            }
        }

        public static string PropertyName
        {
            get
            {
                return "Name";
            }
        }

        [XmlAttribute("Name")]
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                base.SetProperty<string>(ExcelTour.PropertyName, ref this._Name, value);
            }
        }

        [XmlAttribute("Id")]
        public string TourVersionId { get; set; }

        [XmlAttribute("TourId")]
        public string TourId { get; set; }

        [XmlAttribute("XmlVer")]
        public int XmlVersion { get; set; }

        [XmlAttribute("MinXmlVer")]
        public int MinimumCompatibleXmlVersion { get; set; }

        [XmlIgnore]
        public int TourHandle { get; set; }

        public static string PropertyDescription
        {
            get
            {
                return "Description";
            }
        }

        [XmlElement("Description")]
        public string Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                base.SetProperty<string>(ExcelTour.PropertyDescription, ref this._Description, value);
            }
        }

        [XmlElement("Image")]
        public string Base64Image
        {
            get
            {
                return this._Base64Image;
            }
            set
            {
                this.RaisePropertyChanging(ExcelTour.PropertyImage);
                this._Base64Image = value;
                this.RaisePropertyChanged(ExcelTour.PropertyImage);
            }
        }

        public static string PropertyImage
        {
            get
            {
                return "Image";
            }
        }

        [XmlIgnore]
        public BitmapSource Image
        {
            get
            {
                return this.Base64ToImage(this.Base64Image);
            }
        }

        [XmlIgnore]
        public Func<bool> DeleteConfirmationAction { get; set; }

        public ExcelTour()
        {
            this.onHostWindowClosed = new WeakEventListener<ExcelTour, object, EventArgs>(this)
            {
                OnEventAction = ExcelTour.OnHostWindowClosedHandler
            };
            this.onHostWindowOpened = new WeakEventListener<ExcelTour, object, EventArgs>(this)
            {
                OnEventAction = ExcelTour.OnHostWindowOpenedHandler
            };
            this.OpenCommand = new DelegatedCommand(this.OnOpenTour);
            this.PlayCommand = new DelegatedCommand(this.OnPlayTour);
            this.DeleteCommand = new DelegatedCommand(this.OnDeleteTour);
            this.DuplicateCommand = new DelegatedCommand(this.OnDuplicateTour);
            this.TourHandle = 0;
            this.TourId = Guid.NewGuid().ToString();
            this.XmlVersion = 3;
            this.MinimumCompatibleXmlVersion = 3;
        }

        private BitmapSource Base64ToImage(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return null;
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(base64)))
                return BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        private bool CheckXmlVersion()
        {
            if (this.MinimumCompatibleXmlVersion > 3)
            {
                this.DisplayErrorMessage(Resources.TourXmlIncompatible);
                return false;
            }
            else if (this.XmlVersion > 3)
                return this.DisplayWarningMessage(Resources.TourXmlVersionHigher);
            else
                return true;
        }

        private void OnOpenTour()
        {
            if (this.WorkbookState == null)
                return;
            this.ShowWaitState(true);
            try
            {
                if (this.IsOpened || this.CheckXmlVersion())
                {
                    this.WorkbookState.OpenTour(this);
                }
                else
                {
                    this.ShowWaitState(false);
                    return;
                }
            }
            catch (TourDeserializationException ex)
            {
                this.DisplayErrorMessage(ex.Message);
                this.ShowWaitState(false);
                return;
            }
            if (this.OnExcelTourOperation != null)
                this.OnExcelTourOperation(this, new ExcelTourOperationEventArgs(ExcelTourOperation.Open));
            this.ShowWaitState(false);
        }

        private void OnPlayTour()
        {
            if (this.WorkbookState == null)
                return;
            this.ShowWaitState(true);
            try
            {
                if (this.IsOpened || this.CheckXmlVersion())
                {
                    this.WorkbookState.PlayTour(this);
                }
                else
                {
                    this.ShowWaitState(false);
                    return;
                }
            }
            catch (TourDeserializationException ex)
            {
                this.DisplayErrorMessage(ex.Message);
                this.ShowWaitState(false);
                return;
            }
            if (this.OnExcelTourOperation != null)
                this.OnExcelTourOperation(this, new ExcelTourOperationEventArgs(ExcelTourOperation.Play));
            this.ShowWaitState(false);
        }

        private void OnDeleteTour()
        {
            if (this.WorkbookState == null || this.DeleteConfirmationAction == null || !this.DeleteConfirmationAction())
                return;
            this.ShowWaitState(true);
            this.WorkbookState.DeleteTour(this);
            if (this.OnExcelTourOperation != null)
                this.OnExcelTourOperation(this, new ExcelTourOperationEventArgs(ExcelTourOperation.Delete));
            this.ShowWaitState(false);
        }

        private void OnDuplicateTour()
        {
            if (this.WorkbookState == null)
                return;
            this.ShowWaitState(true);
            try
            {
                this.WorkbookState.DuplicateTour(this);
            }
            catch (TourDeserializationException ex)
            {
                this.DisplayErrorMessage(ex.Message);
                this.ShowWaitState(false);
                return;
            }
            if (this.OnExcelTourOperation != null)
                this.OnExcelTourOperation(this, new ExcelTourOperationEventArgs(ExcelTourOperation.Duplicate));
            this.ShowWaitState(false);
        }

        private static void OnHostWindowOpenedHandler(ExcelTour tour, object sender, EventArgs args)
        {
            tour.RaisePropertyChanged("IsOpened");
            tour.RaisePropertyChanged("IsWindowOpened");
        }

        private static void OnHostWindowClosedHandler(ExcelTour tour, object sender, EventArgs args)
        {
            tour.RaisePropertyChanged("IsOpened");
            tour.RaisePropertyChanged("IsWindowOpened");
        }
    }
}
