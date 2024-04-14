using Fractions;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;
using Scale = MusicTheory.Scale;

public class BeatBox(IRhythmMeasureMaker rhythmMeasureMaker, IMeasureHarmonizer measureHarmonizer)
{
    public IMeasureHarmonizer MeasureHarmonizer = measureHarmonizer;
    public IRhythmMeasureMaker RhythmMeasureMaker = rhythmMeasureMaker;

    public List<Measure> MakeMeasures()
    {
        List<int?[]> velocities = RhythmMeasureMaker.MakeVelocities();
        return MeasureHarmonizer.MeasuresFromVelocities(velocities);
    }

    public void WriteMeasuresToMidi(List<Measure> measures, string folderPath, string fileName, bool overWrite = false)
    {

        /** Example Usage
            string folderPath = @"E:\Documents\Reaper Projects\Melodroid\MIDI_write_testing\";
            int timeDivision = 8;
            NoteValue?[] noteValues = new NoteValue?[timeDivision];
            noteValues[3] = new(NoteName.A, 4, 64);
            Measure measure = new(noteValues);
            List<Measure> measureList = new();
            measureList.Add(measure);
            WriteMeasuresToMidi(measureList, folderPath, "test", true);
        **/

        MidiFile midiFile = new MidiFile();

        //TODO set tempo

        TrackChunk trackChunk = new TrackChunk();
        using (TimedObjectsManager<Melanchall.DryWetMidi.Interaction.Note> notesManager = trackChunk.ManageNotes())
        {
            TimedObjectsCollection<Melanchall.DryWetMidi.Interaction.Note> notes = notesManager.Objects;
            NoteBuilder nb = new NoteBuilder(measures);
            midiFile.TimeDivision = new TicksPerQuarterNoteTimeDivision(nb.MidiTimeDivision);
            foreach (var note in nb.Notes)
            {
                notes.Add(note);
            }
        }

        midiFile.Chunks.Add(trackChunk);
        midiFile.Write(Path.Combine(folderPath, fileName + ".mid"), overWrite);
    }
}

//Decorates rhythms (lists with time division number of nullable velocities) with scientific pitch according to the rules of consonance 
public interface IMeasureHarmonizer
{
    //Wants some sort of rhtyhmic proto measure - NoteValues without name or octave -> Velocity values only?
    //Velocity 0 is note off
    //Else note on
    //Cannot represent crescendo - how to differ between change of velocity for old note and new note with new velocity value?
    // - Midi has no explicit support for crescendo - must use expression/volume messages, so perhaps the problem is elsewhere in any way.
    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities);
}

//Creates rhythms (lists with time division number of nullable velocities) according to some heuristic (e.g. simply isochrony or triplet swing time)
public interface IRhythmMeasureMaker
{
    public List<int?[]> MakeVelocities();
}

public class SimpleIsochronicRhythmMaker(int timeDivision, int numberOfMeasures, int beatsPerMeasure, int velocity = 64) : IRhythmMeasureMaker
{
    public int TimeDivision = timeDivision;
    public int NumberOfMeasures = numberOfMeasures;
    public int BeatsPerMeasure = beatsPerMeasure;
    public int Velocity = velocity;

    public List<int?[]> MakeVelocities()
    {
        List<int?[]> velocityMeasures = new();
        for (int i = 0; i < NumberOfMeasures; i++)
        {
            int?[] velocities = new int?[TimeDivision];
            int beatsPerDivision = TimeDivision / BeatsPerMeasure;
            for (int j = 0; j < velocities.Length; j++)
            {
                if (j % beatsPerDivision == 0)
                    velocities[j] = Velocity;
            }
            velocityMeasures.Add(velocities);
        }
        return velocityMeasures;
    }
}

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

//Walks a path of chords from origin to destination
public class PathWalkMeasureHarmonizer(Scale originScale, Scale destinationScale, int targetSteps) : IMeasureHarmonizer
{
    public Scale CurrentScale = originScale;
    public Scale DestinationScale = destinationScale;
    public int TargetSteps = targetSteps;
    public NoteName CurrentFundamental = 0;
    public int CurrentOctave = 4;
    Random random = new();

    private ScaleCalculator _scaleCalculator = new();

    public List<(NoteName fundamental, Scale scale)> ChordPerMeasure = new();
    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities)
    {
        ChordProgressionGraph chordProgressionGraph = new(_scaleCalculator);
        ChordProgressionPathFinder pathFinder = new(chordProgressionGraph);
        Queue<ChordPath> chordPaths = pathFinder.FindPathsFrom(CurrentScale, TargetSteps);
        List<ChordPath> legalPaths = new();
        foreach (ChordPath chordPath in chordPaths)
        {
            if (chordPath.Nodes.Last().Scale == DestinationScale)
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
            ChordPerMeasure.Add((CurrentFundamental, CurrentScale));
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

                int noteNameNumber = ((int)(CurrentFundamental + currentIntervals.TakeRandom())) % 12; //C is 0
                int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
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
            CurrentFundamental = (NoteName)(((int)(CurrentFundamental + currentPath.PathSteps[indexInsidePath])) % 12);
        }
        return measures;
    }
}

//TODO: WIP - supply with chords from melody to generate measures with block chords at measure start
//public class ChordMeasureHarmonizer(List<(NoteName Fundamental, Scale Scale)> chordProgressionsPerMeasure, int initialOctave) : IMeasureHarmonizer
//{
//    public int InitialOctave = initialOctave;
//    public List<(NoteName Fundamental, Scale Scale)> ChordProgressionsPerMeasure { get; } = chordProgressionsPerMeasure;

//    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities)
//    {
//        List<Measure> measures = new();
//        int measureIndex = 0;
//        foreach (int?[] velocityMeasure in velocities)
//        {
//            List<int> intervals = ChordProgressionsPerMeasure[measureIndex].Scale.ToIntervals();
//            NoteValue?[] noteValues = new NoteValue?[velocityMeasure.Length];
//            for (int i = 0; i < velocityMeasure.Length; i++)
//            {
//                if (velocityMeasure[i] == null)
//                    continue;

//                NoteName noteName = (NoteName)(((int)(ChordProgressionsPerMeasure[measureIndex].Fundamental + intervals.TakeRandom())) % 12); //C is 0
//                int octave = InitialOctave;
//                int velocity = (int)velocityMeasure[i]!; //checked for null on the lines just above
//                noteValues[i] = new(noteName, octave, velocity); //Needs support for multiple simultaneous note values
//                break;
//            }
//            Measure measure = new(noteValues);
//            measures.Add(measure);
//        }
//        return measures;
//    }
//}