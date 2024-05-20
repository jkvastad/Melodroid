
//Example Usage:
//int timeDivision = 24;
//int numberOfMeasures = 32;
//int beatsPerMeasure = 8;
//SimpleIsochronicRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure);
using MathNet.Numerics.Random;
//Groove as in deviations from isochronic rhythm, i.e. implying the isochrony rather than stating it 
public class SimpleGrooveRhythmMaker(
    int timeDivision,
    int numberOfMeasures,
    int beatsPerMeasure,
    int deviationsPerMeasure,
    int velocity = 64) : IRhythmMeasureMaker
{
    public int TimeDivision = timeDivision;
    public int NumberOfMeasures = numberOfMeasures;
    public int BeatsPerMeasure = beatsPerMeasure;
    public int DeviationsPerMeasure = deviationsPerMeasure;
    public int Velocity = velocity;
    private Random _random = new();
    public List<int?[]> VelocityMeasures = new();

    public List<int?[]> MakeVelocities()
    {
        for (int i = 0; i < NumberOfMeasures; i++)
        {
            //Which beats will deviate from isochrony
            List<int> deviatingBeats = Enumerable.Repeat(0, DeviationsPerMeasure).Select(_ => _random.Next(BeatsPerMeasure)).ToList();

            int?[] velocities = new int?[TimeDivision];
            int divisionsPerBeat = TimeDivision / BeatsPerMeasure;

            for (int beat = 0; beat < BeatsPerMeasure; beat++)
            {
                if (deviatingBeats.Contains(beat))
                {
                    //deviate by at most divisionsPerBeat - 1 so we don't overlap with next beat
                    int deviation = _random.Next(1, divisionsPerBeat);
                    //deviate both ways in measure
                    //TODO more sophisticated version allow deviations to happen in previous measure
                    if (beat > 0)
                        deviation = _random.NextBoolean() ? deviation : -deviation;

                    velocities[beat * divisionsPerBeat + deviation] = Velocity;
                }
                else
                    velocities[beat * divisionsPerBeat] = Velocity;
            }
            VelocityMeasures.Add(velocities);
        }
        return VelocityMeasures;
    }
}