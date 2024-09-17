using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal sealed class SubstitutedParameter : IParameterImpl, ISubstitutedDeclaration
{
    private readonly SubstitutedMember _declaringMember;
    private readonly IParameterImpl _definition;

    public SubstitutedParameter( SubstitutedMember declaringMember, IParameter definition )
    {
        this._declaringMember = declaringMember;
        this._definition = (IParameterImpl) definition;
    }

    public CompilationModel Compilation => this._declaringMember.Compilation;

    public IDeclaration Definition => this._definition;

    ICompilation ICompilationElement.Compilation => this.Compilation;

    ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences => this._definition.GetDeclaringSyntaxReferences();

    public bool CanBeInherited => this._definition.CanBeInherited;

    SyntaxTree? IDeclarationImpl.PrimarySyntaxTree => this._definition.GetPrimarySyntaxTree();

    public IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
        => this._definition.GetDerivedDeclarations( options )
            .Select( d => this.Compilation.Factory.GetSubstitutedDeclaration(  d, this._declaringMember ) );

    public Ref<IDeclaration> ToValueTypedRef() => Ref.FromDeclarationId<IDeclaration>( this.GetSerializableId() );

    IRef<IDeclaration> IDeclaration.ToRef() => this.ToValueTypedRef();

    Ref<ICompilationElement> ICompilationElementImpl.ToValueTypedRef() => this.ToValueTypedRef().As<ICompilationElement>();

    [Memo]
    private BoxedRef<IParameter> BoxedRef => new( this.ToValueTypedRef() );

    IRef<IParameter> IParameter.ToRef() => this.BoxedRef;

    public SerializableDeclarationId ToSerializableId() => this.GetSerializableId();

    public IAssembly DeclaringAssembly => this._declaringMember.DeclaringAssembly;

    public IDeclarationOrigin Origin => this._definition.Origin;

    public IDeclaration ContainingDeclaration => this._declaringMember;

    public DeclarationKind DeclarationKind => DeclarationKind.Parameter;

    public bool IsImplicitlyDeclared => this._definition.IsImplicitlyDeclared;

    public int Depth => this._definition.Depth;

    public bool BelongsToCurrentProject => this._declaringMember.BelongsToCurrentProject;

    public ImmutableArray<SourceReference> Sources => this._definition.Sources;

    public string Name => this._definition.Name;

    public IType Type => this._declaringMember.Substitute( this._definition.Type );

    public RefKind RefKind => this._definition.RefKind;

    bool IExpression.IsAssignable => true;

    public ref object? Value => ref this._definition.Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
    {
        var sourceExpression = (TypedExpressionSyntaxImpl) this._definition.ToTypedExpressionSyntax( syntaxGenerationContext ).Implementation;

        return new TypedExpressionSyntax(
            new TypedExpressionSyntaxImpl(
                sourceExpression.Syntax,
                this.Substitute( sourceExpression.ExpressionType ),
                ((SyntaxSerializationContext) syntaxGenerationContext).CompilationModel,
                sourceExpression.IsReferenceable,
                sourceExpression.CanBeNull ) );
    }

    public int Index => this._definition.Index;

    [Memo]
    public TypedConstant? DefaultValue
        => this._definition.DefaultValue is { } sourceValue
            ? TypedConstant.CreateUnchecked( sourceValue.Value, this.Substitute( sourceValue.Type ) )
            : null;

    public bool IsParams => this._definition.IsParams;

    public IHasParameters DeclaringMember => (IHasParameters) this._declaringMember;

    public ParameterInfo ToParameterInfo() => throw new NotImplementedException();

    public bool IsReturnParameter => this._definition.IsReturnParameter;

    public ISymbol? Symbol => null;

    T IMeasurableInternal.GetMetric<T>() => this._definition.GetMetric<T>();

    public Location? DiagnosticLocation => this._definition.GetDiagnosticLocation();

    [Memo]
    public IAttributeCollection Attributes => SubstitutedMember.CreateSubstitutedAttributeCollection( this, this._definition.Attributes );

    public bool Equals( IDeclaration? other ) => ReferenceEquals( this, other );

    public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();
}