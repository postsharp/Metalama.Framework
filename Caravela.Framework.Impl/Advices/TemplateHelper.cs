// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal static class TemplateHelper
    {
        public static IMethod? GetTemplateMethod(
            this INamedType aspectType,
            CompilationModel compilation,
            string? methodName,
            string adviceName,
            [DoesNotReturnIf( true )] bool throwIfMissing = true )
        {
            if ( methodName == null )
            {
                return null;
            }

            // We do the search against the Roslyn compilation because it is cheaper.

            var members = aspectType.GetSymbol().GetMembers( methodName ).ToList();
            var expectedAttributeTypeSymbol = compilation.ReflectionMapper.GetTypeSymbol( typeof(TemplateAttribute) );

            if ( members.Count != 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMethod.CreateException( (aspectType, methodName) );
            }

            var method = members.OfType<IMethodSymbol>().Single();

            if ( !method.SelectRecursive( m => m.OverriddenMethod, includeThis: true )
                .SelectMany( m => m.GetAttributes() )
                .Any( a => a.AttributeClass != null && StructuralSymbolComparer.Default.Equals( a.AttributeClass, expectedAttributeTypeSymbol ) ) )
            {
                if ( throwIfMissing )
                {
                    throw GeneralDiagnosticDescriptors.TemplateMemberMissesAttribute.CreateException(
                        (DeclarationKind.Method, method, expectedAttributeTypeSymbol, adviceName) );
                }
                else
                {
                    return null;
                }
            }

            return compilation.Factory.GetMethod( method );
        }

        public static IProperty? GetTemplateProperty(
            this INamedType aspectType,
            CompilationModel compilation,
            string propertyName,
            string adviceName,
            [DoesNotReturnIf( true )] bool throwIfMissing = true )
        {
            // We do the search against the Roslyn compilation because it is cheaper.

            var members = aspectType.GetSymbol().GetMembers( propertyName ).ToList();
            var expectedAttributeTypeSymbol = compilation.ReflectionMapper.GetTypeSymbol( typeof(TemplateAttribute) );

            if ( members.Count != 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMethod.CreateException( (aspectType, propertyName) );
            }

            var property = members.OfType<IPropertySymbol>().Single();

            if ( !property.SelectRecursive( m => m.OverriddenProperty, includeThis: true )
                .SelectMany( m => m.GetAttributes() )
                .Any( a => a.AttributeClass?.Equals( expectedAttributeTypeSymbol, SymbolEqualityComparer.Default ) ?? false ) )
            {
                if ( throwIfMissing )
                {
                    throw GeneralDiagnosticDescriptors.TemplateMemberMissesAttribute.CreateException(
                        (DeclarationKind.Property, property, expectedAttributeTypeSymbol, adviceName) );
                }
                else
                {
                    return null;
                }
            }

            return compilation.Factory.GetProperty( property );
        }
    }
}