// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SpecialType = Metalama.Framework.Code.SpecialType;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal abstract class PseudoAccessor<T> : IMethodImpl
        where T : IMemberWithAccessorsImpl
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
                ? this.DeclaringMember.Compilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void )
                : ((IFieldOrProperty) this.DeclaringMember).Type;

        [Memo]
        public IGenericParameterList TypeParameters => TypeParameterList.Empty;

        [Memo]
        public IReadOnlyList<IType> TypeArguments => ImmutableArray<IType>.Empty;

        bool IDeclaration.IsImplicitlyDeclared => true;

        public bool IsOpenGeneric => this.DeclaringMember.DeclaringType.IsOpenGeneric;

        public bool IsGeneric => false;

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>( ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ) );

        public IMethod? OverriddenMethod => null;

        public abstract IParameterList Parameters { get; }

        public MethodKind MethodKind { get; }

        public abstract Accessibility Accessibility { get; }

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

        IRef<IDeclaration> IDeclaration.ToRef() => throw new NotImplementedException();

        public IAssembly DeclaringAssembly => this.DeclaringMember.DeclaringAssembly;

        public DeclarationOrigin Origin => DeclarationOrigin.Source;

        public IDeclaration? ContainingDeclaration => this.DeclaringMember;

        public IAttributeCollection Attributes => AttributeCollection.Empty;

        public DeclarationKind DeclarationKind => DeclarationKind.Method;

        public OperatorKind OperatorKind => OperatorKind.None;

        public ICompilation Compilation => this.DeclaringMember.Compilation;

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => Array.Empty<IMethod>();

        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        public System.Reflection.MethodBase ToMethodBase() => throw new NotImplementedException();

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

        IMemberWithAccessors? IMethod.DeclaringMember => this.DeclaringMember;

        public ISymbol? Symbol => null;

        public Ref<IDeclaration> ToRef() => Ref.PseudoAccessor( this );

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        bool IDeclarationImpl.CanBeInherited => false;

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public IGeneric ConstructGenericInstance( params IType[] typeArguments ) => throw new NotImplementedException();

        public IDeclaration OriginalDefinition => throw new NotImplementedException();

        public IMember? OverriddenMember => ((IMemberWithAccessors?) this.DeclaringMember.OverriddenMember)?.GetAccessor( this.MethodKind );

        public Location? DiagnosticLocation => this.DeclaringMember.GetDiagnosticLocation();

        public SyntaxTree? PrimarySyntaxTree => this.DeclaringMember.PrimarySyntaxTree;

        public TExtension GetMetric<TExtension>()
            where TExtension : IMetric
            => this.GetCompilationModel().MetricManager.GetMetric<TExtension>( this );
    }
}