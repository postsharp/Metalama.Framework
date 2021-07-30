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
        public string Name { get; }

        public AspectClass ImplementingClass { get; }

        public TemplateKind Kind { get; }

        public bool IsNull => this.Kind == TemplateKind.None;

        public TemplateRef( string name, AspectClass implementingClass, TemplateKind kind )
        {
            this.Name = name;
            this.ImplementingClass = implementingClass;
            this.Kind = kind;
        }

        public Template<T> GetTemplate<T>( CompilationModel compilation, IServiceProvider serviceProvider )
            where T : IMemberOrNamedType
        {
            if ( this.IsNull )
            {
                return default;
            }

            var classifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( compilation.RoslynCompilation );

            var type = compilation.RoslynCompilation.GetTypeByMetadataNameSafe( this.ImplementingClass.FullName );
            var symbol = type.GetMembers( this.Name ).Single( m => classifier.GetTemplateMemberKind( m ) != TemplateMemberKind.None );

            return new Template<T>( (T) compilation.Factory.GetDeclaration( symbol ), this.Kind );
        }

        public override string ToString() => this.IsNull ? "null" : $"{this.Name}:{this.Kind}";
    }
}