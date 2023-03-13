// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal sealed class CompileTimeType : Type, ICompileTimeReflectionObject<IType>, ICompileTimeType
    {
        // We store a reference-typed ISdkRef instead of the value-typed Ref because it is only being accessed
        // through ICompileTimeReflectionObject, so boxing cannot be avoided anyway. It is better in this case
        // to box only once.
        internal ISdkRef<IType> Target { get; }

        ISdkRef<IType> ICompileTimeReflectionObject<IType>.Target => this.Target;

        private static Exception CreateNotSupportedException() => CompileTimeMocksHelper.CreateNotSupportedException( "Type" );

        private CompileTimeType( ISdkRef<IType> targetRef, ITypeSymbol symbolForMetadata )
        {
            this.Namespace = symbolForMetadata.ContainingNamespace.GetFullName();
            this.Name = symbolForMetadata.GetReflectionName().AssertNotNull();
            this.FullName = symbolForMetadata.GetReflectionFullName().AssertNotNull();
            this._toStringName = symbolForMetadata.GetReflectionToStringName().AssertNotNull();

            this.Target = targetRef;
        }

        internal static CompileTimeType CreateFromSymbolId( SymbolId symbolId, ITypeSymbol symbolForMetadata )
            => new( Ref.FromSymbolId<IType>( symbolId ), symbolForMetadata );

        // For test only.
        internal static CompileTimeType Create( IType type ) => Create( type.GetSymbol(), type.GetCompilationModel().CompilationContext );

        // For test only.
        private static CompileTimeType Create( ITypeSymbol typeSymbol, CompilationContext compilation )
            => new( Ref.FromSymbol<IType>( typeSymbol, compilation ), typeSymbol );

        public override string? Namespace { get; }

        public override string Name { get; }

        public override string FullName { get; }

        private readonly string _toStringName;

        public override object[] GetCustomAttributes( bool inherit ) => throw CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CreateNotSupportedException();

        public override Module Module => throw CreateNotSupportedException();

        protected override TypeAttributes GetAttributeFlagsImpl() => throw CreateNotSupportedException();

        protected override ConstructorInfo GetConstructorImpl(
            BindingFlags bindingAttr,
            Binder? binder,
            CallingConventions callConvention,
            Type[] types,
            ParameterModifier[]? modifiers )
            => throw CreateNotSupportedException();

        public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override Type GetElementType() => throw CreateNotSupportedException();

        public override EventInfo GetEvent( string name, BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override EventInfo[] GetEvents( BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override FieldInfo GetField( string name, BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override FieldInfo[] GetFields( BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override MemberInfo[] GetMembers( BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        protected override MethodInfo GetMethodImpl(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            CallingConventions callConvention,
            Type[]? types,
            ParameterModifier[]? modifiers )
            => throw CreateNotSupportedException();

        public override MethodInfo[] GetMethods( BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override PropertyInfo[] GetProperties( BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override object InvokeMember(
            string name,
            BindingFlags invokeAttr,
            Binder? binder,
            object? target,
            object?[]? args,
            ParameterModifier[]? modifiers,
            CultureInfo? culture,
            string[]? namedParameters )
            => throw CreateNotSupportedException();

        public override Type UnderlyingSystemType => throw CreateNotSupportedException();

        public override bool IsGenericType => throw CreateNotSupportedException();

        protected override bool IsArrayImpl() => throw CreateNotSupportedException();

        protected override bool IsByRefImpl() => throw CreateNotSupportedException();

        protected override bool IsCOMObjectImpl() => throw CreateNotSupportedException();

        protected override bool IsPointerImpl() => throw CreateNotSupportedException();

        protected override bool IsPrimitiveImpl() => throw CreateNotSupportedException();

        public override Assembly Assembly => throw CreateNotSupportedException();

        public override string AssemblyQualifiedName => throw CreateNotSupportedException();

        public override Type BaseType => throw CreateNotSupportedException();

        public override Guid GUID => throw CreateNotSupportedException();

        protected override PropertyInfo GetPropertyImpl(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type? returnType,
            Type[]? types,
            ParameterModifier[]? modifiers )
            => throw CreateNotSupportedException();

        protected override bool HasElementTypeImpl() => throw CreateNotSupportedException();

        public override Type GetNestedType( string name, BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override Type[] GetNestedTypes( BindingFlags bindingAttr ) => throw CreateNotSupportedException();

        public override Type GetInterface( string name, bool ignoreCase ) => throw CreateNotSupportedException();

        public override Type[] GetInterfaces() => throw CreateNotSupportedException();

        public override string ToString() => this._toStringName;
    }
}