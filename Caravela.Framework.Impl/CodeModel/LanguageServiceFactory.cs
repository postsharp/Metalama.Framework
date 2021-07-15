// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Editing;
using System;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static partial class LanguageServiceFactory
    {
        public static AnnotatingSyntaxGenerator CSharpSyntaxGenerator { get; }

        static LanguageServiceFactory()
        {
            var version =
                typeof(LanguageServiceFactory).Assembly.GetReferencedAssemblies()
                    .Single( a => string.Equals( a.Name, "Microsoft.CodeAnalysis.Workspaces", StringComparison.OrdinalIgnoreCase ) )
                    .Version;

            var assembly = Assembly.Load( "Microsoft.CodeAnalysis.CSharp.Workspaces, Version=" + version );

            var type = assembly.GetType( $"Microsoft.CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator" )!;
            var field = type.GetField( "Instance", BindingFlags.Public | BindingFlags.Static )!;
            var syntaxGenerator = (SyntaxGenerator) field.GetValue( null );
            CSharpSyntaxGenerator = new AnnotatingSyntaxGenerator( syntaxGenerator );
        }
    }
}