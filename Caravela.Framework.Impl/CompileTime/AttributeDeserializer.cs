// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TypedConstant = Microsoft.CodeAnalysis.TypedConstant;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class AttributeDeserializer
    {
        private readonly ICompileTimeTypeResolver _compileTimeTypeResolver;
        private readonly UserCodeInvoker _userCodeInvoker;

        public AttributeDeserializer( IServiceProvider serviceProvider, ICompileTimeTypeResolver compileTimeTypeResolver )
        {
            this._compileTimeTypeResolver = compileTimeTypeResolver;
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
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
            // TODO: this is insufficiently tested, especially the case with Type arguments.
            // TODO: Exception handling and recovery should be better. Don't throw an exception but return false and emit a diagnostic.

            var constructorSymbol = attribute.AttributeConstructor;

            if ( constructorSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(attribute), "Cannot instantiate an invalid attribute." );
            }

            var type = this._compileTimeTypeResolver.GetCompileTimeType( constructorSymbol.ContainingType, false );

            if ( type == null )
            {
                diagnosticAdder.Report(
                    AttributeDeserializerDiagnostics.CannotFindAttributeType.CreateDiagnostic( attribute.GetLocation(), constructorSymbol.ContainingType ) );

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

            var parameters = new object?[attribute.ConstructorArguments.Length];

            for ( var i = 0; i < parameters.Length; i++ )
            {
                var constructorArgument = attribute.ConstructorArguments[i];

                parameters[i] = this.TranslateAttributeArgument(
                    attribute,
                    constructorArgument,
                    constructor.GetParameters()[i].ParameterType,
                    diagnosticAdder );
            }

            Attribute localAttributeInstance;

            try
            {
                localAttributeInstance = attributeInstance =
                    attributeInstance = this._userCodeInvoker.Invoke( () => (Attribute) constructor.Invoke( parameters ) ).AssertNotNull();
            }
            catch ( Exception e )
            {
                diagnosticAdder.Report(
                    GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                        attribute.GetLocation(),
                        (type.Name, "new", e.GetType().Name, e.ToString()) ) );

                attributeInstance = null;

                return false;
            }

            foreach ( var (name, value) in attribute.NamedArguments )
            {
                if ( value.Kind == TypedConstantKind.Error )
                {
                    // Ignore a field or property assigned to a value that could not be parsed to the correct type by Roslyn.
                    continue;
                }

                PropertyInfo? property;
                FieldInfo? field;

                if ( (property = type.GetProperty( name )) != null )
                {
                    var translatedValue = this.TranslateAttributeArgument( attribute, value, property.PropertyType, diagnosticAdder );

                    try
                    {
                        this._userCodeInvoker.Invoke( () => property.SetValue( localAttributeInstance, translatedValue ) );
                    }
                    catch ( Exception e )
                    {
                        diagnosticAdder.Report(
                            GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                                attribute.GetLocation(),
                                (type.Name, property.Name, e.GetType().Name, e.ToString()) ) );

                        attributeInstance = null;

                        return false;
                    }
                }
                else if ( (field = type.GetField( name )) != null )
                {
                    var translatedValue = this.TranslateAttributeArgument( attribute, value, field.FieldType, diagnosticAdder );

                    this._userCodeInvoker.Invoke( () => field.SetValue( localAttributeInstance, translatedValue ) );
                }
                else
                {
                    // We should never get an invalid member because Roslyn would not add it to the collection.
                    throw new AssertionFailedException( $"Cannot find a FieldInfo or PropertyInfo named '{name}'." );
                }
            }

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