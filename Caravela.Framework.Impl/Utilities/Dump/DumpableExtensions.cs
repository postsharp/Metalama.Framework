// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Utilities.Dump
{
    internal static class DumpableExtensions
    {
        public static object ToDumpImpl( this IDumpable obj ) => ObjectDumper.Dump( obj )!;
    }
}