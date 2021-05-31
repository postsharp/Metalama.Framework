// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;

namespace Caravela.TestFramework
{
    public static class TestCompilationFactory
    {
        public static CSharpCompilation CreateEmptyCSharpCompilation( string? name = null, params Type[] additionalAssemblies )
        {
            var standardLibraries = new[] { "netstandard" }
                .Select( r => MetadataReference.CreateFromFile( Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) )
                .ToList();

            var systemLibraries = AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.IsDynamic && a.FullName != null && a.FullName.StartsWith( "System", StringComparison.Ordinal ) 
                                                                                                   && !string.IsNullOrEmpty( a.Location ));

            return CSharpCompilation.Create( name ?? "test_" + Guid.NewGuid() )
                .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true ) )
                .AddReferences( standardLibraries )
                .AddReferences(
                    additionalAssemblies.Prepend( typeof(IAspect) )
                        .Select( t => t.Assembly )
                        .Concat( systemLibraries )
                        .Distinct()
                        .Select( a => MetadataReference.CreateFromFile( a.Location ) ) );
        }
    }
}