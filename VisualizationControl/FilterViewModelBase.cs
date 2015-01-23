using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class FilterViewModelBase : ViewModelBase
  {
    private static readonly AggregationFunction[] DefaultFunctions = new AggregationFunction[3]
    {
      AggregationFunction.None,
      AggregationFunction.Count,
      AggregationFunction.DistinctCount
    };
    private static readonly AggregationFunction[] UserDefinedFunctions = new AggregationFunction[1]
    {
      AggregationFunction.UserDefined
    };
    private static readonly AggregationFunction[] NumericFunctions = new AggregationFunction[7]
    {
      AggregationFunction.None,
      AggregationFunction.Sum,
      AggregationFunction.Average,
      AggregationFunction.Count,
      AggregationFunction.DistinctCount,
      AggregationFunction.Max,
      AggregationFunction.Min
    };
    public static string PropertyName = "Name";
    public static string PropertyIsExpanded = "IsExpanded";
    public static string PropertyDescription = "Description";
    public static string PropertyModeTooltip = "ModeTooltip";
    public static string PropertyFunction = "Function";
    public static string PropertyFunctionTooltip = "FunctionTooltip";
    public static string PropertyIsEnabled = "IsEnabled";
    public static string PropertyIsDefault = "IsDefault";
    private string description = Resources.FiltersTab_StatusProcessing;
    protected FieldListPickerViewModel fieldListPickerViewModel;
    protected Dispatcher UIDispatcher;
    public FilterViewModelBase.ChangeAggregationFunction ChangeAggregationFunctionCallback;
    private string name;
    private bool isExpanded;
    private string modeTooltip;
    private AggregationFunction function;
    private string functionTooltip;
    private bool isEnabled;

    public ICommand DeleteCommand { get; set; }

    public ICommand ClearCommand { get; set; }

    public ICommand ModeCommand { get; set; }

    public ICommand ApplyCommand { get; set; }

    public string Name
    {
      get
      {
        return this.name;
      }
      set
      {
        this.SetProperty<string>(FilterViewModelBase.PropertyName, ref this.name, value, false);
      }
    }

    public bool IsExpanded
    {
      get
      {
        return this.isExpanded;
      }
      set
      {
        this.SetProperty<bool>(FilterViewModelBase.PropertyIsExpanded, ref this.isExpanded, value, false);
      }
    }

    public string Description
    {
      get
      {
        return this.description;
      }
      set
      {
        this.SetProperty<string>(FilterViewModelBase.PropertyDescription, ref this.description, value, false);
      }
    }

    public string ModeTooltip
    {
      get
      {
        return this.modeTooltip;
      }
      set
      {
        this.SetProperty<string>(FilterViewModelBase.PropertyModeTooltip, ref this.modeTooltip, value, false);
      }
    }

    public AggregationFunction Function
    {
      get
      {
        return this.function;
      }
      set
      {
        AggregationFunction previous = this.function;
        if (!this.SetProperty<AggregationFunction>(FilterViewModelBase.PropertyFunction, ref this.function, value, false) || this.ChangeAggregationFunctionCallback == null)
          return;
        this.ChangeAggregationFunctionCallback(this, previous, this.Function);
      }
    }

    public string FunctionTooltip
    {
      get
      {
        return this.functionTooltip;
      }
      set
      {
        this.SetProperty<string>(FilterViewModelBase.PropertyFunctionTooltip, ref this.functionTooltip, value, false);
      }
    }

    public TableMemberDataType DataType { get; set; }

    public CultureInfo Culture { get; set; }

    public AggregationFunction[] Functions { get; private set; }

    public bool IsEnabled
    {
      get
      {
        return this.isEnabled;
      }
      set
      {
        this.SetProperty<bool>(FilterViewModelBase.PropertyIsEnabled, ref this.isEnabled, value, false);
      }
    }

    public abstract bool IsDefault { get; }

    public virtual bool IsModeChangeSupported
    {
      get
      {
        return true;
      }
    }

    protected FilterViewModelBase()
    {
      this.UIDispatcher = Dispatcher.CurrentDispatcher;
      this.ApplyCommand = (ICommand) new DelegatedCommand(new Action(this.ApplyFilter), new Predicate(this.CanApplyFilter));
      this.ClearCommand = (ICommand) new DelegatedCommand(new Action(this.ClearFilter));
    }

    internal static FilterViewModelBase Get(FieldListPickerViewModel fieldListPickerVm, FilterClause clause, bool add = false)
    {
      FilterViewModelBase filterViewModelBase = (FilterViewModelBase) null;
      NumericRangeFilterClause filterClause = clause as NumericRangeFilterClause;
      if (filterClause != null)
      {
        RangeFilterViewModel rangeFilterViewModel = new RangeFilterViewModel(fieldListPickerVm, filterClause);
        rangeFilterViewModel.ModeTooltip = clause.AggregationFunction == AggregationFunction.None ? Resources.FiltersTab_ListFilterModeTooltip : Resources.FiltersTab_AdvancedFilterModeTooltip;
        filterViewModelBase = (FilterViewModelBase) rangeFilterViewModel;
      }
      else
      {
        AndOrFilterClause andOrFilterClause = clause as AndOrFilterClause;
        if (andOrFilterClause != null)
        {
          ExpressionFilterViewModel expressionFilterViewModel = new ExpressionFilterViewModel(fieldListPickerVm, andOrFilterClause);
          expressionFilterViewModel.ModeTooltip = clause.AggregationFunction != AggregationFunction.None || clause.TableMember.DataType != TableMemberDataType.String && clause.TableMember.DataType != TableMemberDataType.DateTime ? Resources.FiltersTab_RangeFilterModeTooltip : Resources.FiltersTab_ListFilterModeTooltip;
          filterViewModelBase = (FilterViewModelBase) expressionFilterViewModel;
        }
        else
        {
          CategoryFilterClause<string> clause1 = clause as CategoryFilterClause<string>;
          if (clause1 != null)
          {
            ListFilterViewModel<string> listFilterViewModel = new ListFilterViewModel<string>(fieldListPickerVm, clause1);
            listFilterViewModel.ModeTooltip = Resources.FiltersTab_AdvancedFilterModeTooltip;
            filterViewModelBase = (FilterViewModelBase) listFilterViewModel;
          }
          else
          {
            CategoryFilterClause<bool> clause2 = clause as CategoryFilterClause<bool>;
            if (clause2 != null)
            {
              ListFilterViewModel<bool> listFilterViewModel = new ListFilterViewModel<bool>(fieldListPickerVm, clause2);
              listFilterViewModel.ModeTooltip = Resources.FiltersTab_ListFilterModeTooltip;
              filterViewModelBase = (FilterViewModelBase) listFilterViewModel;
            }
            else
            {
              CategoryFilterClause<DateTime> clause3 = clause as CategoryFilterClause<DateTime>;
              if (clause3 != null)
              {
                ListFilterViewModel<DateTime> listFilterViewModel = new ListFilterViewModel<DateTime>(fieldListPickerVm, clause3);
                listFilterViewModel.ModeTooltip = Resources.FiltersTab_AdvancedFilterModeTooltip;
                filterViewModelBase = (FilterViewModelBase) listFilterViewModel;
              }
              else
              {
                CategoryFilterClause<double> clause4 = clause as CategoryFilterClause<double>;
                if (clause4 != null)
                {
                  ListFilterViewModel<double> listFilterViewModel = new ListFilterViewModel<double>(fieldListPickerVm, clause4);
                  listFilterViewModel.ModeTooltip = Resources.FiltersTab_AdvancedFilterModeTooltip;
                  filterViewModelBase = (FilterViewModelBase) listFilterViewModel;
                }
              }
            }
          }
        }
      }
      if (filterViewModelBase != null)
      {
        filterViewModelBase.DataType = clause.TableMember.DataType;
        filterViewModelBase.Function = clause.AggregationFunction;
        filterViewModelBase.Functions = FilterViewModelBase.GetAggregationFunctions(clause.TableMember);
        filterViewModelBase.Name = FilterViewModelBase.GetName(clause.TableMember.Name, clause.AggregationFunction);
        if (add)
          fieldListPickerVm.AddFilterClause(clause);
      }
      return filterViewModelBase;
    }

    public abstract void Delete();

    internal abstract FilterViewModelBase GetNextCompatibleFilter();

    internal abstract FilterViewModelBase GetNextCompatibleFilterForFunction(AggregationFunction previous);

    protected virtual void ApplyFilter()
    {
    }

    protected virtual bool CanApplyFilter()
    {
      return this.IsEnabled;
    }

    protected abstract void Clear();

    private void ClearFilter()
    {
      if (this.IsDefault)
        return;
      this.Clear();
    }

    protected void UpdateDescriptionToAllOrFiltered()
    {
      this.Description = this.IsDefault ? Resources.FiltersTab_StatusAll : Resources.FiltersTab_StatusFiltered;
    }

    private static string GetName(string name, AggregationFunction function)
    {
      switch (function)
      {
        case AggregationFunction.None:
        case AggregationFunction.UserDefined:
          return name;
        case AggregationFunction.Sum:
          return string.Format(Resources.FiltersTab_FieldSumName, (object) name);
        case AggregationFunction.Average:
          return string.Format(Resources.FiltersTab_FieldAvgName, (object) name);
        case AggregationFunction.Count:
        case AggregationFunction.DistinctCount:
          return string.Format(Resources.FiltersTab_FieldCountName, (object) name);
        case AggregationFunction.Min:
          return string.Format(Resources.FiltersTab_FieldMinName, (object) name);
        case AggregationFunction.Max:
          return string.Format(Resources.FiltersTab_FieldMaxName, (object) name);
        default:
          return (string) null;
      }
    }

    private static AggregationFunction[] GetAggregationFunctions(TableMember tableMember)
    {
      if (tableMember is TableMeasure)
        return FilterViewModelBase.UserDefinedFunctions;
      switch (tableMember.DataType)
      {
        case TableMemberDataType.Unknown:
          return (AggregationFunction[]) null;
        case TableMemberDataType.String:
        case TableMemberDataType.Bool:
        case TableMemberDataType.DateTime:
          return FilterViewModelBase.DefaultFunctions;
        case TableMemberDataType.Double:
        case TableMemberDataType.Long:
        case TableMemberDataType.Currency:
          return FilterViewModelBase.NumericFunctions;
        default:
          return (AggregationFunction[]) null;
      }
    }

    public delegate void ChangeAggregationFunction(FilterViewModelBase filterViewModel, AggregationFunction previous, AggregationFunction current);
  }
}
