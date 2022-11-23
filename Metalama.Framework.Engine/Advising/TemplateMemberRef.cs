// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal readonly struct TemplateMemberRef
    {
        public TemplateClassMember TemplateMember { get; }

        public TemplateKind SelectedKind { get; }

        public TemplateKind InterpretedKind { get; }

        public bool IsNull => this.SelectedKind == TemplateKind.None;

        public TemplateMemberRef( in TemplateClassMember template, TemplateKind selectedKind ) : this( template, selectedKind, selectedKind ) { }

        private TemplateMemberRef( in TemplateClassMember template, TemplateKind selectedKind, TemplateKind interpretedKind )
        {
            this.TemplateMember = template;
            this.SelectedKind = selectedKind;
            this.InterpretedKind = interpretedKind;
        }

        public TemplateMember<T> GetTemplateMember<T>( CompilationModel compilation, ProjectServiceProvider serviceProvider )
            where T : class, IMemberOrNamedType
        {
            if ( this.IsNull )
            {
                throw new InvalidOperationException();
            }

            var classifier = compilation.CompilationServices.SymbolClassifier;

            var type = compilation.RoslynCompilation.GetTypeByMetadataNameSafe( this.TemplateMember.TemplateClass.FullName );
            var symbol = type.GetMembers( this.TemplateMember.Name ).Single( m => !classifier.GetTemplateInfo( m ).IsNone );

            var declaration = compilation.Factory.GetDeclaration( symbol );

            if ( declaration is not T typedSymbol )
            {
                throw new InvalidOperationException(
                    $"The template '{symbol}' is a {declaration.DeclarationKind} but it was expected to be an {typeof(T).Name}" );
            }

            // Create the attribute instance.
            IAdviceAttribute? attribute;

            if ( this.TemplateMember.TemplateInfo.Attribute != null )
            {
                // If we have a system attribute, return it.

                attribute = this.TemplateMember.TemplateInfo.Attribute;
            }
            else
            {
                if ( !serviceProvider.GetRequiredService<TemplateAttributeFactory>()
                        .TryGetTemplateAttribute( this.TemplateMember.TemplateInfo.SymbolId, NullDiagnosticAdder.Instance, out attribute ) )
                {
                    throw new AssertionFailedException( $"Cannot instantiate the template attribute for '{symbol.ToDisplayString()}'" );
                }
            }

            if ( attribute is ITemplateAttribute templateAttribute )
            {
                return TemplateMemberFactory.Create( typedSymbol, this.TemplateMember, templateAttribute, this.SelectedKind, this.InterpretedKind );
            }
            else
            {
                throw new AssertionFailedException( $"The attribute '{attribute.GetType().FullName}' does not implement ITemplateAttribute." );
            }
        }

        public TemplateMemberRef InterpretedAs( TemplateKind interpretedKind ) => new( this.TemplateMember, this.SelectedKind, interpretedKind );

        public override string ToString() => this.IsNull ? "null" : $"{this.TemplateMember.Name}:{this.SelectedKind}";
    }
}