// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Advising
{
    [Obfuscation( Exclude = true )] // Not obfuscated to have a decent call stack in case of user exception.
    internal static class TemplateBindingHelper
    {
        public static BoundTemplateMethod ForIntroduction( this in TemplateMember<IMethod> template, IObjectReader? arguments = null )
        {
            return new BoundTemplateMethod( template, null, GetTemplateArguments( template, arguments ) );
        }

        public static BoundTemplateMethod ForInitializer( this in TemplateMember<IMethod> template, IObjectReader? arguments = null )
        {
            // The template must be void.
            if ( !template.Declaration!.ReturnType.Is( SpecialType.Void ) )
            {
                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as an initializer template: the method return type must be a void." ) );
            }

            // The template must not have run-time parameters.
            if ( template.TemplateClassMember.Parameters.Any( p => !p.IsCompileTime ) )
            {
                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as an initializer template: the method cannot have run-time parameters." ) );
            }

            return new BoundTemplateMethod( template, null, GetTemplateArguments( template, arguments ) );
        }

        public static BoundTemplateMethod ForContract( this in TemplateMember<IMethod> template, string parameterName, IObjectReader? arguments = null )
        {
            // The template must be void.
            if ( !template.Declaration!.ReturnType.Is( SpecialType.Void ) )
            {
                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a contract template: the method return type must be a void." ) );
            }

            // The template must not have run-time parameters.
            if ( template.TemplateClassMember.Parameters.Any( p => !p.IsCompileTime && p.Name != "value" ) )
            {
                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a contract template: the method cannot have run-time parameters except 'value'." ) );
            }

            if ( !template.TemplateClassMember.IndexedParameters.TryGetValue( "value", out var valueTemplateParameter )
                 || valueTemplateParameter.IsCompileTime )
            {
                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a contract template: the method must have a run-time parameter named 'value'." ) );
            }

            var parameterMapping = ImmutableDictionary<string, ExpressionSyntax>.Empty;

            if ( parameterName != "value" )
            {
                parameterMapping = parameterMapping.Add( "value", IdentifierName( parameterName ) );
            }

            return new BoundTemplateMethod( template, null, GetTemplateArguments( template, arguments, parameterMapping ) );
        }

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
                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the template return type '{template.Declaration.ReturnType}' is not compatible with the type of the target method '{targetMethod.ReturnType}'." ) );
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

                    throw new InvalidTemplateSignatureException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the target method does not contain a parameter '{templateParameter.Name}'. Available parameters are: {parameterNames}." ) );
                }

                if ( !VerifyTemplateType( templateParameter.Type, methodParameter.Type, template, arguments ) )
                {
                    throw new InvalidTemplateSignatureException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the type of the template parameter '{templateParameter.Name}' is not compatible with the type of the target method parameter '{methodParameter.Name}'." ) );
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
                    throw new InvalidTemplateSignatureException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the target method does not contain a generic parameter '{templateParameter.Name}'." ) );
                }

                if ( !templateParameter.IsCompatibleWith( methodParameter ) )
                {
                    throw new InvalidTemplateSignatureException(
                        UserMessageFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the constraints on the template parameter '{templateParameter.Name}' are not compatible with the constraints on the target method parameter '{methodParameter.Name}'." ) );
                }
            }

            return new BoundTemplateMethod( template, targetMethod, templateArguments );
        }

        public static BoundTemplateMethod ForOverride( this in TemplateMember<IMethod> template, IFinalizer? targetFinalizer, IObjectReader? arguments = null )
        {
            if ( targetFinalizer == null || template.IsNull )
            {
                return default;
            }

            arguments ??= ObjectReader.Empty;

            // We first check template arguments because it verifies them and we need them in VerifyTemplateType.
            var templateArguments = GetTemplateArguments( template, arguments );

            var voidType = TypeFactory.GetType( SpecialType.Void );

            // Verity that the template return type matches the target.
            if ( !VerifyTemplateType( template.Declaration!.ReturnType, voidType, template, arguments ) )
            {
                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the template '{template.Declaration}' to override the finalizer '{targetFinalizer}': the template return type '{template.Declaration.ReturnType}' is not compatible with the return type of the target finalizer '{voidType}'." ) );
            }

            // Check that template run-time parameters match the target.
            foreach ( var templateParameter in template.Declaration.Parameters )
            {
                if ( template.TemplateClassMember.Parameters[templateParameter.Index].IsCompileTime )
                {
                    continue;
                }

                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the template '{template.Declaration}' to override the finalizer '{targetFinalizer}': finalizers do not have parameters." ) );
            }

            // Check that template generic parameters match the target.
            foreach ( var templateParameter in template.Declaration.TypeParameters )
            {
                if ( template.TemplateClassMember.TypeParameters[templateParameter.Index].IsCompileTime )
                {
                    continue;
                }

                throw new InvalidTemplateSignatureException(
                    UserMessageFormatter.Format(
                        $"Cannot use the template '{template.Declaration}' to override the finalizer '{targetFinalizer}': finalizers do not have generic parameters." ) );
            }

            return new BoundTemplateMethod( template, targetFinalizer, templateArguments );
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
                    var syntaxForTypeOf = OurSyntaxGenerator.CompileTime.TypeOfExpression( typeModel.GetSymbol() ).Type;

                    templateParameters.Add( new TemplateTypeArgument( typeModel, syntax, syntaxForTypeOf ) );
                }
            }

            // Check that all provided properties map to a compile-time parameter.
            foreach ( var name in compileTimeParameters.Keys )
            {
                if ( !template.TemplateClassMember.IndexedParameters.TryGetValue( name, out var parameter ) )
                {
                    throw new InvalidTemplateSignatureException(
                        UserMessageFormatter.Format( $"There is no parameter '{name}' in template '{template.Declaration}'." ) );
                }

                if ( !parameter.IsCompileTime )
                {
                    throw new InvalidTemplateSignatureException(
                        UserMessageFormatter.Format( $"The parameter '{name}' of template '{template.Declaration}' is not compile-time." ) );
                }
            }

            return templateParameters.ToArray();
        }
    }
}