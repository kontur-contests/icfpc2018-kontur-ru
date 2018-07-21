using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

using Ionic.Zip;

using lib;
using lib.ElasticDTO;
using lib.Utils;

using MoreLinq;

using Nest;

namespace packer
{
    class Program
    {
        private const string elasticUrl = "http://efk2-elasticsearch9200.efk2.10.217.14.7.xip.io";
        private const string elasticIndex = "testruns";

        static void Main(string[] args)
        {
            Console.WriteLine("Downloading solutions from Elastic...");
            var fileNames = DownloadSolutionsFromElastic();
            Console.WriteLine($"  {fileNames.Count} solutions");
            
            var secretKey = Environment.GetEnvironmentVariable("ICFPC2018_SECRET_KEY");
            var uploadToken = Environment.GetEnvironmentVariable("ICFPC2018_UPLOAD_TOKEN");

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(uploadToken))
                return;
            
            Console.WriteLine("Creating submission.zip...");
            CreateSubmissionZip(secretKey);

            Console.WriteLine("Calculating submission hash..");
            var hash = CalculateSubmissionHash();
            Console.WriteLine($"  {hash}");

            Console.WriteLine("Uploading submission.zip...");
            UploadSubmissionZip(uploadToken);

            Console.WriteLine("Getting download link...");
            var link = GetDownloadLink(uploadToken);
            
            Console.WriteLine("Submitting the solution...");
            var result = SubmitSolution(link, hash, secretKey);
            Console.WriteLine($"  {result}");
        }

        private static List<string> DownloadSolutionsFromElastic()
        {
            var client = new ElasticClient(new ConnectionSettings(new Uri(elasticUrl)).DefaultIndex(elasticIndex));

            var searchResponse = client.Search<TaskRunMeta>(
                s => s.RequestConfiguration(r => r.DisableDirectStreaming())
                      .Query(q => q.Term(p => p.Field(f => f.IsSuccess).Value(true)))
                      .Size(0)
                      .Aggregations(
                          aggs => aggs.Terms(
                              "task_name",
                              terms => terms
                                       .Field("taskName.keyword")
                                       .Size(1000)
                                       .Aggregations(
                                           childAggs => childAggs
                                               .Min("min_energy",
                                                    min => min
                                                        .Field("energySpent"))))));
            
            if (Directory.Exists(FileHelper.SolutionsDir))
                Directory.Delete(FileHelper.SolutionsDir, true);
            
            Directory.CreateDirectory(FileHelper.SolutionsDir);
            
            foreach (var traceFile in Directory.GetFiles(FileHelper.DefaultTracesDir))
                File.Copy(traceFile, Path.Combine(FileHelper.SolutionsDir, Path.GetFileName(traceFile)));

            var fileNames = new List<string>();
            
            foreach (var bucket in searchResponse.Aggregations.Terms("task_name").Buckets)
            {
                var taskName = bucket.Key;
                
                // Allow only solutions for the Full round
                if (!taskName.StartsWith("F")) continue;
                
                var energySpent = bucket.Min("min_energy").Value;

                var docSearchResponse = client.Search<TaskRunMeta>(
                    s => s
                             .Size(1)
                             .RequestConfiguration(r => r.DisableDirectStreaming())
                             .Query(q => q.Bool(b => b.Should(bs => bs.Term(p => p.Field("taskName.keyword").Value(taskName)),
                                                              bs => bs.Term(p => p.Field(f => f.EnergySpent).Value(energySpent))))));

                
                foreach (var document in docSearchResponse.Documents.Where(x => x.Solution != null))
                {
                    
                    var solutionBase64 = document.Solution;
                    var solutionContent = solutionBase64.SerializeSolutionFromString();
                    var fileName = $"{document.TaskName.Split('_')[0]}.nbt";
                    var targetSolutionPath = Path.Combine(FileHelper.SolutionsDir, fileName);
                    
                    var infoMessage = $"Saving solution for task '{document.TaskName}' to '{targetSolutionPath}'";
//                    Console.Error.WriteLine(infoMessage);
                    Log.For("packer").Info(infoMessage);
                    
                    fileNames.Add(fileName);
                    File.WriteAllBytes(targetSolutionPath, solutionContent);
                }
            }

            return fileNames;
        }

        private static void CreateSubmissionZip(string secretKey)
        {
            using (var zip = new ZipFile())
            {
                zip.Password = secretKey;

                foreach (var file in Directory.GetFiles(FileHelper.SolutionsDir))
                {
                    zip.AddFile(file, "");
                }

                zip.Save("submission.zip");
            }
        }
        
        private static string CalculateSubmissionHash()
        {
            var sha = SHA256.Create();
            var fileStream = new FileStream("submission.zip", FileMode.Open) {Position = 0};
            var hashValue = sha.ComputeHash(fileStream);
            fileStream.Close();
            return GetHashSha256(hashValue);
        }
        
        private static void UploadSubmissionZip(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("Dropbox-API-Arg", "{\"path\":\"/submission.zip\",\"mode\":{\".tag\":\"overwrite\"}}");

            var fileStream = new FileStream("submission.zip", FileMode.Open);
            var content = new StreamContent(fileStream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            var result = client.PostAsync("https://content.dropboxapi.com/2/files/upload", content).Result;
            fileStream.Close();
        }
        
        private static string GetDownloadLink(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent("{\"path\":\"/submission.zip\"}");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = client.PostAsync("https://api.dropboxapi.com/2/files/get_temporary_link", content).Result.Content.ReadAsStringAsync().Result;
            return result.Split('"').SkipLast(1).TakeLast(1).First();
        }

        private static string SubmitSolution(string link, string checksum, string secretKey)
        {
            var client = new HttpClient();

            var content = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("action", "submit"),
                    new KeyValuePair<string, string>("privateID", secretKey),
                    new KeyValuePair<string, string>("submissionURL", link),
                    new KeyValuePair<string, string>("submissionSHA", checksum),
                };
            var request = new HttpRequestMessage(HttpMethod.Post, "https://script.google.com/macros/s/AKfycbzQ7Etsj7NXCN5thGthCvApancl5vni5SFsb1UoKgZQwTzXlrH7/exec")
                {
                    Content = new FormUrlEncodedContent(content)
                };
            return client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
        }
        
        private static string GetHashSha256(byte[] array)
        {
            var builder = new StringBuilder();
            int i;
            for (i = 0; i < array.Length; i++)
            {
                builder.Append($"{array[i]:X2}");
            }
            return builder.ToString();
        }
    }
}