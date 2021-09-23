// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Accessibility = Caravela.Framework.Code.Accessibility;
using MethodKind = Caravela.Framework.Code.MethodKind;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.CodeModel.Pseudo
{
    internal abstract class PseudoAccessor<T> : IMethodImpl
        where T : IMember
    {
        protected T DeclaringMember { get; }

        protected PseudoAccessor( T containingMember, MethodKind semantic )
        {
            this.DeclaringMember = containingMember;
            this.MethodKind = semantic;
        }

        [Memo]
        public IParameter ReturnParameter => new PseudoParameter( this, -1, this.ReturnType, null );

        public IType ReturnType
            => this.MethodKind != MethodKind.PropertyGet
                ? this.DeclaringMember.Compilation.TypeFactory.GetSpecialType( SpecialType.Void )
                : ((IFieldOrProperty) this.DeclaringMember).Type;

        [Memo]
        public IGenericParameterList TypeParameters => GenericParameterList.Empty;

        [Memo]
        public IReadOnlyList<IType> TypeArguments => ImmutableArray<IType>.Empty;

        public bool IsOpenGeneric => this.DeclaringMember.DeclaringType.IsOpenGeneric;

        public bool IsGeneric => false;

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>( ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ) );

        public IMethod? OverriddenMethod => null;

        public IMethodList LocalFunctions => MethodList.Empty;

        public abstract IParameterList Parameters { get; }

        public MethodKind MethodKind { get; }

        public Accessibility Accessibility => this.DeclaringMember.Accessibility;

        public abstract string Name { get; }

        public bool IsAbstract => false;

        public bool IsStatic => this.DeclaringMember.IsStatic;

        public bool IsVirtual => false;

        public bool IsSealed => false;

        public bool IsReadOnly => false;

        public bool IsOverride => false;

        public bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public bool IsNew => this.DeclaringMember.IsNew;

        public bool IsAsync => false;

        public INamedType DeclaringType => this.DeclaringMember.DeclaringType;

        public DeclarationOrigin Origin => DeclarationOrigin.Source;

        public IDeclaration? ContainingDeclaration => this.DeclaringMember;

        public IAttributeList Attributes => AttributeList.Empty;

        public DeclarationKind DeclarationKind => DeclarationKind.Method;

        public IDiagnosticLocation? DiagnosticLocation => this.DeclaringMember.DiagnosticLocation;

        public ICompilation Compilation => this.DeclaringMember.Compilation;

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => Array.Empty<IMethod>();

        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        public System.Reflection.MethodBase ToMethodBase() => throw new NotImplementedException();

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

        IMemberWithAccessors? IMethod.DeclaringMember => (IMemberWithAccessors) this.DeclaringMember;

        public ISymbol? Symbol => null;

        public DeclarationRef<IDeclaration> ToRef() => throw new NotImplementedException();

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public IGeneric ConstructGenericInstance( params IType[] typeArguments ) => throw new NotImplementedException();

        public IDeclaration OriginalDefinition => throw new NotImplementedException();
    }
}