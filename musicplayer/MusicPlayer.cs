using MusicPlayer.Music;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MusicPlayer
{
    [ApiVersion(2, 1)]
	public class MusicPlayer : TerrariaPlugin
	{
        public override string Author => "Cjx适配，肝帝熙恩修改";

        public override string Description => "一个简单的音乐播放插件.";

        public override string Name => "音乐播放器";

        public override Version Version => new Version(1, 0, 0, 1);

        public string songPath;

        internal static SongPlayer?[] SongPlayers = new SongPlayer[255];

        private bool songPlayersIsAllNull;

        public MusicPlayer(Main game) : base(game)
        {
            songPath = Path.Combine(TShock.SavePath, "Songs");

            if (!Directory.Exists(songPath))
            {
                Directory.CreateDirectory(songPath);
            }
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("song", PlaySong, "song"));
            Commands.ChatCommands.Add(new Command("song2", PlaySongAll, "song2"));

            ServerApi.Hooks.NetGreetPlayer.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
            }
            base.Dispose(disposing);
        }

        private void OnJoin(GreetPlayerEventArgs args)
        {
            int who = args.Who;
            SongPlayers[who] = new SongPlayer(TShock.Players[who]); // 创建新的 SongPlayer 对象
        }

        private void OnLeave(LeaveEventArgs args)
        {
            SongPlayers[args.Who] = null; // 移除对应的 SongPlayer 对象
            songPlayersIsAllNull = Array.TrueForAll(SongPlayers, x => x is null);
        }
        public void OnUpdate(EventArgs args)
        {
            if (songPlayersIsAllNull)
            {
                return;
            }
            for(int i = 0; i < 255; i++)
            {
                if (SongPlayers[i] is null)
                {
                    continue;
                }
                var songPlayer = SongPlayers[i]!;
                if (!songPlayer.Listening)
                {
                    continue;
                }
                songPlayer.currentSong.Update(i);
            }
        }
        public void PlaySong(CommandArgs args)
        {
            if (!args.Player.RealPlayer)
            {
                args.Player.SendInfoMessage("此命令要求在游戏内使用");
                return;
            }
            var songPlayer = SongPlayers[args.Player.Index];
            if (songPlayer is null)
            {
                return;
            }
            const string invalidUsageMessage = "方式: /song \"歌曲名称\"";
            const string stopPlaybackMessage = "使用 /song ,来停止播放.";

            if (args.Parameters.Count == 0)
            {
                if (songPlayer.Listening)
                {
                    songPlayer.EndSong();
                }
                else
                {
                    args.Player.SendInfoMessage(invalidUsageMessage);
                    args.Player.SendInfoMessage(stopPlaybackMessage);
                }
            }
            else
            {
                string songName = args.Parameters[0];
                string filePath = Path.Combine(songPath, songName);

                if (File.Exists(filePath))
                {
                    var notes = NoteFileParser.Read(filePath, out var tempo);
                    songPlayer.StartSong(new PlaySongInfo(notes, tempo));
                    args.Player.SendInfoMessage("正在播放: {0}", songName); // 添加这条消息来提示正在播放
                }
                else
                {
                    args.Player.SendErrorMessage("加载歌曲失败: '{0}'", songName);
                }
            }
        }


        public void PlaySongAll(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                string songName = args.Parameters[0];
                string filePath = Path.Combine(songPath, songName);

                if (File.Exists(filePath))
                {
                    var notes = NoteFileParser.Read(filePath, out var tempo);
                    for (int i = 0; i < SongPlayers.Length; i++)
                    {
                        var songPlayer = SongPlayers[i];
                        if (songPlayer is not null)
                        {
                            songPlayer.StartSong(new PlaySongInfo(notes, tempo));
                            if (TShock.Players[i].Active)
                            {
                                TShock.Players[i].SendInfoMessage("正在给您播放: {0}，使用/song停止播放", songName);
                            }
                        }
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("加载歌曲失败: '{0}'", songName);
                }
            }
            else
            {
                // 如果没有提供歌曲名称，则停止所有玩家正在播放的歌曲
                foreach (var songPlayer in SongPlayers)
                {
                    if (songPlayer != null && songPlayer.Listening)
                    {
                        songPlayer.EndSong();
                    }
                }
            }
        }
	}
}
