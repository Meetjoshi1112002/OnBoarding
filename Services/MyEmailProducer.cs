using Confluent.Kafka;
using System.Text.Json;
using OnBoarding.Models.POCOs;

namespace OnBoarding.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private const string Topic = "email-notify";

        public KafkaProducerService()
        {
            // Load Kafka credentials from environment variables
            var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
            var saslUsername = Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME");
            var saslPassword = Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD");

            if (string.IsNullOrEmpty(bootstrapServers) || string.IsNullOrEmpty(saslUsername) || string.IsNullOrEmpty(saslPassword))
            {
                throw new InvalidOperationException("Kafka environment variables are not set.");
            }

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = saslUsername,
                SaslPassword = saslPassword,
                SslCaLocation = "probe",
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5,
                MessageSendMaxRetries = 2,
                RetryBackoffMs = 1000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendMessageAsync(EmailBody emailBody)
        {
            string messageValue = JsonSerializer.Serialize(emailBody);
            try
            {
                var result = await _producer.ProduceAsync(Topic, new Message<string, string>
                {
                    Key = emailBody.Email,
                    Value = messageValue
                });

                Console.WriteLine($"Message sent to {result.TopicPartitionOffset}");
            }
            catch (ProduceException<string, string> e)
            {
                Console.WriteLine($"Error sending message: {e.Error.Reason}");
            }
            finally
            {
                _producer.Flush(TimeSpan.FromSeconds(10));
            }
        }
    }
}
