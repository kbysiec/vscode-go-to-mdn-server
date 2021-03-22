using System;
using System.Collections.Generic;
using System.IO;

namespace WebApp.Utils
{
    public class Logger
    {
        private static string Dir => Directory.GetCurrentDirectory();
        private static string FileName => $@"{Dir}/log.txt";

        public void Add(string message)
        {
            File.AppendAllText(FileName,
                $@"MESSAGE: {message} | TIMESTAMP: {DateTime.UtcNow} | {Environment.NewLine}");
        }

        public List<string> GetAll()
        {
            var logs = new List<string>();
            using var reader = new StreamReader(FileName);
            while (reader.Peek() >= 0)
            {
                var line = reader.ReadLine();
                logs.Add(line);
            }

            logs.Reverse();

            return logs;
        }

        public void ClearAll()
        {
            File.WriteAllText(FileName, string.Empty);
        }
    }
}
