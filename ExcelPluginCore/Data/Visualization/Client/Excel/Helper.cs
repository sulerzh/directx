using stdole;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Microsoft.Data.Visualization.Client.Excel
{
    internal class Helper
    {
        internal class BitmapSourceConverter
        {
            private const short _PictureTypeBitmap = 1;

            [DllImport("oleaut32.dll", CharSet = CharSet.Ansi)]
            private static extern int OleCreatePictureIndirect([In] Helper.BitmapSourceConverter.PictDescBitmap pictdesc, ref Guid iid, bool fOwn, [MarshalAs(UnmanagedType.Interface)] out object ppVoid);

            public static IPictureDisp Base64ImageToPictureDisp(string base64)
            {
                if (string.IsNullOrEmpty(base64))
                    return null;
                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    BitmapFrame bitmapFrame = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    if (bitmapFrame.CanFreeze)
                        bitmapFrame.Freeze();
                    IPictureDisp pictureDisp = Helper.BitmapSourceConverter.BitmapSourceToPictureDisp(bitmapFrame);
                    return pictureDisp;
                }
            }

            public static IPictureDisp BitmapSourceToPictureDisp(BitmapSource bitmapsource)
            {
                if (bitmapsource == null)
                    return null;
                Bitmap bitmap;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                    bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapsource));
                    bitmapEncoder.Save(memoryStream);
                    bitmap = new Bitmap(memoryStream);
                }
                Helper.BitmapSourceConverter.PictDescBitmap pictdesc = new Helper.BitmapSourceConverter.PictDescBitmap(bitmap);
                object ppVoid;
                Guid guid = typeof(IPictureDisp).GUID;
                Helper.BitmapSourceConverter.OleCreatePictureIndirect(pictdesc, ref guid, true, out ppVoid);
                IPictureDisp pictureDisp = (IPictureDisp)ppVoid;
                return pictureDisp;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal class PictDescBitmap
            {
                internal int cbSizeOfStruct = Marshal.SizeOf(typeof(Helper.BitmapSourceConverter.PictDescBitmap));
                internal int pictureType = 1;
                internal IntPtr hBitmap = IntPtr.Zero;
                internal IntPtr hPalette = IntPtr.Zero;
                internal int unused;

                internal PictDescBitmap(Bitmap bitmap)
                {
                    this.hBitmap = bitmap.GetHbitmap();
                }
            }
        }
    }
}
