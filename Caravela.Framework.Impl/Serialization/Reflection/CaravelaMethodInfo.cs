using System;
using System.Globalization;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaMethodInfo : MethodInfo, ICaravelaMethodOrConstructorInfo
    {
        public ISymbol Symbol { get; }

        public ITypeSymbol? DeclaringTypeSymbol { get; }

        public CaravelaMethodInfo( Method method )
        {
            this.Symbol = method.Symbol;
            this.DeclaringTypeSymbol = FindDeclaringTypeSymbol( method );
        }

        public static CaravelaMethodInfo Create( IMethod method )
        {
            return new CaravelaMethodInfo( (method as Method)! );
        }

        public static ITypeSymbol? FindDeclaringTypeSymbol( Member method )
        {
            var methodDeclaringType = (method.DeclaringType as ITypeInternal)!;
            var typeSymbol = methodDeclaringType.TypeSymbol;
            if ( typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeParameters.Length > 0 )
            {
                return namedTypeSymbol;
            }
            else
            {
                return null;
            }
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

        public override MethodInfo GetBaseDefinition() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();
    }
}