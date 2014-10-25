using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPhoneのMOVファイルの日付変更
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (Directory.Exists(arg))
                {
                    ProcessDirectory(new DirectoryInfo(arg));
                }
                else if (File.Exists(arg))
                {
                    ProcessFile(new FileInfo(arg));
                }
            }

            Console.ReadLine();
        }

        private static void ProcessDirectory(DirectoryInfo dirInfo)
        {
            foreach (var file in dirInfo.GetFiles())
            {
                ProcessFile(file);
            }
        }

        private static void ProcessFile(FileInfo fileInfo)
        {
            if (fileInfo.Extension.ToLower() != ".mov") return;
            byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
            var dt = GetRecordingDate(fileBytes);
            if (dt != null)
            {
                fileInfo.CreationTime = dt;
                fileInfo.LastWriteTime = dt;
                Console.WriteLine("{0} のファイル日付を {1} に変更しました。", fileInfo.Name, dt);
            }
        }

        private static DateTime GetRecordingDate(byte[] fileBytes)
        {
            var dt = new DateTime();

            var searchData = new List<byte>();
            foreach (var hex in "64 61 74 61 00 00 00 01 4A 50 2A 0E 32 30".Split(' '))
            {
                searchData.Add(Convert.ToByte(hex, 16));
            }
            for (long i = 0; i < fileBytes.Length; i++)
            {
                var isMatched = true;
                for (int j = 0; j < searchData.Count; j++)
                {
                    if (fileBytes[i + j] != searchData[j])
                    {
                        isMatched = false;
                        break;
                    }
                }
                if (isMatched)
                {
                    byte[] dateBytes = new byte[24];
                    Array.Copy(fileBytes, i + 12, dateBytes, 0, 24);
                    DateTime.TryParseExact(
                        Encoding.ASCII.GetString(dateBytes),
                        "yyyy-MM-ddTHH:mm:sszzz",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dt);
                    break;
                }
            }
            return dt;
        }

    }
}
