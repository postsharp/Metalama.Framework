﻿using Caravela.Framework.Code;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Member : CodeElement, IMember
    {
        public abstract string Name { get; }

        public abstract bool IsStatic { get; }

        public abstract bool IsVirtual { get; }

        INamedType? IMember.DeclaringType => this.DeclaringType;

        public NamedType? DeclaringType => (NamedType?)this.ContainingElement;

        public Member(NamedType declaringType) : base(declaringType)
        {
        }
    }
}
