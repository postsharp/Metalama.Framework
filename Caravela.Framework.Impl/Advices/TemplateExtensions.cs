// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal static class TemplateExtensions
    {
        public static void ValidateTarget( this in Template<IMethod> template, IMethod? targetMethod )
        {
            if ( targetMethod == null || template.IsNull )
            {
                return;
            }

            if ( !VerifyTemplateType( template.Declaration!.ReturnType, targetMethod.ReturnType ) )
            {
                throw new AssertionFailedException(
                    $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the template return type '{template.Declaration.ReturnType}' is not compatible with the type of the target method '{targetMethod.ReturnType}'." );
            }

            foreach ( var templateParameter in template.Declaration.Parameters )
            {
                var methodParameter = targetMethod.Parameters.OfName( templateParameter.Name );

                if ( methodParameter == null )
                {
                    throw new AssertionFailedException(
                        $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the target method does not contain a parameter '{templateParameter.Name}'." );
                }

                if ( !VerifyTemplateType( templateParameter.ParameterType, methodParameter.ParameterType ) )
                {
                    throw new AssertionFailedException(
                        $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the type of the template parameter '{templateParameter.Name}' is not compatible with the type of the target method parameter '{methodParameter.Name}'." );
                }
            }
        }

        private static bool VerifyTemplateType( IReadOnlyList<IType> fromTypes, IReadOnlyList<IType> toTypes )
        {
            if ( fromTypes.Count != toTypes.Count )
            {
                return false;
            }
            else
            {
                for ( var i = 0; i < fromTypes.Count; i++ )
                {
                    if ( !VerifyTemplateType( fromTypes[i], toTypes[i] ) )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool VerifyTemplateType( IType fromType, IType toType )
        {
            if ( fromType.TypeKind == TypeKind.Dynamic )
            {
                return true;
            }
            else if ( fromType.Is( toType ) )
            {
                return true;
            }
            else if ( fromType is INamedType { IsGeneric: true } fromNamedType && toType is INamedType toNamedType )
            {
                if ( fromNamedType.OriginalDeclaration.SpecialType == SpecialType.Task_T
                     && fromNamedType.GenericArguments[0].TypeKind == TypeKind.Dynamic )
                {
                    // We accept Task<dynamic> for any awaitable.

                    if ( toType.SpecialType == SpecialType.Void || toType.GetAsyncInfo().IsAwaitable ||
                         toNamedType.OriginalDeclaration.SpecialType is SpecialType.IAsyncEnumerable_T or SpecialType.IAsyncEnumerator_T )
                    {
                        return true;
                    }
                }
                else if ( fromNamedType.OriginalDeclaration.Equals( toNamedType.OriginalDeclaration ) &&
                          VerifyTemplateType( fromNamedType.GenericArguments, toNamedType.GenericArguments ) )
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsAsyncTask( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.Async => true,
                _ => false
            };

        public static bool IsAsync( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.Async => true,
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool IsAsyncIterator( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool IsIterator( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.IEnumerable => true,
                TemplateKind.IEnumerator => true,
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool MustInterpretAsAsync( this in Template<IMethod> template )
            => template.Declaration is { IsAsync: true }
               || (template.SelectedKind == TemplateKind.Default && template.InterpretedKind.IsAsync());

        public static bool MustInterpretAsAsyncIterator( this in Template<IMethod> template )
            => template.InterpretedKind.IsAsyncIterator() && (template.Declaration!.IsAsync || template.SelectedKind == TemplateKind.Default);
    }
}