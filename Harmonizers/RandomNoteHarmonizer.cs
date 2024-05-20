using MusicTheory;

namespace Melodroid.Harmonizers
{
    //TODO Random chords
    public class RandomNoteHarmonizer : IMeasureHarmonizer
    {
        public int CurrentFundamental = 0;
        public int CurrentOctave = 4;

        public List<(int fundamentalNoteNumber, Scale scale)> ChordPerMeasure = new();
        public List<Measure> MeasuresFromVelocities(List<int?[]> velocityMeasures)
        {
            ChordPerMeasure.Clear();

            List<Measure> measures = new();
            int measureIndex = 0;

            List<int> currentIntervals = Enumerable.Range(0, 12).ToList();

            foreach (var velocityMeasure in velocityMeasures)
            {
                Dictionary<int, int>?[] measureNoteValues = new Dictionary<int, int>?[velocityMeasure.Length];

                //Close notes from previous measure
                measureNoteValues[0] = new();
                if (measureIndex > 0)
                {
                    foreach (int noteNumber in measures[^1].MIDIKeys.Where(item => item != null).SelectMany(item => item!.Keys).ToHashSet())
                    {
                        measureNoteValues[0]![noteNumber] = 0;
                    }
                }

                //Play random notes from current allowed intervals
                HashSet<int> usedNotes = new();
                for (int i = 0; i < velocityMeasure.Length; i++)
                {
                    if (velocityMeasure[i] == null)
                        continue;

                    if (measureNoteValues[i] == null) //might be created earlier for i = 0 when closing old notes
                        measureNoteValues[i] = new();

                    //play random interval from current intervals and fundamental 
                    int noteNameNumber = (CurrentFundamental + currentIntervals.TakeRandom()) % 12; //C is 0
                    int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
                    int velocity = (int)velocityMeasure[i]!; //checked for null on the lines above                
                    measureNoteValues[i]![noteNumber] = velocity; //created the dictionary on the lines above

                    usedNotes.Add(noteNameNumber);
                }
                Measure measure = new(measureNoteValues);
                measures.Add(measure);
                measureIndex++;

                //Save used notes
                var usedIntervals = usedNotes.OrderBy(i => i).ToList();
                var effectiveFundamental = usedIntervals.Min();
                var effectiveChordIntervals = usedIntervals.Select(interval => interval - effectiveFundamental);
                Scale chord = new(effectiveChordIntervals.ToArray());
                ChordPerMeasure.Add((effectiveFundamental, chord)); //used by chord harmonizers
            }
            return measures;
        }
    }
}
