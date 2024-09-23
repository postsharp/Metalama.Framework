// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class TypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>, ITypeUpdatableCollection
{
    public TypeUpdatableCollection( CompilationModel compilation, IRef<INamespaceOrNamedType> declaringTypeOrNamespace ) : base(
        compilation,
        declaringTypeOrNamespace ) { }

    protected override IEqualityComparer<IRef<INamedType>> MemberRefComparer => this.Compilation.CompilationContext.NamedTypeRefComparer;

    public IEnumerable<IRef<INamedType>> OfTypeDefinition( INamedType typeDefinition )
    {
        var comparer = (DeclarationEqualityComparer) this.Compilation.Comparers.GetTypeComparer( TypeComparison.Default );

        // TODO: This should not use GetSymbol.
        return
            this.GetMemberRefs()
                .Where( t => comparer.Is( t, typeDefinition, ConversionKind.TypeDefinition ) );
    }

    protected override DeclarationKind DeclarationKind => DeclarationKind.NamedType;
}