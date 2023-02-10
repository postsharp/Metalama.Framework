// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Advising
{
    internal static class TemplateBindingHelper
    {
        public static BoundTemplateMethod ForIntroduction(
            this TemplateMember<IMethod> template,
            IMethod targetMethod,
            IObjectReader? arguments = null)
        {
            return template.ForIntroductionInitial( arguments ).ForIntroductionFinal( targetMethod );
        }

        public static PartiallyBoundTemplateMethod ForIntroductionInitial( 
            this TemplateMember<IMethod> template,
            IObjectReader? arguments = null )
        {
            var templateTypeArguments = GetTemplateTypeArguments( template, arguments );

            return new PartiallyBoundTemplateMethod( template, templateTypeArguments, arguments );
        }

        public static BoundTemplateMethod ForIntroductionFinal(
            this PartiallyBoundTemplateMethod template,
            IMethod targetMethod )
        {
            var arguments = template.Arguments ?? ObjectReader.Empty;

            // We first check template arguments because it verifies them and we need them in VerifyTemplateType.
            var templateArguments = GetTemplateArguments( template.TemplateMember, arguments );

            // Verify that the template return type matches the target.
            if ( !VerifyTemplateType( template.Declaration.ReturnType, targetMethod.ReturnType, template.TemplateMember, arguments ) )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the template return type '{template.Declaration.ReturnType}' is not compatible with the type of the target method '{targetMethod.ReturnType}'." ) );
            }

            for ( var i = 0; i < template.TemplateMember.TemplateClassMember.RunTimeParameters.Length; i++ )
            {
                var templateParameter = template.Declaration.Parameters[template.TemplateMember.TemplateClassMember.RunTimeParameters[i].SourceIndex];
                var methodParameter = targetMethod.Parameters[i];

                if ( !VerifyTemplateType( templateParameter.Type, methodParameter.Type, template.TemplateMember, arguments ) )
                {
                    throw new InvalidTemplateSignatureException(
                        MetalamaStringFormatter.Format(
                            $"Cannot use the template '{template.Declaration}' to override the operator '{targetMethod}': the type of the template parameter '{templateParameter.Name}' is not compatible with the type of the target operator parameter '{methodParameter.Name}'." ) );
                }
            }

            return new BoundTemplateMethod( template.TemplateMember, templateArguments );
        }

        public static BoundTemplateMethod ForOperatorIntroduction(
            this TemplateMember<IMethod> template,
            OperatorKind operatorKind,
            IObjectReader? arguments = null )
        {
            var runTimeParameters = template.TemplateClassMember.RunTimeParameters;

            var expectedParameterCount = operatorKind.GetCategory() switch
            {
                OperatorCategory.Binary => 2,
                OperatorCategory.Conversion => 1,
                OperatorCategory.Unary => 1,
                _ => throw new AssertionFailedException( $"Invalid value for OperatorCategory: {operatorKind.GetCategory()}." )
            };

            if ( runTimeParameters.Length != expectedParameterCount )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a template for the {operatorKind} operator: this operator expects {expectedParameterCount} parameter(s) but got {runTimeParameters.Length}." ) );
            }

            // TODO: verify the types.

            return new BoundTemplateMethod( template, GetTemplateArguments( template, arguments ) );
        }

        public static BoundTemplateMethod ForInitializer( this TemplateMember<IMethod> template, IObjectReader? arguments = null )
        {
            // The template must be void.
            if ( !template.Declaration.ReturnType.Is( SpecialType.Void ) )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as an initializer template: the method return type must be a void." ) );
            }

            // The template must not have run-time parameters.
            if ( !template.TemplateClassMember.RunTimeParameters.IsEmpty )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as an initializer template: the method cannot have run-time parameters." ) );
            }

            return new BoundTemplateMethod( template, GetTemplateArguments( template, arguments ) );
        }

        public static BoundTemplateMethod ForContract( this TemplateMember<IMethod> template, string parameterName, IObjectReader? arguments = null )
        {
            // The template must be void.
            if ( !template.Declaration.ReturnType.Is( SpecialType.Void ) )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a contract template: the method return type must be a void." ) );
            }

            // The template must not have run-time parameters.
            if ( template.TemplateClassMember.RunTimeParameters.Any( p => p.Name != "value" ) )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a contract template: the method cannot have run-time parameters except 'value'." ) );
            }

            if ( !template.TemplateClassMember.IndexedParameters.TryGetValue( "value", out var valueTemplateParameter )
                 || valueTemplateParameter.IsCompileTime )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a contract template: the method must have a run-time parameter named 'value'." ) );
            }

            var parameterMapping = ImmutableDictionary<string, ExpressionSyntax>.Empty;

            if ( parameterName != "value" )
            {
                parameterMapping = parameterMapping.Add( "value", IdentifierName( parameterName ) );
            }

            return new BoundTemplateMethod( template, GetTemplateArguments( template, arguments, parameterMapping ) );
        }

        public static BoundTemplateMethod ForOverride( this TemplateMember<IMethod> template, IMethod targetMethod, IObjectReader? arguments = null )
        {
            arguments ??= ObjectReader.Empty;

            // We first check template arguments because it verifies them and we need them in VerifyTemplateType.
            var templateArguments = GetTemplateArguments( template, arguments );

            // Verify that the template return type matches the target.
            if ( !VerifyTemplateType( template.Declaration.ReturnType, targetMethod.ReturnType, template, arguments ) )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the template return type '{template.Declaration.ReturnType}' is not compatible with the type of the target method '{targetMethod.ReturnType}'." ) );
            }            

            // Check that template run-time parameters match the target.
            if ( targetMethod.OperatorKind == OperatorKind.None )
            {
                // For non-operator methods, we match parameters by name.
                foreach ( var templateParameter in template.Declaration.Parameters )
                {
                    if ( template.TemplateClassMember.Parameters[templateParameter.Index].IsCompileTime )
                    {
                        continue;
                    }

                    var methodParameter = targetMethod.Parameters.OfName( templateParameter.Name );

                    if ( methodParameter == null )
                    {
                        var parameterNames = string.Join( ", ", targetMethod.Parameters.SelectAsImmutableArray( p => "'" + p.Name + "'" ) );

                        throw new InvalidTemplateSignatureException(
                            MetalamaStringFormatter.Format(
                                $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the target method does not contain a parameter '{templateParameter.Name}'. Available parameters are: {parameterNames}." ) );
                    }

                    if ( !VerifyTemplateType( templateParameter.Type, methodParameter.Type, template, arguments ) )
                    {
                        throw new InvalidTemplateSignatureException(
                            MetalamaStringFormatter.Format(
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
                            MetalamaStringFormatter.Format(
                                $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the target method does not contain a generic parameter '{templateParameter.Name}'." ) );
                    }

                    if ( !templateParameter.IsCompatibleWith( methodParameter ) )
                    {
                        throw new InvalidTemplateSignatureException(
                            MetalamaStringFormatter.Format(
                                $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the constraints on the template parameter '{templateParameter.Name}' are not compatible with the constraints on the target method parameter '{methodParameter.Name}'." ) );
                    }
                }
            }
            else
            {
                // For operators, if the template has any run-time parameter, then we match parameters by index and their number must be exact.

                if ( template.TemplateClassMember.RunTimeParameters.Length > 0 )
                {
                    var expectedParameterCount = targetMethod.OperatorKind.GetCategory() switch
                    {
                        OperatorCategory.Binary => 2,
                        OperatorCategory.Conversion => 1,
                        OperatorCategory.Unary => 1,
                        _ => throw new AssertionFailedException( $"Unexpected OperatorCategory: {targetMethod.OperatorKind.GetCategory()}." )
                    };

                    if ( template.TemplateClassMember.RunTimeParameters.Length != expectedParameterCount )
                    {
                        throw new InvalidTemplateSignatureException(
                            MetalamaStringFormatter.Format(
                                $"Cannot use the method '{template.Declaration}' as a template for the operator '{targetMethod}': this  operator expects {expectedParameterCount} parameter(s) but got {template.TemplateClassMember.RunTimeParameters.Length} were provided." ) );
                    }

                    for ( var i = 0; i < template.TemplateClassMember.RunTimeParameters.Length; i++ )
                    {
                        var templateParameter = template.Declaration.Parameters[template.TemplateClassMember.RunTimeParameters[i].SourceIndex];
                        var methodParameter = targetMethod.Parameters[i];

                        if ( !VerifyTemplateType( templateParameter.Type, methodParameter.Type, template, arguments ) )
                        {
                            throw new InvalidTemplateSignatureException(
                                MetalamaStringFormatter.Format(
                                    $"Cannot use the template '{template.Declaration}' to override the operator '{targetMethod}': the type of the template parameter '{templateParameter.Name}' is not compatible with the type of the target operator parameter '{methodParameter.Name}'." ) );
                        }
                    }
                }
            }

            return new BoundTemplateMethod( template, templateArguments );
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
                    _ => throw new AssertionFailedException( $"Unexpected value of type '{arguments[genericParameter.Name]?.GetType()}'." )
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
            else if ( fromType is INamedType { TypeArguments.Count: > 0 } fromNamedType && toType is INamedType toNamedType )
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

        private static object?[] GetTemplateTypeArguments(
            TemplateMember<IMethod>? template,
            IObjectReader? compileTimeArguments )
        {
            if ( template == null )
            {
                return Array.Empty<object?>();
            }

            compileTimeArguments ??= ObjectReader.Empty;

            var templateTypeArguments = new List<object?>();

            AddTemplateTypeParameters( template, compileTimeArguments, templateTypeArguments );

            return templateTypeArguments.ToArray();
        }

        private static object?[] GetTemplateArguments(
            PartiallyBoundTemplateMethod? template,
            ImmutableDictionary<string, ExpressionSyntax>? runTimeParameterMapping = null )
        {
            if ( template == null )
            {
                return Array.Empty<object?>();
            }

            var compileTimeArguments = template.Arguments ?? ObjectReader.Empty;

            var templateArguments = new List<object?>();

            // Add arguments for template parameters.
            AddTemplateParameters( template.TemplateMember, compileTimeArguments, runTimeParameterMapping, templateArguments );

            // Add already prepared arguments for template type parameters.
            templateArguments.AddRange( template.TypeArguments );

            VerifyArguments( template.TemplateMember, compileTimeArguments );

            return templateArguments.ToArray();
        }

        private static object?[] GetTemplateArguments(
            TemplateMember<IMethod>? template,
            IObjectReader? compileTimeArguments,
            ImmutableDictionary<string, ExpressionSyntax>? runTimeParameterMapping = null )
        {
            if ( template == null )
            {
                return Array.Empty<object?>();
            }

            compileTimeArguments ??= ObjectReader.Empty;

            var templateArguments = new List<object?>();

            // Add arguments for template parameters.
            AddTemplateParameters( template, compileTimeArguments, runTimeParameterMapping, templateArguments );

            // Add arguments for template type parameters.
            AddTemplateTypeParameters( template, compileTimeArguments, templateArguments );

            VerifyArguments( template, compileTimeArguments );

            return templateArguments.ToArray();
        }

        private static void AddTemplateParameters( 
            TemplateMember<IMethod> template, 
            IObjectReader compileTimeArguments, 
            ImmutableDictionary<string, ExpressionSyntax>? runTimeParameterMapping, 
            List<object?> templateArguments )
        {
            foreach ( var parameter in template.TemplateClassMember.Parameters )
            {
                if ( parameter.IsCompileTime )
                {
                    if ( !compileTimeArguments.TryGetValue( parameter.Name, out var parameterValue ) )
                    {
                        throw new InvalidAdviceParametersException(
                            MetalamaStringFormatter.Format(
                                $"No value has been provided for the parameter '{parameter.Name}' of template '{template.Declaration}'." ) );
                    }

                    templateArguments.Add( parameterValue );
                }
                else
                {
                    var expression = runTimeParameterMapping != null && runTimeParameterMapping.TryGetValue( parameter.Name, out var mapped )
                        ? mapped
                        : IdentifierName( parameter.Name );

                    templateArguments.Add( expression );
                }
            }
        }

        private static void AddTemplateTypeParameters( TemplateMember<IMethod> template, IObjectReader compileTimeParameters, List<object?> templateArguments )
        {
            foreach ( var parameter in template.TemplateClassMember.TypeParameters )
            {
                if ( parameter.IsCompileTime )
                {
                    if ( !compileTimeParameters.TryGetValue( parameter.Name, out var parameterValue ) )
                    {
                        throw new InvalidAdviceParametersException(
                            MetalamaStringFormatter.Format(
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
                                MetalamaStringFormatter.Format(
                                    $"The value of parameter '{parameter.Name}' for template '{template.Declaration}' must be of type IType or Type." ) );
                    }

                    var syntax = OurSyntaxGenerator.CompileTime.Type( typeModel.GetSymbol() ).AssertNotNull();
                    var syntaxForTypeOf = OurSyntaxGenerator.CompileTime.TypeOfExpression( typeModel.GetSymbol() ).Type;

                    templateArguments.Add( new TemplateTypeArgument( parameter.Name, typeModel, syntax, syntaxForTypeOf ) );
                }
            }
        }

        private static void VerifyArguments( TemplateMember<IMethod> template, IObjectReader compileTimeArguments )
        {
            // Check that all provided properties map to a compile-time parameter.
            foreach ( var name in compileTimeArguments.Keys )
            {
                if ( !template.TemplateClassMember.IndexedParameters.TryGetValue( name, out var parameter ) )
                {
                    throw new InvalidTemplateSignatureException(
                        MetalamaStringFormatter.Format( $"There is no parameter '{name}' in template '{template.Declaration}'." ) );
                }

                if ( !parameter.IsCompileTime )
                {
                    throw new InvalidTemplateSignatureException(
                        MetalamaStringFormatter.Format( $"The parameter '{name}' of template '{template.Declaration}' is not compile-time." ) );
                }
            }
        }
    }
}