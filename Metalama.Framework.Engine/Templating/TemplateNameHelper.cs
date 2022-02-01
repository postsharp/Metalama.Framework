// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Templating
{
    internal static class TemplateNameHelper
    {
        public static string GetCompiledTemplateName( ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol { MethodKind: MethodKind.PropertyGet } method => GetCompiledTemplateName( $"Get{method.AssociatedSymbol!.Name}" ),
                IMethodSymbol { MethodKind: MethodKind.PropertySet } method => GetCompiledTemplateName( $"Set{method.AssociatedSymbol!.Name}" ),
                IMethodSymbol method => GetCompiledTemplateName( method.Name ),
                IFieldSymbol field => GetCompiledTemplateName( field.Name ),
                IPropertySymbol property => GetCompiledTemplateName( property.Name ),
                IEventSymbol @event => GetCompiledTemplateName( @event.Name ),
                _ => throw new AssertionFailedException()
            };

        public static string GetCompiledTemplateName( string templateMemberName ) => "__" + templateMemberName;
    }
}