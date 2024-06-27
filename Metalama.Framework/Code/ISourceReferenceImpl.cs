// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code;

internal interface ISourceReferenceImpl
{
    string GetKind( in SourceReference sourceReference );

    SourceSpan GetSourceSpan( in SourceReference sourceReference );

    string GetText( in SourceSpan sourceSpan );

    string GetText( in SourceReference sourceReference, bool normalized );

    bool IsImplementationPart( in SourceReference sourceReference );
}