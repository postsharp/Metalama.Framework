using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed record ResolvedAttributeRef( ImmutableArray<AttributeData> Attributes, ISymbol ParentSymbol, RefTargetKind ParentRefTargetKind )
{
    public static ResolvedAttributeRef Invalid { get; } = new( default, null!, default );
}