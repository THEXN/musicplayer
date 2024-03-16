using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TShockAPI;

namespace MusicPlayer.Music
{
    internal class NoteFileParser
    {
        public static List<List<Note>> Read(string path, out int tempo)
        {
            List<List<Note>> list = new List<List<Note>>();
            int num = Tempo;
            using (StreamReader streamReader = new StreamReader(path))
            {
                bool isHeaderRead = false;
                string text;
                while ((text = streamReader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(text) && !text.Trim().StartsWith('#'))
                    {
                        if (!isHeaderRead)
                        {
                            if (int.TryParse(text, out num))
                            {
                                isHeaderRead = true;
                            }
                        }
                        else
                        {
                            List<Note> noteList = new List<Note>();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                foreach (string noteNameText in text.Split(',', StringSplitOptions.None).ToList())
                                {
                                    try
                                    {
                                        float noteValue = NoteName.GetNoteByName(noteNameText);
                                        noteList.Add(new Note
                                        {
                                            Value = noteValue
                                        });
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        Console.Error.WriteLine("错误的读取: {0}", noteNameText);

                                    }
                                }
                            }
                            list.Add(noteList);
                        }
                    }
                }
            }
            tempo = num;
            return list;
        }


        public static int Tempo = 250;
    }

}
