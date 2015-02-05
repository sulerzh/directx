using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls.Interop
{
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct PropVariant
    {
        [FieldOffset(0)]
        public ushort vt;
        [FieldOffset(2)]
        private readonly byte wReserved1;
        [FieldOffset(3)]
        private readonly byte wReserved2;
        [FieldOffset(4)]
        private readonly int wReserved3;
        [FieldOffset(8)]
        public sbyte cVal;
        [FieldOffset(8)]
        public byte bVal;
        [FieldOffset(8)]
        public short iVal;
        [FieldOffset(8)]
        public ushort uiVal;
        [FieldOffset(8)]
        public int lVal;
        [FieldOffset(8)]
        public uint ulVal;
        [FieldOffset(8)]
        public int intVal;
        [FieldOffset(8)]
        public uint uintVal;
        [FieldOffset(8)]
        public long hVal;
        [FieldOffset(8)]
        public ulong uhVal;
        [FieldOffset(8)]
        public float fltVal;
        [FieldOffset(8)]
        public double dblVal;
        [FieldOffset(8)]
        public short boolVal;
        [FieldOffset(8)]
        public int scode;
        [FieldOffset(8)]
        public long filetime;
        [FieldOffset(8)]
        public IntPtr ptr;
        [FieldOffset(8)]
        public int cElems;
        [FieldOffset(12)]
        public IntPtr pElems;
        [FieldOffset(8)]
        public int cbSize;
        [FieldOffset(12)]
        public IntPtr pBlobData;
    }
}
