// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Runtime.InteropServices;

namespace Metalama.Framework.DesignTime.Contracts;

[ComImport]
[Guid( "6d4389b1-9d7f-4534-a52e-6be2d76ef60e" )]
public interface ICompileTimeEditingStatusServiceCallback
{
    void OnIsEditingChanged( bool value );
}