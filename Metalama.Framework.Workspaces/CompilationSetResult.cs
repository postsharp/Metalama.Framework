// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Workspaces;

internal class CompilationSetResult : ICompilationSetResult
{
    public ImmutableArray<IIntrospectionCompilationResult> CompilationResults { get; }

    public CompilationSetResult( ImmutableArray<IIntrospectionCompilationResult> compilationsResults, string name )
    {
        this.CompilationResults = compilationsResults;
        this.TransformedCode = new CompilationSet( name, compilationsResults.Select( x => x.Compilation ).ToImmutableArray() );
    }

    public ICompilationSet TransformedCode { get; }

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

    [Memo]
    public ImmutableArray<IIntrospectionAdvice> Advice => this.CompilationResults.SelectMany( x => x.Advice ).ToImmutableArray();

    [Memo]
    public ImmutableArray<IIntrospectionTransformation> Transformations => this.CompilationResults.SelectMany( x => x.Transformations ).ToImmutableArray();
}