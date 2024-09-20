// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Builders;

/// <summary>
/// The base class for the read-only facade of introduced declarations, represented by <see cref="DeclarationBuilder"/>. Facades
/// are consistent with the consuming <see cref="CompilationModel"/>, while builders are consistent with the producing <see cref="CompilationModel"/>. 
/// </summary>
internal abstract class BuiltDeclaration : BaseDeclaration, IBuilderBasedDeclaration
{
    protected BuiltDeclaration( CompilationModel compilation, IGenericContext genericContext )
    {
        this.Compilation = compilation;
        this.GenericContext = genericContext.AsGenericContext();
    }

    IDeclarationBuilder IBuilderBasedDeclaration.Builder => this.Builder;

    public override CompilationModel Compilation { get; }

    public abstract DeclarationBuilder Builder { get; }

    internal override GenericContext GenericContext { get; }

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        => this.Builder.ToDisplayString( format, context );

    [Memo]
    public override IAssembly DeclaringAssembly => this.Compilation.Factory.Translate( this.Builder.DeclaringAssembly ).AssertNotNull();

    public override IDeclarationOrigin Origin => this.Builder.Origin;

    [return: NotNullIfNotNull( nameof(type) )]
    protected T? MapType<T>( T? type )
        where T : class, IType
    {
        if ( type == null )
        {
            return null;
        }

        return (T) this.GenericContext.Map( this.Compilation.Factory.Translate( type, ReferenceResolutionOptions.CanBeMissing ) ).AssertNotNull();
    }

    [Memo]
    public override IDeclaration? ContainingDeclaration
        => this.Compilation.Factory.Translate( this.Builder.ContainingDeclaration, ReferenceResolutionOptions.CanBeMissing );

    public sealed override SyntaxTree? PrimarySyntaxTree => this.Builder.PrimarySyntaxTree;

    [Memo]
    public override IAttributeCollection Attributes
        => new AttributeCollection(
            this,
            this.Compilation.GetAttributeCollection( this.ToDeclarationRef() ) );

    public override DeclarationKind DeclarationKind => this.Builder.DeclarationKind;

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Builder.DeclaringSyntaxReferences;

    public override bool CanBeInherited => this.Builder.CanBeInherited;

    public override string ToString() => this.Builder.ToString();

    public override Location? DiagnosticLocation => this.Builder.DiagnosticLocation;

    public override bool IsImplicitlyDeclared => false;

    public override bool Equals( IDeclaration? other )
    {
        switch ( other )
        {
            case BuiltDeclaration builtDeclaration when this.Builder.Equals( builtDeclaration.Builder ):
            case DeclarationBuilder declarationBuilder when this.Builder.Equals( declarationBuilder ):
                return true;

            default:
                return false;
        }
    }

    protected override int GetHashCodeCore() => this.Builder.GetHashCode();

    public override bool BelongsToCurrentProject => true;

    public override ImmutableArray<SourceReference> Sources => ImmutableArray<SourceReference>.Empty;

    internal override ICompilationElement? Translate(
        CompilationModel newCompilation,
        ReferenceResolutionOptions options = ReferenceResolutionOptions.Default,
        IGenericContext? genericContext = null )
        => this.Builder.Translate( newCompilation, options, genericContext );
}