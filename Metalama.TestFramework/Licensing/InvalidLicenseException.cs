// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.TestFramework.Licensing
{
    public sealed class InvalidLicenseException : Exception
    {
        public InvalidLicenseException( string message ) : base( message )
        {
        }
    }
}