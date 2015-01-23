using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GeoFieldMappingViewModel : ViewModelBase
  {
    private RemoveItemPlaceholder removeOption = new RemoveItemPlaceholder(Resources.FieldWellEntry_RemoveOption);
    public Func<bool> ChartTypeIsRegion;
    private TableFieldViewModel _Field;
    private GeoFieldMappingType _MappingType;
    private string _DisplayString;
    private string _MapByDisplayString;
    private bool _isMapByField;
    private object _SelectedOption;

    public bool UserSelectedMapByField { get; set; }

    public static string PropertyField
    {
      get
      {
        return "Field";
      }
    }

    public TableFieldViewModel Field
    {
      get
      {
        return this._Field;
      }
      private set
      {
        if (!this.SetProperty<TableFieldViewModel>(GeoFieldMappingViewModel.PropertyField, ref this._Field, value, false))
          return;
        this.RefreshDisplayString();
      }
    }

    public List<GeoFieldMappingType> MappingTypes { get; private set; }

    public static string PropertyMappingType
    {
      get
      {
        return "MappingType";
      }
    }

    public GeoFieldMappingType MappingType
    {
      get
      {
        return this._MappingType;
      }
      set
      {
        base.SetProperty<GeoFieldMappingType>(GeoFieldMappingViewModel.PropertyMappingType, ref this._MappingType, value, new Action(this.OnMappingTypeChanged));
      }
    }

    public string PropertyDisplayString
    {
      get
      {
        return "DisplayString";
      }
    }

    public string DisplayString
    {
      get
      {
        return this._DisplayString;
      }
      set
      {
        this.SetProperty<string>(this.PropertyDisplayString, ref this._DisplayString, value, false);
      }
    }

    public string PropertyMapByDisplayString
    {
      get
      {
        return "MapByDisplayString";
      }
    }

    public string MapByDisplayString
    {
      get
      {
        return this._MapByDisplayString;
      }
      set
      {
        this.SetProperty<string>(this.PropertyMapByDisplayString, ref this._MapByDisplayString, value, false);
      }
    }

    public static string PropertyIsMapByField
    {
      get
      {
        return "IsMapByField";
      }
    }

    public bool IsMapByField
    {
      get
      {
        return this._isMapByField;
      }
      set
      {
        if (this._isMapByField != value)
          this.UserSelectedMapByField = value;
        this.SetProperty<bool>(GeoFieldMappingViewModel.PropertyIsMapByField, ref this._isMapByField, value, false);
      }
    }

    public ObservableCollectionEx<object> DropDownOptions { get; private set; }

    public string PropertySelectedOption
    {
      get
      {
        return "SelectedOption";
      }
    }

    public object SelectedOption
    {
      get
      {
        return this._SelectedOption;
      }
      set
      {
        base.SetProperty<object>(this.PropertySelectedOption, ref this._SelectedOption, value, new Action(this.OnSelectedOptionChanged));
      }
    }

    public event Action<GeoFieldMappingViewModel> RemoveOptionSelected;

    public GeoFieldMappingViewModel(TableFieldViewModel field, bool detectMappingType = true)
    {
      this.Field = field;
      this.MappingTypes = new List<GeoFieldMappingType>()
      {
        GeoFieldMappingType.None,
        GeoFieldMappingType.Latitude,
        GeoFieldMappingType.Longitude,
        GeoFieldMappingType.XCoord,
        GeoFieldMappingType.YCoord,
        GeoFieldMappingType.City,
        GeoFieldMappingType.Country,
        GeoFieldMappingType.County,
        GeoFieldMappingType.State,
        GeoFieldMappingType.Street,
        GeoFieldMappingType.Zip,
        GeoFieldMappingType.Address,
        GeoFieldMappingType.Other
      };
      this.DropDownOptions = new ObservableCollectionEx<object>();
      foreach (int num in this.MappingTypes)
        this.DropDownOptions.Add((object) (GeoFieldMappingType) num);
      this.DropDownOptions.Add(ListUtilities.Separator);
      this.DropDownOptions.Add((object) this.removeOption);
      this.SelectedOption = (object) GeoFieldMappingType.None;
      if (!detectMappingType)
        return;
      this.DetectAndSetMappingType();
    }

    public GeoFieldMappingViewModel(TableFieldViewModel field, GeoFieldMappingType mappingType, bool isMapByField, bool userSelectedMapByField)
      : this(field, false)
    {
      this.MappingType = mappingType;
      this.IsMapByField = isMapByField;
      this.UserSelectedMapByField = userSelectedMapByField;
    }

    private void DetectAndSetMappingType()
    {
      if (this.Field == null || this.Field.ColumnClassification == GeoFieldMappingType.None)
        return;
      this.MappingType = this.Field.ColumnClassification;
    }

    private void OnMappingTypeChanged()
    {
      this.RefreshDisplayString();
      if (this.SelectedOption == null || (GeoFieldMappingType) this.SelectedOption == this.MappingType)
        return;
      this.SelectedOption = (object) this.MappingType;
    }

    private void RefreshDisplayString()
    {
      DisplayStringAttribute displayStringAttribute = (DisplayStringAttribute) typeof (GeoFieldMappingType).GetMember(((object) this.MappingType).ToString())[0].GetCustomAttributes(typeof (DisplayStringAttribute), false)[0];
      this.DisplayString = this.Field.Name;
      this.MapByDisplayString = string.Format(Resources.GeoFieldMappingViewModel_DisplayStringFormat, (object) this.Field.Name, (object) displayStringAttribute.Value);
    }

    private void OnSelectedOptionChanged()
    {
      if (this.SelectedOption == this.removeOption)
      {
        if (this.RemoveOptionSelected == null)
          return;
        this.RemoveOptionSelected(this);
      }
      else
      {
        if (!(this.SelectedOption is GeoFieldMappingType) || this.SelectedOption == null || (GeoFieldMappingType) this.SelectedOption == this.MappingType)
          return;
        this.MappingType = (GeoFieldMappingType) this.SelectedOption;
      }
    }
  }
}
