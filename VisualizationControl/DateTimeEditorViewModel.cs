using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DateTimeEditorViewModel : ViewModelBase
  {
    private bool _CurrentCultureUsesTwentyFourHourFormat = true;
    private int _HourTwelveHour = DateTimeConstants.DefaultHours;
    private int _HourTwentyFourHour = DateTimeConstants.DefaultHours;
    private DateTime _MinimumCalendarDate = DateTime.MinValue;
    private DateTime _MaximumCalendarDate = DateTime.MaxValue;
    private bool composedDateBeingDecomposed;
    private bool _userOverridenValueforTwentyFourHourFormat;
    private AMPM _AMPM;
    private int _Minute;
    private int _Second;
    private DateTime _CalendarDate;
    private DateTime _ComposedDate;

    public string PropertyCurrentCultureUsesTwentyFourHourFormat
    {
      get
      {
        return "CurrentCultureUsesTwentyFourHourFormat";
      }
    }

    public bool CurrentCultureUsesTwentyFourHourFormat
    {
      get
      {
        if (this._userOverridenValueforTwentyFourHourFormat)
          return this._CurrentCultureUsesTwentyFourHourFormat;
        else
          return CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Contains("H");
      }
      set
      {
        this._userOverridenValueforTwentyFourHourFormat = true;
        this.SetProperty<bool>(this.PropertyCurrentCultureUsesTwentyFourHourFormat, ref this._CurrentCultureUsesTwentyFourHourFormat, value, false);
      }
    }

    public string PropertyAMPM
    {
      get
      {
        return "AMPM";
      }
    }

    public AMPM AMPM
    {
      get
      {
        return this._AMPM;
      }
      set
      {
        if (!this.SetProperty<AMPM>(this.PropertyAMPM, ref this._AMPM, value, false))
          return;
        this.HourTwentyFourHour = DateTimeConstants.ConvertTwelveHourToTwentyFourHour(this.HourTwelveHour, this.AMPM);
      }
    }

    public string PropertyHourTwelveHour
    {
      get
      {
        return "HourTwelveHour";
      }
    }

    public int HourTwelveHour
    {
      get
      {
        return this._HourTwelveHour;
      }
      set
      {
        if (!this.SetProperty<int>(this.PropertyHourTwelveHour, ref this._HourTwelveHour, value, false))
          return;
        this.HourTwentyFourHour = DateTimeConstants.ConvertTwelveHourToTwentyFourHour(value, this.AMPM);
      }
    }

    public string PropertyHourTwentyFourHour
    {
      get
      {
        return "HourTwentyFourHour";
      }
    }

    public int HourTwentyFourHour
    {
      get
      {
        return this._HourTwentyFourHour;
      }
      set
      {
        if (!this.SetProperty<int>(this.PropertyHourTwentyFourHour, ref this._HourTwentyFourHour, value, false))
          return;
        this.AMPM = DateTimeConstants.ConvertTwentyFourHourToAMPM(value);
        this.HourTwelveHour = DateTimeConstants.ConvertTwentyFourHourToTwelveHour(value);
      }
    }

    public string PropertyMinute
    {
      get
      {
        return "Minute";
      }
    }

    public int Minute
    {
      get
      {
        return this._Minute;
      }
      set
      {
        this.SetProperty<int>(this.PropertyMinute, ref this._Minute, value, false);
      }
    }

    public string PropertySecond
    {
      get
      {
        return "Second";
      }
    }

    public int Second
    {
      get
      {
        return this._Second;
      }
      set
      {
        this.SetProperty<int>(this.PropertySecond, ref this._Second, value, false);
      }
    }

    public string PropertyCalendarDate
    {
      get
      {
        return "CalendarDate";
      }
    }

    public DateTime CalendarDate
    {
      get
      {
        return this._CalendarDate;
      }
      set
      {
        this.SetProperty<DateTime>(this.PropertyCalendarDate, ref this._CalendarDate, value, false);
      }
    }

    public string PropertyMinimumCalendarDate
    {
      get
      {
        return "MinimumCalendarDate";
      }
    }

    public DateTime MinimumCalendarDate
    {
      get
      {
        return this._MinimumCalendarDate;
      }
      set
      {
        this.SetProperty<DateTime>(this.PropertyMinimumCalendarDate, ref this._MinimumCalendarDate, value, false);
      }
    }

    public string PropertyMaximumCalendarDate
    {
      get
      {
        return "MaximumCalendarDate";
      }
    }

    public DateTime MaximumCalendarDate
    {
      get
      {
        return this._MaximumCalendarDate;
      }
      set
      {
        this.SetProperty<DateTime>(this.PropertyMaximumCalendarDate, ref this._MaximumCalendarDate, value, false);
      }
    }

    public string PropertyComposedDate
    {
      get
      {
        return "ComposedDate";
      }
    }

    public DateTime ComposedDate
    {
      get
      {
        return this._ComposedDate;
      }
      set
      {
        if (!this.SetProperty<DateTime>(this.PropertyComposedDate, ref this._ComposedDate, value, false))
          return;
        this.composedDateBeingDecomposed = true;
        this.CalendarDate = value;
        this.HourTwentyFourHour = value.Hour;
        this.Minute = value.Minute;
        this.Second = value.Second;
        this.composedDateBeingDecomposed = false;
      }
    }

    public DateTimeEditorViewModel()
    {
      this.PropertyChanged += new PropertyChangedEventHandler(this.DateTimeEditorViewModel_PropertyChanged);
    }

    private void DateTimeEditorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.composedDateBeingDecomposed || !(e.PropertyName != this.PropertyComposedDate) || (!(e.PropertyName != this.PropertyMinimumCalendarDate) || !(e.PropertyName != this.PropertyMaximumCalendarDate)))
        return;
      this.ComposedDate = new DateTime(this.CalendarDate.Year, this.CalendarDate.Month, this.CalendarDate.Day, this.HourTwentyFourHour, this.Minute, this.Second);
    }
  }
}
