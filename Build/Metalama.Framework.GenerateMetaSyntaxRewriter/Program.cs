// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter
{
    internal class Program
    {
        private static int Main( string[] args )
        {
            if ( args.Length != 1 )
            {
                Console.Error.WriteLine( "Usage: GenerateMetaSyntaxRewriter.exe <targetDirectory>" );

                return 1;
            }

            var versions = new[] { "4.0.1", "4.1.0" };
            var baseDirectory = args[0];

            foreach ( var version in versions )
            {
                Generator generator = new( version, Path.Combine( baseDirectory, $".generated\\{version}" ) );
                generator.GenerateTemplateFiles( "Metalama.Framework.Engine\\MetaSyntaxRewriter.g.cs" );
                generator.GenerateHasher( "Metalama.Framework.DesignTime\\RunTimeCodeHasher.g.cs", "RunTimeCodeHasher", false );
                generator.GenerateHasher( "Metalama.Framework.DesignTime\\CompileTimeCodeHasher.g.cs", "CompileTimeCodeHasher", true );
            }

            return 0;
        }
    }
}