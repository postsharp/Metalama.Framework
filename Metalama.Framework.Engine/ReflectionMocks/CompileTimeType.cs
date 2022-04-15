// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    // This class must be public because it is referenced from compiled templates.
    public sealed class CompileTimeType : Type, ICompileTimeReflectionObject<IType>
    {
        // We store a reference-typed ISdkRef instead of the value-typed Ref because it is only being accessed
        // through ICompileTimeReflectionObject, so boxing cannot be avoided anyway. It is better in this case
        // to box only once.
        internal ISdkRef<IType> Target { get; }

        ISdkRef<IType> ICompileTimeReflectionObject<IType>.Target => this.Target;

        private CompileTimeType( ISdkRef<IType> typeSymbol, string fullName )
        {
            if ( string.IsNullOrEmpty( fullName ) )
            {
                throw new ArgumentNullException( nameof(fullName) );
            }

            this.FullName = fullName;
            this.Target = typeSymbol;
        }

        public static Type GetCompileTimeType( string id, string fullMetadataName )
            => UserCodeExecutionContext.Current.ServiceProvider.GetRequiredService<CompileTimeTypeFactory>().Get( new SymbolId( id ), fullMetadataName );

        internal static Type CreateFromSymbolId( SymbolId symbolId, string fullMetadataName )
            => new CompileTimeType( Ref.FromSymbolId<IType>( symbolId ), fullMetadataName );

        // For test only. This is also used from serializers but these used should be removed when serializers will stop using symbols.
        internal static Type Create( IType type ) => Create( type.GetSymbol(), type.GetCompilationModel().RoslynCompilation );

        // For test only.
        internal static Type Create( ITypeSymbol typeSymbol, Compilation compilation )
            => new CompileTimeType( Ref.FromSymbol<IType>( typeSymbol, compilation ), typeSymbol.ToDisplayString() );

        public override string Namespace => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string FullName { get; }

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

        public override string ToString() => this.FullName;

        public override int GetHashCode() => this.Target.GetHashCode();
    }
}