// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal interface IHierarchicalOptionsSource
{
    Task CollectOptionsAsync( CompilationModel compilation, AspectResultCollector collector, CancellationToken cancellationToken );
}