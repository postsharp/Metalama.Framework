// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.TestFramework
{
    /// <summary>
    /// This attribute marks the declaration that should be included in the output
    /// and compared with the expected output. 
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    public sealed class TestOutputAttribute : Attribute
    {
    }
}