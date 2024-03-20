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
    protected BuiltMember SourceMember { get; }

    public INamedTypeSymbol SubstitutedType { get; }

    public GenericMap GenericMap { get; }

    protected SubstitutedMember( BuiltMember sourceMember, INamedTypeSymbol substitutedType )
    {
        this.SourceMember = sourceMember;
        this.SubstitutedType = substitutedType;
        this.GenericMap = new( substitutedType.TypeArguments, sourceMember.Compilation.RoslynCompilation );
    }
    
    protected IType Substitute( IType sourceType ) => this.SourceMember.Compilation.Factory.GetIType( this.GenericMap.Map( sourceType.GetSymbol() ) );

    public ICompilation Compilation => this.SourceMember.Compilation;

    IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.SourceMember.DeclaringSyntaxReferences;

    public bool CanBeInherited => this.SourceMember.CanBeInherited;

    public SyntaxTree? PrimarySyntaxTree => this.SourceMember.PrimarySyntaxTree;

    public IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => throw new NotImplementedException();

    public Ref<IDeclaration> ToRef() => Ref.FromSubstitutedDeclaration( this );

    // TODO: test
    public SerializableDeclarationId ToSerializableId() => this.GetSerializableId();

    public IAssembly DeclaringAssembly => this.SourceMember.DeclaringAssembly;

    public IDeclarationOrigin Origin => this.SourceMember.Origin;

    public DeclarationKind DeclarationKind => this.SourceMember.DeclarationKind;

    public bool IsImplicitlyDeclared => this.SourceMember.IsImplicitlyDeclared;

    public int Depth => this.SourceMember.Depth;

    public bool BelongsToCurrentProject => this.SourceMember.BelongsToCurrentProject;

    public ImmutableArray<SourceReference> Sources => this.SourceMember.Sources;

    public string Name => this.SourceMember.Name;

    public Accessibility Accessibility => this.SourceMember.Accessibility;

    public bool IsAbstract => this.SourceMember.IsAbstract;

    public bool IsStatic => this.SourceMember.IsStatic;

    public bool IsSealed => this.SourceMember.IsSealed;

    public bool IsNew => this.SourceMember.IsNew;

    public bool IsVirtual => this.SourceMember.IsVirtual;

    public bool IsAsync => this.SourceMember.IsAsync;

    public bool IsOverride => this.SourceMember.IsOverride;

    public bool IsExplicitInterfaceImplementation => this.SourceMember.IsExplicitInterfaceImplementation;

    public bool HasImplementation => this.SourceMember.HasImplementation;

    public IMember Definition => this.SourceMember;

    IMemberOrNamedType IMemberOrNamedType.Definition => this.Definition;

    public Location? DiagnosticLocation => this.SourceMember.DiagnosticLocation;

    public bool? HasNewKeyword => this.SourceMember.HasNewKeyword;

    [Memo]
    public INamedType DeclaringType => this.SourceMember.Compilation.Factory.GetNamedType( this.SubstitutedType );

    public IDeclaration ContainingDeclaration => this.DeclaringType;

    public MemberInfo ToMemberInfo() => throw new NotImplementedException();

    public ExecutionScope ExecutionScope => ((IMemberOrNamedType) this.SourceMember).ExecutionScope;

    ISymbol? ISdkDeclaration.Symbol => null;

    public T GetMetric<T>()
        where T : IMetric
        => this.GetCompilationModel().MetricManager.GetMetric<T>( this );

    // TODO: test thoroughly
    public IMember? OverriddenMember
    {
        get
        {
            var sourceOverriddenMember = this.SourceMember.OverriddenMember;

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
                return SubstitutedMemberFactory.Substitute( sourceOverriddenMember, baseType.GetSymbol() )
                    .GetTarget( ReferenceResolutionOptions.Default );
            }
        }
    }

    [Memo]
    public IAttributeCollection Attributes => CreateSubstitutedAttributeCollection( this, this.SourceMember.Attributes );

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