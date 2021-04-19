// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Runtime.InteropServices;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Base interface for a implemented by the compiler part of the software (not the UI part) that
    /// can be returned synchronously.
    /// </summary>
    [Guid( "32aeeb0f-92e3-4952-91c0-1477f791b309" )]
    public interface ICompilerService { }
}