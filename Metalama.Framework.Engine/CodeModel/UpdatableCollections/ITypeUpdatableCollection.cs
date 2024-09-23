// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal interface ITypeUpdatableCollection : ISourceDeclarationCollection<INamedType>
{
    IEnumerable<IRef<INamedType>> OfTypeDefinition( INamedType typeDefinition );
}