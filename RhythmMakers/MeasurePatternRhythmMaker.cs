
//TODO read pattern block lists
public class SimpleMeasurePatternRhythmMaker(
    int timeDivision,
    int numberOfMeasures,
    int beatsPerMeasure,
    List<List<PatternBlock>> measurePatterns,
    int velocity = 64) : IRhythmMeasureMaker
{
    public int TimeDivision = timeDivision;
    public int NumberOfMeasures = numberOfMeasures;
    public int BeatsPerMeasure = beatsPerMeasure;
    List<List<PatternBlock>> MeasurePatterns = measurePatterns;
    public int Velocity = velocity;

    public List<int?[]> VelocityMeasures = new();
    public List<int?[]> MakeVelocities()
    {
        Dictionary<string, int?[]> patternBlocks = new();
        for (int i = 0; i < NumberOfMeasures; i++)
        {
            List<int?[]> velocityPatterns = new();
            int accumulatedTimeDivision = 0;
            List<PatternBlock> currentPatternBlocks = MeasurePatterns[i % MeasurePatterns.Count];
            foreach (PatternBlock patternBlock in currentPatternBlocks)
            {
                //if block has already been created, reuse it
                if (patternBlocks.Keys.Contains(patternBlock.BlockName))
                {
                    velocityPatterns.Add(patternBlocks[patternBlock.BlockName]);
                    accumulatedTimeDivision += patternBlock.TimeDivisions;
                }
                else
                {
                    //Create new block
                    int?[] patternBlockVelocities = new int?[patternBlock.TimeDivisions];
                    int divisionsPerBeat = TimeDivision / BeatsPerMeasure;
                    for (int timeDivision = accumulatedTimeDivision; timeDivision < accumulatedTimeDivision + patternBlockVelocities.Length; timeDivision++)
                    {
                        //place beats at beat divisions
                        if (timeDivision % divisionsPerBeat == 0)
                            patternBlockVelocities[timeDivision - accumulatedTimeDivision] = Velocity;
                    }
                    velocityPatterns.Add(patternBlockVelocities);
                    patternBlocks[patternBlock.BlockName] = patternBlockVelocities;
                }
            }

            int?[] velocities = velocityPatterns.SelectMany(velocity => velocity).ToArray();
            VelocityMeasures.Add(velocities);
        }
        return VelocityMeasures;
    }
}

public class PatternBlock(string blockName, int timeDivision)
{
    public string BlockName = blockName;
    public int TimeDivisions = timeDivision;
}