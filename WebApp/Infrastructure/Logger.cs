using System;
using System.Collections.Generic;
using System.IO;

namespace WebApp.Infrastructure
{
    public class Logger
    {
        public void Log(string message)
        {
            var dir = Directory.GetCurrentDirectory();
            File.AppendAllText($@"{dir}/log.txt",
                $@"MESSAGE: {message} | TIMESTAMP: {DateTime.UtcNow} | {Environment.NewLine}");
        }

        public List<string> GetAll()
        {
            var dir = Directory.GetCurrentDirectory();
            var file = $@"{dir}/log.txt";
            var logs = new List<string>();
            using (var reader = new StreamReader(file))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    logs.Add(line);
                }
            }
            return logs;
        }
    }
}
