// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

#pragma warning disable SA1118 // Allow multi-line parameters.

namespace Caravela.Framework.Impl.Serialization
{
    internal static class SerializationDiagnosticDescriptors
    {
        // Reserved range 200-299

        private const string _category = "Caravela.Serialization";

        public static readonly StrongDiagnosticDescriptor<object> UnsupportedSerialization = new(
            "CR0200",
            "Compile-time type not serializable.",
            "Cannot serialize the compile-time value of type '{0}' to a run-time value because this type is not serializable.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<object> CycleInSerialization = new(
            "CR0201",
            "A collection contains itself.",
            "Cannot serialize the compile-time value of type '{0}' to a run-time value because it contains a cyclic reference.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<object> MultidimensionalArray = new(
            "CR0202",
            "Multidimensional arrays not supported.",
            "Cannot serialize the compile-time array of type '{0}' because it has more than one dimension.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<object> UnsupportedDictionaryComparer = new(
            "CR0203", "Custom equality comparers not supported.",
            "Cannot serialize the compile-time dictionary into a run-time value because it has an unsupported equality comparer '{0}'. "
            + " Only the default comparer and predefined string comparers are supported.",
            _category,
            Error );
    }
}