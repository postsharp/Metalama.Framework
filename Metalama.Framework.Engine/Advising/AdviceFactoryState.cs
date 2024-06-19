// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.UserCode;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising;

internal sealed class AdviceFactoryState : IAdviceExecutionContext
{
    private readonly int _pipelineStepIndex;
    private readonly int _orderWithinType;
    private readonly ProjectServiceProvider _serviceProvider;
    private int _nextTransformationOrder;

    public CompilationModel CurrentCompilation { get; }

    public IAspectInstanceInternal AspectInstance { get; }

    public ref readonly ProjectServiceProvider ServiceProvider => ref this._serviceProvider;

    public CompilationModel InitialCompilation { get; }

    public IDiagnosticAdder Diagnostics { get; }

    public IntrospectionPipelineListener? IntrospectionListener { get; }

    public List<ITransformation> Transformations { get; } = new();

    public IAspectBuilder? AspectBuilder { get; set; }

    public UserCodeExecutionContext ExecutionContext { get; }

    public AdviceFactoryState(
        in ProjectServiceProvider serviceProvider,
        CompilationModel initialCompilation,
        CompilationModel currentCompilation,
        IAspectInstanceInternal aspectInstance,
        IDiagnosticAdder diagnostics,
        UserCodeExecutionContext executionContext,
        int pipelineStepIndex,
        int orderWithinType )
    {
        this._pipelineStepIndex = pipelineStepIndex;
        this._orderWithinType = orderWithinType;
        this.InitialCompilation = initialCompilation;
        this.CurrentCompilation = currentCompilation;
        this.AspectInstance = aspectInstance;
        this._serviceProvider = serviceProvider;
        this.Diagnostics = diagnostics;
        this.IntrospectionListener = serviceProvider.GetService<IntrospectionPipelineListener>();
        this.ExecutionContext = executionContext;
    }

    public void AddTransformations( List<ITransformation> transformations )
    {
        this.Transformations.AddRange( transformations );

        foreach ( var transformation in transformations )
        {
            if ( transformation.Observability != TransformationObservability.None )
            {
                this.CurrentCompilation.AddTransformation( transformation );

                if ( transformation is ISyntaxTreeTransformation syntaxTreeTransformation )
                {
                    UserCodeExecutionContext.CurrentOrNull?.AddDependencyTo( syntaxTreeTransformation.TransformedSyntaxTree );
                }
            }
        }
    }

    public void SetOrders( ITransformation transformation )
    {
        transformation.OrderWithinPipelineStepAndTypeAndAspectInstance = this._nextTransformationOrder++;
        transformation.OrderWithinPipelineStepAndType = this._orderWithinType;
        transformation.OrderWithinPipeline = this._pipelineStepIndex;
    }
}