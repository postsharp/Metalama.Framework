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
    protected SubstitutedMember( INamedType declaringType )
    {
        this.DeclaringType = (INamedTypeImpl) declaringType;
    }

    // ReSharper disable once InconsistentNaming
    protected abstract IMemberImpl GetDefinition();
    
    public IMemberImpl Definition => this.GetDefinition();
    
    IMember IMember.Definition => this.Definition;

    public CompilationModel Compilation => this.Definition.Compilation;

    internal IType Substitute( IType sourceType )
        => this.DeclaringType.GenericMap.Map( sourceType );

    ICompilation ICompilationElement.Compilation => this.Definition.Compilation;

    IRef<IDeclaration> IDeclaration.ToRef() => this.ToValueTypedRef();

    Ref<ICompilationElement> ICompilationElementImpl.ToValueTypedRef() => this.ToValueTypedRef().As<ICompilationElement>();

    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Definition.DeclaringSyntaxReferences;

    public bool CanBeInherited => this.Definition.CanBeInherited;

    public SyntaxTree? PrimarySyntaxTree => this.Definition.PrimarySyntaxTree;

    public IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => throw new NotImplementedException();

    public Ref<IDeclaration> ToValueTypedRef() => Ref.FromSubstitutedDeclaration( this );

    // TODO: test
    IRef<IMemberOrNamedType> IMemberOrNamedType.ToRef() => ((IMemberOrNamedType) this.Definition).ToRef();

    IRef<IMember> IMember.ToRef() => this.Definition.ToRef();

    public SerializableDeclarationId ToSerializableId() => this.GetSerializableId();

    public IAssembly DeclaringAssembly => this.Definition.DeclaringAssembly;

    public IDeclarationOrigin Origin => this.Definition.Origin;

    public DeclarationKind DeclarationKind => this.Definition.DeclarationKind;

    public bool IsImplicitlyDeclared => this.Definition.IsImplicitlyDeclared;

    public int Depth => this.Definition.Depth;

    public bool BelongsToCurrentProject => this.Definition.BelongsToCurrentProject;

    public ImmutableArray<SourceReference> Sources => this.Definition.Sources;

    public string Name => this.Definition.Name;

    public Accessibility Accessibility => this.Definition.Accessibility;

    public bool IsAbstract => this.Definition.IsAbstract;

    public bool IsStatic => this.Definition.IsStatic;

    public bool IsSealed => this.Definition.IsSealed;

    public bool IsNew => this.Definition.IsNew;

    public bool IsVirtual => this.Definition.IsVirtual;

    public bool IsAsync => this.Definition.IsAsync;

    public bool IsOverride => this.Definition.IsOverride;

    public bool IsExplicitInterfaceImplementation => this.Definition.IsExplicitInterfaceImplementation;

    public bool HasImplementation => this.Definition.HasImplementation;
    
    IDeclaration ISubstitutedDeclaration.Definition => this.Definition;

    IMemberOrNamedType IMemberOrNamedType.Definition => this.Definition;

    public Location? DiagnosticLocation => this.Definition.DiagnosticLocation;

    public bool? HasNewKeyword => this.Definition.HasNewKeyword;

    public INamedTypeImpl DeclaringType { get; }

    INamedType IMemberOrNamedType.DeclaringType => this.DeclaringType;
    
    INamedType IMember.DeclaringType => this.DeclaringType;
    
    public IDeclaration ContainingDeclaration => this.DeclaringType;

    public MemberInfo ToMemberInfo() => throw new NotImplementedException();

    public ExecutionScope ExecutionScope => this.Definition.ExecutionScope;

    ISymbol? ISdkDeclaration.Symbol => null;

    public T GetMetric<T>()
        where T : IMetric
        => this.GetCompilationModel().MetricManager.GetMetric<T>( this );

    // TODO: test thoroughly
    public IMember? OverriddenMember
    {
        get
        {
            var sourceOverriddenMember = this.Definition.OverriddenMember;

            if ( sourceOverriddenMember == null )
            {
                return null;
            }

            INamedType baseType = this.DeclaringType;

            var genericMap = GenericMap.Empty;
            
            do
            {
                baseType = baseType.BaseType.AssertNotNull();
                genericMap = ((INamedTypeImpl) baseType.BaseType).GenericMap.Apply( genericMap );
            }
            while ( sourceOverriddenMember.DeclaringType != baseType.AssertNotNull().Definition );

            if ( baseType == sourceOverriddenMember.DeclaringType )
            {
                return sourceOverriddenMember;
            }
            else
            {
                return this.Compilation.Factory.GetSubstitutedDeclaration( sourceOverriddenMember, this.DeclaringType, genericMap );
            }
        }
    }

    [Memo]
    public IAttributeCollection Attributes => CreateSubstitutedAttributeCollection( this, this.Definition.Attributes );

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