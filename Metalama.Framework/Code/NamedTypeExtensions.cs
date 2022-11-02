// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;

namespace Metalama.Framework.Code;

[CompileTime]
public static class NamedTypeExtensions
{
    public static IEnumerable<IMethod> MethodsAndAccessors( this INamedType type )
    {
        foreach ( var m in type.Methods )
        {
            yield return m;
        }

        foreach ( var p in type.Properties )
        {
            if ( p.GetMethod != null )
            {
                yield return p.GetMethod;
            }

            if ( p.SetMethod != null )
            {
                yield return p.SetMethod;
            }
        }

        foreach ( var e in type.Events )
        {
            yield return e.AddMethod;
            yield return e.RemoveMethod;
        }
    }
}