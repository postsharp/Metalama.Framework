// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class SyntaxGeneratorFactory
    {
        public static SyntaxGenerator NullObliviousSyntaxGenerator { get; }
        public static SyntaxGenerator DefaultSyntaxGenerator { get; }

        public static SyntaxGenerator GetSyntaxGenerator( bool nullableContext ) => nullableContext ? DefaultSyntaxGenerator : NullObliviousSyntaxGenerator;

        private Compilation _compilation;

        
        public SyntaxGenerator GetSyntaxGenerator( SyntaxTree syntaxTree, int position )
        {
            var semanticModel = this._compilation.GetSemanticModel( syntaxTree );
            var nullableContext = semanticModel.GetNullableContext( position );

            return (nullableContext & NullableContext.AnnotationsEnabled) != 0 ? DefaultSyntaxGenerator : NullObliviousSyntaxGenerator;
        }


        static SyntaxGeneratorFactory()
        {
            var version =
                typeof(SyntaxGeneratorFactory).Assembly.GetReferencedAssemblies()
                    .Single( a => string.Equals( a.Name, "Microsoft.CodeAnalysis.Workspaces", StringComparison.OrdinalIgnoreCase ) )
                    .Version;

            var assembly = Assembly.Load( "Microsoft.CodeAnalysis.CSharp.Workspaces, Version=" + version );

            var type = assembly.GetType( $"Microsoft.CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator" )!;
            var field = type.GetField( "Instance", BindingFlags.Public | BindingFlags.Static )!;
            var syntaxGenerator = (Microsoft.CodeAnalysis.Editing.SyntaxGenerator) field.GetValue( null );
            DefaultSyntaxGenerator = new SyntaxGenerator( syntaxGenerator, true );
            NullObliviousSyntaxGenerator = new SyntaxGenerator( syntaxGenerator, false );
        }
    }
}