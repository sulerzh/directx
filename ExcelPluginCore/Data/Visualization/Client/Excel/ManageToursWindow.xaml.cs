using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.Client.Excel
{
    public partial class ManageToursWindow : Window
    {
        private ManageToursViewModel viewModel;
        private readonly WeakDelegate<ManageToursWindow, bool> deleteConfirmation;

        public ManageToursWindow()
        {
            this.InitializeComponent();
            this.deleteConfirmation = new WeakDelegate<ManageToursWindow, bool>(this)
            {
                OnInvokeAction = new Func<ManageToursWindow, bool>(ManageToursWindow.ConfirmDeletion)
            };
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ManageToursViewModel manageToursViewModel = e.OldValue as ManageToursViewModel;
            if (manageToursViewModel != null)
            {
                manageToursViewModel.CloseAction = (Action<bool>)null;
                manageToursViewModel.DeleteConfirmationAction = (Func<bool>)null;
                manageToursViewModel.ShowWaitState = (Action<bool>)null;
                manageToursViewModel.DisplayErrorMessage = (Action<string>)null;
                manageToursViewModel.DisplayWarningMessage = (Func<string, bool>)null;
            }
            this.viewModel = e.NewValue as ManageToursViewModel;
            if (this.viewModel == null)
                return;
            this.viewModel.CloseAction = new Action<bool>(this.CloseWindow);
            this.viewModel.DeleteConfirmationAction = new Func<bool>(this.deleteConfirmation.OnInvoke);
            this.viewModel.ShowWaitState = new Action<bool>(this.ShowWaitState);
            this.viewModel.DisplayErrorMessage = new Action<string>(this.DisplayErrorMessage);
            this.viewModel.DisplayWarningMessage = new Func<string, bool>(this.DisplayWarningMessage);
            ManageToursViewModel.UpdateCommandsOnTours(this.viewModel, (object)null, (NotifyCollectionChangedEventArgs)null);
        }

        private void CloseWindow(bool newTour)
        {
            if (this.viewModel != null)
                this.viewModel.CloseAction = (Action<bool>)null;
            try
            {
                this.DialogResult = new bool?(newTour);
            }
            catch
            {
            }
            this.ShowWaitState(false);
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.ShowWaitState(false);
            this.Close();
        }

        private static bool ConfirmDeletion(ManageToursWindow window)
        {
            return MessageBox.Show((Window)window,
                Microsoft.Data.Visualization.Client.Excel.Resources.DeleteTourMessage,
                Microsoft.Data.Visualization.Client.Excel.Resources.DeleteTourTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, 
                Microsoft.Data.Visualization.Client.Excel.Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None) == MessageBoxResult.Yes;
        }

        private void DisplayMessage(string message, MessageBoxImage messageBoxImage = MessageBoxImage.Hand)
        {
            int num = (int)MessageBox.Show((Window)this, message,
                Microsoft.Data.Visualization.Client.Excel.Resources.Product, 
                MessageBoxButton.OK, messageBoxImage, MessageBoxResult.OK, Microsoft.Data.Visualization.Client.Excel.Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
        }

        private void DisplayErrorMessage(string message)
        {
            int num = (int)MessageBox.Show((Window)this, message,
                Microsoft.Data.Visualization.Client.Excel.Resources.Product, 
                MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK, Microsoft.Data.Visualization.Client.Excel.Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None);
        }

        private bool DisplayWarningMessage(string message)
        {
            return MessageBox.Show((Window)this, message,
                Microsoft.Data.Visualization.Client.Excel.Resources.Product, 
                MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel, Microsoft.Data.Visualization.Client.Excel.Resources.Culture.TextInfo.IsRightToLeft ? MessageBoxOptions.RtlReading : MessageBoxOptions.None) == MessageBoxResult.OK;
        }

        private void ShowWaitState(bool waiting)
        {
            this.Cursor = waiting ? Cursors.Wait : Cursors.Arrow;
        }

        private void ManageToursMainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;
            this.ShowWaitState(true);
            this.Close();
        }
    }
}
