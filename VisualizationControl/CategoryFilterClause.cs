using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class CategoryFilterClause<T> : FilterClause where T : IComparable<T>
    {
        private IEqualityComparer<T> equalityComparer;
        private CultureInfo modelCulture;
        private IComparer<T> comparer;
        private Func<T, bool> isBlank;
        private T[] selectables;
        private bool blankIsASelectableItem;
        private List<T> specifiedItems;

        public IEnumerable<T> SpecifiedItems
        {
            get
            {
                return this.specifiedItems;
            }
        }

        public bool AllSpecified { get; private set; }

        public bool BlankSpecified { get; private set; }

        public override bool Unfiltered
        {
            get
            {
                if (this.specifiedItems.Count == 0)
                    return !this.BlankSpecified;
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

        public CategoryFilterClause(TableMember tableMember, CultureInfo modelCulture, IEnumerable<T> specifiedItems = null, bool blankSpecified = false, bool allSpecified = false)
            : base(tableMember, AggregationFunction.None)
        {
            Type type = typeof(T);
            if (modelCulture == null)
                throw new ArgumentNullException("modelCulture");
            if (tableMember is TableMeasure)
                throw new ArgumentNullException("TableMeasures may not be used in category filters");
            if (!(type == typeof(string)) && !(type == typeof(double)) && (!(type == typeof(bool)) && !(type == typeof(DateTime))))
                throw new ArgumentException("Unsupported type: " + this.GetType().FullName);
            this.equalityComparer = EqualityComparer<T>.Default;
            this.comparer = Comparer<T>.Default;
            this.modelCulture = modelCulture;
            switch (tableMember.DataType)
            {
                case TableMemberDataType.String:
                    if (type != typeof(string))
                        throw new ArgumentException(type.FullName + "can not be used with TableMember data type " + ((object)tableMember.DataType).ToString());
                    this.comparer = StringComparer.Create(this.modelCulture, true) as IComparer<T>;
                    this.equalityComparer = this.comparer as IEqualityComparer<T>;
                    this.isBlank = val => (object)val == null;
                    break;
                case TableMemberDataType.Bool:
                    if (type != typeof(bool))
                        throw new ArgumentException(type.FullName + "can not be used with TableMember data type " + ((object)tableMember.DataType).ToString());
                    this.comparer = Comparer<T>.Create((x, y) => -Comparer<T>.Default.Compare(x, y));
                    this.isBlank = val => false;
                    break;
                case TableMemberDataType.Double:
                case TableMemberDataType.Long:
                case TableMemberDataType.Currency:
                    if (!(type == typeof(float)) && !(type == typeof(double)) && (!(type == typeof(long)) && !(type == typeof(short))) && (!(type == typeof(int)) && !(type == typeof(byte)) && (!(type == typeof(ulong)) && !(type == typeof(ushort)))) && !(type == typeof(uint)))
                        throw new ArgumentException(type.FullName + "can not be used with TableMember data type " + ((object)tableMember.DataType).ToString());
                    this.isBlank = val => double.IsNaN(Convert.ToDouble(val));
                    break;
                case TableMemberDataType.DateTime:
                    if (type != typeof(DateTime))
                        throw new ArgumentException(type.FullName + "can not be used with TableMember data type " + ((object)tableMember.DataType).ToString());
                    this.isBlank = val => Convert.ToDateTime(val) == ModelQuery.UnknownDateTime;
                    break;
                default:
                    throw new ArgumentException("Unsupported table column data type " + ((object)tableMember.DataType).ToString());
            }
            if (specifiedItems == null)
                specifiedItems = Enumerable.Empty<T>();
            this.specifiedItems = new List<T>(specifiedItems);
            for (int index = 0; index < this.specifiedItems.Count - 1; ++index)
            {
                if (this.comparer.Compare(this.specifiedItems[index], this.specifiedItems[index + 1]) >= 0)
                    throw new ArgumentException("specifiedItems must be sorted and distinct: Value for index " + (object)index.ToString() + " is '" + (string)(object)this.specifiedItems[index] + "', value for next index is '" + (string)(object)this.specifiedItems[index + 1] + "'");
            }
            this.BlankSpecified = blankSpecified;
            this.AllSpecified = allSpecified;
            this.selectables = null;
            this.blankIsASelectableItem = false;
        }

        public CategoryFilterClause(CategoryFilterClause<T> oldFilterClause, IEnumerable<T> specifiedItems = null, bool blankSpecified = false, bool allSpecified = false)
            : base(oldFilterClause)
        {
            this.equalityComparer = oldFilterClause.equalityComparer;
            this.comparer = oldFilterClause.comparer;
            this.modelCulture = oldFilterClause.modelCulture;
            this.isBlank = oldFilterClause.isBlank;
            if (specifiedItems == null)
                specifiedItems = Enumerable.Empty<T>();
            this.specifiedItems = new List<T>(specifiedItems);
            for (int index = 0; index < this.specifiedItems.Count - 1; ++index)
            {
                if (this.comparer.Compare(this.specifiedItems[index], this.specifiedItems[index + 1]) >= 0)
                    throw new ArgumentException("specifiedItems must be sorted and distinct: Value for index " + (object)index.ToString() + " is '" + (string)(object)this.specifiedItems[index] + "', value for next index is '" + (string)(object)this.specifiedItems[index + 1] + "'");
            }
            this.BlankSpecified = blankSpecified;
            this.AllSpecified = allSpecified;
            this.selectables = null;
            this.blankIsASelectableItem = false;
            if (oldFilterClause.selectables == null)
                return;
            this.selectables = oldFilterClause.selectables;
            this.blankIsASelectableItem = oldFilterClause.blankIsASelectableItem;
        }

        internal CategoryFilterClause(FilterClause.SerializableFilterClause state, CultureInfo modelCulture)
            : base(state)
        {
            this.modelCulture = modelCulture;
            Type type = typeof(T);
            if (type == typeof(string))
            {
                this.comparer = StringComparer.Create(this.modelCulture, true) as IComparer<T>;
                this.equalityComparer = this.comparer as IEqualityComparer<T>;
                this.isBlank = val => (object)val == null;
            }
            else if (type == typeof(bool))
            {
                this.equalityComparer = EqualityComparer<T>.Default;
                this.comparer = Comparer<T>.Create((x, y) => -Comparer<T>.Default.Compare(x, y));
                this.isBlank = val => false;
            }
            else if (type == typeof(DateTime))
            {
                this.equalityComparer = EqualityComparer<T>.Default;
                this.comparer = Comparer<T>.Default;
                this.isBlank = val => Convert.ToDateTime(val) == ModelQuery.UnknownDateTime;
            }
            else
            {
                if (!(type == typeof(double)))
                    throw new ArgumentException("Unexpected type: " + type.Name);
                this.equalityComparer = EqualityComparer<T>.Default;
                this.comparer = Comparer<T>.Default;
                this.isBlank = val => double.IsNaN(Convert.ToDouble(val));
            }
            this.Unwrap(state, modelCulture);
        }

        public bool TryGetProperties(out IEnumerable<T> selectableItems, out BitArray selectedIndices, out bool showBlank, out bool blankSpecified, out bool allSpecified)
        {
            if (this.selectables == null)
            {
                selectableItems = null;
                selectedIndices = new BitArray(0);
                showBlank = false;
                allSpecified = false;
                blankSpecified = false;
                return false;
            }
            else
            {
                selectableItems = this.Merge();
                showBlank = this.blankIsASelectableItem || this.BlankSpecified;
                blankSpecified = this.BlankSpecified;
                allSpecified = this.AllSpecified;
                int length = selectableItems.Count();
                selectedIndices = new BitArray(length);
                int index = 0;
                foreach (T obj in selectableItems)
                {
                    bool flag = this.SpecifiedItems.Contains(obj, this.equalityComparer);
                    selectedIndices[index] = allSpecified ? !flag : flag;
                    ++index;
                }
                return true;
            }
        }

        private IEnumerable<T> Merge()
        {
            int selectableItemsIndex = 0;
            int specifiedItemsIndex = 0;
            int selectableItemsCount = this.selectables.Length;
            int specifiedItemsCount = this.specifiedItems.Count;
            while (selectableItemsIndex < selectableItemsCount && specifiedItemsIndex < specifiedItemsCount)
            {
                if (this.isBlank(this.selectables[selectableItemsIndex]))
                    ++selectableItemsIndex;
                else if (selectableItemsIndex < selectableItemsCount - 1 && this.equalityComparer.Equals(this.selectables[selectableItemsIndex], this.selectables[selectableItemsIndex + 1]))
                {
                    ++selectableItemsIndex;
                }
                else
                {
                    int retval = this.comparer.Compare(this.selectables[selectableItemsIndex], this.specifiedItems[specifiedItemsIndex]);
                    if (retval == 0)
                    {
                        yield return this.selectables[selectableItemsIndex];
                        ++selectableItemsIndex;
                        ++specifiedItemsIndex;
                    }
                    else if (retval < 0)
                    {
                        yield return this.selectables[selectableItemsIndex];
                        ++selectableItemsIndex;
                    }
                    else
                    {
                        yield return this.specifiedItems[specifiedItemsIndex];
                        ++specifiedItemsIndex;
                    }
                }
            }
            while (selectableItemsIndex < selectableItemsCount)
            {
                if (this.isBlank(this.selectables[selectableItemsIndex]))
                    ++selectableItemsIndex;
                else if (selectableItemsIndex < selectableItemsCount - 1 && this.equalityComparer.Equals(this.selectables[selectableItemsIndex], this.selectables[selectableItemsIndex + 1]))
                {
                    ++selectableItemsIndex;
                }
                else
                {
                    yield return this.selectables[selectableItemsIndex];
                    ++selectableItemsIndex;
                }
            }
            for (; specifiedItemsIndex < specifiedItemsCount; ++specifiedItemsIndex)
                yield return this.specifiedItems[specifiedItemsIndex];
        }

        internal override bool UpdateProperties(int queryVersion, ModelQueryColumn queryColumn)
        {
            bool flag1 = base.UpdateProperties(queryVersion, queryColumn) && queryColumn != null;

            if (flag1 && queryColumn.Values != null)
            {
                Type type = typeof(T);
                if (!(type == typeof(bool)) ? queryColumn.Values is T[] : queryColumn.Values is bool?[])
                {
                    if (type == typeof(string))
                    {
                        this.selectables = (T[])queryColumn.Values;
                        this.blankIsASelectableItem = this.selectables.Any<T>(this.isBlank);
                    }
                    else if (type == typeof(double))
                    {
                        this.selectables = (T[])queryColumn.Values;
                        this.blankIsASelectableItem = this.selectables.Any<T>(this.isBlank);
                    }
                    else if (type == typeof(DateTime))
                    {
                        this.selectables = (T[])queryColumn.Values;
                        this.blankIsASelectableItem = this.selectables.Any<T>(this.isBlank);
                    }
                    else
                    {
                        if (!(type == typeof(bool)))
                            return false;

                        bool haveTrueItems = ((bool?[])queryColumn.Values).Contains<bool?>(true);
                        bool haveFalseItems = ((bool?[])queryColumn.Values).Contains<bool?>(false);
                        bool[] flagArray = new bool[(haveTrueItems ? 1 : 0) + (haveFalseItems ? 1 : 0)];
                        int num1 = 0;
                        if (haveTrueItems)
                            flagArray[num1++] = true;
                        if (haveFalseItems)
                            flagArray[num1++] = false;

                        this.blankIsASelectableItem = ((bool?[])queryColumn.Values).Contains<bool?>(null);
                        this.selectables = flagArray as T[];
                    }
                    this.DispatchPropertiesUpdated();
                    return true;
                }

                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "UpdateProperties(): returning false for AndOrFilterClause (table column = {0}, Agg Fn = {1}, queryVersion={2}, RequestedVersion={3}) - rightType = false, queryColumn.Values.Type={4}, type = {5}", base.TableMember.Name, base.AggregationFunction, queryVersion, base.CurrentVersion, queryColumn.Values.GetType().Name, typeof(T).Name);
            }
            return false;
        }

        internal override FilterClause.SerializableFilterClause Wrap()
        {
            CategoryFilterClause<T>.SerializableCategoryFilterClause categoryFilterClause = new CategoryFilterClause<T>.SerializableCategoryFilterClause()
            {
                SpecifiedItems = this.specifiedItems,
                AllSpecified = this.AllSpecified,
                BlankSpecified = this.BlankSpecified
            };
            base.Wrap(categoryFilterClause);
            return categoryFilterClause;
        }

        internal void Unwrap(FilterClause.SerializableFilterClause wrappedState, CultureInfo modelCulture)
        {
            if (wrappedState == null)
                throw new ArgumentNullException("wrappedState");
            CategoryFilterClause<T>.SerializableCategoryFilterClause categoryFilterClause = wrappedState as CategoryFilterClause<T>.SerializableCategoryFilterClause;
            if (categoryFilterClause == null)
                throw new ArgumentException("wrappedState must be of type SerializableCategoryFilterClause");
            base.Unwrap(categoryFilterClause);
            this.specifiedItems = categoryFilterClause.SpecifiedItems;
            for (int index = 0; index < this.specifiedItems.Count - 1; ++index)
            {
                if (this.comparer.Compare(this.specifiedItems[index], this.specifiedItems[index + 1]) >= 0)
                {
                    this.specifiedItems = categoryFilterClause.SpecifiedItems.Distinct(this.equalityComparer).OrderBy(val => val, this.comparer).ToList();
                    break;
                }
            }
            this.AllSpecified = categoryFilterClause.AllSpecified;
            this.BlankSpecified = categoryFilterClause.BlankSpecified;
            this.selectables = null;
            this.blankIsASelectableItem = false;
        }

        [Serializable]
        public class SerializableCategoryFilterClause : FilterClause.SerializableFilterClause
        {
            [XmlArray("Is")]
            [XmlArrayItem("I")]
            public List<T> SpecifiedItems;
            [XmlAttribute("AllSpecified")]
            public bool AllSpecified;
            [XmlAttribute("BlankSpecified")]
            public bool BlankSpecified;

            internal override FilterClause Unwrap(CultureInfo modelCulture)
            {
                if (this.SpecifiedItems == null)
                    this.SpecifiedItems = new List<T>();
                return new CategoryFilterClause<T>(this, modelCulture);
            }
        }
    }
}
