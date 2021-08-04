// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that applied to a member of an aspect class and means that this aspect member is a template implementing a member of an interface implemented by
    /// <see cref="IAdviceFactory.ImplementInterface(Caravela.Framework.Code.INamedType,Caravela.Framework.Code.INamedType,Caravela.Framework.Aspects.OverrideStrategy,System.Collections.Generic.Dictionary{string,object?}?)"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
    public sealed class InterfaceMemberAttribute : TemplateAttribute
    {
        public InterfaceMemberAttribute() : base( TemplateKind.Introduction ) { }

        /// <summary>
        /// Gets or sets a value indicating whether the interface member should be introduced explicitly.
        /// </summary>
        public bool IsExplicit { get; set; }
    }
}