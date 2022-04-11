using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAdvice : IIntrospectionAdvice
{
    private readonly Advice _advice;
    private readonly AdviceResult _adviceResult;

    public IntrospectionAdvice( Advice advice, AdviceResult adviceResult )
    {
        this._advice = advice;
        this._adviceResult = adviceResult;
    }

    public IDeclaration TargetDeclaration => this._advice.TargetDeclaration;

    public string AspectLayerId => this._advice.AspectLayerId.ToString();

    public ImmutableDictionary<string, object?> Tags => this._advice.Tags;

    public ImmutableArray<object> Transformations
        => this._adviceResult.ObservableTransformations.As<object>().AddRange( this._adviceResult.NonObservableTransformations );
}