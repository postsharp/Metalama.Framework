// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class CodeModelInternalExtensions
    {
        public static CompilationModel GetCompilationModel( this ICompilationElement declaration ) => (CompilationModel) declaration.Compilation;

        public static AttributeData GetAttributeData( this IAttribute attribute )
        {
            if ( attribute is Attribute attributeModel )
            {
                return attributeModel.AttributeData;
            }

            throw new ArgumentOutOfRangeException( nameof(attribute), "This is not a source attribute." );
        }

        public static bool IsAccessor( this IMethod method )
            => method.MethodKind switch
            {
                MethodKind.PropertyGet => true,
                MethodKind.PropertySet => true,
                MethodKind.EventAdd => true,
                MethodKind.EventRemove => true,
                MethodKind.EventRaise => true,
                _ => false
            };

        public static SyntaxNode? GetPrimaryDeclaration( this IDeclaration declaration )
        {
            return declaration.GetSymbol()?.GetPrimaryDeclaration();
        }

        public static InsertPosition ToInsertPosition( this IMember declaration )
        {
            switch ( declaration )
            {
                case BuiltDeclaration:
                case IDeclarationBuilder:
                    return new InsertPosition(
                        InsertPositionRelation.Within,
                        (MemberDeclarationSyntax) declaration.DeclaringType.GetPrimaryDeclaration().AssertNotNull() );

                default:
                    var symbol = declaration.GetSymbol().AssertNotNull();

                    var memberDeclaration = symbol.GetPrimaryDeclaration().FindMemberDeclaration();

                    return new InsertPosition( InsertPositionRelation.After, memberDeclaration );
            }
        }
    }
}