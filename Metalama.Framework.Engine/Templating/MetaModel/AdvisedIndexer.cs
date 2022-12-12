// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal class AdvisedIndexer : AdvisedFieldOrPropertyOrIndexer<IIndexerImpl>, IAdvisedIndexer
    {
        public AdvisedIndexer( IIndexer underlying ) : base( (IIndexerImpl) underlying ) { }

        public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public PropertyInfo ToPropertyInfo() => this.Underlying.ToPropertyInfo();

        public IInvokerFactory<IIndexerInvoker> Invokers => this.Underlying.Invokers;

        public IIndexer? OverriddenIndexer => this.Underlying.OverriddenIndexer;

        IParameterList IHasParameters.Parameters => this.Underlying.Parameters;

        public IAdvisedParameterList Parameters => new AdvisedParameterList( this.Underlying );
    }
}