using MathNet.Numerics.Random;
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
        private Scale _currentScale;
        private Scale _currentChord = new([0, 4, 7]);
        private Scale _semitone = new([0, 1]);
        //List<Scale> _scalesOfInterest = [new([0, 2, 4, 5, 7, 9, 11])];        
        List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];
        //List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 2, 4, 5, 7, 9, 11])];
        //List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 1, 3, 5, 6, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];        
        //List<Scale> _scalesOfInterest = [new([0, 2, 4, 7, 11]), new([0, 1, 3, 5, 6, 8, 9]), new([0, 1, 3, 5, 6, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];
        //List<Scale> _scalesOfInterest = [new([0, 2, 4, 7, 11]), new([0, 1, 3, 5, 6, 8, 9]), new([0, 1, 3, 5, 6, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];

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

                //New scale once per phrase
                if (measureIndex % MeasuresPerPhrase == 0)
                {
                    //get key multiplicity for chord in all interesting scales
                    Dictionary<Scale, List<int>[]> keyMultiplicityPerScale = new();
                    foreach (Scale scale in _scalesOfInterest)
                    {
                        keyMultiplicityPerScale[scale] = scale.CalculateKeyMultiplicity(_currentChord);
                    }
                    //keep only multiplicities containing the current chord
                    keyMultiplicityPerScale = keyMultiplicityPerScale.Where(kv => kv.Value[0].Count > 0).ToDictionary();
                    //take any of the remaining scales and fundamental
                    _currentScale = keyMultiplicityPerScale.Keys.TakeRandom();
                    CurrentScaleFundamental = (CurrentScaleFundamental + keyMultiplicityPerScale[_currentScale][0].TakeRandom()) % 12;
                }
                //Always take new random chord
                do
                {
                    //3 notes from current scale
                    List<int> newIntervals = _currentScale.ToIntervals().TakeRandom(3);
                    //new chord at position relative to scale fundamental
                    int relativeChordFundamental = newIntervals.Min();
                    newIntervals = newIntervals.Select(interval => interval - relativeChordFundamental).ToList();
                    _currentChord = new(newIntervals.ToArray());
                    CurrentChordFundamental = (CurrentScaleFundamental + relativeChordFundamental) % 12;
                    //redo if semitone (avoid beating from 16/15)
                } while (_semitone.IsSubClassTo(_currentChord));

                HashSet<int> currentIntervals = _currentScale.ToIntervals().ToHashSet();

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

                ChordPerMeasure.Add((CurrentChordFundamental, _currentChord)); //used by chord harmonizers

                Log.Information($"Measure {measureIndex}");
                Log.Information($"Scale fundamental {CurrentScaleFundamental,-2}, Scale {_currentScale} ");
                Log.Information($"Chord fundamental {CurrentChordFundamental,-2}, Chord {_currentChord}");

                measureIndex++;
            }
            return measures;
        }
    }

    //Collapse refers to using the base 15 scale for calculating multiplicity, but using base 24 as the new scale
    public class MajorChordCollapsingKeyMultiplicityPhraseHarmonizer() : IMeasureHarmonizer
    {
        public int CurrentScaleFundamental = 0;
        public int CurrentChordFundamental = 0;
        public int CurrentOctave = 4;
        public int MeasuresPerPhrase = 1;
        private Random _random = new();
        private Scale _currentScale = base24Scale;
        private Scale _currentChord = new([0, 4, 7]);
        private Scale _semitone = new([0, 1]);

        //base 15 at [0], base 24 at [1]        
        public static Scale base15Scale = new([0, 1, 3, 5, 6, 8, 9, 10]);
        public static Scale base24Scale = new([0, 2, 4, 5, 7, 9, 11]);
        List<int>[] base15MajorChordMultiplicity = base15Scale.CalculateKeyMultiplicity(new Scale([0, 4, 7]));
        List<int>[] base24MajorChordMultiplicity = base24Scale.CalculateKeyMultiplicity(new Scale([0, 4, 7]));


        public List<(int fundamentalNoteNumber, Scale scale)> ChordPerMeasure = new();
        public List<Measure> MeasuresFromVelocities(List<int?[]> velocityMeasures)
        {
            ChordPerMeasure.Clear();

            List<Measure> measures = new();
            int measureIndex = 0;

            List<int> previousIntervals = new() { 0 };

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

                //New scale once per phrase - always collapse to base 24
                if (measureIndex % MeasuresPerPhrase == 0)
                {
                    //depending on notes from last measure choose a scale
                    List<List<int>> multiplicities = new();
                    foreach (int interval in previousIntervals)
                    {
                        multiplicities.Add(base15MajorChordMultiplicity[interval]);
                    }
                    List<int> legalIntervals = multiplicities.Skip(1).Aggregate(
                        new HashSet<int>(multiplicities.First()),
                        (commonIntervals, nextIntervals) => commonIntervals.Intersect(nextIntervals).ToHashSet()
                        ).ToList();
                    //increment all base 15 intervals with 1 to get the base 24 scale
                    legalIntervals = legalIntervals.Select(interval => interval + 1).ToList();
                    Console.WriteLine(string.Join(" ", legalIntervals));

                    //if scale is base 15, replace it with base 24 one fundamental step up
                    CurrentScaleFundamental = (CurrentScaleFundamental + legalIntervals.TakeRandom()) % 12;
                }
                //Always use major chord (TODO use arbitrary chords)
                CurrentChordFundamental = CurrentScaleFundamental;

                HashSet<int> currentIntervals = _currentScale.ToIntervals().ToHashSet();
                previousIntervals.Clear();

                //Play random notes from current allowed intervals                
                for (int i = 0; i < velocityMeasure.Length; i++)
                {
                    if (velocityMeasure[i] == null)
                        continue;

                    if (measureNoteValues[i] == null) //might be created earlier for i = 0 when closing old notes
                        measureNoteValues[i] = new();

                    int currentInterval = currentIntervals.TakeRandom();
                    previousIntervals.Add(currentInterval); //bookkeep for choosing next scale
                    //play random interval from current intervals and fundamental 
                    int noteNameNumber = (CurrentScaleFundamental + currentInterval) % 12; //C is 0
                    int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
                    int velocity = (int)velocityMeasure[i]!; //checked for null on the lines above                
                    measureNoteValues[i]![noteNumber] = velocity; //created the dictionary on the lines above                    
                }
                Measure measure = new(measureNoteValues);
                measures.Add(measure);

                ChordPerMeasure.Add((CurrentChordFundamental, _currentChord)); //used by chord harmonizers

                Log.Information($"Measure {measureIndex}");
                Log.Information($"Scale fundamental {CurrentScaleFundamental,-2}, Scale {_currentScale} ");
                Log.Information($"Chord fundamental {CurrentChordFundamental,-2}, Chord {_currentChord}");

                measureIndex++;
            }
            return measures;
        }
    }
}
