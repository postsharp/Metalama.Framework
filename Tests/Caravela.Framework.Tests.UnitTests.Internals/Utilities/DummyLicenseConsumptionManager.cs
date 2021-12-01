// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using PostSharp.Backstage.Licensing;
using PostSharp.Backstage.Licensing.Consumption;

namespace Caravela.Framework.Tests.UnitTests.Utilities
{
    /// <summary>
    /// License consumption manager allowing consumption of all license features.
    /// </summary>
    internal class DummyLicenseConsumptionManager : ILicenseConsumptionManager, IService
    {
        /// <inheritdoc />
        public bool CanConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures ) => true;

        /// <inheritdoc />
        public void ConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures ) { }
    }
}