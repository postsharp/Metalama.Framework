using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class AttributeDeserializer
    {
        public static AttributeDeserializer SystemTypesDeserializer { get; } = new AttributeDeserializer( new SystemTypeResolver() );
        
        private ICompileTimeTypeResolver _compileTimeTypeResolver;

        public AttributeDeserializer( ICompileTimeTypeResolver compileTimeTypeResolver )
        {
            this._compileTimeTypeResolver = compileTimeTypeResolver;
        }

        public T CreateAttribute<T>( IAttribute attribute )
            where T : Attribute
            => (T) this.CreateAttribute( attribute, typeof(T) );

        public Attribute CreateAttribute( IAttribute attribute )
        {
            // TODO: this is insufficiently tested, especially the case with Type arguments.
            // TODO: Exception handling and recovery should be better. Don't throw an exception but return false and emit a diagnostic.

            var constructorSymbol = attribute.Constructor.GetSymbol();
            var type = this._compileTimeTypeResolver.GetCompileTimeType( constructorSymbol.ContainingType, false ).AssertNotNull();

        

            return this.CreateAttribute(attribute, type);
        }

        private Attribute CreateAttribute(IAttribute attribute, Type type)
        {
            var constructorSymbol = attribute.Constructor.GetSymbol();
            var constructor = type?.GetConstructors().Single( c => this.ParametersMatch( c.GetParameters(), constructorSymbol.Parameters ) );

            if ( constructor == null )
            {
                throw new InvalidOperationException( $"Could not load type {constructorSymbol.ContainingType}." );
            }
            
            var parameters = attribute.ConstructorArguments.Select(
                (a, i) => this.TranslateAttributeArgument(a, constructor.GetParameters()[i].ParameterType)).ToArray();
            var result = (Attribute?) constructor.Invoke(parameters);


            foreach (var (name, value) in attribute.NamedArguments)
            {
                PropertyInfo? property;
                FieldInfo? field;

                if ((property = type.GetProperty(name)) != null)
                {
                    property.SetValue(result, this.TranslateAttributeArgument(value, property.PropertyType));
                }
                else if ((field = type.GetField(name)) != null)
                {
                    field.SetValue(result, this.TranslateAttributeArgument(value, field.FieldType));
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot find a field or property {name} in type {constructor.DeclaringType.Name}");
                }
            }

            return result;
        }

        private object? TranslateAttributeArgument( object? roslynArgument, Type targetType )
        {
            if ( roslynArgument == null )
            {
                return null;
            }

            switch ( roslynArgument )
            {
                case IType type:
                    if ( !targetType.IsAssignableFrom( typeof( Type ) ) )
                    {
                        throw new InvalidOperationException( $"System.Type can't be assigned to {targetType}" );
                    }

                    return this._compileTimeTypeResolver.GetCompileTimeType( type.GetSymbol(), true );

                case IReadOnlyList<object?> list:
                    if ( !targetType.IsArray )
                    {
                        throw new InvalidOperationException( $"Array can't be assigned to {targetType}" );
                    }

                    var array = Array.CreateInstance( targetType.GetElementType()!, list.Count );

                    for ( var i = 0; i < list.Count; i++ )
                    {
                        array.SetValue( this.TranslateAttributeArgument( list[i], targetType.GetElementType()! ), i );
                    }

                    return array;

                default:
                    if ( targetType.IsEnum )
                    {
                        return Enum.ToObject( targetType, roslynArgument );
                    }

                    if ( roslynArgument != null && !targetType.IsInstanceOfType( roslynArgument ) )
                    {
                        throw new InvalidOperationException( $"{roslynArgument.GetType()} can't be assigned to {targetType}" );
                    }

                    return roslynArgument;
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