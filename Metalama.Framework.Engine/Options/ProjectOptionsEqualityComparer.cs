// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Options;

[GenerateEquality( typeof(IProjectOptions) )]
public static partial class ProjectOptionsEqualityComparer
{
    [AttributeUsage( AttributeTargets.Class )]
    internal class GenerateEqualityAttribute( Type targetType ) : Attribute
    {
        public Type TargetType { get; } = targetType;
    }
}