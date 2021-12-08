// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Project;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
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

        public TemplateMember<T> GetTemplateMember<T>( CompilationModel compilation, IServiceProvider serviceProvider )
            where T : class, IMemberOrNamedType
        {
            if ( this.IsNull )
            {
                return default;
            }

            var classifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( compilation.RoslynCompilation );

            var type = compilation.RoslynCompilation.GetTypeByMetadataNameSafe( this.TemplateMember.TemplateClass.FullName );
            var symbol = type.GetMembers( this.TemplateMember.Name ).Single( m => !classifier.GetTemplateInfo( m ).IsNone );

            var declaration = compilation.Factory.GetDeclaration( symbol );

            if ( declaration is not T typedSymbol )
            {
                throw new InvalidOperationException(
                    $"The template '{symbol}' is a {declaration.DeclarationKind} but it was expected to be an {typeof(T).Name}" );
            }

            return Advices.TemplateMember.Create( typedSymbol, this.TemplateMember.TemplateInfo, this.SelectedKind, this.InterpretedKind );
        }

        public TemplateMemberRef InterpretedAs( TemplateKind interpretedKind ) => new( this.TemplateMember, this.SelectedKind, interpretedKind );

        public override string ToString() => this.IsNull ? "null" : $"{this.TemplateMember.Name}:{this.SelectedKind}";
    }
}