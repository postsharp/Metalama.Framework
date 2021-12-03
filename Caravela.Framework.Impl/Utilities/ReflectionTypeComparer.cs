// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Utilities
{
    internal class ReflectionTypeComparer : IEqualityComparer<Type>
    {
        public bool Equals( Type x, Type y )
        {
            if ( x == y )
            {
                return true;
            }
            else if ( (x, y) is (CompileTimeType xWrapper, CompileTimeType yWrapper ) )
            {
                return xWrapper.Target.Equals( yWrapper.Target );
            }

            return false;
        }

        public int GetHashCode( Type obj ) => obj.GetHashCode();
    }
}