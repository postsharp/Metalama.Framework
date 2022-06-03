// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.LinqPad.Tests.Assets;

internal class Impl : IDerived
{
    int IBase.Property => throw new NotImplementedException();

    string IDerived.Property => throw new NotImplementedException();
}