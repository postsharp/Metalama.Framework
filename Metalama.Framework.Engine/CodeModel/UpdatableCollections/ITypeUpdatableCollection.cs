using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal interface ITypeUpdatableCollection : ISourceDeclarationCollection<INamedType, IRef<INamedType>>
{
    IEnumerable<IRef<INamedType>> OfTypeDefinition( INamedType typeDefinition );
}