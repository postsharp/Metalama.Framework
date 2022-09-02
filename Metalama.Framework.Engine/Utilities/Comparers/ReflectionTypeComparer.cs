// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.ReflectionMocks;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Comparers
{
    internal class ReflectionTypeComparer : IEqualityComparer<Type>
    {
        public bool Equals( Type x, Type y )
        {
            if ( x == y )
            {
                return true;
            }
            else if ( (x, y) is (CompileTimeType xWrapper, CompileTimeType yWrapper) )
            {
                return xWrapper.Target.Equals( yWrapper.Target );
            }

            return false;
        }

        public int GetHashCode( Type obj ) => obj.GetHashCode();
    }
}