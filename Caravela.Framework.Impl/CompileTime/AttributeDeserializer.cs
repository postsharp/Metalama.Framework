using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Attribute = System.Attribute;
using TypedConstant = Caravela.Framework.Code.TypedConstant;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class AttributeDeserializer
    {
        public static AttributeDeserializer SystemTypesDeserializer { get; } = new AttributeDeserializer( new SystemTypeResolver() );

        private readonly ICompileTimeTypeResolver _compileTimeTypeResolver;

        public AttributeDeserializer( ICompileTimeTypeResolver compileTimeTypeResolver )
        {
            this._compileTimeTypeResolver = compileTimeTypeResolver;
        }

        public T CreateAttribute<T>( IAttribute attribute )
            where T : Attribute
            => (T) this.CreateAttribute( attribute, typeof( T ) );

        public Attribute CreateAttribute( IAttribute attribute )
        {
            // TODO: this is insufficiently tested, especially the case with Type arguments.
            // TODO: Exception handling and recovery should be better. Don't throw an exception but return false and emit a diagnostic.

            var constructorSymbol = attribute.Constructor.GetSymbol();
            var type = this._compileTimeTypeResolver.GetCompileTimeType( constructorSymbol.ContainingType, false ).AssertNotNull();

            return this.CreateAttribute( attribute, type );
        }

        private Attribute CreateAttribute( IAttribute attribute, Type type )
        {
            var constructorSymbol = attribute.Constructor.GetSymbol();
            var constructor = type.GetConstructors().Single( c => this.ParametersMatch( c.GetParameters(), constructorSymbol.Parameters ) );

            if ( constructor == null )
            {
                throw new InvalidOperationException( $"Could not load type {constructorSymbol.ContainingType}." );
            }

            var parameters = attribute.ConstructorArguments.Select(
                ( a, i ) => this.TranslateAttributeArgument( a, constructor.GetParameters()[i].ParameterType ) ).ToArray();

            var result = (Attribute) constructor.Invoke( parameters ).AssertNotNull();

            foreach ( var (name, value) in attribute.NamedArguments )
            {
                PropertyInfo? property;
                FieldInfo? field;

                if ( (property = type.GetProperty( name )) != null )
                {
                    property.SetValue( result, this.TranslateAttributeArgument( value, property.PropertyType ) );
                }
                else if ( (field = type.GetField( name )) != null )
                {
                    field.SetValue( result, this.TranslateAttributeArgument( value, field.FieldType ) );
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot find a field or property {name} in type {constructor.DeclaringType!.Name}" );
                }
            }

            return result;
        }

        private object? TranslateAttributeArgument( TypedConstant typedConstant, Type targetType ) => this.TranslateAttributeArgument( typedConstant.Value, typedConstant.Type, targetType );
        
        private object? TranslateAttributeArgument( object? value, IType sourceType, Type targetType )
        {
            if ( value == null )
            {
                return null;
            }

            switch ( value )
            {
                case TypedConstant typedConstant:
                    return this.TranslateAttributeArgument( typedConstant, targetType );
                
                case IType type:
                    if ( !targetType.IsAssignableFrom( typeof( Type ) ) )
                    {
                        throw new InvalidOperationException( $"System.Type cannot be assigned to {targetType}" );
                    }

                    return this._compileTimeTypeResolver.GetCompileTimeType( type.GetSymbol(), true );
                
                case string str:
                    // Make sure we don't fall under the IEnumerable case.
                    if ( !targetType.IsAssignableFrom( typeof( string ) ) )
                    {
                        throw new InvalidOperationException( $"System.Type cannot be assigned to {targetType}" );
                    }

                    return str;

                case IEnumerable list:
                    // We cannot use generic collections here because array of value types are not convertible to arrays of objects.
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
                        array.SetValue( this.TranslateAttributeArgument( item, sourceType, elementType ), index );
                        index++;
                    }

                    return array;

                default:
                    if ( sourceType is INamedType enumType && enumType.TypeKind == TypeKind.Enum && ((ITypeInternal) enumType).TypeSymbol is { } enumTypeSymbol )
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
                        throw new InvalidOperationException( $"{value.GetType()} cannot be assigned to {targetType}" );
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
                if ( reflectionParameters[i].ParameterType != this._compileTimeTypeResolver.GetCompileTimeType( roslynParameters[i].Type, true ) )
                {
                    return false;
                }
            }

            return true;
        }
    }
}