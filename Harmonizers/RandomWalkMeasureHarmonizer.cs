using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;
using Scale = MusicTheory.Scale;
//Do a random walk from some starting chord
public class RandomWalkMeasureHarmonizer(Scale currentScale) : IMeasureHarmonizer
{
    public Scale CurrentScale = currentScale;
    public NoteName CurrentFundamental = 0;
    public int CurrentOctave = 4;

    private ScaleCalculator _scaleCalculator = new();

    public List<(NoteName fundamental, Scale scale)> ChordPerMeasure = new();

    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities)
    {
        List<Measure> measures = new();
        ChordPerMeasure.Clear();
        foreach (int?[] velocityMeasure in velocities)
        {
            List<int> currentIntervals = CurrentScale.ToIntervals();
            ChordPerMeasure.Add((CurrentFundamental, CurrentScale));
            Dictionary<int, int>?[] measureNoteValues = new Dictionary<int, int>?[velocityMeasure.Length];
            for (int i = 0; i < velocityMeasure.Length; i++)
            {
                if (velocityMeasure[i] == null)
                    continue;

                measureNoteValues[i] = new();

                int noteNameNumber = ((int)(CurrentFundamental + currentIntervals.TakeRandom())) % 12; //C is 0
                int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
                int velocity = (int)velocityMeasure[i]!; //checked for null on the lines just above                
                measureNoteValues[i][noteNumber] = velocity;
            }
            Measure measure = new(measureNoteValues);
            measures.Add(measure);

            //superscales from triad chord
            List<List<(int keySteps, Scale legalKeys)>> chordProgressionsPerSuperClass =
                _scaleCalculator.CalculateSuperClassProgressionsPerSuperClass(new(CurrentScale.ToIntervals().TakeRandom(3).ToArray()));
            chordProgressionsPerSuperClass = chordProgressionsPerSuperClass.Where(superClass => superClass.Count > 0).ToList();
            List<(int keySteps, Scale legalKeys)> chordProgressions = chordProgressionsPerSuperClass.TakeRandom();
            (int keySteps, Scale legalKeys) chordProgression = chordProgressions.TakeRandom();
            CurrentScale = new(chordProgression.legalKeys.ToIntervals().ToArray());

            CurrentFundamental = (NoteName)(((int)(CurrentFundamental + chordProgression.keySteps)) % 12);
        }
        return measures;
    }
}
