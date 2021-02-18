using System;
using System.Globalization;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaType : Type
    {
        public ITypeSymbol Symbol { get; }

        public CaravelaType( ITypeSymbol symbol )
        {
            this.Symbol = symbol;
        }

        public static CaravelaType Create( IType type )
        {
            return new CaravelaType( ((type as ITypeInternal)!).TypeSymbol );
        }

        public override object[] GetCustomAttributes( bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Module Module => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override string Namespace => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override string Name => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override TypeAttributes GetAttributeFlagsImpl() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override ConstructorInfo GetConstructorImpl( BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type GetElementType() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override EventInfo GetEvent( string name, BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override EventInfo[] GetEvents( BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override FieldInfo GetField( string name, BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override FieldInfo[] GetFields( BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override MemberInfo[] GetMembers( BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override MethodInfo[] GetMethods( BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override PropertyInfo[] GetProperties( BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type UnderlyingSystemType => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override bool IsArrayImpl() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override bool IsByRefImpl() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override bool IsCOMObjectImpl() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override bool IsPointerImpl() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override bool IsPrimitiveImpl() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Assembly Assembly => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override string AssemblyQualifiedName => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type BaseType => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override string FullName => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Guid GUID => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        protected override bool HasElementTypeImpl() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type GetNestedType( string name, BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type[] GetNestedTypes( BindingFlags bindingAttr ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type GetInterface( string name, bool ignoreCase ) => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();

        public override Type[] GetInterfaces() => throw ReflectionSerializationExceptionHelper.CreateNotSupportedException();
    }
}