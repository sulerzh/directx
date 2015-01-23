using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ListFilterViewModel<T> : ListFilterViewModel where T : IComparable<T>
  {
    private BitArray selectedItems = new BitArray(1);
    private bool replace = true;
    private bool isAllSpecified;
    private bool isBlankSpecified;
    private bool showBlank;
    private IEnumerable<T> allItems;
    private CategoryFilterClause<T> categoryFilterClause;

    public ObservableCollectionEx<ListFilterItemViewModel<T>> Fields { get; private set; }

    public override bool IsModeChangeSupported
    {
      get
      {
        return typeof (T) != typeof (bool);
      }
    }

    public override bool IsDefault
    {
      get
      {
        return this.categoryFilterClause.Unfiltered;
      }
    }

    public ListFilterViewModel(FieldListPickerViewModel fieldListPickerVm, CategoryFilterClause<T> clause)
    {
      this.IsEnabled = false;
      if (typeof (T) == typeof (string))
      {
        this.SearchCommand = (ICommand) new DelegatedCommand(new Action(this.OnSearch), (Predicate) (() =>
        {
          if (!string.IsNullOrEmpty(this.SearchString))
            return this.InSearchMode;
          else
            return false;
        }));
        this.ClearSearchCommand = (ICommand) new DelegatedCommand(new Action(this.ClearSearch), (Predicate) (() => this.ShowingSearchResults));
        this.IsSearchSupported = true;
      }
      this.fieldListPickerViewModel = fieldListPickerVm;
      this.Fields = new ObservableCollectionEx<ListFilterItemViewModel<T>>();
      this.Fields.ItemPropertyChanged += new ObservableCollectionExItemChangedHandler<ListFilterItemViewModel<T>>(this.FieldsOnItemPropertyChanged);
      this.categoryFilterClause = clause;
      this.categoryFilterClause.OnPropertiesUpdated += new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
      this.categoryFilterClause_OnPropertiesUpdated((object) this.categoryFilterClause, EventArgs.Empty);
    }

    protected override void OnSearch()
    {
      this.replace = false;
      this.Fields.RemoveAll();
      long count = 0L;
      long maxThusFar = -1L;
      SortedSet<long> results = new SortedSet<long>();
      string searchString = this.SearchString;
      Parallel.ForEach<T>(this.allItems, (Action<T, ParallelLoopState, long>) ((f, pls, i) =>
      {
        if (pls.IsStopped && i > maxThusFar)
          return;
        int num1 = this.Culture.CompareInfo.IndexOf(f.ToString(), searchString, 0, CompareOptions.IgnoreCase);
        if (num1 >= 0)
        {
          lock (results)
            results.Add(i);
        }
        int num2 = 100;
        long comparand;
        do
        {
          comparand = Volatile.Read(ref maxThusFar);
        }
        while (i > comparand && Interlocked.CompareExchange(ref maxThusFar, i, comparand) != comparand && --num2 > 0);
        if (num1 < 0 || Interlocked.Increment(ref count) != 50L)
          return;
        pls.Stop();
      }));
      int num = 0;
      foreach (int index in Enumerable.Take<long>((IEnumerable<long>) results, 51))
      {
        if (++num != 51)
        {
          T obj = Enumerable.ElementAt<T>(this.allItems, index);
          this.Fields.Add(new ListFilterItemViewModel<T>()
          {
            IsSelected = new bool?(this.selectedItems[index]),
            Item = obj,
            Name = obj.ToString(),
            Index = index
          });
        }
        else
          break;
      }
      this.IsListGreaterThanMaxShown = num == 51;
      this.ShowingSearchResults = true;
      this.replace = true;
    }

    public void ClearSearch()
    {
      this.replace = false;
      this.InSearchMode = false;
      this.ShowingSearchResults = false;
      this.PopulateInitialFields();
      this.replace = true;
    }

    public override void Delete()
    {
      this.categoryFilterClause.OnPropertiesUpdated -= new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
      this.fieldListPickerViewModel.RemoveFilterClause((FilterClause) this.categoryFilterClause);
    }

    protected override void Clear()
    {
      this.isAllSpecified = this.isBlankSpecified = this.showBlank = false;
      this.selectedItems.SetAll(false);
      this.Fields.RemoveAll();
      CategoryFilterClause<T> oldFilterClause = this.categoryFilterClause;
      oldFilterClause.OnPropertiesUpdated -= new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
      this.categoryFilterClause = new CategoryFilterClause<T>(oldFilterClause, (IEnumerable<T>) null, false, false);
      this.categoryFilterClause.OnPropertiesUpdated += new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
      this.fieldListPickerViewModel.ReplaceFilterClause((FilterClause) oldFilterClause, (FilterClause) this.categoryFilterClause);
      this.categoryFilterClause_OnPropertiesUpdated((object) this.categoryFilterClause, EventArgs.Empty);
    }

    private void categoryFilterClause_OnPropertiesUpdated(object sender, EventArgs e)
    {
      if (!object.ReferenceEquals(sender, (object) this.categoryFilterClause))
        return;
      this.replace = false;
      IEnumerable<T> selectableItems;
      BitArray selectedIndices;
      bool showBlank;
      bool blankSpecified;
      bool allSpecified;
      if (!this.categoryFilterClause.TryGetProperties(out selectableItems, out selectedIndices, out showBlank, out blankSpecified, out allSpecified))
        return;
      this.selectedItems = selectedIndices;
      this.allItems = selectableItems;
      this.isAllSpecified = allSpecified;
      this.isBlankSpecified = blankSpecified;
      this.showBlank = showBlank;
      this.UIDispatcher.Invoke((Action) (() =>
      {
        this.InSearchMode = this.ShowingSearchResults = this.IsListGreaterThanMaxShown = false;
        this.PopulateInitialFields();
        this.IsEnabled = true;
      }));
      this.replace = true;
      this.UpdateDescriptionToAllOrFiltered();
      this.RaisePropertyChanged(FilterViewModelBase.PropertyIsDefault);
    }

    private void PopulateInitialFields()
    {
      this.Fields.RemoveAll();
      this.Fields.Add(new ListFilterItemViewModel<T>()
      {
        Item = default (T),
        Name = Resources.FiltersTab_FilterAll,
        IsAll = true,
        IsSelected = this.IsAllSelected(),
        Index = -1
      });
      if (this.showBlank)
        this.Fields.Add(new ListFilterItemViewModel<T>()
        {
          Item = default (T),
          Name = Resources.FiltersTab_FilterBlank,
          IsBlank = true,
          IsSelected = new bool?(this.IsBlankSelected()),
          Index = -1
        });
      int index = 0;
      foreach (T obj in Enumerable.Take<T>(this.allItems, 50))
      {
        this.IsListGreaterThanMaxShown = index == 49;
        if (index == 49)
          break;
        this.Fields.Add(new ListFilterItemViewModel<T>()
        {
          IsSelected = new bool?(this.selectedItems[index]),
          Item = obj,
          Name = obj.ToString(),
          Index = index
        });
        ++index;
      }
    }

    private bool? IsAllSelected()
    {
      return !this.isAllSpecified && Enumerable.Any<bool>(Enumerable.Cast<bool>((IEnumerable) this.selectedItems), (Func<bool, bool>) (si => si)) || this.isAllSpecified && Enumerable.Any<bool>(Enumerable.Cast<bool>((IEnumerable) this.selectedItems), (Func<bool, bool>) (si => !si)) || this.isBlankSpecified ? new bool?() : (!this.isAllSpecified ? new bool?(false) : new bool?(true));
    }

    private bool IsBlankSelected()
    {
      if (this.isBlankSpecified && !this.isAllSpecified)
        return true;
      if (!this.isBlankSpecified)
        return this.isAllSpecified;
      else
        return false;
    }

    private void FieldsOnItemPropertyChanged(ListFilterItemViewModel<T> item, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (!this.replace || !propertyChangedEventArgs.PropertyName.Equals(ListFilterItemViewModel<T>.PropertyIsSelected))
        return;
      this.replace = false;
      CategoryFilterClause<T> oldFilterClause1 = this.categoryFilterClause;
      oldFilterClause1.OnPropertiesUpdated -= new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
      if (item.IsAll)
      {
        ListFilterItemViewModel<T> filterItemViewModel1 = item;
        bool? isSelected = item.IsSelected;
        bool? nullable = new bool?(isSelected.HasValue && isSelected.GetValueOrDefault());
        filterItemViewModel1.IsSelected = nullable;
        this.isAllSpecified = item.IsSelected.Value;
        this.selectedItems.SetAll(this.isAllSpecified);
        this.isBlankSpecified = false;
        foreach (ListFilterItemViewModel<T> filterItemViewModel2 in Enumerable.Skip<ListFilterItemViewModel<T>>((IEnumerable<ListFilterItemViewModel<T>>) this.Fields, 1))
          filterItemViewModel2.IsSelected = new bool?(this.isAllSpecified);
        CategoryFilterClause<T> oldFilterClause2 = oldFilterClause1;
        this.categoryFilterClause = new CategoryFilterClause<T>(categoryFilterClause, null, this.isBlankSpecified, isAllSpecified);
      }
      else if (item.IsSelected.HasValue)
      {
        if (!item.IsBlank)
          this.selectedItems[item.Index] = item.IsSelected.Value;
        else
          this.isBlankSpecified = item.IsSelected.Value && !this.isAllSpecified || !item.IsSelected.Value && this.isAllSpecified;
        if (this.Fields[0].IsAll)
          this.Fields[0].IsSelected = this.IsAllSelected();
        this.categoryFilterClause = new CategoryFilterClause<T>(oldFilterClause1, Enumerable.Where<T>(this.allItems, (Func<T, int, bool>) ((t, i) =>
        {
          if (!this.isAllSpecified)
            return this.selectedItems[i];
          else
            return !this.selectedItems[i];
        })), this.isBlankSpecified, this.isAllSpecified);
      }
      if (oldFilterClause1 != this.categoryFilterClause)
      {
        this.categoryFilterClause.OnPropertiesUpdated += new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
        this.fieldListPickerViewModel.ReplaceFilterClause((FilterClause) oldFilterClause1, (FilterClause) this.categoryFilterClause);
        this.UpdateDescriptionToAllOrFiltered();
        this.RaisePropertyChanged(FilterViewModelBase.PropertyIsDefault);
      }
      this.replace = true;
    }

    internal override FilterViewModelBase GetNextCompatibleFilterForFunction(AggregationFunction previous)
    {
      if (previous == this.Function)
        return (FilterViewModelBase) this;
      if (this.Function == AggregationFunction.None)
        return (FilterViewModelBase) null;
      NumericRangeFilterClause rangeFilterClause = new NumericRangeFilterClause(this.categoryFilterClause.TableMember, this.Function, new double?(), new double?());
      this.categoryFilterClause.OnPropertiesUpdated -= new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
      this.fieldListPickerViewModel.ReplaceFilterClause((FilterClause) this.categoryFilterClause, (FilterClause) rangeFilterClause);
      return FilterViewModelBase.Get(this.fieldListPickerViewModel, (FilterClause) rangeFilterClause, false);
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
            filterClause = (FilterClause) new AndOrFilterClause(this.categoryFilterClause.TableMember, this.Function, FilterPredicateOperator.And, (FilterPredicate) new StringFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<StringFilterPredicateComparison>(), (string) null), (FilterPredicate) new StringFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<StringFilterPredicateComparison>(), (string) null));
            break;
          case TableMemberDataType.Double:
          case TableMemberDataType.Long:
          case TableMemberDataType.Currency:
            filterClause = (FilterClause) new AndOrFilterClause(this.categoryFilterClause.TableMember, this.Function, FilterPredicateOperator.And, (FilterPredicate) new NumericFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<NumericFilterPredicateComparison>(), new double?()), (FilterPredicate) new NumericFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<NumericFilterPredicateComparison>(), new double?()));
            break;
          case TableMemberDataType.DateTime:
            filterClause = (FilterClause) new AndOrFilterClause(this.categoryFilterClause.TableMember, this.Function, FilterPredicateOperator.And, (FilterPredicate) new DateTimeFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<DateTimeFilterPredicateComparison>(), new DateTime?()), (FilterPredicate) new DateTimeFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<DateTimeFilterPredicateComparison>(), new DateTime?()));
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      if (filterClause == null)
        return (FilterViewModelBase) null;
      this.categoryFilterClause.OnPropertiesUpdated -= new EventHandler(this.categoryFilterClause_OnPropertiesUpdated);
      this.fieldListPickerViewModel.ReplaceFilterClause((FilterClause) this.categoryFilterClause, filterClause);
      return FilterViewModelBase.Get(this.fieldListPickerViewModel, filterClause, false);
    }
  }
}
