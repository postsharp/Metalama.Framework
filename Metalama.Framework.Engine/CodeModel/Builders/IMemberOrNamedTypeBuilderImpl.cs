using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IMemberOrNamedTypeBuilderImpl : IMemberOrNamedTypeBuilder, INamedDeclarationBuilderImpl
{
    bool? HasNewKeyword { get; }
}