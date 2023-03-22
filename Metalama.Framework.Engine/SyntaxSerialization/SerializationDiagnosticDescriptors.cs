// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
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

        internal static readonly DiagnosticDefinition<(INamedTypeSymbol Type, INamedTypeSymbol BaseType)> MissingDeserializingConstructor = new(
            "LAMA0204",
            _category,
            "Cannot generate a compile-time serializer for '{0}', the base type '{1}' declared in a referenced assembly is serializable " +
            "and does not have a visible deserializing constructor with a single parameter of type IArgumentsReader.",
            Error,
            "Missing deserializing constructor." );

        internal static readonly DiagnosticDefinition<(INamedTypeSymbol Type, INamedTypeSymbol BaseType)> MissingParameterlessConstructor = new(
            "LAMA0205",
            _category,
            "Cannot generate a compile-time serializer for '{0}', the base type '{1}' is not serializable and does not have a visible parameterless constructor.",
            Error,
            "Missing parameterless constructor." );

        internal static readonly DiagnosticDefinition<(INamedTypeSymbol Type, INamedTypeSymbol BaseType)> MissingBaseSerializer = new(
            "LAMA0206",
            _category,
            "Cannot generate a compile-time serializer for '{0}', the base type '{1}' declared in a referenced assembly must declare a visible serializer.",
            Error,
            "Missing base serializer." );

        internal static readonly DiagnosticDefinition<(INamedTypeSymbol Type, INamedTypeSymbol BaseTypeSerializer)> MissingBaseSerializerConstructor = new(
            "LAMA0207",
            _category,
            "Cannot generate a compile-time serializer for '{0}', the base type serializer '{1}' must declare a visible parameterless constructor.",
            Error,
            "Missing base serializer constructor." );
    }
}