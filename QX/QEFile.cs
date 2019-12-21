using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QEFile : QueueAbstract, IDisposable {

        private string fullPath;
        private bool deleteAfterSend = true;
        private QEMSMQ serviceQueue;
        private string fileFilter = "*.*";
        private FileSystemWatcher watcher;

        public QEFile(XElement defn) : base(defn) {
        }
        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait) {
            try {
                ExchangeMessage xm = serviceQueue.Listen(immediateReturn, priorityWait);
                return xm;
            } catch (Exception ex) {
                logger.Error(ex);
                return null;
            }
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage mess) {

            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath)) {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }

            try {
                await Task.Run(() => File.WriteAllText(newFullPath, mess.payload));
                mess.sent = true;
                mess.status = $"Sent file to {newFullPath}";

            } catch (Exception) {
                mess.sent = false;
                mess.status = $"Unable to write file to {newFullPath}";
            }

            return mess;
        }

        public new void Stop() {
            OK_TO_RUN = false;

            try {
                watcher.EnableRaisingEvents = false;
            } catch (Exception) {
                //
            }
            try {
                serviceQueue.Stop();
            } catch (Exception) {
                //
            }
        }
        public override bool SetUp() {

            try {
                fullPath = definition.Attribute("path").Value;
            } catch (Exception) {
                fullPath = null;
            }
            try {
                fileFilter = definition.Attribute("fileFilter").Value;
            } catch (Exception) {
                fileFilter = "*.*";
            }

            try {
                deleteAfterSend = bool.Parse(definition.Attribute("deleteAfterSend").Value);
            } catch (Exception) {
                deleteAfterSend = false;
            }

            if (definition.Name == "input") {
                logger.Trace("Starting FileWatcher");
                // Create a service queue manager to write to and read from the buffer queue
                serviceQueue = new QEMSMQ(bufferQueueName);
                _ = Task.Run(() => Watch());
            }

            return true;
        }

        private async Task<bool> Watch() {
            var tcs = new TaskCompletionSource<bool>();

            try {
                watcher = new FileSystemWatcher();
                watcher.Path = fullPath;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = fileFilter;
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;

                logger.Info("Started FileWatcher");
                return await tcs.Task;
            } catch (Exception ex) {
                logger.Error(ex.Message);
                throw;
            }
        }

        //private static void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e) {
        //    logger.Trace($"A new file has been renamed from {e.OldName} to {e.Name}");
        //}
        //private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e) {
        //    Console.WriteLine($"A new file has been created - {e.Name}");
        //}

        private void OnChanged(object source, FileSystemEventArgs e) {
            logger.Info($"Change Detected - {e.ChangeType} {e.FullPath}");

            if (File.Exists(e.FullPath)) {
                if (e.ChangeType == WatcherChangeTypes.Changed) {
                    _ = Task.Run(() => ProcessFile(e.FullPath));
                }
            }


            //if (e.ChangeType == WatcherChangeTypes.Created) {
            //    _ = Task.Run(() => ProcessFile(e.FullPath));
            //}
        }

        private void ProcessFile(string fullPath) {



            while (!IsFileReady(fullPath)) {
                Thread.Sleep(5);
            }
            logger.Info($"Received file {fullPath}");

            if (!File.Exists(fullPath)) {
                logger.Info($"File {fullPath} no longer exists");
                return;
            }

            try {
                string text = File.ReadAllText(fullPath, Encoding.UTF8);
                ExchangeMessage xm = new ExchangeMessage(text);

                _ = Task.Run(() => serviceQueue.ServiceSend(xm));

                if (deleteAfterSend) {
                    File.Delete(fullPath);
                }
            } catch (Exception ex) {
                logger.Error(ex);
            }

        }

        public static bool IsFileReady(string filename) {
            // If the file can be opened for exclusive access it means that the file is no longer locked by another process.
            try {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            } catch (Exception) {
                return false;
            }
        }

        public void Dispose() {
            try {
                serviceQueue.Dispose();
                watcher.Dispose();
            } catch { }
        }
    }
}
