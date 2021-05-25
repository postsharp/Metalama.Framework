// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceInterfaceAdvice : Advice, IIntroduceInterfaceAdvice
    {
        private readonly IReadOnlyDictionary<IMember, IMember>? _explicitMemberMap;
        private readonly Dictionary<IMember, IMember>? _implicitMemberMap;

        public INamedType InterfaceType { get; }

        public bool IsExplicit { get; }

        public IReadOnlyDictionary<IMember, IMember> MemberMap => this._explicitMemberMap ?? this._implicitMemberMap!;

        public ConflictBehavior ConflictBehavior { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IntroduceInterfaceAdvice(
            AspectInstance aspect,
            INamedType targetType,
            INamedType interfaceType,
            bool isExplicit,
            IReadOnlyDictionary<IMember, IMember>? memberMap,
            ConflictBehavior conflictBehavior,
            AspectLinkerOptions? linkerOptions,
            IReadOnlyDictionary<string, object?> aspectTags ) : base( aspect, targetType, aspectTags )
        {
            this.InterfaceType = interfaceType;
            this.IsExplicit = isExplicit;
            this._explicitMemberMap = memberMap;
            this.ConflictBehavior = conflictBehavior;
            this.LinkerOptions = linkerOptions;

            if ( this._explicitMemberMap == null )
            {
                this._implicitMemberMap = new Dictionary<IMember, IMember>();
            }
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            if ( this._explicitMemberMap == null )
            {
                // No explicit member map was given, we have to detect introduced members correspoding to all interface members.
                var compilation = this.TargetDeclaration.Compilation;

                foreach ( var interfaceMethod in this.InterfaceType.Methods )
                {
                    var introductionAdvice = declarativeAdvices
                        .OfType<IntroduceMethodAdvice>()
                        .SingleOrDefault(
                            x =>
                                x.Builder.Name == interfaceMethod.Name
                                && x.Builder.GenericParameters.Count == interfaceMethod.GenericParameters.Count
                                && x.Builder.Parameters.Count == interfaceMethod.Parameters.Count
                                && x.Builder.Parameters
                                    .Select( ( p, i ) => (p, i) )
                                    .All(
                                        x =>
                                            compilation.InvariantComparer.Equals( x.p.ParameterType, interfaceMethod.Parameters[x.i].ParameterType )
                                            && x.p.RefKind == interfaceMethod.Parameters[x.i].RefKind ) );

                    if ( introductionAdvice == null )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMemberIntroduction.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, this.InterfaceType, interfaceMethod) ) );
                    }
                    else if (
                        !compilation.InvariantComparer.Equals(
                            interfaceMethod.ReturnParameter.ParameterType,
                            introductionAdvice.Builder.ReturnParameter.ParameterType )
                        || interfaceMethod.ReturnParameter.RefKind != introductionAdvice.Builder.ReturnParameter.RefKind )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberIntroductionDoesNotMatch.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, this.InterfaceType, introductionAdvice.Builder,
                                 interfaceMethod) ) );
                    }
                    else
                    {
                        this._implicitMemberMap!.Add( interfaceMethod, introductionAdvice.Builder );
                    }
                }

                foreach ( var interfaceProperty in this.InterfaceType.Properties )
                {
                    var introductionAdvice = declarativeAdvices
                        .OfType<IntroducePropertyAdvice>()
                        .SingleOrDefault(
                            x =>
                                x.Builder.Name == interfaceProperty.Name
                                && x.Builder.Parameters.Count == interfaceProperty.Parameters.Count
                                && x.Builder.Parameters
                                    .Select( ( p, i ) => (p, i) )
                                    .All(
                                        x =>
                                            compilation.InvariantComparer.Equals( x.p.ParameterType, interfaceProperty.Parameters[x.i].ParameterType )
                                            && x.p.RefKind == interfaceProperty.Parameters[x.i].RefKind ) );

                    if ( introductionAdvice == null )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMemberIntroduction.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, this.InterfaceType, interfaceProperty) ) );
                    }
                    else if (
                        !compilation.InvariantComparer.Equals( interfaceProperty.Type, introductionAdvice.Builder.Type )
                        || interfaceProperty.RefKind != introductionAdvice.Builder.RefKind )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberIntroductionDoesNotMatch.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, this.InterfaceType, introductionAdvice.Builder,
                                 interfaceProperty) ) );
                    }
                    else
                    {
                        this._implicitMemberMap!.Add( interfaceProperty, introductionAdvice.Builder );
                    }
                }

                foreach ( var interfaceEvent in this.InterfaceType.Events )
                {
                    var introductionAdvice = declarativeAdvices
                        .OfType<IntroduceEventAdvice>()
                        .SingleOrDefault( x => x.Builder.Name == interfaceEvent.Name );

                    if ( introductionAdvice == null )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.MissingDeclarativeInterfaceMemberIntroduction.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, this.InterfaceType, interfaceEvent) ) );
                    }
                    else if ( !compilation.InvariantComparer.Equals( interfaceEvent.EventType, introductionAdvice.Builder.EventType ) )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.DeclarativeInterfaceMemberIntroductionDoesNotMatch.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.DisplayName, this.TargetDeclaration, this.InterfaceType, introductionAdvice.Builder,
                                 interfaceEvent) ) );
                    }
                    else
                    {
                        this._implicitMemberMap!.Add( interfaceEvent, introductionAdvice.Builder );
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: Conflicts, errors, etc.

            return AdviceResult.Create( new IntroducedInterface( this, this.TargetDeclaration, this.InterfaceType, this.IsExplicit, this.MemberMap ) );
        }
    }
}