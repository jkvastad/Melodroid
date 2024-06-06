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
                foreach (int noteNumber in measures[^1].MIDIKeys[0]!.Keys)
                {
                    measureNoteValues[0]![noteNumber] = 0;
                }
            }

            //set chord and fundamental
            _currentScale = ChordProgressionsPerMeasure[measureIndex].scale;
            _currentFundamentalNoteNumber = ChordProgressionsPerMeasure[measureIndex].fundamentalNoteNumber;

            foreach (int interval in _currentScale.ToIntervals())
            {
                int currentNoteNumber = (CurrentOctave * 12) + _currentFundamentalNoteNumber + interval;
                if (currentNoteNumber >= ((CurrentOctave + 1) * 12) - 1) //stay one semitone from melody
                    currentNoteNumber -= 12;

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