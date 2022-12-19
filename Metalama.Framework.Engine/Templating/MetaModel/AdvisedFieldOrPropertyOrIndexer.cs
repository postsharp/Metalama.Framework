// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal abstract class AdvisedFieldOrPropertyOrIndexer<T> : AdvisedMember<T>, IAdvisedFieldOrPropertyOrIndexer
        where T : IFieldOrPropertyOrIndexer, IDeclarationImpl
    {
        protected AdvisedFieldOrPropertyOrIndexer( T underlying ) : base( underlying ) { }

        [Obfuscation( Exclude = true )]
        public bool IsAssignable => this.Underlying.Writeability >= Writeability.ConstructorOnly;

        public IType Type => this.Underlying.Type;

        public RefKind RefKind => this.Underlying.RefKind;

        IMethod? IFieldOrPropertyOrIndexer.GetMethod => this.GetMethod;

        IMethod? IFieldOrPropertyOrIndexer.SetMethod => this.SetMethod;

        [Memo]
        public IAdvisedMethod? GetMethod => this.Underlying.GetMethod != null ? new AdvisedMethod( (IMethodImpl) this.Underlying.GetMethod ) : null;

        [Memo]
        public IAdvisedMethod? SetMethod => this.Underlying.SetMethod != null ? new AdvisedMethod( (IMethodImpl) this.Underlying.SetMethod ) : null;

        public Writeability Writeability => this.Underlying.Writeability;

        public IMethod? GetAccessor( MethodKind methodKind ) => this.Underlying.GetAccessor( methodKind );

        public IEnumerable<IMethod> Accessors => this.Underlying.Accessors;
    }
}