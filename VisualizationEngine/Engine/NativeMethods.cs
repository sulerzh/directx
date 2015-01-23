// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.NativeMethods
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine
{
  internal static class NativeMethods
  {
    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr MemCpy(IntPtr dest, IntPtr src, UIntPtr count);
  }
}
