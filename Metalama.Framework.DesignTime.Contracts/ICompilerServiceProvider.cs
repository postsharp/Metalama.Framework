// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.InteropServices;

namespace Metalama.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.
    [Guid( "cb5737d7-85df-4165-b7cc-12c35d0436ef" )]
    [ComImport]
    public interface ICompilerServiceProvider
    {
        Version Version { get; }

        // Cannot have a generic method in a type-equivalent interface.
        ICompilerService? GetCompilerService( Type type );

        event Action Unloaded;
    }
}