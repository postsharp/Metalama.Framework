// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
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
                case IReplaceMember replaceMember:
                    return replaceMember.ReplacedMember.AssertNotNull().GetTarget( declaration.Compilation ).ToInsertPosition();

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