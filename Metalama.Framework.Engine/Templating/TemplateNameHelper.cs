// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Templating
{
    internal static class TemplateNameHelper
    {
        public static string GetCompiledTemplateName( ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol { MethodKind: MethodKind.PropertyGet } method => GetCompiledTemplateName(
                    $"Get{method.AssociatedSymbol!.Name}",
                    method.Parameters ),
                IMethodSymbol { MethodKind: MethodKind.PropertySet } method => GetCompiledTemplateName(
                    $"Set{method.AssociatedSymbol!.Name}",
                    method.Parameters,
                    true ),
                IMethodSymbol { MethodKind: MethodKind.EventAdd } method => GetCompiledTemplateName(
                    $"Add{method.AssociatedSymbol!.Name}",
                    method.Parameters,
                    true ),
                IMethodSymbol { MethodKind: MethodKind.EventRemove } method => GetCompiledTemplateName(
                    $"Remove{method.AssociatedSymbol!.Name}",
                    method.Parameters,
                    true ),
                IMethodSymbol method => GetCompiledTemplateName( method.Name, method.Parameters ),
                IFieldSymbol field => GetCompiledTemplateName( field.Name ),
                IPropertySymbol property => GetCompiledTemplateName( property.Name, property.Parameters ),
                IEventSymbol @event => GetCompiledTemplateName( @event.Name ),
                _ => throw new AssertionFailedException()
            };

        private static string GetCompiledTemplateName(
            string templateMemberName,
            ImmutableArray<IParameterSymbol> parameters = default,
            bool ignoredLastParameter = false )
        {
            var principal = "__" + templateMemberName;

            if ( parameters.IsDefaultOrEmpty || (ignoredLastParameter && parameters.Length == 1) )
            {
                return principal;
            }

            var hashCode = new XXH64();
            var parameterCount = parameters.Length;

            if ( ignoredLastParameter )
            {
                parameterCount--;
            }

            for ( var i = 0; i < parameterCount; i++ )
            {
                var parameterType = parameters[i].Type.GetDocumentationCommentId();
                if ( parameterType != null )
                {
                    hashCode.Update( parameterType );
                }
                else
                {
                    // This happens for dynamics.
                    hashCode.Update( parameters[i].Type.ToString() );
                }
            }

            return $"{principal}_{hashCode.Digest():x}";
        }
    }
}