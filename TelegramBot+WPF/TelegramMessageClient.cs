using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace TelegramBot_WPF
{

    class TelegramMessageClient
    {

        static string FileStoragePath = "C:\\telBotStorage";
        static string json1 = System.IO.File.ReadAllText(FileStoragePath + "/history.json");
        private MainWindow w;

        private TelegramBotClient bot;
        public ObservableCollection<MessageLog> BotMessageLog { get; set; }

        public TelegramMessageClient(MainWindow W)
        {
            this.BotMessageLog = new ObservableCollection<MessageLog>();
            this.w = W;

            bot = new TelegramBotClient("5347192057:AAH1D7QqYNn6hXGnYWH8xqTe8HJ0PfU2QdE");

            //bot.OnMessage += MessageListener;

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {update.Message.Chat.FirstName} {update.Message.Chat.Id} {update.Message.Text}";
            var messageText = update.Message.Text;

            w.Dispatcher.Invoke(() =>
            {
                BotMessageLog.Add(
                new MessageLog(
                    DateTime.Now.ToLongTimeString(), messageText, update.Message.Chat.FirstName, update.Message.Chat.Id));
                // json converting
                CheckJsonFile();
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
                string json = json1 + System.Text.Json.JsonSerializer.Serialize(BotMessageLog, options);
                System.IO.File.WriteAllText(FileStoragePath + "/history.json", json);
            });
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text?.ToLower() == "/start") //start
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Welcome to VlasiusBot! To print file list enter \"/allfiles\". To download file enter \"/file [filename]\"");
                    return;
                }
                else if (message.Text?.ToLower() == "/allfiles") //out all files
                {
                    OutFiles(botClient, update);
                    return;
                }
                else if (message.Text != null && message.Text.Contains("/file")) //download file
                {
                    if (update.Message.Text == "/file")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Enter file name");
                    }
                    else
                    {
                        string[] fileNameArr = update.Message.Text.Split(' ');
                        string fileName = fileNameArr[1];
                        DownloadFile(botClient, update, fileName);

                    }
                    return;
                }
                else if ((update.Message.Document is not null)) //upload file
                {
                    string fileName = update.Message.Document.FileName;
                    UploadFile(botClient, update, fileName);
                    return;
                }
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public void SendMessage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            bot.SendTextMessageAsync(id, Text);
        }
        public void CheckJsonFile() //Method to create history.json if it doesn't exist
        {
            CheckDir();
            string curFile = @"c:\telBotStorage\history.json";
            if (!(System.IO.File.Exists(curFile)))
            {
                FileStream fs = System.IO.File.Create(curFile);
                fs.Close();
            }

        }
        public void CheckDir() //Method to create folder if it doesn't exist
        {
            //DirectoryInfo di = Directory.CreateDirectory(FileStoragePath);
            var dir = new DirectoryInfo(@"C:\telBotStorage"); //C:\\telBotStorage
            if (!dir.Exists) dir.Create();
        }

        public async Task OutFiles(ITelegramBotClient botClient, Update update) //Print available files list
        {
            var message = update.Message;
            var dir = new DirectoryInfo(FileStoragePath);
            if (dir.Exists && dir.GetFiles().Length > 0)
            {
                string str2 = "";
                foreach (var fs in dir.GetFiles())
                {
                    string str1 = fs.Name.ToString();
                    str2 = str2 + str1 + ";   ";
                }
                await botClient.SendTextMessageAsync(message.Chat, $"Files available to download:  {str2}. To download file enter \"/file [filename]\"");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, "No available files to download");
            }
        }

        public async Task DownloadFile(ITelegramBotClient botClient, Update update, string fileName)
        {
            var message = update.Message;
            CheckDir();
            var filePath = System.IO.Path.Combine(FileStoragePath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                await using var stream = System.IO.File.Open(filePath, FileMode.Open);
                await botClient.SendDocumentAsync(message.Chat!, new InputOnlineFile(stream, fileName));
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, "File doesn't exist");
            }
        }


        public async Task UploadFile(ITelegramBotClient botClient, Update update, string fileName)
        {
            CheckDir();

            var message = update.Message;
            var document = update.Message.Document;
            var file = await botClient.GetFileAsync(document.FileId);
            var filePath = System.IO.Path.Combine(FileStoragePath, document.FileName!);
            await using var fs = new FileStream(filePath, FileMode.Create);
            await botClient.DownloadFileAsync(file.FilePath!, fs);
            await botClient.SendTextMessageAsync(message.Chat, $"File {document.FileName!} downloaded into {FileStoragePath}");

        }
    }
}