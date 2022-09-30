// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal class CompileTimeMethodInfo : MethodInfo, ICompileTimeReflectionObject<IMethod>
    {
        public ISdkRef<IMethod> Target { get; set; }

        private CompileTimeMethodInfo( IMethod method )
        {
            this.Target = method.ToTypedRef();
        }
        
        private static Exception CreateNotSupportedException() => CompileTimeMocksHelper.CreateNotSupportedException( "MethodInfo" );

        public static MethodInfo Create( IMethod method )
        {
            return new CompileTimeMethodInfo( method );
        }

        public override object[] GetCustomAttributes( bool inherit ) => throw CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CreateNotSupportedException();

        public override Type DeclaringType => throw CreateNotSupportedException();

        public override string Name => throw CreateNotSupportedException();

        public override Type ReflectedType => throw CreateNotSupportedException();

        public override MethodImplAttributes GetMethodImplementationFlags() => throw CreateNotSupportedException();

        public override ParameterInfo[] GetParameters() => throw CreateNotSupportedException();

        public override object Invoke( object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture )
            => throw CreateNotSupportedException();

        public override MethodAttributes Attributes => throw CreateNotSupportedException();

        public override RuntimeMethodHandle MethodHandle => throw CreateNotSupportedException();

        public override MethodInfo GetBaseDefinition() => throw CreateNotSupportedException();

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw CreateNotSupportedException();
    }
}