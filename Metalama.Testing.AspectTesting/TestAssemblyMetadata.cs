// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Testing.AspectTesting.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Metalama.Testing.AspectTesting;

/// <summary>
/// Represents the properties read from assembly metadata and set from the MSBuild project.
/// </summary>
internal sealed record TestAssemblyMetadata(
    string ProjectDirectory,
    ImmutableArray<string> ParserSymbols,
    string TargetFramework,
    bool MustLaunchDebugger,
    ImmutableArray<TestAssemblyReference> AssemblyReferences,
    ImmutableArray<TestAssemblyReference> AnalyzerReferences,
    string? GlobalUsingsFile,
    TestFrameworkLicenseStatus License,
    ImmutableArray<string> IgnoredWarnings )
{
    private static bool IsUserAnalyzer( TestAssemblyReference x )
    {
        var fileName = Path.GetFileName( x.Path! );

        return !fileName.StartsWith( "Microsoft.", StringComparison.Ordinal )
               && !fileName.StartsWith( "System.", StringComparison.Ordinal )
               && !fileName.StartsWith( "Metalama.Framework.", StringComparison.Ordinal )
               && !fileName.StartsWith( "xunit.", StringComparison.Ordinal )
               && !fileName.StartsWith( "FakeItEasy.", StringComparison.Ordinal )
               && !fileName.StartsWith( "StyleCop.", StringComparison.Ordinal );
    }

    public TestProjectReferences ToProjectReferences()
    {
        var instances = ImmutableArray.CreateBuilder<object>();

        // First load all assemblies in the AppDomain.

        var assemblies = this.AnalyzerReferences.Where( IsUserAnalyzer ).Select( x => Assembly.LoadFile( x.Path! ) ).ToList();

        foreach ( var assembly in assemblies )
        {
            List<Type> exportedTypes;

            try
            {
                exportedTypes = assembly.GetTypes().Where( t => t.GetCustomAttribute<MetalamaPlugInAttribute>() != null ).ToList();
            }
            catch
            {
                // The safest option is to skip the assembly because there may be analyzer assemblies that have design-time dependencies, and we don't have them here.
                // TODO: Log
                continue;
            }

            foreach ( var type in exportedTypes )
            {
                try
                {
                    instances.Add( Activator.CreateInstance( type )! );
                }
                catch ( Exception e )
                {
                    throw new InvalidOperationException( $"Cannot instantiate the type '{type.Name}': {e.Message}", e );
                }
            }
        }

        return new TestProjectReferences(
            this.AssemblyReferences.Select( x => x.ToMetadataReference()! ).ToImmutableArray(),
            instances.ToImmutable(),
            this.GlobalUsingsFile );
    }
}