using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IBuilderBasedDeclaration : IDeclarationImpl
{
    IDeclarationBuilder Builder { get; }
}