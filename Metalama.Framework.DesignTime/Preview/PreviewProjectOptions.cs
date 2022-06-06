// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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