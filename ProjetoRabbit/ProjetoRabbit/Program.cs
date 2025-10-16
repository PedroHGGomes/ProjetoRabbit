using System;
using System.Text;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Program
{
    const string FRUITS_EXCHANGE = "exchange.fruits";
    const string USERS_EXCHANGE = "exchange.users";
    const string VALID_FRUITS_EXCHANGE = "exchange.validated.fruits";
    const string VALID_USERS_EXCHANGE = "exchange.validated.users";

    const string FRUITS_QUEUE = "queue.fruit.validation";
    const string USERS_QUEUE = "queue.user.validation";
    const string VALID_FRUITS_QUEUE = "queue.fruit.received";
    const string VALID_USERS_QUEUE = "queue.user.received";

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.WriteLine("\n=== ProjetoRabbit (.NET 8 + RabbitMQ) ===");
            Console.WriteLine("[1] Enviar FRUTAS");
            Console.WriteLine("[2] Enviar USUÁRIOS");
            Console.WriteLine("[3] VALIDATION (verifica e repassa)");
            Console.WriteLine("[4] Receiver FRUTAS");
            Console.WriteLine("[5] Receiver USUÁRIOS");
            Console.WriteLine("[0] Sair");
            Console.Write("Escolha: ");

            string? opt = Console.ReadLine();

            if (opt == "0") break;

            switch (opt)
            {
                case "1": SenderFruits(); break;
                case "2": SenderUsers(); break;
                case "3": Validation(); break;
                case "4": ReceiverFruits(); break;
                case "5": ReceiverUsers(); break;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
    }

    static ConnectionFactory Factory() => new ConnectionFactory { HostName = "localhost" };

    static void SenderFruits()
    {
        using var conn = Factory().CreateConnection();
        using var ch = conn.CreateModel();

        ch.ExchangeDeclare(FRUITS_EXCHANGE, ExchangeType.Direct);
        ch.QueueDeclare(FRUITS_QUEUE, false, false, false);
        ch.QueueBind(FRUITS_QUEUE, FRUITS_EXCHANGE, "fruit.data");

        string[] fruits = { "Manga", "Uva", "Abacaxi", "Laranja" };

        foreach (var fruit in fruits)
        {
            string message = $"name={fruit};desc=Fruta testada;createdAt={DateTime.Now:O}";
            var body = Encoding.UTF8.GetBytes(message);
            ch.BasicPublish(FRUITS_EXCHANGE, "fruit.data", null, body);
            Console.WriteLine($"[Sender FRUITS] Enviado: {message}");
        }

        Console.WriteLine("Mensagens enviadas. Pressione ENTER...");
        Console.ReadLine();
    }

    static void SenderUsers()
    {
        using var conn = Factory().CreateConnection();
        using var ch = conn.CreateModel();

        ch.ExchangeDeclare(USERS_EXCHANGE, ExchangeType.Direct);
        ch.QueueDeclare(USERS_QUEUE, false, false, false);
        ch.QueueBind(USERS_QUEUE, USERS_EXCHANGE, "user.data");

        string[,] users = {
            {"Ana Souza", "Rua das Flores 123", "123.456.789-00"},
            {"Carlos Lima", "Av. Brasil 1000", "234.567.890-11"},
            {"Pedro Gomes", "Rua X 99", "345.678.901-22"}
        };

        for (int i = 0; i < users.GetLength(0); i++)
        {
            string msg = $"name={users[i, 0]};address={users[i, 1]};cpf={users[i, 2]};registeredAt={DateTime.Now:O}";
            var body = Encoding.UTF8.GetBytes(msg);
            ch.BasicPublish(USERS_EXCHANGE, "user.data", null, body);
            Console.WriteLine($"[Sender USERS] Enviado: {msg}");
        }

        Console.WriteLine("Mensagens enviadas. Pressione ENTER...");
        Console.ReadLine();
    }

    static void Validation()
    {
        using var conn = Factory().CreateConnection();
        using var ch = conn.CreateModel();

        ch.ExchangeDeclare(FRUITS_EXCHANGE, ExchangeType.Direct);
        ch.ExchangeDeclare(USERS_EXCHANGE, ExchangeType.Direct);
        ch.ExchangeDeclare(VALID_FRUITS_EXCHANGE, ExchangeType.Direct);
        ch.ExchangeDeclare(VALID_USERS_EXCHANGE, ExchangeType.Direct);

        ch.QueueDeclare(FRUITS_QUEUE, false, false, false);
        ch.QueueDeclare(USERS_QUEUE, false, false, false);

        var fruitConsumer = new EventingBasicConsumer(ch);
        fruitConsumer.Received += (s, e) =>
        {
            string msg = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"[Validation] FRUIT recebido: {msg}");

            if (msg.Contains("name=") && msg.Contains("desc="))
            {
                string validated = "[OK] " + msg;
                ch.BasicPublish(VALID_FRUITS_EXCHANGE, "fruit.validated", null, Encoding.UTF8.GetBytes(validated));
                Console.WriteLine("[Validation] FRUIT validado.");
            }
        };
        ch.BasicConsume(FRUITS_QUEUE, true, fruitConsumer);

        var userConsumer = new EventingBasicConsumer(ch);
        userConsumer.Received += (s, e) =>
        {
            string msg = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"[Validation] USER recebido: {msg}");

            string digits = Regex.Replace(msg, @"\D", "");
            if (digits.Length >= 11)
            {
                string validated = "[OK] " + msg;
                ch.BasicPublish(VALID_USERS_EXCHANGE, "user.validated", null, Encoding.UTF8.GetBytes(validated));
                Console.WriteLine("[Validation] USER validado.");
            }
        };
        ch.BasicConsume(USERS_QUEUE, true, userConsumer);

        Console.WriteLine("Validation aguardando mensagens... ENTER para sair");
        Console.ReadLine();
    }

    static void ReceiverFruits()
    {
        using var conn = Factory().CreateConnection();
        using var ch = conn.CreateModel();

        ch.ExchangeDeclare(VALID_FRUITS_EXCHANGE, ExchangeType.Direct);
        ch.QueueDeclare(VALID_FRUITS_QUEUE, false, false, false);
        ch.QueueBind(VALID_FRUITS_QUEUE, VALID_FRUITS_EXCHANGE, "fruit.validated");

        var consumer = new EventingBasicConsumer(ch);
        consumer.Received += (s, e) =>
        {
            string msg = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"[Receiver FRUITS] Recebido: {msg}");
        };
        ch.BasicConsume(VALID_FRUITS_QUEUE, true, consumer);

        Console.WriteLine("Receiver FRUITS ativo. ENTER para sair");
        Console.ReadLine();
    }

    static void ReceiverUsers()
    {
        using var conn = Factory().CreateConnection();
        using var ch = conn.CreateModel();

        ch.ExchangeDeclare(VALID_USERS_EXCHANGE, ExchangeType.Direct);
        ch.QueueDeclare(VALID_USERS_QUEUE, false, false, false);
        ch.QueueBind(VALID_USERS_QUEUE, VALID_USERS_EXCHANGE, "user.validated");

        var consumer = new EventingBasicConsumer(ch);
        consumer.Received += (s, e) =>
        {
            string msg = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"[Receiver USERS] Recebido: {msg}");
        };
        ch.BasicConsume(VALID_USERS_QUEUE, true, consumer);

        Console.WriteLine("Receiver USERS ativo. ENTER para sair");
        Console.ReadLine();
    }
}
