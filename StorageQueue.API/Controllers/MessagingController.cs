using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using System.ComponentModel;
using System.Net;

namespace StorageQueue.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController : ControllerBase
    {
        private const string _queueName = "episode-queue";

        private readonly QueueClient _int2QueueClient;
        private readonly QueueClient _dev2QueueClient;
        public MessagingController(IAzureClientFactory<QueueServiceClient> clientFactory)
        {
            _int2QueueClient = clientFactory.CreateClient("int2").GetQueueClient(_queueName);
            _dev2QueueClient = clientFactory.CreateClient("dev2").GetQueueClient(_queueName);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] int numberOfMessages = 10, CancellationToken cancellationToken = default)
        {

            try
            {
                var messagesInInt2 = await GetMessagesAsync(_int2QueueClient, numberOfMessages, cancellationToken);
                var curatedMessages = messagesInInt2.DistinctBy(c => c.MessageId); 

                var sentMessages = messagesInInt2.Select(message => SendMessageAsync(message.MessageText, cancellationToken));
                
                var result = await Task.WhenAll(sentMessages);
               
                return Ok($"Messages sent to {nameof(_dev2QueueClient)} queue: {result.Length}");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<List<PeekedMessage>> GetMessagesAsync(QueueClient client, int numberOfMessages, CancellationToken cancellationToken = default)
        {
            var responseMessages = new List<PeekedMessage>();
            int maxMessages = numberOfMessages < 32 ? numberOfMessages : 32;
            bool continueReading = true;
            int times = maxMessages < 32 ? 1 : numberOfMessages / maxMessages;
            int residual = numberOfMessages % maxMessages;

            if (await client?.ExistsAsync(cancellationToken))
            {
                int counter = 0;
                while (continueReading) 
                { 
                    counter++;
                    int messagesToPeek = maxMessages;
                    if (counter > times)
                        messagesToPeek = residual;

                    var messages = await client.PeekMessagesAsync(messagesToPeek, cancellationToken: cancellationToken);
                    foreach (var message in messages.Value)
                    {
                        responseMessages.Add(message);
                    }

                    if (counter == times)
                        continueReading = false;
                    ////Means no more messages in queue, should finish reading.
                    //client can only dequeue 32 messages at the time
                    //if (numberOfMessages < 32 || messages.Value.Length < maxMessages || responseMessages.Count >= numberOfMessages)
                    //    shouldContinueReading = false;
                }
                if(residual > 0)
                {
                    var messages = await client.PeekMessagesAsync(residual, cancellationToken: cancellationToken);
                    foreach (var message in messages.Value)
                    {
                        responseMessages.Add(message);
                    }
                }
            }

            return responseMessages;
        }

        private async Task<Response<SendReceipt>?> SendMessageAsync(string messageText, CancellationToken cancellationToken)
        {
            var response = await _dev2QueueClient.SendMessageAsync(messageText, TimeSpan.FromSeconds(1), TimeSpan.FromDays(7), cancellationToken: cancellationToken);
            if(response.GetRawResponse().Status == (int)HttpStatusCode.Created)
            {
                return response;
            }

            return null;
        }
    }
}
