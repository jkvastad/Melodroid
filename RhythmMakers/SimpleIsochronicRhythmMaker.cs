//Example Usage:
//int timeDivision = 24;
//int numberOfMeasures = 32;
//int beatsPerMeasure = 8;
//SimpleIsochronicRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure);
public class SimpleIsochronicRhythmMaker(int timeDivision, int numberOfMeasures, int beatsPerMeasure, int velocity = 64) : IRhythmMeasureMaker
{
    public int TimeDivision = timeDivision;
    public int NumberOfMeasures = numberOfMeasures;
    public int BeatsPerMeasure = beatsPerMeasure;
    public int Velocity = velocity;
    public List<int?[]> VelocityMeasures = new();

    public List<int?[]> MakeVelocities()
    {
        for (int i = 0; i < NumberOfMeasures; i++)
        {
            int?[] velocities = new int?[TimeDivision];
            int divisionsPerBeat = TimeDivision / BeatsPerMeasure;
            for (int j = 0; j < velocities.Length; j++)
            {
                if (j % divisionsPerBeat == 0)
                    velocities[j] = Velocity;
            }
            VelocityMeasures.Add(velocities);
        }
        return VelocityMeasures;
    }
}
