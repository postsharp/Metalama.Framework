// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Attribute = System.Attribute;
using TypedConstant = Microsoft.CodeAnalysis.TypedConstant;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.CompileTime
{
    internal class AttributeDeserializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CompileTimeTypeResolver _compileTimeTypeResolver;
        private readonly UserCodeInvoker _userCodeInvoker;

        public AttributeDeserializer( IServiceProvider serviceProvider, CompileTimeTypeResolver compileTimeTypeResolver )
        {
            this._serviceProvider = serviceProvider;
            this._compileTimeTypeResolver = compileTimeTypeResolver;
            this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
        }

        // Coverage: ignore
        public bool TryCreateAttribute<T>( IAttribute attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out T? attributeInstance )
            where T : Attribute
            => this.TryCreateAttribute( attribute.GetAttributeData(), diagnosticAdder, out attributeInstance );

        // Coverage: ignore
        public bool TryCreateAttribute<T>( AttributeData attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out T? attributeInstance )
            where T : Attribute
        {
            if ( this.TryCreateAttribute( attribute, diagnosticAdder, out var untypedAttribute ) )
            {
                attributeInstance = (T) untypedAttribute;

                return true;
            }
            else
            {
                attributeInstance = null;

                return false;
            }
        }

        public bool TryCreateAttribute( IAttribute attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out Attribute? attributeInstance )
            => this.TryCreateAttribute( attribute.GetAttributeData(), diagnosticAdder, out attributeInstance );

        public bool TryCreateAttribute( AttributeData attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out Attribute? attributeInstance )
        {
            var constructorSymbol = attribute.AttributeConstructor;

            if ( constructorSymbol == null )
            {
                // This may happen at design time or with an invalid file. In this case, no error message is necessary.

                attributeInstance = null;

                return false;
            }

            var type = this._compileTimeTypeResolver.GetCompileTimeType( constructorSymbol.ContainingType, false );

            if ( type == null )
            {
                diagnosticAdder.Report(
                    AttributeDeserializerDiagnostics.CannotFindAttributeType.CreateRoslynDiagnostic(
                        attribute.GetDiagnosticLocation(),
                        constructorSymbol.ContainingType ) );

                attributeInstance = null;

                return false;
            }

            return this.TryCreateAttribute( attribute, type, diagnosticAdder, out attributeInstance! );
        }

        private bool TryCreateAttribute(
            AttributeData attribute,
            Type type,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out Attribute? attributeInstance )
        {
            var constructorSymbol = attribute.AttributeConstructor!;

            var constructors = type.GetConstructors().Where( c => this.ParametersMatch( c.GetParameters(), constructorSymbol.Parameters ) ).ToList();

            if ( constructors.Count == 0 )
            {
                throw new AssertionFailedException( $"Cannot map find the ConstructorInfo for '{constructorSymbol}'." );
            }
            else if ( constructors.Count > 1 )
            {
                throw new AssertionFailedException( $"Found more than one ConstructorInfo for '{constructorSymbol}'." );
            }

            var constructor = constructors[0];

            var constructorParameters = constructor.GetParameters();
            var arguments = new object?[constructorParameters.Length];
            ParameterInfo? paramsParameter = null;
            Array? paramsArgument = null;
            var paramsIndex = 0;

            if ( constructorParameters.Length > 0 )
            {
                // This code seems to execute only at design time.

                var lastParameter = constructorSymbol.Parameters[constructorParameters.Length - 1];

                if ( lastParameter.IsParams && constructorParameters.Length != attribute.ConstructorArguments.Length )
                {
                    paramsParameter = constructorParameters[constructorParameters.Length - 1];

                    paramsArgument = Array.CreateInstance(
                        paramsParameter.ParameterType.GetElementType(),
                        attribute.ConstructorArguments.Length - constructorParameters.Length + 1 );
                }
            }

            for ( var i = 0; i < attribute.ConstructorArguments.Length; i++ )
            {
                var constructorArgument = attribute.ConstructorArguments[i];

                var translatedArgument = this.TranslateAttributeArgument(
                    attribute,
                    constructorArgument,
                    constructorParameters[i].ParameterType,
                    diagnosticAdder );

                if ( paramsParameter != null && i >= paramsParameter.Position )
                {
                    paramsArgument!.SetValue( translatedArgument, paramsIndex );
                    paramsIndex++;
                }
                else
                {
                    arguments[i] = translatedArgument;
                }
            }

            if ( paramsArgument != null )
            {
                arguments[paramsParameter!.Position] = paramsArgument;
            }

            var executionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, UserCodeMemberInfo.FromMemberInfo( constructor ) );

            if ( !this._userCodeInvoker.TryInvoke( () => (Attribute) constructor.Invoke( arguments ), executionContext, out var localAttributeInstance ) )
            {
                attributeInstance = null;

                return false;
            }

            foreach ( var arg in attribute.NamedArguments )
            {
                if ( arg.Value.Kind == TypedConstantKind.Error )
                {
                    // Ignore a field or property assigned to a value that could not be parsed to the correct type by Roslyn.
                    continue;
                }

                PropertyInfo? property;
                FieldInfo? field;

                if ( (property = type.GetProperty( arg.Key )) != null )
                {
                    var translatedValue = this.TranslateAttributeArgument( attribute, arg.Value, property.PropertyType, diagnosticAdder );

                    if ( !this._userCodeInvoker.TryInvoke(
                            () => property.SetValue( localAttributeInstance, translatedValue ),
                            executionContext.WithInvokedMember( UserCodeMemberInfo.FromMemberInfo( property ) ) ) )
                    {
                        attributeInstance = null;

                        return false;
                    }
                }
                else if ( (field = type.GetField( arg.Key )) != null )
                {
                    var translatedValue = this.TranslateAttributeArgument( attribute, arg.Value, field.FieldType, diagnosticAdder );

                    if ( !this._userCodeInvoker.TryInvoke(
                            () => field.SetValue( localAttributeInstance, translatedValue ),
                            executionContext.WithInvokedMember( UserCodeMemberInfo.FromMemberInfo( field ) ) ) )
                    {
                        attributeInstance = null;

                        return false;
                    }
                }
                else
                {
                    // We should never get an invalid member because Roslyn would not add it to the collection.
                    throw new AssertionFailedException( $"Cannot find a FieldInfo or PropertyInfo named '{arg.Key}'." );
                }
            }

            attributeInstance = localAttributeInstance!;

            return true;
        }

        private object? TranslateAttributeArgument(
            AttributeData attribute,
            TypedConstant typedConstant,
            Type targetType,
            IDiagnosticAdder diagnosticAdder )
        {
            object? value;

            switch ( typedConstant.Kind )
            {
                case TypedConstantKind.Error:
                    // We should never get here if there is an invalid value.
                    throw new AssertionFailedException( "Got an invalid attribute argument value. " );

                case TypedConstantKind.Array when typedConstant.Values.IsDefault:
                    return null;

                case TypedConstantKind.Array:
                    value = typedConstant.Values;

                    break;

                default:
                    value = typedConstant.Value;

                    break;
            }

            return this.TranslateAttributeArgument(
                attribute,
                value,
                typedConstant.Type,
                targetType,
                diagnosticAdder );
        }

        private object? TranslateAttributeArgument(
            AttributeData attribute,
            object? value,
            ITypeSymbol? sourceType,
            Type targetType,
            IDiagnosticAdder diagnosticAdder )
        {
            if ( value == null )
            {
                return null;
            }

            switch ( value )
            {
                case TypedConstant typedConstant:
                    return this.TranslateAttributeArgument( attribute, typedConstant, targetType, diagnosticAdder );

                case ITypeSymbol type:
                    if ( !targetType.IsAssignableFrom( typeof(Type) ) )
                    {
                        // This should not happen because we don't process invalid values.
                        throw new AssertionFailedException( $"Cannot convert '{value.GetType().Name}' to '{targetType.Name}'." );
                    }

                    return this._compileTimeTypeResolver.GetCompileTimeType( type, true );

                case string str:
                    // Make sure we don't fall under the IEnumerable case.
                    if ( !targetType.IsAssignableFrom( typeof(string) ) )
                    {
                        // This should not happen because we don't process invalid values.
                        throw new AssertionFailedException( $"Cannot convert '{value.GetType().Name}' to '{targetType.Name}'." );
                    }

                    return str;

                case IEnumerable enumerable:
                    // We cannot use generic collections here because array of value types are not convertible to arrays of objects.

                    var list = enumerable.ToReadOnlyList();
                    var elementType = targetType.GetElementType() ?? typeof(object);

                    var count = 0;

                    foreach ( var unused in list )
                    {
                        count++;
                    }

                    var array = Array.CreateInstance( elementType, count );

                    var index = 0;

                    foreach ( var item in list )
                    {
                        var translatedItem = this.TranslateAttributeArgument( attribute, item, sourceType, elementType, diagnosticAdder );

                        array.SetValue( translatedItem, index );
                        index++;
                    }

                    return array;

                default:
                    if ( sourceType is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType )
                    {
                        // Convert the underlying value of an enum to a strongly typed enum when we can.
                        var enumReflectionType = this._compileTimeTypeResolver.GetCompileTimeType( enumType, false );

                        if ( enumReflectionType != null )
                        {
                            value = Enum.ToObject( enumReflectionType, value );
                        }
                    }

                    if ( !targetType.IsInstanceOfType( value ) )
                    {
                        // This should not happen because we don't process invalid values.
                        throw new AssertionFailedException( $"Cannot convert '{value.GetType().Name}' to '{targetType.Name}'." );
                    }

                    return value;
            }
        }

        private bool ParametersMatch( ParameterInfo[] reflectionParameters, ImmutableArray<IParameterSymbol> roslynParameters )
        {
            if ( reflectionParameters.Length != roslynParameters.Length )
            {
                return false;
            }

            for ( var i = 0; i < reflectionParameters.Length; i++ )
            {
                var compileTimeType = this._compileTimeTypeResolver.GetCompileTimeType( roslynParameters[i].Type, true );

                if ( reflectionParameters[i].ParameterType != compileTimeType )
                {
                    return false;
                }
            }

            return true;
        }
    }
}