using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using HotlineManageBot.Modules.Networks;
using System.Net.NetworkInformation;
using HotlineManageBot.Modules.Scripts;
using HotlineManageBot.Modules.Auntification;
using Microsoft.Extensions.Configuration;
using HotlineManageBot.Modules.Data;

public class Program
{
    private static ITelegramBotClient _botClient;

    private static ReceiverOptions _receiverOptions;
    private static IConfigurationRoot config;
    static async Task Main()
    {

        var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);

        config = builder.Build();

        _botClient = new TelegramBotClient(config["TelegramToken"]);
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
        DateTime dateTime = DateTime.Now;
        Console.WriteLine($"{dateTime.ToString("dd.mm.yyyy HH:MM")}: {me.FirstName} запущен");
        Console.WriteLine($"{dateTime.ToString("dd.mm.yyyy HH:MM")}: Ключ для авторизации - {new AuthOptions().CreateJWT("kvant")}");
        WOLService wOLService = new WOLService();
        wOLService.SendWakeOnLan(PhysicalAddress.Parse(config["ClientMAC"]), 7);
        await Task.Delay(-1);
    }

 
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}", _ => error.ToString()
        };
        Console.WriteLine(ErrorMessage);
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
                    var chat = message.Chat;
                    long userChat = message.Chat.Id;
                    
                    Console.WriteLine($" {DateTime.Now.ToString("dd.mm.yyyy HH:MM")} {user.FirstName} ({user.Id}) написал сообщение: {message.Text}");

                    switch (message.Type)
                    {
                        case MessageType.Text:
                        {
                            if (message.Text == "/start")
                            {
                                await botClient.SendTextMessageAsync(chat.Id, System.IO.File.ReadAllText("MessagesData/start.txt"), parseMode: ParseMode.Markdown);
                                return;
                            }
                            else if (message.Text == "/shutdown")
                            {
                                new WOLService().SendWakeOnLan(PhysicalAddress.Parse(config["ClientMAC"]), 9);
                                await botClient.SendTextMessageAsync(chat.Id, "Сервер отключен!");
                                System.Diagnostics.Process.Start("CMD.exe", "/C shutdown /s");
                                return;
                            }
                            else if (message.Text == "/restart" && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                await botClient.SendTextMessageAsync(chat.Id, "Сервер перезагружен!");
                                System.Diagnostics.Process.Start("CMD.exe", "/C shutdown /r");
                                return;
                            }
                            else if (message.Text.Contains("/block") && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                await botClient.SendTextMessageAsync(chat.Id, "Учетка пользователя "+message.Text.Split()[1]+" заблокирована!");
                                System.Diagnostics.Process.Start("CMD.exe", $"/C dsmod.exe user \"CN={message.Text.Split()[1]},OU={config["OU"]},{config["DomainPath"]}\" -disabled yes");
                                return;
                            }
                            else if (message.Text.Contains("/unblock") && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                await botClient.SendTextMessageAsync(chat.Id, "Учетка пользователя " + message.Text.Split()[1] + " разблокирована!");
                                System.Diagnostics.Process.Start("CMD.exe", $"/C dsmod.exe user \"CN={message.Text.Split()[1]}OU={config["OU"]},{config["DomainPath"]}\" -disabled no");
                                return;
                            }
                            else if (message.Text.Contains("/addit") && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                new PowerShellHM().RunScript($"$secureString = convertto-securestring \"@k55555Rte%\" -asplaintext -force;New-ADUser -Name \"{message.Text.Split()[1]}\" -Path \"OU={config["OU"]},{config["DomainPath"]}\" -AccountPassword $secureString -ChangePasswordAtLogon $false -Enabled $true");
                                new PowerShellHM().RunScript($"Add-ADGroupMember -Identity \"IT\" -Members \"{message.Text.Split()[1]}\"");
                                await botClient.SendTextMessageAsync(chat.Id, "Учетка пользователя " + message.Text.Split()[1] + " создана в группе пользователей IT c паролем @k55555$");
                                return;
                            }
                            else if (message.Text.Contains("/addeng") && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                new PowerShellHM().RunScript($"$secureString = convertto-securestring \"@k22333Rte%\" -asplaintext -force;New-ADUser -Name \"{message.Text.Split()[1]}\" -Path \"OU={config["OU"]},{config["DomainPath"]}\" -AccountPassword $secureString -ChangePasswordAtLogon $false -Enabled $true");
                                new PowerShellHM().RunScript($"Add-ADGroupMember -Identity \"ENGLISH\" -Members \"{message.Text.Split()[1]}\"");
                                await botClient.SendTextMessageAsync(chat.Id, "Учетка пользователя " + message.Text.Split()[1] + " создана в группе пользователей ENGLISH c паролем @k22333Rte%");
                                return;
                            }
                            else if (message.Text.Contains("/delete") && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                new PowerShellHM().RunScript($"Remove-ADUser -Identity \"{message.Text.Split()[1]}\" -Confirm:$False");
                                await botClient.SendTextMessageAsync(chat.Id, "Учетка пользователя " + message.Text.Split()[1] + " была удалена");
                                return;
                            }
                            else if (message.Text.Contains("/resetpass") && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                new PowerShellHM().RunScript($"Set-ADAccountPassword -Identity \"{message.Text.Split()[1]}\" -Reset -NewPassword (ConvertTo-SecureString -AsPlainText \"p@ssw0rd\" -Force)");
                                await botClient.SendTextMessageAsync(chat.Id, "Пароль пользователя " + message.Text.Split()[1] + " был сброшен - p@ssw0rd");
                                return;
                            }
                            else if (message.Text == "/scan" && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                await botClient.SendTextMessageAsync(chat.Id, new Networking().IPAddress+" "+ Networking.NetworkGateway());
                                return;
                            }
                            else if (message.Text == "/wakeupalldevices" && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                new WOLService();
                                await botClient.SendTextMessageAsync(chat.Id, "Запрос на включение всех устройств отправлен");
                                return;
                            }
                            else if (message.Text.Contains("/wakeupmac") && FileChatsId.ChatidIsExists(message.Chat.Id, "chatid.json"))
                            {
                                new WOLService().SendWakeOnLan(PhysicalAddress.Parse(message.Text.Split()[1]), 7);
                                await botClient.SendTextMessageAsync(chat.Id, "Запрос на включение устройства отправлен");
                                return;
                            }
                            else if (message.Text.Contains("/key"))
                            {
                                if (FileChatsId.Add(message.Text.Split()[1], message.Chat.Id, "chatid.json"))
                                    await botClient.SendTextMessageAsync(chat.Id, "Учетная запись зарегистрирована!");
                                else
                                    await botClient.SendTextMessageAsync(chat.Id, "Учетная запись уже была зарегистрирована!");
                            }
                            else if (message.Text.Contains("/help"))
                            {
                                if (!string.IsNullOrEmpty(message.Text.Split()[1]))
                                    await botClient.SendTextMessageAsync(chat.Id, System.IO.File.ReadAllText("MessagesData/"+message.Text.Split()[1]+".txt"), parseMode: ParseMode.Markdown);
                                else
                                    await botClient.SendTextMessageAsync(chat.Id, System.IO.File.ReadAllText("MessagesData/help.txt"), parseMode: ParseMode.Markdown);
                                return;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chat.Id, System.IO.File.ReadAllText("MessagesData/error.txt"), parseMode: ParseMode.Markdown);
                                return;
                            }
                            return;
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