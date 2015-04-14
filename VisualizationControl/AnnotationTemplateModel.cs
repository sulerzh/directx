using Microsoft.Data.Visualization.VisualizationCommon;
using System;
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
                return _BackgroundColor;
            }
            set
            {
                base.SetProperty(PropertyBackgroundColor, ref _BackgroundColor, value);
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
                return _Title;
            }
            set
            {
                base.SetProperty(PropertyTitle, ref _Title, value);
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
                return _TitleField;
            }
            set
            {
                base.SetProperty(PropertyTitleField, ref _TitleField, value);
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
                return _TitleAF;
            }
            set
            {
                base.SetProperty(PropertyTitleAF, ref _TitleAF, value);
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
                return _Description;
            }
            set
            {
                base.SetProperty(PropertyDescription, ref _Description, value);
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
                return _FieldFormat;
            }
            set
            {
                base.SetProperty(PropertyFieldFormat, ref _FieldFormat, value);
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
                return _DescriptionType;
            }
            set
            {
                base.SetProperty(PropertyDescriptionType, ref _DescriptionType, value);
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
                return _ImageSize;
            }
            set
            {
                if (!base.SetProperty(PropertyImageSize, ref _ImageSize, value))
                    return;
                RaisePropertyChanged(PropertyImage);
            }
        }

        [XmlIgnore]
        public BitmapSource OriginalImage
        {
            get
            {
                return _OriginalImage;
            }
            set
            {
                base.SetProperty(PropertyImage, ref _OriginalImage, value);
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
                if (_OriginalImage == null)
                    return null;
                int num1 = 0;
                switch (_ImageSize)
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
                double width = _OriginalImage.Width;
                double height = _OriginalImage.Height;
                if (width <= num1 && height <= num1)
                    return _OriginalImage;
                double num2 = num1 / width;
                double num3 = num1 / height;
                double num4 = num2 < num3 ? num2 : num3;
                TransformedBitmap transformedBitmap = new TransformedBitmap();
                transformedBitmap.BeginInit();
                transformedBitmap.Source = _OriginalImage;
                transformedBitmap.Transform = new ScaleTransform(num4, num4);
                transformedBitmap.EndInit();
                return transformedBitmap;
            }
        }

        [XmlElement("Image")]
        public string Base64Image
        {
            get
            {
                if (_OriginalImage == null)
                    return null;
                return ImageToBase64(_OriginalImage);
            }
            set
            {
                if (value != null)
                    _OriginalImage = Base64ToImage(value);
                else
                    _OriginalImage = null;
            }
        }

        public ObservableCollectionEx<string> NamesOfColumnsToDisplay { get; set; }

        public ObservableCollectionEx<AggregationFunction?> ColumnAggregationFunctions { get; set; }

        [XmlIgnore]
        public ObservableCollectionEx<string> FormattedFieldDisplayStrings { get; set; }

        public AnnotationTemplateModel()
        {
            Title = new RichTextModel()
            {
                FormatType = RichTextFormatType.Static,
                FontSize = 16,
                TextTemplate = Resources.AnnotationTitleTemplate
            };
            Description = new RichTextModel()
            {
                FormatType = RichTextFormatType.Static,
                FontSize = 12
            };
            Description.PropertyChanged += (PropertyChangedEventHandler)((s, e) => FieldFormat.CopyProperties(Description));
            FieldFormat = new RichTextModel()
            {
                FormatType = RichTextFormatType.Template,
                FontSize = 12,
                TextTemplate = Resources.AnnotationFieldTemplate
            };
            FieldFormat.PropertyChanged += (PropertyChangedEventHandler)((s, e) => Description.CopyProperties(FieldFormat));
            NamesOfColumnsToDisplay = new ObservableCollectionEx<string>();
            ColumnAggregationFunctions = new ObservableCollectionEx<AggregationFunction?>();
            FormattedFieldDisplayStrings = new ObservableCollectionEx<string>();
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
                pngBitmapEncoder.Save(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        private static BitmapSource Base64ToImage(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return null;
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(base64)))
                return BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        public void Apply(DataRowModel model)
        {
            if (model == null)
                return;
            if (Title.FormatType == RichTextFormatType.Template)
            {
                string valueForField = model.GetValueForField(TitleField, TitleAF);
                Title.UpdateFormattedText((object)model.GetColumnName(TitleField, TitleAF), (object)valueForField);
            }
            FormattedFieldDisplayStrings.Clear();
            int index = 0;
            foreach (string fieldName in NamesOfColumnsToDisplay)
            {
                string valueForField = model.GetValueForField(fieldName, ColumnAggregationFunctions[index]);
                if (valueForField != null)
                    FormattedFieldDisplayStrings.Add(FieldFormat.UpdateFormattedText((object)model.GetColumnName(fieldName, ColumnAggregationFunctions[index]), (object)valueForField));
                ++index;
            }
        }

        public AnnotationTemplateModel Clone()
        {
            return new AnnotationTemplateModel()
            {
                BackgroundColor = BackgroundColor,
                Description = Description.Clone(),
                DescriptionType = DescriptionType,
                ImageSize = ImageSize,
                FieldFormat = FieldFormat.Clone(),
                FormattedFieldDisplayStrings = FormattedFieldDisplayStrings.Clone(),
                NamesOfColumnsToDisplay = NamesOfColumnsToDisplay.Clone(),
                ColumnAggregationFunctions = ColumnAggregationFunctions.Clone(),
                Title = Title.Clone(),
                TitleField = TitleField,
                TitleAF = TitleAF,
                Base64Image = Base64Image
            };
        }

        public void Repair()
        {
            while (NamesOfColumnsToDisplay.Count > ColumnAggregationFunctions.Count)
                NamesOfColumnsToDisplay.RemoveAt(ColumnAggregationFunctions.Count);
            while (ColumnAggregationFunctions.Count > NamesOfColumnsToDisplay.Count)
                ColumnAggregationFunctions.RemoveAt(NamesOfColumnsToDisplay.Count);
        }

        internal void Shutdown()
        {
            RemoveSubscriptions();
        }
    }
}
