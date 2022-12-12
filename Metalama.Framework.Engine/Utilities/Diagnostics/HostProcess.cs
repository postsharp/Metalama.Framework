// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
{
    public sealed class HostProcess
    {
        public static HostProcess Current { get; } = new();

        public HostProduct Product { get; }

        public bool IsInteractiveProcess { get; }

        private HostProcess()
        {
            switch ( ProcessUtilities.ProcessKind )
            {
                case ProcessKind.Compiler:
                    this.Product = HostProduct.Compiler;
                    this.IsInteractiveProcess = false;

                    break;

                case ProcessKind.Rider:
                    this.Product = HostProduct.Rider;
                    this.IsInteractiveProcess = false;

                    break;

                case ProcessKind.DevEnv:
                    this.Product = HostProduct.VisualStudio;
                    this.IsInteractiveProcess = true;

                    break;

                case ProcessKind.RoslynCodeAnalysisService:
                    this.Product = HostProduct.VisualStudio;
                    this.IsInteractiveProcess = false;

                    break;

                default:
                    this.Product = HostProduct.Other;
                    this.IsInteractiveProcess = false;

                    break;
            }
        }
    }
}