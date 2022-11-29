using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;

[JsonObject]
public class SerializablePreviewTransformationResult
{
    public bool IsSuccessful { get; }

    public SerializableAnnotatedSyntaxTree TransformedSyntaxTree { get; }
    

    public string[]? ErrorMessages { get; }

    public SerializablePreviewTransformationResult( bool isSuccessful, SerializableAnnotatedSyntaxTree transformedSyntaxTree, string[]? errorMessages )
    {
        this.IsSuccessful = isSuccessful;
        this.TransformedSyntaxTree = transformedSyntaxTree;
        this.ErrorMessages = errorMessages;
    }
    
    public static SerializablePreviewTransformationResult Failure( params string[] errorMessage ) => new( false, null, errorMessage );

    public static SerializablePreviewTransformationResult Success( SerializableAnnotatedSyntaxTree transformedSyntaxTree, string[]? errorMessages ) => new( true, transformedSyntaxTree, errorMessages );
}