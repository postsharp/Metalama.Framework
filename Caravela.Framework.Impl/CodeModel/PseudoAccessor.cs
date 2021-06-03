// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class PseudoAccessor : IMethod
    {
        private readonly IMember _containingMember;
        private readonly AccessorSemantic _semantic;

        public PseudoAccessor( IMember containingMember, AccessorSemantic semantic )
        {
            this._containingMember = containingMember;
            this._semantic = semantic;
        }

        [Memo]
        public IParameter ReturnParameter => new ReturnParam( this );

        [Memo]
        public IType ReturnType
            => this._semantic != AccessorSemantic.Get
                ? ((CompilationModel) this._containingMember.Compilation).Factory.GetTypeByReflectionType( typeof(void) )
                : ((IProperty) this._containingMember).Type;

        [Memo]
        public IGenericParameterList GenericParameters => new GenericParameterList( this, Enumerable.Empty<DeclarationRef<IGenericParameter>>() );

        [Memo]
        public IReadOnlyList<IType> GenericArguments => ImmutableList<IType>.Empty;

        public bool IsOpenGeneric => this._containingMember.DeclaringType.IsOpenGeneric;

        public IMethodInvoker Invoker => throw new NotImplementedException();

        public IMethod? OverriddenMethod => null;

        public IMethodList LocalFunctions => MethodList.Empty;

        public IParameterList Parameters => throw new NotImplementedException();

        public MethodKind MethodKind
            => this._semantic switch
            {
                AccessorSemantic.Get => MethodKind.PropertyGet,
                AccessorSemantic.Set => MethodKind.PropertySet,
                AccessorSemantic.Add => MethodKind.EventAdd,
                AccessorSemantic.Remove => MethodKind.EventRemove,
                AccessorSemantic.Raise => MethodKind.EventRaise,
                _ => throw new NotSupportedException()
            };

        public Accessibility Accessibility => this._containingMember.Accessibility;

        public string Name
            => this._semantic switch
            {
                AccessorSemantic.Get => $"get_{this._containingMember.Name}",
                AccessorSemantic.Set => $"set_{this._containingMember.Name}",
                AccessorSemantic.Add => $"add_{this._containingMember.Name}",
                AccessorSemantic.Remove => $"remove_{this._containingMember.Name}",
                AccessorSemantic.Raise => $"raise_{this._containingMember.Name}",
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

        public DeclarationOrigin Origin => DeclarationOrigin.Source;

        public IDeclaration? ContainingDeclaration => this._containingMember;

        public IAttributeList Attributes => AttributeList.Empty;

        public DeclarationKind DeclarationKind => DeclarationKind.Method;

        public bool HasAspect<T>()
            where T : IAspect
            => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public IAnnotationList GetAnnotations<T>()
            where T : IAspect
            => throw new NotImplementedException();

        public IDiagnosticLocation? DiagnosticLocation => this._containingMember.DiagnosticLocation;

        public ICompilation Compilation => this._containingMember.Compilation;

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => Array.Empty<IMethod>();

        [return: RunTimeOnly]
        public MemberInfo ToMemberInfo()
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public System.Reflection.MethodBase ToMethodBase()
        {
            throw new NotImplementedException();
        }

        [return: RunTimeOnly]
        public MethodInfo ToMethodInfo()
        {
            throw new NotImplementedException();
        }

        public IMethod WithGenericArguments( params IType[] genericArguments ) => throw new NotSupportedException();

        public IMethodInvoker? BaseInvoker => throw new NotImplementedException();

        private sealed class ReturnParam : IParameter
        {
            public PseudoAccessor DeclaringAccessor { get; }

            public IMemberOrNamedType DeclaringMember => this.DeclaringAccessor;

            public RefKind RefKind
                => this.DeclaringAccessor.ContainingDeclaration switch
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

            public DeclarationOrigin Origin => DeclarationOrigin.Source;

            public IDeclaration? ContainingDeclaration => this.DeclaringAccessor;

            public IAttributeList Attributes => throw new NotImplementedException();

            public DeclarationKind DeclarationKind => DeclarationKind.Parameter;

            public bool HasAspect<T>()
                where T : IAspect
                => throw new NotImplementedException();

            [Obsolete( "Not implemented." )]
            public IAnnotationList GetAnnotations<T>()
                where T : IAspect
                => throw new NotImplementedException();

            public IDiagnosticLocation? DiagnosticLocation => throw new NotImplementedException();

            public ICompilation Compilation => throw new NotImplementedException();

            public ReturnParam( PseudoAccessor declaringAccessor )
            {
                this.DeclaringAccessor = declaringAccessor;
            }

            public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

            [return: RunTimeOnly]
            public ParameterInfo ToParameterInfo()
            {
                throw new NotImplementedException();
            }
        }
    }
}