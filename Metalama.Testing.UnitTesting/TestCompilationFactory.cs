// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Metalama.Testing.UnitTesting
{
    /// <summary>
    /// Utility class that creates a <see cref="CSharpCompilation"/>.
    /// </summary>
    public static class TestCompilationFactory
    {
        private static readonly ConcurrentDictionary<Assembly, PortableExecutableReference> _metadataReferenceCache = new();

        /// <summary>
        /// List of system assemblies that can be added as references to compilation if they are present in the AppDomain.
        /// </summary>
        private static readonly ImmutableHashSet<string> _allowedSystemAssemblies = ImmutableHashSet.Create(
            "System",
            "System.Collections",
            "System.Collections.Concurrent",
            "System.Collections.Immutable",
            "System.Collections.NonGeneric",
            "System.ComponentModel",
            "System.Console",
            "System.Core",
            "System.IO",
            "System.IO.FileSystem",
            "System.IO.FileSystem.Watcher",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Memory",
            "System.ObjectModel",
            "System.Reflection",
            "System.Runtime",
            "System.Text.RegularExpressions",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Threading.Thread",
            "System.Threading.ThreadPool",
            "System.Private.CoreLib" );

        public static CSharpCompilation CreateEmptyCSharpCompilation(
            string? name,
            IEnumerable<Assembly> additionalAssemblies,
            bool addMetalamaReferences = true,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
            ImmutableArray<string> implicitUsings = default,
            NullableContextOptions nullableContextOptions = NullableContextOptions.Enable )
            => CreateEmptyCSharpCompilation(
                name,
                GetMetadataReferences( additionalAssemblies, addMetalamaReferences ),
                outputKind,
                implicitUsings,
                nullableContextOptions );

        public static CSharpCompilation CreateEmptyCSharpCompilation(
            string? name,
            IEnumerable<MetadataReference> metadataReferences,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
            ImmutableArray<string> implicitUsings = default,
            NullableContextOptions nullableContextOptions = NullableContextOptions.Enable )
            => CSharpCompilation.Create( name ?? "test_" + RandomIdGenerator.GenerateId() )
                .WithOptions(
                    new CSharpCompilationOptions(
                        outputKind,
                        allowUnsafe: true,
                        nullableContextOptions: nullableContextOptions,
                        usings: implicitUsings.IsDefault
                            ? ImmutableArray<string>.Empty
                            : implicitUsings ) )
                .AddReferences( metadataReferences );

        public static IReadOnlyList<PortableExecutableReference> GetMetadataReferences(
            IEnumerable<Assembly>? additionalAssemblies = null,
            bool addMetalamaReferences = true )
        {
#if NET5_0_OR_GREATER
            var standardLibrariesNames = new[] { "netstandard" };
#else
            var standardLibrariesNames = new[] { "netstandard", "mscorlib" };
#endif

            var standardLibraries = standardLibrariesNames
                .SelectAsImmutableArray(
                    r => MetadataReference.CreateFromFile( Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) );

            var metalamaLibraries = addMetalamaReferences
                ? new[] { typeof(IAspect).Assembly, typeof(IAspectWeaver).Assembly, typeof(ITemplateSyntaxFactory).Assembly }
                : null;

            // Force the loading of some system assemblies before we search them in the AppDomain.
            _ = typeof(DynamicAttribute);

            var systemLibraries = AppDomainUtility.GetLoadedAssemblies(
                    a => !a.IsDynamic && _allowedSystemAssemblies.Contains( a.GetName().Name.AssertNotNull() )
                                      && !string.IsNullOrEmpty( a.Location ) )
                .Concat( metalamaLibraries ?? Enumerable.Empty<Assembly>() )
                .Concat( additionalAssemblies ?? Enumerable.Empty<Assembly>() )
                .Distinct()
                .Select( GetCachedMetadataReference )
                .ToList();

            return standardLibraries.ConcatList( systemLibraries );
        }

        // Caching is critical for memory usage, otherwise we get random OutOfMemoryException in parallel tests.
        private static PortableExecutableReference GetCachedMetadataReference( Assembly assembly )
            => _metadataReferenceCache.GetOrAdd( assembly, a => MetadataReference.CreateFromFile( a.Location ) );

        public static CSharpCompilation CreateCSharpCompilation(
            string code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null,
            bool addMetalamaReferences = true,
            IEnumerable<string>? preprocessorSymbols = null,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary )
            => CreateCSharpCompilation(
                new Dictionary<string, string> { { RandomIdGenerator.GenerateId() + ".cs", code } },
                dependentCode,
                ignoreErrors,
                additionalReferences,
                name,
                addMetalamaReferences,
                preprocessorSymbols,
                outputKind );

        public static CSharpCompilation CreateCSharpCompilation(
            IReadOnlyDictionary<string, string> code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null,
            bool addMetalamaReferences = true,
            IEnumerable<string>? preprocessorSymbols = null,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary )
        {
            var additionalAssemblies = new[] { typeof(UnitTestClass).Assembly };

            var parseOptions =
                SupportedCSharpVersions.DefaultParseOptions.WithPreprocessorSymbols( preprocessorSymbols: preprocessorSymbols ?? new[] { "METALAMA" } );

            var mainRoslynCompilation = CreateEmptyCSharpCompilation( name, additionalAssemblies, addMetalamaReferences, outputKind: outputKind )
                .AddSyntaxTrees( code.SelectAsEnumerable( c => SyntaxFactory.ParseSyntaxTree( c.Value, path: c.Key, options: parseOptions ) ) );

            if ( dependentCode != null )
            {
                var dependentCompilation = CreateEmptyCSharpCompilation( name == null ? null : null + ".Dependency", additionalAssemblies )
                    .AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( dependentCode, parseOptions ) );

                mainRoslynCompilation = mainRoslynCompilation.AddReferences( dependentCompilation.ToMetadataReference() );
            }

            if ( additionalReferences != null )
            {
                mainRoslynCompilation = mainRoslynCompilation.AddReferences( additionalReferences );
            }

            if ( !ignoreErrors )
            {
                AssertNoError( mainRoslynCompilation );
            }

            return mainRoslynCompilation;
        }

        private static void AssertNoError( CSharpCompilation mainRoslynCompilation )
        {
            var diagnostics = mainRoslynCompilation.GetDiagnostics();

            if ( diagnostics.Any( diag => diag.Severity >= DiagnosticSeverity.Error ) )
            {
                var lines = diagnostics.Select( diag => diag.ToString() ).Prepend( "The given code produced errors:" );

                throw new InvalidOperationException( string.Join( Environment.NewLine, lines ) );
            }
        }
    }
}