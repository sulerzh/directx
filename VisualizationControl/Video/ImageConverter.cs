using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.VisualizationControls.Video
{
    /// <summary>
    /// Image Stride
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa473780(v=vs.85).aspx
    /// </summary>
    public static class ImageConverter
    {
        public static unsafe void RGB32ToYV12(byte[] destination, int destStride, IntPtr pSrc, int srcStride, int widthInPixels, int heightInPixels)
        {
            fixed (byte* pDest = destination)
            {
                byte* pSrcRow = (byte*)pSrc.ToPointer();
                byte* pDestY = pDest;

                // Calculate the offsets for the V and U planes.

                // In YV12, each chroma plane has half the stride and half the height  
                // as the Y plane.
                byte* pDestV = pDest + (destStride * heightInPixels);
                byte* pDestU = pDest + 
                    (destStride * heightInPixels) + 
                    (destStride * heightInPixels) / 4;

                // Convert the Y plane.
                for (int y = 0; y < heightInPixels; ++y)
                {
                    RGBQUAD* pSrcPixel = (RGBQUAD*)pSrcRow;
                    for (int x = 0; x < widthInPixels; ++x)
                        pDestY[x] = RGBtoY(pSrcPixel[x]); // Y0
                    pDestY += destStride;
                    pSrcRow += srcStride;
                }

                // Convert the V and U planes.

                // YV12 is a 4:2:0 format, so each chroma sample is derived from four 
                // RGB pixels.
                byte* pSrcRow1 = (byte*)pSrc.ToPointer();
                for (int y = 0; y < heightInPixels; y += 2)
                {
                    RGBQUAD* pSrcPixel = (RGBQUAD*)pSrcRow1;
                    RGBQUAD* pNextSrcRow = (RGBQUAD*)(pSrcRow1 + srcStride);
                    byte* pbV = pDestV;
                    byte* pbU = pDestU;
                    for (int x = 0; x < widthInPixels; x += 2)
                    {
                        // Use a simple average to downsample the chroma.
                        *pbV++ = (byte)((
                            RGBtoV(pSrcPixel[x]) + 
                            RGBtoV(pSrcPixel[x + 1]) +
                            RGBtoV(pNextSrcRow[x]) + 
                            RGBtoV(pNextSrcRow[x + 1])) / 4);
                        *pbU++ = (byte)((
                            RGBtoU(pSrcPixel[x]) + 
                            RGBtoU(pSrcPixel[x + 1]) + 
                            RGBtoU(pNextSrcRow[x]) + 
                            RGBtoU(pNextSrcRow[x + 1])) / 4);
                    }
                    pDestV += destStride / 2;
                    pDestU += destStride / 2;

                    // Skip two lines on the source image.
                    pSrcRow1 += srcStride * 2;
                }
            }
        }

        public static unsafe void ParallelRGB32ToYV12(byte[] destination, int destStride, IntPtr pSrc, int srcStride, int widthInPixels, int heightInPixels)
        {
            fixed (byte* pDest = destination)
            {
                byte* pSrcRow = (byte*)pSrc.ToPointer();
                byte* pDestY = pDest;
                // YUV planes Convert Tasks
                Task[] taskArray = new Task[3];
                taskArray[0] = Task.Factory.StartNew(
                    (Action)(() => Parallel.For(0, heightInPixels, 
                        (y =>
                        {
                            RGBQUAD* pSrcPixel = (RGBQUAD*)(pSrcRow + y * srcStride);
                            byte* pDestPixel = pDestY + y*destStride;
                            for (int x = 0; x < widthInPixels; ++x)
                                pDestPixel[x] = RGBtoY(pSrcPixel[x]);
                        }))));
                byte* pDestV = pDest + (destStride * heightInPixels);
                byte* pDestU = pDest + 
                    (destStride * heightInPixels) +
                    (destStride * heightInPixels) / 4;
                taskArray[1] = Task.Factory.StartNew(
                    (() => ConvertChromaParallel(true, pDestV, destStride, pSrcRow, srcStride, heightInPixels, widthInPixels)));
                taskArray[2] = Task.Factory.StartNew(
                    (() => ConvertChromaParallel(false, pDestU, destStride, pSrcRow, srcStride, heightInPixels, widthInPixels)));
                Task.WaitAll(taskArray);
            }
        }

        private static unsafe void ConvertChromaParallel(bool isV, byte* pDest, int destStride, byte* pSrcRow, int srcStride, int heightInPixels, int widthInPixels)
        {
            Parallel.For(0, heightInPixels/2,
                (y =>
                {
                    RGBQUAD* pSrcPixel = (RGBQUAD*) (pSrcRow + 2*y*srcStride);
                    RGBQUAD* pNextSrcRow = (RGBQUAD*) (pSrcRow + (2*y + 1)*srcStride);
                    byte* pbDestPlane = pDest + destStride*y/2;
                    int index = 0;
                    while (index < widthInPixels)
                    {
                        *pbDestPlane++ = !isV
                            ? (byte)
                                ((RGBtoU(pSrcPixel[index]) + RGBtoU(pSrcPixel[index + 1]) + RGBtoU(pNextSrcRow[index]) +
                                  RGBtoU(pNextSrcRow[index + 1]))/4)
                            : (byte)
                                ((RGBtoV(pSrcPixel[index]) + RGBtoV(pSrcPixel[index + 1]) + RGBtoV(pNextSrcRow[index]) +
                                  RGBtoV(pNextSrcRow[index + 1]))/4);
                        index += 2;
                    }
                }));
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/aa917087.aspx
        /// </summary>
        #region Converting Between YUV and RGB

        private static byte RGBtoY(RGBQUAD q)
        {
            return (byte)((66 * q.R + 129 * q.G + 25 * q.B + 128 >> 8) + 16);
        }

        private static byte RGBtoU(RGBQUAD q)
        {
            return (byte)((-38 * q.R - 74 * q.G + 112 * q.B + 128 >> 8) + 128);
        }

        private static byte RGBtoV(RGBQUAD q)
        {
            return (byte)((112 * q.R - 94 * q.G - 18 * q.B + 128 >> 8) + 128);
        }

        #endregion

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/dd162938(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct RGBQUAD
        {
            [FieldOffset(0)]
            public byte B;
            [FieldOffset(1)]
            public byte G;
            [FieldOffset(2)]
            public byte R;
            [FieldOffset(3)]
            public byte A;
        }
    }
}
