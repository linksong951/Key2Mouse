using System.Runtime.InteropServices;

namespace Key2Mouse.Native;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}