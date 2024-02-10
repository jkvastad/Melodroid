using Melanchall.DryWetMidi.MusicTheory;
using System.Text;

namespace MusicTheory
{
    public class Phrase
    {
        public List<BeatMeasure> Measures { get; } = new();
    }

    public class BeatMeasure
    {
        public int TimeDivision { get; }
        //Max velocity 127, min 0
        public int?[] NoteVelocities { get; }
        public BeatMeasure(int timeDivision)
        {
            TimeDivision = timeDivision;
            NoteVelocities = new int?[timeDivision];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NoteVelocities.Length; i++)
            {
                int? noteVelocity = NoteVelocities[i];
                if (noteVelocity == null)
                {
                    sb.Append("-");
                    continue;
                }
                else
                {
                    sb.Append(noteVelocity);
                }
            }
            return sb.ToString();
        }
    }

    public class HarmonicMeasure
    {
        public int TimeDivision { get; }
        public NoteValue?[] NoteValues { get; }
        public HarmonicMeasure(int timeDivision)
        {
            TimeDivision = timeDivision;
            NoteValues = new NoteValue?[TimeDivision];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NoteValues.Length; i++)
            {
                NoteValue? noteValue = NoteValues[i];
                if (noteValue is NoteValue valueOfNote)
                {
                    sb.Append(valueOfNote.Name);
                    sb.Append(valueOfNote.Octave);
                }
                else
                {
                    sb.Append("-");
                    continue;
                }
            }
            return sb.ToString();
        }
    }

    public readonly struct NoteValue
    {
        public NoteName Name { get; }
        public int Octave { get; }
        public NoteValue(NoteName name, int octave)
        {
            
            Name = name;
            Octave = octave;
        }
    }
}