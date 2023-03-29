// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Code;

internal class NullableReferenceType : Type
{
    public Type UnderlyingType { get; }

    internal NullableReferenceType( Type underlyingType )
    {
        if ( underlyingType is NullableReferenceType )
        {
            throw new ArgumentException( "Can't create nullable type of nullable type." );
        }

        if ( underlyingType.IsValueType )
        {
            throw new ArgumentException( "Can't create nullable reference type of value type." );
        }

#if DEBUG
        if ( underlyingType is ICompileTimeType )
        {
            throw new ArgumentException( "This type shouldn't be used to represent nullable compile-time types." );
        }
#endif

        this.UnderlyingType = underlyingType;
    }

    public override object[] GetCustomAttributes( bool inherit ) => this.UnderlyingType.GetCustomAttributes( inherit );

    public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => this.UnderlyingType.GetCustomAttributes( attributeType, inherit );

    public override bool IsDefined( Type attributeType, bool inherit ) => this.UnderlyingType.IsDefined( attributeType, inherit );

    public override Module Module => this.UnderlyingType.Module;

    public override string? Namespace => this.UnderlyingType.Namespace;

    public override string Name => this.UnderlyingType.Name;

    protected override TypeAttributes GetAttributeFlagsImpl() => this.UnderlyingType.Attributes;

    protected override ConstructorInfo? GetConstructorImpl( BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers )
        => this.UnderlyingType.GetConstructor( bindingAttr, binder, callConvention, types, modifiers );

    public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr ) => this.UnderlyingType.GetConstructors( bindingAttr );

    public override Type? GetElementType() => this.UnderlyingType.GetElementType();

    public override EventInfo? GetEvent( string name, BindingFlags bindingAttr ) => this.UnderlyingType.GetEvent( name, bindingAttr );

    public override EventInfo[] GetEvents( BindingFlags bindingAttr ) => this.UnderlyingType.GetEvents( bindingAttr );

    public override FieldInfo? GetField( string name, BindingFlags bindingAttr ) => this.UnderlyingType.GetField( name, bindingAttr );

    public override FieldInfo[] GetFields( BindingFlags bindingAttr ) => this.UnderlyingType.GetFields( bindingAttr );

    public override MemberInfo[] GetMembers( BindingFlags bindingAttr ) => this.UnderlyingType.GetMembers( bindingAttr );

    protected override MethodInfo? GetMethodImpl(
        string name,
        BindingFlags bindingAttr,
        Binder? binder,
        CallingConventions callConvention,
        Type[]? types,
        ParameterModifier[]? modifiers )
        => this.UnderlyingType.GetMethod( name, bindingAttr, binder, callConvention, types!, modifiers );

    public override MethodInfo[] GetMethods( BindingFlags bindingAttr ) => this.UnderlyingType.GetMethods( bindingAttr );

    public override PropertyInfo[] GetProperties( BindingFlags bindingAttr ) => this.UnderlyingType.GetProperties( bindingAttr );

    public override object? InvokeMember(
        string name,
        BindingFlags invokeAttr,
        Binder? binder,
        object? target,
        object?[]? args,
        ParameterModifier[]? modifiers,
        CultureInfo? culture,
        string[]? namedParameters )
        => this.UnderlyingType.InvokeMember( name, invokeAttr, binder, target, args, modifiers, culture, namedParameters );

    public override Type UnderlyingSystemType => this.UnderlyingType.UnderlyingSystemType;

    protected override bool IsArrayImpl() => this.UnderlyingType.IsArray;

    protected override bool IsByRefImpl() => this.UnderlyingType.IsByRef;

    protected override bool IsCOMObjectImpl() => this.UnderlyingType.IsCOMObject;

    protected override bool IsPointerImpl() => this.UnderlyingType.IsPointer;

    protected override bool IsPrimitiveImpl() => this.UnderlyingType.IsPrimitive;

    public override Assembly Assembly => this.UnderlyingType.Assembly;

    public override string? AssemblyQualifiedName => this.UnderlyingType.AssemblyQualifiedName;

    public override Type? BaseType => this.UnderlyingType.BaseType;

    public override string? FullName => this.UnderlyingType.FullName;

    public override Guid GUID => this.UnderlyingType.GUID;

    protected override PropertyInfo? GetPropertyImpl( string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers )
        => this.UnderlyingType.GetProperty( name, bindingAttr, binder, returnType, types!, modifiers );

    protected override bool HasElementTypeImpl() => this.UnderlyingType.HasElementType;

    public override Type? GetNestedType( string name, BindingFlags bindingAttr ) => this.UnderlyingType.GetNestedType( name, bindingAttr );

    public override Type[] GetNestedTypes( BindingFlags bindingAttr ) => this.UnderlyingType.GetNestedTypes( bindingAttr );

    public override Type? GetInterface( string name, bool ignoreCase ) => this.UnderlyingType.GetInterface( name, ignoreCase );

    public override Type[] GetInterfaces() => this.UnderlyingType.GetInterfaces();
}