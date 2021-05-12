// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder : CodeElementBuilder, IMethodBuilder
    {
        private readonly MemberBuilder _containingElement;

        private Accessibility? _accessibility;

        public AccessorBuilder( MemberBuilder containingElement, MethodKind methodKind )
            : base( containingElement.ParentAdvice )
        {
            this._containingElement = containingElement;
            this._accessibility = null;
            this.MethodKind = methodKind;
        }

        [Memo]
        public IParameterBuilder ReturnParameter
            => (this.ContainingElement, this.MethodKind) switch
            {
                (PropertyBuilder _, MethodKind.PropertyGet) => new PropertyGetReturnParameter( this, -1 ),
                (PropertyBuilder _, MethodKind.PropertySet) => new VoidReturnParameter( this, -1 ),
                _ => throw new AssertionFailedException()
            };

        public IType ReturnType
        {
            get => this.ReturnParameter.ParameterType;
            set => throw new NotSupportedException();
        }

        [Memo]
        public IGenericParameterList GenericParameters => GenericParameterList.Empty;

        [Memo]
        public IReadOnlyList<IType> GenericArguments => Array.Empty<IType>();

        public bool IsOpenGeneric => false;

        public bool HasBase => throw new NotImplementedException();

        public IMethodInvocation Base => throw new NotImplementedException();

        public IMethod? OverriddenMethod => throw new NotImplementedException();

        // TODO: Local functions from templates will never be visible (which is probably only thing possible).
        public IMethodList LocalFunctions => MethodList.Empty;

        IParameterList IMethodBase.Parameters => this.Parameters;

        public ParameterBuilderList Parameters
            => (this._containingElement, this.MethodKind) switch
            {
                // TODO: Indexer parameters (need to have special IParameterList implementation that would mirror adding parameters to the indexer property).
                // TODO: Events.
                (IProperty property, MethodKind.PropertyGet) when property.Parameters.Count == 0 => new ParameterBuilderList(),
                (IProperty property, MethodKind.PropertySet) when property.Parameters.Count == 0 => new ParameterBuilderList(
                    new[] { new PropertySetValueParameter( this, 0 ) } ),
                _ => throw new AssertionFailedException()
            };

        public MethodKind MethodKind { get; }

        public Accessibility Accessibility
        {
            get => this._accessibility ?? this._containingElement.Accessibility;

            // TODO: Changing accessibility of all accessors at the same time should be prohibited or should change the visibility of the method group.
            // TODO: Throw if the set accessibility does not restrict the property/event accessibility.
            set => this._accessibility = value;
        }

        public string Name
        {
            get
                => this.MethodKind switch
                {
                    MethodKind.PropertyGet => $"get_{this._containingElement.Name}",
                    MethodKind.PropertySet => $"set_{this._containingElement.Name}",
                    MethodKind.EventAdd => $"add_{this._containingElement.Name}",
                    MethodKind.EventRemove => $"remove_{this._containingElement.Name}",
                    _ => throw new AssertionFailedException()
                };
            set => throw new NotSupportedException();
        }

        public bool IsStatic { get => this._containingElement.IsStatic; set => throw new NotSupportedException(); }

        public bool IsVirtual { get => this._containingElement.IsVirtual; set => throw new NotSupportedException(); }

        public bool IsSealed { get => this._containingElement.IsSealed; set => throw new NotSupportedException(); }

        public bool IsAbstract => this._containingElement.IsAbstract;

        public bool IsReadOnly => false;

        public bool IsOverride => this._containingElement.IsOverride;

        public bool IsNew => this._containingElement.IsNew;

        public bool IsAsync => false;

        public INamedType DeclaringType => this._containingElement.DeclaringType;

        public override ICodeElement? ContainingElement => this._containingElement;

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        IParameter IMethod.ReturnParameter => this.ReturnParameter;

        IType IMethod.ReturnType => this.ReturnType;

        Accessibility IMember.Accessibility => this.Accessibility;

        string IMember.Name => this.Name;

        bool IMember.IsStatic => this.IsStatic;

        bool IMember.IsVirtual => this.IsVirtual;

        bool IMember.IsSealed => this.IsSealed;

        public IGenericParameterBuilder AddGenericParameter( string name )
        {
            throw new NotSupportedException( "Cannot add generic parameters to accessors." );
        }

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default )
        {
            throw new NotSupportedException( "Cannot directly add parameters to accessors." );
        }

        public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null )
        {
            throw new NotSupportedException( "Cannot directly add parameters to accessors." );
        }

        public dynamic Invoke( dynamic instance, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            throw new NotSupportedException( "Cannot add generic parameters to accessors." );
        }
    }
}