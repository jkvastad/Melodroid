using System;
using System.Text;

namespace MusicTheory
{
    public class Phrase
    {
        public List<Measure> Measures { get; } = new();
    }

    public class Measure
    {
        public int TimeDivision { get; }
        public NoteOnOff?[] Notes { get; }
        public Measure(int timeDivision)
        {
            TimeDivision = timeDivision;
            Notes = new NoteOnOff[timeDivision];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Notes.Length; i++)
            {
                NoteOnOff? noteOnOff = Notes[i];
                if (noteOnOff == null)
                {
                    sb.Append("-");
                    continue;
                }
                else
                {
                    if (noteOnOff.IsOn)
                        sb.Append($"{noteOnOff.Velocity}");
                    else
                        sb.Append("0");
                }
            }
            return sb.ToString();
        }
    }

    public class NoteOnOff
    {
        //On is true, off is false
        public bool IsOn { get; }
        public int Velocity { get; }
        public static NoteOnOff NoteOff { get; } = new(false, 0);
        public NoteOnOff(bool isOn, int velocity)
        {
            IsOn = isOn;
            Velocity = velocity;
        }
    }
}