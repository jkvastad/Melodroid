using MusicTheory;
using Serilog;

namespace Melodroid.Harmonizers
{
    public class ChordPreferenceKeyMultiplicityPhraseHarmonizer() : IMeasureHarmonizer
    {
        public int CurrentScaleFundamental = 0;
        public int CurrentChordFundamental = 0;
        public int CurrentOctave = 4;
        public int MeasuresPerPhrase = 4;
        private Random _random = new();
        private Scale CurrentScale;
        private Scale CurrentChord;
        //base 24 and 15 scales - prime combinations of 2,3 and 3,5
        //List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 2, 4, 5, 7, 9, 11])];
        //List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 1, 3, 5, 6, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];
        List<Scale> _scalesOfInterest = [new([0, 2, 4, 7, 11]), new([0, 1, 3, 5, 6, 8, 9]), new([0, 1, 3, 5, 6, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];
        //List<Scale> _scalesOfInterest = [new([0, 2, 4, 5, 7, 9, 11])];
        //List<Scale> _chordsOfInterest = [new([0, 2, 7]), new([0, 3, 7]), new([0, 4, 7]), new([0, 4, 7, 10])];
        //List<Scale> _chordsOfInterest = [new([0, 2, 7]), new([0, 3, 6]), new([0, 3, 7]), new([0, 4, 7])];
        //List<Scale> _chordsOfInterest = [new([0, 2, 7]), new([0, 3, 7]), new([0, 4, 7])];
        List<Scale> _chordsOfInterest = [new([0, 3, 7]), new([0, 4, 7])];
        //List<Scale> _chordsOfInterest = [new([0, 4, 7])];

        public List<(int fundamentalNoteNumber, Scale scale)> ChordPerMeasure = new();
        public List<Measure> MeasuresFromVelocities(List<int?[]> velocityMeasures)
        {
            ChordPerMeasure.Clear();

            //intial chord
            CurrentChord = _chordsOfInterest.TakeRandom();

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

                List<int>[] keyMultiplicity;

                //TODO overhaul of how melody is created from multiplicity
                //New scale once per phrase
                if (measureIndex % MeasuresPerPhrase == 0)
                {
                    //Take any scale of interest
                    CurrentScale = _scalesOfInterest.TakeRandom();
                    //Use old chord for key multiplicity
                    keyMultiplicity = CurrentScale.CalculateKeyMultiplicity(CurrentChord);
                    //Select new fundamental for scale from old chord key multiplicity
                    CurrentScaleFundamental = (CurrentScaleFundamental + keyMultiplicity[0].Where(mult => mult != 0).TakeRandom()) % 12;
                }
                //Always take new random chord
                CurrentChord = _chordsOfInterest.TakeRandom();
                //Chord fundamental is any chord placement in current scale.
                //(Since key multiplicity is the scales fundamental translation to the right, chord placement is negative multiplicity)
                keyMultiplicity = CurrentScale.CalculateKeyMultiplicity(CurrentChord);
                CurrentChordFundamental = (12 + (CurrentScaleFundamental - keyMultiplicity[0].TakeRandom())) % 12;

                HashSet<int> currentIntervals = CurrentScale.ToIntervals().ToHashSet();

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

                ChordPerMeasure.Add((CurrentChordFundamental, CurrentChord)); //used by chord harmonizers

                Log.Information($"Measure {measureIndex}");
                Log.Information($"Scale fundamental {CurrentScaleFundamental,-2}, Scale {CurrentScale} ");
                Log.Information($"Chord fundamental {CurrentChordFundamental,-2}, Chord {CurrentChord}");

                measureIndex++;
            }
            return measures;
        }
    }
}
