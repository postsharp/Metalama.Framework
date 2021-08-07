// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal readonly struct TemplateRef
    {
        public AspectClassMember TemplateMember { get; }

        public TemplateKind SelectedKind { get; }

        public TemplateKind InterpretedKind { get; }

        public bool IsNull => this.SelectedKind == TemplateKind.None;

        public TemplateRef( in AspectClassMember template, TemplateKind selectedKind ) : this( template, selectedKind, selectedKind ) { }

        private TemplateRef( in AspectClassMember template, TemplateKind selectedKind, TemplateKind interpretedKind )
        {
            this.TemplateMember = template;
            this.SelectedKind = selectedKind;
            this.InterpretedKind = interpretedKind;
        }

        public Template<T> GetTemplate<T>( CompilationModel compilation, IServiceProvider serviceProvider )
            where T : class, IMemberOrNamedType
        {
            if ( this.IsNull )
            {
                return default;
            }

            var classifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( compilation.RoslynCompilation );

            var type = compilation.RoslynCompilation.GetTypeByMetadataNameSafe( this.TemplateMember.AspectClass.FullName );
            var symbol = type.GetMembers( this.TemplateMember.Name ).Single( m => !classifier.GetTemplateInfo( m ).IsNone );

            var declaration = compilation.Factory.GetDeclaration( symbol );

            if ( declaration is not T typedSymbol )
            {
                throw new InvalidOperationException(
                    $"The template '{symbol}' is a {declaration.DeclarationKind} but it was expected to be an {typeof(T).Name}" );
            }

            return Template.Create( typedSymbol, this.TemplateMember.TemplateInfo, this.SelectedKind, this.InterpretedKind );
        }

        public TemplateRef InterpretedAs( TemplateKind interpretedKind ) => new( this.TemplateMember, this.SelectedKind, interpretedKind );

        public override string ToString() => this.IsNull ? "null" : $"{this.TemplateMember.Name}:{this.SelectedKind}";
    }
}