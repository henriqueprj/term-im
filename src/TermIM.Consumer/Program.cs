using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TermIM.Consumer
{
    class Program
    {
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
            
            var factory = new ConnectionFactory { HostName = "10.0.16.31", VirtualHost = "lab", UserName = "lab", Password = "bal" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Garante que o Exchange exista
				channel.ExchangeDeclare(
                    exchange: "chat", 
                    type: ExchangeType.Topic);

                var groupName = "desenvolvedores";
                var routingKey = $"chat.group.{groupName}";

                // Cria uma nova fila não durável, auto-delete, com nome randômico
                var queueName = channel.QueueDeclare().QueueName;

                // faz o Binding Entre o Exchange e a Fila
                channel.QueueBind(queueName, "chat", routingKey);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) => {

                    var body = e.Body;
                    var jsonMessage = Encoding.UTF8.GetString(body);
                    var message = JsonConvert.DeserializeObject<TermIM.Contracts.Message>(jsonMessage);

                    Console.WriteLine($"{message.Sender} ({message.Timestamp.ToLocalTime():dd/MM/yyyy HH:mm:ss}):");
                    Console.WriteLine(message.Content);
                    Console.WriteLine();
                };

                channel.BasicConsume(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer
                );
                
                
                Console.WriteLine("Pressione qualquer tecla para sair");
                Console.ReadKey();
            }
        }
        
        private static void PrintWelcome()
        {
            Console.WriteLine("Bem-vindo ao Leitor TermIM (v1.0)");
            Console.WriteLine();
        }

        private static string ReadUsername()
        {
            Console.Write("Informe seu username: ");
            return Console.ReadLine();
        }

        private static bool IsUsernameValid(string username)
        {
            var isMatch = Regex.IsMatch(username, "^[a-zA-Z]+\\w*$");
            return !string.IsNullOrWhiteSpace(username) || isMatch;
        }
    }
}
