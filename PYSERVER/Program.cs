using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Diagnostics;

namespace PyServer
{
    class pyserverClass
    {
        private static int MERCHANT_ID = -1;
        private static String CONTAINER_NAME = "";
        private static String DIR_NAME = "";
        private static String LOG_DIR_NAME = "";
        private static String LOG_FILE_NAME = "";
        private static string LECENSE_FILE_PATH = "./";
        private static String CONTAINER = "CONTAINER";
        private static String DIRECTORYWATCHER = "DirectoryWatcher";
        private static string BLOBSASTOKEN = "";
        private static string QUEUESASTOKEN = "";
        private static string STORAGENAME = "";

        private CloudBlobContainer container = null;
        private CloudQueue queueContainer = null;

        private System.Collections.Generic.Dictionary<String, String> appProps = new Dictionary<String, String>();

        static void Main(string[] args)
        {

            System.Diagnostics.Process[] processes = Process.GetProcessesByName("PYSERVER");
            if (processes.Length > 1)
            { 
                Log("Can't duplicate run this program,it is already running in the background.");
                return;
            }

            pyserverClass mainProgram = new pyserverClass();
            mainProgram.init();
            mainProgram.directoryWatcher();
        }

        private void pyserverGetConfigInfo(string licFileName)
        {
            Dictionary<String, Object> configDict;
            try
            {
                String json = File.ReadAllText(licFileName);
                json = StringCipher.Decrypt(json, "CITRINE");
                configDict = JsonConvert.DeserializeObject<Dictionary<String, Object>>(json);
                foreach (var x in configDict)
                {
                    if (x.Key == "storageUrl")
                        STORAGENAME = x.Value.ToString();
                    if (x.Key == "containerName")
                    {
                        CONTAINER_NAME = x.Value.ToString();
                    }
                    if (x.Key == "sasBlobKey")
                    {
                        BLOBSASTOKEN = x.Value.ToString();
                    }
                    if (x.Key == "sasQueueKey")
                    {
                        QUEUESASTOKEN = x.Value.ToString();
                    }
                    if (x.Key == "merchantId")
                    {
                        MERCHANT_ID = int.Parse((x.Value.ToString()));
                    }
                }
                
            }
            catch (Exception ex)
            {
                Log("Could not read Configuration File " + ex.Message);
            }
        }

        private void init()
        {
            DIR_NAME = ConfigurationManager.AppSettings["dirtowatch"];
            LOG_DIR_NAME = ConfigurationManager.AppSettings["LogFileDir"];
            LOG_FILE_NAME = ConfigurationManager.AppSettings["LogFileName"];
            LECENSE_FILE_PATH = ConfigurationManager.AppSettings["licenseFilePath"];


            appProps = new Dictionary<String, String>();
            appProps.Add(CONTAINER, CONTAINER_NAME);

            if (!System.IO.Directory.Exists(DIR_NAME))
            {
                System.IO.Directory.CreateDirectory(DIR_NAME);
            }
            appProps.Add(DIRECTORYWATCHER, DIR_NAME);


            //Checking if log directory exists
            if (!System.IO.Directory.Exists(LOG_DIR_NAME))
            {
                //if not creating a new directory
                System.IO.Directory.CreateDirectory(LOG_DIR_NAME);

            }

            processStaleDataAndLogFile(LOG_DIR_NAME, LOG_FILE_NAME, DIR_NAME);

            // full Log file name 
            LOG_FILE_NAME = System.IO.Path.Combine(LOG_DIR_NAME, LOG_FILE_NAME);

            try
            {

                transferFilesToAzure();

            }
            catch (System.Exception e)
            {
                LogParam("Not able to connect to Azure, exception:", e.Message);
            }
        }


        private void prserverReadConfigFile()
        {
            String licFileName = LECENSE_FILE_PATH + "pyserver.json";
            pyserverGetConfigInfo(licFileName);
        }

        public void processStaleDataAndLogFile(string logDirName, string logFileName, string notSentDirInfo)
        {
            try
            {
                // Delete the *.csv file in archieve  folder as it is already transferred to cloud
                string archiveDirPathString = Path.Combine(notSentDirInfo, "archive");
                System.IO.DirectoryInfo dInfoArch = new System.IO.DirectoryInfo(archiveDirPathString);
                DeletingFiles(dInfoArch, ".csv");

                // Move log file with

                string logFileFullPathName = Path.Combine(logDirName, logFileName);

                DateTime currentDate = DateTime.Now;

                string dateText = currentDate.ToString("yyyyMMdd_HHmmss");

                string logFileFullPathNameWithDate = logFileFullPathName + "_" + dateText + ".txt";

                Log(" Log file with name is " + logFileFullPathName);

                File.Copy(logFileFullPathName, logFileFullPathNameWithDate, true);
                File.Delete(logFileFullPathName);


            }
            catch (Exception ex)
            {
                Log("Exeception in processStaleDataAndLogFile " + ex.Message);

            }
        }

        public static void Log(string msg)
        {
            try
            {

                using (var sw = new System.IO.StreamWriter(LOG_FILE_NAME, true))
                {
                    sw.Write(string.Format("{0} - {1}\n", DateTime.Now, msg));
                    sw.Flush();
                }
            }
            catch (Exception)
            {

            }
        }

        private static void LogParam(string message, string param)
        {
            string finalLogMsg = string.Format("{0}  {1} ", message, param);
            Log(finalLogMsg);
        }

        public static void DeletingFiles(System.IO.DirectoryInfo directory, string pattern)
        {
            try
            {
                //delete files:
                foreach (System.IO.FileInfo file in directory.GetFiles())
                {
                    if (file.FullName.Contains(pattern))
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void queueCreateContainer()
        {
            try
            {
                // Create new storage credentials using the SAS token.

                StorageCredentials accountSAS = new StorageCredentials(QUEUESASTOKEN);
                // Use these credentials and the account name to create a queueservice client.
                CloudStorageAccount accountWithSAS = new CloudStorageAccount(accountSAS, "pymarketing", endpointSuffix: null, useHttps: true);

                //Create the queue client.
                CloudQueueClient queueClient = accountWithSAS.CreateCloudQueueClient();

                //Getting queue reference
                queueContainer = queueClient.GetQueueReference(CONTAINER_NAME);
            }
            catch(Exception ex)
            {
                Log(" Failed in Queue Create Container ");
            }


        }
        static int GettWordAfteKey(string[] array, string keyvalue)
        {

            for (int i = 0; i < array.Length; i++)
            {
                // Get element by index.
                if (array[i] == keyvalue)
                {
                    return i + 1;
                }
            }
            return -1;
        }


        private int GetMerchantIdFromFileName(string fileName)
        {
            try
            {
                string[] words = fileName.Split('_');
                // Get mechant id 
                int key4_index = GettWordAfteKey(words, "KEY4");
                int merchant_id = 0;
                if (key4_index != -1)
                {
                    merchant_id = Int32.Parse(words[key4_index]);
                }
                else
                {

                    merchant_id = 0;
                }
                return merchant_id;
            }
            catch ( Exception ex)
            {
                Log(" Error in GetMerchantIdFromFileName " + ex.Message);
                return 0;
            }
        }


        private void directoryWatcher()
        {
            //System.Console.WriteLine("inside directoryWatcher");
            if (appProps[DIRECTORYWATCHER] != null && appProps[DIRECTORYWATCHER].Length > 0)
            {
                LogParam(" directory to watch : ", appProps[DIRECTORYWATCHER]);

                //System.IO.FileSystemWatcher wastcher = new System.IO.FileSystemWatcher(appProps[DIRECTORYWATCHER] + "\\", "*.*");

                FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
                watcher.Filter = "*.csv";
                watcher.Path = appProps[DIRECTORYWATCHER] + "\\";
                watcher.IncludeSubdirectories = false;


                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                //watcher.Changed += new System.IO.FileSystemEventHandler(onFileCreate);
                watcher.Created += new System.IO.FileSystemEventHandler(onFileCreate);



                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                //Console.WriteLine("Press \'q\' to quit the sample.");
                while (true)
                {
                    Thread.Sleep(2000);

                }

            }
            else
            {
                Log("NO Directory to watch");
            }
        }

        private void onFileCreate(object source, System.IO.FileSystemEventArgs e)
        {
            //System.Console.WriteLine("onFileCreate fired -> File Full path :" + e.FullPath + ", File name :" + e.Name);
            transferFilesToAzure();


        }

        private void queueMsgPush(string fileName)
        {
            try
            {
                if(queueContainer == null)
                {
                    queueCreateContainer();
                }
                //Create message client to pass to queue
                CloudQueueMessage message = new CloudQueueMessage(fileName);
                // Put message to the queue
                queueContainer.AddMessage(message);
                Log("Sucessfully added the message to the queue...");

            }

            catch (StorageException ex)
            {
                Log(ex.Message);
            }


        }

        private bool pycrmServerCheckForGoodFileName(string fileName)
        {
            if (MERCHANT_ID == -1)
            {
                prserverReadConfigFile();
            }

#if false
                if (MERCHANT_ID == GetMerchantIdFromFileName(fileName))
            {
                return true;
            }
            else
            {
                return false;
            }

#endif
                // Always return TRUE
                return true;
        }

        private void transferFilesToAzure()
        {
            int retrycount = 0;

            try
            {
                RETRY_TRANSFER = 0;
                //System.Console.WriteLine("Inside watchDirectoryCustom()+");
                //getting handle to watch directory
                System.IO.DirectoryInfo watchDirectory = new System.IO.DirectoryInfo(appProps[DIRECTORYWATCHER]);

                //Creating archive directory path
                string archiveDirPathString = System.IO.Path.Combine(appProps[DIRECTORYWATCHER], "archive");

                //Checking if archive directory exists
                if (!System.IO.Directory.Exists(archiveDirPathString))
                {
                    //if not creating a new directory
                    System.IO.Directory.CreateDirectory(archiveDirPathString);
                }

                // infinte loop to keep looking for files in a directory.
                System.IO.FileInfo[] fileList = watchDirectory.GetFiles();
                if (fileList != null && fileList.Length > 0)
                {
                    //System.Console.WriteLine("Found files to transfer");
                    foreach (System.IO.FileInfo file in fileList)
                    {
                        retrycount = 0;


                        //LogParam("file name:" , file.FullName);
                        if (System.IO.File.Exists(file.FullName) &&( file.FullName.Contains(".csv") || (file.FullName.Contains(".txt"))))
                        {
                            while (true)
                            {
                                // CHeck if file is still locked 
                                if (!FileIsLocked(file.FullName))
                                {
                                    // Check if it is making sense to send the file
                                    // By comparing Merchant id

                                    if (pycrmServerCheckForGoodFileName(file.Name) == false)
                                    {
                                        Log(" Rougue File Name Found Deleting it : " + file.FullName);
                                        System.IO.File.Delete(file.FullName);
                                        break;
                                    }

                                    System.Threading.Thread.Sleep(1000);
                                    // Check if Merchant Id of 

                                    blobTransfer(file.FullName, file.Name);


                                    // if transfer is successful copy file to archive
                                    string archiveFilePathString = System.IO.Path.Combine(archiveDirPathString, file.Name);

                                    // Copy file from main folder to archive and then delete
                                    // earlier we tried Move option but since it doesn't have override option trying Copy and delete
                                    System.IO.File.Copy(file.FullName, archiveFilePathString, true);
                                    System.IO.File.Delete(file.FullName);
                                    break;
                                }
                                else
                                {
                                    Log(" File is Locked going to sleep and retry later\n");
                                    System.Threading.Thread.Sleep(2000);
                                    retrycount++;
                                    // max 30 seconds of wait, otherwise come out
                                    if (retrycount > 15)
                                    {
                                        break;
                                    }
                                }

                            }


                        }
                    }
                }
                else
                {
                    Log("No Files ");
                }
            }
            catch (System.Exception e)
            {
               Log("Exception in transferFilesToAzure, Not able to send file to Azure. message: " + e.Message);
            }
        }
        public bool FileIsLocked(string strFullFileName)
        {
            bool blnReturn = false;
            System.IO.FileStream fs;
            try
            {
                fs = System.IO.File.Open(strFullFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                fs.Close();
            }
            catch (System.IO.IOException ex)
            {
                blnReturn = true;
            }
            return blnReturn;
        }

        private int RETRY_TRANSFER = 0;
        /// <summary>
        /// This method transfer file to Azure client
        /// </summary>
        /// <param name="fileName"></param>
        private void blobTransfer(String fullFilePath, String fileName)
        {
            try
            {
                if (System.IO.File.Exists(fullFilePath))
                {
                    if (container == null)
                    {
                        prserverReadConfigFile();

                    }
                    string sasUri = STORAGENAME + CONTAINER_NAME + "/" + fileName + BLOBSASTOKEN;
                    var cloudBlockBlob = new CloudBlockBlob(new Uri(sasUri));


                    using (var fileStream = System.IO.File.OpenRead(fullFilePath))
                    {
                        cloudBlockBlob.UploadFromStream(fileStream);
                    }

                    queueMsgPush(fileName);

                    LogParam("File sent SUCCESSFULLY, file name:", fullFilePath);
                }
                else
                {
                    LogParam("File doens't exist, file name:", fullFilePath);
                }
                RETRY_TRANSFER = 0;

            }
            catch (Microsoft.WindowsAzure.Storage.StorageException e)
            {
                LogParam("exception in blobTransfer, e:", e.Message);
                if (RETRY_TRANSFER < 5)
                {
                    // retry transfer
                    Log("retrying sending file to cloud:" + fullFilePath);
                    RETRY_TRANSFER++;
                    blobTransfer(fullFilePath, fileName);
                }
                else
                {
                    RETRY_TRANSFER = 0;


                }
            }

        }

    }

    class StringCipher
    {
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }
        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }

}
