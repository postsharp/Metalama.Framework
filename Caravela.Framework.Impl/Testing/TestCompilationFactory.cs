// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Testing
{
    /// <summary>
    /// Utility class that creates a <see cref="CSharpCompilation"/>.
    /// </summary>
    public static class TestCompilationFactory
    {
        public static CSharpCompilation CreateEmptyCSharpCompilation(
            string? name,
            IEnumerable<Assembly> additionalAssemblies,
            bool addCaravelaReferences = true )
            => CreateEmptyCSharpCompilation( name, GetMetadataReferences( additionalAssemblies, addCaravelaReferences ) );

        public static CSharpCompilation CreateEmptyCSharpCompilation( string? name, IEnumerable<MetadataReference> metadataReferences )
            => CSharpCompilation.Create( name ?? "test_" + Guid.NewGuid() )
                .WithOptions(
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        allowUnsafe: true,
                        nullableContextOptions: NullableContextOptions.Enable ) )
                .AddReferences( metadataReferences );

        public static IEnumerable<PortableExecutableReference> GetMetadataReferences(
            IEnumerable<Assembly>? additionalAssemblies = null,
            bool addCaravelaReferences = true )
        {
#if NET5_0_OR_GREATER
            var standardLibrariesNames = new[] { "netstandard" };
#else
            var standardLibrariesNames = new[] { "netstandard", "mscorlib" };
#endif

            var standardLibraries = standardLibrariesNames
                .Select( r => MetadataReference.CreateFromFile( Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) )
                .ToList();

            var caravelaLibraries = addCaravelaReferences ? new[] { typeof(IAspect).Assembly, typeof(IAspectWeaver).Assembly } : null;

            var systemLibraries = AppDomain.CurrentDomain.GetAssemblies()
                .Where(
                    a => !a.IsDynamic && a.FullName != null && a.FullName.StartsWith( "System", StringComparison.Ordinal )
                         && !string.IsNullOrEmpty( a.Location ) )
                .Concat( caravelaLibraries ?? Enumerable.Empty<Assembly>() )
                .Concat( additionalAssemblies ?? Enumerable.Empty<Assembly>() )
                .Distinct()
                .Select( a => MetadataReference.CreateFromFile( a.Location ) )
                .ToList();

            return standardLibraries.Concat( systemLibraries ).ToList();
        }
    }
}