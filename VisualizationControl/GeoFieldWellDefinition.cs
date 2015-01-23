using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class GeoFieldWellDefinition : FieldWellDefinition
    {
        private TimeChunkPeriod timeChunkPeriod;
        private bool accumulateResultsOverTime;
        private GeoFieldWellDefinition.PlaybackValueDecayType decay;
        private TimeSpan? decayTime;

        public GeoField Geo { get; private set; }

        public List<Tuple<TableField, AggregationFunction>> Measures { get; private set; }

        public TableField Category { get; private set; }

        public Tuple<TableField, AggregationFunction> Color { get; private set; }

        public TableField Time { get; private set; }

        public List<TableField> ChosenGeoFields { get; private set; }

        public List<GeoMappingType> ChosenGeoMappings { get; private set; }

        public Filter Filter { get; private set; }

        public bool UserSelectedMapByField { get; set; }

        public TimeSetting UserSelectedTimeSetting { get; set; }

        public bool ChoosingGeoFields { get; set; }

        public TimeChunkPeriod ChunkBy
        {
            get
            {
                return this.timeChunkPeriod;
            }
            set
            {
                if (this.timeChunkPeriod == value)
                    return;
                this.timeChunkPeriod = value;
                this.FieldsChangedSinceLastQuery = true;
            }
        }

        public bool AccumulateResultsOverTime
        {
            get
            {
                return this.accumulateResultsOverTime;
            }
            set
            {
                if (this.accumulateResultsOverTime == value)
                    return;
                this.accumulateResultsOverTime = value;
                this.FieldsChangedSinceLastQuery = true;
            }
        }

        public GeoFieldWellDefinition.PlaybackValueDecayType Decay
        {
            get
            {
                return this.decay;
            }
            set
            {
                if (this.decay == value)
                    return;
                this.decay = value;
                this.FieldsChangedSinceLastQuery = true;
            }
        }

        public TimeSpan? DecayTime
        {
            get
            {
                return this.decayTime;
            }
            set
            {
                TimeSpan? nullable1 = this.decayTime;
                TimeSpan? nullable2 = value;
                if ((nullable1.HasValue != nullable2.HasValue ? 1 : (!nullable1.HasValue ? 0 : (nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : 0))) == 0)
                    return;
                this.decayTime = value;
                this.FieldsChangedSinceLastQuery = true;
            }
        }

        public bool ViewModelPersistTimeData { get; set; }

        public bool ViewModelAccumulateResultsOverTime { get; set; }

        public GeoFieldWellDefinition(GeoVisualization visualization)
            : base((Visualization)visualization)
        {
            this.Measures = new List<Tuple<TableField, AggregationFunction>>();
            this.ChosenGeoFields = new List<TableField>();
            this.ChosenGeoMappings = new List<GeoMappingType>();
            this.Decay = GeoFieldWellDefinition.PlaybackValueDecayType.None;
            this.ChunkBy = TimeChunkPeriod.None;
            this.Filter = new Filter();
        }

        internal GeoFieldWellDefinition(GeoFieldWellDefinition.SerializableGeoFieldWellDefinition state, GeoVisualization visualization, CultureInfo modelCulture)
            : this(visualization)
        {
            this.Unwrap((FieldWellDefinition.SerializableFieldWellDefinition)state, modelCulture);
        }

        public bool SetGeo(GeoField geoField)
        {
            if (geoField != null)
                this.ValidateGeoField(geoField);
            bool flag = geoField != this.Geo;
            if (flag)
            {
                this.Geo = geoField;
                this.FieldsChangedSinceLastQuery = true;
            }
            return flag;
        }

        public bool SetCategory(TableField categoryField)
        {
            if (categoryField != null)
                this.ValidateCategoryField(categoryField);
            bool flag = categoryField != this.Category;
            if (flag)
            {
                this.Category = categoryField;
                this.FieldsChangedSinceLastQuery = true;
            }
            return flag;
        }

        public bool SetTime(TableField timeField)
        {
            if (timeField != null)
                this.ValidateTimeField(timeField);
            bool flag = timeField != this.Time;
            if (flag)
            {
                this.Time = timeField;
                this.FieldsChangedSinceLastQuery = true;
            }
            return flag;
        }

        public bool SetFilter(Filter filter)
        {
            Filter filter1 = this.Filter;
            if (filter1 == null || !filter1.SetFilterClausesFrom(filter))
                return false;
            this.FieldsChangedSinceLastQuery = true;
            return true;
        }

        public bool SetColor(TableField colorField, AggregationFunction aggregationFunction)
        {
            return this.SetColor(colorField == null ? (Tuple<TableField, AggregationFunction>)null : new Tuple<TableField, AggregationFunction>(colorField, aggregationFunction));
        }

        public bool SetColor(Tuple<TableField, AggregationFunction> color)
        {
            if (color != null)
                this.ValidateColorField(color.Item1);
            bool flag = color != this.Color;
            if (flag)
            {
                this.Color = color;
                this.FieldsChangedSinceLastQuery = true;
            }
            return flag;
        }

        public bool AddMeasure(TableField measureField, AggregationFunction aggregationFunction)
        {
            return this.AddMeasure(measureField == null ? (Tuple<TableField, AggregationFunction>)null : new Tuple<TableField, AggregationFunction>(measureField, aggregationFunction));
        }

        public bool AddMeasure(Tuple<TableField, AggregationFunction> measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");
            this.ValidateMeasureField(measure.Item1);
            if (this.Measures.Contains(measure))
                return false;
            this.Measures.Add(measure);
            this.FieldsChangedSinceLastQuery = true;
            return true;
        }

        public bool RemoveAllMeasures()
        {
            bool flag = false;
            if (this.Measures.Count > 0)
            {
                this.Measures.Clear();
                this.FieldsChangedSinceLastQuery = true;
                flag = true;
            }
            return flag;
        }

        public bool RemoveMeasure(TableField measureField, AggregationFunction aggregationFunction)
        {
            return this.RemoveMeasure(measureField == null ? (Tuple<TableField, AggregationFunction>)null : new Tuple<TableField, AggregationFunction>(measureField, aggregationFunction));
        }

        public bool RemoveMeasure(Tuple<TableField, AggregationFunction> measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");
            bool flag = this.Measures.Remove(measure);
            if (flag)
                this.FieldsChangedSinceLastQuery = true;
            return flag;
        }

        private bool AddFilterClause(FilterClause filterClause)
        {
            if (filterClause == null)
                throw new ArgumentNullException("filterClause");
            Filter filter = this.Filter;
            if (filter == null || !filter.AddFilterClause(filterClause))
                return false;
            this.FieldsChangedSinceLastQuery = true;
            return true;
        }

        private bool RemoveAllFilterClauses()
        {
            Filter filter = this.Filter;
            if (filter == null || !filter.RemoveAllFilterClauses())
                return false;
            this.FieldsChangedSinceLastQuery = true;
            return true;
        }

        internal override void Shutdown()
        {
            this.decayTime = new TimeSpan?();
            this.Geo = (GeoField)null;
            this.Category = (TableField)null;
            this.Color = (Tuple<TableField, AggregationFunction>)null;
            this.Time = (TableField)null;
            this.Measures = (List<Tuple<TableField, AggregationFunction>>)null;
            this.Filter = (Filter)null;
            base.Shutdown();
        }

        internal override void ModelMetadataChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData,
            ref bool requery, ref bool queryChanged)
        {
            TableIsland tableIsland = (TableIsland)null;
            if (this.Geo != null)
            {
                bool flag = false;
                this.Geo.ModelMetadataChanged(modelMetadata, tablesWithUpdatedData, ref requery, ref flag);
                queryChanged |= flag;
                if (this.Geo.GeoColumns.Count > 0)
                    tableIsland = this.Geo.GeoColumns[0].Table.Island;
                else
                    this.UserSelectedMapByField = false;
            }
            else
                this.UserSelectedMapByField = false;
            TableIsland islandForQuery1 = tableIsland;
            this.Category =
                (TableField)
                    this.FindTableColumnInModelMetadata(this.Category, modelMetadata, tablesWithUpdatedData, ref requery,
                        ref queryChanged, ref islandForQuery1);
            TableIsland islandForQuery2 = tableIsland;
            bool flag1 = this.Time != null;
            this.Time =
                (TableField)
                    this.FindTableColumnInModelMetadata(this.Time, modelMetadata, tablesWithUpdatedData, ref requery,
                        ref queryChanged, ref islandForQuery2);
            if (this.Time != null && ((TableMember)this.Time).DataType != TableMemberDataType.DateTime)
            {
                this.Time = (TableField)null;
                requery = queryChanged = true;

            }
            if (flag1 && this.Time == null)
            {
                this.ChunkBy = TimeChunkPeriod.None;
                this.Decay = GeoFieldWellDefinition.PlaybackValueDecayType.None;
                this.AccumulateResultsOverTime = false;
                this.DecayTime = new TimeSpan?();
                this.ViewModelPersistTimeData = false;
                this.ViewModelAccumulateResultsOverTime = false;
            }
            if (this.ChosenGeoFields.Count > 0)
            {
                int index = 0;
                List<TableField> list1 = new List<TableField>();
                List<GeoMappingType> list2 = new List<GeoMappingType>();
                bool requery1 = false;
                bool queryChanged1 = false;
                TableIsland islandForQuery3 = (TableIsland)null;
                foreach (TableField field in this.ChosenGeoFields)
                {
                    TableColumn columnInModelMetadata = this.FindTableColumnInModelMetadata(field, modelMetadata,
                        tablesWithUpdatedData, ref requery1, ref queryChanged1, ref islandForQuery3);
                    if (columnInModelMetadata != null)
                    {
                        list1.Add((TableField)columnInModelMetadata);
                        list2.Add(this.ChosenGeoMappings[index]);
                    }
                    ++index;
                }
                this.ChosenGeoFields = list1;
                this.ChosenGeoMappings = list2;
            }
            List<Tuple<TableField, AggregationFunction>> list = new List<Tuple<TableField, AggregationFunction>>();
            bool flag2 = this.Measures.Count > 0 && this.Measures[0].Item2 == AggregationFunction.None;
            foreach (Tuple<TableField, AggregationFunction> tuple in this.Measures)
            {
                bool flag3 = tuple.Item1 is TableMeasure;
                TableIsland islandForQuery3 = tableIsland;
                TableMemberDataType dataType1;
                TableMemberDataType dataType2;
                TableField tableField;
                if (!flag3)
                {
                    TableColumn columnInModelMetadata = this.FindTableColumnInModelMetadata(tuple.Item1, modelMetadata,
                        tablesWithUpdatedData, ref requery, ref queryChanged, ref islandForQuery3);
                    if (columnInModelMetadata != null)
                    {
                        dataType1 = ((TableMember)tuple.Item1).DataType;
                        dataType2 = columnInModelMetadata.DataType;
                        tableField = (TableField)columnInModelMetadata;
                    }
                    else
                        continue;
                }
                else
                {
                    TableMeasure measureInModelMetadata = this.FindTableMeasureInModelMetadata(
                        tuple.Item1 as TableMeasure, modelMetadata, tablesWithUpdatedData, ref requery, ref queryChanged,
                        ref islandForQuery3);
                    if (measureInModelMetadata != null)
                    {
                        dataType1 = ((TableMember)tuple.Item1).DataType;
                        dataType2 = measureInModelMetadata.DataType;
                        tableField = (TableField)measureInModelMetadata;
                    }
                    else
                        continue;
                }
                if (flag2)
                {
                    switch (dataType2)
                    {
                        case TableMemberDataType.Double:
                        case TableMemberDataType.Long:
                        case TableMemberDataType.Currency:
                            list.Add(new Tuple<TableField, AggregationFunction>(tableField,
                                flag3 ? AggregationFunction.UserDefined : AggregationFunction.None));
                            requery |= dataType1 != dataType2;
                            break;
                        default:
                            requery = queryChanged = true;
                            break;
                    }
                }
                else if (dataType1 == dataType2)
                {
                    list.Add(new Tuple<TableField, AggregationFunction>(tableField, tuple.Item2));
                }
                else
                {
                    requery = queryChanged = true;
                    AggregationFunction aggregationFunction = flag3 ? AggregationFunction.UserDefined :
                        (dataType2 == TableMemberDataType.Double ||
                        dataType2 == TableMemberDataType.Long ||
                        dataType2 == TableMemberDataType.Currency ?
                        tuple.Item2
                        : AggregationFunction.Count);
                    list.Add(new Tuple<TableField, AggregationFunction>(tableField, aggregationFunction));
                }
            }
            this.Measures = list;
            if (this.Filter != null)
            {
                foreach (FilterClause clause in this.Filter.FilterClauses)
                {
                    bool flag3 = clause.TableMember is TableMeasure;
                    TableIsland islandForQuery3 = tableIsland;
                    if (!flag3)
                    {
                        TableColumn columnInModelMetadata =
                            this.FindTableColumnInModelMetadata((TableField)clause.TableMember, modelMetadata,
                                tablesWithUpdatedData, ref requery, ref queryChanged, ref islandForQuery3);
                        if (!clause.UpdateTableMember((TableMember)columnInModelMetadata))
                        {
                            this.Filter.RemoveFilterClause(clause);
                            requery = queryChanged = true;

                        }
                    }
                    else
                    {
                        TableMeasure measureInModelMetadata =
                            this.FindTableMeasureInModelMetadata(clause.TableMember as TableMeasure, modelMetadata,
                                tablesWithUpdatedData, ref requery, ref queryChanged, ref islandForQuery3);
                        if (!clause.UpdateTableMember((TableMember)measureInModelMetadata))
                        {
                            this.Filter.RemoveFilterClause(clause);
                            requery = queryChanged = true;

                        }
                    }
                }
                this.Filter.ForceUpdate();
            }
            if (requery || queryChanged)
                this.FieldsChangedSinceLastQuery = true;
        }

        internal void UpdateDisplayProperties(GeoFieldWellDefinition newGeoFwd)
        {
            this.ChosenGeoFields = newGeoFwd.ChosenGeoFields;
            this.ChosenGeoMappings = newGeoFwd.ChosenGeoMappings;
            this.ChoosingGeoFields = newGeoFwd.ChoosingGeoFields;
            this.UserSelectedMapByField = newGeoFwd.UserSelectedMapByField;
        }

        internal override FieldWellDefinition.SerializableFieldWellDefinition Wrap()
        {
            GeoFieldWellDefinition.SerializableGeoFieldWellDefinition fieldWellDefinition = new GeoFieldWellDefinition.SerializableGeoFieldWellDefinition()
            {
                Geo = this.Geo == null ? (GeoField.SerializableGeoField)null : this.Geo.Wrap() as GeoField.SerializableGeoField,
                MeasureFields = Enumerable.ToList<TableField.SerializableTableField>(Enumerable.Select<Tuple<TableField, AggregationFunction>, TableField.SerializableTableField>((IEnumerable<Tuple<TableField, AggregationFunction>>)this.Measures, (Func<Tuple<TableField, AggregationFunction>, TableField.SerializableTableField>)(tuple => tuple.Item1.Wrap()))),
                MeasureAFs = Enumerable.ToList<AggregationFunction>(Enumerable.Select<Tuple<TableField, AggregationFunction>, AggregationFunction>((IEnumerable<Tuple<TableField, AggregationFunction>>)this.Measures, (Func<Tuple<TableField, AggregationFunction>, AggregationFunction>)(tuple => tuple.Item2))),
                Category = this.Category == null ? (TableField.SerializableTableField)null : this.Category.Wrap(),
                Time = this.Time == null ? (TableField.SerializableTableField)null : this.Time.Wrap(),
                ColorField = this.Color == null ? (TableField.SerializableTableField)null : this.Color.Item1.Wrap(),
                ColorAF = this.Color == null ? AggregationFunction.None : this.Color.Item2,
                ChunkBy = this.ChunkBy,
                AccumulateResultsOverTime = this.AccumulateResultsOverTime,
                Decay = this.Decay,
                DecayTimeIsNull = !this.DecayTime.HasValue,
                DecayTimeTicks = this.DecayTime.HasValue ? this.DecayTime.Value.Ticks : 0L,
                ViewModelAccumulateResultsOverTime = this.ViewModelAccumulateResultsOverTime,
                ViewModelPersistTimeData = this.ViewModelPersistTimeData,
                ChosenGeoFields = Enumerable.ToList<TableField.SerializableTableField>(Enumerable.Select<TableField, TableField.SerializableTableField>((IEnumerable<TableField>)this.ChosenGeoFields, (Func<TableField, TableField.SerializableTableField>)(field => field.Wrap()))),
                ChoosingGeoFields = this.ChoosingGeoFields,
                NotUserSelectedMapByField = !this.UserSelectedMapByField,
                UserSelectedTimeSetting = this.UserSelectedTimeSetting,
                Filter = this.Filter == null ? (Filter.SerializableFilter)null : this.Filter.Wrap()
            };
            fieldWellDefinition.ChosenGeoMappings = new List<GeoMappingType>((IEnumerable<GeoMappingType>)this.ChosenGeoMappings);
            base.Wrap((FieldWellDefinition.SerializableFieldWellDefinition)fieldWellDefinition);
            return (FieldWellDefinition.SerializableFieldWellDefinition)fieldWellDefinition;
        }

        internal void Unwrap(FieldWellDefinition.SerializableFieldWellDefinition wrappedState, CultureInfo modelCulture)
        {
            if (wrappedState == null)
                throw new ArgumentNullException("wrappedState");
            base.Unwrap(wrappedState);
            GeoFieldWellDefinition.SerializableGeoFieldWellDefinition fieldWellDefinition = wrappedState as GeoFieldWellDefinition.SerializableGeoFieldWellDefinition;
            if (fieldWellDefinition == null)
                throw new ArgumentException("wrappedState must be of type SerializableGeoFieldWellDefinition");
            if (fieldWellDefinition.MeasureFields == null)
                throw new ArgumentException("state.MeasureFields must not be null");
            if (fieldWellDefinition.MeasureAFs == null)
                throw new ArgumentException("state.MeasureAFs must not be null");
            base.Unwrap((FieldWellDefinition.SerializableFieldWellDefinition)fieldWellDefinition);
            this.Geo = fieldWellDefinition.Geo == null ? (GeoField)null : fieldWellDefinition.Geo.Unwrap() as GeoField;
            this.Measures = Enumerable.ToList<Tuple<TableField, AggregationFunction>>(Enumerable.Zip<TableField.SerializableTableField, AggregationFunction, Tuple<TableField, AggregationFunction>>((IEnumerable<TableField.SerializableTableField>)fieldWellDefinition.MeasureFields, (IEnumerable<AggregationFunction>)fieldWellDefinition.MeasureAFs, (Func<TableField.SerializableTableField, AggregationFunction, Tuple<TableField, AggregationFunction>>)((fld, afn) => new Tuple<TableField, AggregationFunction>(fld.Unwrap(), afn))));
            this.Category = fieldWellDefinition.Category == null ? (TableField)null : fieldWellDefinition.Category.Unwrap();
            this.Color = fieldWellDefinition.ColorField == null ? (Tuple<TableField, AggregationFunction>)null : new Tuple<TableField, AggregationFunction>(fieldWellDefinition.ColorField.Unwrap(), fieldWellDefinition.ColorAF);
            this.Time = fieldWellDefinition.Time == null ? (TableField)null : fieldWellDefinition.Time.Unwrap();
            this.ChunkBy = fieldWellDefinition.ChunkBy;
            this.AccumulateResultsOverTime = fieldWellDefinition.AccumulateResultsOverTime;
            this.ViewModelAccumulateResultsOverTime = fieldWellDefinition.ViewModelAccumulateResultsOverTime;
            this.ViewModelPersistTimeData = fieldWellDefinition.ViewModelPersistTimeData;
            this.Decay = fieldWellDefinition.Decay;
            this.DecayTime = fieldWellDefinition.DecayTimeIsNull ? new TimeSpan?() : new TimeSpan?(TimeSpan.FromTicks(fieldWellDefinition.DecayTimeTicks));
            this.ChoosingGeoFields = fieldWellDefinition.ChoosingGeoFields;
            this.ChosenGeoFields = Enumerable.ToList<TableField>(Enumerable.Select<TableField.SerializableTableField, TableField>((IEnumerable<TableField.SerializableTableField>)fieldWellDefinition.ChosenGeoFields, (Func<TableField.SerializableTableField, TableField>)(fld => fld.Unwrap())));
            this.ChosenGeoMappings = new List<GeoMappingType>((IEnumerable<GeoMappingType>)fieldWellDefinition.ChosenGeoMappings);
            this.UserSelectedMapByField = !fieldWellDefinition.NotUserSelectedMapByField;
            this.UserSelectedTimeSetting = fieldWellDefinition.UserSelectedTimeSetting;
            this.Filter = fieldWellDefinition.Filter == null ? new Filter() : fieldWellDefinition.Filter.Unwrap(modelCulture);
        }

        protected virtual void ValidateGeoField(GeoField field)
        {
        }

        protected virtual void ValidateMeasureField(TableField field)
        {
            if (field is GeoField)
                throw new ArgumentException("field must not be a GeoField");
        }

        protected virtual void ValidateCategoryField(TableField field)
        {
            if (field is GeoField)
                throw new ArgumentException("field must not be a GeoField");
        }

        protected virtual void ValidateTimeField(TableField field)
        {
            if (field is GeoField)
                throw new ArgumentException("field must not be a GeoField");
        }

        protected virtual void ValidateColorField(TableField field)
        {
            if (field is GeoField)
                throw new ArgumentException("field must not be a GeoField");
        }

        [Serializable]
        public enum PlaybackValueDecayType
        {
            None,
            HoldTillReplaced,
        }

        [Serializable]
        public class SerializableGeoFieldWellDefinition : FieldWellDefinition.SerializableFieldWellDefinition
        {
            [XmlElement("LatLong", typeof(LatLongField.SerializableLatLongField))]
            [XmlElement("GeoEntity", typeof(GeoEntityField.SerializableGeoEntityField))]
            [XmlElement("XY", typeof(XYField.SerializableXYField))]
            [XmlElement("GeoFullAddress", typeof(GeoFullAddressField.SerializableGeoFullAddressField))]
            public GeoField.SerializableGeoField Geo;
            [XmlArrayItem("Measure", typeof(TableColumn.SerializableTableColumn))]
            [XmlArray("Measures")]
            [XmlArrayItem("CalcFn", typeof(TableMeasure.SerializableTableMeasure))]
            public List<TableField.SerializableTableField> MeasureFields;
            public List<AggregationFunction> MeasureAFs;
            [XmlElement(typeof(TableColumn.SerializableTableColumn))]
            public TableField.SerializableTableField Category;
            [XmlElement(typeof(TableColumn.SerializableTableColumn))]
            public TableField.SerializableTableField Time;
            [XmlElement(typeof(TableColumn.SerializableTableColumn))]
            public TableField.SerializableTableField ColorField;
            public AggregationFunction ColorAF;
            [XmlArray("ChosenFields")]
            [XmlArrayItem("ChosenField", typeof(TableColumn.SerializableTableColumn))]
            public List<TableField.SerializableTableField> ChosenGeoFields;

            public TimeChunkPeriod ChunkBy { get; set; }

            [XmlAttribute("Accumulate")]
            public bool AccumulateResultsOverTime { get; set; }

            [XmlAttribute]
            public GeoFieldWellDefinition.PlaybackValueDecayType Decay { get; set; }

            [XmlAttribute]
            public bool DecayTimeIsNull { get; set; }

            [XmlAttribute]
            public long DecayTimeTicks { get; set; }

            [XmlAttribute("VMTimeAccumulate")]
            public bool ViewModelAccumulateResultsOverTime { get; set; }

            [XmlAttribute("VMTimePersist")]
            public bool ViewModelPersistTimeData { get; set; }

            public List<GeoMappingType> ChosenGeoMappings { get; set; }

            [XmlAttribute("UserNotMapBy")]
            public bool NotUserSelectedMapByField { get; set; }

            [XmlAttribute("SelTimeStg")]
            public TimeSetting UserSelectedTimeSetting { get; set; }

            [XmlAttribute]
            public bool ChoosingGeoFields { get; set; }

            public Filter.SerializableFilter Filter { get; set; }

            internal override FieldWellDefinition Unwrap(Visualization visualization, CultureInfo modelCulture)
            {
                if (!(visualization is GeoVisualization))
                    throw new ArgumentException("visualization is not a GeoVisualization");
                else
                    return (FieldWellDefinition)new GeoFieldWellDefinition(this, visualization as GeoVisualization, modelCulture);
            }
        }
    }
}
