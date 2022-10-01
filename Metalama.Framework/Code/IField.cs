// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a field. Note that fields can be promoted to properties by aspects.
    /// </summary>
    public interface IField : IFieldOrProperty
    {
        [CompileTimeReturningRunTime]
        FieldInfo ToFieldInfo();
    }
}