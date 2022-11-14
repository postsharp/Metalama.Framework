// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionPipelineListener : IService
{
    private readonly IntrospectionFactory _introspectionFactory;

    public IntrospectionPipelineListener( IServiceProvider serviceProvider )
    {
        this._introspectionFactory = serviceProvider.GetRequiredService<IntrospectionFactory>();
    }

    public void AddAdviceResult( IAspectInstance aspectInstance, Advice advice, AdviceImplementationResult result, ICompilation compilation )
    {
        var introspectionAdviceInstance = this._introspectionFactory.GetIntrospectionAspectInstance( aspectInstance );
        introspectionAdviceInstance.Advice.Add( new IntrospectionAdvice( advice, result, compilation, this._introspectionFactory ) );
    }

    public void AddAspectResult( AspectInstanceResult result )
    {
        var introspectionAdviceInstance = this._introspectionFactory.GetIntrospectionAspectInstance( result.AspectInstance );
        introspectionAdviceInstance.AspectInstanceResult = result;

        foreach ( var predecessor in result.AspectInstance.Predecessors )
        {
            var predecessorObject = this._introspectionFactory.GetIntrospectionAspectPredecessor( predecessor.Instance );
            predecessorObject.AddSuccessor( result.AspectInstance );
        }
    }

    public void AddStaticFabricResult( StaticFabricResult result ) { }

    public void AddStaticFabricFailure( StaticFabricDriver driver ) { }
}