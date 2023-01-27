// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.GenerateMetaSyntaxRewriter.Model;
using System;
using System.IO;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter
{
    internal static class Program
    {
        private static int Main( string[] args )
        {
            if ( args.Length != 1 )
            {
                Console.Error.WriteLine( "Usage: GenerateMetaSyntaxRewriter.exe <targetDirectory>" );

                return 1;
            }

            var versionNames = new[] { "4.0.1", "4.4.0" };
            var baseDirectory = args[0];

            var syntaxDocuments = new SyntaxDocument[versionNames.Length];

            for ( var versionIndex = 0; versionIndex < versionNames.Length; versionIndex++ )
            {
                var version = new RoslynVersion( versionNames[versionIndex], versionIndex );
                syntaxDocuments[versionIndex] = new SyntaxDocument( version );
            }

            VersionDetector.DetectVersions( syntaxDocuments );

            foreach ( var syntax in syntaxDocuments )
            {
                Generator generator = new( syntax, Path.Combine( baseDirectory, $".generated\\{syntax.Version.Name}" ) );
                generator.GenerateRoslynApiVersionEnum( "Metalama.Framework.Engine\\RoslynApiVersion.g.cs", syntaxDocuments );
                generator.GenerateTemplateFiles( "Metalama.Framework.Engine\\MetaSyntaxRewriter.g.cs", syntaxDocuments );
                generator.GenerateVersionChecker( "Metalama.Framework.Engine\\RoslynVersionSyntaxVerifier.g.cs" );
                generator.GenerateHasher( "Metalama.Framework.DesignTime\\RunTimeCodeHasher.g.cs", "RunTimeCodeHasher", false );
                generator.GenerateHasher( "Metalama.Framework.DesignTime\\CompileTimeCodeHasher.g.cs", "CompileTimeCodeHasher", true );
            }

            return 0;
        }
    }
}