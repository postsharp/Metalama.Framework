// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a field. Note that fields can be promoted to properties by aspects.
    /// </summary>
    public interface IField : IFieldOrProperty
    {
        FieldInfo ToFieldInfo();
    }
}