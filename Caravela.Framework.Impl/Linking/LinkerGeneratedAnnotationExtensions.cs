// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    internal static class LinkerGeneratedAnnotationExtensions
    {
        public const string AnnotationKind = "CaravelaAspectLinkerGeneratedNode";

        public static LinkerGeneratedFlags GetLinkerGeneratedFlags( this SyntaxNode node )
        {
            var annotations = node.GetAnnotations( AnnotationKind );

            LinkerGeneratedFlags flags = default;

            foreach (var annotation in annotations)
            {
                if ( annotation?.Data != null )
                {
                    flags |= LinkerGeneratedAnnotation.FromString( annotation.Data ).Flags;
                }
            }

            return flags;
        }

        public static T AddLinkerGeneratedFlags<T>( this T node, in LinkerGeneratedFlags flags )
            where T : SyntaxNode
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( AnnotationKind, new LinkerGeneratedAnnotation( flags ).ToString() ) );
        }
    }
}