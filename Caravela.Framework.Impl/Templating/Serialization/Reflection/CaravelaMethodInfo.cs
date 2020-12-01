using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
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
        public static ITypeSymbol? FindDeclaringTypeSymbol( Method method )
        {
            ITypeInternal methodDeclaringType = (method.DeclaringType as ITypeInternal)!;
            ITypeSymbol typeSymbol = methodDeclaringType.TypeSymbol;
            if ( typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeParameters.Length > 0)
            {
                return namedTypeSymbol;
            }
            else
            {
                return null;
            }
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