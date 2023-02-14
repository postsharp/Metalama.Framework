// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using static Metalama.Framework.Diagnostics.Severity;

namespace Metalama.Framework.Engine.CompileTime
{
    internal static class AttributeDeserializerDiagnostics
    {
        // Reserved range 400-499

        private const string _category = "Metalama.AttributeDeserializer";

        internal static readonly DiagnosticDefinition<ITypeSymbol>
            CannotFindAttributeType
                = new(
                    "LAMA0401",
                    _category,
                    "Cannot instantiate a custom attribute: cannot find the build-time type '{0}'. Make sure that the type exists and is annotated with [CompileTime] or [RunTimeOrCompileTime].",
                    Error,
                    "Cannot instantiate a custom attribute: cannot find type." );

        internal static readonly DiagnosticDefinition<string>
            PropertyHasNoSetter
                = new(
                    "LAMA0405",
                    _category,
                    "Cannot instantiate a custom attribute: the property '{0}' has no setter.",
                    Error,
                    "Cannot instantiate a custom attribute: a property has no setter." );

        internal static readonly DiagnosticDefinition<string>
            CannotSetIntroducedField
                = new(
                    "LAMA0406",
                    _category,
                    "Cannot set an [Introduce] field {0} from an attribute.",
                    Error,
                    "Cannot set an [Introduce] field from an attribute." );

        internal static readonly DiagnosticDefinition<string>
            CannotSetTemplateProperty
                = new(
                    "LAMA0407",
                    _category,
                    "Cannot set a template property {0} from an attribute.",
                    Error,
                    "Cannot set a template property from an attribute." );
    }
}