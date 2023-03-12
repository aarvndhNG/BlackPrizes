using IL.Terraria.Properties;
using Microsoft.Xna.Framework;
using Prizes.Models;
using Prizes.Modules;
using System;
using System.Data;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Prizes
{

    [ApiVersion(2, 1)]
    public class Prizes : TerrariaPlugin
    {
        private Timer _chatGames;
        private readonly TSCommandFramework _fx;
        private static PrizesSettings config;
        public static ChatGame cg;
        public override string Name => "Prizes";

        public override Version Version => new Version(1, 0, 0);

        public override string Author => "Average,Blackwolf";
        public override string Description => "A plugin intended for TBC, implementing interactive chat games and TSL vote rewards!";

        public Prizes(Main game) : base(game)
        {
            _fx = new(new()
            {
                DefaultLogLevel = CSF.LogLevel.Warning,
            });
        }
        public async override void Initialize()
        {
            Configuration<PrizesSettings>.Load("Prizes");
            config = Configuration<PrizesSettings>.Settings;

            if (config.APIKey == "xxx")
            {
                Console.WriteLine("Prizes: You should probably set your api key in Prizes.json in your tShock folder! If not, whatevs, you do you b.");
            }

            GeneralHooks.ReloadEvent += (x) =>
            {
                Configuration<PrizesSettings>.Load("Prizes");
                config = Configuration<PrizesSettings>.Settings;
                x.Player.SendSuccessMessage("[Prizes] has been reloaded!",Color.Green);
            };
            
            TerrariaApi.Server.ServerApi.Hooks.NetGreetPlayer.Register(this, GreetPlayer);

            #region Chat Games Timer initialization
            if (config.ChatGamesEnabled == true)
            {
                cg = new ChatGame();
                _chatGames = new(config.ChatGamesTimer*1000*60)
                {
                    AutoReset = true
                };
                _chatGames.Elapsed += async (_, x)
                    => await ChatGames(x);
                _chatGames.Start();
            }


            #endregion

            await _fx.BuildModulesAsync(typeof(Prizes).Assembly);
        }

        private void GreetPlayer(GreetPlayerEventArgs args)
        {
            TSPlayer Player = TShock.Players[args.Who];
            
            if(Player == null)
            {
                return;
            }
            if (!Player.IsLoggedIn)
            {
                return;
            }

            if (Prizes.checkifPlayerVoted(Player).Result == true)
            {
                if (Prizes.rewardClaimed(Player).Result == true)
                {
                    string rewardMsg = config.OnVoteMessage;
                    rewardMsg = rewardMsg.Replace("%PLAYER%", Player.Name);
                    foreach (string cmd in config.CommandsOnVote)
                    {
                        string newCmd = cmd.Replace("%PLAYER%", '"' + Player.Name + '"');
                        Commands.HandleCommand(TSPlayer.Server, newCmd);
                    }
                    TSPlayer.All.SendMessage(rewardMsg, Color.LightGreen);
                    return;
                }
                else
                {
                    return;
                }
              

            }

            Player.SendMessage(config.HaventVotedMessage, Color.LightGreen);
            return;
        }

        public async Task ChatGames(ElapsedEventArgs _)
        {
            if (TShock.Utils.GetActivePlayerCount() == 0)
                return;

            Random rand = new Random();
            string Oper = null;
            int gamemode = rand.Next(0, 5);
            string mathProblem = null;
            string wordProblem = null;
            int answer = 0;
            DataTable eval = new DataTable();

            switch (gamemode)
            {
                case 1:
                    cg.wordAnswer = "";
                    Oper = "-";
                    mathProblem = "" + rand.Next(1, 100) + Oper + rand.Next(1, 150);
                    answer = (int)eval.Compute(mathProblem, "");
                    cg.answer = answer;
                    break;
                case 2:
                    cg.wordAnswer = "";
                    Oper = "+";
                    mathProblem = rand.Next(1, 100) + Oper + rand.Next(1, 150);
                    answer = (int)eval.Compute(mathProblem, "");
                    cg.answer = answer;

                    break;
                case 3:
                    cg.wordAnswer = "";
                    Oper = "*";
                    mathProblem = rand.Next(1, 12) + Oper + rand.Next(1, 12);
                    answer = (int)eval.Compute(mathProblem, "");
                    cg.answer = answer;
                    break;
                case 4:
                    var problem = rand.Next(0, WordList.list.Length);
                    wordProblem = WordList.list[problem];
                    cg.wordAnswer = wordProblem;
                    break;
                default:
                    cg.wordAnswer = "";
                    Oper = "+";
                    mathProblem = rand.Next(1, 100) + Oper + rand.Next(1, 150);
                    answer = (int)eval.Compute(mathProblem, "");
                    cg.answer = answer;
                    break;
            }
            cg.Occuring = true;


            if (wordProblem != null)
            {
                TSPlayer.All.SendMessage("[Chat Games]  Unscramble this word problem and receive your rewards! To answer the problem use [c/4ed4c6:/ans]: " + ScrambleWord(wordProblem), Color.Gold);
            }
            else
            {
                cg.wordAnswer = "";
                TSPlayer.All.SendMessage("[Chat Games] Answer this math problem and receive your rewards! To answer the problem use [c/4ed4c6:/ans]: " + mathProblem, Color.Gold);
            }


        }

        public string ScrambleWord(string word)
        {
            char[] chars = new char[word.Length];
            Random rand = new Random(10000);

            int index = 0;

            while (word.Length > 0)
            {
                // Get a random number between 0 and the length of the word.
                int next = rand.Next(0, word.Length - 1);

                // Take the character from the random position and add to our char array.
                chars[index] = word[next];

                // Remove the character from the word.
                word = word.Substring(0, next) + word.Substring(next + 1);

                ++index;
            }

            return new string(chars);
        }

        public static async Task<bool> rewardClaimed(TSPlayer player)
        {
            bool hasVoted = false;

            string voteUrl = "http://terraria-servers.com/api/?action=post&object=votes&element=claim&key=" + config.APIKey + "&username=" + player.Name;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(voteUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            var data = await content.ReadAsStringAsync();

                            if (data != null)
                            {
                                if (data == "1")
                                {
                                    hasVoted = true;
                                    return hasVoted;
                                }
                                else
                                {
                                    hasVoted = false;
                                    return hasVoted;
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return hasVoted;
            }

            return hasVoted;

        }

        public static async Task<bool> checkifPlayerVoted(TSPlayer player)
        {
            bool hasVoted = false;

            string voteUrl = ($"http://terraria-servers.com/api/?object=votes&element=claim&key={config.APIKey}&username={player.Name}");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(voteUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            var data = await content.ReadAsStringAsync();

                            if (data != null)
                            {
                                if (data == "1" || data == "2")
                                {
                                    hasVoted = true;
                                    return hasVoted;
                                }
                                else
                                {
                                    hasVoted = false;
                                    return hasVoted;
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return hasVoted;
            }

            return hasVoted;

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TerrariaApi.Server.ServerApi.Hooks.NetGreetPlayer.Deregister(this, GreetPlayer);

            }
            base.Dispose(disposing);
        }
    }
}
