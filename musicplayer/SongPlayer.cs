using System;
using MusicPlayer.Music;
using TShockAPI;

namespace MusicPlayer
{
    internal class SongPlayer
    {
        public bool Listening { get; set; }
        public TSPlayer Player { get; set; }
        public Song currentSong = null;

        public SongPlayer(TSPlayer ply)
        {
            this.Player = ply;
            this.Listening = false;
        }

        public void StartSong(Song s)
        {
            if (this.currentSong != null)
            {
                this.currentSong.EndSong();
            }
            this.currentSong = s;

            // 只为执行命令的玩家设置 Player 属性
            s.Player = this.Player;
            s.StartSong();
        }

        public void StartSongAll(Song s)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player?.Active ?? false)
                {
                    s.Player = player; // 设置当前播放歌曲的玩家
                    s.StartSong(); // 开始播放歌曲
                }
            }
        }



        public void EndSong()
        {
            if (this.currentSong != null)
            {
                this.currentSong.EndSong();
            }
            this.currentSong = null;
        }
    }

}
