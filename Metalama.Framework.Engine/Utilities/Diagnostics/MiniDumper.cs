// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Metalama.Framework.Engine.Utilities;

public static class MiniDumper
{
    [StructLayout( LayoutKind.Sequential, Pack = 4 )] // Pack=4 is important! So it works also for x64!
    private struct MiniDumpExceptionInformation
    {
        public uint ThreadId;
        public IntPtr ExceptionPointers;

        [MarshalAs( UnmanagedType.Bool )]
        public bool ClientPointers;
    }

    [DllImport(
        "dbghelp.dll",
        EntryPoint = "MiniDumpWriteDump",
        CallingConvention = CallingConvention.StdCall,
        CharSet = CharSet.Unicode,
        ExactSpelling = true,
        SetLastError = true )]
    private static extern bool MiniDumpWriteDump(
        IntPtr hProcess,
        uint processId,
        IntPtr hFile,
        uint dumpType,
        ref MiniDumpExceptionInformation expParam,
        IntPtr userStreamParam,
        IntPtr callbackParam );

    [DllImport( "kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true )]
    private static extern uint GetCurrentThreadId();

    [DllImport( "kernel32.dll", EntryPoint = "GetCurrentProcess", ExactSpelling = true )]
    private static extern IntPtr GetCurrentProcess();

    [DllImport( "kernel32.dll", EntryPoint = "GetCurrentProcessId", ExactSpelling = true )]
    private static extern uint GetCurrentProcessId();

    public static bool Write( string? fileName = null, MiniDumpKind kind = MiniDumpKind.MiniDumpWithFullMemory, IntPtr exceptionPointers = default )
    {
        if ( fileName == null )
        {
            var directory = TempPathHelper.GetTempPath( "CrashReports" );

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            fileName = Path.Combine( directory, $"{Guid.NewGuid()}.dmp" );
        }

        Console.WriteLine( $"Saving a dump to '{fileName}.'" );

        using ( var file = new FileStream( fileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
        {
            MiniDumpExceptionInformation exp = default;
            exp.ThreadId = GetCurrentThreadId();
            exp.ClientPointers = false;
            exp.ExceptionPointers = exceptionPointers;

            var bRet = MiniDumpWriteDump(
                GetCurrentProcess(),
                GetCurrentProcessId(),
                file.SafeFileHandle!.DangerousGetHandle(),
                (uint) kind,
                ref exp,
                IntPtr.Zero,
                IntPtr.Zero );

            return bRet;
        }
    }
}