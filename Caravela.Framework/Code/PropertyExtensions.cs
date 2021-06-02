// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    public static class PropertyExtensions
    {
        // TODO: C# 10 ref fields: implement and update this documentation comment

        /// <summary>
        /// Returns <c>true</c> for <c>ref</c> and <c>ref readonly</c> properties.
        /// </summary>
        public static bool IsByRef( this IProperty property ) => property.RefKind != RefKind.None;

        /// <summary>
        /// Returns <c>true</c> for <c>ref</c> but <c>false</c> for <c>ref readonly</c> properties.
        /// </summary>
        public static bool IsRef( this IProperty property ) => property.RefKind == RefKind.Ref;

        /// <summary>
        /// Returns <c>true</c> for <c>ref readonly</c> properties.
        /// </summary>
        public static bool IsRefReadonly( this IProperty property ) => property.RefKind == RefKind.RefReadOnly;
    }
}