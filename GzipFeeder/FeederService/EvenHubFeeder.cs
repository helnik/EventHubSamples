using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace GzipFeeder.FeederService
{
    public class EvenHubFeeder : IFeed
    {
        private EventHubProducerClient _producerClient;

        public EvenHubFeeder(IConfiguration configuration)
        {
            EventHubProducerClientOptions producerOptions = new EventHubProducerClientOptions
            {
                RetryOptions = new EventHubsRetryOptions
                {
                    Mode = EventHubsRetryMode.Exponential,
                    MaximumDelay = TimeSpan.FromMilliseconds(double.Parse(configuration["MaximumDelayInMs"])),
                    MaximumRetries = int.Parse(configuration["MaximumRetries"])
                },
                ConnectionOptions = new EventHubConnectionOptions
                {
                    Proxy = string.IsNullOrWhiteSpace(configuration["ProxyAddress"]) ? null : new WebProxy
                    {
                        Address = new Uri(configuration["ProxyAddress"])
                    }
                }
            };
            _producerClient = new EventHubProducerClient(configuration["EvhConnectionString"], configuration["EvhName"], producerOptions);
        }

        public async Task FeedAsync<T>(List<T> payload, string tableName, string mappingName)
        {
            var eventBatch = await _producerClient.CreateBatchAsync();

            foreach (var change in payload)
            {
                EventData eventData = CreateEventDataFromChange(change, tableName, mappingName);

                //try add to the batch. If the batch cannot take up more then send 
                if (!eventBatch.TryAdd(eventData))
                {
                    if (eventBatch.Count == 0) //if there are no other events this change is too large no way to ever send it this way
                    {
                        continue;
                    }

                    await TrySendBatch(eventBatch);
                    eventBatch = await _producerClient.CreateBatchAsync(); //we just send the batch so it has been closed, recreate it

                    if (!eventBatch.TryAdd(eventData) && eventBatch.Count == 0) //add current change to batch since it was not send and check if this item is too big
                    {
                        continue; //if it is too big we cannot send it anyway. Have a fallback mechanism here
                    }
                }
            }
            //send all or the whatever left
            if (eventBatch != null)
            {
                await TrySendBatch(eventBatch);
            }

        }

        private static EventData CreateEventDataFromChange<T>(T change, string tableName, string mappingName)
        {
            string serializedChange = JsonConvert.SerializeObject(change);
            byte[] compressedPayloadBytes = CompressJsonData(serializedChange);
            var eventData = new EventData(compressedPayloadBytes);
            
            //properties required to route the data to ADX
            eventData.Properties.Add("Table", tableName);
            eventData.Properties.Add("Format", "JSON");
            eventData.Properties.Add("IngestionMappingReference", mappingName);
            return eventData;
        }

        private static byte[] CompressJsonData(string jsonData)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonData);

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }
                return memoryStream.ToArray();
            }
        }

        private async Task TrySendBatch(EventDataBatch eventBatch)
        {
            try
            {
                await _producerClient.SendAsync(eventBatch);
            }
            catch (Exception ex)
            {
                //do error handling and fallback here
            }
            finally
            {
                eventBatch.Dispose();
            }
        }


    }
}
