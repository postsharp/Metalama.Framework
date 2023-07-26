// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
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

namespace Metalama.Framework.Engine.CodeModel.Pseudo;

internal abstract class PseudoAccessor<T> : IMethodImpl, IPseudoDeclaration
    where T : IHasAccessorsImpl
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

    public int Depth => this.GetDepthImpl();

    public bool IsGeneric => false;

    public bool IsCanonicalGenericInstance => this.DeclaringType.IsCanonicalGenericInstance;

    public IMethod? OverriddenMethod => null;

    public abstract IParameterList Parameters { get; }

    public MethodKind MethodKind { get; }

    public abstract Accessibility Accessibility { get; }

    public abstract string Name { get; }

    public bool IsPartial => false;

    public bool HasImplementation => true;

    public bool IsAbstract => false;

    public bool IsStatic => this.DeclaringMember.IsStatic;

    public bool IsVirtual => false;

    public bool IsSealed => false;

    public bool IsReadOnly => false;

    public bool IsOverride => false;

    public bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    public bool IsNew => this.DeclaringMember.IsNew;

    public bool? HasNewKeyword => this.DeclaringMember.HasNewKeyword != null ? false : null;

    public bool IsAsync => false;

    public INamedType DeclaringType => this.DeclaringMember.DeclaringType;

    IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

    public SerializableDeclarationId ToSerializableId() => this.DeclaringMember.GetSerializableId( this.MethodKind.ToDeclarationRefTargetKind() );

    public IAssembly DeclaringAssembly => this.DeclaringMember.DeclaringAssembly;

    public IDeclarationOrigin Origin => this.DeclaringMember.Origin;

    public IDeclaration ContainingDeclaration => this.DeclaringMember;

    public IAttributeCollection Attributes => AttributeCollection.Empty;

    public DeclarationKind DeclarationKind => DeclarationKind.Method;

    public OperatorKind OperatorKind => OperatorKind.None;

    IMethod IMethod.MethodDefinition => this;

    bool IMethod.IsExtern => false;

    public IMethodInvoker With( InvokerOptions options ) => new MethodInvoker( this, options );

    public IMethodInvoker With( object? target, InvokerOptions options = default ) => new MethodInvoker( this, options, target );

    public object? Invoke( params object?[] args ) => new MethodInvoker( this ).Invoke( args );

    public ICompilation Compilation => this.DeclaringMember.Compilation;

    public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        => this.ContainingDeclaration.GetClosestNamedType().AssertNotNull().ToDisplayString( format, context ) + "." + this.Name;

    public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => Array.Empty<IMethod>();

    MemberInfo IMemberOrNamedType.ToMemberInfo()
        => throw new NotSupportedException( $"'{this}' is implicitly defined  declaration and cannot be represented as a System.Reflection object." );

    public ExecutionScope ExecutionScope => this.DeclaringMember.ExecutionScope;

    System.Reflection.MethodBase IMethodBase.ToMethodBase()
        => throw new NotSupportedException( $"'{this}' is implicitly defined  declaration and cannot be represented as a System.Reflection object." );

    MethodInfo IMethod.ToMethodInfo()
        => throw new NotSupportedException( $"'{this}' is implicitly defined  declaration and cannot be represented as a System.Reflection object." );

    IHasAccessors IMethod.DeclaringMember => this.DeclaringMember;

    public ISymbol? Symbol => null;

    public Ref<IDeclaration> ToRef() => Ref.PseudoAccessor( this );

    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

    bool IDeclarationImpl.CanBeInherited => false;

    IEnumerable<IDeclaration> IDeclarationImpl.GetDerivedDeclarations( DerivedTypesOptions options ) => throw new NotSupportedException();

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments ) => throw new NotSupportedException();

    public IDeclaration OriginalDefinition
        => this.MethodKind switch
        {
            MethodKind.PropertyGet => ((IFieldOrProperty) this.DeclaringMember.GetOriginalDefinition()).GetMethod.AssertNotNull(),
            MethodKind.PropertySet => ((IFieldOrProperty) this.DeclaringMember.GetOriginalDefinition()).SetMethod.AssertNotNull(),
            MethodKind.EventAdd => ((IEvent) this.DeclaringMember.GetOriginalDefinition()).AddMethod.AssertNotNull(),
            MethodKind.EventRemove => ((IEvent) this.DeclaringMember.GetOriginalDefinition()).RemoveMethod.AssertNotNull(),
            MethodKind.EventRaise => ((IEvent) this.DeclaringMember.GetOriginalDefinition()).RaiseMethod.AssertNotNull(),
            _ => throw new AssertionFailedException( $"Unexpected MethodKind: {this.MethodKind}." )
        };

    public IMember? OverriddenMember => ((IHasAccessors?) this.DeclaringMember.OverriddenMember)?.GetAccessor( this.MethodKind );

    public Location? DiagnosticLocation => this.DeclaringMember.GetDiagnosticLocation();

    public SyntaxTree? PrimarySyntaxTree => this.DeclaringMember.PrimarySyntaxTree;

    public TExtension GetMetric<TExtension>()
        where TExtension : IMetric
        => this.GetCompilationModel().MetricManager.GetMetric<TExtension>( this );

    public bool Equals( IDeclaration? other ) => ReferenceEquals( this, other );

    bool? IMethodImpl.IsIteratorMethod => false;

    public override string ToString() => this.ToDisplayString();

    public bool BelongsToCurrentProject => this.ContainingDeclaration.BelongsToCurrentProject;
}