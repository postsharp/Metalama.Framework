// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Syntax
{
    /// <summary>
    /// Base interface for compile-time objects that can be converted to run-time syntax.
    /// </summary>
    [CompileTime]
    public interface ISyntaxBuilder
    {
        ISyntax ToSyntax();
    }
}