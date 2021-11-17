namespace Audit.Service.Lambda
{
    /// <summary>
    /// CRUD actions.
    /// </summary>
    public enum ActionType
    {
        None,
        Read,
        ReadAll,
        Update,
        Delete,
        Create
    }

    /// <summary>
    /// The type of entity the request is working with.
    /// </summary>
    public enum EntityType
    {
        None,
        Audit,
        Health
    }

    /// <summary>
    /// Requests can be made via HTTP or from async messages. The important information about the request
    /// is defined by this class.
    /// </summary>
    public class RequestWrapper
    {
        public ActionType ActionType { get; set; }
        public EntityType EntityType { get; set; }
        public string Entity { get; set; }
        public int Id { get; set; }
        public string Tenant { get; set; }
    }
}