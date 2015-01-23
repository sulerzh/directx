using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.VisualizationControls.Video
{
  public static class ImageConverter
  {
    public static unsafe void RGB32ToYV12(byte[] destination, int destStride, IntPtr pSrc, int srcStride, int widthInPixels, int heightInPixels)
    {
        fixed (byte* dest = destination)
        {
            byte* srcBuffer = (byte*)pSrc.ToPointer();
            byte* destBuffer = dest;
            byte* numPtr4 = dest + destStride * heightInPixels;
            byte* numPtr5 = dest + 5 * destStride * heightInPixels / 4;
            for (int i = 0; i < heightInPixels; ++i)
            {
                RGBQUAD* quad = (RGBQUAD*)srcBuffer;
                for (int j = 0; j < widthInPixels; ++j)
                    destBuffer[j] = RGBtoY(quad[j]);
                destBuffer += destStride;
                srcBuffer += srcStride;
            }
            byte* src = (byte*)pSrc.ToPointer();
            for (int j = 0;j < heightInPixels;j += 2)
            {
                RGBQUAD* rgbquadPtr1 = (RGBQUAD*)src;
                RGBQUAD* rgbquadPtr2 = (RGBQUAD*)(src + srcStride);
                byte* numPtr7 = numPtr4;
                byte* numPtr8 = numPtr5;
                for (int k = 0; k < widthInPixels; k += 2)
                {
                    *numPtr7++ = (byte)((RGBtoV(rgbquadPtr1[k]) + RGBtoV(rgbquadPtr1[k + 1]) + RGBtoV(rgbquadPtr2[k]) + RGBtoV(rgbquadPtr2[k + 1])) / 4);
                    *numPtr8++ = (byte)((RGBtoU(rgbquadPtr1[k]) + RGBtoU(rgbquadPtr1[k + 1]) + RGBtoU(rgbquadPtr2[k]) + RGBtoU(rgbquadPtr2[k + 1])) / 4);
                }
                numPtr4 += destStride / 2;
                numPtr5 += destStride / 2;
                src += srcStride * 2;
            }
        }
    }

    public static unsafe void ParallelRGB32ToYV12(byte[] destination, int destStride, IntPtr pSrc, int srcStride, int widthInPixels, int heightInPixels)
    {
        fixed (byte* numPtr1 = destination)
        {
            byte* pSrcRow = (byte*)pSrc.ToPointer();
            byte* pDestY = numPtr1;
            Task[] taskArray = new Task[3];
            taskArray[0] = Task.Factory.StartNew((Action)(() => Parallel.For(0, heightInPixels, (Action<int>)(y =>
            {
                RGBQUAD* quad = (RGBQUAD*)(pSrcRow + y * srcStride);
                byte* cursor = pDestY + y * destStride;
                for (int i = 0; i < widthInPixels; ++i)
                    cursor[i] = RGBtoY(quad[i]);
            }))));
            byte* pDestV = numPtr1 + destStride * heightInPixels;
            byte* pDestU = numPtr1 + 5 * destStride * heightInPixels / 4;
            taskArray[1] = Task.Factory.StartNew((Action)(() => ConvertChromaParallel(true, pDestV, destStride, pSrcRow, srcStride, heightInPixels, widthInPixels)));
            taskArray[2] = Task.Factory.StartNew((Action)(() => ConvertChromaParallel(false, pDestU, destStride, pSrcRow, srcStride, heightInPixels, widthInPixels)));
            Task.WaitAll(taskArray);
        }
    }

    private static unsafe void ConvertChromaParallel(bool isV, byte* pDest, int destStride, byte* pSrcRow, int srcStride, int heightInPixels, int widthInPixels)
    {
        Parallel.For(0, heightInPixels / 2, (Action<int>)(y =>
        {
            RGBQUAD* evenRow = (RGBQUAD*)(pSrcRow + 2 * y * srcStride);
            RGBQUAD* oddRow = (RGBQUAD*)(pSrcRow + (2 * y + 1) * srcStride);
            byte* cussor = pDest + destStride * y / 2;
            int index = 0;
            while (index < widthInPixels)
            {
                *cussor++ = !isV ? 
                    (byte)((RGBtoU(evenRow[index]) + RGBtoU(evenRow[index + 1]) + RGBtoU(oddRow[index]) + RGBtoU(oddRow[index + 1])) / 4) : 
                    (byte)((RGBtoV(evenRow[index]) + RGBtoV(evenRow[index + 1]) + RGBtoV(oddRow[index]) + RGBtoV(oddRow[index + 1])) / 4);
                index += 2;
            }
        }));
    }

    private static byte RGBtoY(ImageConverter.RGBQUAD q)
    {
      return (byte) ((66 * q.R + 129 * q.G + 25 * q.B + 128 >> 8) + 16);
    }

    private static byte RGBtoU(ImageConverter.RGBQUAD q)
    {
      return (byte) ((-38 * q.R - 74 * q.G + 112 * q.B + 128 >> 8) + 128);
    }

    private static byte RGBtoV(ImageConverter.RGBQUAD q)
    {
      return (byte) ((112 * q.R - 94 * q.G - 18 * q.B + 128 >> 8) + 128);
    }

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
