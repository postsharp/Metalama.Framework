// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAdvice : IIntrospectionAdvice
{
    private readonly Advice _advice;
    private readonly AdviceImplementationResult _adviceResult;
    private readonly ICompilation _compilation;

    public IntrospectionAdvice( Advice advice, AdviceImplementationResult adviceResult, ICompilation compilation )
    {
        this._advice = advice;
        this._adviceResult = adviceResult;
        this._compilation = compilation;
    }

    public IDeclaration TargetDeclaration => this._advice.TargetDeclaration.GetTarget( this._compilation );

    public string AspectLayerId => this._advice.AspectLayerId.ToString();

    public ImmutableArray<object> Transformations
        => this._adviceResult.ObservableTransformations.As<object>().AddRange( this._adviceResult.NonObservableTransformations );
}