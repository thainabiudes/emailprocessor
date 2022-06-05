using EmailProcessor.Messages;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace EmailProcessor.MessageConsumer
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private const string APIKey = "API KEY";
        private const string BaseUri = "https://api.mailgun.net/v3";
        private const string Domain = "API - DOMAIN";
        private const string SenderAddress = "thainabiudes1@gmail.com";
        private const string SenderDisplayName = "Faturas Online - Thainá Biudes Development";
        private const string Tag = "sampleTag";

        public RabbitMQCheckoutConsumer()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "emailqueue", false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                MessageVO vo = System.Text.Json.JsonSerializer.Deserialize<MessageVO>(content);
                ProcessEmail(vo).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume("emailqueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task<IRestResponse> ProcessEmail(MessageVO vo)
        {
            string body = $"<html><head><title>Fatura</title></head><body style='margin: 0px'><h1>Sua fatura chegou! {vo.invoice.customer.Name}</h1> <br/> <h2>Valor:{vo.invoice.Value}</h2> <br/> <h2>Data limite de pagamento:{vo.invoice.FinalDate}</body>";

            RestClient client = new RestClient();
            client.BaseUrl = new Uri(Domain + "/messages");
            client.Authenticator =
                new HttpBasicAuthenticator("api",
                    APIKey);

            RestRequest request = new RestRequest();
            request.AddParameter("domain", Domain, ParameterType.UrlSegment);
            request.AddParameter("from", $"{SenderDisplayName} <{SenderAddress}>");

            request.AddParameter("to", vo.invoice.customer.Email);
            request.AddParameter("subject", "Olá, chegou sua fatura!");
            request.AddParameter("html", body);
            request.AddParameter("o:tag", Tag);
            request.Method = Method.POST;

            var result = await client.ExecuteAsync(request);

            return result;
        }
    }
}
