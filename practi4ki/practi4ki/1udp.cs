using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpClientApp
{
    class UDP
    {
        // либо определит их сразу.
        static string remoteAddress; // хост для отправки данных
        static int remotePort; // порт для отправки данных
        static int localPort; // локальный порт для прослушивания входящих подключений

        static string peredacha;
        static string fileName;

        static void Main(string[] args)
        {
            // запись в файл
            using (FileStream fstream = new FileStream(@"F:\LABDotNET\practi4ki\practi4ki\Hello.txt", FileMode.OpenOrCreate))
            {
                fstream.Seek(0, SeekOrigin.End);
                // считываем 10 символов с текущей позиции
                byte[] output = new byte[10];
                fstream.Read(output, 0, output.Length);
                // декодируем байты в строку
                string textFromFile = Encoding.Default.GetString(output);
                // считываем весь файл
                // возвращаем указатель в начало файла
                fstream.Seek(0, SeekOrigin.Begin);
                output = new byte[fstream.Length];
                fstream.Read(output, 0, output.Length);
                // декодируем байты в строку
                textFromFile = Encoding.Default.GetString(output);
                Console.WriteLine($"Текст из файла: {textFromFile}"); // hello house
                peredacha = textFromFile;
            }

            try
            {
                Console.Write("Введите порт для прослушивания: "); // локальный порт
                Console.Write("8000 \n");
                localPort = Int32.Parse("8000");

                Console.Write("Введите удаленный адрес для подключения: ");
                Console.Write("127.0.0.1 \n");
                remoteAddress = "127.0.0.1"; // адрес, к которому мы подключаемся

                Console.Write("Введите порт для подключения: ");
                Console.Write("8000 \n");
                remotePort = Int32.Parse("8000"); // порт, к которому мы подключаемся

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
                SendMessage(); // отправляем сообщение
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        private static void SendMessage()
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений
            try
            {
                while (true)
                {
                    string secondLine = peredacha; 
                    //File.ReadLines("Hello.txt"); 
                    // сообщение для отправки
                    fileName = "Hello.txt";
                    FileInfo info = new FileInfo(fileName);
                    string message = $"{fileName}:{info.Length}";
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    sender.Send(data, data.Length, remoteAddress, remotePort);
                    FileStream input = info.OpenRead();
                    int count = 1024;
                    //int offset = 0;
                    data = new byte[count];
                    // byte[] data = Encoding.Unicode.GetBytes(secondLine);
                    // sender.Send(data, data.Length, remoteAddress, remotePort); // отправка
                    while (input.Read(data, 0, count) > 0)
                    {
                        string messageToSend = File.ReadAllText(fileName); // сообщение для отправки
                        byte[] dataToSend = Encoding.Unicode.GetBytes(messageToSend);
                        sender.Send(dataToSend, dataToSend.Length, remoteAddress, remotePort); // отправка
                    }
                    input.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }



        }

        private static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(localPort); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    //string message = Encoding.Unicode.GetString(data);
                    string[] str = Encoding.Unicode.GetString(data).Split(':');
                   // Console.WriteLine("Собеседник: {0}", message);
                    int lenght = Int32.Parse(str[1]);
                    Console.WriteLine($"{remoteIp.Address}:{lenght}");

                    int recieved = 0;
                    FileStream output = File.Create($@"./folder{fileName}");//uotputFile.txt");
                    while (recieved < lenght)
                    {
                        data = receiver.Receive(ref remoteIp);
                        output.Write(data, 0, data.Length);
                        recieved += data.Length;
                    }
                    output.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }
    }
}
