// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Introspection;

public interface IIntrospectionAspectPredecessorInternal : IIntrospectionAspectPredecessor
{
    void AddSuccessor( IAspectInstance aspectInstance );
}