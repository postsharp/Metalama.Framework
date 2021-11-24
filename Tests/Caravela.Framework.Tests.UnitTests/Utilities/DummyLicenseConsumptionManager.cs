// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing;
using PostSharp.Backstage.Licensing.Consumption;
using System;

namespace Caravela.Framework.Tests.UnitTests.Utilities
{
    internal class DummyLicenseConsumptionManager : ILicenseConsumptionManager
    {
        public bool CanConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures ) => true;

        public void ConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures )
        {
            if ( !this.CanConsumeFeatures( consumer, requiredFeatures ) )
            {
                throw new InvalidOperationException( "This manager is not supposed to refuse consumption." );
            }
        }
    }
}