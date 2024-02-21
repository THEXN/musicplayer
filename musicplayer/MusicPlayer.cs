using System;
using System.IO;
using MusicPlayer.Music;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MusicPlayer
{
	[ApiVersion(2, 1)]
	public class MusicPlayer : TerrariaPlugin
	{
		public override string Author
		{
			get
			{
				return "Cjx适配";
			}
		}

		public override string Description
		{
			get
			{
				return "播放音乐.";
			}
		}
		public override string Name
		{
			get
			{
				return "Song Player";
			}
		}

		public override Version Version
		{
			get
			{
				return new Version(1, 0, 0, 0);
			}
		}

        public MusicPlayer(Main game) : base(game)
        {
            this.songPath = Path.Combine(TShock.SavePath, "Songs");

            if (!Directory.Exists(this.songPath))
            {
                Directory.CreateDirectory(this.songPath);
            }
        }


        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("", PlaySong, "song"));
            Commands.ChatCommands.Add(new Command("", PlaySongAll, "song2"));

            ServerApi.Hooks.NetGreetPlayer.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }
            base.Dispose(disposing);
        }

        private void OnJoin(GreetPlayerEventArgs args)
        {
            int who = args.Who;
            if (this.songPlayers[who] != null)
            {
                this.songPlayers[who].EndSong(); // 停止之前的音乐播放
            }
            this.songPlayers[who] = new SongPlayer(TShock.Players[who]); // 创建新的 SongPlayer 对象
        }


        private void OnLeave(LeaveEventArgs args)
        {
            int who = args.Who;
            this.songPlayers[who] = null; // 移除对应的 SongPlayer 对象
        }



        public void PlaySong(CommandArgs args)
        {
            if (this.songPlayers[args.Player.Index] != null)
            {
                string invalidUsageMessage = "方式: /song \"歌曲名称\"";
                string stopPlaybackMessage = "使用 /song ,来停止播放.";

                if (args.Parameters.Count == 0)
                {
                    if (!this.songPlayers[args.Player.Index].Listening)
                    {
                        args.Player.SendInfoMessage(invalidUsageMessage);
                        args.Player.SendInfoMessage(stopPlaybackMessage);
                    }
                    else
                    {
                        this.songPlayers[args.Player.Index].EndSong();
                    }
                }
                else
                {
                    string songName = args.Parameters[0];
                    string filePath = Path.Combine(this.songPath, songName);

                    if (!File.Exists(filePath))
                    {
                        args.Player.SendErrorMessage("加载歌曲失败: '{0}'", songName);
                    }
                    else
                    {
                        Song s = new Song(this, songName, filePath, this.songPlayers[args.Player.Index]);
                        this.songPlayers[args.Player.Index].StartSong(s);
                    }
                }
            }
        }

        public void PlaySongAll(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                // 如果没有提供歌曲名称，则停止所有玩家正在播放的歌曲
                foreach (var songPlayer in this.songPlayers)
                {
                    if (songPlayer != null && songPlayer.Listening)
                    {
                        songPlayer.EndSong();
                    }
                }
            }
            else
            {
                string songName = args.Parameters[0];
                string filePath = Path.Combine(this.songPath, songName);

                if (!File.Exists(filePath))
                {
                    args.Player.SendErrorMessage("加载歌曲失败: '{0}'", songName);
                }
                else
                {
                    // 停止所有玩家正在播放的歌曲
                    foreach (var songPlayer in this.songPlayers)
                    {
                        if (songPlayer != null && songPlayer.Listening)
                        {
                            songPlayer.EndSong();
                        }
                    }

                    // 开始播放新的歌曲，让所有玩家都听到
                    foreach (var songPlayer in this.songPlayers)
                    {
                        if (songPlayer != null)
                        {
                            Song s = new Song(this, songName, filePath, songPlayer);
                            songPlayer.StartSong(s);
                        }
                    }
                }
            }
        }





        public string songPath;

		private SongPlayer[] songPlayers = new SongPlayer[255];
	}
}
