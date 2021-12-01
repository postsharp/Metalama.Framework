// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using PostSharp.Backstage.Licensing;
using PostSharp.Backstage.Licensing.Consumption;

namespace Caravela.TestFramework.Licensing
{
    internal class TestFrameworkLicenseConsumptionManager : ILicenseConsumptionManager, IService
    {
        public bool CanConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures ) => true;

        public void ConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures ) { }
    }
}