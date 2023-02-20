// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using static Metalama.Framework.Diagnostics.Severity;

#pragma warning disable SA1118 // Allow multi-line parameters.

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    public static class SerializationDiagnosticDescriptors
    {
        // Reserved range 200-219

        private const string _category = "Metalama.Serialization";

        internal static readonly DiagnosticDefinition<object> UnsupportedSerialization = new(
            "LAMA0200",
            _category,
            "A compile-time value of type '{0}' was used in a context where a run-time value was expected.",
            Error,
            "Compile-time type not serializable." );

        internal static readonly DiagnosticDefinition<object> CycleInSerialization = new(
            "LAMA0201",
            _category,
            "Cannot serialize the compile-time value of type '{0}' to a run-time value because it contains a cyclic reference.",
            Error,
            "A collection contains itself." );

        internal static readonly DiagnosticDefinition<object> MultidimensionalArray = new(
            "LAMA0202",
            _category,
            "Cannot serialize the compile-time array of type '{0}' because it has more than one dimension.",
            Error,
            "Multidimensional arrays not supported." );

        internal static readonly DiagnosticDefinition<object> UnsupportedDictionaryComparer = new(
            "LAMA0203",
            _category,
            "Cannot serialize the compile-time dictionary into a run-time value because it has an unsupported equality comparer '{0}'. "
            + " Only the default comparer and predefined string comparers are supported.",
            Error,
            "Custom equality comparers not supported." );
    }
}