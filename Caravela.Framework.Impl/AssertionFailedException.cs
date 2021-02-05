using System;

namespace Caravela.Framework.Impl
{
    internal class AssertionFailedException : Exception
    {
        public AssertionFailedException()
        {
        }

        public AssertionFailedException( string message ) : base( message )
        {
        }
    }
}