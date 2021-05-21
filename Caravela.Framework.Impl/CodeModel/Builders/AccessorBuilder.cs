// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Project;
using System;
using System.Collections.Generic;
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
            => (this._containingDeclaration, this.MethodKind) switch
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
            get => this._accessibility ?? this._containingDeclaration.Accessibility;

            // TODO: Changing accessibility of all accessors at the same time should be prohibited or should change the visibility of the method group.
            // TODO: Throw if the set accessibility does not restrict the property/event accessibility.
            set => this._accessibility = value;
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

        public bool IsStatic { get => this._containingDeclaration.IsStatic; set => throw new NotSupportedException(); }

        public bool IsVirtual { get => this._containingDeclaration.IsVirtual; set => throw new NotSupportedException(); }

        public bool IsSealed { get => this._containingDeclaration.IsSealed; set => throw new NotSupportedException(); }

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

        public dynamic Invoke( dynamic? instance, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            throw new NotSupportedException( "Cannot add generic parameters to accessors." );
        }

        [return: RunTimeOnly]
        public MethodInfo ToMethodInfo()
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public System.Reflection.MethodBase ToMethodBase()
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public MemberInfo ToMemberInfo()
        {
            throw new NotImplementedException();
        }
    }
}