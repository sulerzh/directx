using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class GeoModelDataId
    {
        private static readonly Regex GeoModelDataIdV1RCRegex = new Regex(V1RCRegExString, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex GeoModelDataIdAltMapsRegex = new Regex(V1AltMapsRegExString, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex GeoModelDataIdOct2014Regex = new Regex(V1Oct2014RegExString, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly CultureInfo CultureInId = CultureInfo.InvariantCulture;
        private StringBuilder stringBuilder = new StringBuilder();
        private const string StringTerminator = "#";
        private const string LatColRegExGroup = "Lat";
        private const string LatValRegExGroup = "LatVal";
        private const string LonColRegExGroup = "Lon";
        private const string LonValRegExGroup = "LonVal";
        private const string XCoordColRegExGroup = "XCoord";
        private const string XCoordValRegExGroup = "XCoordVal";
        private const string YCoordColRegExGroup = "YCoord";
        private const string YCoordValRegExGroup = "YCoordVal";
        private const string AddrColRegExGroup = "Addr";
        private const string AddrValRegExGroup = "AddrVal";
        private const string AdColRegExGroup = "Ad";
        private const string AdValRegExGroup = "AdVal";
        private const string Ad2ColRegExGroup = "Ad2";
        private const string Ad2ValRegExGroup = "Ad2Val";
        private const string CountryColRegExGroup = "Country";
        private const string CountryValRegExGroup = "CountryVal";
        private const string LocColRegExGroup = "Loc";
        private const string LocValRegExGroup = "LocVal";
        private const string ZipColRegExGroup = "Zip";
        private const string ZipValRegExGroup = "ZipVal";
        private const string FullAddrColRegExGroup = "FullAddr";
        private const string FullAddrValRegExGroup = "FullAddrVal";
        private const string OldColRegExGroup = "Old";
        private const string OldValRegExGroup = "OldVal";
        private const string CatColRegExGroup = "Cat";
        private const string CatValRegExGroup = "CatVal";
        private const string MsrColRegExGroup = "Msr";
        private const string MsrAFRegExGroup = "MsrAF";
        private const string MsrValRegExGroup = "MsrVal";
        private const string MsrCalcFnColRegExGroup = "MsrCalcFn";
        private const string AnyMeasureRegExGroup = "AnyMeas";
        private const string AnyCatValRegExGroup = "AnyCatVal";
        private const string V1PublicBetaRegExString = "^Lat(?<Lat>(.{0}|(\\'.*\\'\\[.*\\])))LatVal(?<LatVal>.*)Lon(?<Lon>(.{0}|(\\'.*\\'\\[.*\\])))LonVal(?<LonVal>.*)Addr(?<Addr>(.{0}|(\\'.*\\'\\[.*\\])))AddrVal(?<AddrVal>.*)Ad(?<Ad>(.{0}|(\\'.*\\'\\[.*\\])))AdVal(?<AdVal>.*)Ad2(?<Ad2>(.{0}|(\\'.*\\'\\[.*\\])))Ad2Val(?<Ad2Val>.*)Country(?<Country>(.{0}|(\\'.*\\'\\[.*\\])))CountryVal(?<CountryVal>.*)Loc(?<Loc>(.{0}|(\\'.*\\'\\[.*\\])))LocVal(?<LocVal>.*)Zip(?<Zip>(.{0}|(\\'.*\\'\\[.*\\])))ZipVal(?<ZipVal>.*)FullAddr(?<FullAddr>(.{0}|(\\'.*\\'\\[.*\\])))FullAddrVal(?<FullAddrVal>.*)Old(?<Old>(.{0}|(\\'.*\\'\\[.*\\])))OldVal(?<OldVal>.*)Cat(?<Cat>(.{0}|(\\'.*\\'\\[.*\\])))CatVal(?<CatVal>.*)Msr(?<Msr>(.{0}|(\\'.*\\'\\[.*\\])))MsrAF(?<MsrAF>.*)MsrVal(?<MsrVal>.*)";
        private const string V1RCRegExString = "^Lat(?<Lat>(.{0}|(\\'.*\\'\\[.*\\])))LatVal(?<LatVal>.*)Lon(?<Lon>(.{0}|(\\'.*\\'\\[.*\\])))LonVal(?<LonVal>.*)Addr(?<Addr>(.{0}|(\\'.*\\'\\[.*\\])))AddrVal(?<AddrVal>.*)Ad(?<Ad>(.{0}|(\\'.*\\'\\[.*\\])))AdVal(?<AdVal>.*)Ad2(?<Ad2>(.{0}|(\\'.*\\'\\[.*\\])))Ad2Val(?<Ad2Val>.*)Country(?<Country>(.{0}|(\\'.*\\'\\[.*\\])))CountryVal(?<CountryVal>.*)Loc(?<Loc>(.{0}|(\\'.*\\'\\[.*\\])))LocVal(?<LocVal>.*)Zip(?<Zip>(.{0}|(\\'.*\\'\\[.*\\])))ZipVal(?<ZipVal>.*)FullAddr(?<FullAddr>(.{0}|(\\'.*\\'\\[.*\\])))FullAddrVal(?<FullAddrVal>.*)Old(?<Old>(.{0}|(\\'.*\\'\\[.*\\])))OldVal(?<OldVal>.*)Cat(?<Cat>(.{0}|(\\'.*\\'\\[.*\\])))CatVal(?<CatVal>.*)Msr(?<Msr>(.{0}|(\\'.*\\'\\[.*\\])))MsrAF(?<MsrAF>.*)MsrVal(?<MsrVal>.*)MsrCalcFn(?<MsrCalcFn>(.{0}|(\\'.*\\'\\[.*\\])))AnyMeas(?<AnyMeas>.*)AnyCatVal(?<AnyCatVal>.*)#";
        private const string V1AltMapsRegExString = "^Lat(?<Lat>(.{0}|(\\'.*\\'\\[.*\\])))LatVal(?<LatVal>.*)Lon(?<Lon>(.{0}|(\\'.*\\'\\[.*\\])))LonVal(?<LonVal>.*)Addr(?<Addr>(.{0}|(\\'.*\\'\\[.*\\])))AddrVal(?<AddrVal>.*)Ad(?<Ad>(.{0}|(\\'.*\\'\\[.*\\])))AdVal(?<AdVal>.*)Ad2(?<Ad2>(.{0}|(\\'.*\\'\\[.*\\])))Ad2Val(?<Ad2Val>.*)Country(?<Country>(.{0}|(\\'.*\\'\\[.*\\])))CountryVal(?<CountryVal>.*)Loc(?<Loc>(.{0}|(\\'.*\\'\\[.*\\])))LocVal(?<LocVal>.*)Zip(?<Zip>(.{0}|(\\'.*\\'\\[.*\\])))ZipVal(?<ZipVal>.*)FullAddr(?<FullAddr>(.{0}|(\\'.*\\'\\[.*\\])))FullAddrVal(?<FullAddrVal>.*)Old(?<Old>(.{0}|(\\'.*\\'\\[.*\\])))OldVal(?<OldVal>.*)Cat(?<Cat>(.{0}|(\\'.*\\'\\[.*\\])))CatVal(?<CatVal>.*)Msr(?<Msr>(.{0}|(\\'.*\\'\\[.*\\])))MsrAF(?<MsrAF>.*)MsrVal(?<MsrVal>.*)MsrCalcFn(?<MsrCalcFn>(.{0}|(\\'.*\\'\\[.*\\])))AnyMeas(?<AnyMeas>.*)AnyCatVal(?<AnyCatVal>.*)#XCoord(?<XCoord>(.{0}|(\\'.*\\'\\[.*\\])))XCoordVal(?<XCoordVal>.*)YCoord(?<YCoord>(.{0}|(\\'.*\\'\\[.*\\])))YCoordVal(?<YCoordVal>.*)#";
        private const string V1Oct2014RegExString = "^Lat(?<Lat>(.{0}|(\\'.*\\'\\[.*\\])))LatVal(?<LatVal>.*)Lon(?<Lon>(.{0}|(\\'.*\\'\\[.*\\])))LonVal(?<LonVal>.*)Addr(?<Addr>(.{0}|(\\'.*\\'\\[.*\\])))AddrVal(?<AddrVal>.*)Ad(?<Ad>(.{0}|(\\'.*\\'\\[.*\\])))AdVal(?<AdVal>.*)Ad2(?<Ad2>(.{0}|(\\'.*\\'\\[.*\\])))Ad2Val(?<Ad2Val>.*)Country(?<Country>(.{0}|(\\'.*\\'\\[.*\\])))CountryVal(?<CountryVal>.*)Loc(?<Loc>(.{0}|(\\'.*\\'\\[.*\\])))LocVal(?<LocVal>.*)Zip(?<Zip>(.{0}|(\\'.*\\'\\[.*\\])))ZipVal(?<ZipVal>.*)FullAddr(?<FullAddr>(.{0}|(\\'.*\\'\\[.*\\])))FullAddrVal(?<FullAddrVal>.*)Old(?<Old>(.{0}|(\\'.*\\'\\[.*\\])))OldVal(?<OldVal>.*)Cat(?<Cat>(.{0}|(\\'.*\\'\\[.*\\])))CatVal(?<CatVal>.*)Msr(?<Msr>(.{0}|(\\'.*\\'\\[.*\\])))MsrAF(?<MsrAF>.*)MsrVal(?<MsrVal>.*)MsrCalcFn(?<MsrCalcFn>(.{0}|(\\'.*\\'\\[.*\\])))AnyMeas(?<AnyMeas>.*)AnyCatVal(?<AnyCatVal>.*)#XCoord(?<XCoord>(.{0}|(\\'.*\\'\\[.*\\])))XCoordVal(?<XCoordVal>.*)YCoord(?<YCoord>(.{0}|(\\'.*\\'\\[.*\\])))YCoordVal(?<YCoordVal>.*)##";

        public TableColumn LatitudeColumn { get; set; }

        public double? Latitude { get; set; }

        public TableColumn LongitudeColumn { get; set; }

        public double? Longitude { get; set; }

        public TableColumn XCoordColumn { get; set; }

        public double? XCoord { get; set; }

        public TableColumn YCoordColumn { get; set; }

        public double? YCoord { get; set; }

        public string AddressLine { get; set; }

        public string AdminDistrict { get; set; }

        public string AdminDistrict2 { get; set; }

        public string Country { get; set; }

        public string Locality { get; set; }

        public string PostalCode { get; set; }

        public TableColumn AddressLineColumn { get; set; }

        public TableColumn AdminDistrictColumn { get; set; }

        public TableColumn AdminDistrict2Column { get; set; }

        public TableColumn CountryColumn { get; set; }

        public TableColumn LocalityColumn { get; set; }

        public TableColumn PostalCodeColumn { get; set; }

        public string FullAddress { get; set; }

        public TableColumn FullAddressColumn { get; set; }

        public string OtherLocationDescription { get; set; }

        public TableColumn OtherLocationDescriptionColumn { get; set; }

        public TableColumn CategoryColumn { get; set; }

        public string Category { get; set; }

        public Tuple<TableField, AggregationFunction> Measure { get; set; }

        public double? MeasureValue { get; set; }

        public bool AnyMeasure { get; set; }

        public bool AnyCategoryValue { get; set; }

        private void ClearGeo()
        {
            this.LatitudeColumn = null;
            this.Latitude = new double?();
            this.LongitudeColumn = null;
            this.Longitude = new double?();
            this.XCoordColumn = null;
            this.XCoord = new double?();
            this.YCoordColumn = null;
            this.YCoord = new double?();
            this.AddressLineColumn = null;
            this.AddressLine = null;
            this.AdminDistrictColumn = null;
            this.AdminDistrict = null;
            this.AdminDistrict2Column = null;
            this.AdminDistrict2 = null;
            this.CountryColumn = null;
            this.Country = null;
            this.LocalityColumn = null;
            this.Locality = null;
            this.PostalCodeColumn = null;
            this.PostalCode = null;
            this.FullAddress = null;
            this.FullAddressColumn = null;
            this.OtherLocationDescription = null;
            this.OtherLocationDescriptionColumn = null;
        }

        private void ClearNonGeo()
        {
            this.CategoryColumn = null;
            this.Category = null;
            this.Measure = null;
            this.MeasureValue = new double?();
        }

        public void Clear()
        {
            this.ClearGeo();
            this.ClearNonGeo();
        }

        public override string ToString()
        {
            this.BuildV1RCGeo();
            bool anyMeasure = this.AnyMeasure;
            bool anyCategoryValue = this.AnyCategoryValue;
            this.stringBuilder.AppendFormat(CultureInId, "{0}", CatColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.CategoryColumn == null
                    ? string.Empty
                    : this.CategoryColumn.ModelQueryName);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", CatValRegExGroup);
            string category = this.Category;
            if (this.CategoryColumn != null && !string.IsNullOrWhiteSpace(category))
            {
                switch (this.CategoryColumn.DataType)
                {
                    case TableMemberDataType.DateTime:
                        DateTime time;
                        if (DateTime.TryParse(category, out time))
                        {
                            category = time.ToString(CultureInId);
                        }
                        break;
                    case TableMemberDataType.Long:
                        long val;
                        if (long.TryParse(category, out val))
                        {
                            category = val.ToString(CultureInId);
                        }
                        break;
                    case TableMemberDataType.Double:
                    case TableMemberDataType.Currency:
                        double num;
                        if (double.TryParse(category, out num))
                        {
                            category = num.ToString(CultureInId);
                        }
                        break;
                }
            }
            this.stringBuilder.Append(category);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", MsrColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.Measure == null || !(this.Measure.Item1 is TableColumn)
                    ? string.Empty
                    : (this.Measure.Item1 as TableColumn).ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", MsrAFRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.Measure == null ? string.Empty : ((object)this.Measure.Item2).ToString());
            this.stringBuilder.AppendFormat(CultureInId, "{0}", MsrValRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.MeasureValue.HasValue ? this.MeasureValue.Value.ToString() : string.Empty);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", MsrCalcFnColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.Measure == null || !(this.Measure.Item1 is TableMeasure)
                    ? string.Empty
                    : (this.Measure.Item1 as TableMeasure).ModelQueryName
            );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", AnyMeasureRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", this.AnyMeasure.ToString().ToUpperInvariant());
            this.stringBuilder.AppendFormat(CultureInId, "{0}", AnyCatValRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", this.AnyCategoryValue.ToString().ToUpperInvariant());
            this.stringBuilder.AppendFormat(CultureInId, "{0}", StringTerminator);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", XCoordColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", this.XCoordColumn == null ? string.Empty : this.XCoordColumn.ModelQueryName);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", XCoordValRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", this.XCoord.HasValue ? this.XCoord.Value.ToString() : string.Empty);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", YCoordColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", this.YCoordColumn == null ? string.Empty : this.YCoordColumn.ModelQueryName);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", YCoordValRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", this.YCoord.HasValue ? this.YCoord.Value.ToString() : string.Empty);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", StringTerminator);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", StringTerminator);
            return this.stringBuilder.ToString();
        }

        private void BuildV1RCGeo()
        {
            this.stringBuilder.Clear();
            this.stringBuilder.AppendFormat(CultureInId, "{0}", LatColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.LatitudeColumn == null
                    ? string.Empty
                    : this.LatitudeColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", LatValRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.Latitude.HasValue
                    ? this.Latitude.Value.ToString()
                    : string.Empty
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", LonColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.LongitudeColumn == null
                    ? string.Empty
                    : this.LongitudeColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", LonValRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.Longitude.HasValue
                    ? this.Longitude.Value.ToString()
                    : string.Empty
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", AddrColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.AddressLineColumn == null
                    ? string.Empty
                    : this.AddressLineColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", AddrValRegExGroup);
            this.stringBuilder.Append(this.AddressLine);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", AdColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.AdminDistrictColumn == null
                    ? string.Empty
                    : this.AdminDistrictColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", AdValRegExGroup);
            this.stringBuilder.Append(this.AdminDistrict);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", Ad2ColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.AdminDistrict2Column == null
                    ? string.Empty
                    : this.AdminDistrict2Column.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", Ad2ValRegExGroup);
            this.stringBuilder.Append(this.AdminDistrict2);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", CountryColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.CountryColumn == null
                    ? string.Empty
                    : this.CountryColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", CountryValRegExGroup);
            this.stringBuilder.Append(this.Country);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", LocColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.LocalityColumn == null
                    ? string.Empty
                    : this.LocalityColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", LocValRegExGroup);
            this.stringBuilder.Append(this.Locality);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", ZipColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.PostalCodeColumn == null
                    ? string.Empty
                    : this.PostalCodeColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", ZipValRegExGroup);
            this.stringBuilder.Append(this.PostalCode);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", FullAddrColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.FullAddressColumn == null
                    ? string.Empty
                    : this.FullAddressColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", FullAddrValRegExGroup);
            this.stringBuilder.Append(this.FullAddress);
            this.stringBuilder.AppendFormat(CultureInId, "{0}", OldColRegExGroup);
            this.stringBuilder.AppendFormat(CultureInId, "{0}",
                this.OtherLocationDescriptionColumn == null
                    ? string.Empty
                    : this.OtherLocationDescriptionColumn.ModelQueryName
                );
            this.stringBuilder.AppendFormat(CultureInId, "{0}", OldValRegExGroup);
            this.stringBuilder.Append(this.OtherLocationDescription);
        }

        public static GeoModelDataId TryParse(string modelIdString, GeoField geo, TableColumn cat, List<Tuple<TableField, AggregationFunction>> measures, bool noGeoValues)
        {
            if (measures == null)
                return null;
            if (string.IsNullOrWhiteSpace(modelIdString))
                return null;
            bool flag = true;
            // 开始尝试匹配
            Match match = GeoModelDataIdOct2014Regex.Match(modelIdString);
            if (!match.Success)
            {
                flag = false;
                match = GeoModelDataIdAltMapsRegex.Match(modelIdString);
                if (!match.Success)
                {
                    match = GeoModelDataIdV1RCRegex.Match(modelIdString);
                    if (!match.Success)
                        return null;
                }
            }

            // 构建GeoModelDataId
            GeoModelDataId geoModelDataId = new GeoModelDataId();

            // step 1: 分类
            if (cat == null && 
                (!string.IsNullOrEmpty(match.Groups[CatColRegExGroup].Value) ||
                !string.IsNullOrEmpty(match.Groups[CatValRegExGroup].Value) ||
                string.IsNullOrEmpty(match.Groups[AnyCatValRegExGroup].Value)))
                return null;
            if (cat != null)
            {
                if (string.Compare(match.Groups[CatColRegExGroup].Value, cat.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0)
                    return null;
                if (string.IsNullOrEmpty(match.Groups[AnyCatValRegExGroup].Value))
                    return null;
                geoModelDataId.AnyCategoryValue = string.Compare(match.Groups[AnyCatValRegExGroup].Value, true.ToString().ToUpperInvariant(), StringComparison.Ordinal) == 0;
                geoModelDataId.Category =
                    match.Groups[CatValRegExGroup].Value == string.Empty ? null : match.Groups[CatValRegExGroup].Value;
                geoModelDataId.CategoryColumn = cat;
                if (geoModelDataId.AnyCategoryValue && geoModelDataId.Category != null)
                    return null;
                if (flag && !string.IsNullOrWhiteSpace(geoModelDataId.Category))
                {
                    string category = geoModelDataId.Category;
                    switch (cat.DataType)
                    {
                        case TableMemberDataType.DateTime:
                            DateTime time;
                            if (DateTime.TryParse(category, CultureInId, DateTimeStyles.None, out time))
                            {
                                category = time.ToString();
                            }
                            break;
                        case TableMemberDataType.Long:
                            long val;
                            if (long.TryParse(category, NumberStyles.Any, CultureInId, out val))
                            {
                                category = val.ToString();
                            }
                            break;
                        case TableMemberDataType.Double:
                        case TableMemberDataType.Currency:
                            double num;
                            if (double.TryParse(category, NumberStyles.Any, CultureInId, out num))
                            {
                                category = num.ToString();
                            }
                            break;
                    }
                    geoModelDataId.Category = category;
                }
            }

            // step 2:构建统计
            if (measures.Count == 0)
            {
                if (!string.IsNullOrEmpty(match.Groups[MsrColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[MsrAFRegExGroup].Value) ||
                    (!string.IsNullOrEmpty(match.Groups[MsrValRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[MsrCalcFnColRegExGroup].Value)) ||
                    string.IsNullOrEmpty(match.Groups[AnyMeasureRegExGroup].Value))
                    return null;
                geoModelDataId.AnyMeasure = string.Compare(match.Groups[AnyMeasureRegExGroup].Value, true.ToString().ToUpperInvariant(), StringComparison.Ordinal) == 0;
            }
            else if (measures.Count > 0)
            {
                if (string.IsNullOrEmpty(match.Groups[AnyMeasureRegExGroup].Value))
                    return null;
                geoModelDataId.AnyMeasure = string.Compare(match.Groups[AnyMeasureRegExGroup].Value, true.ToString().ToUpperInvariant(), StringComparison.Ordinal) == 0;
                if (geoModelDataId.AnyMeasure)
                    return null;
                if (!string.IsNullOrEmpty(match.Groups[MsrCalcFnColRegExGroup].Value))
                {
                    if (!string.IsNullOrEmpty(match.Groups[MsrColRegExGroup].Value) ||
                        string.Compare(match.Groups[MsrAFRegExGroup].Value, ((object)AggregationFunction.UserDefined).ToString(), StringComparison.Ordinal) != 0)
                        return null;
                }
                else if (string.IsNullOrEmpty(match.Groups[MsrColRegExGroup].Value) ||
                    string.IsNullOrEmpty(match.Groups[MsrAFRegExGroup].Value))
                    return null;
                Tuple<TableField, AggregationFunction> tuple = Enumerable.FirstOrDefault(measures, (msr =>
                {
                    if (msr.Item1 is TableColumn &&
                        string.Compare((msr.Item1 as TableColumn).ModelQueryName, match.Groups[MsrColRegExGroup].Value, StringComparison.OrdinalIgnoreCase) == 0)
                        return string.Compare(((object)msr.Item2).ToString(), match.Groups[MsrAFRegExGroup].Value, StringComparison.Ordinal) == 0;
                    return false;
                }));
                if (tuple == null)
                {
                    tuple = Enumerable.FirstOrDefault(measures, (msr =>
                    {
                        if (msr.Item1 is TableMeasure &&
                            string.Compare((msr.Item1 as TableMeasure).ModelQueryName, match.Groups[MsrCalcFnColRegExGroup].Value, StringComparison.OrdinalIgnoreCase) == 0)
                            return string.Compare(((object)msr.Item2).ToString(), match.Groups[MsrAFRegExGroup].Value, StringComparison.Ordinal) == 0;
                        return false;
                    }));
                    if (tuple == null)
                        return null;
                }
                geoModelDataId.Measure = new Tuple<TableField, AggregationFunction>(tuple.Item1, tuple.Item2);
                if (string.IsNullOrEmpty(match.Groups[MsrValRegExGroup].Value))
                {
                    if (geoModelDataId.Measure.Item2 == AggregationFunction.None)
                        return null;
                    geoModelDataId.MeasureValue = new double?();
                }
                else
                {
                    if (geoModelDataId.Measure.Item2 != AggregationFunction.None)
                        return null;
                    double result;
                    if (!double.TryParse(match.Groups[MsrValRegExGroup].Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInId, out result))
                        return null;
                    geoModelDataId.MeasureValue = new double?(result);
                }
            }

            // step 3:构建空间
            if (noGeoValues)
            {
                if (!string.IsNullOrEmpty(match.Groups[LatColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[LatValRegExGroup].Value) ||
                    (!string.IsNullOrEmpty(match.Groups[LonColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[LonValRegExGroup].Value)) ||
                    (!string.IsNullOrEmpty(match.Groups[XCoordColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[XCoordValRegExGroup].Value) ||
                    (!string.IsNullOrEmpty(match.Groups[YCoordColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[YCoordValRegExGroup].Value))) ||
                    (!string.IsNullOrEmpty(match.Groups[FullAddrColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[FullAddrValRegExGroup].Value) ||
                    (!string.IsNullOrEmpty(match.Groups[OldColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[OldValRegExGroup].Value)) ||
                    (!string.IsNullOrEmpty(match.Groups[AddrColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[AddrValRegExGroup].Value) ||
                    (!string.IsNullOrEmpty(match.Groups[AdColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[AdValRegExGroup].Value)))) ||
                    (!string.IsNullOrEmpty(match.Groups[Ad2ColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[Ad2ValRegExGroup].Value) ||
                    (!string.IsNullOrEmpty(match.Groups[CountryColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[CountryValRegExGroup].Value)) ||
                    (!string.IsNullOrEmpty(match.Groups[LocColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[LocValRegExGroup].Value) ||
                    (!string.IsNullOrEmpty(match.Groups[ZipColRegExGroup].Value) ||
                    !string.IsNullOrEmpty(match.Groups[ZipValRegExGroup].Value)))))
                    return null;
            }
            else if (geo is LatLongField && !geo.IsUsingXY)
            {
                string strA1 = match.Groups[LatColRegExGroup].Value;
                string strA2 = match.Groups[LonColRegExGroup].Value;
                if (string.IsNullOrWhiteSpace(strA1) || string.IsNullOrWhiteSpace(strA2))
                    return null;
                if (string.Compare(strA1, geo.Latitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0 || string.Compare(strA2, geo.Longitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0)
                    return null;
                geoModelDataId.LatitudeColumn = geo.Latitude;
                geoModelDataId.LongitudeColumn = geo.Longitude;
                if (match.Groups[LatValRegExGroup].Value == null)
                {
                    geoModelDataId.Latitude = new double?();
                }
                else
                {
                    double result;
                    if (!double.TryParse(match.Groups[LatValRegExGroup].Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInId, out result))
                        return null;
                    geoModelDataId.Latitude = new double?(result);
                }
                if (match.Groups[LonValRegExGroup].Value == null)
                {
                    geoModelDataId.Longitude = new double?();
                }
                else
                {
                    double result;
                    if (!double.TryParse(match.Groups[LonValRegExGroup].Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInId, out result))
                        return null;
                    geoModelDataId.Longitude = new double?(result);
                }
            }
            else if (geo is LatLongField && geo.IsUsingXY)
            {
                string strA1 = match.Groups[XCoordColRegExGroup].Value;
                string strA2 = match.Groups[YCoordColRegExGroup].Value;
                if (string.IsNullOrWhiteSpace(strA1) || string.IsNullOrWhiteSpace(strA2))
                    return null;
                if (string.Compare(strA1, geo.Longitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0 || string.Compare(strA2, geo.Latitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0)
                    return null;
                geoModelDataId.XCoordColumn = geo.Longitude;
                geoModelDataId.YCoordColumn = geo.Latitude;
                if (match.Groups[XCoordValRegExGroup].Value == null)
                {
                    geoModelDataId.XCoord = new double?();
                }
                else
                {
                    double result;
                    if (!double.TryParse(match.Groups[XCoordValRegExGroup].Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInId, out result))
                        return null;
                    geoModelDataId.XCoord = new double?(result);
                }
                if (match.Groups[YCoordValRegExGroup].Value == null)
                {
                    geoModelDataId.YCoord = new double?();
                }
                else
                {
                    double result;
                    if (!double.TryParse(match.Groups[YCoordValRegExGroup].Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInId, out result))
                        return null;
                    geoModelDataId.YCoord = new double?(result);
                }
            }
            else if (geo is GeoFullAddressField)
            {
                GeoFullAddressField fullAddressField = geo as GeoFullAddressField;
                string strB1 = match.Groups[FullAddrColRegExGroup].Value;
                string strB2 = match.Groups[OldColRegExGroup].Value;
                if (!(string.IsNullOrWhiteSpace(strB1) ^ string.IsNullOrWhiteSpace(strB2)))
                    return null;
                if (!string.IsNullOrWhiteSpace(strB1))
                {
                    if (fullAddressField.FullAddress == null || string.Compare(fullAddressField.FullAddress.ModelQueryName, strB1, StringComparison.OrdinalIgnoreCase) != 0)
                        return null;
                    string str = match.Groups[FullAddrValRegExGroup].Value;
                    if (str == string.Empty)
                        str = null;
                    if (str == null || !string.IsNullOrEmpty(match.Groups[OldValRegExGroup].Value))
                        return null;
                    geoModelDataId.FullAddress = str;
                    geoModelDataId.FullAddressColumn = fullAddressField.FullAddress;
                }
                else
                {
                    if (fullAddressField.OtherLocationDescription == null || string.Compare(fullAddressField.OtherLocationDescription.ModelQueryName, strB2, StringComparison.OrdinalIgnoreCase) != 0)
                        return null;
                    string str = match.Groups[OldValRegExGroup].Value;
                    if (str == string.Empty)
                        str = null;
                    if (str == null || !string.IsNullOrEmpty(match.Groups[FullAddrValRegExGroup].Value))
                        return null;
                    geoModelDataId.OtherLocationDescription = str;
                    geoModelDataId.OtherLocationDescriptionColumn = fullAddressField.OtherLocationDescription;
                }
            }
            else
            {
                if (!(geo is GeoEntityField))
                    return null;
                GeoEntityField geo1 = geo as GeoEntityField;
                string geoVal;
                TableColumn geoCol;
                if (!GetGeo(geo1, GeoEntityField.GeoEntityLevel.AddressLine,
                    match.Groups[AddrValRegExGroup].Value,
                    match.Groups[AddrColRegExGroup].Value, out geoVal, out geoCol))
                    return null;
                geoModelDataId.AddressLine = geoVal;
                geoModelDataId.AddressLineColumn = geoCol;
                if (!GetGeo(geo1, GeoEntityField.GeoEntityLevel.AdminDistrict,
                    match.Groups[AdValRegExGroup].Value,
                    match.Groups[AdColRegExGroup].Value, out geoVal, out geoCol))
                    return null;
                geoModelDataId.AdminDistrict = geoVal;
                geoModelDataId.AdminDistrictColumn = geoCol;
                if (!GetGeo(geo1, GeoEntityField.GeoEntityLevel.AdminDistrict2,
                    match.Groups[Ad2ValRegExGroup].Value,
                    match.Groups[Ad2ColRegExGroup].Value, out geoVal, out geoCol))
                    return null;
                geoModelDataId.AdminDistrict2 = geoVal;
                geoModelDataId.AdminDistrict2Column = geoCol;
                if (!GetGeo(geo1, GeoEntityField.GeoEntityLevel.Country,
                    match.Groups[CountryValRegExGroup].Value,
                    match.Groups[CountryColRegExGroup].Value, out geoVal, out geoCol))
                    return null;
                geoModelDataId.Country = geoVal;
                geoModelDataId.CountryColumn = geoCol;
                if (!GetGeo(geo1, GeoEntityField.GeoEntityLevel.Locality,
                    match.Groups[LocValRegExGroup].Value,
                    match.Groups[LocColRegExGroup].Value, out geoVal, out geoCol))
                    return null;
                geoModelDataId.Locality = geoVal;
                geoModelDataId.LocalityColumn = geoCol;
                if (!GetGeo(geo1, GeoEntityField.GeoEntityLevel.PostalCode,
                    match.Groups[ZipValRegExGroup].Value,
                    match.Groups[ZipColRegExGroup].Value, out geoVal, out geoCol))
                    return null;
                geoModelDataId.PostalCode = geoVal;
                geoModelDataId.PostalCodeColumn = geoCol;
                foreach (TableColumn column in geo1.GeoColumns)
                {
                    switch (geo1.GeoLevel(column))
                    {
                        case GeoEntityField.GeoEntityLevel.AddressLine:
                            if (geoModelDataId.AddressLineColumn == null)
                                return null;
                            continue;
                        case GeoEntityField.GeoEntityLevel.Locality:
                            if (geoModelDataId.LocalityColumn == null)
                                return null;
                            continue;
                        case GeoEntityField.GeoEntityLevel.AdminDistrict2:
                            if (geoModelDataId.AdminDistrict2Column == null)
                                return null;
                            continue;
                        case GeoEntityField.GeoEntityLevel.AdminDistrict:
                            if (geoModelDataId.AdminDistrictColumn == null)
                                return null;
                            continue;
                        case GeoEntityField.GeoEntityLevel.PostalCode:
                            if (geoModelDataId.PostalCodeColumn == null)
                                return null;
                            continue;
                        case GeoEntityField.GeoEntityLevel.Country:
                            if (geoModelDataId.CountryColumn == null)
                                return null;
                            continue;
                        default:
                            return null;
                    }
                }
            }
            return geoModelDataId;
        }

        private static bool GetGeo(
            GeoEntityField geo, GeoEntityField.GeoEntityLevel geoLevel,
            string modelVal, string modelColVal,
            out string geoVal, out TableColumn geoCol)
        {
            if (modelColVal == string.Empty)
            {
                geoCol = null;
                geoVal = null;
                return true;
            }
            geoVal = modelVal;
            geoCol = geo.GeoColumns.FirstOrDefault(
                (col =>
                    0 == string.Compare(col.ModelQueryName, modelColVal, StringComparison.OrdinalIgnoreCase)));
            return geoCol != null && geo.GeoLevel(geoCol) == geoLevel;
        }
    }
}
