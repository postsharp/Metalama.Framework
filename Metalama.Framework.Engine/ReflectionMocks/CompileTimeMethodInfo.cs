// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.SyntaxSerialization;
using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal sealed class CompileTimeMethodInfo : MethodInfo, ICompileTimeReflectionObject<IMethod>
    {
        public ISdkRef<IMethod> Target { get; }

        private CompileTimeMethodInfo( IMethod method )
        {
            this.Target = method.ToValueTypedRef();
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

        public bool IsAssignable => false;

        public IType Type => TypeFactory.GetType( typeof(MethodInfo) );

        public Type ReflectionType => typeof(MethodInfo);

        public RefKind RefKind => RefKind.None;

        public ref object? Value => ref RefHelper.Wrap( this );

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => CompileTimeMocksHelper.ToTypedExpressionSyntax( this, CompileTimeMethodInfoSerializer.SerializeMethodBase, syntaxGenerationContext );
    }
}