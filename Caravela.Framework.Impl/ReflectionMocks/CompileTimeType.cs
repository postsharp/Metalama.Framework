// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    // This class must be public because it is referenced from compiled templates.
    public class CompileTimeType : Type, ICompileTimeReflectionObject<IType>
    {
        private readonly string _fullName;

        internal IDeclarationRef<IType> Target { get; }

        IDeclarationRef<IType> ICompileTimeReflectionObject<IType>.Target => this.Target;

        private CompileTimeType( IDeclarationRef<IType> typeSymbol, string fullName )
        {
            this._fullName = fullName;
            this.Target = typeSymbol;
        }

        public static Type CreateFromDocumentationId( string documentationId, string fullName )
            => new CompileTimeType( DeclarationRef.FromDocumentationId<IType>( documentationId ), fullName );

        internal static Type Create( IType type ) => Create( type.GetSymbol() );

        internal static Type Create( ITypeSymbol typeSymbol )
            => new CompileTimeType( DeclarationRef.FromSymbol<IType>( typeSymbol ), typeSymbol.ToDisplayString() );

        public override string Namespace => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string FullName => this._fullName;

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Module Module => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override TypeAttributes GetAttributeFlagsImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override ConstructorInfo GetConstructorImpl(
            BindingFlags bindingAttr,
            Binder binder,
            CallingConventions callConvention,
            Type[] types,
            ParameterModifier[] modifiers )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type GetElementType() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override EventInfo GetEvent( string name, BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override EventInfo[] GetEvents( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override FieldInfo GetField( string name, BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override FieldInfo[] GetFields( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MemberInfo[] GetMembers( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override MethodInfo GetMethodImpl(
            string name,
            BindingFlags bindingAttr,
            Binder binder,
            CallingConventions callConvention,
            Type[] types,
            ParameterModifier[] modifiers )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo[] GetMethods( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override PropertyInfo[] GetProperties( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object InvokeMember(
            string name,
            BindingFlags invokeAttr,
            Binder binder,
            object target,
            object[] args,
            ParameterModifier[] modifiers,
            CultureInfo culture,
            string[] namedParameters )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type UnderlyingSystemType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsArrayImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsByRefImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsCOMObjectImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsPointerImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsPrimitiveImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Assembly Assembly => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string AssemblyQualifiedName => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type BaseType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Guid GUID => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override PropertyInfo GetPropertyImpl(
            string name,
            BindingFlags bindingAttr,
            Binder binder,
            Type returnType,
            Type[] types,
            ParameterModifier[] modifiers )
            => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool HasElementTypeImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type GetNestedType( string name, BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type[] GetNestedTypes( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type GetInterface( string name, bool ignoreCase ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type[] GetInterfaces() => throw CompileTimeMocksHelper.CreateNotSupportedException();
    }
}