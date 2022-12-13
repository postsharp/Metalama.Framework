﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that applied to a member of an aspect class and means that this aspect member is a template implementing a member of an interface implemented by
    /// <see cref="IAdviceFactory.ImplementInterface(Code.INamedType,Code.INamedType,OverrideStrategy,object?)"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
    public sealed class InterfaceMemberAttribute : TemplateAttribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the interface member should be introduced explicitly.
        /// </summary>
        public bool IsExplicit { get; set; }

        /// <summary>
        /// Gets or sets a value indication the override strategy when interface member conflicts with an existing class member.
        /// </summary>
        public InterfaceMemberOverrideStrategy WhenExists { get; set; }
    }
}