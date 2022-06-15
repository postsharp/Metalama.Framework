// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using MethodBase = System.Reflection.MethodBase;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal class AdvisedFinalizer : AdvisedMember<IFinalizerImpl>, IAdvisedFinalizer, IFinalizerImpl
    {
        public AdvisedFinalizer( IFinalizer underlying ) : base( (IFinalizerImpl) underlying ) { }

        [Memo]
        public IAdvisedParameterList Parameters => new AdvisedParameterList( this.Underlying );

        IParameterList IHasParameters.Parameters => this.Underlying.Parameters;

        public MethodKind MethodKind => this.Underlying.MethodKind;

        public MethodBase ToMethodBase() => this.Underlying.ToMethodBase();

        public IFinalizer? OverriddenFinalizer => this.Underlying.OverriddenFinalizer;

        public IMember? OverriddenMember => this.OverriddenFinalizer;
    }
}