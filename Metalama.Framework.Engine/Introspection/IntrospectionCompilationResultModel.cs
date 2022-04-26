// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionCompilationResultModel : IIntrospectionCompilationOutput
{
    private readonly AspectPipelineResult? _pipelineResult;
    private readonly CompilationModel _compilation;
    private readonly IntrospectionAspectInstanceFactory _introspectionAspectInstanceFactory;

    public IntrospectionCompilationResultModel(
        IServiceProvider serviceProvider,
        bool isSuccessful,
        CompilationModel compilationModel,
        ImmutableArray<IIntrospectionDiagnostic> diagnostics,
        AspectPipelineResult? pipelineResult = null )
    {
        this._pipelineResult = pipelineResult;
        this.IsSuccessful = isSuccessful;
        this.Diagnostics = diagnostics;
        this._compilation = compilationModel;
        this._introspectionAspectInstanceFactory = new IntrospectionAspectInstanceFactory( serviceProvider, compilationModel );
    }

    public bool IsSuccessful { get; }

    public ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }

    [Memo]
    public ImmutableArray<IIntrospectionAspectInstance> AspectInstances
        => this._pipelineResult == null
            ? ImmutableArray<IIntrospectionAspectInstance>.Empty
            : this._pipelineResult.AspectInstanceResults.Select( x => this._introspectionAspectInstanceFactory.GetIntrospectionAspectInstance( x ) )
                .ToImmutableArray<IIntrospectionAspectInstance>();

    [Memo]
    public ImmutableArray<IIntrospectionAspectClass> AspectClasses => this.GetAspectClassesWithInstances();

    private ImmutableArray<IIntrospectionAspectClass> GetAspectClassesWithInstances()
    {
        if ( this._pipelineResult == null )
        {
            return ImmutableArray<IIntrospectionAspectClass>.Empty;
        }

        return this._pipelineResult.AspectInstanceResults.GroupBy( x => x.AspectInstance.AspectClass )
            .Select( x => new IntrospectionAspectClass( x.Key, x.ToImmutableArray(), this._introspectionAspectInstanceFactory.GetIntrospectionAspectInstance ) )
            .ToImmutableArray<IIntrospectionAspectClass>();
    }

    public ICompilation Compilation => this._compilation;
}