// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

[RunTimeOrCompileTime]
public interface IHierarchicalOptionsProvider { }

public interface IHierarchicalOptionsProvider<out T> : IHierarchicalOptionsProvider
    where T : class, IHierarchicalOptions
{
    T GetOptions();
}