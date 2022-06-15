// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionPipelineListener : IService
{
    private readonly IntrospectionAspectInstanceFactory _introspectionFactory;

    public IntrospectionPipelineListener( IServiceProvider serviceProvider )
    {
        this._introspectionFactory = serviceProvider.GetRequiredService<IntrospectionAspectInstanceFactory>();
    }

    public void AddAdviceResult( IAspectInstance aspectInstance, Advice advice, AdviceImplementationResult result, ICompilation compilation )
    {
        var introspectionAdviceInstance = this._introspectionFactory.GetIntrospectionAspectInstance( aspectInstance );
        introspectionAdviceInstance.Advice.Add( new IntrospectionAdvice( advice, result, compilation ) );
    }

    public void AddAspectResult( AspectInstanceResult result )
    {
        var introspectionAdviceInstance = this._introspectionFactory.GetIntrospectionAspectInstance( result.AspectInstance );
        introspectionAdviceInstance.AspectInstanceResult = result;
    }
}