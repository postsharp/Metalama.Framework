// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Introspection;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection.References;

internal class OutboundDeclarationReference(
    ISymbol referencedSymbol,
    ISymbol referencingSymbol,
    IEnumerable<OutboundReference> references,
    CompilationModel compilation )
    : IDeclarationReference
{
    [Memo]
    public IDeclaration DestinationDeclaration => compilation.Factory.GetDeclaration( referencedSymbol );

    [Memo]
    public IDeclaration OriginDeclaration => compilation.Factory.GetDeclaration( referencingSymbol );

    [Memo]
    public ReferenceKinds Kinds => references.Select( r => r.ReferenceKind ).Union();

    [Memo]
    public IReadOnlyList<Reference> References
        => references.Select( r => new Reference( this, r.ReferenceKind, new SourceReference( r.Node.AsNode() ?? (object) r.Node.AsToken(), SourceReferenceImpl.Instance ) ) ).ToReadOnlyList();

    public override string ToString() => $"{this.OriginDeclaration} -> {this.DestinationDeclaration}";
}