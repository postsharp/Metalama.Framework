using System;

namespace Metalama.Framework.Workspaces;

public class DotNetSdkLoadException : Exception
{
    public DotNetSdkLoadException( string message ) : base( message ) { }

    public DotNetSdkLoadException( string message, Exception inner ) : base( message, inner ) { }
}