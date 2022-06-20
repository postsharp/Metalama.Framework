// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Metalama.Framework.Engine.CompileTime
{
    public static class CompileTimeConstants
    {
        public static string GetPrefixedSyntaxTreeName( string name ) => "(" + name + ")";

        public static bool IsPredefinedSyntaxTree( string path )
        {
            var fileName = Path.GetFileNameWithoutExtension( path );

            return fileName.StartsWith( "(", StringComparison.Ordinal ) && fileName.EndsWith( ")", StringComparison.Ordinal );
        }

        public const string CompileTimeProjectResourceName = "Metalama.CompileTimeProject";

        public const string InheritableAspectManifestResourceName = "Metalama.InheritableAspects";
    }
}