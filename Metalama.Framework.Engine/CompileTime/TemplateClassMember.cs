// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Aspects;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.CompileTime
{
    // TODO: a class member should not store an ISymbol because we should not store references to a Roslyn compilation.

    internal record TemplateClassMember( string Name, TemplateClass TemplateClass, TemplateInfo TemplateInfo, bool IsAsync, ISymbol Symbol );
}