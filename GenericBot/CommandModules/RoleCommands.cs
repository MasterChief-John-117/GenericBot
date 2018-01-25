﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GenericBot.Entities;
using LiteDB;

namespace GenericBot.CommandModules
{
    public class RoleCommands
    {
        public List<Command> GetRoleCommands()
        {
            List<Command> RoleCommands = new List<Command>();

            Command UserRoles = new Command("userroles");
            UserRoles.Description = $"Show all user roles on this server";
            UserRoles.Usage = "userroles";
            UserRoles.ToExecute += async (client, msg, paramList) =>
            {
                string prefix = (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                ? GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix : GenericBot.GlobalConfiguration.DefaultPrefix;
                string message = $"You can use `{prefix}iam` and `{prefix}iamnot` with any of these roles:\n";
                foreach (var role in msg.GetGuild().Roles
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id))
                    .OrderByDescending(r => r.Position))
                {
                    message += $"`{role.Name}`, ";
                }
                message = message.Trim(' ', ',');

                foreach (var str in message.SplitSafe())
                {
                    await msg.ReplyAsync(str);
                }
            };

            RoleCommands.Add(UserRoles);

            Command iam = new Command("iam");
            iam.Description = "Join a User Role";
            iam.Usage = "iam <role name>";
            iam.Aliases = new List<string>{"join"};
            iam.ToExecute += async (client, msg, paramList) =>
            {
                IMessage rep;
                if (paramList.Empty())
                {
                    rep = msg.ReplyAsync($"Please select a role to join").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
                }
                string input = paramList.Aggregate((i, j) => i + " " + j);

                var roles = msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(input.ToLower()))
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id));

                if (!roles.Any())
                {
                    rep = msg.ReplyAsync($"Could not find any user roles matching `{input}`").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
                }
                else if (roles.Count() == 1)
                {
                    try
                    {
                        RestUserMessage message;
                        if (msg.GetGuild().GetUser(msg.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                        {
                            message = await msg.ReplyAsync("You already have that role!");
                        }
                        else
                        {
                            await msg.GetGuild().GetUser(msg.Author.Id).AddRoleAsync(roles.First());
                            message = await msg.ReplyAsync("Done!");
                        }

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await message.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        await GenericBot.Logger.LogErrorMessage(e.Message);
                        await msg.ReplyAsync($"I may not have permissions to do that!");
                    }
                }
                else if (roles.Count() > 1)
                {
                    try
                    {
                        var role = roles.Any(r => r.Name.ToLower() == input.ToLower())
                            ? roles.First(r => r.Name.ToLower() == input.ToLower())
                            : roles.First();
                            RestUserMessage message;
                        if (msg.GetGuild().GetUser(msg.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                        {
                            message = await msg.ReplyAsync("You already have that role!");
                        }
                        else
                        {
                            await msg.GetGuild().GetUser(msg.Author.Id).AddRoleAsync(role);
                            message = await msg.ReplyAsync($"I've assigned you `{role.Name}`");
                        }

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await message.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        await GenericBot.Logger.LogErrorMessage(e.Message);
                        await msg.ReplyAsync($"I may not have permissions to do that!");
                    }                }
            };

            RoleCommands.Add(iam);

            Command iamnot = new Command("iamnot");
            iamnot.Description = "Leave a User Role";
            iamnot.Usage = "iamnot <role name>";
            iamnot.Aliases = new List<string>{"leave"};
            iamnot.ToExecute += async (client, msg, paramList) =>
            {
                IMessage rep;
                if (paramList.Empty())
                {
                    rep =  msg.ReplyAsync($"Please select a role to leave").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
                }
                string input = paramList.Aggregate((i, j) => i + " " + j);

                var roles = msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(input.ToLower()))
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id));

                if (!roles.Any())
                {
                    rep = msg.ReplyAsync($"Could not find any user roles matching `{input}`").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
                }
                else if (roles.Count() == 1)
                {
                    try
                    {
                        RestUserMessage message;
                        if (!msg.GetGuild().GetUser(msg.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                        {
                            message = await msg.ReplyAsync("You don't have that role!");
                        }
                        else
                        {
                            await msg.GetGuild().GetUser(msg.Author.Id).RemoveRoleAsync(roles.First());
                            message = await msg.ReplyAsync("Done!");
                        }

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await message.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        await GenericBot.Logger.LogErrorMessage(e.Message);
                        await msg.ReplyAsync($"I may not have permissions to do that!");
                    }
                }
                else if (roles.Count() > 1)
                {
                    try
                    {
                        RestUserMessage message;
                        if (!msg.GetGuild().GetUser(msg.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                        {
                            message = await msg.ReplyAsync("You don't have that role!");
                        }
                        else
                        {
                            await msg.GetGuild().GetUser(msg.Author.Id).RemoveRoleAsync(roles.First());
                            message = await msg.ReplyAsync($"Removed `{roles.First()}`");
                        }

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await message.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        await GenericBot.Logger.LogErrorMessage(e.Message);
                        await msg.ReplyAsync($"I may not have permissions to do that!");
                    }
                }
            };

            RoleCommands.Add(iamnot);

            Command getrole = new Command("getrole");
            getrole.Description = "Get the ID of a role";
            getrole.Usage = "getrole <role name>";
            getrole.RequiredPermission = Command.PermissionLevels.Moderator;
            getrole.ToExecute += async (client, msg, paramList) =>
            {
                string message = $"Roles matching `{paramList.reJoin()}`:\n";
                foreach (var role in msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(paramList.reJoin())))
                {
                    message += $"{role.Name} (`{role.Id}`)\n";
                }

                foreach (var str in message.SplitSafe())
                {
                    msg.ReplyAsync(str);
                }
            };

            RoleCommands.Add(getrole);

            Command membersOf = new Command("membersof");
            membersOf.Description = "List all members of a role";
            membersOf.Usage = "membersof <rolename>";
            membersOf.RequiredPermission = Command.PermissionLevels.Moderator;
            membersOf.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync($"You need to specify a role");
                    return;
                }
                string result = "";
                foreach (var role in msg.GetGuild().Roles.OrderByDescending(r => r.Position).Where(r => new Regex(parameters.reJoin(), RegexOptions.IgnoreCase).IsMatch(r.Name) && r.Name != "@everyone"))
                {
                    result += $"\n**`{role.Name}` ({role.Members.Count()} Members)**\n";
                    foreach (var user in role.Members.OrderBy(u => u.Username))
                    {
                        if (!string.IsNullOrEmpty(user.Nickname)) result += $"{user.Nickname} ";
                        else result += $"{user.Username} ";
                        result += $"(`{user}`)\n";
                    }
                }

                foreach (var str in result.SplitSafe('\n'))
                {
                    await msg.ReplyAsync(str);
                }

            };

            RoleCommands.Add(membersOf);

            Command createRole = new Command("createRole");
            createRole.Description = "Create a new role with default permissions";
            createRole.Usage = "createRole <name>";
            createRole.RequiredPermission = Command.PermissionLevels.Admin;
            createRole.ToExecute += async (client, msg, parameters) =>
            {
                RestRole role;
                bool makeMentionable = false;
                if (parameters[0].ToLower().Equals("+m"))
                {
                    parameters.RemoveAt(0);
                    makeMentionable = true;
                }

                role = msg.GetGuild().CreateRoleAsync(parameters.reJoin(), GuildPermissions.None).Result;
                await role.ModifyAsync(r => r.Mentionable = true);
                await msg.ReplyAsync($"Created new role `{role.Name}` with ID `{role.Id}`");
            };

            RoleCommands.Add(createRole);

            Command roleeveryone = new Command("roleeveryone");
            roleeveryone.Aliases = new List<string>{"roleveryone"};
            roleeveryone.Description = "Give or remove a role from everyone";
            roleeveryone.Usage = "roleveryone [+-] <roleID>";
            roleeveryone.RequiredPermission = Command.PermissionLevels.Admin;
            roleeveryone.ToExecute += async (client, msg, parameters) =>
            {
                if (!(parameters[0].Contains("+") || parameters[0].Contains("-")))
                {
                    await msg.ReplyAsync($"Invalid option `{parameters[0]}`");
                }
                ulong id;
                if (ulong.TryParse(parameters[1], out id) && msg.GetGuild().Roles.Any(r => r.Id == id))
                {
                    int i = 0;
                    await msg.GetGuild().DownloadUsersAsync();
                    var role = msg.GetGuild().GetRole(id);
                    foreach (var u in msg.GetGuild().Users)
                    {
                        if ( parameters[0].Contains("-") && u.Roles.Any(r => r.Id == id))
                        {
                            await u.RemoveRoleAsync(role);
                            i++;
                        }
                        if (parameters[0].Contains("+") && !u.Roles.Any(r => r.Id == id))
                        {
                            await u.AddRoleAsync(role);
                            i++;
                        }
                    }
                    string addrem = parameters[0].Contains("+") ? "Added" : "Removed";
                    string tofrom = parameters[0].Contains("+") ? "to" : "from";
                    await msg.ReplyAsync($"{addrem} `{role.Name}` {tofrom} `{i}` users.");
                }
                else await msg.ReplyAsync("Invalid Role Id");
            };

            RoleCommands.Add(roleeveryone);

            Command roleStore = new Command("roleStore");
            roleStore.Description = "Store your roles so you can restore them later";
            roleStore.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters[0].ToLower().Equals("save"))
                {
                    using (var db = new LiteDatabase(GenericBot.DBConnectionString))
                    {
                        DBUser dbUser;
                        var col = db.GetCollection<DBGuild>("userDatabase");
                        col.EnsureIndex(c => c.ID, true);
                        DBGuild guildDb;
                        if(col.Exists(g => g.ID.Equals(msg.GetGuild().Id)))
                            guildDb = col.FindOne(g => g.ID.Equals(msg.GetGuild().Id));
                        else guildDb = new DBGuild (msg.GetGuild());
                        dbUser = guildDb.Users.First(u => u.ID.Equals(msg.Author.Id));
                        List<ulong> roles = new List<ulong>();
                        foreach(var role in (msg.Author as SocketGuildUser).Roles)
                        {
                            roles.Add(role.Id);
                        }

                        dbUser.SavedRoles = roles;

                        await msg.ReplyAsync($"I've saved `{dbUser.SavedRoles.Count}` roles for you!");
                        col.Upsert(guildDb);
                        db.Dispose();
                    }

                }
                else if (parameters[0].ToLower().Equals("restore"))
                {
                    using (var db = new LiteDatabase(GenericBot.DBConnectionString))
                    {
                        DBUser dbUser;
                        var col = db.GetCollection<DBGuild>("userDatabase");
                        col.EnsureIndex(c => c.ID, true);
                        DBGuild guildDb;
                        if(col.Exists(g => g.ID.Equals(msg.GetGuild().Id)))
                            guildDb = col.FindOne(g => g.ID.Equals(msg.GetGuild().Id));
                        else guildDb = new DBGuild (msg.GetGuild());
                        if (guildDb.Users.Any(u => u.ID.Equals(msg.Author.Id))) // if already exists
                        {
                            dbUser = guildDb.Users.First(u => u.ID.Equals(msg.Author.Id));
                            if (dbUser.SavedRoles == null || !dbUser.SavedRoles.Any())
                            {
                                await msg.ReplyAsync("No roles saved. Try using `rolestore save` first!");
                                return;
                            }
                            int success = 0;
                            int fails = 0;
                            foreach (var id in dbUser.SavedRoles.Where(id => !(msg.Author as IGuildUser).RoleIds.Contains(id)))
                            {
                                try
                                {
                                    await (msg.Author as SocketGuildUser).AddRoleAsync(
                                        msg.GetGuild().Roles.First(r => r.Id.Equals(id)));
                                    success++;
                                }
                                catch (Exception e)
                                {
                                    fails++;
                                }
                            }
                            await msg.ReplyAsync($"`{success}` roles restored, `{fails}` failed");
                        }
                        else
                        {
                            dbUser = new DBUser(msg.Author as SocketGuildUser);
                            await msg.ReplyAsync("I don't have any saved roles for you");
                            return;
                        }
                        db.Dispose();
                    }
                }
                else
                {
                    await msg.ReplyAsync("Invalid option");
                }
            };

            RoleCommands.Add(roleStore);

            return RoleCommands;
        }
    }
}
