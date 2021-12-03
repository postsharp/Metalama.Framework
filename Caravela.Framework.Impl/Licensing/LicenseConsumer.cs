// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Licensing.Consumption;
using System;

namespace Caravela.Framework.Impl.Licensing
{
    internal class LicenseConsumer : ILicenseConsumer
    {
        public string? TargetTypeNamespace => null;

        public string? TargetTypeName => null;

        public IBackstageDiagnosticSink Diagnostics { get; }

        public IDiagnosticsLocation? DiagnosticsLocation => null;

        public LicenseConsumer( IServiceProvider services )
        {
            this.Diagnostics = services.GetRequiredService<IBackstageDiagnosticSink>();
        }
    }
}