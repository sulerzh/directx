using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class FieldColumnSelectionViewModel : ViewModelBase
  {
    private Tuple<AggregationFunction?, string, object> _Model;
    private bool _IsSelected;
    private string _DisplayName;

    public static string PropertyModel
    {
      get
      {
        return "Model";
      }
    }

    public Tuple<AggregationFunction?, string, object> Model
    {
      get
      {
        return this._Model;
      }
      set
      {
        this.SetProperty<Tuple<AggregationFunction?, string, object>>(FieldColumnSelectionViewModel.PropertyModel, ref this._Model, value, false);
      }
    }

    public static string PropertyIsSelected
    {
      get
      {
        return "IsSelected";
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._IsSelected;
      }
      set
      {
        this.SetProperty<bool>(FieldColumnSelectionViewModel.PropertyIsSelected, ref this._IsSelected, value, false);
      }
    }

    public static string PropertyDisplayName
    {
      get
      {
        return "DisplayName";
      }
    }

    public string DisplayName
    {
      get
      {
        return this._DisplayName;
      }
      set
      {
        this.SetProperty<string>(FieldColumnSelectionViewModel.PropertyDisplayName, ref this._DisplayName, value, false);
      }
    }

    public string NameToPersist { get; set; }

    public override string ToString()
    {
      if (this.Model == null)
        return (string) null;
      else
        return this.Model.Item2;
    }
  }
}
