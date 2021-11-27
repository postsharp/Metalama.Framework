// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.LinqPad
{
    internal record ObjectFacadeProperty( string Name, Type Type, Func<object, object?> GetFunc, bool IsLazy );
}