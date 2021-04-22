// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class PseudoAccessor : IMethod
    {
        private readonly IMember _containingMember;
        private readonly PseudoAccessorSemantic _semantic;

        public PseudoAccessor( IMember containingMember, PseudoAccessorSemantic semantic )
        {
            this._containingMember = containingMember;
            this._semantic = semantic;
        }

        [Memo]
        public IParameter ReturnParameter => new ReturnParam( this );

        [Memo]
        public IType ReturnType
            => this._semantic != PseudoAccessorSemantic.Get
                ? ((CompilationModel) this._containingMember.Compilation).Factory.GetTypeByReflectionType( typeof(void) )
                : ((IProperty) this._containingMember).Type;

        [Memo]
        public IGenericParameterList GenericParameters => new GenericParameterList( this, Enumerable.Empty<CodeElementLink<IGenericParameter>>() );

        [Memo]
        public IReadOnlyList<IType> GenericArguments => ImmutableList<IType>.Empty;

        public bool IsOpenGeneric => this._containingMember.DeclaringType.IsOpenGeneric;

        public bool HasBase => false;

        public IMethodInvocation Base => throw new InvalidOperationException();

        public IMethod? OverriddenMethod => null;

        public IMethodList LocalFunctions => MethodList.Empty;

        public IParameterList Parameters => throw new NotImplementedException();

        public MethodKind MethodKind
            => this._semantic switch
            {
                PseudoAccessorSemantic.Get => MethodKind.PropertyGet,
                PseudoAccessorSemantic.Set => MethodKind.PropertySet,
                PseudoAccessorSemantic.Add => MethodKind.EventAdd,
                PseudoAccessorSemantic.Remove => MethodKind.EventRemove,
                PseudoAccessorSemantic.Raise => MethodKind.EventRaise,
                _ => throw new NotSupportedException()
            };

        public Accessibility Accessibility => this._containingMember.Accessibility;

        public string Name
            => this._semantic switch
            {
                PseudoAccessorSemantic.Get => $"get_{this._containingMember.Name}",
                PseudoAccessorSemantic.Set => $"set_{this._containingMember.Name}",
                PseudoAccessorSemantic.Add => $"add_{this._containingMember.Name}",
                PseudoAccessorSemantic.Remove => $"remove_{this._containingMember.Name}",
                PseudoAccessorSemantic.Raise => $"raise_{this._containingMember.Name}",
                _ => throw new NotSupportedException()
            };

        public bool IsAbstract => false;

        public bool IsStatic => this._containingMember.IsStatic;

        public bool IsVirtual => false;

        public bool IsSealed => false;

        public bool IsReadOnly => false;

        public bool IsOverride => false;

        public bool IsNew => this._containingMember.IsNew;

        public bool IsAsync => false;

        public INamedType DeclaringType => this._containingMember.DeclaringType;

        public CodeOrigin Origin => CodeOrigin.Source;

        public ICodeElement? ContainingElement => this._containingMember;

        public IAttributeList Attributes => AttributeList.Empty;

        public CodeElementKind ElementKind => CodeElementKind.Method;

        public IDiagnosticLocation? DiagnosticLocation => this._containingMember.DiagnosticLocation;

        public ICompilation Compilation => this._containingMember.Compilation;

        public dynamic Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public IMethod WithGenericArguments( params IType[] genericArguments ) => throw new NotSupportedException();

        private sealed class ReturnParam : IParameter
        {
            public PseudoAccessor DeclaringAccessor { get; }

            public IMember DeclaringMember => this.DeclaringAccessor;

            public RefKind RefKind
                => this.DeclaringAccessor.ContainingElement switch
                {
                    Property property => property.RefKind,
                    Field _ => RefKind.None,
                    Event _ => RefKind.None,
                    _ => throw new AssertionFailedException()
                };

            public IType ParameterType => this.DeclaringAccessor.ReturnType;

            public string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );

            public int Index => -1;

            public TypedConstant DefaultValue => default;

            public bool IsParams => false;

            public CodeOrigin Origin => CodeOrigin.Source;

            public ICodeElement? ContainingElement => this.DeclaringAccessor;

            public IAttributeList Attributes => throw new NotImplementedException();

            public CodeElementKind ElementKind => CodeElementKind.Parameter;

            public IDiagnosticLocation? DiagnosticLocation => throw new NotImplementedException();

            public ICompilation Compilation => throw new NotImplementedException();

            public ReturnParam( PseudoAccessor declaringAccessor )
            {
                this.DeclaringAccessor = declaringAccessor;
            }

            public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();
        }
    }
}