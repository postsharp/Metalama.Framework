using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal interface IMemberOrNamedTypeBuilderImpl : IMemberOrNamedTypeBuilder, INamedDeclarationBuilderImpl
{
    bool? HasNewKeyword { get; }
}