using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface ICompilationBoundRefImpl : IRefImpl
{
    CompilationContext CompilationContext { get; }

    ResolvedAttributeRef GetAttributeData();

    bool IsDefinition { get; }

    IRef Definition { get; }
    
    IRefCollectionStrategy CollectionStrategy { get; }
}