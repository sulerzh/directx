using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class TableViewModel : ViewModelBase
  {
    private bool _IsExpanded = true;
    private string _Name;
    private bool _IsEnabled;

    public string PropertyName
    {
      get
      {
        return "Name";
      }
    }

    public string Name
    {
      get
      {
        return this._Name;
      }
      set
      {
        this.SetProperty<string>(this.PropertyName, ref this._Name, value, false);
      }
    }

    public string PropertyIsEnabled
    {
      get
      {
        return "IsEnabled";
      }
    }

    public bool IsEnabled
    {
      get
      {
        return this._IsEnabled;
      }
      set
      {
        base.SetProperty<bool>(this.PropertyIsEnabled, ref this._IsEnabled, value, new Action(this.OnIsEnabledChanged));
      }
    }

    public string PropertyIsExpanded
    {
      get
      {
        return "IsExpanded";
      }
    }

    public bool IsExpanded
    {
      get
      {
        return this._IsExpanded;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyIsExpanded, ref this._IsExpanded, value, false);
      }
    }

    public ObservableCollectionEx<TableFieldViewModel> Fields { get; private set; }

    public DragItemsHandler<TableFieldViewModel> FieldsDragHandler { get; private set; }

    public event TableViewModel.TableFieldSelectedChangedDelegate TableFieldSelectedChanged;

    public TableViewModel()
    {
      this.Fields = new ObservableCollectionEx<TableFieldViewModel>();
      this.Fields.ItemAdded += (ObservableCollectionExChangedHandler<TableFieldViewModel>) (item => item.PropertyChanged += new PropertyChangedEventHandler(this.OnTableFieldSelectedChanged));
      this.Fields.ItemRemoved += (ObservableCollectionExChangedHandler<TableFieldViewModel>) (item => item.PropertyChanged -= new PropertyChangedEventHandler(this.OnTableFieldSelectedChanged));
      this.FieldsDragHandler = new DragItemsHandler<TableFieldViewModel>((Collection<TableFieldViewModel>) this.Fields, true);
    }

    public TableViewModel(TableMetadata model, bool disableTableMeasures, Action<TableFieldViewModel> addHeight = null, Action<TableFieldViewModel> addCategory = null, Action<TableFieldViewModel> addTime = null, Func<bool> chartAllowsCategories = null)
      : this()
    {
      this.Name = model.DisplayName;
      foreach (TableField model1 in model.Fields)
      {
        if (model1.Visible)
          this.Fields.Add(new TableFieldViewModel(model1, false, addHeight, addCategory, addTime, chartAllowsCategories));
      }
      foreach (TableMeasure tableMeasure in model.Measures)
      {
        if (tableMeasure.Visible)
          this.Fields.Add(new TableFieldViewModel((TableField) tableMeasure, disableTableMeasures, addHeight, addCategory, addTime, chartAllowsCategories));
      }
    }

    private void OnTableFieldSelectedChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.TableFieldSelectedChanged == null || !(e.PropertyName == TableFieldViewModel.PropertyIsSelected))
        return;
      this.TableFieldSelectedChanged(this, (TableFieldViewModel) sender);
    }

    private void OnIsEnabledChanged()
    {
      this.IsExpanded = this.IsEnabled;
    }

    public delegate void TableFieldSelectedChangedDelegate(TableViewModel table, TableFieldViewModel field);
  }
}
