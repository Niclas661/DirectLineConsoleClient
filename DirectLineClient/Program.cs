using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using System.Diagnostics;

namespace DirectLineClientConsole
{
    class Program
    {
        static Activity previousActivity = null;
        static Stopwatch stp = new Stopwatch();
        static string DirectLineSecret = "YOUR_DIRECT_LINE_API_SECRET";
        static string botId = "YOUR_BOT_ID";
        static void Main(string[] args)
        {
            StartConvo().Wait();
        }
        private static async Task StartConvo()
        {
            DirectLineClient client = new DirectLineClient(DirectLineSecret);
            

            var convo = await client.Conversations.StartConversationAsync();
            Console.WriteLine("SigmaBot - Direct Line API");
            Console.WriteLine("Conversation ID: " + convo.ConversationId);
            Console.WriteLine();

            new System.Threading.Thread(async () => await ReadBotMessagesAsync(client, convo.ConversationId)).Start();

            Console.Write("> ");

            while (true)
            {
                string input = Console.ReadLine().Trim();

                if(input.ToLower() == "exit")
                {
                    break;
                }
                //get info about previous activity in conversation
                else if(input.ToLower() == "activity info")
                {
                    if (previousActivity != null)
                    {
                        Console.WriteLine("-----------------------------------");
                        Console.WriteLine("From: " + previousActivity.From.Id);
                        Console.WriteLine("Time: " + previousActivity.LocalTimestamp);
                        Console.WriteLine("Id: " + previousActivity.Id);
                        Console.WriteLine("Conversation Id: " + previousActivity.Conversation.Id);
                        Console.WriteLine("Type: " + previousActivity.Type);
                        Console.WriteLine("Text: \n" + previousActivity.Text);
                        Console.WriteLine("-----------------------------------");
                        Console.WriteLine();
                        Console.Write("> ");
                    }
                    else
                    {
                        //There are no activities in the conversation in the beginning
                        Console.WriteLine("Can't do that!");
                        Console.WriteLine();
                        Console.Write("> ");
                    }
                }
                else
                {
                    if(input.Length > 0)
                    {
                        Activity act = new Activity
                        {
                            From = new ChannelAccount("DirectLineUser"),
                            Text = input,
                            Type = ActivityTypes.Message

                        };
                        stp.Start();
                        await client.Conversations.PostActivityAsync(convo.ConversationId, act);
                        Console.WriteLine("-----------------------------------");
                        Console.WriteLine("From: " + act.From.Id);
                        //cannot retrieve timestamp, is empty for now
                        Console.WriteLine("Time: " + act.LocalTimestamp);
                        Console.WriteLine("-----------------------------------");
                        Console.WriteLine();
                    }
                }
            }

        }
        private static async Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;

            while (true)
            {
                var actSet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
                watermark = actSet.Watermark;
                stp.Stop();
                
                var activities = from x in actSet.Activities
                                 where x.From.Id == botId
                                 select x;
                foreach(Activity ac in activities)
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("From: " + ac.From.Id);
                    Console.WriteLine("Time: " + ac.LocalTimestamp);
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine();
                    Console.WriteLine("< " + ac.Text);
                    Console.WriteLine();
                    Console.WriteLine("Sending and receiving a message took " + stp.ElapsedMilliseconds + "ms.");
                    Console.Write(">");
                    previousActivity = ac;
                }
                
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
