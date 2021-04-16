// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl.Serialization
{
    internal static class SerializationDiagnosticDescriptors
    {
        // Reserved range 200-299

        private const string _category = "Caravela.Serialization";

        public static readonly StrongDiagnosticDescriptor<Type> UnsupportedSerialization =
            new( "CR0200", "Build-time code not serializable.", "Build-time code attempted to create {0} but no serializer is registered for that type.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<Type> CycleInSerialization =
            new( "CR0201", "A collection contains itself.", "Build-time code attempted to create a collection which contains itself: {0} ", _category, Error );

        public static readonly StrongDiagnosticDescriptor<Type> MultidimensionalArray =
            new( "CR0202", "Multidimensional arrays not supported.", "Build-time array {0} has more than one dimension.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<Type> UnsupportedDictionaryComparer =
            new( "CR0203", "Custom equality comparers not supported.", "Build-time dictionary has an equality comparer '{0}' that is not supported. Only the default comparer and predefined string comparers are supported.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<IType> TypeNotSerializable =
            new( "CR0204", "Type not serializable.", "Build-time type value '{0}' is of a form that is not supported for serialization.", _category, Error );

        public static readonly StrongDiagnosticDescriptor<IType> MoreThanOneAdvicePerElement =
            new( "CR0205", "More than one advice per code element.", "'{0}' has more than one advice, which is currently not supported.", _category, Error );
    }
}