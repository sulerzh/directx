using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public partial class GeocodingReportView : UserControl
    {

        static GeocodingReportView()
        {
            FrameworkElement.DataContextProperty.AddOwner(typeof(DataGridColumn));
            FrameworkElement.DataContextProperty.OverrideMetadata(typeof(DataGrid), (PropertyMetadata)new FrameworkPropertyMetadata((object)null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(GeocodingReportView.OnDataContextChanged)));
        }

        public GeocodingReportView()
        {
            this.InitializeComponent();
        }

        private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = d as DataGrid;
            if (dataGrid == null)
                return;
            foreach (DependencyObject dependencyObject in (Collection<DataGridColumn>)dataGrid.Columns)
                dependencyObject.SetValue(FrameworkElement.DataContextProperty, e.NewValue);
        }

        private void root_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;
            if (this.CurrentPageTextBox.IsFocused)
            {
                e.Handled = true;
            }
            else
            {
                if (!this.cancelButton.Command.CanExecute((object)null))
                    return;
                this.cancelButton.Command.Execute((object)null);
            }
        }

    }
}
