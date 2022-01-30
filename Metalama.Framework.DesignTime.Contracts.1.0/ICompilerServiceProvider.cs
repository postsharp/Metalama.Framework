// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.InteropServices;

namespace Metalama.Framework.DesignTime.Contracts
{
    public interface ICompilerServiceProvider
    {
        Version Version { get; }

        // Cannot have a generic method in a type-equivalent interface.
        ICompilerService? GetCompilerService( Type type );

        event Action Unloaded;
    }
}