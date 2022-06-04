// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices;

internal class AdviceFactoryState
{
    internal List<Advice> Advices { get; } = new();

    public Dictionary<INamedType, ImplementInterfaceAdvice> ImplementInterfaceAdvices { get; }

    public Dictionary<IMember, ContractAdvice> ContractAdvices { get; }

    public CompilationModel Compilation { get; }

    public IAspectInstanceInternal AspectInstance { get; }

    public IServiceProvider ServiceProvider { get; }

    public IDiagnosticAdder Diagnostics { get; }

    public AspectPipelineConfiguration PipelineConfiguration { get; }

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
        this.ImplementInterfaceAdvices = new Dictionary<INamedType, ImplementInterfaceAdvice>( compilation.InvariantComparer );
        this.ContractAdvices = new Dictionary<IMember, ContractAdvice>( compilation.InvariantComparer );
    }
}