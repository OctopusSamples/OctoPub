﻿namespace Audit.Service.Lambda
{
    public enum ActionType
    {
        Read,
        Update,
        Delete,
        Create
    }

    public enum EntityType
    {
        Individual,
        Collection,
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