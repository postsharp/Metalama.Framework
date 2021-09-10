// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using static Caravela.Framework.Diagnostics.Severity;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class AttributeDeserializerDiagnostics
    {
        // Reserved range 400-499

        private const string _category = "Caravela.AttributeDeserializer";

        internal static readonly DiagnosticDefinition<(Type ActualType, Type ExpectedType)>
            CannotReferenceCompileTimeOnly
                = new(
                    "CR0400",
                    _category,
                    "Cannot instantiate a custom attribute: got a value of type '{0}', but a value of type '{1}' was expected.",
                    Error,
                    "Cannot instantiate a custom attribute: invalid type conversion." );

        internal static readonly DiagnosticDefinition<ITypeSymbol>
            CannotFindAttributeType
                = new(
                    "CR0401",
                    _category,
                    "Cannot instantiate a custom attribute: cannot find the build-time type '{0}'. Make sure that the type exists and is annotated with [BuildTime].",
                    Error,
                    "Cannot instantiate a custom attribute: cannot find type." );

        internal static readonly DiagnosticDefinition<ITypeSymbol>
            NoConstructor
                = new(
                    "CR0402",
                    _category,
                    "Cannot instantiate a custom attribute: cannot find a suitable constructor in type '{0}'.",
                    Error,
                    "Cannot instantiate a custom attribute: no compatible constructor was found." );

        internal static readonly DiagnosticDefinition<ITypeSymbol>
            AmbiguousConstructor
                = new(
                    "CR0403",
                    _category,
                    "Cannot instantiate a custom attribute: several suitable constructors were found in type '{0}' and the ambiguity cannot be resolved",
                    Error,
                    "Cannot instantiate a custom attribute: ambiguous constructor match." );

        internal static readonly DiagnosticDefinition<(ITypeSymbol Type, string MemberName)>
            CannotFindMember
                = new(
                    "CR0404",
                    _category,
                    "Cannot instantiate a custom attribute: cannot find a public field or property named '{1}' in type '{0}'.",
                    Error,
                    "Cannot instantiate a custom attribute: cannot find a field or property." );
    }
}