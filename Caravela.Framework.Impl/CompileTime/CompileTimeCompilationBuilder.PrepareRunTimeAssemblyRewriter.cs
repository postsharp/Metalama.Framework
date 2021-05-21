
            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

                // Special case: aspect weavers and other aspect drivers are preserved in the runtime assembly.
                // This only happens if regular Caravela.Framework is referenced from the weaver project, which generally shouldn't happen.
                // But it is a pattern used by Caravela.Samples for try.postsharp.net.
                if ( this._aspectDriverSymbol != null && symbol.AllInterfaces.Any( i => SymbolEqualityComparer.Default.Equals( i, this._aspectDriverSymbol ) ) )
                {
                    return node;
                }

                return base.VisitClassDeclaration( node );
            }

            public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) == SymbolDeclarationScope.CompileTimeOnly )
                {
                    return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
                }

                return node;
            }

            public override SyntaxNode? VisitIndexerDeclaration( IndexerDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) is SymbolDeclarationScope.CompileTimeOnly )
                {
                    return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
                }

                return node;
            }

            public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                // Properties can be in following forms:
                //  * Accessors with implicit bodies and backing field:         int Foo { get; set; }
                //  * Accessors with explicit bodies:                           int Foo { get { ... } set { ... } }
                //  * Accessors without bodies (abstract):                      abstract int Foo { get; set; }
                //  * Expression body:                                          int Foo => 42;
                //  * Accessors and initializer and backing field:              int Foo { get; } = 42;

                if ( this.GetSymbolDeclarationScope( node ) is SymbolDeclarationScope.CompileTimeOnly )
                {
                    if ( node.Modifiers.All( x => x.Kind() != SyntaxKind.AbstractKeyword )
                         && node.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true )
                    {
                        // This is auto property - we keep it as it is (otherwise we lose the initial value and the fact that it is an auto property).
                        return node;
                    }

                    return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
                }

                return node;
            }
        }
    }
}