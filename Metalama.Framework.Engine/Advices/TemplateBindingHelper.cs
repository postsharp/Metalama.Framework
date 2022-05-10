// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.Advices
{
    internal class TemplateTypeArgument
    {
        public ExpressionSyntax Syntax { get; }
        public IType Type { get; }

        public TemplateTypeArgument( ExpressionSyntax syntax, IType type )
        {
            this.Syntax = syntax;
            this.Type = type;
        }
    }

    [Obfuscation( Exclude = true )] // Not obfuscated to have a decent call stack in case of user exception.
    internal static class TemplateBindingHelper
    {
        public static BoundTemplateMethod ForIntroduction( this in TemplateMember<IMethod> template, IObjectReader? parameters = null )
            => new( template, null, GetTemplateArguments( template, parameters ) );

        public static BoundTemplateMethod ForOverride( this in TemplateMember<IMethod> template, IMethod? targetMethod, IObjectReader? arguments = null )
        {
            if ( targetMethod == null || template.IsNull )
            {
                return default;
            }

            arguments ??= ObjectReader.Empty;

            // We first check template arguments because it verifies them and we need them in VerifyTemplateType.
            var templateArguments = GetTemplateArguments( template, arguments );

            // Verity that the template return type matches the target.
            if ( !VerifyTemplateType( template.Declaration!.ReturnType, targetMethod.ReturnType, template, arguments ) )
            {
                throw new InvalidAdviceTargetException(
                    UserMessageFormatter.Format(
                        $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the template return type '{template.Declaration.ReturnType}' is not compatible with the type of the target method '{targetMethod.ReturnType}'." ) );
            }

            // Check that template run-time parameters match the target.
            foreach ( var templateParameter in template.Declaration.Parameters )
            {
                if ( template.TemplateClassMember.Parameters[templateParameter.Index].IsCompileTime )
                {
                    continue;
                }

                var methodParameter = targetMethod.Parameters.OfName( templateParameter.Name );

                if ( methodParameter == null )
                {
                    var parameterNames = string.Join( ", ", targetMethod.Parameters.Select( p => "'" + p.Name + "'" ) );

                    throw new InvalidAdviceTargetException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the target method does not contain a parameter '{templateParameter.Name}'. Available parameters are: {parameterNames}." ) );
                }

                if ( !VerifyTemplateType( templateParameter.Type, methodParameter.Type, template, arguments ) )
                {
                    throw new InvalidAdviceTargetException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the type of the template parameter '{templateParameter.Name}' is not compatible with the type of the target method parameter '{methodParameter.Name}'." ) );
                }
            }

            // Check that template generic parameters match the target.
            foreach ( var templateParameter in template.Declaration.TypeParameters )
            {
                if ( template.TemplateClassMember.TypeParameters[templateParameter.Index].IsCompileTime )
                {
                    continue;
                }

                var methodParameter = targetMethod.TypeParameters.SingleOrDefault( p => p.Name == templateParameter.Name );

                if ( methodParameter == null )
                {
                    throw new InvalidAdviceTargetException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the target method does not contain a generic parameter '{templateParameter.Name}'." ) );
                }

                if ( !templateParameter.IsCompatibleWith( methodParameter ) )
                {
                    throw new InvalidAdviceTargetException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' on method '{targetMethod}': the constraints on the template parameter '{templateParameter.Name}' are not compatible with the constraints on the target method parameter '{methodParameter.Name}'." ) );
                }
            }

            return new BoundTemplateMethod( template, targetMethod, templateArguments );
        }

        private static bool VerifyTemplateType(
            IReadOnlyList<IType> fromTypes,
            IReadOnlyList<IType> toTypes,
            TemplateMember<IMethod> template,
            IObjectReader arguments )
        {
            if ( fromTypes.Count != toTypes.Count )
            {
                return false;
            }
            else
            {
                for ( var i = 0; i < fromTypes.Count; i++ )
                {
                    if ( !VerifyTemplateType( fromTypes[i], toTypes[i], template, arguments ) )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool VerifyTemplateType( IType fromType, IType toType, TemplateMember<IMethod> template, IObjectReader arguments )
        {
            // Replace type parameters by arguments.
            if ( fromType is ITypeParameter genericParameter && template.TemplateClassMember.TypeParameters[genericParameter.Index].IsCompileTime )
            {
                fromType = arguments[genericParameter.Name] switch
                {
                    IType typeArg => typeArg,
                    Type type => TypeFactory.GetType( type ),
                    _ => throw new AssertionFailedException()
                };
            }

            if ( fromType is ITypeParameter fromGenericParameter && toType is ITypeParameter toGenericParameter
                                                                 && fromGenericParameter.Name == toGenericParameter.Name )
            {
                // If we are comparing two generic parameters, we only compare the name here. The caller will then check
                // the compatibility of constraints, so we don't have to do it here.

                return true;
            }
            else if ( fromType.TypeKind == TypeKind.Dynamic )
            {
                return true;
            }
            else if ( fromType.Is( toType ) )
            {
                return true;
            }
            else if ( fromType is INamedType fromNamedType && fromNamedType.TypeArguments.Count > 0 && toType is INamedType toNamedType )
            {
                var fromOriginalDefinition = fromNamedType.GetOriginalDefinition();

                if ( fromOriginalDefinition.SpecialType == SpecialType.Task_T
                     && fromNamedType.TypeArguments[0].TypeKind == TypeKind.Dynamic )
                {
                    // We accept Task<dynamic> for any awaitable.

                    if ( toType.SpecialType == SpecialType.Void || toType.GetAsyncInfo().IsAwaitable ||
                         toNamedType.GetOriginalDefinition().SpecialType is SpecialType.IAsyncEnumerable_T or SpecialType.IAsyncEnumerator_T )
                    {
                        return true;
                    }
                }
                else if ( fromOriginalDefinition.Equals( toNamedType.GetOriginalDefinition() ) &&
                          VerifyTemplateType( fromNamedType.TypeArguments, toNamedType.TypeArguments, template, arguments ) )
                {
                    return true;
                }
            }

            return false;
        }

        private static object?[] GetTemplateArguments(
            in TemplateMember<IMethod> template,
            IObjectReader? compileTimeParameters,
            ImmutableDictionary<string, ExpressionSyntax>? runTimeParameterMapping = null )
        {
            if ( template.IsNull )
            {
                return Array.Empty<object?>();
            }

            compileTimeParameters ??= ObjectReader.Empty;

            var templateParameters = new List<object?>();

            // Add parameters.
            foreach ( var parameter in template.TemplateClassMember.Parameters )
            {
                if ( parameter.IsCompileTime )
                {
                    if ( !compileTimeParameters.TryGetValue( parameter.Name, out var parameterValue ) )
                    {
                        throw new InvalidAdviceParametersException(
                            UserMessageFormatter.Format(
                                $"No value has been provided for the parameter '{parameter.Name}' of template '{template.Declaration}'." ) );
                    }

                    templateParameters.Add( parameterValue );
                }
                else
                {
                    var expression = runTimeParameterMapping != null && runTimeParameterMapping.TryGetValue( parameter.Name, out var mapped )
                        ? mapped
                        : IdentifierName( parameter.Name );

                    templateParameters.Add( expression );
                }
            }

            // Add type parameters.
            foreach ( var parameter in template.TemplateClassMember.TypeParameters )
            {
                if ( parameter.IsCompileTime )
                {
                    if ( !compileTimeParameters.TryGetValue( parameter.Name, out var parameterValue ) )
                    {
                        throw new InvalidAdviceParametersException(
                            UserMessageFormatter.Format(
                                $"No value has been provided for the type parameter '{parameter.Name}' of template '{template.Declaration}'." ) );
                    }

                    IType typeModel;

                    switch ( parameterValue )
                    {
                        case IType type:
                            typeModel = type;

                            break;

                        case Type type:
                            typeModel = TypeFactory.Implementation.GetTypeByReflectionType( type );
                            break;

                        default:
                            throw new InvalidAdviceParametersException(
                                UserMessageFormatter.Format(
                                    $"The value of parameter '{parameter.Name}' for template '{template.Declaration}' must be of type IType or Type." ) );
                    }

                    var syntax = OurSyntaxGenerator.CompileTime.Type( typeModel.GetSymbol() ).AssertNotNull();


                    templateParameters.Add( new TemplateTypeArgument( syntax, typeModel ) );
                }
            }

            // Check that all provided properties map to a compile-time parameter.
            foreach ( var name in compileTimeParameters.Keys )
            {
                if ( !template.TemplateClassMember.IndexedParameters.TryGetValue( name, out var parameter ) )
                {
                    throw new InvalidAdviceTargetException(
                        UserMessageFormatter.Format( $"There is no parameter '{name}' in template '{template.Declaration}'." ) );
                }

                if ( !parameter.IsCompileTime )
                {
                    throw new InvalidAdviceTargetException(
                        UserMessageFormatter.Format( $"The parameter '{name}' of template '{template.Declaration}' is not compile-time." ) );
                }
            }

            return templateParameters.ToArray();
        }
    }
}