namespace Caravela.Framework.Diagnostics
{
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// Something that is an issue, but is not surfaced through normal means.
        /// There may be different mechanisms that act on these issues.
        /// </summary>
        Hidden = 0,
        
        /// <summary>
        /// Information that does not indicate a problem (i.e. not prescriptive).
        /// </summary>
        Info = 1,
        
        /// <summary>
        /// Something suspicious but allowed.
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// Something not allowed by the rules of the aspect.
        /// </summary>
        Error = 3
        
    }
}