using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class GeocodingReportViewModel : DialogViewModelBase
  {
    private static readonly int DefaultPageSize = 50;
    public string PropertyTotallyConfidentName = "TotallyConfident";
    private bool _HasAddress;
    private bool _HasOther;
    private bool _HasCountry;
    private bool _HasState;
    private bool _HasCounty;
    private bool _HasCity;
    private bool _HasZipcode;
    private bool _HasStreet;
    private string _TruncationWarning;
    private string _Confidence;
    private string _SubTitle;
    private string _LayerName;
    private bool _TotallyConfident;

    public string PropertyHasAddress
    {
      get
      {
        return "HasAddress";
      }
    }

    public bool HasAddress
    {
      get
      {
        return this._HasAddress;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasAddress, ref this._HasAddress, value, false);
      }
    }

    public string PropertyHasOther
    {
      get
      {
        return "HasOther";
      }
    }

    public bool HasOther
    {
      get
      {
        return this._HasOther;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasOther, ref this._HasOther, value, false);
      }
    }

    public string PropertyHasCountry
    {
      get
      {
        return "HasCountry";
      }
    }

    public bool HasCountry
    {
      get
      {
        return this._HasCountry;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasCountry, ref this._HasCountry, value, false);
      }
    }

    public string PropertyHasState
    {
      get
      {
        return "HasState";
      }
    }

    public bool HasState
    {
      get
      {
        return this._HasState;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasState, ref this._HasState, value, false);
      }
    }

    public string PropertyHasCounty
    {
      get
      {
        return "HasCounty";
      }
    }

    public bool HasCounty
    {
      get
      {
        return this._HasCounty;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasCounty, ref this._HasCounty, value, false);
      }
    }

    public string PropertyHasCity
    {
      get
      {
        return "HasCity";
      }
    }

    public bool HasCity
    {
      get
      {
        return this._HasCity;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasCity, ref this._HasCity, value, false);
      }
    }

    public string PropertyHasZipcode
    {
      get
      {
        return "HasZipcode";
      }
    }

    public bool HasZipcode
    {
      get
      {
        return this._HasZipcode;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasZipcode, ref this._HasZipcode, value, false);
      }
    }

    public string PropertyHasStreet
    {
      get
      {
        return "HasStreet";
      }
    }

    public bool HasStreet
    {
      get
      {
        return this._HasStreet;
      }
      set
      {
        this.SetProperty<bool>(this.PropertyHasStreet, ref this._HasStreet, value, false);
      }
    }

    public string PropertyTruncationWarning
    {
      get
      {
        return "TruncationWarning";
      }
    }

    public string TruncationWarning
    {
      get
      {
        return this._TruncationWarning;
      }
      set
      {
        this.SetProperty<string>(this.PropertyTruncationWarning, ref this._TruncationWarning, value, false);
      }
    }

    public string PropertyConfidence
    {
      get
      {
        return "Confidence";
      }
    }

    public string Confidence
    {
      get
      {
        return this._Confidence;
      }
      set
      {
        base.SetProperty<string>(this.PropertyConfidence, ref this._Confidence, value, new Action(this.RefreshSubtitle));
      }
    }

    public string PropertySubTitle
    {
      get
      {
        return "SubTitle";
      }
    }

    public string SubTitle
    {
      get
      {
        return this._SubTitle;
      }
      set
      {
        this.SetProperty<string>(this.PropertySubTitle, ref this._SubTitle, value, false);
      }
    }

    public string PropertyLayerName
    {
      get
      {
        return "LayerName";
      }
    }

    public string LayerName
    {
      get
      {
        return this._LayerName;
      }
      set
      {
        base.SetProperty<string>(this.PropertyLayerName, ref this._LayerName, value, new Action(this.RefreshSubtitle));
      }
    }

    private List<GeoAmbiguity> Ambiguities { get; set; }

    public PagingListCollectionView EntriesViewSource { get; private set; }

    public ObservableCollectionEx<DelegatedCommand> Commands { get; private set; }

    public bool TotallyConfident
    {
      get
      {
        return this._TotallyConfident;
      }
      set
      {
        base.SetProperty<bool>(this.PropertyTotallyConfidentName, ref this._TotallyConfident, value, new Action(this.RefreshSubtitle));
      }
    }

    public GeocodingReportViewModel(List<GeoAmbiguity> ambiguities = null, float confidencePercentage = 1f)
    {
      this.Commands = new ObservableCollectionEx<DelegatedCommand>();
      this.Commands.Add(new DelegatedCommand(new Action(this.OnExecuteOk))
      {
        Name = Resources.Dialog_OkayText
      });
      this.TotallyConfident = (double) confidencePercentage >= 1.0;
      this.Title = Resources.GeocodingReport_Title;
      this.Description = this.TotallyConfident ? (string) null : Resources.GeocodingReport_Description;
      this.Ambiguities = ambiguities;
      if (this.Ambiguities != null && !this.TotallyConfident)
      {
        this.EntriesViewSource = new PagingListCollectionView((IList) ambiguities, GeocodingReportViewModel.DefaultPageSize);
        this.EntriesViewSource.SortDescriptions.Add(new SortDescription("ResolutionType", ListSortDirection.Ascending));
      }
      this.RefreshColumnVisibility();
      this.Confidence = (double) confidencePercentage <= 1.0 ? string.Format(Resources.GeocodingConfidencePercentageFormat, (object) ((int) (100.0 * (double) confidencePercentage)).ToString()) : (string) null;
    }

    private void OnExecuteOk()
    {
      this.CancelCommand.Execute((object) null);
    }

    private void RefreshColumnVisibility()
    {
      this.HasCountry = false;
      this.HasCity = false;
      this.HasState = false;
      this.HasCounty = false;
      this.HasStreet = false;
      this.HasZipcode = false;
      this.HasAddress = false;
      this.HasOther = false;
      if (this.Ambiguities == null)
        return;
      foreach (GeoAmbiguity geoAmbiguity in this.Ambiguities)
      {
        if (!string.IsNullOrEmpty(geoAmbiguity.Country))
          this.HasCountry = true;
        if (!string.IsNullOrEmpty(geoAmbiguity.AdminDistrict))
          this.HasState = true;
        if (!string.IsNullOrEmpty(geoAmbiguity.AdminDistrict2))
          this.HasCounty = true;
        if (!string.IsNullOrEmpty(geoAmbiguity.Locality))
          this.HasCity = true;
        if (!string.IsNullOrEmpty(geoAmbiguity.PostalCode))
          this.HasZipcode = true;
        if (!string.IsNullOrEmpty(geoAmbiguity.AddressLine))
          this.HasStreet = true;
        if (!string.IsNullOrEmpty(geoAmbiguity.AddressOrOther))
        {
          GeoFullAddressField fullAddressField = geoAmbiguity.GeoField as GeoFullAddressField;
          if (fullAddressField != null && fullAddressField.GeoColumns.Count != 0)
          {
            if (object.ReferenceEquals((object) fullAddressField.GeoColumns[0], (object) fullAddressField.FullAddress))
              this.HasAddress = true;
            else
              this.HasOther = true;
          }
        }
        if (this.HasCountry && this.HasState && (this.HasCounty && this.HasCity) && (this.HasZipcode && this.HasStreet) || (this.HasAddress || this.HasOther))
          break;
      }
    }

    private void RefreshSubtitle()
    {
      if (this.TotallyConfident)
        this.SubTitle = string.Format(Resources.GeocodingReport_TotallyConfidentSubTitle, (object) this.LayerName);
      else
        this.SubTitle = string.Format(Resources.GeocodingReport_SubTitle, (object) this.Confidence, (object) this.LayerName);
    }
  }
}
