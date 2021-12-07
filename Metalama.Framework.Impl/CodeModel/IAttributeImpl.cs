// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.Aspects;

namespace Metalama.Framework.Impl.CodeModel
{
    // IAttributeImpl does not implement IDeclarationImpl because it is not backed by an ISymbol.
    internal interface IAttributeImpl : IAttribute, IDiagnosticLocationImpl, IAspectPredecessorImpl { }
}