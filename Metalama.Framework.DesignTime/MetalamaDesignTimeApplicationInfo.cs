// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Compiler;

namespace Metalama.Framework.DesignTime
{
    internal sealed class MetalamaDesignTimeApplicationInfo : ApplicationInfoBase
    {
        public override string Name => "Metalama.DesignTime";

        public override bool IsLongRunningProcess => !MetalamaCompilerInfo.IsActive;

        public MetalamaDesignTimeApplicationInfo() : base( typeof(MetalamaDesignTimeApplicationInfo).Assembly ) { }
    }
}