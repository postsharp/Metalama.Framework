// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    internal enum SerializationIntrinsicType : byte
    {
        None,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Single,
        Double,
        String,
        DottedString,
        Char,
        Boolean,
        SByte,
        Struct,
        Class,
        Array,
        ObjRef,
        Type,
        GenericTypeParameter,
        GenericMethodParameter,
        Enum
    }
}