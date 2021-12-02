namespace Audit.Service
{
    /// <summary>
    /// Constant values used throughout the code.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The name of the service.
        /// </summary>
        public static readonly string ServiceName = "AuditsService";

        /// <summary>
        /// The name of the default data partition.
        /// </summary>
        public static readonly string DefaultPartition = "main";

        /// <summary>
        /// The name of the "Accept" header.
        /// </summary>
        public static readonly string AcceptHeader = "accept";

        /// <summary>
        /// The name of the filter query string parameter.
        /// </summary>
        public static readonly string FilterQuery = "filter";

        /// <summary>
        /// The name of the field in the "Accept" header that defines the data partition.
        /// </summary>
        public static readonly string AcceptPartitionInfo = "dataPartition";

        /// <summary>
        /// The MySQL database timeout.
        /// </summary>
        public static readonly int DefaultMySqlTimeout = 180;

        /// <summary>
        /// The MySQL server version.
        /// </summary>
        public static readonly string MySqlVersion = "8.0";

        /// <summary>
        /// The JSONAPI MIME type.
        /// </summary>
        public static readonly string JsonApiMimeType = "application/vnd.api+json";
    }
}