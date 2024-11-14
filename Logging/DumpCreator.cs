using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

public class DumpCreator
{
    [Flags]
    public enum Typ : uint
    {
        // Add the MiniDump flags you need, for example:
        MiniDumpNormal = 0x00000000,
        MiniDumpWithDataSegs = 0x00000001,
        // etc.
    }

    [DllImport("DbgHelp.dll")]
    public static extern bool MiniDumpWriteDump(IntPtr hProcess, uint ProcessId, SafeHandle hFile, Typ DumpType,
        IntPtr ExceptionParam, IntPtr UserStreamParam, IntPtr CallbackParam);
}
