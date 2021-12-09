// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Aspects
{
    internal interface IAspectPredecessorImpl : IAspectPredecessor
    {
        FormattableString FormatPredecessor( ICompilation compilation );

        Location? GetDiagnosticLocation( Compilation compilation );
    }
}