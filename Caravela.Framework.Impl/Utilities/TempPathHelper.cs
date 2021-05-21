// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class TempPathHelper
    {
        public static string GetTempPath( string purpose, Guid? guid = null )
            => Path.Combine( Path.GetTempPath(), "Caravela", purpose, AssemblyMetadataReader.MainVersionId, guid?.ToString() ?? "" );
    }
}