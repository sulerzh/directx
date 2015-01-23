using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ExpressionFilterViewModel : FilterViewModelBase
  {
    private bool setValue = true;
    private bool isAndConnector = true;
    private static readonly List<object> DateTimePredicates = new List<object>();
    private static readonly List<object> NumericPredicates = new List<object>();
    private static readonly List<object> StringPredicates = new List<object>();
    public static string PropertyFirstPredicateComparison = "FirstPredicateComparison";
    public static string PropertySecondPredicateComparison = "SecondPredicateComparison";
    public static string PropertyFirstPredicateValue = "FirstPredicateValue";
    public static string PropertyFirstPredicateValueEnabled = "FirstPredicateValueEnabled";
    public static string PropertySecondPredicateValue = "SecondPredicateValue";
    public static string PropertySecondPredicateValueEnabled = "SecondPredicateValueEnabled";
    public static string PropertyIsAndConnector = "IsAndConnector";
    private const int defaultPredicateIndex = 0;
    private const int IsBlankIndex = 7;
    private const int IsNotBlankIndex = 8;
    private AndOrFilterClause andOrFilterClause;
    private object firstPredicateComparison;
    private object secondPredicateComparison;
    private object firstPredicateValue;
    private bool firstPredicateValueEnabled;
    private object secondPredicateValue;
    private object secondPredicateValueEnabled;

    public List<object> Predicates { get; private set; }

    public object FirstPredicateComparison
    {
      get
      {
        return this.firstPredicateComparison;
      }
      set
      {
        if (!this.SetProperty<object>(ExpressionFilterViewModel.PropertyFirstPredicateComparison, ref this.firstPredicateComparison, value, false))
          return;
        if (value == this.Predicates[0] || value == this.Predicates[7] || value == this.Predicates[8])
        {
          this.setValue = false;
          if (this.FirstPredicateDateTime != null)
            this.FirstPredicateDateTime.ComposedDate = this.FirstPredicateDateTime.MinimumCalendarDate;
          this.FirstPredicateValue = (object) null;
          this.FirstPredicateValueEnabled = false;
          this.setValue = true;
        }
        else
        {
          this.FirstPredicateValueEnabled = true;
          if (this.FirstPredicateDateTime == null)
            return;
          this.FirstPredicateValue = (object) this.FirstPredicateDateTime.ComposedDate;
        }
      }
    }

    public object SecondPredicateComparison
    {
      get
      {
        return this.secondPredicateComparison;
      }
      set
      {
        if (!this.SetProperty<object>(ExpressionFilterViewModel.PropertySecondPredicateComparison, ref this.secondPredicateComparison, value, false))
          return;
        if (value == this.Predicates[0] || value == this.Predicates[7] || value == this.Predicates[8])
        {
          this.setValue = false;
          if (this.SecondPredicateDateTime != null)
            this.SecondPredicateDateTime.ComposedDate = this.SecondPredicateDateTime.MinimumCalendarDate;
          this.SecondPredicateValue = (object) null;
          this.SecondPredicateValueEnabled = (object) false;
          this.setValue = true;
        }
        else
        {
          this.SecondPredicateValueEnabled = (object) true;
          if (this.SecondPredicateDateTime == null)
            return;
          this.SecondPredicateValue = (object) this.SecondPredicateDateTime.ComposedDate;
        }
      }
    }

    public object FirstPredicateValue
    {
      get
      {
        return this.firstPredicateValue;
      }
      set
      {
        this.SetProperty<object>(ExpressionFilterViewModel.PropertyFirstPredicateValue, ref this.firstPredicateValue, value, false);
      }
    }

    public bool FirstPredicateValueEnabled
    {
      get
      {
        return this.firstPredicateValueEnabled;
      }
      set
      {
        this.SetProperty<bool>(ExpressionFilterViewModel.PropertyFirstPredicateValueEnabled, ref this.firstPredicateValueEnabled, value, false);
      }
    }

    public object SecondPredicateValue
    {
      get
      {
        return this.secondPredicateValue;
      }
      set
      {
        this.SetProperty<object>(ExpressionFilterViewModel.PropertySecondPredicateValue, ref this.secondPredicateValue, value, false);
      }
    }

    public object SecondPredicateValueEnabled
    {
      get
      {
        return this.secondPredicateValueEnabled;
      }
      set
      {
        this.SetProperty<object>(ExpressionFilterViewModel.PropertySecondPredicateValueEnabled, ref this.secondPredicateValueEnabled, value, false);
      }
    }

    public bool IsAndConnector
    {
      get
      {
        return this.isAndConnector;
      }
      set
      {
        this.SetProperty<bool>(ExpressionFilterViewModel.PropertyIsAndConnector, ref this.isAndConnector, value, false);
      }
    }

    public DateTimeEditorViewModel FirstPredicateDateTime { get; private set; }

    public DateTimeEditorViewModel SecondPredicateDateTime { get; private set; }

    public override bool IsDefault
    {
      get
      {
        return this.andOrFilterClause.Unfiltered;
      }
    }

    static ExpressionFilterViewModel()
    {
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.Unknown);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.Is);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.IsNot);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.IsAfter);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.IsOnOrAfter);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.IsBefore);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.IsOnOrBefore);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.IsBlank);
      ExpressionFilterViewModel.DateTimePredicates.Add((object) DateTimeFilterPredicateComparison.IsNotBlank);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.Unknown);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.IsLessThan);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.IsLessThanOrEqualTo);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.IsGreaterThan);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.IsGreaterThanOrEqualTo);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.Is);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.IsNot);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.IsBlank);
      ExpressionFilterViewModel.NumericPredicates.Add((object) NumericFilterPredicateComparison.IsNotBlank);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.Unknown);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.Contains);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.DoesNotContain);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.StartsWith);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.DoesNotStartWith);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.Is);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.IsNot);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.IsBlank);
      ExpressionFilterViewModel.StringPredicates.Add((object) StringFilterPredicateComparison.IsNotBlank);
    }

    public ExpressionFilterViewModel(FieldListPickerViewModel fieldListPickerVm, AndOrFilterClause andOrFilterClause)
    {
      this.fieldListPickerViewModel = fieldListPickerVm;
      this.andOrFilterClause = andOrFilterClause;
      if (andOrFilterClause.First.GetType() == typeof (DateTimeFilterPredicate))
      {
        this.Predicates = ExpressionFilterViewModel.DateTimePredicates;
        DateTimeFilterPredicate first = ExpressionFilterViewModel.Convert<DateTimeFilterPredicate>(andOrFilterClause.First);
        DateTimeFilterPredicate second = ExpressionFilterViewModel.Convert<DateTimeFilterPredicate>(andOrFilterClause.Second);
        if (first != null)
        {
          this.FirstPredicateComparison = this.Predicates.Find((Predicate<object>) (pc => (DateTimeFilterPredicateComparison) pc == first.Operator));
          this.FirstPredicateValue = (object) (first.Value.HasValue ? new DateTime?(first.Value.Value) : new DateTime?());
          this.FirstPredicateDateTime = new DateTimeEditorViewModel()
          {
            ComposedDate = first.Value.HasValue ? first.Value.Value : DateTime.MinValue
          };
          this.FirstPredicateDateTime.PropertyChanged += new PropertyChangedEventHandler(this.DateTimeOnPropertyChanged);
        }
        if (second != null)
        {
          this.SecondPredicateComparison = this.Predicates.Find((Predicate<object>) (pc => (DateTimeFilterPredicateComparison) pc == second.Operator));
          this.SecondPredicateValue = (object) (second.Value.HasValue ? new DateTime?(second.Value.Value) : new DateTime?());
          this.SecondPredicateDateTime = new DateTimeEditorViewModel()
          {
            ComposedDate = second.Value.HasValue ? second.Value.Value : DateTime.MinValue
          };
          this.SecondPredicateDateTime.PropertyChanged += new PropertyChangedEventHandler(this.DateTimeOnPropertyChanged);
        }
      }
      else if (andOrFilterClause.First.GetType() == typeof (NumericFilterPredicate))
      {
        this.Predicates = ExpressionFilterViewModel.NumericPredicates;
        NumericFilterPredicate first = ExpressionFilterViewModel.Convert<NumericFilterPredicate>(andOrFilterClause.First);
        NumericFilterPredicate second = ExpressionFilterViewModel.Convert<NumericFilterPredicate>(andOrFilterClause.Second);
        if (first != null)
        {
          this.FirstPredicateComparison = this.Predicates.Find((Predicate<object>) (pc => (NumericFilterPredicateComparison) pc == first.Operator));
          this.FirstPredicateValue = first.Value.HasValue ? (object) first.Value.Value.ToString((IFormatProvider) Resources.Culture) : (object) (string) null;
        }
        if (second != null)
        {
          this.SecondPredicateComparison = this.Predicates.Find((Predicate<object>) (pc => (NumericFilterPredicateComparison) pc == second.Operator));
          this.SecondPredicateValue = second.Value.HasValue ? (object) second.Value.Value.ToString((IFormatProvider) Resources.Culture) : (object) (string) null;
        }
      }
      else if (andOrFilterClause.First.GetType() == typeof (StringFilterPredicate))
      {
        this.Predicates = ExpressionFilterViewModel.StringPredicates;
        StringFilterPredicate first = ExpressionFilterViewModel.Convert<StringFilterPredicate>(andOrFilterClause.First);
        StringFilterPredicate second = ExpressionFilterViewModel.Convert<StringFilterPredicate>(andOrFilterClause.Second);
        if (first != null)
        {
          this.FirstPredicateComparison = this.Predicates.Find((Predicate<object>) (pc => (StringFilterPredicateComparison) pc == first.Operator));
          this.FirstPredicateValue = (object) first.Value;
        }
        if (second != null)
        {
          this.SecondPredicateComparison = this.Predicates.Find((Predicate<object>) (pc => (StringFilterPredicateComparison) pc == second.Operator));
          this.SecondPredicateValue = (object) second.Value;
        }
      }
      this.IsAndConnector = andOrFilterClause.Operator == FilterPredicateOperator.And;
      if (this.andOrFilterClause.HasUpdatableProperties)
      {
        this.andOrFilterClause.OnPropertiesUpdated += new EventHandler(this.AndOrFilterClauseOnPropertiesUpdated);
        this.AndOrFilterClauseOnPropertiesUpdated((object) this.andOrFilterClause, EventArgs.Empty);
      }
      else
      {
        this.UpdateDescriptionToAllOrFiltered();
        this.RaisePropertyChanged(FilterViewModelBase.PropertyIsDefault);
        this.IsEnabled = true;
      }
    }

    public static T GetDefaultPredicate<T>()
    {
      if (typeof (T) == typeof (DateTimeFilterPredicateComparison))
        return (T) ExpressionFilterViewModel.DateTimePredicates[0];
      if (typeof (T) == typeof (StringFilterPredicateComparison))
        return (T) ExpressionFilterViewModel.StringPredicates[0];
      if (typeof (T) == typeof (NumericFilterPredicateComparison))
        return (T) ExpressionFilterViewModel.NumericPredicates[0];
      else
        return default (T);
    }

    private void AndOrFilterClauseOnPropertiesUpdated(object sender, EventArgs eventArgs)
    {
      if (!object.ReferenceEquals(sender, (object) this.andOrFilterClause))
        return;
      this.UIDispatcher.Invoke((Action) (() =>
      {
        FilterPredicateProperties properties;
        if (!this.andOrFilterClause.TryGetProperties(out properties))
          return;
        this.setValue = false;
        DateTimeFilterPredicateProperties predicateProperties = properties as DateTimeFilterPredicateProperties;
        if (predicateProperties != null && predicateProperties.AllowedMin.HasValue)
        {
          if (this.FirstPredicateValue == null)
            this.FirstPredicateDateTime.ComposedDate = this.FirstPredicateDateTime.MinimumCalendarDate = predicateProperties.AllowedMin.Value <= DateTime.MinValue.AddYears(1) ? DateTime.MinValue : new DateTime(predicateProperties.AllowedMin.Value.Subtract(new TimeSpan(365, 0, 0, 0)).Year, 1, 1);
          if (this.SecondPredicateValue == null)
            this.SecondPredicateDateTime.ComposedDate = this.SecondPredicateDateTime.MinimumCalendarDate = predicateProperties.AllowedMin.Value <= DateTime.MinValue.AddYears(1) ? DateTime.MinValue : new DateTime(predicateProperties.AllowedMin.Value.Subtract(new TimeSpan(365, 0, 0, 0)).Year, 1, 1);
        }
        this.IsEnabled = true;
        this.setValue = true;
        this.UpdateDescriptionToAllOrFiltered();
        this.RaisePropertyChanged(FilterViewModelBase.PropertyIsDefault);
      }));
    }

    private void DateTimeOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      DateTimeEditorViewModel timeEditorViewModel = sender as DateTimeEditorViewModel;
      if (!this.setValue || timeEditorViewModel == null || !propertyChangedEventArgs.PropertyName.Equals(timeEditorViewModel.PropertyComposedDate))
        return;
      if (timeEditorViewModel == this.FirstPredicateDateTime)
        this.FirstPredicateValue = (object) timeEditorViewModel.ComposedDate;
      else
        this.SecondPredicateValue = (object) timeEditorViewModel.ComposedDate;
    }

    private static T Convert<T>(FilterPredicate predicate) where T : FilterPredicate
    {
      if (predicate == null)
        return default (T);
      else
        return predicate as T;
    }

    public override void Delete()
    {
      if (this.andOrFilterClause.HasUpdatableProperties)
        this.andOrFilterClause.OnPropertiesUpdated -= new EventHandler(this.AndOrFilterClauseOnPropertiesUpdated);
      this.fieldListPickerViewModel.RemoveFilterClause((FilterClause) this.andOrFilterClause);
    }

    protected override void Clear()
    {
      this.FirstPredicateComparison = this.SecondPredicateComparison = this.Predicates[0];
      this.FirstPredicateValue = this.SecondPredicateValue = (object) null;
      this.IsAndConnector = true;
      if (this.FirstPredicateDateTime != null)
        this.FirstPredicateDateTime.ComposedDate = this.FirstPredicateDateTime.MinimumCalendarDate;
      if (this.SecondPredicateDateTime != null)
        this.SecondPredicateDateTime.ComposedDate = this.SecondPredicateDateTime.MinimumCalendarDate;
      this.ApplyFilter();
      if (this.andOrFilterClause.HasUpdatableProperties)
      {
        this.AndOrFilterClauseOnPropertiesUpdated((object) this.andOrFilterClause, EventArgs.Empty);
      }
      else
      {
        this.UpdateDescriptionToAllOrFiltered();
        this.RaisePropertyChanged(FilterViewModelBase.PropertyIsDefault);
      }
    }

    protected override bool CanApplyFilter()
    {
      if (!base.CanApplyFilter())
        return false;
      if ((this.FirstPredicateValue == null || string.IsNullOrEmpty(this.FirstPredicateValue.ToString()) || this.FirstPredicateComparison == this.Predicates[0]) && (this.SecondPredicateValue == null || string.IsNullOrEmpty(this.SecondPredicateValue.ToString()) || this.SecondPredicateComparison == this.Predicates[0]) && (this.FirstPredicateComparison != this.Predicates[7] && this.FirstPredicateComparison != this.Predicates[8] && this.SecondPredicateComparison != this.Predicates[7]))
        return this.secondPredicateComparison == this.Predicates[8];
      else
        return true;
    }

    internal override FilterViewModelBase GetNextCompatibleFilterForFunction(AggregationFunction previous)
    {
      if (previous == this.Function)
        return (FilterViewModelBase) this;
      FilterClause filterClause = (FilterClause) null;
      if (this.Function != AggregationFunction.None)
      {
        filterClause = (FilterClause) new NumericRangeFilterClause(this.andOrFilterClause.TableMember, this.Function, new double?(), new double?());
      }
      else
      {
        switch (this.DataType)
        {
          case TableMemberDataType.Unknown:
            break;
          case TableMemberDataType.String:
            filterClause = (FilterClause) new CategoryFilterClause<string>(this.andOrFilterClause.TableMember, this.Culture, (IEnumerable<string>) null, false, false);
            break;
          case TableMemberDataType.Bool:
            filterClause = (FilterClause) new CategoryFilterClause<bool>(this.andOrFilterClause.TableMember, this.Culture, (IEnumerable<bool>) null, false, false);
            break;
          case TableMemberDataType.Double:
          case TableMemberDataType.Long:
          case TableMemberDataType.Currency:
            filterClause = (FilterClause) new NumericRangeFilterClause(this.andOrFilterClause.TableMember, this.Function, new double?(), new double?());
            break;
          case TableMemberDataType.DateTime:
            filterClause = (FilterClause) new AndOrFilterClause(this.andOrFilterClause.TableMember, this.Function, FilterPredicateOperator.And, (FilterPredicate) new DateTimeFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<DateTimeFilterPredicateComparison>(), new DateTime?()), (FilterPredicate) new DateTimeFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<DateTimeFilterPredicateComparison>(), new DateTime?()));
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      if (filterClause == null)
        return (FilterViewModelBase) null;
      this.andOrFilterClause.OnPropertiesUpdated -= new EventHandler(this.AndOrFilterClauseOnPropertiesUpdated);
      this.fieldListPickerViewModel.ReplaceFilterClause((FilterClause) this.andOrFilterClause, filterClause);
      return FilterViewModelBase.Get(this.fieldListPickerViewModel, filterClause, false);
    }

    internal override FilterViewModelBase GetNextCompatibleFilter()
    {
      FilterClause filterClause = (FilterClause) null;
      if (this.Function == AggregationFunction.None)
      {
        switch (this.DataType)
        {
          case TableMemberDataType.Unknown:
          case TableMemberDataType.Bool:
            break;
          case TableMemberDataType.String:
            filterClause = (FilterClause) new CategoryFilterClause<string>(this.andOrFilterClause.TableMember, this.Culture, (IEnumerable<string>) null, false, false);
            break;
          case TableMemberDataType.Double:
          case TableMemberDataType.Long:
          case TableMemberDataType.Currency:
            filterClause = (FilterClause) new NumericRangeFilterClause(this.andOrFilterClause.TableMember, this.Function, new double?(), new double?());
            break;
          case TableMemberDataType.DateTime:
            filterClause = (FilterClause) new CategoryFilterClause<DateTime>(this.andOrFilterClause.TableMember, this.Culture, (IEnumerable<DateTime>) null, false, false);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      else
        filterClause = (FilterClause) new NumericRangeFilterClause(this.andOrFilterClause.TableMember, this.Function, new double?(), new double?());
      if (filterClause == null)
        return (FilterViewModelBase) null;
      if (this.andOrFilterClause.HasUpdatableProperties)
        this.andOrFilterClause.OnPropertiesUpdated -= new EventHandler(this.AndOrFilterClauseOnPropertiesUpdated);
      this.fieldListPickerViewModel.ReplaceFilterClause((FilterClause) this.andOrFilterClause, filterClause);
      return FilterViewModelBase.Get(this.fieldListPickerViewModel, filterClause, false);
    }

    protected override void ApplyFilter()
    {
      FilterPredicate predicate1;
      FilterPredicate predicate2;
      if (!this.TryConstruct(true, out predicate1) || !this.TryConstruct(false, out predicate2))
        return;
      AndOrFilterClause oldFilterClause = this.andOrFilterClause;
      if (oldFilterClause.HasUpdatableProperties)
        oldFilterClause.OnPropertiesUpdated -= new EventHandler(this.AndOrFilterClauseOnPropertiesUpdated);
      this.andOrFilterClause = new AndOrFilterClause(oldFilterClause, this.IsAndConnector ? FilterPredicateOperator.And : FilterPredicateOperator.Or, predicate1, predicate2);
      if (this.andOrFilterClause.HasUpdatableProperties)
        this.andOrFilterClause.OnPropertiesUpdated += new EventHandler(this.AndOrFilterClauseOnPropertiesUpdated);
      this.fieldListPickerViewModel.ReplaceFilterClause((FilterClause) oldFilterClause, (FilterClause) this.andOrFilterClause);
      this.UpdateDescriptionToAllOrFiltered();
      this.RaisePropertyChanged(FilterViewModelBase.PropertyIsDefault);
    }

    private bool TryConstruct(bool isFirst, out FilterPredicate predicate)
    {
      predicate = (FilterPredicate) null;
      bool flag = isFirst && this.FirstPredicateValue == null || !isFirst && this.SecondPredicateValue == null;
      if (this.Predicates == ExpressionFilterViewModel.DateTimePredicates)
      {
        DateTimeFilterPredicateComparison op = isFirst ? (DateTimeFilterPredicateComparison) this.FirstPredicateComparison : (DateTimeFilterPredicateComparison) this.SecondPredicateComparison;
        predicate = (FilterPredicate) new DateTimeFilterPredicate(op, flag ? new DateTime?() : new DateTime?(isFirst ? (DateTime) this.FirstPredicateValue : (DateTime) this.SecondPredicateValue));
        return true;
      }
      else if (this.Predicates == ExpressionFilterViewModel.StringPredicates)
      {
        StringFilterPredicateComparison op = isFirst ? (StringFilterPredicateComparison) this.FirstPredicateComparison : (StringFilterPredicateComparison) this.SecondPredicateComparison;
        string str = flag ? (string) null : (isFirst ? this.FirstPredicateValue.ToString() : this.SecondPredicateValue.ToString());
        predicate = (FilterPredicate) new StringFilterPredicate(op, str);
        return true;
      }
      else
      {
        NumericFilterPredicateComparison op = isFirst ? (NumericFilterPredicateComparison) this.FirstPredicateComparison : (NumericFilterPredicateComparison) this.SecondPredicateComparison;
        double result = 0.0;
        if (!flag && !double.TryParse(isFirst ? this.FirstPredicateValue.ToString() : this.SecondPredicateValue.ToString(), out result))
          return false;
        predicate = (FilterPredicate) new NumericFilterPredicate(op, flag ? new double?() : new double?(result));
        return true;
      }
    }
  }
}
