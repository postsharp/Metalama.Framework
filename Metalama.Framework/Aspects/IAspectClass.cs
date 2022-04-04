﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System;
using System.ComponentModel;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Represents the metadata of an aspect class.
    /// </summary>
    [InternalImplement]
    [CompileTime]
    public interface IAspectClass
    {
        /// <summary>
        /// Gets the fully qualified type of the aspect.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the name of the aspect type without the namespace and without the <c>Attribute</c> suffix.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Gets the name of the aspect displayed to the design-time UI. This property should not be used to report diagnostics (<see cref="ShortName"/>
        /// should be used instead). It can be set by adding the <see cref="DisplayNameAttribute"/> custom attribute to the aspect class. By default, it is equal to <see cref="ShortName"/>.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the optional long description of the aspect. This property can be set by adding the <see cref="DescriptionAttribute"/> custom attribute to the aspect class. By default, it is <c>null</c>.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets a value indicating whether the aspect class is an abstract class.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether instances of this aspect class are inherited by derived declarations.
        /// This property can be set by adding the <see cref="InheritedAttribute"/> custom attribute to the aspect class.
        /// </summary>
        bool IsInherited { get; }

        /// <summary>
        /// Gets a value indicating whether the aspect class derives from <see cref="System.Attribute" />.
        /// </summary>
        bool IsAttribute { get; }

        /// <summary>
        /// Gets the type of the aspect. 
        /// </summary>
        Type Type { get; }
    }
}