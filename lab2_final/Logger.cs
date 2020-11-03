using lab2_final;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace lab2_2
{
    class Logger
    {
        public static Aes myAes = Aes.Create();
        public byte[] key = myAes.Key;
        public byte[] iv = myAes.IV;

        FileSystemWatcher watcher;
        object obj = new object();
        bool enabled = true;
        public Logger()
        {

            watcher = new FileSystemWatcher(@"C:\Users\User\Desktop\lr2\SourceDir");
            watcher.IncludeSubdirectories = true;
            watcher.Deleted += Watcher_Deleted;
            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;
        }

        public void Start()
        {
            watcher.EnableRaisingEvents = true;
            while (enabled)
            {
                Thread.Sleep(1000);
            }
            RecordEntryForAction("start");
        }
        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }
        // переименование файлов
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string fileEvent = "renamed to " + e.FullPath;
            string filePath = e.OldFullPath;
            RecordEntry(fileEvent, filePath);
        }
        // изменение файлов
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "changed";
            string filePath = e.FullPath;
            RecordEntry(fileEvent, filePath);
        }
        // создание файлов
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "created";
            string filePath = e.FullPath;
            string path = filePath;
            string compressedFile = Path.ChangeExtension(path, "gz");
            string data;
            byte[] dataProcesed;
            byte[] dataDeProcesed;


            string deData;
            if (filePath.EndsWith(".txt"))
            {
                RecordEntry("qwerty", filePath);
               

                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        data = sr.ReadToEnd();

                    }
                    if (data.Length != 0)
                    {
                        dataProcesed = Crypto.EncryptStringToBytes_Aes(data, key, iv);
                        using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate))
                        {
                            fstream.Write(dataProcesed, 0, dataProcesed.Length);
                        }
                    }


                    //begin editing
                    string res = Archive.MyArchive(path, compressedFile);
                    string PreviousSize = res.Split(' ')[0];
                    string compressedSize = res.Split(' ')[1];
                    
                  RecordEntryForAction(String.Format("Compression of file {0} is comleted. Previous size: {1}  compressed size: {2}.",
                                    path, PreviousSize, compressedSize));
                    
                }
                catch (Exception ee)
                {
                    RecordEntryForAction(ee.Message);
                }

               

            }
            else {
                if (filePath.EndsWith(".gz"))
                {
                     try
                    {
                        string[] paths = { @"C:\Users\User\Desktop\lr2\TargetDir\", Path.GetFileName(compressedFile) };
                        string targetPath = Path.Combine(paths);
                        Console.WriteLine(targetPath);

                        // Ensure that the target does not exist.
                        if (File.Exists(targetPath))
                            File.Delete(targetPath);

                        // Move the file.
                        File.Move(compressedFile, targetPath);
                        RecordEntryForAction(String.Format("{0} was moved to {1}.", compressedFile, targetPath));


                        //decompressed
                        string targetPathDecompressed = Path.ChangeExtension(targetPath, "txt");

                        //class
                        Dearchive.DeArchieves(targetPath,targetPathDecompressed);
                        //using (FileStream sourceStream = new FileStream(targetPath, FileMode.OpenOrCreate))
                        //{
                        //    // поток для записи восстановленного файла
                        //    using (FileStream targetStream = File.Create(targetPathDecompressed))
                        //    {
                        //        // поток разархивации
                        //        using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                        //        {
                        //            decompressionStream.CopyTo(targetStream);
                        //        }
                        //    }
                        //}
                        RecordEntryForAction(String.Format("File recovered: {0}", targetPathDecompressed));


                        //string[] paths1 = { @"\\Mac\Home\Desktop\lab2\TargetDir\Archieve", Path.GetFileName(targetPath) };
                        //string targetPathArchive = Path.Combine(paths1);
                        //Console.WriteLine(targetPath);

                        // Ensure that the target does not exist.
                        //if (File.Exists(targetPathArchive))
                        //  File.Delete(targetPathArchive);

                        // Move the file.

                        //create
                        string[] pathsDir = { @"C:\Users\User\Desktop\lr2\TargetDir\Archieve", @"\", Convert.ToString(DateTime.Now.Year) };                       
                        string a = @"C:\Users\User\Desktop\lr2\TargetDir\Archieve" + @"\" + Convert.ToString(DateTime.Now.Year) + @"\" + Convert.ToString(DateTime.Now.Month) + @"\" + Convert.ToString(Convert.ToString(DateTime.Now.Date).Split(' ')[0]) + @"\" + Convert.ToString(DateTime.Now.Hour) + @"\" + Convert.ToString(DateTime.Now.Minute) + @"\" + Convert.ToString(DateTime.Now.Second) + @"\" + Convert.ToString(DateTime.Now.Millisecond);
                        string pathDir = Path.Combine(pathsDir);
                        Console.WriteLine(a);
                        DirectoryInfo di = Directory.CreateDirectory(a);


                        string[] paths1 = { a, Path.GetFileName(targetPath) };
                        string targetPathArchive = Path.Combine(paths1);

                        // Ensure that the target does not exist.
                        if (File.Exists(targetPathArchive))
                            File.Delete(targetPathArchive);

                        File.Move(targetPath, targetPathArchive);
                        RecordEntryForAction(String.Format("{0} was moved to {1}.", targetPath, targetPathArchive));



                        using (FileStream sr = File.OpenRead(targetPathDecompressed))
                        {
                            dataDeProcesed=new byte[sr.Length];
                            sr.Read(dataDeProcesed,0, dataDeProcesed.Length);

                        }
                       
                            deData = Decrypto.DecryptStringFromBytes_Aes(dataDeProcesed, key, iv);
                            using (StreamWriter sw = new StreamWriter(targetPathDecompressed, false, System.Text.Encoding.Default))
                            {
                                sw.Write(deData);
                            }



                        //string[] pathsDir = { @"\\Mac\Home\Desktop\lab2\TargetDir\Archieve", @"\" ,Convert.ToString(DateTime.Now.Year), @"\", Convert.ToString(DateTime.Now.Month), @"\", Convert.ToString(Convert.ToString(DateTime.Now.Date).Split(" ")[0]), @"\", Convert.ToString(DateTime.Now.TimeOfDay) };
                        

                        // DirectoryInfo dirInfo = new DirectoryInfo(pathDir);
                        //if (!dirInfo.Exists)
                        //{
                        //   dirInfo.Create();
                        //}
                        //dirInfo.CreateSubdirectory(subpath);

                    }
                    catch (Exception ee)
                    {
                        RecordEntryForAction(ee.Message);
                    }


                  

                }
            }

            RecordEntry(fileEvent, filePath);
        }

        private void MyArchive(string path, object o)
        {
            throw new NotImplementedException();
        }

        // удаление файлов
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "deleted";
            string filePath = e.FullPath;
            RecordEntry(fileEvent, filePath);
        }

        private void RecordEntry(string fileEvent, string filePath)
        {
            lock (obj)
            {
                using (StreamWriter writer = new StreamWriter(@"C:\Users\User\Desktop\zoo\templog.txt", true))
                {
                    writer.WriteLine(String.Format("{0} file {1} was {2}",
                        DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent));
                    writer.Flush();
                }
            }
        }




        public void RecordEntryForAction(string fileEvent)
        {
            lock (obj)
            {
                using (StreamWriter writer = new StreamWriter(@"C:\Users\User\Desktop\zoo\templog.txt", true))
                {
                    writer.WriteLine(String.Format("{0} file  was {1}",
                        DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),  fileEvent));
                    writer.Flush();
                }
            }
        }
    }
    
    
    
    
    //"\\\\Mac\\Home\\Desktop\\lab2\\SourceDir\\2020\\11\\29\\1 — копия (2).gz"
    // 
}
