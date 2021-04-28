// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class AttributeDeserializerDiagnostics
    {
        // Reserved range 400-499

        private const string _category = "Caravela.AttributeDeserializer";

        internal static readonly StrongDiagnosticDescriptor<(Type ActualType, Type ExpectedType)>
            CannotReferenceCompileTimeOnly
                = new(
                    "CR0400",
                    "Cannot instantiate a custom attribute: invalid type conversion.",
                    "Cannot instantiate a custom attribute: got a value of type '{0}', but a value of type '{1}' was expected.",
                    _category,
                    DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<ITypeSymbol>
            CannotFindType
                = new(
                    "CR0401",
                    "Cannot instantiate a custom attribute: cannot find type.",
                    "Cannot instantiate a custom attribute: cannot find the CLR type '{0}'.",
                    _category,
                    DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<ITypeSymbol>
            NoConstructor
                = new(
                    "CR0402",
                    "Cannot instantiate a custom attribute: no compatible constructor was found.",
                    "Cannot instantiate a custom attribute: cannot find a suitable constructor in type '{0}'.",
                    _category,
                    DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<ITypeSymbol>
            AmbiguousConstructor
                = new(
                    "CR0403",
                    "Cannot instantiate a custom attribute: ambiguous constructor match.",
                    "Cannot instantiate a custom attribute: several suitable constructors were found in type '{0}' and the ambiguity cannot be resolved",
                    _category,
                    DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<(ITypeSymbol Type, string MemberName)>
            CannotFindMember
                = new(
                    "CR0404",
                    "Cannot instantiate a custom attribute: cannot find a field or property.",
                    "Cannot instantiate a custom attribute: cannot find a public field or property named '{1}' in type '{0}'.",
                    _category,
                    DiagnosticSeverity.Error );
    }
}