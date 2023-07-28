// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Code;

internal interface ISyntaxReferenceImpl
{
    IDiagnosticLocation GetDiagnosticLocation( in SourceReference sourceReference );

    string GetKind( in SourceReference sourceReference );

    SourceSpan GetSourceSpan( SourceReference sourceReference );

    string GetText( SourceSpan sourceSpan );

    string GetText( SourceReference sourceReference, bool normalized );

    bool IsImplementationPart( SourceReference sourceReference );
}