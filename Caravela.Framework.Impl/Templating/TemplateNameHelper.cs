// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Templating
{
    internal static class TemplateNameHelper
    {
        public static string GetCompiledTemplateName( IMethodSymbol originalSymbol )
            => originalSymbol switch
            {
                { ContainingSymbol: INamedTypeSymbol _ } => GetCompiledTemplateName( originalSymbol.Name ),
                { ContainingSymbol: IPropertySymbol property, MethodKind: MethodKind.PropertyGet } => GetCompiledTemplateName( $"__get_{property.Name}" ),
                { ContainingSymbol: IPropertySymbol property, MethodKind: MethodKind.PropertySet } => GetCompiledTemplateName( $"__set_{property.Name}" ),
                _ => throw new NotSupportedException()
            };

        public static string GetCompiledTemplateName( string templateMethodName ) => templateMethodName + TemplateCompiler.TemplateMethodSuffix;
    }
}