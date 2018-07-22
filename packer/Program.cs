using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var secretKey = Environment.GetEnvironmentVariable("ICFPC2018_SECRET_KEY");
            var uploadToken = Environment.GetEnvironmentVariable("ICFPC2018_UPLOAD_TOKEN");

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(uploadToken))
            {
                Console.WriteLine("Secret key or upload token missing");
                Environment.Exit(4000);
            }
            
            Console.WriteLine("Downloading solutions from Elastic...");
            var elasticStats = DownloadSolutionsFromElastic();
            Console.WriteLine($"  {elasticStats.Item1} solutions, {elasticStats.Item2} errors");

            Console.WriteLine("Creating submission ZIP...");
            var fileName = CreateSubmissionZip(secretKey);
            Console.WriteLine($"  {fileName}");

            Console.WriteLine("Calculating submission hash...");
            var hash = CalculateSubmissionHash(fileName);
            Console.WriteLine($"  {hash}");

            Console.WriteLine("Uploading submission ZIP...");
            UploadSubmissionZip(fileName, uploadToken);

            Console.WriteLine("Getting download link...");
            var link = GetDownloadLink(fileName, uploadToken);
            Console.WriteLine($"  {link}");

            Console.WriteLine("Submitting the solution...");
            var result = SubmitSolution(link, hash, secretKey);
            Console.WriteLine($"  {result}");

            if (result.Contains("failure"))
            {
                Console.WriteLine("Got failure result from judge");
                Environment.Exit(5000);
            }
            
            Environment.Exit(elasticStats.Item2);
        }

        private static Tuple<int, int> DownloadSolutionsFromElastic()
        {
            var client = new ElasticClient(
                new ConnectionSettings(new Uri(elasticUrl))
                    .DefaultIndex(elasticIndex)
                    .DisableDirectStreaming()
                );

            var searchResponse = client.Search<TaskRunMeta>(
                s => s.Query(q => q.Term(p => p.Field(f => f.IsSuccess).Value(true)))
                      .Size(0)
                      .Aggregations(
                          aggs => aggs.Terms(
                              "task_name",
                              terms => terms.Field("taskName.keyword")
                                            .Size(10000)
                                            .Aggregations(
                                                childAggs => childAggs.Min(
                                                    "min_energy",
                                                    min => min.Field("energySpent"))))));

            if (Directory.Exists(FileHelper.SolutionsDir))
                Directory.Delete(FileHelper.SolutionsDir, true);

            Directory.CreateDirectory(FileHelper.SolutionsDir);

            foreach (var traceFile in Directory.GetFiles(FileHelper.DefaultTracesDir))
                File.Copy(traceFile, Path.Combine(FileHelper.SolutionsDir, Path.GetFileName(traceFile)));

            var errorsCount = 0;
            var successCount = 0;
            foreach (var bucket in searchResponse.Aggregations.Terms("task_name").Buckets)
            {
                var taskName = bucket.Key;

                if (!taskName.StartsWith("F"))
                {
                    Console.WriteLine($"Skipping task {taskName}, it is not a task from Full round");
                    continue;
                }

                if (taskName.EndsWith("tgt"))
                {
                    Console.WriteLine($"Skipping task {taskName}, it has old-style name with _tgt");
                    continue;
                }

                var energySpent = bucket.Min("min_energy").Value;

                if (energySpent < 1)
                {
                    Console.WriteLine($"Skipping task {taskName} with ERROR: energySpent cannot be zero");
                    errorsCount++;
                    continue;
                }

                var docSearchResponse = client.Search<TaskRunMeta>(
                    s => s.Size(1)
                          .Query(q => q.Bool(b => b.Filter(bs => bs.Term(t => t.Field("taskName.keyword").Value(taskName)),
                                                           bs => bs.Term(t => t.Field(fi => fi.EnergySpent).Value(energySpent))))));

                if (docSearchResponse.Documents.Count == 0)
                {
                    Console.WriteLine($"Skipping task {taskName} with ERROR: bucket exists, but no documents found");
                    errorsCount++;
                    continue;
                }
                
                var document = docSearchResponse.Documents.First();

                if (taskName != document.TaskName)
                {
                    Console.WriteLine($"Skipping task {taskName} with ERROR: wrong document with TaskName {document.TaskName} found");
                    errorsCount++;
                    continue;
                }
                
                if (document.Solution == null)
                {
                    Console.WriteLine($"Skipping task {taskName} with ERROR: document found, but solution is empty");
                    errorsCount++;
                    continue;
                }

                var solutionBase64 = document.Solution;
                var solutionContent = solutionBase64.SerializeSolutionFromString();
                var fileName = $"{document.TaskName}.nbt";
                var targetSolutionPath = Path.Combine(FileHelper.SolutionsDir, fileName);

                Console.WriteLine($"Saving solution for task {document.TaskName} to {targetSolutionPath}");

                successCount++;
                File.WriteAllBytes(targetSolutionPath, solutionContent);
            }

            return new Tuple<int, int>(successCount, errorsCount);
        }

        private static string CreateSubmissionZip(string secretKey)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string fileName = $"submission-{timestamp}.zip";
            
            using (var zip = new ZipFile())
            {
                zip.Password = secretKey;

                foreach (var file in Directory.GetFiles(FileHelper.SolutionsDir))
                {
                    zip.AddFile(file, "");
                }

                zip.Save(fileName);
            }

            return fileName;
        }

        private static string CalculateSubmissionHash(string fileName)
        {
            var sha = SHA256.Create();
            var fileStream = new FileStream(fileName, FileMode.Open) {Position = 0};
            var hashValue = sha.ComputeHash(fileStream);
            fileStream.Close();
            return GetHashSha256(hashValue);
        }

        private static bool UploadSubmissionZip(string fileName, string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("Dropbox-API-Arg", "{\"path\":\"/" + fileName + "\",\"mode\":{\".tag\":\"overwrite\"}}");

            var fileStream = new FileStream(fileName, FileMode.Open);
            var content = new StreamContent(fileStream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            var result = client.PostAsync("https://content.dropboxapi.com/2/files/upload", content).Result.Content.ReadAsStringAsync().Result;
            fileStream.Close();
            return result.Contains(fileName);
        }

        private static string GetDownloadLink(string fileName, string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent("{\"path\":\"/" + fileName + "\"}");
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