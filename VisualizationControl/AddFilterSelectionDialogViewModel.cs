using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class AddFilterSelectionDialogViewModel : DialogViewModelBase
  {
    private readonly WeakEventListener<AddFilterSelectionDialogViewModel, object, PropertyChangedEventArgs> onFieldSelectionChanged;
    private ICommand _CreateCommand;
    private bool canAddFilter;

    public static string PropertyCreateCommand
    {
      get
      {
        return "CreateCommand";
      }
    }

    public ICommand CreateCommand
    {
      get
      {
        return this._CreateCommand;
      }
      set
      {
        this.SetProperty<ICommand>(AddFilterSelectionDialogViewModel.PropertyCreateCommand, ref this._CreateCommand, value, false);
      }
    }

    public ObservableCollectionEx<TableIslandViewModel> FilterCandidates { get; private set; }

    public string PropertyCanAddFilterUI
    {
      get
      {
        return "CanAddFilter";
      }
    }

    public bool CanAddFilter
    {
      get
      {
        return this.canAddFilter;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyCanAddFilterUI, ref this.canAddFilter, value, false);
      }
    }

    public AddFilterSelectionDialogViewModel()
    {
      this.onFieldSelectionChanged = new WeakEventListener<AddFilterSelectionDialogViewModel, object, PropertyChangedEventArgs>(this);
      this.onFieldSelectionChanged.OnEventAction = new Action<AddFilterSelectionDialogViewModel, object, PropertyChangedEventArgs>(AddFilterSelectionDialogViewModel.Fields_ItemPropertyChanged);
    }

    public void Initialize(FieldListPickerViewModel fieldListPickerViewModel)
    {
      this.CanAddFilter = false;
      this.FilterCandidates = fieldListPickerViewModel.TableIslandsForFiltering;
      if (this.FilterCandidates == null)
        return;
      foreach (TableIslandViewModel tableIslandViewModel in (Collection<TableIslandViewModel>) this.FilterCandidates)
      {
        foreach (TableViewModel tableViewModel in (Collection<TableViewModel>) tableIslandViewModel.Tables)
        {
          foreach (TableFieldViewModel tableFieldViewModel in (Collection<TableFieldViewModel>) tableViewModel.Fields)
          {
            tableFieldViewModel.PropertyChanged -= new PropertyChangedEventHandler(this.onFieldSelectionChanged.OnEvent);
            tableFieldViewModel.IsSelected = false;
            tableFieldViewModel.PropertyChanged += new PropertyChangedEventHandler(this.onFieldSelectionChanged.OnEvent);
          }
        }
      }
    }

    private static void Fields_ItemPropertyChanged(AddFilterSelectionDialogViewModel dialog, object item, PropertyChangedEventArgs e)
    {
      if (!e.PropertyName.Equals("IsSelected"))
        return;
      TableFieldViewModel tableFieldViewModel1 = item as TableFieldViewModel;
      if (tableFieldViewModel1 != null && tableFieldViewModel1.IsSelected)
      {
        dialog.CanAddFilter = true;
      }
      else
      {
        bool flag = false;
        foreach (TableIslandViewModel tableIslandViewModel in (Collection<TableIslandViewModel>) dialog.FilterCandidates)
        {
          foreach (TableViewModel tableViewModel in (Collection<TableViewModel>) tableIslandViewModel.Tables)
          {
            foreach (TableFieldViewModel tableFieldViewModel2 in (Collection<TableFieldViewModel>) tableViewModel.Fields)
            {
              if (tableFieldViewModel2.IsSelected)
              {
                flag = true;
                break;
              }
            }
          }
        }
        dialog.CanAddFilter = flag;
      }
    }
  }
}
