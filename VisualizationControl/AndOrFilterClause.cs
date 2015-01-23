using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class AndOrFilterClause : FilterClause
    {
        public FilterPredicate First { get; protected set; }

        public FilterPredicate Second { get; protected set; }

        public FilterPredicateOperator Operator { get; protected set; }

        public override bool Unfiltered
        {
            get
            {
                if (this.First != null && !this.First.Unfiltered)
                    return false;
                if (this.Second != null)
                    return this.Second.Unfiltered;
                else
                    return true;
            }
        }

        public override bool HasUpdatableProperties
        {
            get
            {
                FilterPredicate filterPredicate = this.First ?? this.Second;
                if (filterPredicate != null)
                    return filterPredicate.HasUpdatableProperties;
                else
                    return false;
            }
        }

        protected FilterPredicateProperties FilterProperties { get; private set; }

        public AndOrFilterClause(TableMember column, AggregationFunction afn, FilterPredicateOperator predicateOperator, FilterPredicate first, FilterPredicate second)
            : base(column, afn)
        {
            if (first != null && second != null && first.GetType() != second.GetType())
                throw new ArgumentException("Both predicates must be of the same type");
            this.ValidatePredicateType(first ?? second, column, afn);
            this.First = first;
            this.Second = second;
            this.Operator = predicateOperator;
        }

        public AndOrFilterClause(AndOrFilterClause oldFilterClause, FilterPredicateOperator predicateOperator, FilterPredicate first, FilterPredicate second)
            : base((FilterClause)oldFilterClause)
        {
            if (first != null && second != null && first.GetType() != second.GetType())
                throw new ArgumentException("Both predicates must be of the same type");
            this.ValidatePredicateType(first ?? second, oldFilterClause.TableMember, oldFilterClause.AggregationFunction);
            this.First = first;
            this.Second = second;
            this.Operator = predicateOperator;
            this.FilterProperties = oldFilterClause.FilterProperties;
        }

        internal AndOrFilterClause(FilterClause.SerializableFilterClause state)
            : base(state)
        {
            this.Unwrap(state);
        }

        public bool TryGetProperties(out FilterPredicateProperties properties)
        {
            properties = this.FilterProperties;
            return this.FilterProperties != null;
        }

        internal bool UpdatePropertiesInBase(int queryVersion, ModelQueryColumn queryColumn)
        {
            return base.UpdateProperties(queryVersion, queryColumn);
        }

        internal override bool UpdateProperties(int queryVersion, ModelQueryColumn queryColumn)
        {
            bool flag = base.UpdateProperties(queryVersion, queryColumn) && (queryColumn != null);
            if (flag && queryColumn.Values != null)
            {
                if (this.HasUpdatableProperties)
                {
                    FilterPredicate predicate = this.First ?? this.Second;
                    if (predicate != null)
                    {
                        this.FilterProperties = predicate.UpdateProperties(queryColumn);
                    }
                    base.DispatchPropertiesUpdated();
                    return true;
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "UpdateProperties(): returning false for AndOrFilterClause (table column = {0}, Agg Fn = {1}, queryVersion={2}, RequestedVersion={3}) - HasUpdatableProperties = false", new object[] { base.TableMember.Name, base.AggregationFunction, queryVersion, base.CurrentVersion });
            }
            return false;

        }

        internal override FilterClause.SerializableFilterClause Wrap()
        {
            AndOrFilterClause.SerializableAndOrFilterClause andOrFilterClause = new AndOrFilterClause.SerializableAndOrFilterClause();
            this.Wrap((FilterClause.SerializableFilterClause)andOrFilterClause);
            return (FilterClause.SerializableFilterClause)andOrFilterClause;
        }

        protected override void Wrap(FilterClause.SerializableFilterClause wrappedState)
        {
            if (wrappedState == null)
                throw new ArgumentNullException("wrappedState");
            AndOrFilterClause.SerializableAndOrFilterClause andOrFilterClause = wrappedState as AndOrFilterClause.SerializableAndOrFilterClause;
            if (andOrFilterClause == null)
                throw new ArgumentException("wrappedState must be of type SerializableAndOrFilterClause");
            andOrFilterClause.Operator = this.Operator;
            andOrFilterClause.First = this.First == null ? (FilterPredicate.SerializableFilterPredicate)null : this.First.Wrap();
            andOrFilterClause.Second = this.Second == null ? (FilterPredicate.SerializableFilterPredicate)null : this.Second.Wrap();
            base.Wrap((FilterClause.SerializableFilterClause)andOrFilterClause);
        }

        internal override void Unwrap(FilterClause.SerializableFilterClause wrappedState)
        {
            if (wrappedState == null)
                throw new ArgumentNullException("wrappedState");
            AndOrFilterClause.SerializableAndOrFilterClause andOrFilterClause = wrappedState as AndOrFilterClause.SerializableAndOrFilterClause;
            if (andOrFilterClause == null)
                throw new ArgumentException("wrappedState must be of type AndOrFilterClause");
            base.Unwrap((FilterClause.SerializableFilterClause)andOrFilterClause);
            this.Operator = andOrFilterClause.Operator;
            this.First = andOrFilterClause.First == null ? (FilterPredicate)null : andOrFilterClause.First.Unwrap();
            this.Second = andOrFilterClause.Second == null ? (FilterPredicate)null : andOrFilterClause.Second.Unwrap();
        }

        protected virtual void ValidatePredicateType(FilterPredicate predicateToTest, TableMember column, AggregationFunction afn)
        {
            if (predicateToTest == null)
                return;
            switch (column.DataType)
            {
                case TableMemberDataType.String:
                    if (afn == AggregationFunction.None && !(predicateToTest is StringFilterPredicate))
                        throw new ArgumentException("With AggregationFunction.None, the predicate must be a String predicate");
                    if (afn == AggregationFunction.None || predicateToTest is NumericFilterPredicate)
                        break;
                    else
                        throw new ArgumentException("With AggregationFunction != None, the predicate must be a Numeric predicate");
                case TableMemberDataType.Bool:
                    if (afn == AggregationFunction.None)
                        throw new ArgumentException("AggregationFunction = None is not supported for Bool");
                    if (predicateToTest is NumericFilterPredicate)
                        break;
                    else
                        throw new ArgumentException("With AggregationFunction != None, the predicate must be a Numeric predicate");
                case TableMemberDataType.Double:
                case TableMemberDataType.Long:
                case TableMemberDataType.Currency:
                    if (predicateToTest is NumericFilterPredicate)
                        break;
                    else
                        throw new ArgumentException("The predicate must be a Numeric predicate");
                case TableMemberDataType.DateTime:
                    if (afn == AggregationFunction.None && !(predicateToTest is DateTimeFilterPredicate))
                        throw new ArgumentException("With AggregationFunction.None, the predicate must be a DateTime predicate");
                    if (afn == AggregationFunction.None || predicateToTest is NumericFilterPredicate)
                        break;
                    else
                        throw new ArgumentException("With AggregationFunction != None, the predicate must be a Numeric predicate");
                default:
                    throw new ArgumentException("Unsupported table column data type " + ((object)column.DataType).ToString());
            }
        }

        [Serializable]
        public class SerializableAndOrFilterClause : FilterClause.SerializableFilterClause
        {
            [XmlAttribute("Op")]
            public FilterPredicateOperator Operator;
            [XmlElement("FirstNum", typeof(NumericFilterPredicate.SerializableNumericFilterPredicate))]
            [XmlElement("FirstDT", typeof(DateTimeFilterPredicate.SerializableDateTimeFilterPredicate))]
            [XmlElement("FirstStr", typeof(StringFilterPredicate.SerializableStringFilterPredicate))]
            public FilterPredicate.SerializableFilterPredicate First;
            [XmlElement("SecondDT", typeof(DateTimeFilterPredicate.SerializableDateTimeFilterPredicate))]
            [XmlElement("SecondNum", typeof(NumericFilterPredicate.SerializableNumericFilterPredicate))]
            [XmlElement("SecondStr", typeof(StringFilterPredicate.SerializableStringFilterPredicate))]
            public FilterPredicate.SerializableFilterPredicate Second;

            internal override FilterClause Unwrap(CultureInfo modelCulture)
            {
                return (FilterClause)new AndOrFilterClause((FilterClause.SerializableFilterClause)this);
            }
        }
    }
}
