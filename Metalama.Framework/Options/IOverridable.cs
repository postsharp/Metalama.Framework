// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

[CompileTime]
public interface IOverridable
{
    object OverrideWith( object options, in HierarchicalOptionsOverrideContext context );
}