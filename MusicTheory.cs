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
        public Dictionary<int, int>?[] Velocities { get; } //array position is time division steps in measure, key is MIDI note, value is velocity

        public Measure(NoteValue?[] noteValues) : this(noteValues.Select(noteValue =>
        {
            if (noteValue != null)
                return new Dictionary<int, int> { { ((noteValue.Value.Octave + 1) * 12) + (int)noteValue.Value.Name, noteValue.Value.Velocity } };
            return null;
        }).ToArray())
        { }
        public Measure(Dictionary<int, int>?[] noteValues)
        {
            Velocities = noteValues;
            TimeDivision = noteValues.Length;
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
            Dictionary<int, (int velocity, int noteStart)> activeNotes = new(); //Track note on/off 

            short songTimeDivision = (short)MusicTheoryUtils.LCM(_measures.Select((measure, index) => (long)measure.TimeDivision).ToArray());
            int totalMidiTicks = 0;

            //NoteValue initialNoteValue = NoteValue.SilentNote;
            //int initialNoteStart = totalMidiTicks;
            //currentNotes[(initialNoteValue.Name, initialNoteValue.Octave)] = (initialNoteValue, initialNoteStart);

            foreach (Measure measure in _measures)
            {
                foreach (Dictionary<int, int>? velocitiesAtTimeDivision in measure.Velocities)
                {
                    if (velocitiesAtTimeDivision != null)
                    {
                        foreach (int noteNumber in velocitiesAtTimeDivision.Keys)
                        {
                            if (activeNotes.ContainsKey(noteNumber)) //&& currentNotes[(noteNumber.Name, noteNumber.Octave)].noteValue.Velocity > 0)
                            {
                                AddCurrentNote(noteNumber);
                                activeNotes.Remove(noteNumber);
                            }
                            //Begin building next non silent note
                            if (velocitiesAtTimeDivision[noteNumber] > 0)
                                activeNotes[noteNumber] = (velocitiesAtTimeDivision[noteNumber], totalMidiTicks);
                        }
                    }
                    //midi time division is per quarter note rather than per measure, thus 4 times higher resolution
                    int midiTicks = 4 * (songTimeDivision / measure.TimeDivision);
                    totalMidiTicks += midiTicks;
                }
            }
            //Add final non silent note
            foreach (int noteNumber in activeNotes.Keys)
            {
                AddCurrentNote(noteNumber);
            }
            activeNotes.Clear();

            //Pad with one division of silence, otherwise drywetmidi clips the last midi tick.            
            Notes.Add(new(NoteName.A, 4)
            {
                Time = totalMidiTicks,
                Length = songTimeDivision,
                Velocity = (SevenBitNumber)0
            });

            return songTimeDivision;

            void AddCurrentNote(int noteNumber)
            {
                Notes.Add(new((SevenBitNumber)noteNumber)
                {
                    Time = activeNotes[noteNumber].noteStart,
                    Length = totalMidiTicks - activeNotes[noteNumber].noteStart,
                    Velocity = (SevenBitNumber)activeNotes[noteNumber].velocity
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