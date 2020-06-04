using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange
{
    class QEFile : QueueAbstract, IDisposable
    {

        private string fullPath;
        private bool deleteAfterSend = true;
        private QEMSMQ serviceQueue;
        private string fileFilter = "*.*";
        private FileSystemWatcher watcher;
        private readonly Queue<string> files = new Queue<string>();

        public QEFile(XElement defn, IProgress<MonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress)
        {

        }
        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait)
        {
            logger.Info("Listen for files");

            if (files.Count > 0)
            {
                ExchangeMessage xm;
                string fileName = files.Dequeue();
                try
                {
                    xm = new ExchangeMessage(File.ReadAllText(fileName, Encoding.UTF8));
                }
                catch (Exception)
                {
                    return null;
                }
                logger.Info($"Sending file {fileName}");

                try
                {
                    if (deleteAfterSend)
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return xm;
            }
            else
            {
                return null;
            }
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage mess)
        {

            int count = 0;
            string countStr = count.ToString();

            while (countStr.Length < 6)
            {
                countStr = "0" + countStr;
            }


            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath;
            string tempFileName = string.Format("{0}{1}", fileNameOnly, countStr);
            newFullPath = Path.Combine(path, tempFileName + extension);

            while (File.Exists(newFullPath))
            {
                count++;
                countStr = count.ToString();

                while (countStr.Length < 6)
                {
                    countStr = "0" + countStr;
                }
                tempFileName = string.Format("{0}{1}", fileNameOnly, countStr);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }

            try
            {
                await Task.Run(() => File.WriteAllText(newFullPath, mess.payload));
                mess.sent = true;
                mess.status = $"Sent file to {newFullPath}";

            }
            catch (Exception)
            {
                mess.sent = false;
                mess.status = $"Unable to write file to {newFullPath}";
            }

            return mess;
        }

        public new void Stop()
        {
            OK_TO_RUN = false;

            try
            {
                watcher.EnableRaisingEvents = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            try
            {
                serviceQueue.Stop();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
        public override bool SetUp()
        {

            try
            {
                fullPath = definition.Attribute("path").Value;
            }
            catch (Exception)
            {
                fullPath = null;
            }
            try
            {
                fileFilter = definition.Attribute("fileFilter").Value;
            }
            catch (Exception)
            {
                fileFilter = "*.*";
            }

            try
            {
                deleteAfterSend = bool.Parse(definition.Attribute("deleteAfterSend").Value);
            }
            catch (Exception)
            {
                deleteAfterSend = false;
            }


            try
            {
                string[] fileEntries = Directory.GetFiles(fullPath);
                if (fileEntries != null)
                {
                    foreach (string file in fileEntries)
                    {
                        files.Enqueue(file);
                    }
                    Array.Sort(fileEntries);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            if (definition.Name == "input")
            {
                logger.Trace("Starting FileWatcher");
                // Create a service queue manager to write to and read from the buffer queue
                serviceQueue = new QEMSMQ(bufferQueueName);
                _ = Task.Run(() => Watch());
            }

            return true;
        }

        private async Task<bool> Watch()
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                watcher = new FileSystemWatcher
                {
                    Path = fullPath,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = fileFilter
                };
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;

                logger.Info("Started FileWatcher");
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {

            logger.Info($"Change Detected - {e.ChangeType} {e.FullPath}");

            if (File.Exists(e.FullPath))
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    _ = Task.Run(() => ProcessFile(e.FullPath));
                }
            }
        }

        private void ProcessFile(string fullPath)
        {
            while (!IsFileReady(fullPath))
            {
                Thread.Sleep(5);
            }
            logger.Info($"Received file {fullPath}");

            if (!File.Exists(fullPath))
            {
                logger.Info($"File {fullPath} no longer exists");
                return;
            }

            try
            {
                files.Enqueue(fullPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        public static bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                serviceQueue.Dispose();
                watcher.Dispose();
            }
            catch { }
        }
    }
}
