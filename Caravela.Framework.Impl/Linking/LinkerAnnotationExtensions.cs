using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    public static class LinkerAnnotationExtensions
    {
        public const string AnnotationKind = "CaravelaAspectLinker";


        public static LinkerAnnotation? GetCodeVersionFromAnnotation( this SyntaxNode node )
        {
            var annotationValue = node.GetAnnotations( AnnotationKind ).SingleOrDefault()?.Data;

            return annotationValue != null ? LinkerAnnotation.FromString( annotationValue ) : null;
        }

        public static T AddCodeVersionAnnotation<T>( this T node, LinkerAnnotation annotation )
            where T : SyntaxNode
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( AnnotationKind, annotation.ToString() ) );
        }
    }
}
