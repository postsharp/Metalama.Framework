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
    private readonly IntrospectionFactory _factory;

    public IntrospectionCompilationResultModel(
        string name,
        IIntrospectionOptionsProvider? options,
        bool isSuccessful,
        CompilationModel compilationModel,
        ImmutableArray<IIntrospectionDiagnostic> diagnostics,
        IntrospectionFactory factory,
        AspectPipelineResult? pipelineResult = null )
    {
        this._options = options ?? new DefaultIntrospectionOptionsProvider();
        this._pipelineResult = pipelineResult;
        this.Name = name;
        this.IsMetalamaSuccessful = isSuccessful;
        this.Diagnostics = diagnostics;
        this._factory = factory;
        this._compilation = compilationModel;
    }

    public string Name { get; }

    public bool IsMetalamaSuccessful { get; }

    public ImmutableArray<IIntrospectionDiagnostic> Diagnostics { get; }

    [Memo]
    public ImmutableArray<IIntrospectionAspectInstance> AspectInstances => this.AspectClasses.SelectMany( x => x.Instances ).ToImmutableArray();

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
            .Select( x => this._factory.CreateIntrospectionAspectClass( x.Key, x.ToImmutableArray() ) )
            .ToImmutableArray();
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

    public bool IsMetalamaEnabled => true;
}