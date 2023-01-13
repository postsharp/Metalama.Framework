// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Workspaces;

public class DotNetSdkLoadException : Exception
{
    
    public DotNetSdkLoadException( string message, Exception inner ) : base( message, inner ) { }
}