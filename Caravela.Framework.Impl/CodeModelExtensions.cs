// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using Attribute = Caravela.Framework.Impl.CodeModel.Attribute;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl
{
    internal static class CodeModelExtensions
    {
        public static CompilationModel GetCompilationModel( this IDeclaration declaration ) => (CompilationModel) declaration.Compilation;

        // TODO: should this be in the SDK?
        public static INamedTypeSymbol GetSymbol( this INamedType namedType )
        {
            if ( namedType is NamedType sourceNamedType )
            {
                return sourceNamedType.TypeSymbol;
            }

            throw new ArgumentOutOfRangeException( nameof(namedType), "This is not a source symbol." );
        }

        public static ITypeSymbol GetSymbol( this IType type )
        {
            if ( type is ITypeInternal sourceNamedType )
            {
                return sourceNamedType.TypeSymbol.AssertNotNull();
            }

            throw new ArgumentOutOfRangeException( nameof(type), "This is not a source symbol." );
        }

        public static IMethodSymbol GetSymbol( this IMethodBase method )
        {
            if ( method is MethodBase sourceMethod )
            {
                return (IMethodSymbol) sourceMethod.Symbol;
            }

            throw new ArgumentOutOfRangeException( nameof(method), "This is not a source symbol." );
        }

        public static IPropertySymbol GetSymbol( this IProperty property )
        {
            if ( property is Property sourceProperty )
            {
                return (IPropertySymbol) sourceProperty.Symbol;
            }

            throw new ArgumentOutOfRangeException( nameof(property), "This is not a source symbol." );
        }

        public static AttributeData GetAttributeData( this IAttribute attribute )
        {
            if ( attribute is Attribute attributeModel )
            {
                return attributeModel.AttributeData;
            }

            throw new ArgumentOutOfRangeException( nameof(attribute), "This is not a source attribute." );
        }

        public static bool IsAccessor( this IMethod method )
        {
            return method.MethodKind switch
            {
                MethodKind.PropertyGet => true,
                MethodKind.PropertySet => true,
                MethodKind.EventAdd => true,
                MethodKind.EventRemove => true,
                MethodKind.EventRaise => true,
                _ => false
            };
        }
    }
}