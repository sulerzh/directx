using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TaskPanelFiltersTabViewModel : ViewModelBase
  {
    private readonly AddFilterSelectionDialogViewModel filterSelectionDialogViewModel;
    private CultureInfo modelCulture;
    private ILayerManagerViewModel _model;

    public string PropertyModel
    {
      get
      {
        return "Model";
      }
    }

    public ILayerManagerViewModel Model
    {
      get
      {
        return this._model;
      }
      set
      {
        this.SetProperty<ILayerManagerViewModel>(this.PropertyModel, ref this._model, value, false);
      }
    }

    public ICommand AddFiltersCommand { get; private set; }

    public ICommand ClearFiltersCommand { get; private set; }

    public ObservableCollectionEx<FilterViewModelBase> Filters { get; private set; }

    public TaskPanelFiltersTabViewModel(ILayerManagerViewModel layerManagerViewModel)
    {
      this.Model = layerManagerViewModel;
      this.Model.PropertyChanged += new PropertyChangedEventHandler(this.ModelOnPropertyChanged);
      this.Model.PropertyChanging += new PropertyChangingEventHandler(this.ModelOnPropertyChanging);
      this.Filters = new ObservableCollectionEx<FilterViewModelBase>();
      this.filterSelectionDialogViewModel = new AddFilterSelectionDialogViewModel()
      {
        CreateCommand = (ICommand) new DelegatedCommand(new Action(this.CreateFilters))
      };
      this.AddFiltersCommand = (ICommand) new DelegatedCommand(new Action(this.OnAddFilters), new Predicate(this.CanAddFilters));
    }

    private void ModelOnPropertyChanging(object sender, PropertyChangingEventArgs propertyChangingEventArgs)
    {
      if (!propertyChangingEventArgs.PropertyName.Equals(this.Model.PropertySelectedLayer) || this.Model.SelectedLayer == null || this.Model.SelectedLayer.FieldListPicker == null)
        return;
      this.Model.SelectedLayer.FieldListPicker.OnFilterClausesCleared -= new EventHandler(this.FieldListPicker_OnFilterClausesCleared);
    }

    private void FieldListPicker_OnFilterClausesCleared(object sender, EventArgs e)
    {
      FieldListPickerViewModel listPickerViewModel = sender as FieldListPickerViewModel;
      if (listPickerViewModel == null || this.Model.SelectedLayer.FieldListPicker != listPickerViewModel)
        return;
      foreach (FilterViewModelBase filter in (Collection<FilterViewModelBase>) this.Filters)
        this.CleanupFilterCallbacks(filter);
      this.Filters.RemoveAll();
    }

    private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (!propertyChangedEventArgs.PropertyName.Equals(this.Model.PropertySelectedLayer))
        return;
      if (this.Model.SelectedLayer != null && this.Model.SelectedLayer.FieldListPicker != null)
      {
        this.modelCulture = this.Model.SelectedLayer.LayerDefinition.LayerManager.Model.GetModelMetadata().Culture;
        this.Model.SelectedLayer.FieldListPicker.OnFilterClausesCleared += new EventHandler(this.FieldListPicker_OnFilterClausesCleared);
        this.RepopulateFilters();
      }
      else
      {
        foreach (FilterViewModelBase filter in (Collection<FilterViewModelBase>) this.Filters)
          this.CleanupFilterCallbacks(filter);
        this.Filters.RemoveAll();
      }
    }

    private void RepopulateFilters()
    {
      this.Filters.Clear();
      int num = 0;
      foreach (FilterClause clause in this.Model.SelectedLayer.FieldListPicker.Filter.FilterClauses)
        this.AddFilterViewModel(FilterViewModelBase.Get(this.Model.SelectedLayer.FieldListPicker, clause, false), num++);
    }

    private void OnAddFilters()
    {
      this.filterSelectionDialogViewModel.Initialize(this.Model.SelectedLayer.FieldListPicker);
      this.Model.SelectedLayer.FieldListPicker.DialogServiceProvider.ShowDialog((IDialog) this.filterSelectionDialogViewModel);
    }

    private bool CanAddFilters()
    {
      if (this.Model.SelectedLayer != null && this.Model.SelectedLayer.HasTables && (this.Model.SelectedLayer.LayerDefinition != null && this.Model.SelectedLayer.LayerDefinition.GeoVisualization != null) && (this.Model.SelectedLayer.LayerDefinition.GeoVisualization.GeoFieldWellDefinition != null && this.Model.SelectedLayer.LayerDefinition.GeoVisualization.GeoFieldWellDefinition.Geo != null))
        return this.Model.SelectedLayer.LayerDefinition.GeoVisualization.GeoFieldWellDefinition.Geo.GeoColumns.Count > 0;
      else
        return false;
    }

    private void CreateFilters()
    {
      this.Model.SelectedLayer.FieldListPicker.DialogServiceProvider.DismissDialog((IDialog) this.filterSelectionDialogViewModel);
      foreach (TableIslandViewModel tableIslandViewModel in (Collection<TableIslandViewModel>) this.filterSelectionDialogViewModel.FilterCandidates)
      {
        if (tableIslandViewModel.IsEnabled)
        {
          foreach (TableViewModel tableViewModel in (Collection<TableViewModel>) tableIslandViewModel.Tables)
          {
            if (tableViewModel.IsEnabled)
            {
              foreach (TableFieldViewModel tableFieldViewModel in (Collection<TableFieldViewModel>) tableViewModel.Fields)
              {
                if (tableFieldViewModel.IsSelected && tableFieldViewModel.IsEnabled)
                {
                  TableMember tableMember = (TableMember) tableFieldViewModel.Model;
                  FilterClause clause = (FilterClause) null;
                  TableMeasure tableMeasure = tableMember as TableMeasure;
                  if (tableMeasure != null)
                  {
                    clause = (FilterClause) new NumericRangeFilterClause((TableMember) tableMeasure, AggregationFunction.UserDefined, new double?(), new double?());
                  }
                  else
                  {
                    switch (tableFieldViewModel.ColumnDataType)
                    {
                      case TableMemberDataType.String:
                        clause = (FilterClause) new CategoryFilterClause<string>(tableMember, Resources.Culture, (IEnumerable<string>) null, false, false);
                        break;
                      case TableMemberDataType.Bool:
                        clause = (FilterClause) new CategoryFilterClause<bool>(tableMember, Resources.Culture, (IEnumerable<bool>) null, false, false);
                        break;
                      case TableMemberDataType.Double:
                      case TableMemberDataType.Long:
                      case TableMemberDataType.Currency:
                        clause = (FilterClause) new NumericRangeFilterClause(tableMember, AggregationFunction.Sum, new double?(), new double?());
                        break;
                      case TableMemberDataType.DateTime:
                        clause = (FilterClause) new AndOrFilterClause(tableMember, AggregationFunction.None, FilterPredicateOperator.And, (FilterPredicate) new DateTimeFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<DateTimeFilterPredicateComparison>(), new DateTime?()), (FilterPredicate) new DateTimeFilterPredicate(ExpressionFilterViewModel.GetDefaultPredicate<DateTimeFilterPredicateComparison>(), new DateTime?()));
                        break;
                    }
                  }
                  if (clause != null)
                    this.AddFilterViewModel(FilterViewModelBase.Get(this.Model.SelectedLayer.FieldListPicker, clause, true), this.Filters.Count);
                }
              }
            }
          }
        }
      }
    }

    private bool CanChangeMode(object obj)
    {
      FilterViewModelBase filterViewModelBase = obj as FilterViewModelBase;
      if (filterViewModelBase != null && filterViewModelBase.IsEnabled)
        return filterViewModelBase.IsDefault;
      else
        return false;
    }

    private void ChangeMode(object obj)
    {
      if (!this.CanChangeMode(obj))
        return;
      FilterViewModelBase filter = obj as FilterViewModelBase;
      if (filter == null)
        return;
      FilterViewModelBase compatibleFilter = filter.GetNextCompatibleFilter();
      int index;
      if (compatibleFilter == null || (index = this.Filters.IndexOf(filter)) < 0)
        return;
      this.CleanupFilterCallbacks(filter);
      this.Filters.RemoveAt(index);
      this.AddFilterViewModel(compatibleFilter, index);
    }

    private void AddFilterViewModel(FilterViewModelBase filter, int index)
    {
      filter.IsExpanded = true;
      filter.DeleteCommand = (ICommand) new DelegatedCommand(new Action<object>(this.DeleteFilter), (object) filter);
      filter.ModeCommand = (ICommand) new DelegatedCommand(new Action<object>(this.ChangeMode), (object) filter);
      filter.ChangeAggregationFunctionCallback = new FilterViewModelBase.ChangeAggregationFunction(this.ChangeAggregationFunctionCallback);
      filter.Culture = this.modelCulture;
      if (index >= this.Filters.Count)
        this.Filters.Add(filter);
      else
        this.Filters.Insert(index, filter);
    }

    private void ChangeAggregationFunctionCallback(FilterViewModelBase filterViewModel, AggregationFunction previous, AggregationFunction current)
    {
      if (filterViewModel == null || previous == current)
        return;
      FilterViewModelBase filterForFunction = filterViewModel.GetNextCompatibleFilterForFunction(previous);
      if (filterForFunction == null)
        return;
      int index;
      if ((index = this.Filters.IndexOf(filterViewModel)) < 0)
        return;
      this.CleanupFilterCallbacks(filterViewModel);
      this.Filters.RemoveAt(index);
      this.AddFilterViewModel(filterForFunction, index);
    }

    private void DeleteFilter(object toDelete)
    {
      FilterViewModelBase filter = (FilterViewModelBase) toDelete;
      filter.Delete();
      this.CleanupFilterCallbacks(filter);
      this.Filters.Remove(filter);
    }

    private void CleanupFilterCallbacks(FilterViewModelBase filter)
    {
      filter.DeleteCommand = (ICommand) null;
      filter.ModeCommand = (ICommand) null;
      filter.ChangeAggregationFunctionCallback = (FilterViewModelBase.ChangeAggregationFunction) null;
    }
  }
}
