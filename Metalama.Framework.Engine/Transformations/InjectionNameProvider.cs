// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Provides names for overriden declarations.
    /// </summary>
    internal abstract class InjectionNameProvider
    {
        internal abstract string GetOverrideName( INamedType targetType, AspectLayerId aspectLayer, IMember overriddenMember );

        internal abstract string GetInitializerName( INamedType targetType, AspectLayerId aspectLayer, IMember initializedMember );

        internal abstract string GetInitializationName(
            INamedType targetType,
            AspectLayerId aspectLayer,
            IDeclaration targetDeclaration,
            InitializerKind reason );

        internal abstract TypeSyntax GetOverriddenByType( IAspectInstanceInternal aspect, IMember overriddenMember );
    }
}