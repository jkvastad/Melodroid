using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System.Text;

using Fractions;

namespace MusicTheory
{
    public class Phrase
    {
        public List<Measure> Measures { get; }
        public Phrase(List<Measure> measures)
        {
            Measures = measures;
        }
    }

    public class Measure
    {
        public int TimeDivision { get; }
        //TODO add support for simultaneous note values. Simply convert to array of lists? How does MIDI handle multiple simultaneous notes with note on/off?
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

        public static NoteValue SilentNote = new NoteValue(NoteName.A, 4, 0);
        public NoteValue(NoteName name, int octave, int velocity)
        {
            Name = name;
            Octave = octave;
            Velocity = velocity;
        }

        public static NoteValue operator +(NoteValue a, int midiInterval)
        {
            SevenBitNumber noteNumber = (SevenBitNumber)(NoteUtilities.GetNoteNumber(a.Name, a.Octave) + midiInterval);
            return new(NoteUtilities.GetNoteName(noteNumber), NoteUtilities.GetNoteOctave(noteNumber), a.Velocity);
        }

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
            short songTimeDivision = (short)MusicTheoryUtils.LCM(_measures.Select((measure, index) => (long)measure.TimeDivision).ToArray());
            int totalMidiTicks = 0;
            NoteValue currentNoteValue = NoteValue.SilentNote;
            int currentNoteStart = totalMidiTicks;

            foreach (var measure in _measures)
            {
                foreach (NoteValue? noteValue in measure.NoteValues) //TODO how to handle multiple simultaneous note values? Hash name and octave instead of a single "current note"?
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
    }

    public static class MusicTheoryExtensions
    {
        static Random random = new Random();
        public static T TakeRandom<T>(this IEnumerable<T> list)
        {
            return list.ElementAt(random.Next(list.Count()));
        }

        public static List<T> TakeRandom<T>(this IEnumerable<T> list, int uniqueElements)
        {
            HashSet<T> result = new();
            if (uniqueElements > list.Count() || uniqueElements <= 0)
                throw new ArgumentException("Must take more than 0 and less than list count random elements");
            while (result.Count < uniqueElements)
            {
                result.Add(list.ElementAt(random.Next(list.Count())));
            }
            return result.ToList();
        }

        public static int[] OctaveTransposed(this int[] tet12Keys)
        {
            List<int> transposed12TetKeys = new();
            foreach (int key in tet12Keys)
            {
                int transposedKey = key;
                while (transposedKey < 0)
                    transposedKey += 12;
                while (transposedKey >= 12)
                    transposedKey -= 12;
                transposed12TetKeys.Add(transposedKey);
            }
            return transposed12TetKeys.ToArray();
        }

        public static Fraction OctaveTransposed(this Fraction fraction)
        {
            while (fraction < 1)
                fraction *= 2;
            while (fraction >= 2)
                fraction /= 2;
            return fraction;
        }
    }
}