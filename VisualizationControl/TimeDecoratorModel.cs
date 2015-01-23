using System;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class TimeDecoratorModel : DecoratorContentBase
  {
    private static Color DefaultTextColor = Color.FromArgb(byte.MaxValue, (byte) 64, (byte) 64, (byte) 64);
    private RichTextModel _text = new RichTextModel()
    {
      FontSize = 26,
      Color = TimeDecoratorModel.DefaultTextColor
    };
    private string _Format = "g";
    private DateTime _Time;

    public string PropertyText
    {
      get
      {
        return "Text";
      }
    }

    public RichTextModel Text
    {
      get
      {
        return this._text;
      }
      set
      {
        base.SetProperty<RichTextModel>(this.PropertyText, ref this._text, value);
      }
    }

    public string PropertyTime
    {
      get
      {
        return "Time";
      }
    }

    public DateTime Time
    {
      get
      {
        return this._Time;
      }
      set
      {
        this.SetProperty<DateTime>(this.PropertyTime, ref this._Time, value, new Action(this.Refresh));
      }
    }

    public string PropertyFormat
    {
      get
      {
        return "Format";
      }
    }

    public string Format
    {
      get
      {
        return this._Format;
      }
      set
      {
        this.SetProperty<string>(this.PropertyFormat, ref this._Format, value, new Action(this.Refresh));
      }
    }

    public TimeDecoratorModel Clone()
    {
      return new TimeDecoratorModel()
      {
        Text = this.Text.Clone(),
        Time = this.Time,
        Format = this.Format
      };
    }

    private void Refresh()
    {
      try
      {
        this.Text.Text = this.Time.ToString(this.Format);
      }
      catch
      {
        this.Text.Text = this.Time.ToString();
      }
    }
  }
}
