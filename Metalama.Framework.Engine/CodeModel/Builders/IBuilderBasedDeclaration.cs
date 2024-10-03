using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Abstractions;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IBuilderBasedDeclaration : IDeclarationImpl
{
    IDeclarationBuilder Builder { get; }
}