using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Aspects
{
    internal interface IAspectPredecessorImpl : IAspectPredecessor
    {
        FormattableString FormatPredecessor();

        Location? GetDiagnosticLocation( Compilation compilation );
    }
}