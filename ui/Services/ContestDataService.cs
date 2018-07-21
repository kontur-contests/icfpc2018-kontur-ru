using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using CsvHelper;

using JetBrains.Annotations;

using YamlDotNet.Serialization;

namespace ui.Services
{
    public class ContestDataService
    {
        private HttpClient client;

        private const string leaderBoardUrl = "https://raw.githubusercontent.com/icfpcontest2018/icfpcontest2018.github.io/master/_data/full_standings_live.csv";

        private const string teamNamesUrl = "https://raw.githubusercontent.com/icfpcontest2018/icfpcontest2018.github.io/master/_data/pubid_to_name.yaml";

        public ContestDataService([Optional] HttpClient client)
        {
            this.client = client ?? new HttpClient();
        }

        public async Task<IEnumerable<LeaderboardRecord>> GetLeaderBoard()
        {
            var leaderBoardStrings = await GetLeaderBoardCsv();
            var teamNamesStrings = await GetTeamNamesYml();

            var leaderboardRecords = ParseLeaderBoardStrings(leaderBoardStrings).ToArray();
            var teamNames = ParseTeamNamesStrings(teamNamesStrings);

            foreach (var leaderBoardRecord in leaderboardRecords)
            {
                leaderBoardRecord.Name = teamNames[leaderBoardRecord.PublicId];
            }

            return leaderboardRecords;
        }

        private async Task<string> GetLeaderBoardCsv()
        {
            using (var response = await client.GetAsync(leaderBoardUrl))
            using (var content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }

        private async Task<string> GetTeamNamesYml()
        {
            using (var response = await client.GetAsync(teamNamesUrl))
            using (var content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }

        private IEnumerable<LeaderboardRecord> ParseLeaderBoardStrings(string leaderBoardStrings)
        {
            return leaderBoardStrings
                .Trim()
                .Split('\n')
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new LeaderboardRecord
                    {
                        PublicId = x[0],
                        Timestamp = long.Parse(x[1]),
                        ProbNum = x[2],
                        Energy = long.Parse(x[3]),
                        Score = long.Parse(x[4])
                    });
        }

        private static Dictionary<string, string> ParseTeamNamesStrings(string teamNamesStrings)
        {
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<Dictionary<string, string>>(teamNamesStrings);
        }
    }
}