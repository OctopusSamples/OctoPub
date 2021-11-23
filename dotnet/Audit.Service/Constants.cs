namespace Audit.Service
{
    public static class Constants
    {
        public static readonly string AppName = "audits";
        public static readonly string DefaultPartition = "main";
        public static readonly string AcceptHeader = "accept";
        public static readonly string AcceptVersionInfo = "audits-version";
        public static readonly string AcceptPartitionInfo = "dataPartition";
        public static readonly int DefaultMySqlTimeout = 180;
        public static readonly string MySqlVersion = "8.0";
    }
}