using CommandLine;

namespace Audit.Service
{
    /// <summary>
    /// The modes that the app can operate in.
    /// </summary>
    internal enum AppMode
    {
        /// <summary>
        /// Web mode is a self hosted web service.
        /// </summary>
        Web,

        /// <summary>
        /// Sqs mode listens to SQS events.
        /// </summary>
        Sqs
    }

    /// <summary>
    /// The command line options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets the mode in which the app will operate.
        /// </summary>
        [Option('m', "mode", Required = false,
            HelpText = "Sets the mode (web app or event listener) that the app starts in.")]
        public string Mode { get; set; } = AppMode.Web.ToString();

        /// <summary>
        /// Gets or sets the mode in which the app will operate.
        /// </summary>
        [Option('q', "sqs-queue", Required = false,
            HelpText = "Defines the SQS queue URL to listen to.")]
        public string SqsQueue { get; set; } = string.Empty;
    }
}