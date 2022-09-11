// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Options;

namespace Metalama.Framework.DesignTime.Preview
{
    internal class PreviewProjectOptions : ProjectOptionsWrapper
    {
        public PreviewProjectOptions( IProjectOptions underlying ) : base( underlying ) { }

        public override bool FormatOutput => true;

        public override bool FormatCompileTimeCode => false;
    }
}