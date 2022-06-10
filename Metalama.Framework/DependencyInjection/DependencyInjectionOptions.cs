// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.DependencyInjection;

/// <summary>
/// Options that influence the processing of <see cref="DependencyAttribute"/>.
/// </summary>
public sealed class DependencyInjectionOptions : ProjectExtension
{
    private readonly List<IDependencyInjectionFramework> _frameworks = new();
    private Func<WeaveDependencyContext, IDependencyInjectionFramework?>? _selector;

    /// <summary>
    /// Registers an implementation of the <see cref="IDependencyInjectionFramework"/> class.
    /// </summary>
    public void RegisterFramework( IDependencyInjectionFramework framework )
    {
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }

        this._frameworks.Add( framework );
    }

    /// <summary>
    /// Gets or sets a delegate that is called when several dependency injection frameworks have been registered
    /// for the current project and many vote to handle a given dependency.
    /// </summary>
    public Func<WeaveDependencyContext, IDependencyInjectionFramework?>? Selector
    {
        get => this._selector;
        set
        {
            if ( this.IsReadOnly )
            {
                throw new InvalidOperationException();
            }

            this._selector = value;
        }
    }

    internal bool TryGetFramework( WeaveDependencyContext context, [NotNullWhen( true )] out IDependencyInjectionFramework? framework )
    {
        var eligibleFrameworks = this._frameworks.Where( f => f.CanInjectDependency( context ) ).ToImmutableArray();

        if ( eligibleFrameworks.IsEmpty )
        {
            if ( this._frameworks.Count == 0 )
            {
                context.Diagnostics.Report(
                    FrameworkDiagnosticDescriptors.NoDependencyInjectionFrameworkRegistered.WithArguments(
                        (context.AspectFieldOrProperty, context.TargetType) ) );
            }
            else
            {
                context.Diagnostics.Report(
                    FrameworkDiagnosticDescriptors.NoSuitableDependencyInjectionFramework.WithArguments(
                        (context.AspectFieldOrProperty, context.TargetType) ) );
            }

            framework = null;

            return false;
        }

        if ( eligibleFrameworks.Length == 1 )
        {
            framework = eligibleFrameworks[0];
        }
        else if ( this.Selector == null )
        {
            context.Diagnostics.Report(
                FrameworkDiagnosticDescriptors.MoreThanOneSuitableDependencyInjectionFramework.WithArguments(
                    (context.AspectFieldOrProperty, context.TargetType) ) );

            framework = null;

            return false;
        }
        else
        {
            framework = this.Selector.Invoke( context );

            if ( framework == null )
            {
                context.Diagnostics.Report(
                    FrameworkDiagnosticDescriptors.MoreThanOneSuitableDependencyInjectionFramework.WithArguments(
                        (context.AspectFieldOrProperty, context.TargetType) ) );

                return false;
            }
        }

        return true;
    }
}