// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal abstract class SubstitutedMember : IMemberImpl, ISubstitutedDeclaration
{
    private readonly BuiltMember _sourceMember;

    public INamedTypeSymbol SubstitutedType { get; }

    public GenericMap GenericMap { get; }

    protected SubstitutedMember( BuiltMember sourceMember, INamedTypeSymbol substitutedType )
    {
        this._sourceMember = sourceMember;
        this.SubstitutedType = substitutedType;
        this.GenericMap = new GenericMap( substitutedType.TypeArguments, sourceMember.Compilation.RoslynCompilation );
    }

    protected IType Substitute( IType sourceType )
        => this._sourceMember.Compilation.Factory.GetIType(
            this.GenericMap.Map( sourceType.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ) ) );

    public ICompilation Compilation => this._sourceMember.Compilation;

    IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this._sourceMember.DeclaringSyntaxReferences;

    public bool CanBeInherited => this._sourceMember.CanBeInherited;

    public SyntaxTree? PrimarySyntaxTree => this._sourceMember.PrimarySyntaxTree;

    public IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => throw new NotImplementedException();

    public Ref<IDeclaration> ToRef() => Ref.FromSubstitutedDeclaration( this );

    // TODO: test
    public SerializableDeclarationId ToSerializableId() => this.GetSerializableId();

    public IAssembly DeclaringAssembly => this._sourceMember.DeclaringAssembly;

    public IDeclarationOrigin Origin => this._sourceMember.Origin;

    public DeclarationKind DeclarationKind => this._sourceMember.DeclarationKind;

    public bool IsImplicitlyDeclared => this._sourceMember.IsImplicitlyDeclared;

    public int Depth => this._sourceMember.Depth;

    public bool BelongsToCurrentProject => this._sourceMember.BelongsToCurrentProject;

    public ImmutableArray<SourceReference> Sources => this._sourceMember.Sources;

    public string Name => this._sourceMember.Name;

    public Accessibility Accessibility => this._sourceMember.Accessibility;

    public bool IsAbstract => this._sourceMember.IsAbstract;

    public bool IsStatic => this._sourceMember.IsStatic;

    public bool IsSealed => this._sourceMember.IsSealed;

    public bool IsNew => this._sourceMember.IsNew;

    public bool IsVirtual => this._sourceMember.IsVirtual;

    public bool IsAsync => this._sourceMember.IsAsync;

    public bool IsOverride => this._sourceMember.IsOverride;

    public bool IsExplicitInterfaceImplementation => this._sourceMember.IsExplicitInterfaceImplementation;

    public bool HasImplementation => this._sourceMember.HasImplementation;

    public IMember Definition => this._sourceMember;

    IMemberOrNamedType IMemberOrNamedType.Definition => this.Definition;

    public Location? DiagnosticLocation => this._sourceMember.DiagnosticLocation;

    public bool? HasNewKeyword => this._sourceMember.HasNewKeyword;

    [Memo]
    public INamedType DeclaringType => this._sourceMember.Compilation.Factory.GetNamedType( this.SubstitutedType );

    public IDeclaration ContainingDeclaration => this.DeclaringType;

    public MemberInfo ToMemberInfo() => throw new NotImplementedException();

    public ExecutionScope ExecutionScope => ((IMemberOrNamedType) this._sourceMember).ExecutionScope;

    ISymbol? ISdkDeclaration.Symbol => null;

    public T GetMetric<T>()
        where T : IMetric
        => this.GetCompilationModel().MetricManager.GetMetric<T>( this );

    // TODO: test thoroughly
    public IMember? OverriddenMember
    {
        get
        {
            var sourceOverriddenMember = this._sourceMember.OverriddenMember;

            if ( sourceOverriddenMember == null )
            {
                return null;
            }

            var baseType = this.DeclaringType;

            do
            {
                baseType = baseType.BaseType;
            }
            while ( sourceOverriddenMember.DeclaringType != baseType.AssertNotNull().Definition );

            if ( baseType == sourceOverriddenMember.DeclaringType )
            {
                return sourceOverriddenMember;
            }
            else
            {
                return SubstitutedMemberFactory.Substitute(
                        sourceOverriddenMember,
                        baseType.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ) )
                    .GetTarget( ReferenceResolutionOptions.Default );
            }
        }
    }

    [Memo]
    public IAttributeCollection Attributes => CreateSubstitutedAttributeCollection( this, this._sourceMember.Attributes );

    // TODO: do this right?
    internal static AttributeCollection CreateSubstitutedAttributeCollection( ISubstitutedDeclaration targetDeclaration, IAttributeCollection sourceAttributes )
        => new(
            targetDeclaration,
            sourceAttributes.SelectAsArray(
                a => a is BuiltAttribute builtAttribute
                    ? new AttributeRef(
                        new AttributeBuilder(
                            builtAttribute.Builder.ParentAdvice,
                            targetDeclaration,
                            ((AttributeBuilder) builtAttribute.Builder).AttributeConstruction ) )
                    : throw new AssertionFailedException( $"Unexpected attribute type '{a.GetType()}'." ) ) );

    public bool Equals( IDeclaration? other ) => ReferenceEquals( this, other );

    public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
}