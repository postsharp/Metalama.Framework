using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    public static class LinkerAnnotationExtensions
    {
        private const string annotationKind = "CaravelaAspectLinker";

        public static LinkerAnnotation? GetCodeVersionFromAnnotation( this SyntaxNode node )
        {
            var annotationValue = node.GetAnnotations( annotationKind ).SingleOrDefault()?.Data;

            return annotationValue != null ? LinkerAnnotation.FromString( annotationValue ) : null;
        }

        public static SyntaxNode AddCodeVersionAnnotation( this SyntaxNode node, LinkerAnnotation annotation )
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( annotationKind, annotation.ToString() ) );
        }
    }
}
