using System.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SMMBot
{
    class Program
    {

        static BDConnect bdCon = BDConnect.Instance();
        static ITelegramBotClient botClient;

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("YOUR_BOT_TOKEN_HERE");

            bdCon.Server = ConfigurationManager.AppSettings.Get("BDServer");
            bdCon.BDName = ConfigurationManager.AppSettings.Get("BDName");
            bdCon.UserName = ConfigurationManager.AppSettings.Get("UserName");
            bdCon.Password = ConfigurationManager.AppSettings.Get("Password");

            botClient.StartReceiving(updateHandler, errorHandler);

            Console.ReadLine();

            bdCon.Close();
        }

        private static Task errorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.StackTrace
                    + "\n\n"
                    + exception.Message
                    + "\n\n"
                    + exception.StackTrace
                    + "\n\n"
                    + exception.ToString());

            return Task.CompletedTask;
        }

        async static Task updateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            string methodType = "";

            var getButtons = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Пример"),
                    new KeyboardButton("Отзыв")
                }
            });

            if (update.Type == UpdateType.Message && update.Message.Document != null && bdCon.IsConnect())
            {
                var fileId = update.Message.Document.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;
                var fileSize = fileInfo.FileSize;
                var fileTitle = update.Message.Document.FileName;

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "К какой категории загрузить документ?", replyMarkup: getButtons);
            }
            else if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                if (update.Message.Text.ToLower() == "пример")
                {
                    methodType = "pics_for_examples";
                }
                else if (update.Message.Text.ToLower() == "отзыв")
                {
                    methodType = "pics_for_reviews";
                }

                if (!string.IsNullOrEmpty(methodType))
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Добавьте описание.", replyMarkup: null);
                }
            }
            else if (update.Type == UpdateType.Message && update.Message.Text != null && !string.IsNullOrEmpty(methodType))
            {
                var description = update.Message.Text;
                Console.WriteLine(123); // Не приходит сюды
                /*
                var command = new MySqlCommand();
                command.CommandText = "INSERT INTO @table_name (pic_id, pic_blob, pic_date, pic_descr, pic_type, pic_size) VALUES (DEFAULT, @blob, " 
                                        + DateTime.Now.ToString("dd.MM.yyyy") 
                                        + ", @description, @type, @size);";
                var paramTableName = new MySqlParameter("@table_name", MySqlDbType.VarChar, methodType.Length, methodType);
                var paramBlob = new MySqlParameter("@blob", MySqlDbType.Blob, bytes.Length);
                */

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Документ успешно загружен!");
            }
        }
    }
}