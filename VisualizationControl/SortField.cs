namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SortField
  {
    public TableMember Column { get; set; }

    public string CategoryValue { get; set; }

    public AggregationFunction Function { get; set; }

    public string Name
    {
      get
      {
        if (this.Column == null)
          return (string) null;
        if (this.CategoryValue != null)
          return this.CategoryValue;
        if (this.Function == AggregationFunction.UserDefined)
          return this.Column.Name;
        else
          return string.Format(Resources.FieldWellHeightAggregatedNameFormat, (object) this.Column.Name, (object) AggregationFunctionExtensions.DisplayString(this.Function));
      }
    }

    public override bool Equals(object obj)
    {
      SortField sortField = obj as SortField;
      if (sortField == null)
        return false;
      if (object.ReferenceEquals((object) this, (object) sortField))
        return true;
      if (this.Column == null || sortField.Column == null || !this.Column.RefersToTheSameMemberAs(sortField.Column))
        return false;
      if (this.CategoryValue != null && sortField.CategoryValue != null && this.CategoryValue.Equals(sortField.CategoryValue))
        return true;
      if (this.CategoryValue == null && sortField.CategoryValue == null)
        return this.Function == sortField.Function;
      else
        return false;
    }
  }
}
