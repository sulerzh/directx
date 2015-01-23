using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class FilterClause
  {
    private const int UnknownVersion = -1;
    private const int VersionToForceUpdate = -2;

    public TableMember TableMember { get; private set; }

    public AggregationFunction AggregationFunction { get; private set; }

    public int RequestedVersion { get; private set; }

    public int CurrentVersion { get; protected set; }

    public int ForceUpdateCount { get; protected set; }

    public virtual bool Unfiltered
    {
      get
      {
        return false;
      }
    }

    public abstract bool HasUpdatableProperties { get; }

    public event EventHandler OnPropertiesUpdated;

    public FilterClause(TableMember tableMember, AggregationFunction afn)
    {
      if (tableMember == null)
        throw new ArgumentNullException("tableMember");
      this.TableMember = tableMember;
      this.AggregationFunction = afn;
      this.RequestedVersion = -1;
      this.CurrentVersion = -2;
    }

    protected FilterClause(FilterClause oldFilterClause)
      : this(oldFilterClause.TableMember, oldFilterClause.AggregationFunction)
    {
      this.RequestedVersion = oldFilterClause.RequestedVersion;
      this.CurrentVersion = oldFilterClause.CurrentVersion;
    }

    protected FilterClause(FilterClause.SerializableFilterClause state)
    {
      this.RequestedVersion = -1;
      this.CurrentVersion = -2;
    }

    public bool UpdateTableMember(TableMember newMember)
    {
      if (!this.TableMember.RefersToTheSameMemberAs(newMember))
        return false;
      TableMemberDataType dataType1 = this.TableMember.DataType;
      TableMemberDataType dataType2 = newMember.DataType;
      if (dataType1 == dataType2)
      {
        this.TableMember = newMember;
        return true;
      }
      else
      {
        if (dataType1 != TableMemberDataType.Currency && dataType1 != TableMemberDataType.Long && dataType1 != TableMemberDataType.Double || dataType2 != TableMemberDataType.Currency && dataType2 != TableMemberDataType.Long && dataType2 != TableMemberDataType.Double)
          return false;
        this.TableMember = newMember;
        return true;
      }
    }

    protected void DispatchPropertiesUpdated()
    {
      if (this.OnPropertiesUpdated != null)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "DispatchPropertiesUpdated(): Calling OnPropertiesUpdated() for filterclause (table column = {0}, Agg Fn = {1}, CurrentVersion={2})", (object) this.TableMember.Name, (object) this.AggregationFunction, (object) this.CurrentVersion);
        this.OnPropertiesUpdated((object) this, EventArgs.Empty);
      }
      else
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "DispatchPropertiesUpdated(): No subscribers for OnPropertiesUpdated() for filterclause (table column = {0}, Agg Fn = {1}, CurrentVersion={2})", (object) this.TableMember.Name, (object) this.AggregationFunction, (object) this.CurrentVersion);
    }

    protected virtual void Wrap(FilterClause.SerializableFilterClause sfc)
    {
      sfc.AggregationFunction = this.AggregationFunction;
      sfc.TableMember = this.TableMember.Wrap();
    }

    internal virtual void Unwrap(FilterClause.SerializableFilterClause sfc)
    {
      this.AggregationFunction = sfc.AggregationFunction;
      this.TableMember = sfc.TableMember.Unwrap() as TableMember;
      if (this.TableMember == null)
        throw new ArgumentException("Unwrapped TableMember is null");
    }

    internal void UpdateVersion(int newVersion)
    {
      if (this.CurrentVersion == this.RequestedVersion)
      {
        this.CurrentVersion = this.RequestedVersion = newVersion;
      }
      else
      {
        this.RequestedVersion = newVersion;
        this.ForceUpdate();
      }
      this.ForceUpdateCount = 0;
    }

    internal void SetUpdatePending(int newVersion)
    {
      this.RequestedVersion = newVersion;
      this.ForceUpdate();
      this.ForceUpdateCount = 0;
    }

    internal void ForceUpdate()
    {
      if (this.CurrentVersion != this.RequestedVersion)
        return;
      this.CurrentVersion = -2;
      ++this.ForceUpdateCount;
    }

    internal virtual bool UpdateProperties(int queryVersion, ModelQueryColumn queryColumn)
    {
      if (queryVersion == this.RequestedVersion)
      {
        this.CurrentVersion = queryVersion;
        return true;
      }
      else
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "UpdateProperties(): returning false for filterclause (table column = {0}, Agg Fn = {1}, queryVersion={2}, RequestedVersion={3})", (object) this.TableMember.Name, (object) this.AggregationFunction, (object) queryVersion, (object) this.CurrentVersion);
        return false;
      }
    }

    internal abstract FilterClause.SerializableFilterClause Wrap();

    [Serializable]
    public abstract class SerializableFilterClause
    {
      [XmlAttribute("AF")]
      public AggregationFunction AggregationFunction;
      [XmlElement("CalcFn", typeof (TableMeasure.SerializableTableMeasure))]
      [XmlElement("Measure", typeof (TableColumn.SerializableTableColumn))]
      public TableField.SerializableTableField TableMember;

      internal abstract FilterClause Unwrap(CultureInfo modelCulture);
    }
  }
}
