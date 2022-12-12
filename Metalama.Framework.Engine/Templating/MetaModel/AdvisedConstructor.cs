// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Reflection;
using MethodBase = System.Reflection.MethodBase;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal sealed class AdvisedConstructor : AdvisedMember<IConstructorImpl>, IAdvisedConstructor, IConstructorImpl
    {
        public AdvisedConstructor( IConstructor underlying ) : base( (IConstructorImpl) underlying ) { }

        public object Invoke( params object?[] args ) => throw new NotSupportedException();

        public ConstructorInfo ToConstructorInfo() => this.Underlying.ToConstructorInfo();

        public MethodBase ToMethodBase() => this.ToConstructorInfo();

        [Memo]
        public IAdvisedParameterList Parameters => new AdvisedParameterList( this.Underlying );

        IParameterList IHasParameters.Parameters => this.Underlying.Parameters;

        public ConstructorInitializerKind InitializerKind => this.Underlying.InitializerKind;

        public IMember? OverriddenMember => null;

        public IConstructor? GetBaseConstructor() => this.Underlying.GetBaseConstructor();
    }
}