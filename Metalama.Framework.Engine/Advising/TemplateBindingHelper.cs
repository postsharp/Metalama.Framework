// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Advising;

internal static class TemplateBindingHelper
{
    /// <summary>
    /// Binds a template to a introduced target method with given arguments.
    /// </summary>
    public static BoundTemplateMethod ForIntroduction(
        this TemplateMember<IMethod> template,
        IMethod targetMethod,
        IObjectReader? arguments = null )
    {
        return template.PartialForIntroduction( arguments ).ForIntroduction( targetMethod );
    }

    /// <summary>
    /// Partially binds a template with given type arguments when the target declaration is not yet known.
    /// Does partial validation.
    /// </summary>
    public static PartiallyBoundTemplateMethod PartialForIntroduction(
        this TemplateMember<IMethod> template,
        IObjectReader? arguments = null )
    {
        var templateTypeArguments = GetTemplateTypeArguments( template, arguments );

        return new PartiallyBoundTemplateMethod( template, templateTypeArguments, arguments );
    }

    /// <summary>
    /// Binds a partially bound template to a target declaration and finished validation.
    /// </summary>
    [return: NotNullIfNotNull( nameof(targetMethod) )]
    public static BoundTemplateMethod? ForIntroduction(
        this PartiallyBoundTemplateMethod template,
        IMethod? targetMethod )
    {
        if ( targetMethod == null )
        {
            return null;
        }

        ImmutableDictionary<string, ExpressionSyntax> CreateParameterMapping()
        {
            var mappingBuilder = ImmutableDictionary.CreateBuilder<string, ExpressionSyntax>();

            for ( var i = 0; i < template.TemplateMember.TemplateClassMember.RunTimeParameters.Length; i++ )
            {
                var templateParameter = template.TemplateMember.TemplateClassMember.RunTimeParameters[i];
                var parameter = targetMethod.Parameters[i];
                ExpressionSyntax parameterSyntax = IdentifierName( parameter.Name );
                parameterSyntax = SymbolAnnotationMapper.AddExpressionTypeAnnotation( parameterSyntax, parameter.Type.GetSymbol() );
                mappingBuilder.Add( templateParameter.Name, parameterSyntax );
            }

            return mappingBuilder.ToImmutable();
        }

        if ( targetMethod.OperatorKind.GetCategory() == OperatorCategory.None )
        {
            var templateArguments = GetTemplateArguments( template, CreateParameterMapping() );

            return new BoundTemplateMethod( template.TemplateMember, templateArguments );
        }
        else
        {
            var runTimeParameters = template.TemplateMember.TemplateClassMember.RunTimeParameters;

            var expectedParameterCount = targetMethod.OperatorKind.GetCategory() switch
            {
                OperatorCategory.Binary => 2,
                OperatorCategory.Conversion => 1,
                OperatorCategory.Unary => 1,
                _ => throw new AssertionFailedException( $"Invalid value for OperatorCategory: {targetMethod.OperatorKind.GetCategory()}." )
            };

            if ( runTimeParameters.Length != expectedParameterCount )
            {
                throw new InvalidTemplateSignatureException(
                    MetalamaStringFormatter.Format(
                        $"Cannot use the method '{template.Declaration}' as a template for the {targetMethod.OperatorKind} operator: this operator expects {expectedParameterCount} parameter(s) but got {runTimeParameters.Length}." ) );
            }

            var templateArguments = GetTemplateArguments( template.TemplateMember, template.TemplateArguments, CreateParameterMapping() );

            return new BoundTemplateMethod( template.TemplateMember, templateArguments );
        }
    }

    /// <summary>
    /// Binds a template to any initializer with given arguments.
    /// </summary>
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

        return new BoundTemplateMethod( template, GetTemplateArguments( template, arguments, ImmutableDictionary<string, ExpressionSyntax>.Empty ) );
    }

    /// <summary>
    /// Binds arguments for a template that is called from another template using meta.InvokeTemplate.
    /// </summary>
    public static object?[] ArgumentsForCalledTemplate( this TemplateMember<IMethod> template, IObjectReader arguments )
    {
        // The template must not have run-time parameters.
        if ( !template.TemplateClassMember.RunTimeParameters.IsEmpty )
        {
            throw new InvalidTemplateSignatureException(
                MetalamaStringFormatter.Format(
                    $"Cannot use the method '{template.Declaration}' in meta.InvokeTemplate: the method cannot have run-time parameters." ) );
        }

        // The template must not have run-time type parameters.
        if ( !template.TemplateClassMember.RunTimeTypeParameters.IsEmpty )
        {
            throw new InvalidTemplateSignatureException(
                MetalamaStringFormatter.Format(
                    $"Cannot use the method '{template.Declaration}' in meta.InvokeTemplate: the method cannot have run-time type parameters." ) );
        }

        return GetTemplateArguments( template, arguments, ImmutableDictionary<string, ExpressionSyntax>.Empty );
    }

    /// <summary>
    /// Binds a template to a contract for a given location name with given arguments.
    /// </summary>
    public static BoundTemplateMethod ForContract(
        this TemplateMember<IMethod> template,
        ExpressionSyntax parameterExpression,
        IObjectReader? arguments = null )
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

        var parameterMapping = ImmutableDictionary<string, ExpressionSyntax>.Empty
            .Add( "value", parameterExpression );

        return new BoundTemplateMethod( template, GetTemplateArguments( template, arguments, parameterMapping ) );
    }

    /// <summary>
    /// Binds a template to a given overridden method with given template arguments.
    /// </summary>
    public static BoundTemplateMethod ForOverride( this TemplateMember<IMethod> template, IMethod targetMethod, IObjectReader? arguments = null )
    {
        template.Declaration.GetSymbol().ThrowIfBelongsToDifferentCompilationThan( targetMethod.GetSymbol() );
        arguments ??= ObjectReader.Empty;
        var parameterMapping = ImmutableDictionary.CreateBuilder<string, ExpressionSyntax>();

        void AddParameter( IParameter methodParameter, IParameter templateParameter )
        {
            ExpressionSyntax parameterSyntax = IdentifierName( methodParameter.Name );

            parameterSyntax = SymbolAnnotationMapper.AddExpressionTypeAnnotation( parameterSyntax, methodParameter.Type.GetSymbol() );

            parameterMapping.Add( templateParameter.Name, parameterSyntax );
        }

        // Check that template run-time parameters match the target.
        switch ( targetMethod )
        {
            case { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove }:
            case { OperatorKind: not OperatorKind.None }:
                // For operators and accessors, if the template has any run-time parameter, then we match parameters by index and their number must be exact.

                if ( template.TemplateClassMember.RunTimeParameters.Length > 0 )
                {
                    var expectedParameterCount = targetMethod switch
                    {
                        { MethodKind: MethodKind.PropertyGet, ContainingDeclaration: IIndexer { Parameters.Count: var parameterCount } } => parameterCount,
                        { MethodKind: MethodKind.PropertySet, ContainingDeclaration: IIndexer { Parameters.Count: var parameterCount } } => parameterCount + 1,
                        { MethodKind: MethodKind.PropertyGet } => 0,
                        { MethodKind: MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove } => 1,
                        _ when targetMethod.OperatorKind.GetCategory() is OperatorCategory.Binary => 2,
                        _ when targetMethod.OperatorKind.GetCategory() is OperatorCategory.Conversion or OperatorCategory.Unary => 1,
                        _ => throw new AssertionFailedException( $"Unexpected operator/accessor method: {targetMethod}." )
                    };

                    var declarationKind = targetMethod switch
                    {
                        { OperatorKind: not OperatorKind.None } => "operator",
                        _ => "accessor"
                    };

                    if ( template.TemplateClassMember.RunTimeParameters.Length != expectedParameterCount )
                    {
                        throw new InvalidTemplateSignatureException(
                            MetalamaStringFormatter.Format(
                                $"Cannot use the method '{template.Declaration}' as a template for the {declarationKind} '{targetMethod}': this {declarationKind} expects {expectedParameterCount} parameter(s) but got {template.TemplateClassMember.RunTimeParameters.Length} were provided." ) );
                    }

                    for ( var i = 0; i < template.TemplateClassMember.RunTimeParameters.Length; i++ )
                    {
                        var templateParameter = template.Declaration.Parameters[template.TemplateClassMember.RunTimeParameters[i].SourceIndex];
                        var methodParameter = targetMethod.Parameters[i];

                        AddParameter( methodParameter, templateParameter );

                        if ( !VerifyTemplateType( templateParameter.Type, methodParameter.Type, template, arguments ) )
                        {
                            throw new InvalidTemplateSignatureException(
                                MetalamaStringFormatter.Format(
                                    $"Cannot use the template '{template.Declaration}' to override the {declarationKind} '{targetMethod}': the type of the template parameter '{templateParameter.Name}' is not compatible with the type of the target {declarationKind} parameter '{methodParameter.Name}'." ) );
                        }
                    }
                }

                break;

            case { OperatorKind: OperatorKind.None }:
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

                    AddParameter( methodParameter, templateParameter );
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

                break;

            default:
                throw new AssertionFailedException( $"Unsupported target: {targetMethod}" );
        }

        // We first check template arguments because it verifies them and we need them in VerifyTemplateType.
        var templateArguments = GetTemplateArguments( template, arguments, parameterMapping.ToImmutable() );

        // Verify that the template return type matches the target.
        if ( !VerifyTemplateType( template.Declaration.ReturnType, targetMethod.ReturnType, template, arguments, targetMethod.GetAsyncInfo() ) )
        {
            throw new InvalidTemplateSignatureException(
                MetalamaStringFormatter.Format(
                    $"Cannot use the template '{template.Declaration}' to override the method '{targetMethod}': the template return type '{template.Declaration.ReturnType}' is not compatible with the type of the target method '{targetMethod.ReturnType}'." ) );
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

    private static bool VerifyTemplateType(
        IType fromType,
        IType toType,
        TemplateMember<IMethod> template,
        IObjectReader arguments,
        AsyncInfo? toMethodAsyncInfo = null )
    {
        // Replace type parameters by arguments. Do this before translation, in case fromType is a type parameter that can't be translated.
        if ( fromType is ITypeParameter genericParameter && template.TemplateClassMember.TypeParameters[genericParameter.Index].IsCompileTime )
        {
            fromType = arguments[genericParameter.Name] switch
            {
                IType typeArg => typeArg,
                Type type => TypeFactory.GetType( type ),
                _ => throw new AssertionFailedException( $"Unexpected value of type '{arguments[genericParameter.Name]?.GetType()}'." )
            };
        }

        var translatedFromType = fromType.ForCompilation( toType.Compilation );

        if ( translatedFromType != null )
        {
            fromType = translatedFromType;
        }
        else
        {
            // This can happen when fromType is private, because toType compilation does not import private members.
            // In that case, try to translate the other way around.
            var translatedToType = toType.ForCompilation( fromType.Compilation );

            if ( translatedToType != null )
            {
                toType = translatedToType;
            }
            else
            {
                // There is no compilation that the two types have in common, so we can't verify them.
                // We have to return true here and will (hopefully) get a confusing error at a later stage.
                return true;
            }
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
            // dynamic templates support any target.
            return true;
        }
        else if ( fromType.SpecialType == SpecialType.Void )
        {
            // void templates support any target.
            return true;
        }
        else if ( fromType.Is( toType ) )
        {
            // Return types of template and target match.
            return true;
        }
        else if ( toMethodAsyncInfo != null && fromType is INamedType fromNamedType && toType is INamedType toNamedType )
        {
            // Special rules for matching async-related return types.

            var fromOriginalDefinition = fromNamedType.Definition;
            var toTypeAsyncInfo = toMethodAsyncInfo.Value;

            if ( fromOriginalDefinition.SpecialType == SpecialType.Task_T
                 && fromNamedType.TypeArguments[0].TypeKind == TypeKind.Dynamic )
            {
                // We accept Task<dynamic> for any awaitable, async void, and async enumerable.

                if ( toTypeAsyncInfo.IsAwaitable || toTypeAsyncInfo.IsAsync == true ||
                     toNamedType.Definition.SpecialType is SpecialType.IAsyncEnumerable_T or SpecialType.IAsyncEnumerator_T )
                {
                    return true;
                }
            }
            else if ( fromOriginalDefinition.SpecialType == SpecialType.Task )
            {
                // We accept Task for any void-returning awaitable (like the non-generic Task and ValueTask) and async void.

                if ( toTypeAsyncInfo is { IsAwaitable: true, ResultType.SpecialType: SpecialType.Void } ||
                     (toTypeAsyncInfo.IsAsync == true && toType.SpecialType == SpecialType.Void) )
                {
                    return true;
                }
            }
            else if ( fromNamedType.TypeArguments.Count > 0 &&
                      fromOriginalDefinition.Equals( toNamedType.Definition ) &&
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
        ImmutableDictionary<string, ExpressionSyntax> runTimeParameterMapping )
    {
        if ( template == null )
        {
            return Array.Empty<object?>();
        }

        var compileTimeArguments = template.TemplateArguments ?? ObjectReader.Empty;

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
        ImmutableDictionary<string, ExpressionSyntax> runTimeParameterMapping )
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
        ImmutableDictionary<string, ExpressionSyntax> runTimeParameterMapping,
        List<object?> templateArguments )
    {
        foreach ( var parameter in template.TemplateClassMember.Parameters )
        {
            if ( parameter.IsCompileTime )
            {
                if ( compileTimeArguments.TryGetValue( parameter.Name, out var parameterValue ) )
                {
                    templateArguments.Add( parameterValue );
                }
                else if ( parameter.HasDefaultValue )
                {
                    // Note that DefaultValue is null for default(SomeValueType), but MethodInfo.Invoke changes that back to default(SomeValueType).
                    templateArguments.Add( parameter.DefaultValue );
                }
                else
                {
                    throw new InvalidAdviceParametersException(
                        MetalamaStringFormatter.Format(
                            $"No value has been provided for the parameter '{parameter.Name}' of template '{template.Declaration}'." ) );
                }
            }
            else
            {
                var expression = runTimeParameterMapping.TryGetValue( parameter.Name, out var mapped )
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

                var typeModel = parameterValue switch
                {
                    IType type => type,
                    Type type => TypeFactory.Implementation.GetTypeByReflectionType( type ),
                    null => throw new InvalidAdviceParametersException(
                        MetalamaStringFormatter.Format(
                            $"The value of type parameter '{parameter.Name}' for template '{template.Declaration}' must not be null." ) ),
                    _ => throw new InvalidAdviceParametersException(
                        MetalamaStringFormatter.Format(
                            $"The value of parameter '{parameter.Name}' for template '{template.Declaration}' must be of type IType or Type." ) )
                };

                templateArguments.Add( CreateTemplateTypeArgument( parameter.Name, typeModel ) );
            }
        }
    }

    public static TemplateTypeArgument CreateTemplateTypeArgument( string name, IType type )
    {
        var syntax = OurSyntaxGenerator.CompileTime.Type( type.GetSymbol() ).AssertNotNull();
        var syntaxForTypeOf = OurSyntaxGenerator.CompileTime.TypeOfExpression( type.GetSymbol() ).Type;

        return new TemplateTypeArgument( name, type, syntax, syntaxForTypeOf );
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