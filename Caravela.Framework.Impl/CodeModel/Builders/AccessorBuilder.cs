// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder : DeclarationBuilder, IMethodBuilder
    {
        private readonly MemberBuilder _containingDeclaration;

        private Accessibility? _accessibility;

        public AccessorBuilder( MemberBuilder containingDeclaration, MethodKind methodKind )
            : base( containingDeclaration.ParentAdvice )
        {
            this._containingDeclaration = containingDeclaration;
            this._accessibility = null;
            this.MethodKind = methodKind;
        }

        [Memo]
        public IParameterBuilder ReturnParameter
            => (containingDeclaration: this.ContainingDeclaration, this.MethodKind) switch
            {
                (PropertyBuilder _, MethodKind.PropertyGet) => new PropertyGetReturnParameter( this ),
                (PropertyBuilder _, MethodKind.PropertySet) => new VoidReturnParameter( this ),
                (EventBuilder _, _) => new EventReturnParameter( this ),
                _ => throw new AssertionFailedException()
            };

        public IType ReturnType
        {
            get => this.ReturnParameter.ParameterType;
            set => throw new NotSupportedException( "Cannot directly change the return type of an accessor." );
        }

        [Memo]
        public IGenericParameterList GenericParameters => GenericParameterList.Empty;

        [Memo]
        public IReadOnlyList<IType> GenericArguments => Array.Empty<IType>();

        public bool IsOpenGeneric => false;

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>(
                ( order, invokerOperator )
                    => new MethodInvoker( this, order, invokerOperator ),
                false );

        public IMethod? OverriddenMethod
            => (containingDeclaration: this.ContainingDeclaration, this.MethodKind) switch
            {
                (PropertyBuilder propertyBuilder, MethodKind.PropertyGet) => propertyBuilder.OverriddenProperty?.Getter.AssertNotNull(),
                (PropertyBuilder propertyBuilder, MethodKind.PropertySet) => propertyBuilder.OverriddenProperty?.Setter.AssertNotNull(),
                (EventBuilder eventBuilder, MethodKind.EventAdd) => eventBuilder.OverriddenEvent?.Adder.AssertNotNull(),
                (EventBuilder eventBuilder, MethodKind.EventRemove) => eventBuilder.OverriddenEvent?.Remover.AssertNotNull(),
                _ => throw new AssertionFailedException()
            };

        // TODO: Local functions from templates will never be visible (which is probably only thing possible).
        public IMethodList LocalFunctions => MethodList.Empty;

        IParameterList IHasParameters.Parameters => this.Parameters;

        public ParameterBuilderList Parameters
            => (this._containingDeclaration, this.MethodKind) switch
            {
                // TODO: Indexer parameters (need to have special IParameterList implementation that would mirror adding parameters to the indexer property).
                (IProperty property, MethodKind.PropertyGet) when property.Parameters.Count == 0 => new ParameterBuilderList(),
                (IProperty property, MethodKind.PropertySet) when property.Parameters.Count == 0 =>
                    new ParameterBuilderList( new[] { new PropertySetValueParameter( this, 0 ) } ),
                (IEvent _, _) =>
                    new ParameterBuilderList( new[] { new EventValueParameter( this ) } ),
                _ => throw new AssertionFailedException()
            };

        public MethodKind MethodKind { get; }

        public Accessibility Accessibility
        {
            get => this._accessibility ?? this._containingDeclaration.Accessibility;

            set
            {
                if ( this.ContainingDeclaration is not PropertyBuilder propertyBuilder )
                {
                    throw new InvalidOperationException( $"Cannot change event accessor accessibility." );
                }

                if ( !value.CompareAccessibility( propertyBuilder.Accessibility ).IsSubsetOrEqual )
                {
                    throw new InvalidOperationException(
                        $"Cannot change accessor accessibility to {value}, which is not more restrictive than parent accessibility {propertyBuilder.Accessibility}." );
                }

                var otherAccessor = this.MethodKind switch
                {
                    MethodKind.PropertyGet => propertyBuilder.Setter,
                    MethodKind.PropertySet => propertyBuilder.Getter,
                    _ => throw new AssertionFailedException()
                };

                if ( otherAccessor == null )
                {
                    throw new InvalidOperationException( $"Cannot change accessor accessibility, if the property has a single accesor ." );
                }

                if ( otherAccessor.Accessibility.CompareAccessibility( propertyBuilder.Accessibility ).IsSubset )
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
                    MethodKind.PropertyGet => $"get_{this._containingDeclaration.Name}",
                    MethodKind.PropertySet => $"set_{this._containingDeclaration.Name}",
                    MethodKind.EventAdd => $"add_{this._containingDeclaration.Name}",
                    MethodKind.EventRemove => $"remove_{this._containingDeclaration.Name}",
                    _ => throw new AssertionFailedException()
                };
            set => throw new NotSupportedException();
        }

        public bool IsStatic
        {
            get => this._containingDeclaration.IsStatic;
            set => throw new NotSupportedException( "Cannot directly change staticity of an accessor." );
        }

        public bool IsVirtual
        {
            get => this._containingDeclaration.IsVirtual;
            set => throw new NotSupportedException( "Cannot directly change virtuality of an accessor." );
        }

        public bool IsSealed
        {
            get => this._containingDeclaration.IsSealed;
            set => throw new NotSupportedException( "Cannot directly change sealedness of an accessor." );
        }

        public bool IsAbstract => this._containingDeclaration.IsAbstract;

        public bool IsReadOnly => false;

        public bool IsOverride => this._containingDeclaration.IsOverride;

        public bool IsNew => this._containingDeclaration.IsNew;

        public bool IsAsync => false;

        public INamedType DeclaringType => this._containingDeclaration.DeclaringType;

        public override IDeclaration? ContainingDeclaration => this._containingDeclaration;

        public override DeclarationKind DeclarationKind => DeclarationKind.Method;

        IParameter IMethod.ReturnParameter => this.ReturnParameter;

        IType IMethod.ReturnType => this.ReturnType;

        Accessibility IMemberOrNamedType.Accessibility => this.Accessibility;

        string IMemberOrNamedType.Name => this.Name;

        bool IMemberOrNamedType.IsStatic => this.IsStatic;

        bool IMember.IsVirtual => this.IsVirtual;

        bool IMemberOrNamedType.IsSealed => this.IsSealed;

        public IGenericParameterBuilder AddGenericParameter( string name ) => throw new NotSupportedException( "Cannot add generic parameters to accessors." );

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default )
            => throw new NotSupportedException( "Cannot directly add parameters to accessors." );

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null )
            => throw new NotSupportedException( "Cannot directly add parameters to accessors." );

        public IMethod WithGenericArguments( params IType[] genericArguments )
            => throw new NotSupportedException( "Cannot add generic parameters to accessors." );

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
            => (containingDeclaration: this.ContainingDeclaration, this.MethodKind) switch
            {
                (PropertyBuilder propertyBuilder, MethodKind.PropertyGet) 
                    => propertyBuilder.ExplicitInterfaceImplementations.Select( p => p.Getter ).AssertNoneNull().ToArray(),
                (PropertyBuilder propertyBuilder, MethodKind.PropertySet) 
                    => propertyBuilder.ExplicitInterfaceImplementations.Select( p => p.Setter ).AssertNoneNull().ToArray(),
                (EventBuilder eventBuilder, MethodKind.EventAdd) 
                    => eventBuilder.ExplicitInterfaceImplementations.Select( p => p.Adder ).AssertNoneNull().ToArray(),
                (EventBuilder eventBuilder, MethodKind.EventRemove) 
                    => eventBuilder.ExplicitInterfaceImplementations.Select( p => p.Remover ).AssertNoneNull().ToArray(),
                _ => throw new AssertionFailedException()
            };

        public bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        [return: RunTimeOnly]
        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

        [return: RunTimeOnly]
        public System.Reflection.MethodBase ToMethodBase() => throw new NotImplementedException();

        [return: RunTimeOnly]
        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this._containingDeclaration.ToDisplayString( format, context ) + "." + this.MethodKind switch
            {
                MethodKind.EventAdd => "add",
                MethodKind.EventRemove => "remove",
                MethodKind.PropertyGet => "get",
                MethodKind.PropertySet => "set",
                MethodKind.EventRaise => "raise",
                _ => this.MethodKind.ToString()
            };
    }
}