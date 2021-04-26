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
                    "Cannot instantiate a custom attribute: invalid type.",
                    "Cannot instantiate a custom attribute: got a value of type '{0}', but a value of type '{1}' was expected.",
                    _category,
                    DiagnosticSeverity.Error );
    }
}