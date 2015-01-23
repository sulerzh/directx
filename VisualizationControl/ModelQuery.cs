using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class ModelQuery
  {
    public static DateTime UnknownDateTime
    {
      get
      {
        return DateTime.MaxValue;
      }
    }

    public string Name { get; protected set; }

    public Filter Filter { get; protected set; }

    public CultureInfo ModelCulture { get; set; }

    public CancellationToken Cancellationtoken { get; protected set; }

    public string QueryString { get; protected set; }

    public int ResultsItemCount { get; protected set; }

    public bool QueryUsesAggregation { get; protected set; }

    public ModelQuery(string name, Filter filter, CultureInfo modelCulture, CancellationToken cancellationToken)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (modelCulture == null)
        throw new ArgumentNullException("modelCulture");
      this.Name = name;
      this.Filter = new Filter();
      this.Filter.SetFilterClausesFrom(filter);
      this.ModelCulture = modelCulture;
      this.Cancellationtoken = cancellationToken;
    }

    public abstract void Clear();

    protected internal virtual void Shutdown()
    {
      this.Name = string.Empty;
      this.QueryString = string.Empty;
      this.Filter = new Filter();
    }

    public abstract void AddKey(ModelQueryKeyColumn modelQueryKey);

    public abstract void AddMeasure(ModelQueryMeasureColumn modelQueryMeasure);

    public abstract void QueryData(CancellationToken cancellationToken);

    public VisualizationTraceSource CreateLogQueryResultsTraceSource(string sourceName)
    {
      try
      {
        string environmentVariable = Environment.GetEnvironmentVariable("SodoQueryResultsLogFile");
        if (environmentVariable == null || !Directory.Exists(environmentVariable))
          return (VisualizationTraceSource) null;
        VisualizationTraceSource visualizationTraceSource = new VisualizationTraceSource("Microsoft.Data.QueryResultsLogger", SourceLevels.All);
        visualizationTraceSource.AddFileTraceListener(string.Concat(new object[4]
        {
          (object) environmentVariable,
          (object) Path.DirectorySeparatorChar,
          (object) sourceName,
          (object) ".log"
        }));
        visualizationTraceSource.RemoveDefaultTraceListeners();
        visualizationTraceSource.AssertUIEnabled = false;
        foreach (TraceListener traceListener in visualizationTraceSource.Listeners)
          traceListener.TraceOutputOptions = TraceOptions.None;
        visualizationTraceSource.TraceInformation("{0}Query: {1}", (object) "\t", (object) (this.QueryString ?? "null"));
        return visualizationTraceSource;
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "CreateLogQueryResultsTraceSource(): Caught exception, will not log query results, ex={0}", (object) ex);
        return (VisualizationTraceSource) null;
      }
    }
  }
}
