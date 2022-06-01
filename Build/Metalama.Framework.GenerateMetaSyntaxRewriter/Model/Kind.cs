// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#nullable disable

using System.Xml.Serialization;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model
{
    public class Kind
    {
        [XmlAttribute]
        public string Name { get; set; }

        public override bool Equals( object obj )
            => obj is Kind kind &&
               this.Name == kind.Name;

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => this.Name.GetHashCode();
        public override string ToString() => this.Name;
    }
}