using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class ListFilterViewModel : FilterViewModelBase
  {
    public static string PropertySearchString = "SearchString";
    public static string PropertyIsListGreaterThanMaxShown = "IsListGreaterThanMaxShown";
    public static string PropertyShowingSeachResults = "ShowingSearchResults";
    public static string PropertyInSearchMode = "InSearchMode";
    public static string PropertyIsSearchEnabled = "IsSearchEnabled";
    private string searchString = Resources.FiltersTab_FilterSearchText;
    protected const int MaxShown = 50;
    private bool isListGreaterThanMaxShown;
    private bool showingSearchResults;
    private bool inSearchMode;

    public ICommand SearchCommand { get; set; }

    public ICommand ClearSearchCommand { get; set; }

    public string SearchString
    {
      get
      {
        return this.searchString;
      }
      set
      {
        if (!this.SetProperty<string>(ListFilterViewModel.PropertySearchString, ref this.searchString, value, false) || !this.InSearchMode)
          return;
        this.OnSearch();
      }
    }

    public bool IsSearchSupported { get; protected set; }

    public bool IsListGreaterThanMaxShown
    {
      get
      {
        return this.isListGreaterThanMaxShown;
      }
      set
      {
        if (!this.SetProperty<bool>(ListFilterViewModel.PropertyIsListGreaterThanMaxShown, ref this.isListGreaterThanMaxShown, value, false))
          return;
        this.RaisePropertyChanged(ListFilterViewModel.PropertyIsSearchEnabled);
      }
    }

    public bool ShowingSearchResults
    {
      get
      {
        return this.showingSearchResults;
      }
      set
      {
        this.SetProperty<bool>(ListFilterViewModel.PropertyShowingSeachResults, ref this.showingSearchResults, value, false);
      }
    }

    public bool InSearchMode
    {
      get
      {
        return this.inSearchMode;
      }
      set
      {
        if (!this.SetProperty<bool>(ListFilterViewModel.PropertyInSearchMode, ref this.inSearchMode, value, false))
          return;
        this.RaisePropertyChanged(ListFilterViewModel.PropertyIsSearchEnabled);
        this.SearchString = this.inSearchMode ? string.Empty : Resources.FiltersTab_FilterSearchText;
      }
    }

    public bool IsSearchEnabled
    {
      get
      {
        if (!this.IsListGreaterThanMaxShown || !this.IsSearchSupported)
          return this.InSearchMode;
        else
          return true;
      }
    }

    public abstract override bool IsDefault { get; }

    internal abstract override FilterViewModelBase GetNextCompatibleFilterForFunction(AggregationFunction previous);

    protected abstract void OnSearch();
  }
}
