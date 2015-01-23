using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class Filter
    {
        private object syncLock = new object();
        internal Guid Id = Guid.NewGuid();
        private List<FilterClause> filterClauses;
        private int majorVersion;

        public IEnumerable<FilterClause> FilterClauses
        {
            get
            {
                lock (this.syncLock)
                {
                    FilterClause[] local_0 = new FilterClause[this.filterClauses.Count];
                    this.filterClauses.CopyTo(local_0);
                    return (IEnumerable<FilterClause>)local_0;
                }
            }
        }

        public bool HasFilterClauses
        {
            get
            {
                lock (this.syncLock)
                    return this.filterClauses.Count > 0;
            }
        }

        public int MajorVersion
        {
            get
            {
                lock (this.syncLock)
                    return this.majorVersion;
            }
            private set
            {
                if (this.majorVersion == value)
                    return;
                this.majorVersion = value;
                this.MinorVersion = 0;
            }
        }

        public int MinorVersion { get; private set; }

        private bool IsStale
        {
            get
            {
                if (this.HasFilterClauses)
                    return this.MajorVersion < Enumerable.Max<FilterClause>((IEnumerable<FilterClause>)this.filterClauses, (Func<FilterClause, int>)(fc => fc.RequestedVersion));
                else
                    return true;
            }
        }

        public Filter()
        {
            this.filterClauses = new List<FilterClause>();
            this.MajorVersion = 0;
            this.MinorVersion = 0;
        }

        internal Filter(Filter.SerializableFilter state, CultureInfo modelCulture)
        {
            this.MajorVersion = 0;
            this.MinorVersion = 0;
            this.Unwrap(state, modelCulture);
        }

        public bool AddFilterClause(FilterClause filterClause)
        {
            if (filterClause == null)
                return false;
            lock (this.syncLock)
            {
                if (this.filterClauses.Contains(filterClause))
                    return false;
                if (filterClause.Unfiltered)
                {
                    ++this.MinorVersion;
                    filterClause.SetUpdatePending(this.MajorVersion);
                }
                else
                {
                    ++this.MajorVersion;
                    filterClause.UpdateVersion(this.MajorVersion);
                    this.filterClauses.ForEach((Action<FilterClause>)(fc => fc.SetUpdatePending(this.MajorVersion)));
                }
                this.filterClauses.Add(filterClause);
                return true;
            }
        }

        public bool ReplaceFilterClause(FilterClause filterClause, FilterClause newFilterClause)
        {
            if (object.ReferenceEquals((object)filterClause, (object)newFilterClause))
                return false;
            lock (this.syncLock)
            {
                if (this.filterClauses.Contains(newFilterClause))
                    return false;
                int local_0 = this.filterClauses.FindIndex((Predicate<FilterClause>)(fc => fc == filterClause));
                if (local_0 >= 0)
                {
                    bool local_1 = !this.filterClauses[local_0].Unfiltered || !newFilterClause.Unfiltered;
                    this.filterClauses[local_0] = newFilterClause;
                    if (local_1)
                    {
                        ++this.MajorVersion;
                        this.filterClauses.ForEach((Action<FilterClause>)(fc =>
                        {
                            if (fc == newFilterClause)
                                fc.UpdateVersion(this.MajorVersion);
                            else
                                fc.SetUpdatePending(this.MajorVersion);
                        }));
                    }
                    else
                    {
                        ++this.MinorVersion;
                        newFilterClause.UpdateVersion(this.MajorVersion);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool RemoveFilterClause(FilterClause filterClause)
        {
            if (filterClause != null)
            {
                lock (this.syncLock)
                {
                    if (this.filterClauses.Remove(filterClause))
                    {
                        if (!filterClause.Unfiltered)
                        {
                            ++this.MajorVersion;
                            this.filterClauses.ForEach((Action<FilterClause>)(fc => fc.SetUpdatePending(this.MajorVersion)));
                        }
                        else
                            ++this.MinorVersion;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool RemoveAllFilterClauses()
        {
            lock (this.syncLock)
            {
                if (this.HasFilterClauses)
                {
                    if (Enumerable.Any<FilterClause>((IEnumerable<FilterClause>)this.filterClauses, (Func<FilterClause, bool>)(fc => !fc.Unfiltered)))
                        ++this.MajorVersion;
                    else
                        ++this.MinorVersion;
                    this.filterClauses.Clear();
                    return true;
                }
            }
            return false;
        }

        public bool SetFilterClausesFrom(Filter filter)
        {
            lock (this.syncLock)
            {
                lock ((filter ?? this).syncLock)
                {
                    if (this.SameAs(filter))
                        return false;
                    this.RemoveAllFilterClauses();
                    if (filter != null)
                    {
                        this.Id = filter.Id;
                        this.MajorVersion = filter.MajorVersion;
                        this.MinorVersion = filter.MinorVersion;
                        filter.filterClauses.ForEach((Action<FilterClause>)(fc => this.filterClauses.Add(fc)));
                    }
                    return true;
                }
            }
        }

        public bool FilteredEffectSame(Filter otherFilter)
        {
            if (otherFilter == null)
            {
                lock (this.syncLock)
                    return Enumerable.Any<FilterClause>((IEnumerable<FilterClause>)this.filterClauses, (Func<FilterClause, bool>)(fc => !fc.Unfiltered));
            }
            else
            {
                lock (this.syncLock)
                    return this.Id == otherFilter.Id && this.MajorVersion == otherFilter.MajorVersion;
            }
        }

        internal List<Tuple<FilterClause, Filter>> GetFiltersForPropertyQueries()
        {
            lock (this.syncLock)
            {
                if (!this.HasFilterClauses || this.IsStale)
                    return new List<Tuple<FilterClause, Filter>>(0);
                IEnumerable<FilterClause> local_0 = Enumerable.Where<FilterClause>((IEnumerable<FilterClause>)this.filterClauses, (Func<FilterClause, bool>)(fc =>
                {
                    if (fc.HasUpdatableProperties)
                        return fc.CurrentVersion != fc.RequestedVersion;
                    else
                        return false;
                }));
                List<Tuple<FilterClause, Filter>> local_1 = new List<Tuple<FilterClause, Filter>>(Enumerable.Count<FilterClause>(local_0));
                foreach (FilterClause item_0 in local_0)
                {
                    Filter local_3 = new Filter();
                    local_3.SetFilterClausesFrom(this);
                    local_3.filterClauses.Remove(item_0);
                    local_1.Add(new Tuple<FilterClause, Filter>(item_0, local_3));
                }
                return local_1;
            }
        }

        private bool SameAs(Filter otherFilter)
        {
            if (otherFilter == null)
                return !this.HasFilterClauses;
            if (this.Id == otherFilter.Id && this.MajorVersion == otherFilter.MajorVersion)
                return this.MinorVersion == otherFilter.MinorVersion;
            else
                return false;
        }

        internal void ForceUpdate()
        {
            lock (this.syncLock)
                this.filterClauses.ForEach((Action<FilterClause>)(fc => fc.ForceUpdate()));
        }

        internal Filter.SerializableFilter Wrap()
        {
            lock (this.syncLock)
                return new Filter.SerializableFilter()
                {
                    Clauses = Enumerable.ToList<FilterClause.SerializableFilterClause>(Enumerable.Select<FilterClause, FilterClause.SerializableFilterClause>((IEnumerable<FilterClause>)this.filterClauses, (Func<FilterClause, FilterClause.SerializableFilterClause>)(fc => fc.Wrap())))
                };
        }

        internal void Unwrap(Filter.SerializableFilter state, CultureInfo modelCulture)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            this.filterClauses = Enumerable.ToList<FilterClause>(Enumerable.Select<FilterClause.SerializableFilterClause, FilterClause>((IEnumerable<FilterClause.SerializableFilterClause>)state.Clauses, (Func<FilterClause.SerializableFilterClause, FilterClause>)(serializedFilter => serializedFilter.Unwrap(modelCulture))));
            ++this.MajorVersion;
            this.filterClauses.ForEach((Action<FilterClause>)(fc => fc.SetUpdatePending(this.MajorVersion)));
        }

        [Serializable]
        public class SerializableFilter
        {
            [XmlArrayItem("CFCDbl", typeof(CategoryFilterClause<double>.SerializableCategoryFilterClause))]
            [XmlArrayItem("CFCBool", typeof(CategoryFilterClause<bool>.SerializableCategoryFilterClause))]
            [XmlArrayItem("AOFC", typeof(AndOrFilterClause.SerializableAndOrFilterClause))]
            [XmlArrayItem("CFCDateTime", typeof(CategoryFilterClause<DateTime>.SerializableCategoryFilterClause))]
            [XmlArrayItem("NRFC", typeof(NumericRangeFilterClause.SerializableNumericRangeFilterClause))]
            [XmlArray("FCs")]
            [XmlArrayItem("CFCStr", typeof(CategoryFilterClause<string>.SerializableCategoryFilterClause))]
            public List<FilterClause.SerializableFilterClause> Clauses;

            internal Filter Unwrap(CultureInfo modelCulture)
            {
                return new Filter(this, modelCulture);
            }
        }
    }
}
