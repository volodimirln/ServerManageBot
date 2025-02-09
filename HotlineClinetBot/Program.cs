using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Net.NetworkInformation;
using HotlineManageBot.Modules.Networks;
using System.Net.Sockets;
using System.Text;

public class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    private static bool start = false;
    static async Task Main()
    {
         var udpServer = new UdpClient(9);
        Console.WriteLine("Ожидание UDP-каманды для запуска...");
        while (true)
        {
            var result = await udpServer.ReceiveAsync();
            if (result.Buffer.Length > 0)
            {
                Console.WriteLine("Запуск бота");
                Thread.Sleep(18000);
                start = true;
                break;
            }
        }
        if (start) {
            _botClient = new TelegramBotClient(System.IO.File.ReadAllText("Data/token.txt"));
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                UpdateType.Message,
            },
                ThrowPendingUpdates = true,
            };
            using var cts = new CancellationTokenSource();

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            var me = await _botClient.GetMeAsync();
            Console.Title = "Hotline Manage TelegramBot";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(System.IO.File.ReadAllText("Data/title.txt"));
            Console.WriteLine("Журнал записи событий запущен:");
            Console.WriteLine($"{DateTime.Now.ToString("dd.mm.yyyy HH:MM")}: {me.FirstName} запущен");
            await Program.GetShutdown();
            await Task.Delay(-1);
        }
    }

    private static async Task GetShutdown()
    {
        using var udpServer = new UdpClient(7);

        while (true)
        {
            var result = await udpServer.ReceiveAsync();
            if (result.Buffer.Length > 0)
            {
                var message = Encoding.UTF8.GetString(result.Buffer);
                System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
                Environment.Exit(0);
            }
        }
    }
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}", _ => error.ToString()
        };
        Console.WriteLine(ErrorMessage);
        System.Environment.Exit(1);
        return Task.CompletedTask;
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message;
                    var user = message.From;

                    Console.WriteLine($"{DateTime.Now.ToString("dd.mm.yyyy HH:MM")} {user.FirstName} ({user.Id}) написал сообщение: {message.Text}");

                    var chat = message.Chat;
                    long userChat = message.Chat.Id;
                    switch (message.Type)
                    {
                        case MessageType.Text:
                        {
                            if (System.IO.File.Exists("MessagesData"+message.Text+".txt"))
                            {
                                await botClient.SendTextMessageAsync(chat.Id, System.IO.File.ReadAllText("MessagesData"+message.Text+".txt"), parseMode: ParseMode.Markdown);
                                return;
                            }
                            else if (message.Text == "/wakeupserver")
                            {
                                new WOLService().SendWakeOnLan(PhysicalAddress.Parse(System.IO.File.ReadAllText("Data/mac.txt")), 7, System.IO.File.ReadAllText("Data/address.txt"));
                                await botClient.SendTextMessageAsync(chat.Id, System.IO.File.ReadAllText("MessagesData/startserver.txt"), parseMode: ParseMode.Markdown);
                                System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
                                Environment.Exit(0);
                                return;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chat.Id, System.IO.File.ReadAllText("MessagesData/error.txt"), parseMode: ParseMode.Markdown);
                                return;
                            }
                        }
                    default:
                        {
                            await botClient.SendTextMessageAsync(chat.Id, "Используйте только текст!");
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}