// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Fabrics;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal interface IHierarchicalOptionsSource
{
    Task CollectOptionsAsync( OutboundActionCollectionContext context );
}