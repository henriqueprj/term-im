using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;

namespace TermIM.Producer
{
    class Program
    {
        private static readonly JsonSerializerSettings CamelCaseSerializerSettings =
            new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()};
        
        static void Main(string[] args)
        {
            PrintWelcome();
            
            var username = ReadUsername();
            
            if (!IsUsernameValid(username))
            {
                Console.WriteLine("Username inválido!");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Você entrou na sala 'Desenvolvimento'.");
            Console.WriteLine();

            var factory = new ConnectionFactory
            {
                HostName = "10.0.16.31", 
                VirtualHost = "lab", 
                UserName = "lab", 
                Password = "bal"
            };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "chat", type: ExchangeType.Topic);

                var groupName = "desenvolvedores";
                var routingKey = $"chat.group.{groupName}";

                string content;

                Console.WriteLine();
                Console.Write($"{username}: ");
                while ((content = Console.ReadLine()) != ":q!")
                {
					var msg = CreateMessage(username, content);
					var body = System.Text.Encoding.UTF8.GetBytes(msg);

                    var props = channel.CreateBasicProperties();
                    props.ContentType = "application/json";
                    props.DeliveryMode = 2; /* persistente */
                    
					channel.BasicPublish(
						exchange: "chat",
						routingKey: routingKey,
						basicProperties: props,
						body: body
					);

                    Console.WriteLine();
                    Console.Write($"{username}: ");
                }

                Console.WriteLine("Goodbye!");
            }


        }

        private static void PrintWelcome()
        {
            Console.WriteLine("Bem-vindo ao TermIM (v1.0)");
            Console.WriteLine();
        }

        private static string ReadUsername()
        {
            Console.Write("Informe seu username: ");
            return Console.ReadLine();
        }

        private static bool IsUsernameValid(string username)
        {
            return !string.IsNullOrWhiteSpace(username) || Regex.IsMatch(username, "^[a-zA-Z]+\\w*$");
        }

        private static string CreateMessage(string sender, string message)
        {
            var msg = new TermIM.Contracts.Message { Timestamp = DateTime.UtcNow, Sender = sender, Content = message };
            var json = JsonConvert.SerializeObject(msg, CamelCaseSerializerSettings);
            return json;

            //return $"{{\"timestamp\":\"{DateTime.UtcNow:O}\",\"sender\":\"{sender}\",\"message\":\"{message}\"}}";
        }
    }
}
