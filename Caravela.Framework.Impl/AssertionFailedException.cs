using System;

namespace Caravela.Framework.Impl
{
    class AssertionFailedException : Exception
    {
        public AssertionFailedException()
        {

        }

        public AssertionFailedException(string message) : base(message)
        {

        }
    }
}