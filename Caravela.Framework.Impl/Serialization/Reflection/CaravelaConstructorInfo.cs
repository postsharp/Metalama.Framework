using System;
using System.Globalization;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaConstructorInfo : ConstructorInfo, ICaravelaMethodOrConstructorInfo
    {
        public ISymbol Symbol { get; }

        public ITypeSymbol? DeclaringTypeSymbol { get; }

        public CaravelaConstructorInfo( Constructor method )
        {
            this.Symbol = method.Symbol;
            this.DeclaringTypeSymbol = CaravelaMethodInfo.FindDeclaringTypeSymbol( method );
        }

        public static CaravelaConstructorInfo Create( IConstructor method )
        {
            return new CaravelaConstructorInfo( (Constructor) method );
        }

        public override object[] GetCustomAttributes( bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override string Name => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override MethodImplAttributes GetMethodImplementationFlags() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override ParameterInfo[] GetParameters() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override object Invoke( object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override MethodAttributes Attributes => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override RuntimeMethodHandle MethodHandle => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override object Invoke( BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();
    }
}