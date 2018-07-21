using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using lib.Utils;

using NUnit.Framework;

using xxHashSharp;

namespace tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void Test()
        {
            var tasks = ProblemSolutionFactory.GetTasks();

            int replicaCount = 40;
            int[] buckets = new int[replicaCount];
            foreach (var task in tasks)
            {
                //Console.Out.WriteLine(task.Problem.Name);
                //var b = xxHash.CalculateHash(Encoding.UTF8.GetBytes(task.Problem.Name + task.Solution.Name)) % replicaCount;
                var b = (uint)(task.Problem.Name + task.Solution.Name).GetHashCode() % replicaCount;
                buckets[b]++;
            }
            foreach (var b in buckets)
            {
                Console.Out.WriteLine(b);
            }
        }
    }
}