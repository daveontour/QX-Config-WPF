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
        private string fileFilter = "*.*";
        private FileSystemWatcher watcher;
        private bool isOutput = false;
        private readonly Queue<string> files = new Queue<string>();
        public object sendLock = new object();

        public QEFile(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress)
        {

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
        }
        public override bool SetUp()
        {

            try
            {
                if (definition.Name == "output" || definition.Name == "altqueue")
                {
                    isOutput = true;
                }
                else
                {
                    isOutput = false;
                }

            }
            catch (Exception)
            {
                isOutput = false;
            }

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

            if (!isOutput)
            {
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
            }

            return true;
        }

        override public async Task StartListener()
        {
            // First, process any existing files in the directory
            await Task.Run(() =>
            {
                string[] files = Directory.GetFiles(fullPath, fileFilter);
                foreach (string file in files)
                {
                    ProcessFile(file);
                }
            });

            await Task.Run(() => Watch());
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
            FileSystemWatcher fsw = source as FileSystemWatcher;


            lock (sendLock)
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

        }

        private void ProcessFile(string fullPath)
        {

            logger.Info($"Received file {fullPath}");

            if (!File.Exists(fullPath))
            {
                logger.Warn($"File {fullPath} no longer exists");
                return;
            }

            int totalTime = 0;
            while (!IsFileReady(fullPath) && totalTime < 5000)
            {
                Thread.Sleep(10);
                totalTime += 10;
                if (!File.Exists(fullPath))
                {
                    logger.Warn($"File {fullPath} no longer exists");
                    return;
                }
            }

            if (totalTime >= 5000)
            {
                logger.Warn($"Timeout waiting for file {fullPath} to be ready");
                return;
            }

            if (!File.Exists(fullPath))
            {
                logger.Warn($"File {fullPath} no longer exists");
                return;
            }


            SendToPipe(File.ReadAllText(fullPath, Encoding.UTF8));

            try
            {
                if (deleteAfterSend)
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                watcher.Dispose();
            }
            catch { }
        }
    }
}
