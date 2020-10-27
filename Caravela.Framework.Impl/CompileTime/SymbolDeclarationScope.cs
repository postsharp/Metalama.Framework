namespace Caravela.Framework.Impl.CompileTime
{
    public enum SymbolDeclarationScope
    {
        // The symbol can be used both at build time or at run time.
        // The node has not been classified as necessarily build-time or run-time.
        Default,
        
        // The symbol can be only used at run time only.
        // The node must be evaluated at run-time, but its children can be build-time expressions.
        RunTimeOnly,
        
        // The symbol can be used only at build time.
        // The node including all children nodes must be evaluated at build time.
        CompileTimeOnly
    }
}