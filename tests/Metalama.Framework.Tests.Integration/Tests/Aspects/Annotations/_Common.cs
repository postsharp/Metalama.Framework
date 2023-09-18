using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Annotations;

public class MyAnnotation : IAnnotation<INamedType>
{
    public MyAnnotation( string? value )
    {
        Value = value;
    }

    public string? Value { get; }
}