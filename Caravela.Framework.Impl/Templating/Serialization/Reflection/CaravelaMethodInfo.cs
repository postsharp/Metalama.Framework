using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaMethodInfo : MethodInfo
    {
        public ISymbol Symbol { get; }

        public CaravelaMethodInfo( ISymbol symbol )
        {
            this.Symbol = symbol;
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
        public override MethodInfo GetBaseDefinition() => throw new NotImplementedException();

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();
    }
}