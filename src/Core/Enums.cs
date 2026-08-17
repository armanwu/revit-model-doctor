namespace ModelDoctor.Core
{
    /// <summary>
    /// Represents the evaluation status of a health check rule.
    /// </summary>
    public enum HealthStatus
    {
        /// <summary>
        /// Rule passed with no issues detected.
        /// </summary>
        Pass,

        /// <summary>
        /// Non-critical issues or warnings detected.
        /// </summary>
        Warning,

        /// <summary>
        /// Critical health threshold exceeded or rule failure.
        /// </summary>
        Fail
    }
}
