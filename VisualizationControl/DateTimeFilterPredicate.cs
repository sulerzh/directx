using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class DateTimeFilterPredicate : FilterPredicate
    {
        public DateTimeFilterPredicateComparison Operator { get; private set; }

        public DateTime? Value { get; private set; }

        public override bool Unfiltered
        {
            get
            {
                if (this.Operator == DateTimeFilterPredicateComparison.Unknown)
                    return true;
                if (this.Operator != DateTimeFilterPredicateComparison.IsBlank && this.Operator != DateTimeFilterPredicateComparison.IsNotBlank)
                    return !this.Value.HasValue;
                else
                    return false;
            }
        }

        public override bool HasUpdatableProperties
        {
            get
            {
                return true;
            }
        }

        public DateTimeFilterPredicate(DateTimeFilterPredicateComparison op, DateTime? value)
        {
            this.Operator = op;
            this.Value = value;
        }

        internal DateTimeFilterPredicate(DateTimeFilterPredicate.SerializableDateTimeFilterPredicate state)
            : base((FilterPredicate.SerializableFilterPredicate)state)
        {
            this.Unwrap((FilterPredicate.SerializableFilterPredicate)state);
        }

        public override bool SameAs(FilterPredicate filterPredicate)
        {
            DateTimeFilterPredicate timeFilterPredicate = filterPredicate as DateTimeFilterPredicate;
            if (timeFilterPredicate == null || this.Operator != timeFilterPredicate.Operator)
                return false;
            DateTime? nullable1 = this.Value;
            DateTime? nullable2 = timeFilterPredicate.Value;
            if (nullable1.HasValue != nullable2.HasValue)
                return false;
            if (nullable1.HasValue)
                return nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault();
            else
                return true;
        }

        internal override FilterPredicateProperties UpdateProperties(ModelQueryColumn queryColumn)
        {
            DateTime? nullable = null;
            DateTime? nullable2 = null;
            if (queryColumn.Values is DateTime[])
            {
                DateTime[] values = (DateTime[])queryColumn.Values;
                if (values.Length > 0)
                {
                    nullable = new DateTime?(values[0]);
                    nullable2 = new DateTime?(values[values.Length - 1]);
                }
            }
            return new DateTimeFilterPredicateProperties { AllowedMin = nullable, AllowedMax = nullable2 };

        }

        internal override FilterPredicate.SerializableFilterPredicate Wrap()
        {
            DateTimeFilterPredicate.SerializableDateTimeFilterPredicate timeFilterPredicate = new DateTimeFilterPredicate.SerializableDateTimeFilterPredicate()
            {
                Operator = this.Operator,
                IsNull = !this.Value.HasValue,
                Value = this.Value.HasValue ? this.Value.Value : DateTime.MinValue
            };
            base.Wrap((FilterPredicate.SerializableFilterPredicate)timeFilterPredicate);
            return (FilterPredicate.SerializableFilterPredicate)timeFilterPredicate;
        }

        internal override void Unwrap(FilterPredicate.SerializableFilterPredicate wrappedState)
        {
            if (wrappedState == null)
                throw new ArgumentNullException("wrappedState");
            DateTimeFilterPredicate.SerializableDateTimeFilterPredicate timeFilterPredicate = wrappedState as DateTimeFilterPredicate.SerializableDateTimeFilterPredicate;
            if (timeFilterPredicate == null)
                throw new ArgumentException("wrappedState must be of type SerializableDateTimeFilterPredicate");
            base.Unwrap((FilterPredicate.SerializableFilterPredicate)timeFilterPredicate);
            this.Operator = timeFilterPredicate.Operator;
            this.Value = timeFilterPredicate.IsNull ? new DateTime?() : new DateTime?(timeFilterPredicate.Value);
        }

        [Serializable]
        public class SerializableDateTimeFilterPredicate : FilterPredicate.SerializableFilterPredicate
        {
            [XmlAttribute("Op")]
            public DateTimeFilterPredicateComparison Operator;
            [XmlAttribute("Null")]
            public bool IsNull;
            [XmlAttribute("Val")]
            public DateTime Value;

            internal override FilterPredicate Unwrap()
            {
                return (FilterPredicate)new DateTimeFilterPredicate(this);
            }
        }
    }
}
