// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{

    /// <summary>
    /// Results returned by <see cref="DesignTimeAspectPipelineCache"/>.
    /// </summary>
    /// <param name="SyntaxTreeResults"></param>
    internal record DesignTimeResults ( ImmutableArray<DesignTimeSyntaxTreeResult> SyntaxTreeResults );
}