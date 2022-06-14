﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices;

internal class AdviceFactoryState
{
    private int _nextTransformationOrder;
    
    public Dictionary<IMember, ContractAdvice> ContractAdvices { get; }

    public CompilationModel Compilation { get; }

    public IAspectInstanceInternal AspectInstance { get; }

    public IServiceProvider ServiceProvider { get; }

    public IDiagnosticAdder Diagnostics { get; }

    public AspectPipelineConfiguration PipelineConfiguration { get; }

    public IntrospectionPipelineListener? IntrospectionListener { get; }

    public List<ITransformation> Transformations { get; } = new();

    public bool IsAspectSkipped { get; private set; }

    public AdviceFactoryState(
        IServiceProvider serviceProvider,
        CompilationModel compilation,
        IAspectInstanceInternal aspectInstance,
        IDiagnosticAdder diagnostics,
        AspectPipelineConfiguration pipelineConfiguration )
    {
        this.Compilation = compilation;
        this.AspectInstance = aspectInstance;
        this.ServiceProvider = serviceProvider;
        this.Diagnostics = diagnostics;
        this.PipelineConfiguration = pipelineConfiguration;
        this.ContractAdvices = new Dictionary<IMember, ContractAdvice>( compilation.InvariantComparer );
        this.IntrospectionListener = serviceProvider.GetService<IntrospectionPipelineListener>();
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
                this.Compilation.AddTransformation( observableTransformation );
            }
        }
    }

    public int GetTransformationOrder() => this._nextTransformationOrder++;

}