// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using PostSharp.Backstage.Licensing;
using PostSharp.Backstage.Licensing.Consumption;
using System;
using System.Collections.Generic;

namespace Metalama.TestFramework.Licensing
{
    /// <summary>
    /// License consumption manager allowing consumption of all license features.
    /// </summary>
    internal class DummyLicenseConsumptionManager : ILicenseConsumptionManager, IService
    {
        public bool CanConsumeFeatures( LicensedFeatures requiredFeatures, string? consumerNamespace = null ) => true;

        public IReadOnlyList<LicensingMessage> Messages => Array.Empty<LicensingMessage>();
    }
}