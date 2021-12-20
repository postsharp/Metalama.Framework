// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PromotedField : PropertyBuilder, IReplaceMember
    {
        private readonly IField _field;

        public MemberRef<IMemberOrNamedType> ReplacedMember => this._field.ToMemberRef<IMemberOrNamedType>();

        public EqualsValueClauseSyntax? InitializerSyntax { get; set; }

        public PromotedField( Advice advice, IField field ) : base(
            advice,
            field.DeclaringType,
            field.Name,
            true,
            field.Writeability == Writeability.All,
            true,
            false )
        {
            this._field = field;
            this.Type = field.Type;
            this.Accessibility = this._field.Accessibility;
            this.IsStatic = this._field.IsStatic;

            if ( this._field is BuiltField builtField )
            {
                // For field builder, copy the expression and template.
                this.InitializerTemplate = builtField.FieldBuilder.InitializerTemplate;
                this.InitializerExpression = builtField.FieldBuilder.InitializerExpression;
            }
            else
            {
                // For original code fields, copy the initializer syntax.
                var fieldDeclaration = (VariableDeclaratorSyntax) this._field.GetPrimaryDeclaration().AssertNotNull();

                if ( fieldDeclaration.Initializer != null )
                {
                    this.InitializerSyntax = fieldDeclaration.Initializer;
                }
            }

            // TODO: Attributes etc.
        }

        public override InsertPosition InsertPosition => this._field.ToInsertPosition();

        public override bool IsDesignTime => false;

        protected override bool HasBaseInvoker => true;
    }
}