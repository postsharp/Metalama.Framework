// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal enum TemplateMemberKind
    {
        /// <summary>
        /// Not a template member.
        /// </summary>
        None,
        
        Template,
        
        Introduction
        
    }
    
    /// <summary>
    /// Determines the kind of symbol: template, <see cref="TemplatingScope.CompileTimeOnly"/>,
    /// <see cref="TemplatingScope.RunTimeOnly"/>.
    /// </summary>
    internal interface ISymbolClassifier
    {
        TemplateMemberKind GetTemplateMemberKind( ISymbol symbol );

        TemplatingScope GetTemplatingScope( ISymbol symbol );
    }
}