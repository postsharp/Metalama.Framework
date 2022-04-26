// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Project;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionPipelineListener : IService
{
    private readonly Dictionary<Advice, AdviceResult> _adviceResults = new();

    public void AddAdviceResult( Advice advice, AdviceResult result ) => this._adviceResults.Add( advice, result );

    public AdviceResult GetAdviceResult( Advice advice ) => this._adviceResults[advice];
}