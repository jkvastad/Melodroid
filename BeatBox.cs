using Fractions;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;
using Serilog;
using System.Collections.Generic;
using static MusicTheory.MusicTheoryUtils;
using Scale = MusicTheory.Scale;

public class BeatBox
{
    Dictionary<int, HashSet<(int key, Fraction approximation)>> _allKeysCompatibleWithDenominator = new();
    Dictionary<int, List<Fraction>> _tet12FractionApproximations;
    Random _random = new Random();
    public BeatBox(int maxFactors = 4, int maxPatternLength = 50)
    {
        _allKeysCompatibleWithDenominator = CalculateKeysCompatibleWithPatternLength(maxFactors, maxPatternLength);

        Console.WriteLine($"Denominators of 12TET fraction approximations for pattern length {maxPatternLength} and primes {string.Join(",", standardPrimes)} with compatible keys:");
        Console.WriteLine();
        foreach (var entry in new SortedDictionary<int, HashSet<(int key, Fraction approximation)>>(_allKeysCompatibleWithDenominator))
        {
            Console.Write($"Denominator: {entry.Key} - Keys: ");
            foreach (var keyAndApproximation in entry.Value)
            {
                //Console.Write($"{keyAndApproximation.key} ({keyAndApproximation.approximation.Numerator}/{keyAndApproximation.approximation.Denominator}), ");
                Console.Write($"{keyAndApproximation.key}, ");
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Denominators of 12TET fraction approximations for pattern length {maxPatternLength} and primes {string.Join(",", standardPrimes)} with compatible keys:");
        foreach (var entry in new SortedDictionary<int, HashSet<(int key, Fraction approximation)>>(_allKeysCompatibleWithDenominator))
        {
            Console.Write($"Denominator: {entry.Key} - Keys: ");
            foreach (var keyAndApproximation in entry.Value)
            {
                Console.Write($"{keyAndApproximation.key} ({keyAndApproximation.approximation.Numerator}/{keyAndApproximation.approximation.Denominator}), ");
                //Console.Write($"{keyAndApproximation.key}, ");
            }
            Console.WriteLine();
        }
    }

    public Phrase TestPhrase()
    {
        List<Measure> measures = new List<Measure>();

        int measureTimeDivision = 8;
        int measuresInPhrase = 4;
        int firstKey = _random.Next(12);
        int firstDenominator = (int)_tet12FractionApproximations[firstKey].TakeRandom().Denominator;
        for (int measureIndex = 0; measureIndex < measuresInPhrase; measureIndex++)
        {
            int?[] velocities = GetRhythm(measureTimeDivision);

            NoteValue?[] noteValues = GetHarmony(velocities, firstDenominator);
            measures.Add(new(noteValues));
        }

        return new Phrase(measures);
    }

    private int?[] GetRhythm(int measureTimeDivision)
    {
        int?[] velocities = new int?[measureTimeDivision];
        for (int i = 0; i < velocities.Length; i++)
        {
            //TODO generate better rhythms, perhaps things which the brain can easily chunk (based on cerebullum and rhythm - e.g. learning 2 + 3 count)
            if (_random.Next(measureTimeDivision) > measureTimeDivision / 4)
            {
                if (_random.Next(6) > 0)
                    velocities[i] = _random.Next(64, 97);
                else
                    velocities[i] = 0;
            }
        }
        return velocities;
    }

    private NoteValue?[] GetHarmony(int?[] velocities, int firstDenominator)
    {
        NoteValue?[] noteValues = new NoteValue?[velocities.Length];
        NoteValue fundamentalNote = new NoteValue(NoteName.C, 4, 64);
        Log.Information($"denominators dividing {firstDenominator}");
        for (int i = 0; i < velocities.Length; i++)
        {
            if (velocities[i].HasValue)
            {
                noteValues[i] = fundamentalNote + _allKeysCompatibleWithDenominator[firstDenominator].TakeRandom().key;
            }
        }
        return noteValues;
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
interface IMeasureHarmonizer
{
    //Wants some sort of rhtyhmic proto measure - NoteValues without name or octave -> Velocity values only?
    //Velocity 0 is note off
    //Else note on
    //Cannot represent crescendo - how to differ between change of velocity for old note and new note with new velocity value?
    // - Midi has no explicit support for crescendo - must use expression/volume messages, so perhaps the problem is elsewhere in any way.
    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities);
}

//Creates rhythms (lists with time division number of nullable velocities) according to some heuristic (e.g. simply isochrony or triplet swing time)
interface IRhythmMeasureMaker
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
        }
        return velocityMeasures;
    }
}

//Do a random walk from some starting chord
public class RandomWalkMeasureHarmonizer(Scale currentScale) : IMeasureHarmonizer
{
    public Scale CurrentScale = currentScale;
    public NoteName CurrentFundamental = 0;
    public int InitialOctave = 4;
    private ScaleCalculator _scaleCalculator = new();

    public List<Measure> MeasuresFromVelocities(List<int?[]> velocities)
    {
        List<Measure> measures = new();
        foreach (int?[] velocityMeasure in velocities)
        {
            List<int> intervals = CurrentScale.AsIntervals();
            NoteValue?[] noteValues = new NoteValue?[velocityMeasure.Length];
            for (int i = 0; i < velocityMeasure.Length; i++)
            {
                if (velocityMeasure[i] == null)
                    continue;

                NoteName noteName = (NoteName)(((int)(CurrentFundamental + intervals.TakeRandom())) % 12); //C is 0
                int octave = InitialOctave;
                int velocity = (int)velocityMeasure[i]!; //checked for null on the lines just above
                noteValues[i] = new(noteName, octave, velocity);
            }
            Measure measure = new(noteValues);
            measures.Add(measure);

            List<List<(int keySteps, Scale legalKeys)>> chordProgressionsPerSuperClass = _scaleCalculator.CalculateChordProgressionsPerSuperClass(CurrentScale);
            List<(int keySteps, Scale legalKeys)> chordProgressions = chordProgressionsPerSuperClass.TakeRandom();
            (int keySteps, Scale legalKeys) chordProgression = chordProgressions.TakeRandom();
            CurrentScale = chordProgression.legalKeys;
            CurrentFundamental = (NoteName)(((int)(CurrentFundamental + chordProgression.keySteps)) % 12);
        }
        //Get all chord progressions for the current chord, choose a new chord from progression, decorate measure, repeat.
        return measures;
    }
}