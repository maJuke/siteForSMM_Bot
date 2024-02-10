using MySql.Data.MySqlClient;
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
            botClient = new TelegramBotClient("YOUR-TOKEN-HERE");

            bdCon.Server = ConfigurationManager.AppSettings.Get("BDServer");
            bdCon.BDName = ConfigurationManager.AppSettings.Get("BDName");
            bdCon.UserName = ConfigurationManager.AppSettings.Get("UserName");
            bdCon.Password = ConfigurationManager.AppSettings.Get("Password");

            Console.WriteLine("Enter any key to close the bot!\n\n");
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
            string destinationFilePath = "DESTINATION-TO-SAVE-THE-FILE";

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

                await using Stream fileStream = System.IO.File.Create(destinationFilePath);
                var file = await botClient.GetInfoAndDownloadFileAsync(
                    fileId: fileId,
                    destination: fileStream,
                    cancellationToken: token);

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "К какой категории загрузить документ?", replyMarkup: getButtons);
            }
            else if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                if (update.Message.Text.ToLower() == "пример")
                {
                    methodType = "TABLE-NAME-FOR-EXAMPLES";
                }
                else if (update.Message.Text.ToLower() == "отзыв")
                {
                    methodType = "TABLE-NAME-FOR-REVIEWS";
                }

                if (!string.IsNullOrEmpty(methodType))
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Документ обрабатывается...");
                    Console.WriteLine("Downloading photo for " + methodType + " from " 
                                        + update.Message.Chat.Username + " at " + DateTime.Now);
                    try
                    {
                        using (StreamReader sr = new StreamReader(destinationFilePath))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                sr.BaseStream.CopyTo(ms);
                                var bytes = ms.ToArray();
                                var fileSize = ms.Length;

                                var command = new MySqlCommand("", bdCon.Connection);

                                command.CommandText = "INSERT INTO " + methodType + " (pic_id, pic_blob, pic_date, pic_descr, pic_type, pic_size) VALUES (DEFAULT, @blob, "
                                                        + "CURDATE(), null, \"jpg\", @size);";
                                var paramBlob = new MySqlParameter("@blob", MySqlDbType.LongBlob);
                                var paramSize = new MySqlParameter("@size", MySqlDbType.Float, 200);

                                paramBlob.Value = bytes;
                                paramSize.Value = fileSize;

                                command.Parameters.Add(paramBlob);
                                command.Parameters.Add(paramSize);

                                command.ExecuteNonQuery();
                            }
                        }
                        System.IO.File.Delete(destinationFilePath);

                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Документ успешно загружен!");
                    } catch (Exception ex) 
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Произошла ошибка! Смотрите в консоль!");
                        Console.WriteLine(ex.ToString() + "\n\n" + ex.StackTrace);
                    }
                    
                }
            }
            
        }
    }
}