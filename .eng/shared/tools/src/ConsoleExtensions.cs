// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using System;
using System.CommandLine.IO;

namespace PostSharp.Engineering.BuildTools
{
    internal static class ConsoleExtensions
    {
        public static void WriteLine( this IStandardStreamWriter writer, string message )
        {
            writer.Write( message + Environment.NewLine );
        }
    }
}
