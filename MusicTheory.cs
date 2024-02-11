using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
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
        public NoteValue?[] NoteValues { get; }

        public Measure(NoteValue?[] noteValues)
        {
            NoteValues = noteValues;
            TimeDivision = noteValues.Length;
        }

        int stringPadding = 3; //max value 127

        public string NoteVelocitiesString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NoteValues.Length; i++)
            {
                int? noteVelocity = NoteValues[i]?.Velocity;
                if (noteVelocity == null)
                {
                    sb.Append("-".PadRight(stringPadding));
                    continue;
                }
                else
                {
                    sb.Append($"{noteVelocity}".PadRight(stringPadding));
                }
            }
            return sb.ToString();
        }

        public string NoteValuesString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NoteValues.Length; i++)
            {
                NoteValue? noteValue = NoteValues[i];
                if (noteValue is NoteValue valueOfNote)
                {
                    sb.Append($"{valueOfNote.Name}{valueOfNote.Octave}".PadRight(stringPadding));
                }
                else
                {
                    sb.Append("-".PadRight(stringPadding));

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
        public int Velocity { get; }
        public NoteValue(NoteName name, int octave, int velocity)
        {
            Name = name;
            Octave = octave;
            Velocity = velocity;
        }

        public static NoteValue SilentNote = new NoteValue(NoteName.A, 4, 0);

        public static bool operator ==(NoteValue a, NoteValue b)
        {
            if (a.Name == b.Name && a.Octave == b.Octave && a.Velocity == b.Velocity)
                return true;
            return false;
        }

        public static bool operator !=(NoteValue a, NoteValue b) => !(a == b);
    }

    public class NoteBuilder
    {
        List<Measure> _measures { get; }
        public short MidiTimeDivision { get; }
        public List<Melanchall.DryWetMidi.Interaction.Note> Notes { get; } = new();

        public NoteBuilder(List<Measure> measures)
        {
            _measures = measures;
            MidiTimeDivision = ParseNotes();
        }

        short ParseNotes()
        {
            short songTimeDivision = (short)LCM(_measures.Select((measure, index) => (long)measure.TimeDivision).ToArray());
            int totalMidiTicks = 0;
            NoteValue currentNoteValue = NoteValue.SilentNote;
            int currentNoteStart = totalMidiTicks;

            foreach (var measure in _measures)
            {
                foreach (NoteValue? noteValue in measure.NoteValues)
                {
                    if (noteValue != null)
                    {
                        if (currentNoteValue.Velocity > 0)
                            AddCurrentNote();
                        //Begin building next note
                        currentNoteValue = noteValue.Value;
                        currentNoteStart = totalMidiTicks;
                    }
                    //midi time division is per quarter note rather than per measure, thus 4 times higher resolution
                    int midiTicks = 4 * (songTimeDivision / measure.TimeDivision);
                    totalMidiTicks += midiTicks;
                }
            }
            //Add final note
            AddCurrentNote();
            //Pad with one division of silence, otherwise drywetmidi clips the last midi tick.
            if (currentNoteValue.Velocity > 0)
                Notes.Add(new(NoteName.A, 4)
                {
                    Time = totalMidiTicks,
                    Length = songTimeDivision,
                    Velocity = (SevenBitNumber)0
                });

            return songTimeDivision;

            void AddCurrentNote()
            {
                Notes.Add(new(currentNoteValue.Name, currentNoteValue.Octave)
                {
                    Time = currentNoteStart,
                    Length = totalMidiTicks - currentNoteStart,
                    Velocity = (SevenBitNumber)currentNoteValue.Velocity
                });
            }
        }
        //Thanks stack overflow https://stackoverflow.com/questions/147515/least-common-multiple-for-3-or-more-numbers/29717490#29717490
        static long LCM(long[] numbers)
        {
            long LcmResult = numbers.Aggregate(lcm);
            if (LcmResult > short.MaxValue)
                throw new ArgumentException($"Lcm result {LcmResult} is larger than short.MaxValue: This exceeds maximum midi time division");
            return LcmResult;
        }
        static long lcm(long a, long b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }
        static long GCD(long a, long b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }
    }
}