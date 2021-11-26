// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Reflection;

namespace Caravela.Framework.Impl.Utilities.Dump
{
    public interface IDumpFormatter
    {
        object FormatLazyPropertyValue( object owner, MethodInfo getter );

        object FormatPropertyValue( object value, MethodInfo getter );

        object FormatException( Exception exception );
    }
}