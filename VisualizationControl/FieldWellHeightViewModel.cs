using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class FieldWellHeightViewModel : FieldWellEntryViewModel
  {
    private List<AggregationFunction> allowedDropDownOptions = new List<AggregationFunction>();
    private AggregationFunction _AggregationFunction;

    public static string PropertyAggregationFunction
    {
      get
      {
        return "AggregationFunction";
      }
    }

    public AggregationFunction AggregationFunction
    {
      get
      {
        return this._AggregationFunction;
      }
      set
      {
        this.SetProperty<AggregationFunction>(FieldWellHeightViewModel.PropertyAggregationFunction, ref this._AggregationFunction, value, false);
      }
    }

    public bool AggregationFunctionsAreListed { get; private set; }

    public Action<FieldWellHeightViewModel> RemoveCallback { get; set; }

    public FieldWellHeightViewModel(bool listAggregationFunctions)
    {
      this.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
      this.AggregationFunctionsAreListed = listAggregationFunctions;
      if (listAggregationFunctions)
      {
        this.allowedDropDownOptions.Add(AggregationFunction.Sum);
        this.allowedDropDownOptions.Add(AggregationFunction.Average);
        this.allowedDropDownOptions.Add(AggregationFunction.Count);
        this.allowedDropDownOptions.Add(AggregationFunction.DistinctCount);
        this.allowedDropDownOptions.Add(AggregationFunction.Max);
        this.allowedDropDownOptions.Add(AggregationFunction.Min);
        this.allowedDropDownOptions.Add(AggregationFunction.None);
      }
      this.PopulateDropDownOptions();
    }

    public bool AllowsAggregationFunction(AggregationFunction func)
    {
      return this.allowedDropDownOptions.Contains(func);
    }

    public void RemoveAggregationFunction(AggregationFunction func)
    {
      this.DropDownOptions.Remove((object) func);
    }

    public void RestoreAllowedAggregationFunctions()
    {
      this.PopulateDropDownOptions();
    }

    public void SetAggregationFunctions(IEnumerable<AggregationFunction> functions)
    {
      this.allowedDropDownOptions = Enumerable.ToList<AggregationFunction>(functions);
      this.PopulateDropDownOptions();
    }

    private void PopulateDropDownOptions()
    {
      this.DropDownOptions.Clear();
      foreach (int num in this.allowedDropDownOptions)
        this.DropDownOptions.Add((object) (AggregationFunction) num);
      this.AddDropDownOptionControls(this.DropDownOptions.Count > 0);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == this.PropertyTableField) && !(e.PropertyName == FieldWellHeightViewModel.PropertyAggregationFunction))
        return;
      if (this.AggregationFunctionsAreListed)
      {
        this.DisplayString = string.Format(Resources.FieldWellHeightAggregatedNameFormat, (object) this.TableField.Name, (object) AggregationFunctionExtensions.DisplayString(this.AggregationFunction));
        this.SelectedDropDownOption = (object) this.AggregationFunction;
      }
      else
        this.DisplayString = this.TableField.Name;
    }

    protected override void OnSelectedDropDownOptionValueChanged()
    {
      if (!(this.SelectedDropDownOption is AggregationFunction))
        return;
      this.AggregationFunction = (AggregationFunction) this.SelectedDropDownOption;
    }

    protected override void OnSelectedRemoveEntryOption()
    {
      if (this.RemoveCallback == null)
        return;
      this.RemoveCallback(this);
    }
  }
}
