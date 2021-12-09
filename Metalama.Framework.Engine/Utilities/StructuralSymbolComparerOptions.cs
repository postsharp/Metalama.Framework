// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Utilities
{
    [Flags]
    internal enum StructuralSymbolComparerOptions
    {
        ContainingAssembly = 1 << 0,
        ContainingDeclaration = 1 << 1,
        Name = 1 << 2,
        GenericParameterCount = 1 << 3,
        GenericArguments = 1 << 4,
        ParameterTypes = 1 << 5,
        ParameterModifiers = 1 << 6,
        FieldPromotions = 1 << 7,

        MethodSignature = Name | GenericParameterCount | GenericArguments | ParameterTypes | ParameterModifiers,
        Type = Name | GenericParameterCount | GenericArguments
    }
}