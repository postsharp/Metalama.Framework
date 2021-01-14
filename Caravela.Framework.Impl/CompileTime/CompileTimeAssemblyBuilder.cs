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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    [Obfuscation( Exclude = true )]
    partial class CompileTimeAssemblyBuilder
    {
        private static readonly IEnumerable<MetadataReference> _fixedReferences;

        static CompileTimeAssemblyBuilder()
        {
            var referenceAssemblyPaths = ReferenceAssemblyLocator.GetReferenceAssemblies();

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

            _fixedReferences = referenceAssemblyPaths.Concat( caravelaPaths )
                .Select( path => MetadataReference.CreateFromFile( path ) ).ToImmutableArray();
        }

        private readonly ISymbolClassifier _symbolClassifier;
        private readonly TemplateCompiler _templateCompiler;
        private readonly IEnumerable<ResourceDescription>? _resources;
        private readonly bool _debugTransformedCode;

        // can't be constructor-injected, because CompileTimeAssemblyLoader and CompileTimeAssemblyBuilder depend on each other
        public CompileTimeAssemblyLoader? CompileTimeAssemblyLoader { get; set; }

        private (Compilation compilation, MemoryStream compileTimeAssembly)? _previousCompilation;

        private readonly Random _random = new();

        public CompileTimeAssemblyBuilder( 
            ISymbolClassifier symbolClassifier, TemplateCompiler templateCompiler, IEnumerable<ResourceDescription>? resources, bool debugTransformedCode )
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
                        var assemblyBytes = this.CompileTimeAssemblyLoader?.GetCompileTimeAssembly( path );

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
            int GetVersionComponent() => this._random.Next( 0, ushort.MaxValue );

            int major = GetVersionComponent();
            int minor = GetVersionComponent();
            int build = GetVersionComponent();
            int revision = GetVersionComponent();

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

            stream = Callbacks.AssemblyRewriter?.Invoke( stream ) ?? stream;

            this._previousCompilation = (compilation, stream);

            return stream;
        }

        public string GetResourceName() => "Caravela.CompileTimeAssembly.dll";

        /// <summary>
        /// Prepares run-time assembly by making compile-time only methods throw <see cref="NotSupportedException"/>.
        /// </summary>
        public CSharpCompilation PrepareRunTimeAssembly( CSharpCompilation compilation ) =>
            new PrepareRunTimeAssemblyRewriter( this._symbolClassifier, compilation ).VisitAllTrees( compilation );
    }
}
