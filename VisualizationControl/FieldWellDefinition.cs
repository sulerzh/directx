using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public abstract class FieldWellDefinition
    {
        public const int NumberOfAggregationFunctions = 7;
        private const int FieldsChangedCounterValueForNoPendingQueries = 0;
        private int fieldsChangedCounter;
        private volatile bool callDisplayPropertiesUpdated;

        internal bool FieldsChangedSinceLastQuery
        {
            get
            {
                return this.fieldsChangedCounter != FieldsChangedCounterValueForNoPendingQueries;
            }
            set
            {
                Visualization visualization = this.Visualization;
                if (!value)
                    return;
                if (Interlocked.Increment(ref this.fieldsChangedCounter) == 1 && visualization != null)
                {
                    visualization.DisplayPropertiesUpdated(false);
                }
                else
                {
                    if (!this.callDisplayPropertiesUpdated)
                        return;
                    if (visualization != null)
                        visualization.DisplayPropertiesUpdated(false);
                    this.callDisplayPropertiesUpdated = false;
                }
            }
        }

        private Visualization Visualization { get; set; }

        public FieldWellDefinition(Visualization visualization)
        {
            this.Visualization = visualization;
            this.fieldsChangedCounter = 1;
            this.callDisplayPropertiesUpdated = true;
        }

        internal bool GetFieldsChangedSinceLastQuery(out int counter)
        {
            counter = this.fieldsChangedCounter;
            return counter != FieldsChangedCounterValueForNoPendingQueries;
        }

        internal void ResetFieldsChangedSinceLastQuery(int counter)
        {
            this.callDisplayPropertiesUpdated = false;
            Interlocked.CompareExchange(ref this.fieldsChangedCounter, FieldsChangedCounterValueForNoPendingQueries, counter);
        }

        protected TableColumn FindTableColumnInModelMetadata(TableField field, ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged, ref TableIsland islandForQuery)
        {
            if (field == null)
                return null;
            TableColumn col = field as TableColumn;
            TableColumn tableColumn = col != null ? modelMetadata.FindVisibleTableColumnInModelMetadata(col) : null;
            if (tableColumn != null)
            {
                if (islandForQuery == null)
                    islandForQuery = tableColumn.Table.Island;
                else if (islandForQuery != tableColumn.Table.Island)
                    tableColumn = null;
            }
            if (tableColumn == null)
            {
                requery = queryChanged = true;

            }
            else
            {
                requery |= (col.DataType != tableColumn.DataType) || tablesWithUpdatedData.Contains(tableColumn.Table.ModelName);
            }
            return tableColumn;
        }

        protected TableMeasure FindTableMeasureInModelMetadata(TableMeasure measure, ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged, ref TableIsland islandForQuery)
        {
            if (measure == null)
              return null;
            TableMeasure tableMeasure = measure != null ? modelMetadata.FindVisibleTableMeasureInModelMetadata(measure) : null;
            if (tableMeasure != null)
            {
              if (islandForQuery == null)
                islandForQuery = tableMeasure.Table.Island;
              else if (islandForQuery != tableMeasure.Table.Island)
                tableMeasure = null;
            }
            if (tableMeasure == null)
            {
              requery = queryChanged = true;
            }
            else
            {
                requery |= (measure.DataType != tableMeasure.DataType) || tablesWithUpdatedData.Contains(tableMeasure.Table.ModelName);
            }
            return tableMeasure;
        }

        internal virtual void Shutdown()
        {
            this.Visualization = null;
        }

        protected virtual void Wrap(FieldWellDefinition.SerializableFieldWellDefinition state)
        {
        }

        internal virtual void Unwrap(FieldWellDefinition.SerializableFieldWellDefinition state)
        {
        }

        internal abstract void ModelMetadataChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged);

        internal abstract FieldWellDefinition.SerializableFieldWellDefinition Wrap();

        [Serializable]
        public abstract class SerializableFieldWellDefinition
        {
            internal abstract FieldWellDefinition Unwrap(Visualization visualization, CultureInfo modelCulture);
        }
    }
}
