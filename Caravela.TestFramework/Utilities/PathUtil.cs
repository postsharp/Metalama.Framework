// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Caravela.TestFramework.Utilities
{
    internal static class PathUtil
    {
        /// <summary>
        /// This should emulate <c>Path.GetRelativePath</c>, which is not available in .NET Standard.
        /// </summary>
        public static string GetRelativePath( string relativeTo, string path )
        {
            if ( Directory.Exists( relativeTo ) )
            {
                relativeTo += Path.DirectorySeparatorChar;
            }

            var relativeUri = new Uri( relativeTo ).MakeRelativeUri( new Uri( path ) );

            return relativeUri.OriginalString.Replace( '/', Path.DirectorySeparatorChar );
        }
    }
}