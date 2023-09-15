// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Eligibility.Implementation;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.Aspects;

internal partial class EligibilityHelper
{
    private readonly object _prototype;
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly UserCodeInvoker _userCodeInvoker;
    private readonly List<KeyValuePair<Type, IEligibilityRule<IDeclaration>>> _eligibilityRules = new();

    private bool TryInitializeEligibility<T>( IDiagnosticAdder diagnosticAdder, List<KeyValuePair<Type, IEligibilityRule<IDeclaration>>> rules )
        where T : class, IDeclaration
    {
        if ( this._prototype is IEligible<T> eligible )
        {
            var builder = new EligibilityBuilder<T>();

            var executionContext = new UserCodeExecutionContext(
                this._serviceProvider,
                diagnosticAdder,
                UserCodeDescription.Create( "executing BuildEligibility for {0}", this ) );

            if ( !this._userCodeInvoker.TryInvoke( () => eligible.BuildEligibility( builder ), executionContext ) )
            {
                return false;
            }

            rules.Add( new KeyValuePair<Type, IEligibilityRule<IDeclaration>>( typeof(T), ((IEligibilityBuilder<T>) builder).Build() ) );
        }

        return true;
    }

    private static readonly MethodInfo _tryInitializeEligibilityMethod = typeof(AspectClass).GetMethod(
            nameof(TryInitializeEligibility),
            BindingFlags.Instance | BindingFlags.NonPublic )
        .AssertNotNull();

    public EligibilityHelper( object prototype, ProjectServiceProvider serviceProvider )
    {
        this._prototype = prototype;
        this._serviceProvider = serviceProvider;
        this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
    }

    public bool PopulateRules( IDiagnosticAdder diagnosticAdder )
    {
        var eligibilitySuccess = true;

        foreach ( var implementedInterface in this._prototype.GetType()
                     .GetInterfaces()
                     .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEligible<>) ) )
        {
            var declarationInterface = implementedInterface.GenericTypeArguments[0];

            // If methods are eligible, we need to check that the target method is not a local function.
            if ( declarationInterface == typeof(IMethod) )
            {
                this._eligibilityRules.Add( new KeyValuePair<Type, IEligibilityRule<IDeclaration>>( typeof(IMethod), LocalFunctionEligibilityRule.Instance ) );
            }

            eligibilitySuccess &= (bool)
                _tryInitializeEligibilityMethod.MakeGenericMethod( declarationInterface )
                    .Invoke( this, new object[] { diagnosticAdder, this._eligibilityRules } )!;
        }

        return eligibilitySuccess;
    }

    public void Add( Type type, IEligibilityRule<IDeclaration> eligibilityRule )
    {
        this._eligibilityRules.Add( new KeyValuePair<Type, IEligibilityRule<IDeclaration>>( type, eligibilityRule ) );
    }

    public EligibleScenarios GetEligibility( IDeclaration obj, bool isInheritable )
    {
        if ( this._eligibilityRules.Count == 0 )
        {
            // Linker tests do not set this member but don't need to test eligibility.
            return EligibleScenarios.Default;
        }

        // We may execute user code, so we need to execute in a user context. This is not optimal, but we don't know,
        // in the current design, where we have user code. Also, we cannot report diagnostics in the current design,
        // so we have to let the exception fly.
        var executionContext = new UserCodeExecutionContext(
            this._serviceProvider,
            UserCodeDescription.Create( "evaluating eligibility for {0} applied to '{1}'", this, obj ),
            compilationModel: obj.GetCompilationModel() );

        return this._userCodeInvoker.Invoke( GetEligibilityCore, executionContext );

        // Implementation, which all runs in a user context.
        EligibleScenarios GetEligibilityCore()
        {
            var declarationType = obj.GetType();
            var eligibility = EligibleScenarios.All;

            // If the aspect cannot be inherited, remove the inheritance eligibility.
            if ( isInheritable != true )
            {
                eligibility &= ~EligibleScenarios.Inheritance;
            }

            // Evaluate all eligibility rules that apply to the target declaration type.
            foreach ( var rule in this._eligibilityRules )
            {
                if ( rule.Key.IsAssignableFrom( declarationType ) )
                {
                    eligibility &= rule.Value.GetEligibility( obj );

                    if ( eligibility == EligibleScenarios.None )
                    {
                        return EligibleScenarios.None;
                    }
                }
            }

            return eligibility;
        }
    }

    public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<IDeclaration> describedObject )
    {
        var targetDeclaration = describedObject.Object;
        var declarationType = targetDeclaration.GetType();

        var group = new AndEligibilityRule<IDeclaration>(
            this._eligibilityRules.Where( r => r.Key.IsAssignableFrom( declarationType ) )
                .Select( r => r.Value )
                .ToImmutableArray() );

        // We may execute user code, so we need to execute in a user context. This is not optimal, but we don't know,
        // in the current design, where we have user code. Also, we cannot report diagnostics in the current design,
        // so we have to let the exception fly.
        var executionContext = new UserCodeExecutionContext(
            this._serviceProvider,
            UserCodeDescription.Create( "evaluating eligibility description for {0} applied to '{1}'", this, describedObject ) );

        return this._userCodeInvoker.Invoke(
            () =>
                group.GetIneligibilityJustification(
                    requestedEligibility,
                    new DescribedObject<IDeclaration>( targetDeclaration, $"'{targetDeclaration}'" ) ),
            executionContext );
    }
}