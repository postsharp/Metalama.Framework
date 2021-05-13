﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceFieldAdvice : IntroduceMemberAdvice, IIntroduceFieldAdvice
    {
        public IFieldBuilder Builder { get; }

        public new INamedType TargetDeclaration => base.TargetDeclaration!;

        public IntroduceFieldAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            string name,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            AspectLinkerOptions? linkerOptions,
            IReadOnlyDictionary<string, object?> tags )
            : base( aspect, targetDeclaration, null, scope, conflictBehavior, tags, linkerOptions )
        {
            this.Builder = new FieldBuilder( this, this.TargetDeclaration, name, linkerOptions );
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            throw new NotImplementedException();
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            throw new NotImplementedException();
        }
    }
}