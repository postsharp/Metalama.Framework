// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Utility class that creates a <see cref="CSharpCompilation"/>.
    /// </summary>
    internal static class TestCompilationFactory
    {
        public static CSharpCompilation CreateEmptyCSharpCompilation( string? name, IEnumerable<Assembly> additionalAssemblies )
        {
            var standardLibraries = new[] { "netstandard" }
                .Select( r => MetadataReference.CreateFromFile( Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) )
                .ToList();

            var systemLibraries = AppDomain.CurrentDomain.GetAssemblies()
                .Where(
                    a => !a.IsDynamic && a.FullName != null && a.FullName.StartsWith( "System", StringComparison.Ordinal )
                         && !string.IsNullOrEmpty( a.Location ) )
                .Concat( additionalAssemblies )
                .Prepend( typeof(IAspect).Assembly )
                .Distinct()
                .Select( a => MetadataReference.CreateFromFile( a.Location ) )
                .ToList();

            var allReferences = standardLibraries.Concat( systemLibraries );

            return CreateEmptyCSharpCompilation( name, allReferences );
        }

        public static CSharpCompilation CreateEmptyCSharpCompilation( string? name, IEnumerable<MetadataReference> metadataReferences )
            => CSharpCompilation.Create( name ?? "test_" + Guid.NewGuid() )
                .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true ) )
                .AddReferences( metadataReferences );
    }
}