// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Options;

[CompileTime]
internal interface IHierarchicalOptionsManager
{
    TOptions GetOptions<TOptions>( IDeclaration declaration )
        where TOptions : class, IHierarchicalOptions, new();
}