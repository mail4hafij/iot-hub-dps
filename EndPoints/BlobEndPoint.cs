using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EndPoints
{
    public static class BlobEndPoint
    {
        [FunctionName("BlobEndPoint")]
        public static void Run([BlobTrigger("YOUR_BLOB_CONTAINER_NAME/{name}", Connection = "StorageAccountConnection")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"BlobEndPoint: {name} \n Size: {myBlob.Length} Bytes \n");
        }
    }
}
