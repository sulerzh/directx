using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Client.Excel
{
    public class ManageToursViewModel : ViewModelBase
    {
        private readonly object syncObject = new object();
        private readonly WeakEventListener<ManageToursViewModel, object, NotifyCollectionChangedEventArgs> onCollectionChanged;
        private readonly WeakEventListener<ManageToursViewModel, object, ExcelTourOperationEventArgs> onExcelTourOperation;
        private readonly WorkbookState workbookState;
        public Action<bool> ShowWaitState;
        public Action<string> DisplayErrorMessage;
        public Func<string, bool> DisplayWarningMessage;

        public ICommand NewTourCommand { get; private set; }

        public Action<bool> CloseAction { get; set; }

        public Func<bool> DeleteConfirmationAction { get; set; }

        public ObservableCollectionEx<ExcelTour> Tours
        {
            get
            {
                return this.workbookState.Tours;
            }
        }

        public ManageToursViewModel(WorkbookState workbookState)
        {
            this.workbookState = workbookState;
            this.onCollectionChanged = new WeakEventListener<ManageToursViewModel, object, NotifyCollectionChangedEventArgs>(this)
            {
                OnEventAction = ManageToursViewModel.UpdateCommandsOnTours
            };
            this.onExcelTourOperation = new WeakEventListener<ManageToursViewModel, object, ExcelTourOperationEventArgs>(this)
            {
                OnEventAction = ManageToursViewModel.OnExcelTourOperation
            };
            this.workbookState.Tours.CollectionChanged += this.onCollectionChanged.OnEvent;
            ManageToursViewModel.UpdateCommandsOnTours(this, null, null);
            this.NewTourCommand = new DelegatedCommand(this.OnNewTour);
        }

        internal static void UpdateCommandsOnTours(ManageToursViewModel viewModel, object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            lock (viewModel.syncObject)
            {
                foreach (ExcelTour tour in viewModel.Tours)
                {
                    tour.OnExcelTourOperation += viewModel.onExcelTourOperation.OnEvent;
                    tour.DeleteConfirmationAction = viewModel.DeleteConfirmationAction;
                    tour.DisplayErrorMessage = viewModel.DisplayErrorMessage;
                    tour.DisplayWarningMessage = viewModel.DisplayWarningMessage;
                    tour.ShowWaitState = viewModel.ShowWaitState;
                }
            }
        }

        private static void OnExcelTourOperation(ManageToursViewModel viewModel, object sender, ExcelTourOperationEventArgs e)
        {
            if (e == null || e.Operation != ExcelTourOperation.Open && e.Operation != ExcelTourOperation.Play || viewModel.CloseAction == null)
                return;
            viewModel.CloseAction(false);
        }

        public void OnNewTour()
        {
            try
            {
                this.ShowWaitState(true);
                this.workbookState.Explore(true);
            }
            finally
            {
                if (this.CloseAction != null)
                    this.CloseAction(true);
                this.ShowWaitState(false);
            }
        }
    }
}
