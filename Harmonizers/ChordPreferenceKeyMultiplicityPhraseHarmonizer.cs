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
        //List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 2, 4, 5, 7, 9, 11])];
        //List<Scale> _scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 1, 3, 5, 6, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];
        //The natural scales
        List<Scale> _scalesOfInterest = [new([0, 2, 4, 7, 11]), new([0, 1, 3, 5, 6, 8, 9]), new([0, 1, 3, 5, 6, 9, 10]), new([0, 2, 4, 5, 7, 9, 11])];
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
}
