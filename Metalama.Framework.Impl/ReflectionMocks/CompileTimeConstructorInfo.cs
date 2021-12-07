// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.CodeModel.References;
using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeConstructorInfo : ConstructorInfo, ICompileTimeReflectionObject<IConstructor>
    {
        public ISdkRef<IConstructor> Target { get; }

        private CompileTimeConstructorInfo( IConstructor method )
        {
            this.Target = method.ToTypedRef();
        }

        public static ConstructorInfo Create( IConstructor method ) => new CompileTimeConstructorInfo( method );

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

        public override object Invoke( BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}