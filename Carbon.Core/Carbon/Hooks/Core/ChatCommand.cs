﻿using Carbon.Core;
using Carbon.Core.Extensions;
using ConVar;
using Facepunch.Extend;
using Harmony;
using System;
using System.Linq;

[HarmonyPatch ( typeof ( Chat ), "sayAs" )]
public class Chat_SayAs
{
    public static bool Prefix ( Chat.ChatChannel targetChannel, ulong userId, string username, string message, BasePlayer player = null )
    {
        if ( CarbonCore.Instance == null ) return true;

        try
        {
            var fullString = message.Substring ( 1 );
            var split = fullString.Split ( ConsoleArgEx.CommandSpacing, StringSplitOptions.RemoveEmptyEntries );
            var command = split [ 0 ].Trim ();
            var args = split.Length > 1 ? fullString.Substring ( command.Length + 1 ).SplitQuotesStrings () : null;
            Facepunch.Pool.Free ( ref split );

            foreach ( var cmd in CarbonCore.Instance?.AllChatCommands )
            {
                if ( cmd.Command == command )
                {
                    try
                    {
                        cmd.Callback?.Invoke ( player, command, args );
                    }
                    catch ( Exception ex )
                    {
                        CarbonCore.Error ( "ConsoleSystem_Run", ex );
                    }

                    return false;
                }
            }
        }
        catch { }

        return true;
    }
}