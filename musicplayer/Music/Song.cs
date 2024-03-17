using IL.Terraria.ID;
using On.Terraria.ID;
using System;
using System.Collections.Generic;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace MusicPlayer.Music
{
    internal class Song
    {
        public string Name { get; set; }
        public TSPlayer Player { get; set; } // 添加 Player 属性
        public Song(TerrariaPlugin plugin, string name, string path, SongPlayer ply)
        {
            this.Name = name;
            this.notesToPlay = NoteFileParser.Read(path, out this.tempo);
            this.songLength = this.notesToPlay.Count;
            this.player = ply;
            this.plugin = plugin;
        }
        public void Update(EventArgs args)
        {
            if (this.isPlaying)
            {
                if (this.notesToPlay.Count == 0)
                {
                    this.EndSong();
                }
                else
                {
                    this.delta += (int)(DateTime.Now - this.lastUpdate).TotalMilliseconds;

                    if (this.delta > this.tempo)
                    {
                        List<Note> list = this.notesToPlay[0];
                        this.notesToPlay.RemoveAt(0);

                        foreach (Note note in list)
                        {
                            float? noteValue = note.Value;
                            if (noteValue >= -1f && noteValue <= 1f)
                            {
                                this.PlayNote(noteValue.Value);
                            }

                        }

                        this.delta -= this.tempo;
                    }

                    this.currentPlayTime += this.delta;
                    this.lastUpdate = DateTime.Now;
                }
            }
        }


        public void StartSong()
        {
            this.isPlaying = true;
            this.player.Listening = true;
            this.currentPlayTime = 0;
            this.lastUpdate = DateTime.Now;
            ServerApi.Hooks.GamePostUpdate.Register(this.plugin, new HookHandler<EventArgs>(this.Update));
        }
        public void EndSong()
        {
            this.isPlaying = false;
            ServerApi.Hooks.GamePostUpdate.Deregister(this.plugin, new HookHandler<EventArgs>(this.Update));
            this.player.Listening = false;
        }
        private void PlayNote(float note)
        {
            this.Player.SendData(PacketTypes.PlayHarp, "", this.Player.Index, note, 0f, 0f, 0); // 修改为 this.Player
        }
        private List<List<Note>> notesToPlay = new List<List<Note>>();
        private SongPlayer player;
        private int currentPlayTime = 0;
        private bool isPlaying = false;
        private TerrariaPlugin plugin;
        private int tempo = NoteFileParser.Tempo;

        private int songLength = 0;

        private int delta = 0;
        private DateTime lastUpdate = DateTime.Now;
    }
}
