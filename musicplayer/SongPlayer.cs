using System.Diagnostics.CodeAnalysis;
using MusicPlayer.Music;
using TShockAPI;

namespace MusicPlayer
{
    internal class SongPlayer
    {
        private bool listening;
        [MemberNotNullWhen(true, nameof(currentSong))]
        public bool Listening { get => listening; }
        public TSPlayer Player { get; set; }
        public PlaySongInfo? currentSong { get; private set; }

        public SongPlayer(TSPlayer ply)
        {
            Player = ply;
            listening = false;
        }

        public bool StartSong(PlaySongInfo? playSongInfo = null)
        {
            listening = true;
            if (playSongInfo is null)
            {
                if(currentSong is null)
                {
                    return false;
                }
                currentSong.Play();
                return true;
            }
            currentSong = playSongInfo;
            playSongInfo.Play();
            return true;
        }

        public bool EndSong()
        {
            listening = false;
            if(currentSong is null)
            {
                return false;
            }
            currentSong.Stop();
            return true;
        }
    }
}
