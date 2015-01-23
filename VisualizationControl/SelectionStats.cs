using Microsoft.Data.Visualization.VisualizationCommon;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class SelectionStats : PropertyChangedNotificationBase
  {
    private object syncLock = new object();
    private double? _Average = new double?(0.0);
    private double? _Sum = new double?(0.0);
    private double? _Min = new double?(0.0);
    private double? _Max = new double?(0.0);
    private int _Count;

    public string PropertyCount
    {
      get
      {
        return "Count";
      }
    }

    public int Count
    {
      get
      {
        return this._Count;
      }
      private set
      {
        if (value == this._Count)
          return;
        this.SetProperty<int>(this.PropertyCount, ref this._Count, value);
      }
    }

    public string PropertyAverage
    {
      get
      {
        return "Average";
      }
    }

    public double? Average
    {
      get
      {
        return this._Average;
      }
      private set
      {
        double? nullable1 = value;
        double? nullable2 = this._Average;
        if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : (nullable1.HasValue != nullable2.HasValue ? 1 : 0)) == 0)
          return;
        this.SetProperty<double?>(this.PropertyAverage, ref this._Average, value);
      }
    }

    public string PropertySum
    {
      get
      {
        return "Sum";
      }
    }

    public double? Sum
    {
      get
      {
        return this._Sum;
      }
      private set
      {
        double? nullable1 = value;
        double? nullable2 = this._Sum;
        if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : (nullable1.HasValue != nullable2.HasValue ? 1 : 0)) == 0)
          return;
        this.SetProperty<double?>(this.PropertySum, ref this._Sum, value);
      }
    }

    public string PropertyMin
    {
      get
      {
        return "Min";
      }
    }

    public double? Min
    {
      get
      {
        return this._Min;
      }
      private set
      {
        double? nullable1 = value;
        double? nullable2 = this._Min;
        if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : (nullable1.HasValue != nullable2.HasValue ? 1 : 0)) == 0)
          return;
        this.SetProperty<double?>(this.PropertyMin, ref this._Min, value);
      }
    }

    public string PropertyMax
    {
      get
      {
        return "Max";
      }
    }

    public double? Max
    {
      get
      {
        return this._Max;
      }
      private set
      {
        double? nullable1 = value;
        double? nullable2 = this._Max;
        if ((nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : (nullable1.HasValue != nullable2.HasValue ? 1 : 0)) == 0)
          return;
        this.SetProperty<double?>(this.PropertyMax, ref this._Max, value);
      }
    }

    public bool HasValues
    {
      get
      {
        return !this.NoValues;
      }
    }

    private bool NoValues { get; set; }

    private bool HasStatValues { get; set; }

    private int CountForAverage { get; set; }

    private double SumForAverage { get; set; }

    public event Action SelectionUpdate;

    public SelectionStats()
    {
      this.Clear();
    }

    internal void Clear()
    {
      lock (this.syncLock)
      {
        this.Count = 0;
        this.CountForAverage = 0;
        this.SumForAverage = 0.0;
        this.Average = new double?();
        this.Sum = new double?();
        this.Min = new double?();
        this.Max = new double?();
        this.NoValues = true;
        this.HasStatValues = false;
      }
      if (this.SelectionUpdate == null)
        return;
      this.SelectionUpdate();
    }

    internal void UpdateWithValue(double val)
    {
      lock (this.syncLock)
      {
        this.NoValues = false;
        ++this.Count;
        if (double.IsNaN(val))
          return;
        this.HasStatValues = true;
        if (this.CountForAverage == 0)
        {
          this.CountForAverage = 1;
          this.SumForAverage = val;
          this.Average = new double?(val);
          this.Sum = new double?(val);
          this.Min = new double?(val);
          this.Max = new double?(val);
        }
        else
        {
          ++this.CountForAverage;
          this.SumForAverage += val;
          this.Average = new double?(this.SumForAverage / (double) this.CountForAverage);
          SelectionStats temp_32 = this;
          double? local_2 = temp_32.Sum;
          double local_3 = val;
          double? temp_42 = local_2.HasValue ? new double?(local_2.GetValueOrDefault() + local_3) : new double?();
          temp_32.Sum = temp_42;
          this.Min = new double?(Math.Min(this.Min.Value, val));
          this.Max = new double?(Math.Max(this.Max.Value, val));
        }
      }
    }

    internal void UpdateWithSelectionStats(SelectionStats selectionStats)
    {
      if (selectionStats == null || selectionStats.NoValues)
        return;
      lock (this.syncLock)
      {
        lock (selectionStats.syncLock)
        {
          if (this.NoValues)
          {
            this.Count = selectionStats.Count;
            this.NoValues = false;
          }
          else
            this.Count += selectionStats.Count;
          if (selectionStats.HasStatValues)
          {
            if (this.HasStatValues)
            {
              this.CountForAverage += selectionStats.CountForAverage;
              this.SumForAverage += selectionStats.SumForAverage;
              this.Average = new double?(this.SumForAverage / (double) this.CountForAverage);
              SelectionStats temp_80 = this;
              double? local_4 = temp_80.Sum;
              double local_6 = selectionStats.Sum.Value;
              double? temp_93 = local_4.HasValue ? new double?(local_4.GetValueOrDefault() + local_6) : new double?();
              temp_80.Sum = temp_93;
              this.Min = new double?(Math.Min(this.Min.Value, selectionStats.Min.Value));
              this.Max = new double?(Math.Max(this.Max.Value, selectionStats.Max.Value));
            }
            else
            {
              this.HasStatValues = true;
              this.CountForAverage = selectionStats.CountForAverage;
              this.SumForAverage = selectionStats.SumForAverage;
              this.Average = new double?(selectionStats.Average.Value);
              this.Sum = new double?(selectionStats.Sum.Value);
              this.Min = new double?(selectionStats.Min.Value);
              this.Max = new double?(selectionStats.Max.Value);
            }
          }
        }
      }
      if (this.SelectionUpdate == null)
        return;
      this.SelectionUpdate();
    }
  }
}
