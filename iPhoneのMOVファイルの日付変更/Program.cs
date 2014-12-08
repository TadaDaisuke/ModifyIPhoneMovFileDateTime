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
            // 対象の拡張子かをチェック
            var isNotMovie = true;
            foreach (var ext in ".mov;.mp4;.m2ts".Split(';'))
                if (fileInfo.Extension.ToLower() == ext) isNotMovie = false;
            if (isNotMovie) return;

            DateTime dt;
            var fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);

            // ファイル名で日付判定1
            if (DateTime.TryParseExact(fileName, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
            {
                SetFileTime(fileInfo, dt);
                return;
            }

            // ファイル名で日付判定2
            if (DateTime.TryParseExact(fileName, "yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
            {
                SetFileTime(fileInfo, dt);
                return;
            }

            // バイナリで日付判定
            byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
            if (GetRecordingDate(fileBytes, out dt))
            {
                SetFileTime(fileInfo, dt);
                return;
            }
        }

        private static void SetFileTime(FileInfo fileInfo, DateTime dt)
        {
            fileInfo.CreationTime = dt;
            fileInfo.LastWriteTime = dt;
            Console.WriteLine("{0} のファイル日付を {1} に変更しました。", fileInfo.Name, dt);
        }

        private static bool GetRecordingDate(byte[] fileBytes, out DateTime dt)
        {
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
                    return true;
                }
            }
            dt = new DateTime();
            return false;
        }
    }
}
