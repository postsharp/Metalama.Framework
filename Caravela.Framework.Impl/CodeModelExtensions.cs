// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.InternalInterfaces;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Attribute = Caravela.Framework.Impl.CodeModel.Attribute;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl
{
    internal static class CodeModelExtensions
    {
        public static CompilationModel GetCompilationModel( this IDeclaration declaration ) => (CompilationModel) declaration.Compilation;

        public static ISyntaxFactory GetSyntaxFactory( this IDeclaration declaration ) => declaration.GetCompilationModel().ReflectionMapper;

    
    
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
                case BuiltDeclaration builtDeclaration:
                    return new InsertPosition( InsertPositionRelation.After, builtDeclaration.Builder );

                case IDeclarationBuilder builder:
                    return new InsertPosition( InsertPositionRelation.After, builder );

                default:
                    var symbol = declaration.GetSymbol().AssertNotNull();

                    var memberDeclaration = symbol.GetPrimaryDeclaration().GetMemberDeclarationSyntax();

                    if ( memberDeclaration != null )
                    {
                        return new InsertPosition( InsertPositionRelation.After, memberDeclaration );
                    }
                    else
                    {
                        return new InsertPosition(
                            InsertPositionRelation.Within,
                            (MemberDeclarationSyntax) declaration.DeclaringType.GetPrimaryDeclaration().AssertNotNull() );
                    }
            }
        }
    }
}