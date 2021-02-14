using System;
using System.Globalization;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaConstructorInfo : ConstructorInfo, ICaravelaMethodOrConstructorInfo
    {
        public ISymbol Symbol { get; }

        public ITypeSymbol? DeclaringTypeSymbol { get; }

        public CaravelaConstructorInfo( Method method )
        {
            this.Symbol = method.Symbol;
            this.DeclaringTypeSymbol = CaravelaMethodInfo.FindDeclaringTypeSymbol( method );
        }

        public static CaravelaConstructorInfo Create( IMethod method )
        {
            return new CaravelaConstructorInfo( (Method) method );
        }

        public override object[] GetCustomAttributes( bool inherit ) => throw new NotImplementedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw new NotImplementedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw new NotImplementedException();

        public override Type DeclaringType => throw new NotImplementedException();

        public override string Name => throw new NotImplementedException();

        public override Type ReflectedType => throw new NotImplementedException();

        public override MethodImplAttributes GetMethodImplementationFlags() => throw new NotImplementedException();

        public override ParameterInfo[] GetParameters() => throw new NotImplementedException();

        public override object Invoke( object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture ) => throw new NotImplementedException();

        public override MethodAttributes Attributes => throw new NotImplementedException();

        public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

        public override object Invoke( BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture ) => throw new NotImplementedException();
    }
}