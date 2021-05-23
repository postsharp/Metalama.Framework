namespace Caravela.Framework.Project
{
    /// <summary>
    /// Base interface for project configuration objects, which are a way for compile-time libraries
    /// to expose configuration objects that can be configured in a <see cref="IProjectPolicy"/>. 
    /// Implementations must implement the Freezable pattern.
    /// </summary>
    public interface IProjectExtension
    {
        /// <summary>
        /// Initializes the object from project properties.
        /// </summary>
        /// <param name="project"></param>
        void Initialize( IProject project );

        /// <summary>
        /// Prevents further modifications of the current object and all its children objects. This method is called after
        /// all project policies have been built.
        /// </summary>
        void Freeze();
    }
}