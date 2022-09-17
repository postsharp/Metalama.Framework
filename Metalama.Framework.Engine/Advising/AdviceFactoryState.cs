// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising;

internal class AdviceFactoryState
{
    private int _nextTransformationOrder;

    public Dictionary<IMember, ContractAdvice> ContractAdvices { get; }

    public CompilationModel CurrentCompilation { get; }

    public IAspectInstanceInternal AspectInstance { get; }

    public IServiceProvider ServiceProvider { get; }

    public CompilationModel InitialCompilation { get; }

    public IDiagnosticAdder Diagnostics { get; }

    public AspectPipelineConfiguration PipelineConfiguration { get; }

    public IntrospectionPipelineListener? IntrospectionListener { get; }

    public List<ITransformation> Transformations { get; } = new();

    public bool IsAspectSkipped { get; private set; }

    public IAspectBuilder? AspectBuilder { get; set; }

    public UserCodeExecutionContext ExecutionContext { get; }

    public AdviceFactoryState(
        IServiceProvider serviceProvider,
        CompilationModel initialCompilation,
        CompilationModel currentCompilation,
        IAspectInstanceInternal aspectInstance,
        IDiagnosticAdder diagnostics,
        AspectPipelineConfiguration pipelineConfiguration,
        UserCodeExecutionContext executionContext )
    {
        this.InitialCompilation = initialCompilation;
        this.CurrentCompilation = currentCompilation;
        this.AspectInstance = aspectInstance;
        this.ServiceProvider = serviceProvider;
        this.Diagnostics = diagnostics;
        this.PipelineConfiguration = pipelineConfiguration;
        this.ContractAdvices = new Dictionary<IMember, ContractAdvice>( currentCompilation.InvariantComparer );
        this.IntrospectionListener = serviceProvider.GetService<IntrospectionPipelineListener>();
        this.ExecutionContext = executionContext;
    }

    public void SkipAspect()
    {
        this.IsAspectSkipped = true;
    }

    public void AddTransformations( List<ITransformation> transformations )
    {
        this.Transformations.AddRange( transformations );

        foreach ( var transformation in transformations )
        {
            if ( transformation is IObservableTransformation observableTransformation )
            {
                this.CurrentCompilation.AddTransformation( observableTransformation );
            }
        }
    }

    public int GetTransformationOrder() => this._nextTransformationOrder++; 
}