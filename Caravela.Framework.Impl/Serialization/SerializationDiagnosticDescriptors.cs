// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using static Caravela.Framework.Diagnostics.Severity;

#pragma warning disable SA1118 // Allow multi-line parameters.

namespace Caravela.Framework.Impl.Serialization
{
    internal static class SerializationDiagnosticDescriptors
    {
        // Reserved range 200-299

        private const string _category = "Caravela.Serialization";

        public static readonly DiagnosticDefinition<object> UnsupportedSerialization = new(
            "CR0200",
            _category,
            "Cannot serialize the compile-time value of type '{0}' to a run-time value because this type is not serializable.",
            Error,
            "Compile-time type not serializable." );

        public static readonly DiagnosticDefinition<object> CycleInSerialization = new(
            "CR0201",
            _category,
            "Cannot serialize the compile-time value of type '{0}' to a run-time value because it contains a cyclic reference.",
            Error,
            "A collection contains itself." );

        public static readonly DiagnosticDefinition<object> MultidimensionalArray = new(
            "CR0202",
            _category,
            "Cannot serialize the compile-time array of type '{0}' because it has more than one dimension.",
            Error,
            "Multidimensional arrays not supported." );

        public static readonly DiagnosticDefinition<object> UnsupportedDictionaryComparer = new(
            "CR0203",
            _category,
            "Cannot serialize the compile-time dictionary into a run-time value because it has an unsupported equality comparer '{0}'. "
            + " Only the default comparer and predefined string comparers are supported.",
            Error,
            "Custom equality comparers not supported." );
    }
}