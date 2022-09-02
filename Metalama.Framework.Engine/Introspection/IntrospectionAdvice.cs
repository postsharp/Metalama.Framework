// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAdvice : IIntrospectionAdvice
{
    private readonly Advice _advice;
    private readonly AdviceImplementationResult _adviceResult;
    private readonly ICompilation _compilation;

    // TODO: we should not store an ICompilation here because it causes several compilation versions to be mixed in the introspection output.

    public IntrospectionAdvice( Advice advice, AdviceImplementationResult adviceResult, ICompilation compilation )
    {
        this._advice = advice;
        this._adviceResult = adviceResult;
        this._compilation = compilation;
    }

    public IDeclaration TargetDeclaration => this._advice.TargetDeclaration.GetTarget( this._compilation );

    public string AspectLayerId => this._advice.AspectLayerId.ToString();

    public ImmutableArray<object> Transformations => this._adviceResult.Transformations.As<object>();
}