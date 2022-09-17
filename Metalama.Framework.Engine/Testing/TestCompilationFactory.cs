// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Testing
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
            "System.Private.CoreLib",
            "System.Runtime",
            "System.Core",
            "System",
            "System.Console",
            "System.Threading",
            "System.Text.Encoding.Extensions",
            "System.Linq",
            "System.Collections",
            "System.Text.RegularExpressions",
            "System.ComponentModel.TypeConverter",
            "System.Runtime.Extensions",
            "System.Runtime.InteropServices.RuntimeInformation",
            "System.Private.Uri",
            "System.Threading.Thread",
            "System.Memory",
            "System.Diagnostics.Process",
            "System.ComponentModel.Primitives",
            "System.Threading.ThreadPool",
            "System.Runtime.InteropServices",
            "System.Diagnostics.Debug",
            "System.Diagnostics.TraceSource",
            "System.ComponentModel",
            "System.Collections.Concurrent",
            "System.Linq.Expressions",
            "System.Reflection.Emit.ILGeneration",
            "System.Reflection.Emit.Lightweight",
            "System.Reflection.Primitives",
            "System.Runtime.Loader",
            "System.Net.Primitives",
            "System.Reflection.Emit",
            "System.Net.Sockets",
            "System.Diagnostics.Tracing",
            "System.ObjectModel",
            "System.Collections.NonGeneric",
            "System.Threading.Tasks",
            "System.Runtime.Serialization.Formatters",
            "System.IO.FileSystem",
            "System.IO",
            "System.Globalization",
            "System.Reflection",
            "System.IO.FileSystem.Watcher",
            "System.Reflection.Extensions",
            "System.Collections.Immutable",
            "System.Reflection.Metadata" );

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

        public static IEnumerable<PortableExecutableReference> GetMetadataReferences(
            IEnumerable<Assembly>? additionalAssemblies = null,
            bool addMetalamaReferences = true )
        {
#if NET5_0_OR_GREATER
            var standardLibrariesNames = new[] { "netstandard" };
#else
            var standardLibrariesNames = new[] { "netstandard", "mscorlib" };
#endif

            var standardLibraries = standardLibrariesNames
                .Select( r => MetadataReference.CreateFromFile( Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) )
                .ToList();

            var metalamaLibraries = addMetalamaReferences ? new[] { typeof(IAspect).Assembly, typeof(IAspectWeaver).Assembly } : null;

            // Force the loading of some system assemblies before we search them in the AppDomain.
            _ = typeof(DynamicAttribute);

            var systemLibraries = AppDomainUtility.GetLoadedAssemblies(
                    a => !a.IsDynamic && _allowedSystemAssemblies.Contains( a.GetName().Name )
                                      && !string.IsNullOrEmpty( a.Location ) )
                .Concat( metalamaLibraries ?? Enumerable.Empty<Assembly>() )
                .Concat( additionalAssemblies ?? Enumerable.Empty<Assembly>() )
                .Distinct()
                .Select( GetCachedMetadataReference )
                .ToList();

            return standardLibraries.Concat( systemLibraries ).ToList();
        }

        // Caching is critical for memory usage, otherwise we get random OutOfMemoryException in parallel tests.
        private static PortableExecutableReference GetCachedMetadataReference( Assembly assembly )
            => _metadataReferenceCache.GetOrAdd( assembly, a => MetadataReference.CreateFromFile( a.Location ) );
    }
}