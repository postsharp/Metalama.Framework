using System;

namespace Metalama.Testing.AspectTesting;

public sealed class InvalidTestOptionException : Exception
{
    public InvalidTestOptionException( string message ) : base( message ) { }
}