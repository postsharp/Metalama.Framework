﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking
{
    internal static class LinkerGeneratedAnnotationExtensions
    {
        public const string AnnotationKind = "MetalamaAspectLinkerGeneratedNode";

        public static LinkerGeneratedFlags GetLinkerGeneratedFlags( this SyntaxNode node )
        {
            var annotations = node.GetAnnotations( AnnotationKind );

            LinkerGeneratedFlags flags = default;

            foreach ( var annotation in annotations )
            {
                if ( annotation.Data != null )
                {
                    flags |= LinkerGeneratedAnnotation.FromString( annotation.Data ).Flags;
                }
            }

            return flags;
        }

        public static LinkerGeneratedFlags GetLinkerGeneratedFlags( this SyntaxTrivia trivia )
        {
            var annotations = trivia.GetAnnotations( AnnotationKind );

            LinkerGeneratedFlags flags = default;

            foreach ( var annotation in annotations )
            {
                if ( annotation.Data != null )
                {
                    flags |= LinkerGeneratedAnnotation.FromString( annotation.Data ).Flags;
                }
            }

            return flags;
        }

        public static T WithLinkerGeneratedFlags<T>( this T node, in LinkerGeneratedFlags flags )
            where T : SyntaxNode
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( AnnotationKind, new LinkerGeneratedAnnotation( flags ).ToString() ) );
        }

        public static SyntaxTrivia WithLinkerGeneratedFlags( this SyntaxTrivia trivia, in LinkerGeneratedFlags flags )
        {
            return trivia.WithAdditionalAnnotations( new SyntaxAnnotation( AnnotationKind, new LinkerGeneratedAnnotation( flags ).ToString() ) );
        }
    }
}