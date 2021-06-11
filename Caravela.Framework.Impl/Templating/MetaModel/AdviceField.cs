// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects.AdvisedCode;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceField : AdviceFieldOrProperty<IField>, IAdviceField
    {
        public AdviceField( IField underlying ) : base( underlying ) { }
    }
}