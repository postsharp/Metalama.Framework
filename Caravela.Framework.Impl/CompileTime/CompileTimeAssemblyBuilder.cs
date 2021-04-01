﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        private static readonly IEnumerable<MetadataReference> _fixedReferences;

        private readonly IServiceProvider _serviceProvider;
        private readonly ISymbolClassifier _symbolClassifier;
        private readonly TemplateCompiler _templateCompiler;
        private readonly IEnumerable<ResourceDescription>? _resources;
        private readonly Random _random = new();
        private (Compilation Compilation, MemoryStream CompileTimeAssembly)? _previousCompilation;

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

        // cannot be constructor-injected, because CompileTimeAssemblyLoader and CompileTimeAssemblyBuilder depend on each other
        public CompileTimeAssemblyLoader? CompileTimeAssemblyLoader { get; set; }

        public CompileTimeAssemblyBuilder(
            IServiceProvider serviceProvider,
            Compilation roslynCompilation,
            IEnumerable<ResourceDescription>? resources = null )
            : this(
                serviceProvider,
                new SymbolClassifier( roslynCompilation ),
                new TemplateCompiler(),
                resources )
        {
        }

        public CompileTimeAssemblyBuilder(
            IServiceProvider serviceProvider,
            ISymbolClassifier symbolClassifier,
            TemplateCompiler templateCompiler,
            IEnumerable<ResourceDescription>? resources )
        {
            this._serviceProvider = serviceProvider;
            this._symbolClassifier = symbolClassifier;
            this._templateCompiler = templateCompiler;
            this._resources = resources;
        }

        private Compilation? CreateCompileTimeAssembly( Compilation runTimeCompilation )
        {
            var compileTimeReferences =
                runTimeCompilation.References
                    .Select( reference =>
                    {
                        if ( reference is PortableExecutableReference { FilePath: string path } )
                        {
                            var assemblyBytes = this.CompileTimeAssemblyLoader?.GetCompileTimeAssembly( path );

                            if ( assemblyBytes != null )
                            {
                                return MetadataReference.CreateFromImage( assemblyBytes );
                            }
                        }

                        return null!;
                    } )
                    .Where( r => r != null )
                    .Concat( _fixedReferences );

            var compileTimeCompilation = CSharpCompilation.Create(
                runTimeCompilation.AssemblyName,
                Array.Empty<SyntaxTree>(),
                compileTimeReferences,
                new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, deterministic: true ) );
            
            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter( this._symbolClassifier, this._templateCompiler, runTimeCompilation, compileTimeCompilation );
            var modifiedRunTimeCompilation = produceCompileTimeCodeRewriter.VisitAllTrees( runTimeCompilation );

            if ( !produceCompileTimeCodeRewriter.Success )
            {
                // We don't want to continue with the control flow if we have a user error here, so we throw an exception.
                throw new InvalidUserCodeException(
                    "Cannot create the compile-time assembly.",
                    produceCompileTimeCodeRewriter.Diagnostics.ToImmutableArray() );
            }

            if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
            {
                return null;
            }

            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees(
                SyntaxFactory.ParseSyntaxTree(
                    $"[assembly: System.Reflection.AssemblyVersion(\"{this.GetUniqueVersion()}\")]",
                    runTimeCompilation.SyntaxTrees.First().Options ) );
            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( modifiedRunTimeCompilation.SyntaxTrees );
         
            compileTimeCompilation = new RemoveInvalidUsingsRewriter( compileTimeCompilation ).VisitAllTrees( compileTimeCompilation );

            return compileTimeCompilation;
        }

        // this is not nearly as good as a GUID, but should be good enough for the purpose of preventing collisions within the same process
        private string GetUniqueVersion()
        {
            int GetVersionComponent() => this._random.Next( 0, ushort.MaxValue );

            var major = GetVersionComponent();
            var minor = GetVersionComponent();
            var build = GetVersionComponent();
            var revision = GetVersionComponent();

            return new Version( major, minor, build, revision ).ToString();
        }

        private MemoryStream Emit( Compilation compilation )
        {
            
            var buildOptions = this._serviceProvider.GetService<IBuildOptions>();
            var compileTimeProjectDirectory = buildOptions.CompileTimeProjectDirectory;

            var result = this.TryEmit(compilation, compileTimeProjectDirectory, out MemoryStream stream);


            if ( !result.Success )
            {
                throw new AssertionFailedException( "Cannot compile the compile-time assembly.", result.Diagnostics );
            }

        
            return stream;
        }

        private EmitResult TryEmit(Compilation compilation, string compileTimeProjectDirectory, out MemoryStream stream)
        {
            EmitResult? result;
            stream = new MemoryStream();


            // Write the generated files to disk if we should.
            if (!string.IsNullOrWhiteSpace(compileTimeProjectDirectory))
            {
                if (!Directory.Exists(compileTimeProjectDirectory))
                {
                    Directory.CreateDirectory(compileTimeProjectDirectory);
                }

                compilation = compilation.WithOptions(compilation.Options.WithOptimizationLevel(OptimizationLevel.Debug));
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var tree in compilation.SyntaxTrees)
                {
                    // Find a decent and unique name.
                    var treeName = !string.IsNullOrWhiteSpace(tree.FilePath) ? Path.GetFileNameWithoutExtension(tree.FilePath) : "Anonymous";

                    if (names.Contains(treeName))
                    {
                        var treeNameSuffix = treeName;
                        for (var i = 1; names.Contains(treeName = treeNameSuffix + "_" + i); i++)
                        {
                            // Intentionally empty.
                        }

                        _ = names.Add(treeName);
                    }

                    var path = Path.Combine(compileTimeProjectDirectory, treeName + ".cs");
                    var text = tree.GetText();

                    using (var textWriter = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        text.Write(textWriter);
                    }

                    // Update the link to the file path.
                    var newTree = CSharpSyntaxTree.Create((CSharpSyntaxNode) tree.GetRoot(), (CSharpParseOptions?) tree.Options, path, Encoding.UTF8);
                    compilation = compilation.ReplaceSyntaxTree(tree, newTree);
                }

                var options = new EmitOptions(debugInformationFormat: DebugInformationFormat.Embedded);

                result = compilation.Emit(stream, manifestResources: this._resources, options: options);
            }
            else
            {
                result = compilation.Emit(stream, manifestResources: this._resources);
            }

            stream.Position = 0;
            return result;
        }

        public MemoryStream? EmitCompileTimeAssembly( Compilation compilation )
        {
            if ( compilation == this._previousCompilation?.Compilation )
            {
                var lastStream = this._previousCompilation.Value.CompileTimeAssembly;
                lastStream.Position = 0;
                return lastStream;
            }

            var compileTimeCompilation = this.CreateCompileTimeAssembly( compilation );

            if ( compileTimeCompilation == null )
            {
                return null;
            }

            var stream = this.Emit( compileTimeCompilation );

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
