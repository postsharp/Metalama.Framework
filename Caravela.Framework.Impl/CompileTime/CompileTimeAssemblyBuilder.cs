using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static Caravela.Framework.Impl.CompileTime.PackageVersions;

namespace Caravela.Framework.Impl.CompileTime
{
    class CompileTimeAssemblyBuilder
    {
        private static readonly IEnumerable<MetadataReference> _fixedReferences;

        static CompileTimeAssemblyBuilder()
        {
            // TODO: make NuGetPackageRoot MSBuild property compiler-visible and use that here?
            string nugetDirectory = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), ".nuget/packages" );

            string netstandardDirectory = Path.Combine( nugetDirectory, "netstandard.library/2.0.3/build/netstandard2.0/ref" );
            var netStandardPaths = new[] { "netstandard.dll", "System.Runtime.dll" }.Select( name => Path.Combine( netstandardDirectory, name ) );

            // Note: references to Roslyn assemblies can't be simply preserved, because they might have the wrong TFM
            var nugetPaths = new (string package, string version, string assembly)[]
            {
                ("microsoft.csharp", MicrosoftCSharpVersion, "Microsoft.CSharp.dll"),
                ("microsoft.codeanalysis.common", MicrosoftCodeAnalysisCommonVersion, "Microsoft.CodeAnalysis.dll"),
                ("microsoft.codeanalysis.csharp", MicrosoftCodeAnalysisCSharpVersion, "Microsoft.CodeAnalysis.CSharp.dll"),
                ("system.collections.immutable", SystemCollectionsImmutableVersion, "System.Collections.Immutable.dll")
            }
            .Select( x => $"{nugetDirectory}/{x.package}/{x.version}/lib/netstandard2.0/{x.assembly}" );

            // the SDK assembly might not be loaded at this point, so make sure it is
            _ = new AspectWeaverAttribute( null! );

            var caravelaAssemblies = new[]
            {
                "Caravela.Reactive.dll",
                "Caravela.Framework.dll",
                "Caravela.Framework.Sdk.dll",
                "Caravela.Framework.Impl.dll"
            };
            var caravelaPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Where( a => !a.IsDynamic ) // accessing Location of dynamic assemblies throws
                .Select( a => a.Location )
                .Where( path => caravelaAssemblies.Contains( Path.GetFileName( path ) ) );

            _fixedReferences = netStandardPaths.Concat( nugetPaths ).Concat( caravelaPaths )
                .Select( path => MetadataReference.CreateFromFile( path ) ).ToImmutableArray();
        }

        private readonly ISymbolClassifier _symbolClassifier;
        private readonly TemplateCompiler _templateCompiler;
        private readonly IList<ResourceDescription> _resources;
        private readonly bool _debugTransformedCode;

        // can't be constructor-injected, because CompileTimeAssemblyLoader and CompileTimeAssemblyBuilder depend on each other
        public CompileTimeAssemblyLoader CompileTimeAssemblyLoader { get; set; } = null!;

        private (Compilation compilation, MemoryStream compileTimeAssembly)? _previousCompilation;

        private readonly Random _random = new();

        public CompileTimeAssemblyBuilder( 
            ISymbolClassifier symbolClassifier, TemplateCompiler templateCompiler, IList<ResourceDescription> resources, bool debugTransformedCode )
        {
            this._symbolClassifier = symbolClassifier;
            this._templateCompiler = templateCompiler;
            this._resources = resources;
            this._debugTransformedCode = debugTransformedCode;
        }

        // TODO: creating the compile-time assembly like this means it can't use aspects itself; should it?
        private Compilation? CreateCompileTimeAssembly( Compilation compilation )
        {
            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter( this._symbolClassifier, this._templateCompiler, compilation );
            compilation = produceCompileTimeCodeRewriter.VisitAllTrees( compilation );

            if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
                return null;

            compilation = compilation.AddSyntaxTrees(
                SyntaxFactory.ParseSyntaxTree( $"[assembly: System.Reflection.AssemblyVersion(\"{this.GetUniqueVersion()}\")]",
                compilation.SyntaxTrees.First().Options ) );

            compilation = compilation.WithOptions( compilation.Options.WithDeterministic( true ).WithOutputKind( OutputKind.DynamicallyLinkedLibrary ) );

            var compileTimeReferences = compilation.References
                .Select( reference =>
                {
                    if ( reference is PortableExecutableReference { FilePath: string path } )
                    {
                        var assemblyBytes = this.CompileTimeAssemblyLoader.GetCompileTimeAssembly( path );

                        if ( assemblyBytes != null )
                            return MetadataReference.CreateFromImage( assemblyBytes );
                    }

                    return null!;
                } )
                .Where( r => r != null );

            compilation = compilation.WithReferences( _fixedReferences.Concat( compileTimeReferences ) );

            compilation = new RemoveInvalidUsingsRewriter( compilation ).VisitAllTrees( compilation );

            // TODO: produce better errors when there's an incorrect reference from compile-time code to non-compile-time symbol

            return compilation;
        }

        // this is not nearly as good as a GUID, but should be good enough for the purpose of preventing collisions within the same process
        private string GetUniqueVersion()
        {
            int getVersionComponent() => this._random.Next( 0, ushort.MaxValue );

            int major = getVersionComponent();
            int minor = getVersionComponent();
            int build = getVersionComponent();
            int revision = getVersionComponent();

            return new Version( major, minor, build, revision ).ToString();
        }

        private MemoryStream Emit( Compilation compilation, Compilation input )
        {
            var stream = new MemoryStream();

#if DEBUG
            compilation = compilation.WithOptions( compilation.Options.WithOptimizationLevel( OptimizationLevel.Debug ) );

            var options = new EmitOptions( debugInformationFormat: DebugInformationFormat.Embedded );
            var compilationForDebugging = this._debugTransformedCode ? compilation : input;
            var embeddedTexts = compilationForDebugging.SyntaxTrees.Select(
                tree =>
                {
                    string filePath = string.IsNullOrEmpty( tree.FilePath ) ? $"{Guid.NewGuid()}.cs" : tree.FilePath;

                    var text = tree.GetText();
                    if ( !text.CanBeEmbedded )
                        text = SourceText.From( text.ToString(), Encoding.UTF8 );

                    return EmbeddedText.FromSource( filePath, text );
                } );

            var result = compilation.Emit( stream, manifestResources: this._resources, options: options, embeddedTexts: embeddedTexts );
#else
            var result = compilation.Emit( stream, manifestResources: this._resources );
#endif

            if ( !result.Success )
            {
                throw new DiagnosticsException( GeneralDiagnosticDescriptors.ErrorBuildingCompileTimeAssembly, result.Diagnostics );
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

            var stream = this.Emit( compileTimeCompilation, compilation );

            this._previousCompilation = (compilation, stream);

            return stream;
        }

        public string GetResourceName() => "Caravela.CompileTimeAssembly.dll";

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
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;
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

#if DEBUG
                node = node.NormalizeWhitespace();
#endif

                return node;
            }

            // TODO: assembly and module-level attributes?
            public override SyntaxNode? VisitAttributeList( AttributeListSyntax node ) => node.Parent is CompilationUnitSyntax ? null : node;

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            private T? VisitTypeDeclaration<T>( T node ) where T : TypeDeclarationSyntax
            {
                switch ( this.GetSymbolDeclarationScope( node ) )
                {
                    case SymbolDeclarationScope.Default or SymbolDeclarationScope.RunTimeOnly:
                        return null;

                    case SymbolDeclarationScope.CompileTimeOnly:
                        this.FoundCompileTimeCode = true;

                        var members = new List<MemberDeclarationSyntax>();

                        foreach (var member in node.Members)
                        {
                            switch (member)
                            {
                                case MethodDeclarationSyntax method:
                                    members.AddRange( this.VisitMethodDeclaration( method ) );
                                    break;
                                case TypeDeclarationSyntax nestedType:
                                    members.Add( (MemberDeclarationSyntax) this.Visit( nestedType ) );
                                    break;
                                default:
                                    members.Add( member );
                                    break;
                            }
                        }

                        return (T) node.WithMembers( SyntaxFactory.List( members ) );

                    default:
                        throw new NotImplementedException();
                }
            }

            private new IEnumerable<MethodDeclarationSyntax> VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) == SymbolDeclarationScope.Template )
                {
                    var diagnostics = new List<Diagnostic>();
                    bool success =
                        this._templateCompiler.TryCompile( node, this._compilation.GetSemanticModel( node.SyntaxTree ), diagnostics, out _, out var transformedNode );

                    Debug.Assert( success || diagnostics.Any( d => d.Severity >= DiagnosticSeverity.Error ) );

                    if ( success )
                    {
                        // reporting warnings is currently not supported here
                        Debug.Assert( diagnostics.Count == 0 );

                        this._addTemplateUsings = true;
                    }
                    else
                        throw new DiagnosticsException( GeneralDiagnosticDescriptors.ErrorProcessingTemplates, diagnostics.ToImmutableArray() );

                    yield return node;
                    yield return (MethodDeclarationSyntax) transformedNode!;
                }
                else
                {
                    yield return (MethodDeclarationSyntax) base.VisitMethodDeclaration( node )!;
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
