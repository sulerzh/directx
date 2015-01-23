using Microsoft.Data.Visualization.VisualizationCommon;
using System.Collections.Generic;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class RichTextModel : CompositePropertyChangeNotificationBase
  {
    private int _FontSize = 12;
    private string _FontFamily = "Segoe UI";
    private string _FontStyle = "Normal";
    private string _FontWeight = "Normal";
    private Color _Color = Colors.Black;
    public static KeyValuePair<Color, string> DefaultTextColor = new KeyValuePair<Color, string>(Color.FromArgb(byte.MaxValue, (byte) 69, (byte) 69, (byte) 69), Resources.Color_Black);
    private static ICollection<KeyValuePair<Color, string>> _textColors;
    private RichTextFormatType _FormatType;
    private string _Text;
    private string _TextTemplate;
    private object[] _lastTextFormatParams;

    public static List<int> FontSizes { get; private set; }

    public static ICollection<KeyValuePair<Color, string>> TextColors
    {
      get
      {
        if (RichTextModel._textColors == null)
        {
          RichTextModel._textColors = (ICollection<KeyValuePair<Color, string>>) new List<KeyValuePair<Color, string>>();
          RichTextModel._textColors.Add(RichTextModel.DefaultTextColor);
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.DarkRed, Resources.Color_DarkRed));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Red, Resources.Color_Red));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.OrangeRed, Resources.Color_OrangeRed));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Orange, Resources.Color_Orange));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Yellow, Resources.Color_Yellow));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.YellowGreen, Resources.Color_YellowGreen));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Green, Resources.Color_Green));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Teal, Resources.Color_Teal));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Blue, Resources.Color_Blue));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Navy, Resources.Color_Navy));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.Violet, Resources.Color_Violet));
          RichTextModel._textColors.Add(new KeyValuePair<Color, string>(Colors.White, Resources.Color_White));
        }
        return RichTextModel._textColors;
      }
    }

    public static string PropertyFormatType
    {
      get
      {
        return "FormatType";
      }
    }

    public RichTextFormatType FormatType
    {
      get
      {
        return this._FormatType;
      }
      set
      {
        base.SetProperty<RichTextFormatType>(RichTextModel.PropertyFormatType, ref this._FormatType, value);
      }
    }

    public static string PropertyText
    {
      get
      {
        return "Text";
      }
    }

    public string Text
    {
      get
      {
        return this._Text;
      }
      set
      {
        base.SetProperty<string>(RichTextModel.PropertyText, ref this._Text, value);
      }
    }

    public static string PropertyTextTemplate
    {
      get
      {
        return "TextTemplate";
      }
    }

    public string TextTemplate
    {
      get
      {
        return this._TextTemplate;
      }
      set
      {
        if (!base.SetProperty<string>(RichTextModel.PropertyTextTemplate, ref this._TextTemplate, value))
          return;
        this.RefreshFormattedText();
      }
    }

    public static string PropertyFontSize
    {
      get
      {
        return "FontSize";
      }
    }

    public int FontSize
    {
      get
      {
        return this._FontSize;
      }
      set
      {
        base.SetProperty<int>(RichTextModel.PropertyFontSize, ref this._FontSize, value);
      }
    }

    public static string PropertyFontFamily
    {
      get
      {
        return "FontFamily";
      }
    }

    public string FontFamily
    {
      get
      {
        return this._FontFamily;
      }
      set
      {
        base.SetProperty<string>(RichTextModel.PropertyFontFamily, ref this._FontFamily, value);
      }
    }

    public static string PropertyFontStyle
    {
      get
      {
        return "FontStyle";
      }
    }

    public string FontStyle
    {
      get
      {
        return this._FontStyle;
      }
      set
      {
        base.SetProperty<string>(RichTextModel.PropertyFontStyle, ref this._FontStyle, value);
      }
    }

    public static string PropertyFontWeight
    {
      get
      {
        return "FontWeight";
      }
    }

    public string FontWeight
    {
      get
      {
        return this._FontWeight;
      }
      set
      {
        base.SetProperty<string>(RichTextModel.PropertyFontWeight, ref this._FontWeight, value);
      }
    }

    public static string PropertyColor
    {
      get
      {
        return "Color";
      }
    }

    public Color Color
    {
      get
      {
        return this._Color;
      }
      set
      {
        base.SetProperty<Color>(RichTextModel.PropertyColor, ref this._Color, value);
      }
    }

    static RichTextModel()
    {
      RichTextModel.FontSizes = new List<int>();
      RichTextModel.FontSizes.Add(8);
      RichTextModel.FontSizes.Add(9);
      RichTextModel.FontSizes.Add(10);
      RichTextModel.FontSizes.Add(11);
      RichTextModel.FontSizes.Add(12);
      RichTextModel.FontSizes.Add(14);
      RichTextModel.FontSizes.Add(16);
      RichTextModel.FontSizes.Add(18);
      RichTextModel.FontSizes.Add(20);
      RichTextModel.FontSizes.Add(22);
      RichTextModel.FontSizes.Add(24);
      RichTextModel.FontSizes.Add(26);
      RichTextModel.FontSizes.Add(28);
      RichTextModel.FontSizes.Add(36);
      RichTextModel.FontSizes.Add(48);
      RichTextModel.FontSizes.Add(72);
    }

    public string UpdateFormattedText(params object[] parameters)
    {
      this._lastTextFormatParams = parameters;
      this.RefreshFormattedText();
      return this.Text;
    }

    private void RefreshFormattedText()
    {
      if (this.FormatType != RichTextFormatType.Template)
        return;
      if (this._lastTextFormatParams != null && this.TextTemplate != null)
        this.Text = string.Format(this.TextTemplate, this._lastTextFormatParams);
      else
        this.Text = this.TextTemplate;
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      else
        return this.Equals(obj as RichTextModel);
    }

    public bool Equals(RichTextModel other)
    {
      if (other == null)
        return false;
      if (this.FormatType == RichTextFormatType.Static)
      {
        if (this.Color == other.Color && this.FontFamily == other.FontFamily && (this.FontSize == other.FontSize && this.FontStyle == other.FontStyle) && (this.FontWeight == other.FontWeight && this.FormatType == other.FormatType))
          return this.Text == other.Text;
        else
          return false;
      }
      else if (this.Color == other.Color && this.FontFamily == other.FontFamily && (this.FontSize == other.FontSize && this.FontStyle == other.FontStyle) && (this.FontWeight == other.FontWeight && this.FormatType == other.FormatType))
        return this.TextTemplate == other.TextTemplate;
      else
        return false;
    }

    public void CopyProperties(RichTextModel source)
    {
      if (source == null)
        return;
      if (source.FontFamily != this.FontFamily)
        this.FontFamily = source.FontFamily;
      if (source.FontSize != this.FontSize)
        this.FontSize = source.FontSize;
      if (source.FontStyle != this.FontStyle)
        this.FontStyle = source.FontStyle;
      if (source.FontWeight != this.FontWeight)
        this.FontWeight = source.FontWeight;
      if (!(source.Color != this.Color))
        return;
      this.Color = source.Color;
    }

    internal RichTextModel Clone()
    {
      RichTextModel richTextModel = new RichTextModel()
      {
        Color = this.Color,
        FontFamily = this.FontFamily,
        FontSize = this.FontSize,
        FontStyle = this.FontStyle,
        FontWeight = this.FontWeight,
        FormatType = this.FormatType,
        Text = this.Text,
        TextTemplate = this.TextTemplate
      };
      richTextModel._lastTextFormatParams = this._lastTextFormatParams;
      return richTextModel;
    }
  }
}
