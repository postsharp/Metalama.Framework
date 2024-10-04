// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

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

    DeclarationBuilderData IBuilderBasedDeclaration.Builder => this.BuilderData;

    public override CompilationModel Compilation { get; }

    public abstract DeclarationBuilderData BuilderData { get; }

    internal override GenericContext GenericContext { get; }

    public sealed override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        => DisplayStringFormatter.Format( this );

    public override IAssembly DeclaringAssembly => this.Compilation;

    public override IDeclarationOrigin Origin => this.BuilderData.ParentAdvice;

    [return: NotNullIfNotNull( nameof(type) )]
    protected IType? MapType( IRef<IType>? type )
    {
        if ( type == null )
        {
            return null;
        }

        return type.GetTarget( this.Compilation, this.GenericContext );
    }

    [return: NotNullIfNotNull(nameof(declaration))]
    protected T? MapDeclaration<T>( IRef<T>? declaration )
        where T : class, ICompilationElement
        => declaration?.GetTarget(this.Compilation, this.GenericContext);

    protected IReadOnlyList<T> MapDeclarationList<T>( IReadOnlyList<IRef<T>> refs )
        where T : class, ICompilationElement
        => refs.Count == 0 ? [] : refs.SelectAsReadOnlyList( this.MapDeclaration );
    
            
    [Memo]
    public override IDeclaration? ContainingDeclaration => this.MapDeclaration( this.BuilderData.ContainingDeclaration );

    public sealed override SyntaxTree? PrimarySyntaxTree => this.ContainingDeclaration?.GetPrimarySyntaxTree();

    [Memo]
    public override IAttributeCollection Attributes => this.GetAttributes();

    private IAttributeCollection GetAttributes()
    {
        // In the Attributes collection, the backlink IAttribute.ContainingDeclaration will point to the member definition but this should be ok.
            
        var definition = this.GetDefinition();
            
        return new AttributeCollection(
            definition,
            this.Compilation.GetAttributeCollection( definition.ToRef() ) );
    }

    public override DeclarationKind DeclarationKind => this.BuilderData.DeclarationKind;

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;
    
    public sealed override string ToString() => this.ToDisplayString();

    public override Location? DiagnosticLocation => this.ContainingDeclaration?.GetDiagnosticLocation();

    

    public override bool IsImplicitlyDeclared => false;

    public override bool Equals( IDeclaration? other )
    {
        switch ( other )
        {
            case BuiltDeclaration builtDeclaration when this.BuilderData.Equals( builtDeclaration.BuilderData ):
            case DeclarationBuilder declarationBuilder when this.BuilderData.Equals( declarationBuilder ):
                return true;

            default:
                return false;
        }
    }

    protected override int GetHashCodeCore() => this.BuilderData.GetHashCode();

    public override bool BelongsToCurrentProject => true;

    public override ImmutableArray<SourceReference> Sources => ImmutableArray<SourceReference>.Empty;

    internal override ICompilationElement? Translate(
        CompilationModel newCompilation,
        IGenericContext? genericContext = null,
        Type? interfaceType = null )
        => this.BuilderData.Translate( newCompilation, genericContext );
}