using System;

namespace Caravela.Framework.Impl.Templating
{
    class AssertionFailedException : Exception
    {
        public AssertionFailedException()
        {
        }

        public AssertionFailedException( string message )
            : base( message )
        {
        }
    }
}