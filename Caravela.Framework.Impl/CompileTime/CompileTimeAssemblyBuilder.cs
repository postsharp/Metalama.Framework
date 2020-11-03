using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    class CompileTimeAssemblyBuilder
    {
        // TODO: remove this, since it's not used anymore
        private static readonly ImmutableArray<string> _preservedReferenceNames = new string[] { }.ToImmutableArray();

        private static readonly IEnumerable<MetadataReference> _fixedReferences;

        static CompileTimeAssemblyBuilder()
        {
            // TODO: make NuGetPackageRoot MSBuild property compiler-visible and use that here?
            string nugetDirectory = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), ".nuget/packages" );

            string netstandardDirectory = Path.Combine( nugetDirectory, "netstandard.library/2.0.3/build/netstandard2.0/ref" );
            var netStandardPaths = new[] { "netstandard.dll", "System.Runtime.dll" }.Select( name => Path.Combine( netstandardDirectory, name ) );

            // Note: references to Roslyn assemblies can't be simply preserved, because they might have the wrong TFM
            // TODO: check that the path exists and if not, restore the package?
            // TODO: do not hardcode the versions?
            string microsoftCSharpVersion = "4.7.0";
            string roslynVersion = "3.8.0-5.final";
            string immutableCollectionsVersion = "5.0.0-preview.8.20407.11";
            var nugetPaths = new (string package, string version, string assembly)[]
            {
                ("microsoft.csharp", microsoftCSharpVersion, "Microsoft.CSharp.dll"),
                ("microsoft.codeanalysis.common", roslynVersion, "Microsoft.CodeAnalysis.dll"),
                ("microsoft.codeanalysis.csharp", roslynVersion, "Microsoft.CodeAnalysis.CSharp.dll"),
                ("system.collections.immutable", immutableCollectionsVersion, "System.Collections.Immutable.dll")
            }
            .Select( x => $"{nugetDirectory}/{x.package}/{x.version}/lib/netstandard2.0/{x.assembly}" );

            var caravelaAssemblies = new[]
            {
                "Caravela.Reactive.dll",
                "Caravela.Framework.dll",
                "Caravela.Framework.Sdk.dll",
                "Caravela.Framework.Impl.dll"
            };
            var caravelaPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Select( a => a.Location )
                .Where( path => caravelaAssemblies.Contains( Path.GetFileName( path ) ) );

            _fixedReferences = netStandardPaths.Concat( nugetPaths ).Concat( caravelaPaths )
                .Select( path => MetadataReference.CreateFromFile( path ) ).ToImmutableArray();
        }

        private readonly ISymbolClassifier _symbolClassifier;
        private readonly TemplateCompiler _templateCompiler;

        private (Compilation compilation, MemoryStream compileTimeAssembly)? _previousCompilation;

        public CompileTimeAssemblyBuilder( ISymbolClassifier symbolClassifier, TemplateCompiler templateCompiler )
        {
            this._symbolClassifier = symbolClassifier;
            this._templateCompiler = templateCompiler;
        }

        // TODO: creating the compile-time assembly like this means it can't use aspects itself; should it be able to use aspects?
        private Compilation? CreateCompileTimeAssembly( Compilation compilation )
        {
            compilation = new DisableRoslynExTracking().VisitAllTrees( compilation );

            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter( this._symbolClassifier, this._templateCompiler, compilation );
            compilation = produceCompileTimeCodeRewriter.VisitAllTrees( compilation );

            if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
                return null;

            compilation = compilation.WithOptions( compilation.Options.WithDeterministic( true ).WithOutputKind( OutputKind.DynamicallyLinkedLibrary ) );

            var preservedReferences = compilation.References
                .Where( r => r is PortableExecutableReference { FilePath: var path } && _preservedReferenceNames.Contains( Path.GetFileName( path ) ) );

            compilation = compilation.WithReferences( _fixedReferences.Concat( preservedReferences ) );

            compilation = new RemoveInvalidUsingsRewriter( compilation ).VisitAllTrees( compilation );

            // TODO: adjust compilation references correctly

            // TODO: produce better errors when there's an incorrect reference from compile-time code to non-compile-time symbol

            return compilation;
        }

        private static MemoryStream Emit( Compilation compilation )
        {
            var stream = new MemoryStream();

#if DEBUG
            compilation = compilation.WithOptions( compilation.Options.WithOptimizationLevel( OptimizationLevel.Debug ) );

            var options = new EmitOptions( debugInformationFormat: DebugInformationFormat.Embedded );
            var embeddedTexts = compilation.SyntaxTrees.Select( tree => EmbeddedText.FromSource( tree.FilePath, tree.GetText() ) );

            var result = compilation.Emit( stream, options: options, embeddedTexts: embeddedTexts );
#else
            var result = compilation.Emit( stream );
#endif

            if ( !result.Success )
            {
                throw new DiagnosticsException( result.Diagnostics );
            }

            stream.Position = 0;

            return stream;
        }

        public MemoryStream? EmitCompileTimeAssembly( Compilation compilation )
        {
            if ( compilation == this._previousCompilation?.compilation )
            {
                var lastStream = this._previousCompilation.Value.compileTimeAssembly;
                lastStream.Position = 0;
                return lastStream;
            }

            var compileTimeCompilation = this.CreateCompileTimeAssembly( compilation );

            if ( compileTimeCompilation == null )
                return null;

            var stream = Emit( compileTimeCompilation );

            this._previousCompilation = (compilation, stream);

            return stream;
        }

        public string GetResourceName( string assemblyName ) => $"Caravela.{assemblyName}.CompileTimeAssembly";

        // TransformationCompiler uses annotations in a way that is not compatible with RoslynEx tree tracking
        // and since tree tracking is not necessary here, disable it by removing its annotation
        class DisableRoslynExTracking : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
            {
                node = node.WithoutAnnotations( "RoslynEx.Tracking" );

                return base.VisitCompilationUnit( node );
            }
        }

        class ProduceCompileTimeCodeRewriter : CSharpSyntaxRewriter
        {
            private readonly ISymbolClassifier _symbolClassifier;
            private readonly TemplateCompiler _templateCompiler;
            private readonly Compilation _compilation;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter( ISymbolClassifier symbolClassifier, TemplateCompiler templateCompiler, Compilation compilation)
            {
                this._symbolClassifier = symbolClassifier;
                this._templateCompiler = templateCompiler;
                this._compilation = compilation;
            }

            private SymbolDeclarationScope GetSymbolDeclarationScope(MemberDeclarationSyntax node)
            {
                var symbol = this._compilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;
                return this._symbolClassifier.GetSymbolDeclarationScope( symbol );
            }

            private bool _addTemplateUsings;
            private static readonly SyntaxList<UsingDirectiveSyntax> _templateUsings = SyntaxFactory.ParseCompilationUnit( @"
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Impl.Templating.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
" ).Usings;

            public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
            {
                node = (CompilationUnitSyntax)base.VisitCompilationUnit( node )!;

                // TODO: handle namespaces properly
                if ( this._addTemplateUsings )
                {
                    // add all template usings, unless such using is already in the list
                    var usingsToAdd = _templateUsings.Where( tu => !node.Usings.Any( u => u.IsEquivalentTo( tu ) ) );

                    node = node.AddUsings( usingsToAdd.ToArray() );
                }

                return node;
            }

            // TODO: assembly and module-level attributes, incl. assembly version attribute

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitClassDeclaration );
            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitStructDeclaration );
            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitInterfaceDeclaration );
            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node, base.VisitRecordDeclaration );

            private T? VisitTypeDeclaration<T>( T node, Func<T, SyntaxNode?> baseVisit ) where T : TypeDeclarationSyntax
            {
                switch ( this.GetSymbolDeclarationScope( node ) )
                {
                    case SymbolDeclarationScope.Default or SymbolDeclarationScope.RunTimeOnly:
                        return null;

                    case SymbolDeclarationScope.CompileTimeOnly:
                        this.FoundCompileTimeCode = true;
                        return (T?) baseVisit( node );

                    default:
                        throw new NotImplementedException();
                }
            }

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) == SymbolDeclarationScope.Template )
                {
                    // TODO: report diagnostics
                    var diagnostics = new List<Diagnostic>();
                    bool success =
                        this._templateCompiler.TryCompile( node, this._compilation.GetSemanticModel( node.SyntaxTree ), diagnostics, out _, out var transformedNode );

                    Debug.Assert( success || diagnostics.Any( d => d.Severity >= DiagnosticSeverity.Error ) );

                    if ( success )
                        this._addTemplateUsings = true;

                    return transformedNode;
                }
                else
                {
                    return base.VisitMethodDeclaration( node );
                }
            }

            // TODO: top-level statements?
        }

        // TODO: improve perf by explicitly skipping over all other nodes?
        internal class RemoveInvalidUsingsRewriter : CSharpSyntaxRewriter
        {
            private readonly Compilation _compilation;

            public RemoveInvalidUsingsRewriter( Compilation compilation ) => this._compilation = compilation;

            public override SyntaxNode? VisitUsingDirective( UsingDirectiveSyntax node )
            {
                var symbolInfo = this._compilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node.Name );

                if ( symbolInfo.Symbol == null )
                    return null;

                return node;
            }
        }
    }
}
