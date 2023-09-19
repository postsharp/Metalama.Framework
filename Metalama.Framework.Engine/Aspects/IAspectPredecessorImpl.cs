// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Aspects
{
    internal interface IAspectPredecessorImpl : IAspectPredecessor
    {
        FormattableString FormatPredecessor( ICompilation compilation );

        Location? GetDiagnosticLocation( Compilation compilation );

        int TargetDeclarationDepth { get; }
    }
}