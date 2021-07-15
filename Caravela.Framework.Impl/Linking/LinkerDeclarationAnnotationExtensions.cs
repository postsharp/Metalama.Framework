// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    internal static class LinkerDeclarationAnnotationExtensions
    {
        public const string AnnotationKind = "CaravelaAspectLinkerDeclarationNode";

        public static LinkerDeclarationFlags GetLinkerDeclarationFlags( this MemberDeclarationSyntax node )
        {
            var annotationValue = node.GetAnnotations( AnnotationKind ).SingleOrDefault()?.Data;

            return annotationValue != null ? LinkerDeclarationAnnotation.FromString( annotationValue ).Flags : LinkerDeclarationFlags.None;
        }

        public static T WithLinkerDeclarationFlags<T>( this T node, in LinkerDeclarationFlags flags )
            where T : MemberDeclarationSyntax
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( AnnotationKind, new LinkerDeclarationAnnotation( flags ).ToString() ) );
        }
    }
}