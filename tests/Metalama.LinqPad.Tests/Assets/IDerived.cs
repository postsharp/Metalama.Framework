// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.LinqPad.Tests.Assets;

public interface IDerived : IBase
{
    new string Property { get; }
}