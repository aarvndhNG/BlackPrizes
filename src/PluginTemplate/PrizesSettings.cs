using Auxiliary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prizes
{
    public class PrizesSettings : ISettings
    {
        [JsonPropertyName("CommandsOnVote")]
        public List<string> CommandsOnVote { get; set; } = new List<string>() { };

        [JsonPropertyName("APIKey")]
        public string APIKey { get; set; } = "xxx";

        [JsonPropertyName("OnVoteMessage")]
        public string OnVoteMessage { get; set; } = "[Vote Rewards] %PLAYER% has voted for us and receieved a reward. Use /vote to get the same reward!";

        [JsonPropertyName("MustBeLoggedInMessage")]
        public string MustBeLoggedInMessage { get; set; } = "You must be logged in to use this command!";

        [JsonPropertyName("VoteAlreadyClaimedMessage")]
        public string VoteAlreadyClaimedMessage { get; set; } = "You have already claimed your reward for today!";

        [JsonPropertyName("HaventVotedMessage")]
        public string HaventVotedMessage { get; set; } = "You haven't voted today! Head to terraria-servers.com and vote for our server page!";

        [JsonPropertyName("ChatGamesEnabled")]
        public bool ChatGamesEnabled { get; set; } = false;

        [JsonPropertyName("CommandsOnChatGameWin")]
        public List<string> CommandsOnChatGameWin { get; set; } = new List<string>() { };

        [JsonPropertyName("ChatGamesTimer")]
        public int ChatGamesTimer { get; set; } = 3;


    }
}
