// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionDeclarationReference
{
    IDeclaration DestinationDeclaration { get; }

    IDeclaration OriginDeclaration { get; }

    ReferenceKinds Kinds { get; }

    IReadOnlyList<IntrospectionReferenceDetail> Details { get; }
}