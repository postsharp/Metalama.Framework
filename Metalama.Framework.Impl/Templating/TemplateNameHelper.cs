// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.Templating
{
    internal static class TemplateNameHelper
    {
        public static string GetCompiledTemplateName( IMethodSymbol method )
            => method.MethodKind switch
            {
                MethodKind.PropertyGet => GetCompiledTemplateName( $"Get{method.AssociatedSymbol!.Name}" ),
                MethodKind.PropertySet => GetCompiledTemplateName( $"Set{method.AssociatedSymbol!.Name}" ),
                _ => GetCompiledTemplateName( method.Name )
            };

        public static string GetCompiledTemplateName( string templateMethodName ) => "__" + templateMethodName;
    }
}