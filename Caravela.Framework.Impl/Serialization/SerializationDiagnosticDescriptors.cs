// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
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
            "Cannot serialize the type '{0}' but no serializer is registered for that type.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<object> CycleInSerialization = new(
            "CR0201",
            "A collection contains itself.",
            "Cannot serialize this instance of the '{0}' type because it contains a cyclic reference.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<object> MultidimensionalArray = new(
            "CR0202",
            "Multidimensional arrays not supported.",
            "Cannot serialize the array '{0}' because it has more than one dimension.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<object> UnsupportedDictionaryComparer = new(
            "CR0203", "Custom equality comparers not supported.",
            "Cannot serialize the dictionary has an equality comparer '{0}' that is not supported. "
            + " Only the default comparer and predefined string comparers are supported.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<IType> TypeNotSerializable = new(
            "CR0204",
            "Type not serializable.",
            "Compile-time type value '{0}' is of a form that is not supported for serialization.",
            _category,
            Error );

        public static readonly StrongDiagnosticDescriptor<IType> MoreThanOneAdvicePerElement = new(
            "CR0205",
            "More than one advice per code element.",
            "'{0}' has more than one advice, which is currently not supported.",
            _category,
            Error );
    }
}