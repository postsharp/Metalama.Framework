// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Member conflict behavior of interface introduction advice.
    /// </summary>
    [CompileTime]
    public enum InterfaceMemberOverrideStrategy
    {
        /// <summary>
        /// The advice fails with a compilation error if a matching interface member already exists in the target declaration. Same as <see cref="Fail"/>.
        /// </summary>
        Default = Fail,

        /// <summary>
        /// The advice fails with a compilation error if a matching interface member already exists in the target declaration.
        /// </summary>
        Fail = 0,

        // TODO: Implement. Note that C# implements the interface with visible base members when they are equal to interface members:
        //       public interface I { void Foo(); }        
        //       public class A { public void Foo() { } }
        //       public class B : A, I { }
        // /// <summary>
        // /// The advice uses the existing type member if it exactly matches the interface member (including type parameter constraints and return value), otherwise the advice fails with a compilation error.
        // /// </summary>
        // UseExisting = 1,

        // /// <summary>
        // /// The advice introduces the interface member as explicit even if the interface member was supposed to be introduced as implicit.
        // /// </summary>
        // MakeExplicit = 2,
    }
}