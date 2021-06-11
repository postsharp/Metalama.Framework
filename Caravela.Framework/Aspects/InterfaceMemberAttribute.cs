// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that marks the target method as a template for <see cref="IAdviceFactory.IntroduceProperty(Caravela.Framework.Code.INamedType,string,Caravela.Framework.Aspects.IntroductionScope,Caravela.Framework.Aspects.ConflictBehavior,Caravela.Framework.Aspects.AdviceOptions?)"/> and results in creation of the advice.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
    public sealed class InterfaceMemberAttribute : CompileTimeOnlyAttribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the interface member should be introduced explicitly.
        /// </summary>
        public bool IsExplicit { get; set; }
    }
}