// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Utilities;

public static class RandomIdGenerator
{
    private static readonly Random _random = new();

    public static string GenerateId()
    {
        lock ( _random )
        {
            return $"{_random.Next():x}{_random.Next():x}";
        }
    }
}