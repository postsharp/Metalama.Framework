// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal static class LinkerAnnotationExtensions
    {
        public const string AnnotationKind = "CaravelaAspectLinker";

        public static LinkerAnnotation? GetLinkerAnnotation( this SyntaxNode node )
        {
            var annotationValue = node.GetAnnotations( AnnotationKind ).SingleOrDefault()?.Data;

            return annotationValue != null ? LinkerAnnotation.FromString( annotationValue ) : null;
        }

        public static T AddLinkerAnnotation<T>( this T node, LinkerAnnotation annotation )
            where T : SyntaxNode
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( AnnotationKind, annotation.ToString() ) );
        }
    }
}
