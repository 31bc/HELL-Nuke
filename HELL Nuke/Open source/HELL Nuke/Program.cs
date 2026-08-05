
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using HELL_Nuke; 


var services = new ServiceCollection()
    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.All,
    }))
   
    .AddSingleton<HellstormCore>()
    .BuildServiceProvider();

var hellstormCore = services.GetRequiredService<HellstormCore>();


await hellstormCore.Engage();