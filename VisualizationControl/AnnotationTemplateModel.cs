using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class AnnotationTemplateModel : CompositePropertyChangeNotificationBase
  {
    private AnnotationImageSize _ImageSize = AnnotationImageSize.Medium;
    private Color _BackgroundColor;
    private RichTextModel _Title;
    private string _TitleField;
    private AggregationFunction? _TitleAF;
    private RichTextModel _Description;
    private RichTextModel _FieldFormat;
    private AnnotationDescriptionType _DescriptionType;
    private BitmapSource _OriginalImage;

    public static string PropertyBackgroundColor
    {
      get
      {
        return "BackgroundColor";
      }
    }

    public Color BackgroundColor
    {
      get
      {
        return this._BackgroundColor;
      }
      set
      {
        base.SetProperty<Color>(AnnotationTemplateModel.PropertyBackgroundColor, ref this._BackgroundColor, value);
      }
    }

    public static string PropertyTitle
    {
      get
      {
        return "Title";
      }
    }

    public RichTextModel Title
    {
      get
      {
        return this._Title;
      }
      set
      {
        base.SetProperty<RichTextModel>(AnnotationTemplateModel.PropertyTitle, ref this._Title, value);
      }
    }

    public static string PropertyTitleField
    {
      get
      {
        return "TitleField";
      }
    }

    public string TitleField
    {
      get
      {
        return this._TitleField;
      }
      set
      {
        base.SetProperty<string>(AnnotationTemplateModel.PropertyTitleField, ref this._TitleField, value);
      }
    }

    public static string PropertyTitleAF
    {
      get
      {
        return "TitleAF";
      }
    }

    public AggregationFunction? TitleAF
    {
      get
      {
        return this._TitleAF;
      }
      set
      {
        base.SetProperty<AggregationFunction?>(AnnotationTemplateModel.PropertyTitleAF, ref this._TitleAF, value);
      }
    }

    public static string PropertyDescription
    {
      get
      {
        return "Description";
      }
    }

    public RichTextModel Description
    {
      get
      {
        return this._Description;
      }
      set
      {
        base.SetProperty<RichTextModel>(AnnotationTemplateModel.PropertyDescription, ref this._Description, value);
      }
    }

    public static string PropertyFieldFormat
    {
      get
      {
        return "FieldFormat";
      }
    }

    public RichTextModel FieldFormat
    {
      get
      {
        return this._FieldFormat;
      }
      set
      {
        base.SetProperty<RichTextModel>(AnnotationTemplateModel.PropertyFieldFormat, ref this._FieldFormat, value);
      }
    }

    public static string PropertyDescriptionType
    {
      get
      {
        return "DescriptionType";
      }
    }

    public AnnotationDescriptionType DescriptionType
    {
      get
      {
        return this._DescriptionType;
      }
      set
      {
        base.SetProperty<AnnotationDescriptionType>(AnnotationTemplateModel.PropertyDescriptionType, ref this._DescriptionType, value);
      }
    }

    public static string PropertyImageSize
    {
      get
      {
        return "ImageSize";
      }
    }

    public AnnotationImageSize ImageSize
    {
      get
      {
        return this._ImageSize;
      }
      set
      {
        if (!base.SetProperty<AnnotationImageSize>(AnnotationTemplateModel.PropertyImageSize, ref this._ImageSize, value))
          return;
        this.RaisePropertyChanged(AnnotationTemplateModel.PropertyImage);
      }
    }

    [XmlIgnore]
    public BitmapSource OriginalImage
    {
      get
      {
        return this._OriginalImage;
      }
      set
      {
        base.SetProperty<BitmapSource>(AnnotationTemplateModel.PropertyImage, ref this._OriginalImage, value);
      }
    }

    public static string PropertyImage
    {
      get
      {
        return "Image";
      }
    }

    [XmlIgnore]
    public BitmapSource Image
    {
      get
      {
        if (this._OriginalImage == null)
          return (BitmapSource) null;
        int num1 = 0;
        switch (this._ImageSize)
        {
          case AnnotationImageSize.Small:
            num1 = 64;
            break;
          case AnnotationImageSize.Medium:
            num1 = 128;
            break;
          case AnnotationImageSize.Large:
            num1 = 256;
            break;
          case AnnotationImageSize.XLarge:
            num1 = 512;
            break;
        }
        double width = this._OriginalImage.Width;
        double height = this._OriginalImage.Height;
        if (width <= (double) num1 && height <= (double) num1)
          return this._OriginalImage;
        double num2 = (double) num1 / width;
        double num3 = (double) num1 / height;
        double num4 = num2 < num3 ? num2 : num3;
        TransformedBitmap transformedBitmap = new TransformedBitmap();
        transformedBitmap.BeginInit();
        transformedBitmap.Source = this._OriginalImage;
        transformedBitmap.Transform = (Transform) new ScaleTransform(num4, num4);
        transformedBitmap.EndInit();
        return (BitmapSource) transformedBitmap;
      }
    }

    [XmlElement("Image")]
    public string Base64Image
    {
      get
      {
        if (this._OriginalImage == null)
          return (string) null;
        else
          return AnnotationTemplateModel.ImageToBase64(this._OriginalImage);
      }
      set
      {
        if (value != null)
          this._OriginalImage = AnnotationTemplateModel.Base64ToImage(value);
        else
          this._OriginalImage = (BitmapSource) null;
      }
    }

    public ObservableCollectionEx<string> NamesOfColumnsToDisplay { get; set; }

    public ObservableCollectionEx<AggregationFunction?> ColumnAggregationFunctions { get; set; }

    [XmlIgnore]
    public ObservableCollectionEx<string> FormattedFieldDisplayStrings { get; set; }

    public AnnotationTemplateModel()
    {
      this.Title = new RichTextModel()
      {
        FormatType = RichTextFormatType.Static,
        FontSize = 16,
        TextTemplate = Resources.AnnotationTitleTemplate
      };
      this.Description = new RichTextModel()
      {
        FormatType = RichTextFormatType.Static,
        FontSize = 12
      };
      this.Description.PropertyChanged += (PropertyChangedEventHandler) ((s, e) => this.FieldFormat.CopyProperties(this.Description));
      this.FieldFormat = new RichTextModel()
      {
        FormatType = RichTextFormatType.Template,
        FontSize = 12,
        TextTemplate = Resources.AnnotationFieldTemplate
      };
      this.FieldFormat.PropertyChanged += (PropertyChangedEventHandler) ((s, e) => this.Description.CopyProperties(this.FieldFormat));
      this.NamesOfColumnsToDisplay = new ObservableCollectionEx<string>();
      this.ColumnAggregationFunctions = new ObservableCollectionEx<AggregationFunction?>();
      this.FormattedFieldDisplayStrings = new ObservableCollectionEx<string>();
    }

    private static string ImageToBase64(BitmapSource bitmap)
    {
      if (bitmap == null)
        return string.Empty;
      PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
      BitmapFrame bitmapFrame = BitmapFrame.Create(bitmap);
      pngBitmapEncoder.Frames.Add(bitmapFrame);
      using (MemoryStream memoryStream = new MemoryStream())
      {
        pngBitmapEncoder.Save((Stream) memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
      }
    }

    private static BitmapSource Base64ToImage(string base64)
    {
      if (string.IsNullOrEmpty(base64))
        return (BitmapSource) null;
      using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(base64)))
        return (BitmapSource) BitmapFrame.Create((Stream) memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
    }

    public void Apply(DataRowModel model)
    {
      if (model == null)
        return;
      if (this.Title.FormatType == RichTextFormatType.Template)
      {
        string valueForField = model.GetValueForField(this.TitleField, this.TitleAF);
        this.Title.UpdateFormattedText((object) model.GetColumnName(this.TitleField, this.TitleAF), (object) valueForField);
      }
      this.FormattedFieldDisplayStrings.Clear();
      int index = 0;
      foreach (string fieldName in (Collection<string>) this.NamesOfColumnsToDisplay)
      {
        string valueForField = model.GetValueForField(fieldName, this.ColumnAggregationFunctions[index]);
        if (valueForField != null)
          this.FormattedFieldDisplayStrings.Add(this.FieldFormat.UpdateFormattedText((object) model.GetColumnName(fieldName, this.ColumnAggregationFunctions[index]), (object) valueForField));
        ++index;
      }
    }

    public AnnotationTemplateModel Clone()
    {
      return new AnnotationTemplateModel()
      {
        BackgroundColor = this.BackgroundColor,
        Description = this.Description.Clone(),
        DescriptionType = this.DescriptionType,
        ImageSize = this.ImageSize,
        FieldFormat = this.FieldFormat.Clone(),
        FormattedFieldDisplayStrings = this.FormattedFieldDisplayStrings.Clone(),
        NamesOfColumnsToDisplay = this.NamesOfColumnsToDisplay.Clone(),
        ColumnAggregationFunctions = this.ColumnAggregationFunctions.Clone(),
        Title = this.Title.Clone(),
        TitleField = this.TitleField,
        TitleAF = this.TitleAF,
        Base64Image = this.Base64Image
      };
    }

    public void Repair()
    {
      while (this.NamesOfColumnsToDisplay.Count > this.ColumnAggregationFunctions.Count)
        this.NamesOfColumnsToDisplay.RemoveAt(this.ColumnAggregationFunctions.Count);
      while (this.ColumnAggregationFunctions.Count > this.NamesOfColumnsToDisplay.Count)
        this.ColumnAggregationFunctions.RemoveAt(this.NamesOfColumnsToDisplay.Count);
    }

    internal void Shutdown()
    {
      this.RemoveSubscriptions();
    }
  }
}
