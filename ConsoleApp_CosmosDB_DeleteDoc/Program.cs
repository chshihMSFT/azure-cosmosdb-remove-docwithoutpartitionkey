using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp_CosmosDB_DeleteDoc
{
    internal class Program
    {

        private static DocumentClient client;
        static void Main(string[] args)
        {            
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Please enter your account endpoint:");
            string AccountEndpoint = Console.ReadLine();
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Please enter your account key:");
            string AccountKey = Console.ReadLine();
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Please enter your database name:");
            string DBName = Console.ReadLine();
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Please enter your container name:");
            string ContainerName = Console.ReadLine();
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Please enter the document id you want to delete:");
            string DocId = Console.ReadLine();            

            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, ----------");
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, AccountEndpoint: {AccountEndpoint} ");
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Database Name: {DBName} ");
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Container Name: {ContainerName} ");
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, DocId: {DocId} ");
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Please confirm the information you input are correct? (Y/N)");

            string UserConfirm = Console.ReadLine();
            if (UserConfirm.ToUpper() == "Y")
            {
                ReadDocument(AccountEndpoint, AccountKey, DBName, ContainerName, DocId).Wait();
            }
            else
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Please execute the program again. Thanks.");
            }
        }

        private static async Task ReadDocument(string AccountEndpoint, string AccountKey, string DBName, string ContainerName, string DocId)
        {
            try
            {
                using (client = new DocumentClient(new Uri(AccountEndpoint), AccountKey, new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, UserAgentSuffix = "@DeleteDocWithoutPK" })) //UserAgentSuffix can help to identify the application execution from logs.
                {
                    var responseRead = await client.ReadDocumentAsync(
                            UriFactory.CreateDocumentUri(DBName, ContainerName, DocId),
                            new RequestOptions { PartitionKey = new PartitionKey(Undefined.Value) }
                            );

                    Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Document read by Id:");
                    Console.WriteLine(responseRead.Resource.ToString());

                    Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Is this the document you want to delete?");
                    string UserConfirmDelete = Console.ReadLine();
                    if (UserConfirmDelete.ToUpper() == "Y")
                    {
                        //save doc copy into local path
                        string SaveDocFolder = "C:\\temp\\";
                        string SaveDocFilename = String.Format($"{DocId}.saved.{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.json");
                        try
                        {
                            if (!System.IO.Directory.Exists(SaveDocFolder))
                            {
                                System.IO.Directory.CreateDirectory(SaveDocFolder);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Failed to save document copy in path: {SaveDocFolder}{SaveDocFilename} \r\n");
                            Console.ReadLine();
                            Environment.Exit(0);
                        }
                        finally
                        {
                            using (System.IO.StreamWriter file =
                                new System.IO.StreamWriter(SaveDocFolder + SaveDocFilename, true))
                            {

                                file.WriteLine(responseRead.Resource.ToString());

                            }
                            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Successfully saved document copy in path: {SaveDocFolder}{SaveDocFilename} \r\n");
                        }
                        
                        // delete doc
                        try
                        {
                            var responseDeletion = await client.DeleteDocumentAsync(
                                    UriFactory.CreateDocumentUri(DBName, ContainerName, DocId),
                                    new RequestOptions { PartitionKey = new PartitionKey(Undefined.Value) }
                                    );
                            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Document delete success.");
                        }
                        catch (Exception ce)
                        {
                            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Document delete failed: {ce.Message.ToString()}");
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Document read failed: {ce.Message.ToString()}");
            }
            finally
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}, Press any key to exit program.");
                Console.ReadLine();
            }
        }
    }
}
