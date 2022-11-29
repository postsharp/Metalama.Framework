using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;

[JsonObject]
public class SerializableAnnotation
{
    public int SpanStart { get; }
    public int SpanLength { get; }
    public string Kind { get; }
    public string? Data { get; }

    public SerializableAnnotation( int spanStart, int spanLength, string kind, string? data )
    {
        this.SpanStart = spanStart;
        this.SpanLength = spanLength;
        this.Kind = kind;
        this.Data = data;
    }

    public SyntaxAnnotation ToSyntaxAnnotation() => new SyntaxAnnotation( this.Kind, this.Data );

}