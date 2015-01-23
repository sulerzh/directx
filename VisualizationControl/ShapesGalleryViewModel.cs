using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ShapesGalleryViewModel : ViewModelBase
  {
    private TaskPanelFieldsTabViewModel fieldsTab;
    private InstancedShape _SelectedShape;

    public ICommand ApplyShapeCommand { get; private set; }

    public string PropertyChartShapeSelectionEnabled
    {
      get
      {
        return "ChartShapeSelectionEnabled";
      }
    }

    public bool ChartShapeSelectionEnabled
    {
      get
      {
        if (this.fieldsTab.Model.SelectedLayer != null)
          return this.fieldsTab.Model.SelectedLayer.CanVisualShapeBeChanged;
        else
          return true;
      }
    }

    public string PropertySelectedShape
    {
      get
      {
        return "SelectedShape";
      }
    }

    public InstancedShape SelectedShape
    {
      get
      {
        if (this.fieldsTab.Model.SelectedLayer != null && this.fieldsTab.Model.SelectedLayer.VisualShape.HasValue)
          return this.fieldsTab.Model.SelectedLayer.VisualShape.Value;
        else
          return InstancedShape.Square;
      }
      set
      {
        if (!this.SetProperty<InstancedShape>(this.PropertySelectedShape, ref this._SelectedShape, value, false) || this.fieldsTab.Model.SelectedLayer == null)
          return;
        this.fieldsTab.Model.SelectedLayer.VisualShape = new InstancedShape?(value);
      }
    }

    public ShapesGalleryViewModel(TaskPanelFieldsTabViewModel fieldsTabViewModel)
    {
      this.fieldsTab = fieldsTabViewModel;
      this.fieldsTab.Model.PropertyChanging += new PropertyChangingEventHandler(this.OnFieldsTabPropertyChanging);
      this.fieldsTab.Model.PropertyChanged += new PropertyChangedEventHandler(this.OnFieldsTabPropertyChanged);
      if (this.fieldsTab.Model.SelectedLayer != null)
        this.fieldsTab.Model.SelectedLayer.FieldListPicker.PropertyChanged += new PropertyChangedEventHandler(this.OnFieldListPickerPropertyChanged);
      this.ApplyShapeCommand = (ICommand) new DelegatedCommand<InstancedShape>(new Action<InstancedShape>(this.OnApplyShape));
    }

    public void OnApplyShape(InstancedShape shape)
    {
      if (this.fieldsTab.Model.SelectedLayer == null)
        return;
      this.fieldsTab.Model.SelectedLayer.VisualShape = new InstancedShape?(shape);
    }

    private void OnFieldsTabPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.fieldsTab.Model.PropertySelectedLayer))
        return;
      if (this.fieldsTab.Model.SelectedLayer != null)
        this.fieldsTab.Model.SelectedLayer.FieldListPicker.PropertyChanged += new PropertyChangedEventHandler(this.OnFieldListPickerPropertyChanged);
      this.RaisePropertyChanged(this.PropertySelectedShape);
      this.RaisePropertyChanged(this.PropertyChartShapeSelectionEnabled);
    }

    private void OnFieldsTabPropertyChanging(object sender, PropertyChangingEventArgs e)
    {
      if (!(e.PropertyName == this.fieldsTab.Model.PropertySelectedLayer) || this.fieldsTab.Model.SelectedLayer == null)
        return;
      this.fieldsTab.Model.SelectedLayer.FieldListPicker.PropertyChanged -= new PropertyChangedEventHandler(this.OnFieldListPickerPropertyChanged);
    }

    private void OnFieldListPickerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == FieldListPickerViewModel.PropertyCanVisualShapeBeChanged))
        return;
      this.RaisePropertyChanged(this.PropertyChartShapeSelectionEnabled);
    }
  }
}
