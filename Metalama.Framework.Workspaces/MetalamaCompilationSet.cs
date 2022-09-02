// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Workspaces;

internal class MetalamaCompilationSet : CompilationSet, IMetalamaCompilationSet
{
    public ImmutableArray<IIntrospectionCompilationOutput> CompilationResults { get; }

    public MetalamaCompilationSet( ImmutableArray<IIntrospectionCompilationOutput> compilationsResults, string name ) :
        base( name, compilationsResults.Select( x => x.Compilation ).ToImmutableArray() )
    {
        this.CompilationResults = compilationsResults;
    }

    [Memo]
    public override ImmutableArray<IIntrospectionDiagnostic> SourceDiagnostics => this.CompilationResults.SelectMany( x => x.Diagnostics ).ToImmutableArray();

    [Memo]
    public ImmutableArray<IIntrospectionAspectInstance> AspectInstances => this.CompilationResults.SelectMany( x => x.AspectInstances ).ToImmutableArray();

    [Memo]
    public ImmutableArray<IIntrospectionAspectClass> AspectClasses
        => this.CompilationResults.SelectMany( c => c.AspectClasses )
            .GroupBy( x => x.FullName )
            .Select( group => IntrospectionMapper.AggregateAspectClasses( group.First(), group.SelectMany( x => x.Instances ) ) )
            .ToImmutableArray();

    [Memo]
    public ImmutableArray<IIntrospectionDiagnostic> Diagnostics => this.CompilationResults.SelectMany( x => x.Diagnostics ).ToImmutableArray();
}