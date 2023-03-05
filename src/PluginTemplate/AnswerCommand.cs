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
    [RequirePermission("rod.user")]
    internal class AnswerCommand : TSModuleBase<TSCommandContext>
    {
        PrizesSettings config = Configuration<PrizesSettings>.Settings;

        [Command("ans")]
        [Description("The command users use to answer chat games")]
        public IResult Answer(string answer = "")
        {
            var cg = Prizes.cg;
            
            TSPlayer player = Context.Player;

            if (!cg.Occuring)
            {
                return Error("There is no chat game occuring!");
            }
            
            if(answer == "")
            {
                return Error("Please enter an answer!");
            }

            if (cg.wordAnswer == "" && cg.answer == int.Parse(answer)) 
            {
                    foreach (string cmd in config.CommandsOnChatGameWin)
                    {
                        string newCmd = cmd.Replace("%PLAYER%", '"' + player.Name + '"');
                        newCmd = newCmd.Replace("%ACCOUNT%", '"' + player.Account.Name + '"');

                        Commands.HandleCommand(TSPlayer.Server, newCmd);
                    }
                    cg.Occuring = false;
                    cg.wordAnswer = "";
                    cg.answer = 0;
                    return Announce("[Chat Games] [c/3480eb:"+ player.Name +"] has won! the chat game Hooray!. Answer was [c/eb343d:" + answer + "]", Color.Green);
            }
            else if(cg.wordAnswer != "" && cg.wordAnswer.ToLower()==answer.ToLower())
            {
                foreach (string cmd in config.CommandsOnChatGameWin)
                {
                    string newCmd = cmd.Replace("%PLAYER%", '"' + player.Name + '"');
                    newCmd = newCmd.Replace("%ACCOUNT%", '"' + player.Account.Name + '"');

                    Commands.HandleCommand(TSPlayer.Server, newCmd);
                }
                cg.Occuring = false;
                cg.wordAnswer = "";
                cg.answer = 0;
                return Announce("[Chat Games] [c/3480eb:"+ player.Name +"] has won! the chat game Congratz!. Answer was [c/eb343d:" + answer + "]", Color.Green);
            }
            else
            {
                return Error("Incorrect! You entered: " + answer);
            }
        }
    }
}
