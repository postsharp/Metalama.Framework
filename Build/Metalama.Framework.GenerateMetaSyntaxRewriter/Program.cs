// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.GenerateMetaSyntaxRewriter
{
    internal class Program
    {
        private static void Main()
        {
            Generator generator = new();
            generator.GenerateTemplateFiles();
            generator.GenerateHasher( "RunTimeCodeHasher.g.cs", "RunTimeCodeHasher", false );
            generator.GenerateHasher( "CompileTimeCodeHasher.g.cs", "CompileTimeCodeHasher", true );
        }
    }
}