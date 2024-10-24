namespace StorageQueue.API
{
    public class MessageQueueReader : BackgroundService
    {
        private readonly ILogger<MessageQueueReader> _logger;

        public MessageQueueReader(ILogger<MessageQueueReader> logger)
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
