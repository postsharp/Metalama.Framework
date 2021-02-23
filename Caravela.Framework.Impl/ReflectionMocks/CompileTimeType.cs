using System;
using System.Globalization;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeType : Type, IReflectionMockCodeElement
    {
        public ITypeSymbol TypeSymbol { get; }

        public CompileTimeType( ITypeSymbol typeSymbol )
        {
            this.TypeSymbol = typeSymbol;
        }

        public static CompileTimeType Create( IType type )
        {
            return new CompileTimeType( ((ITypeInternal) type).TypeSymbol.AssertNotNull() );
        }

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Module Module => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Namespace => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override TypeAttributes GetAttributeFlagsImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override ConstructorInfo GetConstructorImpl( BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type GetElementType() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override EventInfo GetEvent( string name, BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override EventInfo[] GetEvents( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override FieldInfo GetField( string name, BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override FieldInfo[] GetFields( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MemberInfo[] GetMembers( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo[] GetMethods( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override PropertyInfo[] GetProperties( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type UnderlyingSystemType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsArrayImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsByRefImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsCOMObjectImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsPointerImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool IsPrimitiveImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Assembly Assembly => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string AssemblyQualifiedName => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type BaseType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string FullName => ReflectionNameHelper.GetReflectionName( this.TypeSymbol );

        public override Guid GUID => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        protected override bool HasElementTypeImpl() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type GetNestedType( string name, BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type[] GetNestedTypes( BindingFlags bindingAttr ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type GetInterface( string name, bool ignoreCase ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type[] GetInterfaces() => throw CompileTimeMocksHelper.CreateNotSupportedException();

        ISymbol IReflectionMockCodeElement.Symbol => this.TypeSymbol;
    }
}