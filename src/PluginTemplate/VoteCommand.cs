using Auxiliary.Configuration;
using CSF;
using CSF.TShock;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Prizes.Modules
{
    [RequirePermission("tbc.user")]
    internal class VoteCommand : TSModuleBase<TSCommandContext>
    {
        PrizesSettings config = Configuration<PrizesSettings>.Settings;

        [Command("vote")]
        [Description("The command users use to claim vote rewards")]
        public IResult Vote()
        {
            if (!Context.Player.IsLoggedIn)
            {
                return Error(config.MustBeLoggedInMessage);
            }

            TSPlayer Player = Context.Player;

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
                    return Announce(rewardMsg, Color.LightGreen);           
                }
                else
                {
                    return Error(config.VoteAlreadyClaimedMessage);
                }

            }

            return Respond(config.HaventVotedMessage, Color.LightGreen);
        }
    }
}
