using System.Collections.Generic;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class DateTimeConstants
  {
    public static int DefaultHours = 12;
    public static int TwelveHourFormat_MaxHours = 12;
    public static int TwelveHourFormat_MaxAMHour = 11;
    public static int TwelveHourFormat_MinHours = 1;
    public static int TwentyFourHourFormat_MaxHours = 23;
    public static int TwentyFourHourFormat_MinHours = 0;
    public static int MinMinutesAndSeconds = 0;
    public static int MaxMinutesAndSeconds = 60;
    public static int TwelveHourModulus = 12;
    public static string TimeDigitsFormat = "00";
    private static List<int> _hoursTwelve = new List<int>();
    private static List<int> _hoursTwentyFour = new List<int>();
    private static List<string> _minutes = new List<string>();
    private static List<string> _seconds = new List<string>();
    private static List<AMPM> _ampms = new List<AMPM>();

    public static List<int> HoursTwelve
    {
      get
      {
        return DateTimeConstants._hoursTwelve;
      }
    }

    public static List<int> HoursTwentyFour
    {
      get
      {
        return DateTimeConstants._hoursTwentyFour;
      }
    }

    public static List<string> Minutes
    {
      get
      {
        return DateTimeConstants._minutes;
      }
    }

    public static List<string> Seconds
    {
      get
      {
        return DateTimeConstants._seconds;
      }
    }

    public static List<AMPM> AMPMs
    {
      get
      {
        return DateTimeConstants._ampms;
      }
    }

    static DateTimeConstants()
    {
      DateTimeConstants._ampms.Add(AMPM.AM);
      DateTimeConstants._ampms.Add(AMPM.PM);
      for (int index = DateTimeConstants.TwelveHourFormat_MinHours; index <= DateTimeConstants.TwelveHourFormat_MaxHours; ++index)
        DateTimeConstants.HoursTwelve.Add(index);
      for (int index = DateTimeConstants.TwentyFourHourFormat_MinHours; index <= DateTimeConstants.TwentyFourHourFormat_MaxHours; ++index)
        DateTimeConstants.HoursTwentyFour.Add(index);
      for (int index = DateTimeConstants.MinMinutesAndSeconds; index < DateTimeConstants.MaxMinutesAndSeconds; ++index)
      {
        DateTimeConstants._minutes.Add(index.ToString(DateTimeConstants.TimeDigitsFormat));
        DateTimeConstants._seconds.Add(index.ToString(DateTimeConstants.TimeDigitsFormat));
      }
    }

    public static int ConvertTwelveHourToTwentyFourHour(int twelveHour, AMPM ampm)
    {
      if (ampm == AMPM.PM)
      {
        if (twelveHour == DateTimeConstants.TwelveHourModulus)
          return DateTimeConstants.TwelveHourModulus;
        else
          return twelveHour + DateTimeConstants.TwelveHourModulus;
      }
      else if (twelveHour == DateTimeConstants.TwelveHourModulus)
        return 0;
      else
        return twelveHour;
    }

    public static int ConvertTwentyFourHourToTwelveHour(int value)
    {
      if (value == 0 || value == DateTimeConstants.TwelveHourModulus)
        return DateTimeConstants.TwelveHourModulus;
      else
        return value % DateTimeConstants.TwelveHourModulus;
    }

    public static AMPM ConvertTwentyFourHourToAMPM(int value)
    {
      return value < 0 || value > DateTimeConstants.TwelveHourFormat_MaxAMHour ? AMPM.PM : AMPM.AM;
    }
  }
}
