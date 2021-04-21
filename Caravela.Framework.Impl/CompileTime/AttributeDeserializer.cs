// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Attribute = System.Attribute;
using TypedConstant = Caravela.Framework.Code.TypedConstant;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class AttributeDeserializer
    {
        public static AttributeDeserializer SystemTypesDeserializer { get; } = new( new SystemTypeResolver() );

        private readonly ICompileTimeTypeResolver _compileTimeTypeResolver;

        public AttributeDeserializer( ICompileTimeTypeResolver compileTimeTypeResolver )
        {
            this._compileTimeTypeResolver = compileTimeTypeResolver;
        }

        public bool TryCreateAttribute<T>( IAttribute attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out T? attributeInstance )
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

        public bool TryCreateAttribute( IAttribute attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out Attribute attributeInstance )
        {
            // TODO: this is insufficiently tested, especially the case with Type arguments.
            // TODO: Exception handling and recovery should be better. Don't throw an exception but return false and emit a diagnostic.

            var constructorSymbol = attribute.Constructor.GetSymbol();
            var type = this._compileTimeTypeResolver.GetCompileTimeType( constructorSymbol.ContainingType, false ).AssertNotNull();

            return this.TryCreateAttribute( attribute, type, diagnosticAdder, out attributeInstance! );
        }

        private bool TryCreateAttribute( IAttribute attribute, Type type, IDiagnosticAdder diagnosticAdder, [NotNullWhen(true)] out Attribute? attributeInstance )
        {
            var constructorSymbol = attribute.Constructor.GetSymbol();
            var constructor = type.GetConstructors().Single( c => this.ParametersMatch( c.GetParameters(), constructorSymbol.Parameters ) );

            if ( constructor == null )
            {
                throw new InvalidOperationException( $"Could not load type {constructorSymbol.ContainingType}." );
            }

            var parameters = new object?[attribute.ConstructorArguments.Count];

            for ( var i = 0; i < parameters.Length; i++ )
            {
                if ( !this.TryTranslateAttributeArgument(
                    attribute,
                    attribute.ConstructorArguments[i],
                    constructor.GetParameters()[i].ParameterType,
                    diagnosticAdder,
                    out var translatedArg ) )
                {
                    attributeInstance = null;

                    return false;
                }

                parameters[i] = translatedArg;
            }

            attributeInstance = (Attribute) constructor.Invoke( parameters ).AssertNotNull();

            foreach ( var (name, value) in attribute.NamedArguments )
            {
                PropertyInfo? property;
                FieldInfo? field;

                if ( (property = type.GetProperty( name )) != null )
                {
                    if ( !this.TryTranslateAttributeArgument( attribute, value, property.PropertyType, diagnosticAdder, out var translatedValue ) )
                    {
                        attributeInstance = null;

                        return false;
                    }

                    property.SetValue( attributeInstance, translatedValue );
                }
                else if ( (field = type.GetField( name )) != null )
                {
                    if ( !this.TryTranslateAttributeArgument( attribute, value, field.FieldType, diagnosticAdder, out var translatedValue ) )
                    {
                        attributeInstance = null;

                        return false;
                    }

                    field.SetValue( attributeInstance, translatedValue );
                }
                else
                {
                    throw new InvalidOperationException( $"Cannot find a field or property {name} in type {constructor.DeclaringType!.Name}" );
                }
            }

            return true;
        }

        private bool TryTranslateAttributeArgument(
            IAttribute attribute,
            TypedConstant typedConstant,
            Type targetType,
            IDiagnosticAdder diagnosticAdder,
            out object? translatedValue )
            => this.TryTranslateAttributeArgument( attribute, typedConstant.Value, typedConstant.Type, targetType, diagnosticAdder, out translatedValue );

        private bool TryTranslateAttributeArgument(
            IAttribute attribute,
            object? value,
            IType sourceType,
            Type targetType,
            IDiagnosticAdder diagnosticAdder,
            out object? translatedValue )
        {
            if ( value == null )
            {
                translatedValue = null;

                return true;
            }

            void ReportInvalidTypeDiagnostic()
            {
                diagnosticAdder.ReportDiagnostic(
                    AttributeDeserializerDiagnostics.CannotReferenceCompileTimeOnly.CreateDiagnostic(
                        attribute.GetDiagnosticLocation(),
                        (value.GetType(), targetType) ) );
            }

            switch ( value )
            {
                case TypedConstant typedConstant:
                    return this.TryTranslateAttributeArgument( attribute, typedConstant, targetType, diagnosticAdder, out translatedValue );

                case IType type:
                    if ( !targetType.IsAssignableFrom( typeof(Type) ) )
                    {
                        ReportInvalidTypeDiagnostic();
                        translatedValue = null;

                        return false;
                    }

                    translatedValue = this._compileTimeTypeResolver.GetCompileTimeType( type.GetSymbol(), true );

                    return true;

                case string str:
                    // Make sure we don't fall under the IEnumerable case.
                    if ( !targetType.IsAssignableFrom( typeof(string) ) )
                    {
                        ReportInvalidTypeDiagnostic();
                        translatedValue = null;

                        return false;
                    }

                    translatedValue = str;

                    return true;

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
                        if ( !this.TryTranslateAttributeArgument( attribute, item, sourceType, elementType, diagnosticAdder, out var translatedItem ) )
                        {
                            ReportInvalidTypeDiagnostic();
                            translatedValue = null;

                            return false;
                        }

                        array.SetValue( translatedItem, index );
                        index++;
                    }

                    translatedValue = array;

                    return true;

                default:
                    if ( sourceType is INamedType { TypeKind: TypeKind.Enum } enumType && ((ITypeInternal) enumType).TypeSymbol is { } enumTypeSymbol )
                    {
                        // Convert the underlying value of an enum to a strongly typed enum when we can.
                        var enumReflectionType = this._compileTimeTypeResolver.GetCompileTimeType( enumTypeSymbol, false );

                        if ( enumReflectionType != null )
                        {
                            value = Enum.ToObject( enumReflectionType, value );
                        }
                    }

                    if ( value != null && !targetType.IsInstanceOfType( value ) )
                    {
                        ReportInvalidTypeDiagnostic();
                        translatedValue = null;

                        return false;
                    }

                    translatedValue = value;

                    return true;
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
                if ( reflectionParameters[i].ParameterType != this._compileTimeTypeResolver.GetCompileTimeType( roslynParameters[i].Type, true ) )
                {
                    return false;
                }
            }

            return true;
        }
    }
}