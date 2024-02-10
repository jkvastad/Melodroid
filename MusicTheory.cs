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
        //Max velocity 127, min 0
        public int?[] NoteVelocities { get; }
        public Measure(int timeDivision)
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
}