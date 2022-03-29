// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Provides extension methods for handling of aspect reference annotations.
    /// </summary>
    internal static class AspectReferenceAnnotationExtensions
    {
        public const string AnnotationKind = "MetalamaAspectReference";

        /// <summary>
        /// Gets a specification of aspect reference if it is present on the syntax node.
        /// </summary>
        /// <param name="node">Syntax node.</param>
        /// <param name="specification">Specification of the aspect reference.</param>
        /// <returns></returns>
        public static bool TryGetAspectReference( this SyntaxNode node, out AspectReferenceSpecification specification )
        {
            var annotationValue = node.GetAnnotations( AnnotationKind ).SingleOrDefault()?.Data;

            if ( annotationValue == null )
            {
                specification = default;

                return false;
            }
            else
            {
                specification = AspectReferenceSpecification.FromString( annotationValue );

                return true;
            }
        }

        /// <summary>
        /// Returns the current node with an annotation indicating how the aspect-generated code references a declaration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="annotation"></param>
        /// <returns>Annotated syntax node.</returns>
        public static T WithAspectReferenceAnnotation<T>( this T node, in AspectReferenceSpecification annotation )
            where T : SyntaxNode
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( AnnotationKind, annotation.ToString() ) );
        }

        /// <summary>
        /// Returns the current node with an annotation indicating how the aspect-generated code references a declaration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="aspectLayerId">Aspect layer which created the syntax node.</param>
        /// <param name="order">Version of the target semantic in relation to the aspect layer.</param>
        /// <param name="targetKind">Target kind. For example self or property get accessor.</param>
        /// <param name="flags">Flags</param>
        /// <returns>Annotated syntax node.</returns>
        public static T WithAspectReferenceAnnotation<T>(
            this T node,
            AspectLayerId aspectLayerId,
            AspectReferenceOrder order,
            AspectReferenceTargetKind targetKind = AspectReferenceTargetKind.Self,
            AspectReferenceFlags flags = AspectReferenceFlags.None )
            where T : SyntaxNode
        {
            return node.WithAdditionalAnnotations(
                new SyntaxAnnotation( AnnotationKind, new AspectReferenceSpecification( aspectLayerId, order, targetKind, flags ).ToString() ) );
        }
    }
}