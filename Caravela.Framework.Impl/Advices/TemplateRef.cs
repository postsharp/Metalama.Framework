// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

        public TemplateSelectionKind SelectedKind { get; }

        public TemplateSelectionKind InterpretedKind { get; }

        public bool IsNull => this.SelectedKind == TemplateSelectionKind.None;

        public TemplateRef( in AspectClassMember template, TemplateSelectionKind selectedKind ) : this( template, selectedKind, selectedKind ) { }

        private TemplateRef( in AspectClassMember template, TemplateSelectionKind selectedKind, TemplateSelectionKind interpretedKind )
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
            var symbol = type.GetMembers( this.TemplateMember.Name ).Single( m => classifier.GetTemplateMemberKind( m ) != TemplateAttributeKind.None );

            var declaration = compilation.Factory.GetDeclaration( symbol );

            if ( declaration is not T typedSymbol )
            {
                throw new InvalidOperationException(
                    $"The template '{symbol}' is a {declaration.DeclarationKind} but it was expected to be an {typeof(T).Name}" );
            }

            return new Template<T>( typedSymbol, this.SelectedKind, this.InterpretedKind );
        }

        public TemplateRef InterpretedAs( TemplateSelectionKind interpretedKind ) => new( this.TemplateMember, this.SelectedKind, interpretedKind );

        public override string ToString() => this.IsNull ? "null" : $"{this.TemplateMember.Name}:{this.SelectedKind}";
    }
}