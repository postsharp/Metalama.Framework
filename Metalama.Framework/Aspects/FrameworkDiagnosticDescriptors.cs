// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Aspects;

// Range: 0700-0701

internal static class FrameworkDiagnosticDescriptors
{
    internal static readonly DiagnosticDefinition<(string AspectType, DeclarationKind IntroducedDeclarationKind, DeclarationKind TargetDeclarationKind)>
        CannotUseIntroduceWithoutDeclaringType = new(
            "LAMA0700",
            "Cannot use [Introduce] in an aspect that is applied to a declaration that is neither a type nor a type member.",
            "The aspect '{0}' cannot introduce a {1} because it has been applied to a {2}, which is neither a type nor a type member.",
            "Metalama.Advices",
            Severity.Error );

    internal static readonly DiagnosticDefinition<(IFieldOrProperty Dependency, INamedType TargetType)>
        NoDependencyInjectionFrameworkRegistered = new(
            "LAMA0701",
            "No dependency injection framework has been registered.",
            "Cannot introduce the dependency '{0}' to '{1}' because no dependency injection framework has been registered with Metalama. Add a Metalama dependency injection framework adapter to your project.",
            "Metalama.DependencyInjection",
            Severity.Error );

    internal static readonly DiagnosticDefinition<(IFieldOrProperty Dependency, INamedType TargetType)>
        NoSuitableDependencyInjectionFramework = new(
            "LAMA0702",
            "None of the registered dependency injection frameworks can handle a dependency.",
            "Cannot introduce the dependency '{0}' into '{1} because of none of the registered dependency injection frameworks is able to handle this dependency.",
            "Metalama.DependencyInjection",
            Severity.Error );

    internal static readonly DiagnosticDefinition<(IFieldOrProperty Dependency, INamedType TargetType)>
        MoreThanOneSuitableDependencyInjectionFramework = new(
            "LAMA0703",
            "More than one dependency injection framework can handle a dependency and no Selector has been specified.",
            "Cannot introduce the dependency '{0}' into '{1} because more than one of the registered dependency injection frameworks can handle this dependency, and the DependencyInjectionOptions.Selector property has not been set.",
            "Metalama.DependencyInjection",
            Severity.Error );

    internal static readonly DiagnosticDefinition<(IFieldOrProperty Dependency, INamedType TargetType)>
        NoSelectedDependencyInjectionFramework = new(
            "LAMA0704",
            "The DependencyInjectionOptions.Selector implementation did not select any framework.",
            "Cannot introduce the dependency '{0}' into '{1} because the DependencyInjectionOptions.Selector did not select any of the eligible frameworks.",
            "Metalama.DependencyInjection",
            Severity.Error );
}