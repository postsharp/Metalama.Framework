using Metalama.Framework.Code;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionTransformation : IComparable<IIntrospectionTransformation>
{
    TransformationKind TransformationKind { get; }

    IDeclaration TargetDeclaration { get; }

    string Description { get; }

    IDeclaration? IntroducedDeclaration { get; }
    
    IIntrospectionAdvice Advice { get; }
}