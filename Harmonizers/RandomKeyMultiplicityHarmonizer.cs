using MusicTheory;
using Serilog;

namespace Melodroid.Harmonizers
{
    public class RandomKeyMultiplicityHarmonizer() : IMeasureHarmonizer
    {
        public int CurrentScaleFundamental = 0;
        public int CurrentChordFundamental = 0;
        public int CurrentOctave = 4;
        private Random _random = new();
        //base 24 and 15 scales - prime combinations of 2,3 and 3,5
        List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 2, 4, 5, 7, 9, 11])];

        public List<(int fundamentalNoteNumber, Scale scale)> ChordPerMeasure = new();
        public List<Measure> MeasuresFromVelocities(List<int?[]> velocityMeasures)
        {
            ChordPerMeasure.Clear();

            List<Measure> measures = new();
            int measureIndex = 0;

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

                //Take any scale of interest
                Scale currentScale = _scalesOfInterest.TakeRandom();

                //Select a chord and calculate its chord key multiplicity for generating melody
                Scale currentChord;
                List<int>[] keyMultiplicity;
                do
                {
                    //make up an entire random chord of 3 to 4 notes
                    currentChord = new([0, .. currentScale.ToIntervals().Where(interval => interval != 0).TakeRandom(_random.Next(3, 5))]);

                    keyMultiplicity = currentScale.CalculateKeyMultiplicity(currentChord);
                }
                while (keyMultiplicity[0].Count == 0); //do not allow chords producing 0 multiplicity


                //Chord fundamental is the old scale fundamental
                CurrentChordFundamental = CurrentScaleFundamental;
                //Select new fundamental for scale from chord key multiplicity
                CurrentScaleFundamental = (CurrentScaleFundamental + keyMultiplicity[0].TakeRandom()) % 12;

                HashSet<int> currentIntervals = currentScale.ToIntervals().ToHashSet();

                //Play random notes from current allowed intervals                
                for (int i = 0; i < velocityMeasure.Length; i++)
                {
                    if (velocityMeasure[i] == null)
                        continue;

                    if (measureNoteValues[i] == null) //might be created earlier for i = 0 when closing old notes
                        measureNoteValues[i] = new();

                    //play random interval from current intervals and fundamental 
                    int noteNameNumber = (CurrentScaleFundamental + currentIntervals.TakeRandom()) % 12; //C is 0
                    int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
                    int velocity = (int)velocityMeasure[i]!; //checked for null on the lines above                
                    measureNoteValues[i]![noteNumber] = velocity; //created the dictionary on the lines above                    
                }
                Measure measure = new(measureNoteValues);
                measures.Add(measure);

                ChordPerMeasure.Add((CurrentChordFundamental, currentChord)); //used by chord harmonizers

                Log.Information($"Measure {measureIndex}");
                Log.Information($"Scale fundamental {CurrentScaleFundamental,-2}, Scale {currentScale} ");
                Log.Information($"Chord fundamental {CurrentChordFundamental,-2}, Chord {currentChord}");

                measureIndex++;
            }
            return measures;
        }
    }
}
