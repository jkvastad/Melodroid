using MusicTheory;
using Serilog;
using Scale = MusicTheory.Scale;
//Walks a path of chords from origin to destination
public class PathWalkMeasureHarmonizer(Scale originScale, Scale destinationScale, int targetSteps) : IMeasureHarmonizer
{
    public Scale CurrentScale = originScale;
    public Scale DestinationScale = destinationScale;
    public int TargetSteps = targetSteps;
    public int LowestOctaveNoteNumber = 60; //C4 is 60
    int _currentFundamentalNoteNumber = 60;
    Random random = new();

    private ScaleCalculator _scaleCalculator = new();

    public List<(int fundamentalNoteNumber, Scale scale)> ChordPerMeasure = new();
    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities)
    {
        ChordProgressionGraph chordProgressionGraph = new(_scaleCalculator);
        ChordProgressionPathFinder pathFinder = new(chordProgressionGraph);
        Queue<ChordPath> chordPaths = pathFinder.FindPathsFrom(CurrentScale, TargetSteps);
        List<ChordPath> legalPaths = new();
        foreach (ChordPath chordPath in chordPaths)
        {
            if (chordPath.PathSteps.Sum() % 12 == 0)
            {
                legalPaths.Add(chordPath);
            }
        }
        if (legalPaths.Count == 0)
        {
            throw new ArgumentException($"No path found from {CurrentScale} to {DestinationScale} with {TargetSteps} steps");
        }
        int indexInsidePath = 0;
        ChordPath currentPath = legalPaths[random.Next(legalPaths.Count)];

        List<Measure> measures = new();
        ChordPerMeasure.Clear();


        HashSet<int> activeNotes = new();
        foreach (int?[] velocityMeasure in velocities)
        {
            List<int> currentIntervals = CurrentScale.ToIntervals();
            ChordPerMeasure.Add((_currentFundamentalNoteNumber, CurrentScale));
            Dictionary<int, int>?[] measureNoteValues = new Dictionary<int, int>?[velocityMeasure.Length];

            measureNoteValues[0] = new();
            foreach (int noteNumber in activeNotes) //close notes from last measure
                measureNoteValues[0]![noteNumber] = 0;

            for (int i = 0; i < velocityMeasure.Length; i++)
            {
                if (velocityMeasure[i] == null)
                    continue;

                if (measureNoteValues[i] == null)
                    measureNoteValues[i] = new();

                int noteNumber = _currentFundamentalNoteNumber + currentIntervals.TakeRandom();
                int velocity = (int)velocityMeasure[i]!; //checked for null on the lines just above                
                measureNoteValues[i]![noteNumber] = velocity;
            }
            //note which notes are still open
            foreach (Dictionary<int, int>? noteVelocities in measureNoteValues)
            {
                if (noteVelocities == null)
                    continue;
                foreach (int noteNumber in noteVelocities.Keys)
                {
                    if (noteVelocities[noteNumber] != 0)
                        activeNotes.Add(noteNumber);
                    else
                        activeNotes.Remove(noteNumber);
                }
            }

            Measure measure = new(measureNoteValues);
            measures.Add(measure);

            //Next scale from chord progressions
            indexInsidePath = (indexInsidePath + 1) % currentPath.Nodes.Count;
            //next chord progression
            if (indexInsidePath == 0)
            {
                //currentPath = legalPaths[random.Next(legalPaths.Count)];
                List<ChordPath> evenBasePaths = legalPaths.Where(path => path.Nodes.All(node => true
                    //(node.Base % 2) == 0 &&
                    //(node.Base % 3) != 0 &&
                    //(node.Base % 5) != 0
                    )).ToList();
                currentPath = evenBasePaths[random.Next(evenBasePaths.Count)];
            }

            CurrentScale = currentPath.Nodes[indexInsidePath].Scale;
            int keyStep = currentPath.PathSteps[indexInsidePath];
            Log.Information($"{keyStep,-2} - {CurrentScale.CalculateBase(),-2} - {CurrentScale}");
            _currentFundamentalNoteNumber = ((_currentFundamentalNoteNumber + keyStep) % 12) + LowestOctaveNoteNumber;
        }
        return measures;
    }
}
