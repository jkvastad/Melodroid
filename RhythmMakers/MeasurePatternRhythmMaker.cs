using MathNet.Numerics.Random;

public class SimpleMeasurePatternRhythmMaker(
    int timeDivision,
    int numberOfMeasures,
    int beatsPerMeasure,
    int deviationsPerMeasure,
    List<List<PatternBlock>> measurePatterns,
    int velocity = 64) : IRhythmMeasureMaker
{
    public int TimeDivision = timeDivision;
    public int NumberOfMeasures = numberOfMeasures;
    public int BeatsPerMeasure = beatsPerMeasure;
    public int DeviationsPerMeasure = deviationsPerMeasure;
    List<List<PatternBlock>> MeasurePatterns = measurePatterns;
    public int Velocity = velocity;

    public List<int?[]> VelocityMeasures = new();

    private Random _random = new();
    public List<int?[]> MakeVelocities()
    {
        Dictionary<string, int?[]> patternBlocks = new();
        for (int i = 0; i < NumberOfMeasures; i++)
        {
            //Which beats will deviate from isochrony in the measures (on new block)
            List<int> deviatingBeats = Enumerable.Repeat(0, DeviationsPerMeasure).Select(_ => _random.Next(BeatsPerMeasure)).ToList();

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

                    //Similar procedure as SimpleGrooveRhythmMaker but per block
                    int blockStartingBeat = (int)Math.Ceiling(accumulatedTimeDivision / (double)divisionsPerBeat);
                    for (
                        int beat = blockStartingBeat;
                        beat < ((accumulatedTimeDivision + patternBlock.TimeDivisions) / (double)divisionsPerBeat);
                        beat++)
                    {
                        if (deviatingBeats.Contains(beat))
                        {
                            //deviate by at most divisionsPerBeat - 1 so we don't overlap with next beat
                            int deviation = _random.Next(1, divisionsPerBeat);
                            //deviate both ways in measure                            
                            if (beat > blockStartingBeat)
                                deviation = _random.NextBoolean() ? deviation : -deviation;

                            patternBlockVelocities[(beat - blockStartingBeat) * divisionsPerBeat + deviation] = Velocity;
                        }
                        else
                            patternBlockVelocities[(beat - blockStartingBeat) * divisionsPerBeat] = Velocity;
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