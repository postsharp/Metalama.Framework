// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionCompilationResultModel : IIntrospectionCompilationResult
{
    private readonly IIntrospectionOptionsProvider _options;
    private readonly AspectPipelineResult? _pipelineResult;
    private readonly CompilationModel _compilation;
    private readonly IntrospectionAspectInstanceFactory? _introspectionAspectInstanceFactory;

    public IntrospectionCompilationResultModel(
        string name,
        IIntrospectionOptionsProvider? options,
        bool isSuccessful,
        CompilationModel compilationModel,
        ImmutableArray<IIntrospectionDiagnostic> diagnostics,
        IntrospectionAspectInstanceFactory? introspectionAspectInstanceFactory = null,
        AspectPipelineResult? pipelineResult = null )
    {
        this._options = options ?? new DefaultIntrospectionOptionsProvider();
        this._pipelineResult = pipelineResult;
        this.Name = name;
        this.IsSuccessful = isSuccessful;
        this.Diagnostics = diagnostics;
        this._introspectionAspectInstanceFactory = introspectionAspectInstanceFactory;
        this._compilation = compilationModel;
    }

    public string Name { get; }

    public bool IsSuccessful { get; }

    public ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }

    [Memo]
    public ImmutableArray<IIntrospectionAspectInstance> AspectInstances => this.GetAspectInstances();

    private ImmutableArray<IIntrospectionAspectInstance> GetAspectInstances()
    {
        if ( this._pipelineResult == null )
        {
            if ( this._options.IntrospectionOptions.IgnoreErrors )
            {
                return ImmutableArray<IIntrospectionAspectInstance>.Empty;
            }
            else
            {
                throw this.CreateCompilationFailedException();
            }
        }
        else
        {
            return this._pipelineResult.AspectInstanceResults
                .Select( x => this._introspectionAspectInstanceFactory.AssertNotNull().GetIntrospectionAspectInstance( x.AspectInstance ) )
                .ToImmutableArray<IIntrospectionAspectInstance>();
        }
    }

    private CompilationFailedException CreateCompilationFailedException()
        => new(
            $"The compilation of the project '{this._compilation.Name}' failed. Check the Diagnostics collection for details. Use WithIgnoreErrors(true) to ignore errors.",
            this.Diagnostics.Where( d => d.Severity is Severity.Error or Severity.Warning ).ToImmutableArray() );

    [Memo]
    public ImmutableArray<IIntrospectionAspectClass> AspectClasses => this.GetAspectClassesWithInstances();

    [Memo]
    public ImmutableArray<IIntrospectionAdvice> Advice => this.AspectInstances.SelectMany( i => i.Advice ).ToImmutableArray();

    [Memo]
    public ImmutableArray<IIntrospectionTransformation> Transformations => this.Advice.SelectMany( i => i.Transformations ).ToImmutableArray();

    private ImmutableArray<IIntrospectionAspectClass> GetAspectClassesWithInstances()
    {
        if ( this._pipelineResult == null )
        {
            if ( this._options.IntrospectionOptions.IgnoreErrors )
            {
                return ImmutableArray<IIntrospectionAspectClass>.Empty;
            }
            else
            {
                throw this.CreateCompilationFailedException();
            }
        }

        return this._pipelineResult.AspectInstanceResults.GroupBy( x => x.AspectInstance.AspectClass )
            .Select(
                x => new IntrospectionAspectClass(
                    x.Key,
                    x.ToImmutableArray(),
                    result => this._introspectionAspectInstanceFactory!.GetIntrospectionAspectInstance( result.AspectInstance ) ) )
            .ToImmutableArray<IIntrospectionAspectClass>();
    }

    public ICompilation TransformedCode
    {
        get
        {
            if ( this._pipelineResult == null && !this._options.IntrospectionOptions.IgnoreErrors )
            {
                throw this.CreateCompilationFailedException();
            }

            return this._compilation;
        }
    }
}