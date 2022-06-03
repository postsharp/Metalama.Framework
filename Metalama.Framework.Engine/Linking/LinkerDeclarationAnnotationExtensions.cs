// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal static class LinkerDeclarationAnnotationExtensions
    {
        public const string DeclarationAnnotationKind = "MetalamaAspectLinkerDeclarationNode";
        public const string MarkedNodeIdAnnotationKind = "MetalamaAspectLinkerMarkedNode";

        public static LinkerDeclarationFlags GetLinkerDeclarationFlags( this SyntaxNode node )
        {
            var annotationValue = node.GetAnnotations( DeclarationAnnotationKind ).SingleOrDefault()?.Data;

            return annotationValue != null ? LinkerDeclarationAnnotation.FromString( annotationValue ).Flags : LinkerDeclarationFlags.None;
        }

        public static T WithLinkerDeclarationFlags<T>( this T node, in LinkerDeclarationFlags flags )
            where T : MemberDeclarationSyntax
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( DeclarationAnnotationKind, new LinkerDeclarationAnnotation( flags ).ToString() ) );
        }
    }
}