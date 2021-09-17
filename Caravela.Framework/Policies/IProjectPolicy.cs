// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Policies
{
    /// <summary>
    /// A project policy is a type that can add aspects and validators to the compilation in which they are defined. (Not implemented.)
    /// </summary>
    /// <remarks>
    ///  Project policies can also be defined outside the project in a file named <c>CaravelaPolicy.cs</c> and located
    ///  in any parent directory of the project directory. The code in these files will be moved to a randomly-generated
    ///  namespace at build time, so it is not possible to reference code in these files. Project policies are executed
    ///  in inverse depth order, i.e. the one closest to the root directory first, and the ones located in the project itself last.
    ///  When several policies have the same directory depth, they are alphabetically ordered by type name.
    /// </remarks>
    [CompileTimeOnly]
    [Obsolete( "Not implemented." )]
    public interface IProjectPolicy
    {
        void BuildPolicy( IProjectPolicyBuilder builder );
    }
}