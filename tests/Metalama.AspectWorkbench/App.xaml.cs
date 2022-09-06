﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Pipeline.CompileTime;

namespace Metalama.AspectWorkbench
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App
    {
        public App()
        {
            BackstageServiceFactory.Initialize( () => new MyApplicationInfo(), "AspectWorkbench" );
        }

        private class MyApplicationInfo : ApplicationInfoBase
        {
            public MyApplicationInfo() : base( typeof(CompileTimeAspectPipeline).Assembly ) { }

            public override string Name => "Metalama.AspectWorkbench";

            public override bool IsTelemetryEnabled => false;

            public override bool IsLongRunningProcess => true;

            public override bool ShouldCreateLocalCrashReports => false;
        }
    }
}