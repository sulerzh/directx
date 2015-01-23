using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class PagingListCollectionView : ListCollectionView
  {
    private int currentPage = 1;
    private int totalPages = 1;
    private int itemsPerPage;
    private int start;
    private int end;

    private ListCollectionView InnerListView { get; set; }

    public ICommand NextPageCommand { get; private set; }

    public ICommand PrevPageCommand { get; private set; }

    public ICommand LastPageCommand { get; private set; }

    public ICommand FirstPageCommand { get; private set; }

    public int CurrentPage
    {
      get
      {
        return this.currentPage;
      }
      set
      {
        if (value < 1)
          value = 1;
        if (value > this.TotalPages)
          value = this.TotalPages;
        this.currentPage = value;
        this.Refresh();
        this.RaisePropertyChanged("CurrentPage");
      }
    }

    public int ItemsPerPage
    {
      get
      {
        return this.itemsPerPage;
      }
      set
      {
        this.itemsPerPage = value;
        this.CurrentPage = 1;
        this.RaisePropertyChanged("ItemsPerPage");
      }
    }

    public int TotalPages
    {
      get
      {
        return this.totalPages;
      }
      set
      {
        this.totalPages = value;
        this.RaisePropertyChanged("TotalPages");
      }
    }

    public int Start
    {
      get
      {
        return this.start;
      }
      set
      {
        this.start = value;
        this.RaisePropertyChanged("Start");
      }
    }

    public int End
    {
      get
      {
        return this.end;
      }
      set
      {
        this.end = value;
        this.RaisePropertyChanged("End");
      }
    }

    public override int Count
    {
      get
      {
        if (this.ItemsPerPage >= this.TotalItems)
          return this.TotalItems;
        if (this.CurrentPage != this.TotalPages)
          return this.ItemsPerPage;
        int num = this.TotalItems % this.ItemsPerPage;
        if (num == 0)
          return this.ItemsPerPage;
        else
          return num;
      }
    }

    public int TotalItems
    {
      get
      {
        return this.InnerListView.Count;
      }
    }

    public PagingListCollectionView(IList innerList, int itemsPerPage)
      : base(innerList)
    {
      this.InnerListView = new ListCollectionView(innerList);
      this.ItemsPerPage = itemsPerPage;
      this.NextPageCommand = (ICommand) new DelegatedCommand(new Action(this.NextPage), (Predicate) (() => this.CurrentPage < this.TotalPages));
      this.PrevPageCommand = (ICommand) new DelegatedCommand(new Action(this.PrevPage), (Predicate) (() => this.CurrentPage > 1));
      this.LastPageCommand = (ICommand) new DelegatedCommand(new Action(this.LastPage), (Predicate) (() => this.CurrentPage < this.TotalPages));
      this.FirstPageCommand = (ICommand) new DelegatedCommand(new Action(this.FirstPage), (Predicate) (() => this.CurrentPage > 1));
    }

    public override object GetItemAt(int index)
    {
      if (this.Start + index >= this.InnerListView.Count)
        return this.InnerListView.GetItemAt(this.InnerListView.Count - 1);
      else
        return this.InnerListView.GetItemAt(this.Start + index);
    }

    public void NextPage()
    {
      if (this.currentPage < this.TotalPages)
        ++this.CurrentPage;
      this.Refresh();
    }

    public void LastPage()
    {
      this.CurrentPage = this.TotalPages;
      this.Refresh();
    }

    public void PrevPage()
    {
      if (this.currentPage > 1)
        --this.CurrentPage;
      this.Refresh();
    }

    public void FirstPage()
    {
      this.CurrentPage = 1;
      this.Refresh();
    }

    public override void Refresh()
    {
      this.TotalPages = (int) Math.Ceiling((double) this.InnerListView.Count / (double) this.ItemsPerPage);
      this.Start = (this.CurrentPage - 1) * this.ItemsPerPage;
      this.End = this.Start + this.Count;
      this.InnerListView.SortDescriptions.Clear();
      foreach (SortDescription sortDescription in (Collection<SortDescription>) this.SortDescriptions)
        this.InnerListView.SortDescriptions.Add(sortDescription);
      base.Refresh();
    }

    protected void RaisePropertyChanged(string propertyName)
    {
      base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
  }
}
