namespace Caravela.Framework.Impl.DesignTime
{
    internal enum DesignTimeAspectPipelineStatus
    {
        /// <summary>
        /// The pipeline has never been successfully initialized.
        /// </summary>
        Default,

        /// <summary>
        /// The pipeline has a working configuration.
        /// </summary>
        Ready,

        /// <summary>
        /// The pipeline configuration is outdated. A project build is required.
        /// </summary>
        NeedsExternalBuild
    }
}