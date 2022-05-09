// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Project;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectInstanceFactory
{
    private readonly ConcurrentDictionary<AspectInstanceResult, IntrospectionAspectInstance> _instances = new();
    private readonly CompilationModel _compilation;
    private readonly IntrospectionPipelineListener? _pipelineListener;

    public IntrospectionAspectInstanceFactory( IServiceProvider serviceProvider, CompilationModel compilation )
    {
        this._compilation = compilation;
        this._pipelineListener = serviceProvider.GetService<IntrospectionPipelineListener>();
    }

    public IntrospectionAspectInstance GetIntrospectionAspectInstance( AspectInstanceResult aspectInstanceResult )
        => this._instances.GetOrAdd( aspectInstanceResult, x => new IntrospectionAspectInstance( x, this._compilation, this ) );

    public IntrospectionAdvice GetIntrospectionAdvice( Advice advice )
        => new( advice, this._pipelineListener?.GetAdviceResult( advice ) ?? AdviceResult.Create(), this._compilation );
}