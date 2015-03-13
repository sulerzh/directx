using Microsoft.Data.Visualization.VisualizationControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Data.Visualization.Client.Excel
{
    internal static class FilterExtensions
    {
        internal static readonly string[] DAXFilterPredicateOperator = new string[2]
        {
            "&&",
            "||"
        };

        internal static readonly string[] DAXDateTimeFilterPredicateComparison = new string[9]
        {
            string.Empty,
            "=",
            "<>",
            "ISBLANK",
            "NOT(ISBLANK",
            "<",
            "<=",
            ">",
            ">="
        };

        internal static readonly string[] DAXNumericFilterPredicateComparison = new string[9]
        {
            string.Empty,
            "=",
            "<>",
            "ISBLANK",
            "NOT(ISBLANK",
            "<",
            "<=",
            ">",
            ">="
        };

        internal static readonly string[] DAXStringFilterPredicateComparison = new string[9]
        {
            string.Empty,
            "SEARCH",
            "NOT(SEARCH",
            "SEARCH",
            "NOT(SEARCH",
            "=",
            "<>",
            "ISBLANK",
            "NOT(ISBLANK"
        };

        public static StringBuilder DAXFilterString(this FilterClause self, StringBuilder sb, string timeChunkingFilter)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (sb == null)
                throw new ArgumentNullException("sb");
            NumericRangeFilterClause self1 = self as NumericRangeFilterClause;
            if (self1 != null)
                return FilterExtensions.DAXFilterString(self1, sb, timeChunkingFilter);
            AndOrFilterClause self2 = self as AndOrFilterClause;
            if (self2 != null)
                return FilterExtensions.DAXFilterString(self2, sb, timeChunkingFilter);
            CategoryFilterClause<double> self3 = self as CategoryFilterClause<double>;
            if (self3 != null)
                return FilterExtensions.DAXFilterString<double>(self3, sb);
            CategoryFilterClause<string> self4 = self as CategoryFilterClause<string>;
            if (self4 != null)
                return FilterExtensions.DAXFilterString<string>(self4, sb);
            CategoryFilterClause<DateTime> self5 = self as CategoryFilterClause<DateTime>;
            if (self5 != null)
                return FilterExtensions.DAXFilterString<DateTime>(self5, sb);
            CategoryFilterClause<bool> self6 = self as CategoryFilterClause<bool>;
            if (self6 != null)
                return FilterExtensions.DAXFilterString<bool>(self6, sb);
            throw new ArgumentException("No DAX filter string");
        }

        public static StringBuilder DAXFilterString(this NumericRangeFilterClause self, StringBuilder sb, string timeChunkingFilter)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            return FilterExtensions.DAXFilterString((AndOrFilterClause)self, sb, timeChunkingFilter);
        }

        public static StringBuilder DAXFilterString(this AndOrFilterClause self, StringBuilder sb, string timeChunkingFilter)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (self.Unfiltered)
                return sb;
            FilterPredicate self1 = self.First == null || self.First.Unfiltered ? self.Second : self.First;
            FilterPredicate self2 = self1 == self.Second ? (FilterPredicate)null : (self.Second == null || self.Second.Unfiltered ? (FilterPredicate)null : self.Second);
            string aggregatedColumnName = self.AggregationFunction != AggregationFunction.None ? string.Format("CALCULATE({0}({1}){2})", (object)ExcelModelQuery.DAXAggregationFunction[(int)self.AggregationFunction], (object)TableColumnExtensions.DAXQueryName(self.TableMember), (object)(timeChunkingFilter ?? string.Empty)) : TableColumnExtensions.DAXQueryName(self.TableMember);
            sb.Append("( ");
            sb = FilterExtensions.DAXFilterString(self1, sb, aggregatedColumnName);
            if (self2 == null)
                return sb.Append(" )");
            sb.AppendFormat(" ) {0} ( ", (object)FilterExtensions.DAXFilterPredicateOperator[(int)self.Operator]);
            sb = FilterExtensions.DAXFilterString(self2, sb, aggregatedColumnName);
            return sb.Append(" )");
        }

        public static StringBuilder DAXFilterString(this FilterPredicate self, StringBuilder sb, string aggregatedColumnName)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (aggregatedColumnName == null)
                throw new ArgumentNullException("aggregatedColumnName");
            NumericFilterPredicate self1 = self as NumericFilterPredicate;
            if (self1 != null)
                return FilterExtensions.DAXFilterString(self1, sb, aggregatedColumnName);
            StringFilterPredicate self2 = self as StringFilterPredicate;
            if (self2 != null)
                return FilterExtensions.DAXFilterString(self2, sb, aggregatedColumnName);
            DateTimeFilterPredicate self3 = self as DateTimeFilterPredicate;
            if (self3 != null)
                return FilterExtensions.DAXFilterString(self3, sb, aggregatedColumnName);
            throw new ArgumentException("No DAX filter string");
        }

        public static StringBuilder DAXFilterString(this DateTimeFilterPredicate self, StringBuilder sb, string aggregatedColumnName)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (aggregatedColumnName == null)
                throw new ArgumentNullException("aggregatedColumnName");
            if (self.Unfiltered)
                return sb;
            if (self.Operator == DateTimeFilterPredicateComparison.IsBlank)
                return sb.AppendFormat("{0}({1})", (object)FilterExtensions.DAXDateTimeFilterPredicateComparison[(int)self.Operator], (object)aggregatedColumnName);
            if (self.Operator == DateTimeFilterPredicateComparison.IsNotBlank)
                return sb.AppendFormat("{0}({1}))", (object)FilterExtensions.DAXDateTimeFilterPredicateComparison[(int)self.Operator], (object)aggregatedColumnName);
            DateTime dateTime = self.Value.Value;
            return sb.AppendFormat("{0} {1} (DATE({2}, {3}, {4}) + TIME({5}, {6}, {7}))", (object)aggregatedColumnName, (object)FilterExtensions.DAXDateTimeFilterPredicateComparison[(int)self.Operator], (object)dateTime.Year, (object)dateTime.Month, (object)dateTime.Day, (object)dateTime.Hour, (object)dateTime.Minute, (object)dateTime.Second);
        }

        public static StringBuilder DAXFilterString(this NumericFilterPredicate self, StringBuilder sb, string aggregatedColumnName)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (aggregatedColumnName == null)
                throw new ArgumentNullException("aggregatedColumnName");
            if (self.Unfiltered)
                return sb;
            if (self.Operator == NumericFilterPredicateComparison.IsBlank)
                return sb.AppendFormat("{0}({1})", (object)FilterExtensions.DAXNumericFilterPredicateComparison[(int)self.Operator], (object)aggregatedColumnName);
            if (self.Operator == NumericFilterPredicateComparison.IsNotBlank)
                return sb.AppendFormat("{0}({1}))", (object)FilterExtensions.DAXNumericFilterPredicateComparison[(int)self.Operator], (object)aggregatedColumnName);
            return sb.AppendFormat("{0} {1} {2}", (object)aggregatedColumnName, (object)FilterExtensions.DAXNumericFilterPredicateComparison[(int)self.Operator], (object)Convert.ToString(self.Value.Value, (IFormatProvider)CultureInfo.InvariantCulture));
        }

        public static StringBuilder DAXFilterString(this StringFilterPredicate self, StringBuilder sb, string aggregatedColumnName)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (aggregatedColumnName == null)
                throw new ArgumentNullException("aggregatedColumnName");
            if (self.Unfiltered)
                return sb;
            switch (self.Operator)
            {
                case StringFilterPredicateComparison.Contains:
                    return sb.AppendFormat("{0}(\"{1}\", {2}, 1, -1) >= 1", (object)FilterExtensions.DAXStringFilterPredicateComparison[(int)self.Operator], (object)self.Value.Replace("\"", "\"\""), (object)aggregatedColumnName);
                case StringFilterPredicateComparison.DoesNotContain:
                    return sb.AppendFormat("{0}(\"{1}\", {2}, 1, -1) >= 1)", (object)FilterExtensions.DAXStringFilterPredicateComparison[(int)self.Operator], (object)self.Value.Replace("\"", "\"\""), (object)aggregatedColumnName);
                case StringFilterPredicateComparison.StartsWith:
                    return sb.AppendFormat("{0}(\"{1}\", {2}, 1, -1) = 1", (object)FilterExtensions.DAXStringFilterPredicateComparison[(int)self.Operator], (object)self.Value.Replace("\"", "\"\""), (object)aggregatedColumnName);
                case StringFilterPredicateComparison.DoesNotStartWith:
                    return sb.AppendFormat("{0}(\"{1}\", {2}, 1, -1) = 1)", (object)FilterExtensions.DAXStringFilterPredicateComparison[(int)self.Operator], (object)self.Value.Replace("\"", "\"\""), (object)aggregatedColumnName);
                case StringFilterPredicateComparison.Is:
                case StringFilterPredicateComparison.IsNot:
                    return sb.AppendFormat("{0} {1} \"{2}\"", (object)aggregatedColumnName, (object)FilterExtensions.DAXStringFilterPredicateComparison[(int)self.Operator], (object)self.Value.Replace("\"", "\"\""));
                case StringFilterPredicateComparison.IsBlank:
                    return sb.AppendFormat("{0}({1})", (object)FilterExtensions.DAXStringFilterPredicateComparison[(int)self.Operator], (object)aggregatedColumnName);
                case StringFilterPredicateComparison.IsNotBlank:
                    return sb.AppendFormat("{0}({1}))", (object)FilterExtensions.DAXStringFilterPredicateComparison[(int)self.Operator], (object)aggregatedColumnName);
                default:
                    throw new ArgumentException("Unsupported comparison operator: " + (object)self.Operator);
            }
        }

        public static StringBuilder DAXFilterString<T>(this CategoryFilterClause<T> self, StringBuilder sb) where T : IComparable<T>
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (self.Unfiltered)
                return sb;
            Type type = typeof(T);
            int length = sb.Length;
            string str1 = string.Format("{0}({1})", (object)ExcelModelQuery.DAXAggregationFunction[(int)self.AggregationFunction], (object)TableColumnExtensions.DAXQueryName(self.TableMember));
            if (Enumerable.Count<T>(self.SpecifiedItems) > 0)
            {
                if (type == typeof(string))
                {
                    sb.AppendFormat("(({0} = \"{1}\") && (ISBLANK({0}) = ISBLANK(\"{1}\")))", (object)str1, (object)((object)Enumerable.First<T>(self.SpecifiedItems) as string).Replace("\"", "\"\""));
                    foreach (T obj in Enumerable.Skip<T>(self.SpecifiedItems, 1))
                        sb.AppendFormat("|| (({0} = \"{1}\") && (ISBLANK({0}) = ISBLANK(\"{1}\")))", (object)str1, (object)((object)obj as string).Replace("\"", "\"\""));
                }
                else if (type == typeof(bool))
                {
                    bool flag1 = Enumerable.First<bool>(Enumerable.Cast<bool>((IEnumerable)self.SpecifiedItems));
                    if (flag1)
                        sb.AppendFormat("({0} = {1})", (object)str1, (object)flag1.ToString().ToLower(CultureInfo.InvariantCulture));
                    else
                        sb.AppendFormat("(NOT(ISBLANK({0})) && ({0} = {1}))", (object)str1, (object)flag1.ToString().ToLower(CultureInfo.InvariantCulture));
                    foreach (bool flag2 in Enumerable.Skip<bool>(Enumerable.Cast<bool>((IEnumerable)self.SpecifiedItems), 1))
                    {
                        if (flag2)
                            sb.AppendFormat(" || ({0} = {1})", (object)str1, (object)flag2.ToString().ToLower(CultureInfo.InvariantCulture));
                        else
                            sb.AppendFormat(" || (NOT(ISBLANK({0})) && ({0} = {1}))", (object)str1, (object)flag2.ToString().ToLower(CultureInfo.InvariantCulture));
                    }
                }
                else if (type == typeof(double))
                {
                    sb.AppendFormat("({0} = ({1}))", (object)str1, (object)Convert.ToString((object)Enumerable.First<T>(self.SpecifiedItems), (IFormatProvider)CultureInfo.InvariantCulture));
                    foreach (T obj in Enumerable.Skip<T>(self.SpecifiedItems, 1))
                        sb.AppendFormat(" || ({0} = ({1}))", (object)str1, (object)Convert.ToString((object)obj, (IFormatProvider)CultureInfo.InvariantCulture));
                }
                else
                {
                    IEnumerable<DateTime> source = Enumerable.Cast<DateTime>((IEnumerable)self.SpecifiedItems);
                    DateTime dateTime1 = Enumerable.First<DateTime>(source);
                    sb.AppendFormat("({0} = (DATE({1}, {2}, {3}) + TIME({4}, {5}, {6})))", (object)str1, (object)dateTime1.Year, (object)dateTime1.Month, (object)dateTime1.Day, (object)dateTime1.Hour, (object)dateTime1.Minute, (object)dateTime1.Second);
                    foreach (DateTime dateTime2 in Enumerable.Skip<DateTime>(source, 1))
                        sb.AppendFormat(" || ({0} = (DATE({1}, {2}, {3}) + TIME({4}, {5}, {6})))", (object)str1, (object)dateTime2.Year, (object)dateTime2.Month, (object)dateTime2.Day, (object)dateTime2.Hour, (object)dateTime2.Minute, (object)dateTime2.Second);
                }
            }
            if (self.BlankSpecified)
            {
                string str2 = string.Format("ISBLANK({0}){1}", (object)str1, sb.Length == length ? (object)string.Empty : (object)" || ");
                sb.Insert(length, str2);
            }
            if (self.AllSpecified)
                sb.Insert(length, "NOT ( ").Append(" )");
            return sb;
        }
    }
}
