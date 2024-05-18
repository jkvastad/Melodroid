using MusicTheory;
using System.Numerics;
using Scale = MusicTheory.Scale;

//Supply with chords from melody to generate measures with block chords at measure start
public class ChordMeasureHarmonizer(
    List<(int fundamentalNoteNumber, Scale scale)> chordProgressionsPerMeasure,
    int initialOctave
    ) : IMeasureHarmonizer
{
    public int CurrentOctave = initialOctave;
    Scale _currentScale;
    int _currentFundamentalNoteNumber;
    public List<(int fundamentalNoteNumber, Scale scale)> ChordProgressionsPerMeasure { get; } = chordProgressionsPerMeasure;
    Scale semitoneScale = new(new[] { 0, 1 }); //no good in chord
    Scale tripleWholeToneScale = new(new[] { 0, 2, 4 }); //also seems tense

    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities)
    {
        List<Measure> measures = new();
        int measureIndex = 0;

        foreach (int?[] velocityMeasure in velocities)
        {
            _currentScale = ChordProgressionsPerMeasure[measureIndex].scale;
            _currentFundamentalNoteNumber = ChordProgressionsPerMeasure[measureIndex].fundamentalNoteNumber;
            Dictionary<int, int>?[] measureNoteValues = new Dictionary<int, int>?[velocityMeasure.Length];
            measureNoteValues[0] = new();

            //close notes from previous measure
            if (measureIndex > 0)
            {
                foreach (int noteNumber in measures[^1].Velocities[0]!.Keys)
                {
                    measureNoteValues[0]![noteNumber] = 0;
                }
            }

            //Create chord            
            if (semitoneScale.IsSubClassTo(_currentScale) || tripleWholeToneScale.IsSubClassTo(_currentScale))
            {
                //power set of scale size
                List<int> allSubscalePermutations = new();
                for (int i = 1; i < BigInteger.Pow(2, _currentScale.NumberOfKeys()); i++)
                {
                    allSubscalePermutations.Add(i);
                }
                //all subscale progressions                
                List<(int keyStep, Scale subscale)> subscaleProgressions = new();
                List<int> currentScaleIntervals = _currentScale.ToIntervals();
                foreach (int permutation in allSubscalePermutations)
                {
                    //create the new scale
                    List<int> newScaleIntervals = new();
                    for (int i = 0; i < currentScaleIntervals.Count; i++)
                    {
                        if (((permutation >> i) & 1) == 1) // is interval included?
                            newScaleIntervals.Add(currentScaleIntervals[i]);
                    }
                    int smallestInterval = newScaleIntervals.First();
                    newScaleIntervals = newScaleIntervals.Select(interval => interval - smallestInterval).ToList(); //normalize scale to start at interval 0
                    Scale newScale = new(newScaleIntervals.ToArray());

                    //check for semitone                    
                    if (semitoneScale.IsSubClassTo(newScale) || tripleWholeToneScale.IsSubClassTo(newScale))
                        continue;

                    subscaleProgressions.Add((smallestInterval, newScale));
                }
                //Take the largest progression scale at random. Mr. speaker, we are for the big.
                int largestProgressionScaleSize = subscaleProgressions.Max(progression => progression.subscale.NumberOfKeys());
                var largestProgressions = subscaleProgressions.Where(progression => progression.subscale.NumberOfKeys() == largestProgressionScaleSize);
                var progression = largestProgressions.TakeRandom();
                _currentScale = progression.subscale;
                _currentFundamentalNoteNumber += progression.keyStep;
            }

            foreach (int interval in _currentScale.ToIntervals())
            {
                int currentNoteNumber = (CurrentOctave * 12) + _currentFundamentalNoteNumber + interval - 12; //put the chords one octave below the melody
                int velocity = 64; //This particular harmonizer cares not for actual velocities

                measureNoteValues[0]![currentNoteNumber] = velocity;
            }

            Measure measure = new(measureNoteValues);
            measures.Add(measure);

            measureIndex++;
        }
        return measures;
    }
}