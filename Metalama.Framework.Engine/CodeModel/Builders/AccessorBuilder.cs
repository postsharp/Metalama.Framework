// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder : DeclarationBuilder, IMethodBuilder, IMethodImpl
    {
        public MemberBuilder ContainingMember { get; }

        private Accessibility? _accessibility;

        public AccessorBuilder( MemberBuilder containingDeclaration, MethodKind methodKind, bool isImplicit ) : base( containingDeclaration.ParentAdvice )
        {
            this.ContainingMember = containingDeclaration;
            this._accessibility = null;
            this.MethodKind = methodKind;
            this.IsImplicitlyDeclared = isImplicit;
        }

        [Memo]
        public IParameterBuilder ReturnParameter
            => (containingDeclaration: this.ContainingDeclaration, this.MethodKind) switch
            {
                (PropertyBuilder _, MethodKind.PropertyGet) => new PropertyGetReturnParameter( this ),
                (PropertyBuilder _, MethodKind.PropertySet) => new VoidReturnParameter( this ),
                (FieldBuilder _, MethodKind.PropertyGet) => new PropertyGetReturnParameter( this ),
                (FieldBuilder _, MethodKind.PropertySet) => new VoidReturnParameter( this ),
                (EventBuilder _, _) => new EventReturnParameter( this ),
                _ => throw new AssertionFailedException()
            };

        public IType ReturnType
        {
            get => this.ReturnParameter.Type;
            set => throw new NotSupportedException( "Cannot directly change the return type of an accessor." );
        }

        [Memo]
        public IGenericParameterList TypeParameters => TypeParameterList.Empty;

        public IReadOnlyList<IType> TypeArguments => ImmutableArray<IType>.Empty;

        public override bool IsImplicitlyDeclared { get; }

        public bool IsOpenGeneric => false;

        public bool IsGeneric => false;

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>(
                ( order, invokerOperator )
                    => new MethodInvoker( this, order, invokerOperator ),
                false );

        public IMethod? OverriddenMethod
            => (containingDeclaration: this.ContainingDeclaration, this.MethodKind) switch
            {
                (PropertyBuilder propertyBuilder, MethodKind.PropertyGet) => propertyBuilder.OverriddenProperty?.GetMethod.AssertNotNull(),
                (PropertyBuilder propertyBuilder, MethodKind.PropertySet) => propertyBuilder.OverriddenProperty?.SetMethod.AssertNotNull(),
                (FieldBuilder _, _) => null,
                (EventBuilder eventBuilder, MethodKind.EventAdd) => eventBuilder.OverriddenEvent?.AddMethod.AssertNotNull(),
                (EventBuilder eventBuilder, MethodKind.EventRemove) => eventBuilder.OverriddenEvent?.RemoveMethod.AssertNotNull(),
                _ => throw new AssertionFailedException()
            };

        IParameterList IHasParameters.Parameters => this.Parameters;

        [Memo]
        public ParameterBuilderList Parameters
            => (this.ContainingMember, this.MethodKind) switch
            {
                // TODO: Indexer parameters (need to have special IParameterList implementation that would mirror adding parameters to the indexer property).
                (IProperty, MethodKind.PropertyGet) => new ParameterBuilderList(),
                (IProperty, MethodKind.PropertySet) =>
                    new ParameterBuilderList( new[] { new PropertySetValueParameter( this, 0 ) } ),
                (FieldBuilder _, MethodKind.PropertyGet) => new ParameterBuilderList(),
                (FieldBuilder _, MethodKind.PropertySet) => new ParameterBuilderList( new[] { new PropertySetValueParameter( this, 0 ) } ),
                (IEvent _, _) =>
                    new ParameterBuilderList( new[] { new EventValueParameter( this ) } ),
                _ => throw new AssertionFailedException()
            };

        public MethodKind MethodKind { get; }

        public OperatorKind OperatorKind => OperatorKind.None;

        IMethod IMethod.MethodDefinition => this;

        public Accessibility Accessibility
        {
            get => this._accessibility ?? this.ContainingMember.Accessibility;

            set
            {
                if ( this.ContainingDeclaration is FieldBuilder )
                {
                    throw new InvalidOperationException( "Cannot change field pseudo accessor accessibility." );
                }

                if ( this.ContainingDeclaration is not PropertyBuilder propertyBuilder )
                {
                    throw new InvalidOperationException( $"Cannot change event accessor accessibility." );
                }

                if ( !value.IsSubsetOrEqual( propertyBuilder.Accessibility ) )
                {
                    throw new InvalidOperationException(
                        $"Cannot change accessor accessibility to {value}, which is not more restrictive than parent accessibility {propertyBuilder.Accessibility}." );
                }

                var otherAccessor = this.MethodKind switch
                {
                    MethodKind.PropertyGet => propertyBuilder.SetMethod,
                    MethodKind.PropertySet => propertyBuilder.GetMethod,
                    _ => throw new AssertionFailedException()
                };

                if ( value != propertyBuilder.Accessibility && otherAccessor == null )
                {
                    throw new InvalidOperationException( $"Cannot change accessor accessibility, if the property has a single accessor ." );
                }

                if ( value != propertyBuilder.Accessibility && otherAccessor != null
                                                            && otherAccessor.Accessibility.IsSubsetOf( propertyBuilder.Accessibility ) )
                {
                    throw new InvalidOperationException(
                        $"Cannot change accessor accessibility to {value}, because the other accessor is already restricted to {otherAccessor.Accessibility}." );
                }

                this._accessibility = value;
            }
        }

        public string Name
        {
            get
                => this.MethodKind switch
                {
                    MethodKind.PropertyGet => $"get_{this.ContainingMember.Name}",
                    MethodKind.PropertySet => $"set_{this.ContainingMember.Name}",
                    MethodKind.EventAdd => $"add_{this.ContainingMember.Name}",
                    MethodKind.EventRemove => $"remove_{this.ContainingMember.Name}",
                    _ => throw new AssertionFailedException()
                };
            set => throw new NotSupportedException();
        }

        public bool IsStatic
        {
            get => this.ContainingMember.IsStatic;
            set => throw new NotSupportedException( "Cannot directly change staticity of an accessor." );
        }

        public bool IsVirtual
        {
            get => this.ContainingMember.IsVirtual;
            set => throw new NotSupportedException( "Cannot directly change the IsVirtual property of an accessor." );
        }

        public bool IsSealed
        {
            get => this.ContainingMember.IsSealed;
            set => throw new NotSupportedException( "Cannot directly change the IsSealed property of an accessor." );
        }

        public bool IsAbstract => this.ContainingMember.IsAbstract;

        public bool IsReadOnly => false;

        public bool IsOverride => this.ContainingMember.IsOverride;

        public bool IsNew => this.ContainingMember.IsNew;

        public bool IsAsync => false;

        public INamedType DeclaringType => this.ContainingMember.DeclaringType;

        public override IDeclaration? ContainingDeclaration => this.ContainingMember;

        public override DeclarationKind DeclarationKind => DeclarationKind.Method;

        IParameter IMethod.ReturnParameter => this.ReturnParameter;

        IType IMethod.ReturnType => this.ReturnType;

        Accessibility IMemberOrNamedType.Accessibility => this.Accessibility;

        string INamedDeclaration.Name => this.Name;

        bool IMemberOrNamedType.IsStatic => this.IsStatic;

        bool IMember.IsVirtual => this.IsVirtual;

        bool IMemberOrNamedType.IsSealed => this.IsSealed;

        public ITypeParameterBuilder AddTypeParameter( string name ) => throw new NotSupportedException( "Cannot add generic parameters to accessors." );

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
            => throw new NotSupportedException( "Cannot directly add parameters to accessors." );

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
            => throw new NotSupportedException( "Cannot directly add parameters to accessors." );

        public IGeneric ConstructGenericInstance( params IType[] typeArguments )
            => throw new NotSupportedException( "Cannot add generic parameters to accessors." );

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
            => (containingDeclaration: this.ContainingDeclaration, this.MethodKind) switch
            {
                (PropertyBuilder propertyBuilder, MethodKind.PropertyGet)
                    => propertyBuilder.ExplicitInterfaceImplementations.Select( p => p.GetMethod ).AssertNoneNull().ToArray(),
                (PropertyBuilder propertyBuilder, MethodKind.PropertySet)
                    => propertyBuilder.ExplicitInterfaceImplementations.Select( p => p.SetMethod ).AssertNoneNull().ToArray(),
                (FieldBuilder _, _) => Array.Empty<IMethod>(),
                (EventBuilder eventBuilder, MethodKind.EventAdd)
                    => eventBuilder.ExplicitInterfaceImplementations.Select( p => p.AddMethod ).AssertNoneNull().ToArray(),
                (EventBuilder eventBuilder, MethodKind.EventRemove)
                    => eventBuilder.ExplicitInterfaceImplementations.Select( p => p.RemoveMethod ).AssertNoneNull().ToArray(),
                _ => throw new AssertionFailedException()
            };

        public bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

        IMemberWithAccessors? IMethod.DeclaringMember => (IMemberWithAccessors) this.ContainingMember;

        public System.Reflection.MethodBase ToMethodBase() => throw new NotImplementedException();

        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.ContainingMember.ToDisplayString( this.MethodKind, format, context );

        public IMember? OverriddenMember => (IMemberImpl?) this.OverriddenMethod;

        public override bool CanBeInherited => this.IsVirtual && !this.IsSealed && ((IDeclarationImpl) this.DeclaringType).CanBeInherited;
    }
}