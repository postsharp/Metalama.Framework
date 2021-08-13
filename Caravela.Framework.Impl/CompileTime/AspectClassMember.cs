// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    // TODO: a class member should not store an ISymbol because we should not store references to a Roslyn compilation.
    
    internal record AspectClassMember ( string Name, AspectClass AspectClass, TemplateInfo TemplateInfo, bool IsAsync, ISymbol Symbol );
}