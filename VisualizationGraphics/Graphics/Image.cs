using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Data.Visualization.DirectX.WindowsImagingComponent;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class Image : DisposableResource
    {
        public PixelFormat Format { get; private set; }

        public int PixelSizeInBytes { get; private set; }

        public IntPtr Data { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Image(IntPtr data, int width, int height, PixelFormat format)
        {
            this.Data = data;
            this.Width = width;
            this.Height = height;
            this.Format = format;
            switch (format)
            {
                case PixelFormat.Rgba32Bpp:
                case PixelFormat.Bgra32Bpp:
                case PixelFormat.Prgba32Bpp:
                case PixelFormat.Bgr32Bpp:
                case PixelFormat.Rg16Unorm32Bpp:
                case PixelFormat.Float32Bpp:
                    this.PixelSizeInBytes = 4;
                    break;
                case PixelFormat.Rgb24Bpp:
                case PixelFormat.Bgr24Bpp:
                    this.PixelSizeInBytes = 3;
                    break;
                case PixelFormat.Gray8Bpp:
                case PixelFormat.Alpha8Bpp:
                    this.PixelSizeInBytes = 1;
                    break;
                case PixelFormat.R16Unorm16bpp:
                case PixelFormat.Float16Bpp:
                    this.PixelSizeInBytes = 2;
                    break;
            }
        }

        public Image(Stream stream)
        {
            using (ImagingFactory imagingFactory = ImagingFactory.Create())
            {
                using (BitmapDecoder decoderFromStream = imagingFactory.CreateDecoderFromStream(stream, DecodeMetadataCacheOption.OnLoad))
                {
                    using (BitmapFrameDecode frame = decoderFromStream.GetFrame(0U))
                    {
                        BitmapSource source = null;
                        try
                        {
                            source = frame.ToBitmapSource();
                            Guid guid = (Guid)frame.GetPixelFormat();
                            if (guid != PixelFormats.Gray8Bpp && guid != PixelFormats.Alpha8Bpp)
                            {
                                if (guid != PixelFormats.Bgr24Bpp && guid != PixelFormats.Rgb24Bpp &&
                                    guid != PixelFormats.Bgr32Bpp && guid != PixelFormats.Bgra32Bpp &&
                                    guid != PixelFormats.Prgba32Bpp && guid != PixelFormats.Indexed4Bpp &&
                                    guid != PixelFormats.Indexed8Bpp && guid != PixelFormats.Indexed1Bpp &&
                                    guid != PixelFormats.Indexed2Bpp)
                                    return;
                                Guid rgba32Bpp = PixelFormats.Rgba32Bpp;
                                using (FormatConverter formatConverter = imagingFactory.CreateFormatConverter())
                                {
                                    formatConverter.Initialize(source, rgba32Bpp, BitmapDitherType.None,
                                        BitmapPaletteType.MedianCut);
                                    source.Dispose();
                                    source = formatConverter.ToBitmapSource();
                                    formatConverter.Dispose();
                                }
                            }
                            this.SetPixelFormat(source.PixelFormat);
                            this.Data = source.CopyPixelsToMemory();
                            this.Width = (int)source.Size.Width;
                            this.Height = (int)source.Size.Height;
                        }
                        finally
                        {
                            source.Dispose();
                        }
                    }
                }
            }
        }

        private void SetPixelFormat(Guid wicFormat)
        {
            if (wicFormat == PixelFormats.Rgba32Bpp)
            {
                this.Format = PixelFormat.Rgba32Bpp;
                this.PixelSizeInBytes = 4;
            }
            else if (wicFormat == PixelFormats.Bgra32Bpp)
            {
                this.Format = PixelFormat.Bgra32Bpp;
                this.PixelSizeInBytes = 4;
            }
            else if (wicFormat == PixelFormats.Prgba32Bpp)
            {
                this.Format = PixelFormat.Prgba32Bpp;
                this.PixelSizeInBytes = 4;
            }
            else if (wicFormat == PixelFormats.Rgb24Bpp)
            {
                this.Format = PixelFormat.Rgb24Bpp;
                this.PixelSizeInBytes = 3;
            }
            else if (wicFormat == PixelFormats.Bgr24Bpp)
            {
                this.Format = PixelFormat.Bgr24Bpp;
                this.PixelSizeInBytes = 3;
            }
            else if (wicFormat == PixelFormats.Bgr32Bpp)
            {
                this.Format = PixelFormat.Bgr32Bpp;
                this.PixelSizeInBytes = 4;
            }
            else if (wicFormat == PixelFormats.Gray8Bpp)
            {
                this.Format = PixelFormat.Gray8Bpp;
                this.PixelSizeInBytes = 1;
            }
            else if (wicFormat == PixelFormats.Alpha8Bpp)
            {
                this.Format = PixelFormat.Alpha8Bpp;
                this.PixelSizeInBytes = 1;
            }
            else
            {
                this.Format = PixelFormat.Unknown;
                this.PixelSizeInBytes = 0;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!(this.Data != IntPtr.Zero))
                return;
            Marshal.FreeHGlobal(this.Data);
            this.Data = IntPtr.Zero;
        }
    }
}
