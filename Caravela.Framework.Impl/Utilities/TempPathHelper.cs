// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class TempPathHelper
    {
        private static readonly string _tempPath;
        private static readonly string _version;

        static TempPathHelper()
        {
            _tempPath = Path.GetTempPath();
            _version = AssemblyMetadataReader.BuildId;
        }

        public static string GetTempPath( string purpose, Guid? guid = null )
            => Path.Combine( _tempPath, "Caravela", purpose, _version, guid?.ToString() ?? "" );
    }
}