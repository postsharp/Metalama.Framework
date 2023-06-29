// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal interface INamedTypeCollectionImpl
{
    IEnumerable<MemberRef<INamedType>> OfTypeDefinition( INamedType typeDefinition );
}
