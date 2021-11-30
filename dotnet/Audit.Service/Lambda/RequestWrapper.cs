namespace Audit.Service.Lambda
{
    /// <summary>
    ///     CRUD actions.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// The default action that indicates no other value was provided.
        /// </summary>
        None,

        /// <summary>
        /// Read a single resource.
        /// </summary>
        Read,

        /// <summary>
        /// Read a collection of resources.
        /// </summary>
        ReadAll,

        /// <summary>
        /// Update a single resource.
        /// </summary>
        Update,

        /// <summary>
        /// Delete a single resource.
        /// </summary>
        Delete,

        /// <summary>
        /// Create a single resource.
        /// </summary>
        Create
    }

    /// <summary>
    ///     The type of entity the request is working with.
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// The default value that indicates no other valid value was provided.
        /// </summary>
        None,

        /// <summary>
        /// Indicates a request is for Audit records.
        /// </summary>
        Audit,

        /// <summary>
        /// Indicates a request is for Health information.
        /// </summary>
        Health
    }

    /// <summary>
    ///     Requests can be made via HTTP or from async messages. The important information about the request
    ///     is defined by this class.
    /// </summary>
    public class RequestWrapper
    {
        /// <summary>
        /// Gets or sets the action that is associated with this request.
        /// </summary>
        public ActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the kind of entity this request is for.
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the content when creating or updating resources.
        /// </summary>
        public string Entity { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of a resource when updating, deleting, or reading a single resource.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the data partition that the request was made for.
        /// </summary>
        public string DataPartition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the query associated with reading a collection of resources.
        /// </summary>
        public string Filter { get; set; } = string.Empty;
    }
}