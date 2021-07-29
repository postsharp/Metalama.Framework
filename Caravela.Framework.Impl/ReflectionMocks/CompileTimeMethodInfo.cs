// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeMethodInfo : MethodInfo, ICompileTimeReflectionObject<IMethod>
    {
        public IDeclarationRef<IMethod> Target { get; set; }

        private CompileTimeMethodInfo( IMethod method )
        {
            this.Target = method.ToRef();
        }

        public static MethodInfo Create( IMethod method )
        {
            return new CompileTimeMethodInfo( method );
        }

        public static ITypeSymbol? FindDeclaringTypeSymbol( MemberOrNamedType method )
        {
            var methodDeclaringType = (method.DeclaringType as ITypeInternal)!;
            var typeSymbol = methodDeclaringType.TypeSymbol;

            if ( typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeParameters.Length > 0 )
            {
                return namedTypeSymbol;
            }

            return null;
        }

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodImplAttributes GetMethodImplementationFlags() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override ParameterInfo[] GetParameters() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object Invoke( object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodAttributes Attributes => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override RuntimeMethodHandle MethodHandle => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetBaseDefinition() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}