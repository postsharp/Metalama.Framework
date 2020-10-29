using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    class CompileTimeAssemblyBuilder
    {
        private static readonly ImmutableArray<string> _preservedReferenceNames = new[]
        {
            "Caravela.Reactive.dll",
            "Caravela.Framework.dll",
            "Caravela.Framework.Sdk.dll",
            // TODO: should these be here?
            "Microsoft.CodeAnalysis.dll",
            "Microsoft.CodeAnalysis.CSharp.dll"
        }.ToImmutableArray();

        private static readonly IEnumerable<MetadataReference> _netStandardReferences;

        static CompileTimeAssemblyBuilder()
        {
            // TODO: make NuGetPackageRoot MSBuild property compiler-visible and use that here?
            string netstandardDirectoryPath = Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), ".nuget/packages", "netstandard.library/2.0.3/build/netstandard2.0/ref" );
            _netStandardReferences = new[] { "netstandard.dll", "System.Runtime.dll" }
                .Select( name => MetadataReference.CreateFromFile( Path.Combine( netstandardDirectoryPath, name ) ) )
                .ToArray();
        }

        private readonly ISymbolClassifier _symbolClassifier;

        public CompileTimeAssemblyBuilder( ISymbolClassifier symbolClassifier ) => this._symbolClassifier = symbolClassifier;

        public Compilation? CreateCompileTimeAssembly( Compilation compilation )
        {
            // TODO: if rewriter doesn't find any compile-time types, return null?

            compilation = new Rewriter( this._symbolClassifier, compilation ).VisitAllTrees( compilation );

            compilation = compilation.WithOptions( compilation.Options.WithDeterministic( true ).WithOutputKind( OutputKind.DynamicallyLinkedLibrary ) );

            var preservedReferences = compilation.References
                .Where( r => r is PortableExecutableReference { FilePath: var path } && _preservedReferenceNames.Contains( Path.GetFileName( path ) ) );


            compilation = compilation.WithReferences( _netStandardReferences.Concat( preservedReferences ) );

            // TODO: adjust compilation references correctly

            // TODO: produce better errors when there's an incorrect reference from compile-time code to non-compile-time symbol

            return compilation;
        }

        class Rewriter : CSharpSyntaxRewriter
        {
            private readonly ISymbolClassifier _symbolClassifier;
            private readonly Compilation _compilation;

            public Rewriter( ISymbolClassifier symbolClassifier, Compilation compilation )
            {
                this._symbolClassifier = symbolClassifier;
                this._compilation = compilation;
            }

            private SymbolDeclarationScope GetSymbolDeclarationScope(MemberDeclarationSyntax node)
            {
                var symbol = this._compilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node );
                return this._symbolClassifier.GetSymbolDeclarationScope( symbol );
            }

            // TODO: assembly and module-level attributes, incl. assembly version attribute

            // TODO: remove invalid usings
            public override SyntaxNode? VisitUsingDirective( UsingDirectiveSyntax node ) => node;

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitClassDeclaration );
            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitStructDeclaration );
            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitInterfaceDeclaration );
            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitRecordDeclaration );

            private T? VisitTypeDeclaration<T>( T node, Func<T, SyntaxNode?> baseVisit ) where T : TypeDeclarationSyntax =>
                this.GetSymbolDeclarationScope( node ) switch
                {
                    SymbolDeclarationScope.Default or SymbolDeclarationScope.RunTimeOnly => null,
                    SymbolDeclarationScope.CompileTimeOnly => (T?) baseVisit( node )
                };

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) == SymbolDeclarationScope.Template )
                {
                    throw new NotImplementedException( "TODO: call the template compiler here?" );
                }
                else
                {
                    return base.VisitMethodDeclaration( node );
                }
            }

            // TODO: top-level statements?
        }
    }
}
