using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class NamespaceBuilderData : NamedDeclarationBuilderData
{
    private readonly IRef<INamespace> _ref;

    public NamespaceBuilderData( NamespaceBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new DeclarationBuilderDataRef<INamespace>( this);
    }

    protected override IRef<IDeclaration> ToDeclarationRef() => this._ref;
    
    public new IRef<INamespace> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;
}