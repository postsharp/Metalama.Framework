using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeMethodInfo : MethodInfo, IReflectionMockMember
    {
        public ISymbol Symbol { get; }

        public ITypeSymbol? DeclaringTypeSymbol { get; }

        public CompileTimeMethodInfo( Method method )
        {
            this.Symbol = method.Symbol;
            this.DeclaringTypeSymbol = FindDeclaringTypeSymbol( method );
        }

        public static CompileTimeMethodInfo Create( IMethod method )
        {
            return new CompileTimeMethodInfo( (method as Method)! );
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

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodImplAttributes GetMethodImplementationFlags() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override ParameterInfo[] GetParameters() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object Invoke( object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodAttributes Attributes => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override RuntimeMethodHandle MethodHandle => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetBaseDefinition() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}