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
    private static readonly Regex GeoModelDataIdV1RCRegex = new Regex("^Lat(?<Lat>(.{0}|(\\'.*\\'\\[.*\\])))LatVal(?<LatVal>.*)Lon(?<Lon>(.{0}|(\\'.*\\'\\[.*\\])))LonVal(?<LonVal>.*)Addr(?<Addr>(.{0}|(\\'.*\\'\\[.*\\])))AddrVal(?<AddrVal>.*)Ad(?<Ad>(.{0}|(\\'.*\\'\\[.*\\])))AdVal(?<AdVal>.*)Ad2(?<Ad2>(.{0}|(\\'.*\\'\\[.*\\])))Ad2Val(?<Ad2Val>.*)Country(?<Country>(.{0}|(\\'.*\\'\\[.*\\])))CountryVal(?<CountryVal>.*)Loc(?<Loc>(.{0}|(\\'.*\\'\\[.*\\])))LocVal(?<LocVal>.*)Zip(?<Zip>(.{0}|(\\'.*\\'\\[.*\\])))ZipVal(?<ZipVal>.*)FullAddr(?<FullAddr>(.{0}|(\\'.*\\'\\[.*\\])))FullAddrVal(?<FullAddrVal>.*)Old(?<Old>(.{0}|(\\'.*\\'\\[.*\\])))OldVal(?<OldVal>.*)Cat(?<Cat>(.{0}|(\\'.*\\'\\[.*\\])))CatVal(?<CatVal>.*)Msr(?<Msr>(.{0}|(\\'.*\\'\\[.*\\])))MsrAF(?<MsrAF>.*)MsrVal(?<MsrVal>.*)MsrCalcFn(?<MsrCalcFn>(.{0}|(\\'.*\\'\\[.*\\])))AnyMeas(?<AnyMeas>.*)AnyCatVal(?<AnyCatVal>.*)#", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex GeoModelDataIdAltMapsRegex = new Regex("^Lat(?<Lat>(.{0}|(\\'.*\\'\\[.*\\])))LatVal(?<LatVal>.*)Lon(?<Lon>(.{0}|(\\'.*\\'\\[.*\\])))LonVal(?<LonVal>.*)Addr(?<Addr>(.{0}|(\\'.*\\'\\[.*\\])))AddrVal(?<AddrVal>.*)Ad(?<Ad>(.{0}|(\\'.*\\'\\[.*\\])))AdVal(?<AdVal>.*)Ad2(?<Ad2>(.{0}|(\\'.*\\'\\[.*\\])))Ad2Val(?<Ad2Val>.*)Country(?<Country>(.{0}|(\\'.*\\'\\[.*\\])))CountryVal(?<CountryVal>.*)Loc(?<Loc>(.{0}|(\\'.*\\'\\[.*\\])))LocVal(?<LocVal>.*)Zip(?<Zip>(.{0}|(\\'.*\\'\\[.*\\])))ZipVal(?<ZipVal>.*)FullAddr(?<FullAddr>(.{0}|(\\'.*\\'\\[.*\\])))FullAddrVal(?<FullAddrVal>.*)Old(?<Old>(.{0}|(\\'.*\\'\\[.*\\])))OldVal(?<OldVal>.*)Cat(?<Cat>(.{0}|(\\'.*\\'\\[.*\\])))CatVal(?<CatVal>.*)Msr(?<Msr>(.{0}|(\\'.*\\'\\[.*\\])))MsrAF(?<MsrAF>.*)MsrVal(?<MsrVal>.*)MsrCalcFn(?<MsrCalcFn>(.{0}|(\\'.*\\'\\[.*\\])))AnyMeas(?<AnyMeas>.*)AnyCatVal(?<AnyCatVal>.*)#XCoord(?<XCoord>(.{0}|(\\'.*\\'\\[.*\\])))XCoordVal(?<XCoordVal>.*)YCoord(?<YCoord>(.{0}|(\\'.*\\'\\[.*\\])))YCoordVal(?<YCoordVal>.*)#", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex GeoModelDataIdOct2014Regex = new Regex("^Lat(?<Lat>(.{0}|(\\'.*\\'\\[.*\\])))LatVal(?<LatVal>.*)Lon(?<Lon>(.{0}|(\\'.*\\'\\[.*\\])))LonVal(?<LonVal>.*)Addr(?<Addr>(.{0}|(\\'.*\\'\\[.*\\])))AddrVal(?<AddrVal>.*)Ad(?<Ad>(.{0}|(\\'.*\\'\\[.*\\])))AdVal(?<AdVal>.*)Ad2(?<Ad2>(.{0}|(\\'.*\\'\\[.*\\])))Ad2Val(?<Ad2Val>.*)Country(?<Country>(.{0}|(\\'.*\\'\\[.*\\])))CountryVal(?<CountryVal>.*)Loc(?<Loc>(.{0}|(\\'.*\\'\\[.*\\])))LocVal(?<LocVal>.*)Zip(?<Zip>(.{0}|(\\'.*\\'\\[.*\\])))ZipVal(?<ZipVal>.*)FullAddr(?<FullAddr>(.{0}|(\\'.*\\'\\[.*\\])))FullAddrVal(?<FullAddrVal>.*)Old(?<Old>(.{0}|(\\'.*\\'\\[.*\\])))OldVal(?<OldVal>.*)Cat(?<Cat>(.{0}|(\\'.*\\'\\[.*\\])))CatVal(?<CatVal>.*)Msr(?<Msr>(.{0}|(\\'.*\\'\\[.*\\])))MsrAF(?<MsrAF>.*)MsrVal(?<MsrVal>.*)MsrCalcFn(?<MsrCalcFn>(.{0}|(\\'.*\\'\\[.*\\])))AnyMeas(?<AnyMeas>.*)AnyCatVal(?<AnyCatVal>.*)#XCoord(?<XCoord>(.{0}|(\\'.*\\'\\[.*\\])))XCoordVal(?<XCoordVal>.*)YCoord(?<YCoord>(.{0}|(\\'.*\\'\\[.*\\])))YCoordVal(?<YCoordVal>.*)##", RegexOptions.Compiled | RegexOptions.CultureInvariant);
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
      this.LatitudeColumn = (TableColumn) null;
      this.Latitude = new double?();
      this.LongitudeColumn = (TableColumn) null;
      this.Longitude = new double?();
      this.XCoordColumn = (TableColumn) null;
      this.XCoord = new double?();
      this.YCoordColumn = (TableColumn) null;
      this.YCoord = new double?();
      this.AddressLineColumn = (TableColumn) null;
      this.AddressLine = (string) null;
      this.AdminDistrictColumn = (TableColumn) null;
      this.AdminDistrict = (string) null;
      this.AdminDistrict2Column = (TableColumn) null;
      this.AdminDistrict2 = (string) null;
      this.CountryColumn = (TableColumn) null;
      this.Country = (string) null;
      this.LocalityColumn = (TableColumn) null;
      this.Locality = (string) null;
      this.PostalCodeColumn = (TableColumn) null;
      this.PostalCode = (string) null;
      this.FullAddress = (string) null;
      this.FullAddressColumn = (TableColumn) null;
      this.OtherLocationDescription = (string) null;
      this.OtherLocationDescriptionColumn = (TableColumn) null;
    }

    private void ClearNonGeo()
    {
      this.CategoryColumn = (TableColumn) null;
      this.Category = (string) null;
      this.Measure = (Tuple<TableField, AggregationFunction>) null;
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
      int num1 = this.AnyMeasure ? 1 : 0;
      int num2 = this.AnyCategoryValue ? 1 : 0;
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Cat"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.CategoryColumn == null ? (object) string.Empty : (object) this.CategoryColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "CatVal"
      });
      string s = this.Category;
      if (this.CategoryColumn != null && !string.IsNullOrWhiteSpace(s))
      {
        switch (this.CategoryColumn.DataType)
        {
          case TableMemberDataType.DateTime:
            DateTime result1;
            if (DateTime.TryParse(s, out result1))
            {
              s = result1.ToString((IFormatProvider) GeoModelDataId.CultureInId);
              break;
            }
            else
              break;
          case TableMemberDataType.Long:
            long result2;
            if (long.TryParse(s, out result2))
            {
              s = result2.ToString((IFormatProvider) GeoModelDataId.CultureInId);
              break;
            }
            else
              break;
          case TableMemberDataType.Double:
          case TableMemberDataType.Currency:
            double result3;
            if (double.TryParse(s, out result3))
            {
              s = result3.ToString((IFormatProvider) GeoModelDataId.CultureInId);
              break;
            }
            else
              break;
        }
      }
      this.stringBuilder.Append(s);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Msr"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.Measure == null || !(this.Measure.Item1 is TableColumn) ? (object) string.Empty : (object) (this.Measure.Item1 as TableColumn).ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "MsrAF"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.Measure == null ? (object) string.Empty : (object) ((object) this.Measure.Item2).ToString()
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "MsrVal"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.MeasureValue.HasValue ? (object) this.MeasureValue.Value.ToString() : (object) string.Empty
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "MsrCalcFn"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.Measure == null || !(this.Measure.Item1 is TableMeasure) ? (object) string.Empty : (object) (this.Measure.Item1 as TableMeasure).ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "AnyMeas"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) this.AnyMeasure.ToString().ToUpperInvariant()
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "AnyCatVal"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) this.AnyCategoryValue.ToString().ToUpperInvariant()
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "#"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "XCoord"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.XCoordColumn == null ? (object) string.Empty : (object) this.XCoordColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "XCoordVal"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.XCoord.HasValue ? (object) this.XCoord.Value.ToString() : (object) string.Empty
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "YCoord"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.YCoordColumn == null ? (object) string.Empty : (object) this.YCoordColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "YCoordVal"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.YCoord.HasValue ? (object) this.YCoord.Value.ToString() : (object) string.Empty
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "#"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "#"
      });
      return ((object) this.stringBuilder).ToString();
    }

    private void BuildV1RCGeo()
    {
      this.stringBuilder.Clear();
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Lat"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.LatitudeColumn == null ? (object) string.Empty : (object) this.LatitudeColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "LatVal"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.Latitude.HasValue ? (object) this.Latitude.Value.ToString() : (object) string.Empty
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Lon"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.LongitudeColumn == null ? (object) string.Empty : (object) this.LongitudeColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "LonVal"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.Longitude.HasValue ? (object) this.Longitude.Value.ToString() : (object) string.Empty
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Addr"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.AddressLineColumn == null ? (object) string.Empty : (object) this.AddressLineColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "AddrVal"
      });
      this.stringBuilder.Append(this.AddressLine);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Ad"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.AdminDistrictColumn == null ? (object) string.Empty : (object) this.AdminDistrictColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "AdVal"
      });
      this.stringBuilder.Append(this.AdminDistrict);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Ad2"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.AdminDistrict2Column == null ? (object) string.Empty : (object) this.AdminDistrict2Column.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Ad2Val"
      });
      this.stringBuilder.Append(this.AdminDistrict2);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Country"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.CountryColumn == null ? (object) string.Empty : (object) this.CountryColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "CountryVal"
      });
      this.stringBuilder.Append(this.Country);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Loc"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.LocalityColumn == null ? (object) string.Empty : (object) this.LocalityColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "LocVal"
      });
      this.stringBuilder.Append(this.Locality);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Zip"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.PostalCodeColumn == null ? (object) string.Empty : (object) this.PostalCodeColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "ZipVal"
      });
      this.stringBuilder.Append(this.PostalCode);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "FullAddr"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.FullAddressColumn == null ? (object) string.Empty : (object) this.FullAddressColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "FullAddrVal"
      });
      this.stringBuilder.Append(this.FullAddress);
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "Old"
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        this.OtherLocationDescriptionColumn == null ? (object) string.Empty : (object) this.OtherLocationDescriptionColumn.ModelQueryName
      });
      this.stringBuilder.AppendFormat((IFormatProvider) GeoModelDataId.CultureInId, "{0}", new object[1]
      {
        (object) "OldVal"
      });
      this.stringBuilder.Append(this.OtherLocationDescription);
    }

    public static GeoModelDataId TryParse(string modelIdString, GeoField geo, TableColumn cat, List<Tuple<TableField, AggregationFunction>> measures, bool noGeoValues)
    {
      if (measures == null)
        return (GeoModelDataId) null;
      if (string.IsNullOrWhiteSpace(modelIdString))
        return (GeoModelDataId) null;
      bool flag = true;
      Match match = GeoModelDataId.GeoModelDataIdOct2014Regex.Match(modelIdString);
      if (!match.Success)
      {
        flag = false;
        match = GeoModelDataId.GeoModelDataIdAltMapsRegex.Match(modelIdString);
        if (!match.Success)
        {
          match = GeoModelDataId.GeoModelDataIdV1RCRegex.Match(modelIdString);
          if (!match.Success)
            return (GeoModelDataId) null;
        }
      }
      GeoModelDataId geoModelDataId = new GeoModelDataId();
      if (cat == null && (!string.IsNullOrEmpty(match.Groups["Cat"].Value) || !string.IsNullOrEmpty(match.Groups["CatVal"].Value) || string.IsNullOrEmpty(match.Groups["AnyCatVal"].Value)))
        return (GeoModelDataId) null;
      if (cat != null)
      {
        if (string.Compare(match.Groups["Cat"].Value, cat.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0)
          return (GeoModelDataId) null;
        if (string.IsNullOrEmpty(match.Groups["AnyCatVal"].Value))
          return (GeoModelDataId) null;
        geoModelDataId.AnyCategoryValue = string.Compare(match.Groups["AnyCatVal"].Value, true.ToString().ToUpperInvariant(), StringComparison.Ordinal) == 0;
        geoModelDataId.Category = match.Groups["CatVal"].Value == string.Empty ? (string) null : match.Groups["CatVal"].Value;
        geoModelDataId.CategoryColumn = cat;
        if (geoModelDataId.AnyCategoryValue && geoModelDataId.Category != null)
          return (GeoModelDataId) null;
        if (flag && !string.IsNullOrWhiteSpace(geoModelDataId.Category))
        {
          string s = geoModelDataId.Category;
          switch (cat.DataType)
          {
            case TableMemberDataType.DateTime:
              DateTime result1;
              if (DateTime.TryParse(s, (IFormatProvider) GeoModelDataId.CultureInId, DateTimeStyles.None, out result1))
              {
                s = result1.ToString();
                break;
              }
              else
                break;
            case TableMemberDataType.Long:
              long result2;
              if (long.TryParse(s, NumberStyles.Any, (IFormatProvider) GeoModelDataId.CultureInId, out result2))
              {
                s = result2.ToString();
                break;
              }
              else
                break;
            case TableMemberDataType.Double:
            case TableMemberDataType.Currency:
              double result3;
              if (double.TryParse(s, NumberStyles.Any, (IFormatProvider) GeoModelDataId.CultureInId, out result3))
              {
                s = result3.ToString();
                break;
              }
              else
                break;
          }
          geoModelDataId.Category = s;
        }
      }
      if (measures.Count == 0)
      {
        if (!string.IsNullOrEmpty(match.Groups["Msr"].Value) || !string.IsNullOrEmpty(match.Groups["MsrAF"].Value) || (!string.IsNullOrEmpty(match.Groups["MsrVal"].Value) || !string.IsNullOrEmpty(match.Groups["MsrCalcFn"].Value)) || string.IsNullOrEmpty(match.Groups["AnyMeas"].Value))
          return (GeoModelDataId) null;
        geoModelDataId.AnyMeasure = string.Compare(match.Groups["AnyMeas"].Value, true.ToString().ToUpperInvariant(), StringComparison.Ordinal) == 0;
      }
      else if (measures.Count > 0)
      {
        if (string.IsNullOrEmpty(match.Groups["AnyMeas"].Value))
          return (GeoModelDataId) null;
        geoModelDataId.AnyMeasure = string.Compare(match.Groups["AnyMeas"].Value, true.ToString().ToUpperInvariant(), StringComparison.Ordinal) == 0;
        if (geoModelDataId.AnyMeasure)
          return (GeoModelDataId) null;
        if (!string.IsNullOrEmpty(match.Groups["MsrCalcFn"].Value))
        {
          if (!string.IsNullOrEmpty(match.Groups["Msr"].Value) || string.Compare(match.Groups["MsrAF"].Value, ((object) AggregationFunction.UserDefined).ToString(), StringComparison.Ordinal) != 0)
            return (GeoModelDataId) null;
        }
        else if (string.IsNullOrEmpty(match.Groups["Msr"].Value) || string.IsNullOrEmpty(match.Groups["MsrAF"].Value))
          return (GeoModelDataId) null;
        Tuple<TableField, AggregationFunction> tuple = Enumerable.FirstOrDefault<Tuple<TableField, AggregationFunction>>((IEnumerable<Tuple<TableField, AggregationFunction>>) measures, (Func<Tuple<TableField, AggregationFunction>, bool>) (msr =>
        {
          if (msr.Item1 is TableColumn && string.Compare((msr.Item1 as TableColumn).ModelQueryName, match.Groups["Msr"].Value, StringComparison.OrdinalIgnoreCase) == 0)
            return string.Compare(((object) msr.Item2).ToString(), match.Groups["MsrAF"].Value, StringComparison.Ordinal) == 0;
          else
            return false;
        }));
        if (tuple == null)
        {
          tuple = Enumerable.FirstOrDefault<Tuple<TableField, AggregationFunction>>((IEnumerable<Tuple<TableField, AggregationFunction>>) measures, (Func<Tuple<TableField, AggregationFunction>, bool>) (msr =>
          {
            if (msr.Item1 is TableMeasure && string.Compare((msr.Item1 as TableMeasure).ModelQueryName, match.Groups["MsrCalcFn"].Value, StringComparison.OrdinalIgnoreCase) == 0)
              return string.Compare(((object) msr.Item2).ToString(), match.Groups["MsrAF"].Value, StringComparison.Ordinal) == 0;
            else
              return false;
          }));
          if (tuple == null)
            return (GeoModelDataId) null;
        }
        geoModelDataId.Measure = new Tuple<TableField, AggregationFunction>(tuple.Item1, tuple.Item2);
        if (string.IsNullOrEmpty(match.Groups["MsrVal"].Value))
        {
          if (geoModelDataId.Measure.Item2 == AggregationFunction.None)
            return (GeoModelDataId) null;
          geoModelDataId.MeasureValue = new double?();
        }
        else
        {
          if (geoModelDataId.Measure.Item2 != AggregationFunction.None)
            return (GeoModelDataId) null;
          double result;
          if (!double.TryParse(match.Groups["MsrVal"].Value, NumberStyles.Float | NumberStyles.AllowThousands, (IFormatProvider) GeoModelDataId.CultureInId, out result))
            return (GeoModelDataId) null;
          geoModelDataId.MeasureValue = new double?(result);
        }
      }
      if (noGeoValues)
      {
        if (!string.IsNullOrEmpty(match.Groups["Lat"].Value) || !string.IsNullOrEmpty(match.Groups["LatVal"].Value) || (!string.IsNullOrEmpty(match.Groups["Lon"].Value) || !string.IsNullOrEmpty(match.Groups["LonVal"].Value)) || (!string.IsNullOrEmpty(match.Groups["XCoord"].Value) || !string.IsNullOrEmpty(match.Groups["XCoordVal"].Value) || (!string.IsNullOrEmpty(match.Groups["YCoord"].Value) || !string.IsNullOrEmpty(match.Groups["YCoordVal"].Value))) || (!string.IsNullOrEmpty(match.Groups["FullAddr"].Value) || !string.IsNullOrEmpty(match.Groups["FullAddrVal"].Value) || (!string.IsNullOrEmpty(match.Groups["Old"].Value) || !string.IsNullOrEmpty(match.Groups["OldVal"].Value)) || (!string.IsNullOrEmpty(match.Groups["Addr"].Value) || !string.IsNullOrEmpty(match.Groups["AddrVal"].Value) || (!string.IsNullOrEmpty(match.Groups["Ad"].Value) || !string.IsNullOrEmpty(match.Groups["AdVal"].Value)))) || (!string.IsNullOrEmpty(match.Groups["Ad2"].Value) || !string.IsNullOrEmpty(match.Groups["Ad2Val"].Value) || (!string.IsNullOrEmpty(match.Groups["Country"].Value) || !string.IsNullOrEmpty(match.Groups["CountryVal"].Value)) || (!string.IsNullOrEmpty(match.Groups["Loc"].Value) || !string.IsNullOrEmpty(match.Groups["LocVal"].Value) || (!string.IsNullOrEmpty(match.Groups["Zip"].Value) || !string.IsNullOrEmpty(match.Groups["ZipVal"].Value)))))
          return (GeoModelDataId) null;
      }
      else if (geo is LatLongField && !geo.IsUsingXY)
      {
        string strA1 = match.Groups["Lat"].Value;
        string strA2 = match.Groups["Lon"].Value;
        if (string.IsNullOrWhiteSpace(strA1) || string.IsNullOrWhiteSpace(strA2))
          return (GeoModelDataId) null;
        if (string.Compare(strA1, geo.Latitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0 || string.Compare(strA2, geo.Longitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0)
          return (GeoModelDataId) null;
        geoModelDataId.LatitudeColumn = geo.Latitude;
        geoModelDataId.LongitudeColumn = geo.Longitude;
        if (match.Groups["LatVal"].Value == null)
        {
          geoModelDataId.Latitude = new double?();
        }
        else
        {
          double result;
          if (!double.TryParse(match.Groups["LatVal"].Value, NumberStyles.Float | NumberStyles.AllowThousands, (IFormatProvider) GeoModelDataId.CultureInId, out result))
            return (GeoModelDataId) null;
          geoModelDataId.Latitude = new double?(result);
        }
        if (match.Groups["LonVal"].Value == null)
        {
          geoModelDataId.Longitude = new double?();
        }
        else
        {
          double result;
          if (!double.TryParse(match.Groups["LonVal"].Value, NumberStyles.Float | NumberStyles.AllowThousands, (IFormatProvider) GeoModelDataId.CultureInId, out result))
            return (GeoModelDataId) null;
          geoModelDataId.Longitude = new double?(result);
        }
      }
      else if (geo is LatLongField && geo.IsUsingXY)
      {
        string strA1 = match.Groups["XCoord"].Value;
        string strA2 = match.Groups["YCoord"].Value;
        if (string.IsNullOrWhiteSpace(strA1) || string.IsNullOrWhiteSpace(strA2))
          return (GeoModelDataId) null;
        if (string.Compare(strA1, geo.Longitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0 || string.Compare(strA2, geo.Latitude.ModelQueryName, StringComparison.OrdinalIgnoreCase) != 0)
          return (GeoModelDataId) null;
        geoModelDataId.XCoordColumn = geo.Longitude;
        geoModelDataId.YCoordColumn = geo.Latitude;
        if (match.Groups["XCoordVal"].Value == null)
        {
          geoModelDataId.XCoord = new double?();
        }
        else
        {
          double result;
          if (!double.TryParse(match.Groups["XCoordVal"].Value, NumberStyles.Float | NumberStyles.AllowThousands, (IFormatProvider) GeoModelDataId.CultureInId, out result))
            return (GeoModelDataId) null;
          geoModelDataId.XCoord = new double?(result);
        }
        if (match.Groups["YCoordVal"].Value == null)
        {
          geoModelDataId.YCoord = new double?();
        }
        else
        {
          double result;
          if (!double.TryParse(match.Groups["YCoordVal"].Value, NumberStyles.Float | NumberStyles.AllowThousands, (IFormatProvider) GeoModelDataId.CultureInId, out result))
            return (GeoModelDataId) null;
          geoModelDataId.YCoord = new double?(result);
        }
      }
      else if (geo is GeoFullAddressField)
      {
        GeoFullAddressField fullAddressField = geo as GeoFullAddressField;
        string strB1 = match.Groups["FullAddr"].Value;
        string strB2 = match.Groups["Old"].Value;
        if (!(string.IsNullOrWhiteSpace(strB1) ^ string.IsNullOrWhiteSpace(strB2)))
          return (GeoModelDataId) null;
        if (!string.IsNullOrWhiteSpace(strB1))
        {
          if (fullAddressField.FullAddress == null || string.Compare(fullAddressField.FullAddress.ModelQueryName, strB1, StringComparison.OrdinalIgnoreCase) != 0)
            return (GeoModelDataId) null;
          string str = match.Groups["FullAddrVal"].Value;
          if (str == string.Empty)
            str = (string) null;
          if (str == null || !string.IsNullOrEmpty(match.Groups["OldVal"].Value))
            return (GeoModelDataId) null;
          geoModelDataId.FullAddress = str;
          geoModelDataId.FullAddressColumn = fullAddressField.FullAddress;
        }
        else
        {
          if (fullAddressField.OtherLocationDescription == null || string.Compare(fullAddressField.OtherLocationDescription.ModelQueryName, strB2, StringComparison.OrdinalIgnoreCase) != 0)
            return (GeoModelDataId) null;
          string str = match.Groups["OldVal"].Value;
          if (str == string.Empty)
            str = (string) null;
          if (str == null || !string.IsNullOrEmpty(match.Groups["FullAddrVal"].Value))
            return (GeoModelDataId) null;
          geoModelDataId.OtherLocationDescription = str;
          geoModelDataId.OtherLocationDescriptionColumn = fullAddressField.OtherLocationDescription;
        }
      }
      else
      {
        if (!(geo is GeoEntityField))
          return (GeoModelDataId) null;
        GeoEntityField geo1 = geo as GeoEntityField;
        string geoVal;
        TableColumn geoCol;
        if (!GeoModelDataId.GetGeo(geo1, GeoEntityField.GeoEntityLevel.AddressLine, match.Groups["AddrVal"].Value, match.Groups["Addr"].Value, out geoVal, out geoCol))
          return (GeoModelDataId) null;
        geoModelDataId.AddressLine = geoVal;
        geoModelDataId.AddressLineColumn = geoCol;
        if (!GeoModelDataId.GetGeo(geo1, GeoEntityField.GeoEntityLevel.AdminDistrict, match.Groups["AdVal"].Value, match.Groups["Ad"].Value, out geoVal, out geoCol))
          return (GeoModelDataId) null;
        geoModelDataId.AdminDistrict = geoVal;
        geoModelDataId.AdminDistrictColumn = geoCol;
        if (!GeoModelDataId.GetGeo(geo1, GeoEntityField.GeoEntityLevel.AdminDistrict2, match.Groups["Ad2Val"].Value, match.Groups["Ad2"].Value, out geoVal, out geoCol))
          return (GeoModelDataId) null;
        geoModelDataId.AdminDistrict2 = geoVal;
        geoModelDataId.AdminDistrict2Column = geoCol;
        if (!GeoModelDataId.GetGeo(geo1, GeoEntityField.GeoEntityLevel.Country, match.Groups["CountryVal"].Value, match.Groups["Country"].Value, out geoVal, out geoCol))
          return (GeoModelDataId) null;
        geoModelDataId.Country = geoVal;
        geoModelDataId.CountryColumn = geoCol;
        if (!GeoModelDataId.GetGeo(geo1, GeoEntityField.GeoEntityLevel.Locality, match.Groups["LocVal"].Value, match.Groups["Loc"].Value, out geoVal, out geoCol))
          return (GeoModelDataId) null;
        geoModelDataId.Locality = geoVal;
        geoModelDataId.LocalityColumn = geoCol;
        if (!GeoModelDataId.GetGeo(geo1, GeoEntityField.GeoEntityLevel.PostalCode, match.Groups["ZipVal"].Value, match.Groups["Zip"].Value, out geoVal, out geoCol))
          return (GeoModelDataId) null;
        geoModelDataId.PostalCode = geoVal;
        geoModelDataId.PostalCodeColumn = geoCol;
        foreach (TableColumn column in geo1.GeoColumns)
        {
          switch (geo1.GeoLevel(column))
          {
            case GeoEntityField.GeoEntityLevel.AddressLine:
              if (geoModelDataId.AddressLineColumn == null)
                return (GeoModelDataId) null;
              else
                continue;
            case GeoEntityField.GeoEntityLevel.Locality:
              if (geoModelDataId.LocalityColumn == null)
                return (GeoModelDataId) null;
              else
                continue;
            case GeoEntityField.GeoEntityLevel.AdminDistrict2:
              if (geoModelDataId.AdminDistrict2Column == null)
                return (GeoModelDataId) null;
              else
                continue;
            case GeoEntityField.GeoEntityLevel.AdminDistrict:
              if (geoModelDataId.AdminDistrictColumn == null)
                return (GeoModelDataId) null;
              else
                continue;
            case GeoEntityField.GeoEntityLevel.PostalCode:
              if (geoModelDataId.PostalCodeColumn == null)
                return (GeoModelDataId) null;
              else
                continue;
            case GeoEntityField.GeoEntityLevel.Country:
              if (geoModelDataId.CountryColumn == null)
                return (GeoModelDataId) null;
              else
                continue;
            default:
              return (GeoModelDataId) null;
          }
        }
      }
      return geoModelDataId;
    }

    private static bool GetGeo(GeoEntityField geo, GeoEntityField.GeoEntityLevel geoLevel, string modelVal, string modelColVal, out string geoVal, out TableColumn geoCol)
    {
      geoCol = (TableColumn) null;
      geoVal = modelVal == string.Empty ? (string) null : modelVal;
      switch (modelColVal == string.Empty ? (string) null : modelColVal)
      {
        case null:
          return geoVal == null;
        default:
          geoCol = Enumerable.FirstOrDefault<TableColumn>((IEnumerable<TableColumn>) geo.GeoColumns, (Func<TableColumn, bool>) (col => string.Compare(col.ModelQueryName, modelColVal, StringComparison.OrdinalIgnoreCase) == 0));
          if (geoCol != null)
            return geo.GeoLevel(geoCol) == geoLevel;
          else
            return false;
      }
    }
  }
}
