using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class LayerSettingsViewModel : ViewModelBase
  {
    public string PropertyAnyColorsOverridden = "AnyColorsOverridden";
    private IThemeService _ThemeService;
    private GeoVisualization _GeoVisualization;
    private bool _AnyColorsOverridden;
    private bool _IsSelectedColorOverridden;
    private IList<Tuple<TableField, AggregationFunction>> Measures;
    public string[] _ColorScopeDisplayStrings;
    private FieldWellVisualizationViewModel _FieldWellVisualizationViewModel;
    private int _SelectedColorScopeIndex;
    private bool _SettingsEnabled;
    private bool _ColorPickerVisible;
    private bool _ScalesEnabled;
    private bool _LockScalesEnabled;
    private bool _LockScalesVisible;
    private bool _DataDimensionScaleVisible;
    private double _DataDimensionMaxScale;
    private bool _FixedDimensionScaleVisible;
    private string _DataDimensionScaleLabel;
    private string _FixedDimensionScaleLabel;
    private bool _LockScales;
    private bool _DisplayNullValues;
    private bool _DisplayZeroValues;
    private bool _DisplayNegativeValues;
    private double _DataDimensionScale;
    private double _FixedDimensionScale;
    private double _OpacityFactor;

    public ILayerManagerViewModel Model { get; private set; }

    public bool AnyColorsOverridden
    {
      get
      {
        return this._AnyColorsOverridden;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyAnyColorsOverridden, ref this._AnyColorsOverridden, value, false);
      }
    }

    public string PropertyColorScopeDisplayStrings
    {
      get
      {
        return "ColorScopeDisplayStrings";
      }
    }

    public string[] ColorScopeDisplayStrings
    {
      get
      {
        return this._ColorScopeDisplayStrings;
      }
      private set
      {
        if (!this.SetProperty<string[]>(this.PropertyColorScopeDisplayStrings, ref this._ColorScopeDisplayStrings, value, false))
          return;
        this.SelectedColorScopeIndex = 0;
      }
    }

    public string PropertySelectedColorScopeIndex
    {
      get
      {
        return "SelectedColorScopeIndex";
      }
    }

    public int SelectedColorScopeIndex
    {
      get
      {
        return this._SelectedColorScopeIndex;
      }
      set
      {
        if (!this.SetProperty<int>(this.PropertySelectedColorScopeIndex, ref this._SelectedColorScopeIndex, value, false) || value < 0 || value >= this.GetColorScopeCount())
          return;
        this.RefreshSelectedColorState();
      }
    }

    public string PropertyLayerExists
    {
      get
      {
        return "LayerExists";
      }
    }

    public bool LayerExists
    {
      get
      {
        return this._GeoVisualization != null;
      }
    }

    public ColorPickerViewModel ColorPickerViewModel { get; private set; }

    public string PropertySettingsEnabled
    {
      get
      {
        return "SettingsEnabled";
      }
    }

    public bool SettingsEnabled
    {
      get
      {
        return this._SettingsEnabled;
      }
      private set
      {
        this.SetProperty<bool>(this.PropertySettingsEnabled, ref this._SettingsEnabled, value, false);
      }
    }

    public string PropertyColorPickerVisible
    {
      get
      {
        return "ColorPickerVisible";
      }
    }

    public bool ColorPickerVisible
    {
      get
      {
        return this._ColorPickerVisible;
      }
      private set
      {
        this.SetProperty<bool>(this.PropertyColorPickerVisible, ref this._ColorPickerVisible, value, false);
      }
    }

    public string PropertyScalesEnabled
    {
      get
      {
        return "ScalesEnabled";
      }
    }

    public bool ScalesEnabled
    {
      get
      {
        return this._ScalesEnabled;
      }
      private set
      {
        this.SetProperty<bool>(this.PropertyScalesEnabled, ref this._ScalesEnabled, value, false);
      }
    }

    public string PropertyLockScalesEnabled
    {
      get
      {
        return "LockScalesEnabled";
      }
    }

    public bool LockScalesEnabled
    {
      get
      {
        return this._LockScalesEnabled;
      }
      private set
      {
        this.SetProperty<bool>(this.PropertyLockScalesEnabled, ref this._LockScalesEnabled, value, false);
      }
    }

    public string PropertySeparatorVisible
    {
      get
      {
        return "SeparatorVisible";
      }
    }

    public bool SeparatorVisible
    {
      get
      {
        return this.ColorPickerVisible;
      }
    }

    public string PropertyLockScalesVisible
    {
      get
      {
        return "LockScalesVisible";
      }
    }

    public bool LockScalesVisible
    {
      get
      {
        return this._LockScalesVisible;
      }
      private set
      {
        this.SetProperty<bool>(this.PropertyLockScalesVisible, ref this._LockScalesVisible, value, false);
      }
    }

    public string PropertyDataDimensionScaleVisible
    {
      get
      {
        return "DataDimensionScaleVisible";
      }
    }

    public bool DataDimensionScaleVisible
    {
      get
      {
        return this._DataDimensionScaleVisible;
      }
      private set
      {
        this.SetProperty<bool>(this.PropertyDataDimensionScaleVisible, ref this._DataDimensionScaleVisible, value, false);
      }
    }

    public string PropertyDataDimensionMaxScale
    {
      get
      {
        return "DataDimensionMaxScale";
      }
    }

    public double DataDimensionMaxScale
    {
      get
      {
        return this._DataDimensionMaxScale;
      }
      private set
      {
        this.SetProperty<double>(this.PropertyDataDimensionMaxScale, ref this._DataDimensionMaxScale, value, false);
      }
    }

    public string PropertyFixedDimensionScaleVisible
    {
      get
      {
        return "FixedDimensionScaleVisible";
      }
    }

    public bool FixedDimensionScaleVisible
    {
      get
      {
        return this._FixedDimensionScaleVisible;
      }
      private set
      {
        this.SetProperty<bool>(this.PropertyFixedDimensionScaleVisible, ref this._FixedDimensionScaleVisible, value, false);
      }
    }

    public string PropertyDataDimensionScaleLabel
    {
      get
      {
        return "DataDimensionScaleLabel";
      }
    }

    public string DataDimensionScaleLabel
    {
      get
      {
        return this._DataDimensionScaleLabel;
      }
      private set
      {
        this.SetProperty<string>(this.PropertyDataDimensionScaleLabel, ref this._DataDimensionScaleLabel, value, false);
      }
    }

    public string PropertyFixedDimensionScaleLabel
    {
      get
      {
        return "FixedDimensionScaleLabel";
      }
    }

    public string FixedDimensionScaleLabel
    {
      get
      {
        return this._FixedDimensionScaleLabel;
      }
      private set
      {
        this.SetProperty<string>(this.PropertyFixedDimensionScaleLabel, ref this._FixedDimensionScaleLabel, value, false);
      }
    }

    public string PropertyLockScales
    {
      get
      {
        return "LockScales";
      }
    }

    public bool LockScales
    {
      get
      {
        return this._LockScales;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyLockScales, ref this._LockScales, value, false);
        this.ScalesEnabled = !value && this._GeoVisualization != null;
        if (this._GeoVisualization == null)
          return;
        this._GeoVisualization.LockScales = value;
      }
    }

    public string PropertyDisplayNullValues
    {
      get
      {
        return "DisplayNullValues";
      }
    }

    public bool DisplayNullValues
    {
      get
      {
        return this._DisplayNullValues;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyDisplayNullValues, ref this._DisplayNullValues, value, false);
        if (this._GeoVisualization == null)
          return;
        this._GeoVisualization.DisplayNullValues = value;
      }
    }

    public string PropertyDisplayZeroValues
    {
      get
      {
        return "DisplayZeroValues";
      }
    }

    public bool DisplayZeroValues
    {
      get
      {
        return this._DisplayZeroValues;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyDisplayZeroValues, ref this._DisplayZeroValues, value, false);
        if (this._GeoVisualization == null)
          return;
        this._GeoVisualization.DisplayZeroValues = value;
      }
    }

    public string PropertyDisplayNegativeValues
    {
      get
      {
        return "DisplayNegativeValues";
      }
    }

    public bool DisplayNegativeValues
    {
      get
      {
        return this._DisplayNegativeValues;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyDisplayNegativeValues, ref this._DisplayNegativeValues, value, false);
        if (this._GeoVisualization == null)
          return;
        this._GeoVisualization.DisplayNegativeValues = value;
      }
    }

    public string PropertyDataDimensionScale
    {
      get
      {
        return "DataDimensionScale";
      }
    }

    public double DataDimensionScale
    {
      get
      {
        return this._DataDimensionScale;
      }
      set
      {
        this.SetProperty<double>(this.PropertyDataDimensionScale, ref this._DataDimensionScale, value, false);
        if (this._GeoVisualization == null)
          return;
        this._GeoVisualization.DataDimensionScale = value;
      }
    }

    public string PropertyFixedDimensionScale
    {
      get
      {
        return "FixedDimensionScale";
      }
    }

    public double FixedDimensionScale
    {
      get
      {
        return this._FixedDimensionScale;
      }
      set
      {
        this.SetProperty<double>(this.PropertyFixedDimensionScale, ref this._FixedDimensionScale, value, false);
        if (this._GeoVisualization == null)
          return;
        this._GeoVisualization.FixedDimensionScale = value;
      }
    }

    public string PropertyOpacityFactor
    {
      get
      {
        return "OpacityFactor";
      }
    }

    public double OpacityFactor
    {
      get
      {
        return this._OpacityFactor;
      }
      set
      {
        this.SetProperty<double>(this.PropertyOpacityFactor, ref this._OpacityFactor, value, false);
        if (this._GeoVisualization == null)
          return;
        this._GeoVisualization.OpacityFactor = value;
      }
    }

    public ICommand ResetAllColorsCommand { get; private set; }

    public LayerSettingsViewModel(ILayerManagerViewModel layerManagerViewModel, IThemeService themeService, List<Color4F> customColors)
    {
      this.Model = layerManagerViewModel;
      this._ThemeService = themeService;
      this.ColorPickerViewModel = new ColorPickerViewModel(customColors);
      this.ColorPickerViewModel.ResetColorCommand = (ICommand) new DelegatedCommand((Action) (() =>
      {
        this._IsSelectedColorOverridden = false;
        this.ResetColorForScope(this.SelectedColorScopeIndex);
        this.RefreshSelectedColorState();
      }), (Predicate) (() => this._IsSelectedColorOverridden));
      this.ResetAllColorsCommand = (ICommand) new DelegatedCommand(new Action(this.ResetAllColors), (Predicate) (() => this.AnyColorsOverridden));
    }

    private void SetGeoVisualization(GeoVisualization value)
    {
      if (this._GeoVisualization != null)
      {
        this._GeoVisualization.DisplayPropertiesChanged -= new Action<LayerManager.Settings>(this.OnDisplayPropertiesChanged);
        this._GeoVisualization.DataUpdateCompleted -= new Action<bool>(this.OnDataUpdateCompleted);
        this._ThemeService.OnThemeChanged -= new Action(this.ThemeService_OnThemeChanged);
        this.ColorPickerViewModel.SelectedColorChangedByUser -= new Action<Color4F>(this.OnSelectedColorChanged);
      }
      this._GeoVisualization = value;
      if (value != null)
      {
        value.DisplayPropertiesChanged += new Action<LayerManager.Settings>(this.OnDisplayPropertiesChanged);
        value.DataUpdateCompleted += new Action<bool>(this.OnDataUpdateCompleted);
        this.DisplayNullValues = value.DisplayNullValues;
        this.DisplayZeroValues = value.DisplayZeroValues;
        this.DisplayNegativeValues = value.DisplayNegativeValues;
        this.DataDimensionScale = value.DataDimensionScale;
        this.FixedDimensionScale = value.FixedDimensionScale;
        this.OpacityFactor = value.OpacityFactor;
        this.RefreshColorScopes();
        this.SetScaleLabels(value.VisualType);
        this.ColorPickerVisible = value.VisualType != LayerType.HeatMapChart;
        this.LockScales = value.LockScales;
        this.LockScalesEnabled = value.CanLockScales;
        this.ScalesEnabled = !this.LockScales;
        this.SettingsEnabled = true;
        this.ColorPickerViewModel.SelectedColorChangedByUser += new Action<Color4F>(this.OnSelectedColorChanged);
        this._ThemeService.OnThemeChanged += new Action(this.ThemeService_OnThemeChanged);
      }
      else
      {
        this.DataDimensionScale = 1.0;
        this.FixedDimensionScale = 1.0;
        this.OpacityFactor = 1.0;
        this.LockScales = false;
        this.DisplayZeroValues = true;
        this.DisplayNegativeValues = true;
        this.DisplayNullValues = false;
        this.ScalesEnabled = false;
        this.SettingsEnabled = false;
        this.LockScalesEnabled = false;
        this.LockScalesVisible = false;
        this.DataDimensionScaleVisible = false;
        this.FixedDimensionScaleVisible = false;
        this.ColorPickerVisible = false;
      }
      this.RaisePropertyChanged(this.PropertyLayerExists);
    }

    private void ThemeService_OnThemeChanged()
    {
      if (this._IsSelectedColorOverridden)
        return;
      this.RefreshSelectedColorState();
    }

    private void OnSelectedColorChanged(Color4F newColor)
    {
      GeoVisualization geoVisualization = this._GeoVisualization;
      if (geoVisualization == null)
        return;
      if (this.ColorScopeDisplayStrings == null)
        geoVisualization.LayerColorOverride = new Color4F?(newColor);
      else
        geoVisualization.SetColorForSeries(this.SelectedColorScopeIndex, newColor);
      this._IsSelectedColorOverridden = true;
      this.AnyColorsOverridden = true;
    }

    private void OnDisplayPropertiesChanged(LayerManager.Settings settings)
    {
      if (this._GeoVisualization == null)
        return;
      this.DisplayNullValues = this._GeoVisualization.DisplayNullValues;
      this.DisplayZeroValues = this._GeoVisualization.DisplayZeroValues;
      this.DisplayNegativeValues = this._GeoVisualization.DisplayNegativeValues;
      this.DataDimensionScale = this._GeoVisualization.DataDimensionScale;
      this.FixedDimensionScale = this._GeoVisualization.FixedDimensionScale;
      this.OpacityFactor = this._GeoVisualization.OpacityFactor;
      this.LockScales = this._GeoVisualization.LockScales;
      this.LockScalesEnabled = this._GeoVisualization.CanLockScales;
      this.SetScaleLabels(this._GeoVisualization.VisualType);
      this.RefreshColorScopes();
    }

    private void OnDataUpdateCompleted(bool canceled)
    {
      if (this._GeoVisualization == null)
        return;
      this.DataDimensionScale = this._GeoVisualization.DataDimensionScale;
      this.FixedDimensionScale = this._GeoVisualization.FixedDimensionScale;
      this.LockScales = this._GeoVisualization.LockScales;
      this.LockScalesEnabled = this._GeoVisualization.CanLockScales;
      this.OpacityFactor = this._GeoVisualization.OpacityFactor;
      this.RefreshColorScopes();
    }

    private static string GetDisplayStringForMeasure(Tuple<TableField, AggregationFunction> m)
    {
      TableField tableField = m.Item1;
      if (tableField is TableMeasure)
        return tableField.Name;
      if (tableField is TableColumn)
        return string.Format(Resources.FieldWellHeightAggregatedNameFormat, (object) tableField.Name, (object) AggregationFunctionExtensions.DisplayString(m.Item2));
      else
        return (string) null;
    }

    private void RefreshColorScopes()
    {
      GeoVisualization geoVisualization = this._GeoVisualization;
      if (geoVisualization == null)
        return;
      IEnumerable<string> categories = geoVisualization.Categories;
      IList<Tuple<TableField, AggregationFunction>> list = (IList<Tuple<TableField, AggregationFunction>>) geoVisualization.Measures;
      if (categories != null && Enumerable.Any<string>(categories))
      {
        this.Measures = (IList<Tuple<TableField, AggregationFunction>>) null;
        this.ColorScopeDisplayStrings = Enumerable.ToArray<string>(categories);
      }
      else if (list != null && Enumerable.Any<Tuple<TableField, AggregationFunction>>((IEnumerable<Tuple<TableField, AggregationFunction>>) list))
      {
        this.Measures = list;
        this.ColorScopeDisplayStrings = Enumerable.ToArray<string>(Enumerable.Select<Tuple<TableField, AggregationFunction>, string>((IEnumerable<Tuple<TableField, AggregationFunction>>) this.Measures, (Func<Tuple<TableField, AggregationFunction>, string>) (m => LayerSettingsViewModel.GetDisplayStringForMeasure(m))));
      }
      else
      {
        this.Measures = (IList<Tuple<TableField, AggregationFunction>>) null;
        this.ColorScopeDisplayStrings = (string[]) null;
      }
      this.RefreshSelectedColorState();
      this.AnyColorsOverridden = geoVisualization.ColorsOverridden;
    }

    private void RefreshSelectedColorState()
    {
      this.ColorPickerViewModel.SetSelectedColor(this.GetColorForScope(this.SelectedColorScopeIndex, out this._IsSelectedColorOverridden));
    }

    private Color4F GetColorForScope(int colorScopeIndex, out bool isUserOverride)
    {
      GeoVisualization geoVisualization = this._GeoVisualization;
      if (geoVisualization != null)
      {
        if (this.ColorScopeDisplayStrings == null)
          return geoVisualization.LayerColor(out isUserOverride).GetValueOrDefault(GeoVisualization.DefaultColor);
        if (this.Measures == null)
        {
          if (colorScopeIndex >= 0 && colorScopeIndex < Enumerable.Count<string>((IEnumerable<string>) this.ColorScopeDisplayStrings))
            return geoVisualization.ColorForCategory(this.ColorScopeDisplayStrings[colorScopeIndex], out isUserOverride);
          isUserOverride = false;
          return GeoVisualization.DefaultColor;
        }
        else if (colorScopeIndex >= 0 && colorScopeIndex < this.Measures.Count)
        {
          Tuple<TableField, AggregationFunction> tuple = this.Measures[colorScopeIndex];
          return geoVisualization.ColorForMeasure(tuple.Item1, tuple.Item2, out isUserOverride);
        }
        else
        {
          isUserOverride = false;
          return GeoVisualization.DefaultColor;
        }
      }
      else
      {
        isUserOverride = false;
        return GeoVisualization.DefaultColor;
      }
    }

    private void ResetColorForScope(int colorScopeIndex)
    {
      GeoVisualization geoVisualization = this._GeoVisualization;
      if (geoVisualization == null)
        return;
      if (this.ColorScopeDisplayStrings == null)
        geoVisualization.LayerColorOverride = new Color4F?();
      else
        geoVisualization.ResetColorForSeries(colorScopeIndex);
      this.AnyColorsOverridden = geoVisualization.ColorsOverridden;
    }

    public void ResetAllColors()
    {
      GeoVisualization geoVisualization = this._GeoVisualization;
      if (geoVisualization == null)
        return;
      geoVisualization.ResetAllColors();
      this.AnyColorsOverridden = false;
      this.RefreshSelectedColorState();
    }

    private int GetColorScopeCount()
    {
      if (this.ColorScopeDisplayStrings == null)
        return 1;
      if (this.Measures == null)
        return this.ColorScopeDisplayStrings.Length;
      else
        return this.Measures.Count;
    }

    private void SetFieldWellVisualizationViewModel(FieldWellVisualizationViewModel value)
    {
      if (this._FieldWellVisualizationViewModel != null)
        this._FieldWellVisualizationViewModel.PropertyChanged -= new PropertyChangedEventHandler(this.FieldWellVisualizationViewModel_PropertyChanged);
      if (value != null)
        value.PropertyChanged += new PropertyChangedEventHandler(this.FieldWellVisualizationViewModel_PropertyChanged);
      this._FieldWellVisualizationViewModel = value;
    }

    public void SetParentLayer(LayerViewModel parentLayer)
    {
      if (parentLayer != null)
      {
        if (parentLayer.LayerDefinition != null)
          this.SetGeoVisualization(parentLayer.LayerDefinition.GeoVisualization);
        else
          this.SetGeoVisualization((GeoVisualization) null);
        if (parentLayer.FieldListPicker != null)
          this.SetFieldWellVisualizationViewModel(parentLayer.FieldListPicker.FieldWellVisualizationViewModel);
        else
          this.SetFieldWellVisualizationViewModel((FieldWellVisualizationViewModel) null);
      }
      else
      {
        this.SetGeoVisualization((GeoVisualization) null);
        this.SetFieldWellVisualizationViewModel((FieldWellVisualizationViewModel) null);
      }
    }

    private void SetScaleLabels(LayerType layerType)
    {
      switch (layerType)
      {
        case LayerType.BubbleChart:
        case LayerType.PieChart:
          this.DataDimensionScaleVisible = layerType != LayerType.PointMarkerChart;
          this.DataDimensionMaxScale = 5.0;
          this.DataDimensionScaleLabel = Resources.LayerSettingsSizeLabel;
          this.FixedDimensionScaleLabel = Resources.LayerSettingsThicknessLabel;
          this.FixedDimensionScaleVisible = true;
          this.LockScalesVisible = true;
          break;
        case LayerType.ColumnChart:
        case LayerType.ClusteredColumnChart:
        case LayerType.StackedColumnChart:
          this.DataDimensionScaleVisible = layerType != LayerType.PointMarkerChart;
          this.DataDimensionMaxScale = 5.0;
          this.DataDimensionScaleLabel = Resources.LayerSettingsHeightLabel;
          this.FixedDimensionScaleLabel = Resources.LayerSettingsThicknessLabel;
          this.FixedDimensionScaleVisible = true;
          this.LockScalesVisible = true;
          break;
        case LayerType.HeatMapChart:
          this.DataDimensionScaleVisible = true;
          this.DataDimensionMaxScale = 5.0;
          this.DataDimensionScaleLabel = Resources.LayerSettingsColorScaleLabel;
          this.FixedDimensionScaleLabel = Resources.LayerSettingsRadiusOfInfluenceLabel;
          this.FixedDimensionScaleVisible = true;
          this.LockScalesVisible = true;
          break;
        case LayerType.RegionChart:
          this.DataDimensionScaleVisible = true;
          this.DataDimensionMaxScale = 1.0;
          this.DataDimensionScaleLabel = Resources.LayerSettingsColorScaleLabel;
          this.FixedDimensionScaleLabel = Resources.LayerSettingsThicknessLabel;
          this.FixedDimensionScaleVisible = false;
          this.LockScalesVisible = false;
          break;
        default:
          this.DataDimensionScaleVisible = layerType != LayerType.PointMarkerChart;
          this.DataDimensionMaxScale = 5.0;
          this.DataDimensionScaleLabel = Resources.LayerSettingsScaleLabel;
          this.FixedDimensionScaleLabel = Resources.LayerSettingsSizeLabel;
          this.FixedDimensionScaleVisible = true;
          this.LockScalesVisible = true;
          break;
      }
    }

    private void FieldWellVisualizationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == ((FieldWellVisualizationViewModel) sender).PropertySelectedVisualizationType))
        return;
      this.SetScaleLabels(((FieldWellVisualizationViewModel) sender).SelectedVisualizationType);
      this.ColorPickerVisible = ((FieldWellVisualizationViewModel) sender).SelectedVisualizationType != LayerType.HeatMapChart;
    }

    internal void SetSelectedColorScopeIndex(int colorScopeIndex)
    {
      if (colorScopeIndex < 0 || colorScopeIndex >= this.GetColorScopeCount() || !this.SetProperty<int>(this.PropertySelectedColorScopeIndex, ref this._SelectedColorScopeIndex, colorScopeIndex, false))
        return;
      this.RefreshSelectedColorState();
    }
  }
}
