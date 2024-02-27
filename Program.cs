// See https://aka.ms/new-console-template for more information
using Fractions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using MoreLinq;
using MusicTheory;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static MusicTheory.MusicTheoryUtils;

//MIDI standard: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
//Note time in MIDI is defined in header chunk as number of divisions of quarter beat, e.g. setting "division" to 12 means a quarter beat has 12 divisions.
//Tempo then decides length in seconds of divisions.
/**
Time and length is poorly explained in docs:
Time is when something happens, length is for how long:
    notes.Add(new Melanchall.DryWetMidi.Interaction.Note(NoteName.B, 4)
    {
        Velocity = (SevenBitNumber)64,
        Time = 96,
        Length = 192,
    });
This means "create scientific pitch note B4 at 96 ticks into the midi file, make it 192 ticks long".
**/

// TODO: Start work on beatbox to calculate rhythms based on "How Beat Perception Co-opts Motor Neurophysiology". This can be used to define rhythmic chunks.

string folderPath = @"E:\Documents\Reaper Projects\Melodroid\MIDI_write_testing\";

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(@"D:\Projects\Code\Melodroid\logs\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

//BeatBox beatBox = new BeatBox();
//for (int i = 0; i < 4; i++)
//{
//    WriteMeasuresToMidi(beatBox.TestPhrase().Measures, folderPath, $"beat_box_test_phrase_lcm_15_{i}", true);
//}

//TODO: Check for patterns in complex chords, e.g. in 3/2, 5/4 the 3/2 interval loops twice, cutting 5/4 in "half" and creating a mirrored version -
// - i.e. look for patterns in the numerator part
//TODO: Different approximations require different periodicities, which require longer/shorter time in ms to repeat, e.g. m7 with 7/4, 9/5, 25/14, 23/13, 16/9.
// Preference? Combo with numerator patterns? Does musical context switch which patterns are being screened for when hearing 12TET tones?
//TODO: Investigate lcm vs. pattern ms length when octave transposing notes, e.g. 1, 5/4, 3/2 -> 3/4, 1 , 5/4 -> 1, 4/3, 5/3 :lcm changed from 4 to 3, but ms length is the same!
// is lcm just a proxy for pattern length (via lcm x packet length, where packet length changes when fundamental changes)?
//TODO: Make a dict for Denominator -> midi step and Numerator -> midi step based on rational tuning 2.


BeatBox beatBox = new BeatBox();
List<int> primes = new() { 2, 2, 2, 2, 3, 3, 3, 5, 5, 7 };
PrintTet12FractionApproximations(primes);

//TODO:
// look at 0 1 3 6 8 10 vs. 0 1 3 6 8 9 10 and 0 1 3 5 6 8 10
// - 5/3 sounds very harsh and leaves only 1 compatible interpretation for pax pattern length 24
// - 4/3 "only" removes only 4 interpretations, and only "blacks out" the base 7 scale (and also itself)
QueryKeySetCompatiblePatternLengths(24);

//for (int i = 0; i < 8; i++)
//{
//    WriteMeasuresToMidi(beatBox.TestPhrase().Measures, folderPath, $"melodroid denominator testing {i}", true);
//}

//e.g.:
//BeatBox beatBox = new BeatBox();
//WriteMeasuresToMidi(beatBox.TestPhrase().Measures, folderPath, "melodroid testing");

//Prints possible pattern lengths of inputed keys based on fraction approximations
void QueryKeySetCompatiblePatternLengths(int maxPatternLength = 50)
{
    //Note that transposed keys can display approximations with larger denominators than the pattern length
    // - this is because the key is matched to a fraction approximation inside the octave which is then scaled based on number of transpositions    
    Dictionary<int, HashSet<(int key, Fraction approximation)>> keysCompatibleWithPatternLength = CalculateKeysCompatibleWithPatternLength(maxPatternLength: maxPatternLength);
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for possible pattern lengths of max {maxPatternLength}. (empty input to exit)");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        int[] inputKeys = Array.ConvertAll(input.Split(' '), int.Parse);

        List<int[]> allRotatedKeys = new();
        foreach (int key in inputKeys)
        {
            allRotatedKeys.Add(inputKeys.Select(inputKey => inputKey - key).ToArray());
        }
        List<List<(int patternLength, List<(int key, Fraction approximation)> keysAndApproximations)>> allPatternsAllRotations = new();
        foreach (int[] rotatedKeySet in allRotatedKeys)
        {
            allPatternsAllRotations.Add(CalculatePatternLengthsCompatibleWithInputKeys(keysCompatibleWithPatternLength, rotatedKeySet));
        }

        for (int rotationIndex = 0; rotationIndex < allPatternsAllRotations.Count; rotationIndex++)
        {
            Console.WriteLine($"{string.Join(" ", allRotatedKeys[rotationIndex])} ({string.Join(" ", allRotatedKeys[rotationIndex].OctaveTransposed())})");
            foreach (var pattern in allPatternsAllRotations[rotationIndex])
            {
                Console.WriteLine($"\t{pattern.patternLength}: {string.Join(" ", pattern.keysAndApproximations.Select(keyAndApproximation => keyAndApproximation.approximation))}");
            }
        }

        //int columnWidth = 3 + 5 * inputKeys.Length;
        //foreach (int[] rotatedKeySet in allRotatedKeys)
        //{
        //    Console.Write($"{string.Join(" ", rotatedKeySet)} ({string.Join(" ", rotatedKeySet.OctaveTransposed())})".PadRight(columnWidth));
        //}
        //Console.WriteLine();
        //bool isSomethingPrinted = true; //white lie to start the loop
        //int patternLengthIndex = 0;
        //while (isSomethingPrinted)
        //{
        //    isSomethingPrinted = false;
        //    for (int rotationIndex = 0; rotationIndex < allPatternsAllRotations.Count; rotationIndex++)
        //    {
        //        //more rows to print for pattern
        //        StringBuilder sb = new StringBuilder();
        //        if (allPatternsAllRotations[rotationIndex].Count > patternLengthIndex)
        //        {                    
        //            sb.Append($"{allPatternsAllRotations[rotationIndex][patternLengthIndex].patternLength}: ");
        //            foreach (var keyAndApproximation in allPatternsAllRotations[rotationIndex][patternLengthIndex].keysAndApproximations)
        //            {
        //                sb.Append($"{keyAndApproximation.approximation} ");
        //            }                    
        //            isSomethingPrinted = true;
        //        }
        //        //Allways print to keep formatting 
        //        Console.Write(sb.ToString().PadRight(columnWidth));
        //    }
        //    patternLengthIndex++;
        //    Console.WriteLine();
        //}
    }
}
void WriteMeasuresToMidi(List<Measure> measures, string folderPath, string fileName, bool overWrite = false)
{
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

void PrintLengthOf(ITimeSpan length, long time, TempoMap tempo)
{
    Console.WriteLine($"tempo.TimeDivision: {tempo.TimeDivision}");
    Console.WriteLine($"{nameof(length)}: {length}");
    Console.WriteLine($"{nameof(time)}: {time}");
    Console.WriteLine($"tempo.GetTempoAtTime(length).BeatsPerMinute: {tempo.GetTempoAtTime(length).BeatsPerMinute}");
    Console.WriteLine($"LengthConverter.ConvertFrom(length, time, tempo): {LengthConverter.ConvertFrom(length, time, tempo)}");
}

void WriteMIDIWithMidiEvents(string fullWritePath)
{
    MidiFile midiFile = new MidiFile();
    new TrackChunk(new SetTempoEvent(500000));
    //... etc., seems clunky
    midiFile.Write(fullWritePath);
}

void WriteMIDIWithTimedObjectManager(string fullWritePath)
{
    MidiFile midiFile = new MidiFile();

    using (var tempoManager = new TempoMapManager())
    {
        tempoManager.SetTempo(0, Tempo.FromBeatsPerMinute(60));
        tempoManager.SetTempo(48, Tempo.FromBeatsPerMinute(120));
        tempoManager.SetTempo(144, Tempo.FromBeatsPerMinute(60));
        midiFile.ReplaceTempoMap(tempoManager.TempoMap);
    }

    TrackChunk trackChunk = new TrackChunk();
    using (TimedObjectsManager<Melanchall.DryWetMidi.Interaction.Note> notesManager = trackChunk.ManageNotes())
    {
        TimedObjectsCollection<Melanchall.DryWetMidi.Interaction.Note> notes = notesManager.Objects;

        notes.Add(new Melanchall.DryWetMidi.Interaction.Note(NoteName.A, 4)
        {
            Velocity = (SevenBitNumber)64,
            Time = 0,
            Length = 192,
        });
        notes.Add(new Melanchall.DryWetMidi.Interaction.Note(NoteName.B, 4)
        {
            Velocity = (SevenBitNumber)64,
            Time = 96,
            Length = 192,
        });
    }

    midiFile.Chunks.Add(trackChunk);
    midiFile.Write(fullWritePath, true);
}

void TestMidiWrite(string folderPath)
{
    var noteValues1 = new NoteValue?[8];
    noteValues1[0] = new(NoteName.A, 4, 64);
    noteValues1[4] = NoteValue.SilentNote;
    var noteValues2 = new NoteValue?[7];
    noteValues2[2] = new(NoteName.C, 4, 64);
    var noteValues3 = new NoteValue?[12];
    noteValues3[1] = NoteValue.SilentNote;
    noteValues3[3] = new(NoteName.A, 4, 72);
    noteValues3[8] = NoteValue.SilentNote;

    var measure1 = new Measure(noteValues1);
    var measure2 = new Measure(noteValues2);
    var measure3 = new Measure(noteValues3);

    Console.Write(measure1.NoteVelocitiesString());
    Console.Write(measure2.NoteVelocitiesString());
    Console.Write(measure3.NoteVelocitiesString());
    Console.WriteLine();
    Console.Write(measure1.NoteValuesString());
    Console.Write(measure2.NoteValuesString());
    Console.Write(measure3.NoteValuesString());

    WriteMeasuresToMidi(new List<Measure>() { measure1, measure2, measure3 }, folderPath, "midi_write_test_2", true);
}

static List<(int patternLength, List<(int key, Fraction approximation)> keysAndApproximations)> CalculatePatternLengthsCompatibleWithInputKeys(
    Dictionary<int, HashSet<(int key, Fraction approximation)>> keysCompatibleWithPatternLength, int[] inputKeys)
{
    List<(int patternLength, List<(int key, Fraction approximation)> keysAndApproximations)> compatiblePatternLengths = new();
    foreach (var entry in new SortedDictionary<int, HashSet<(int key, Fraction approximation)>>(keysCompatibleWithPatternLength))
    {
        bool allKeysCompatibleWithPatternLength = true;
        List<(int key, Fraction approximation)> keysAndApproximations = new();
        foreach (int key in inputKeys)
        {
            int octaveTransposedKey = key;
            int transpositions = 0;
            while (octaveTransposedKey < 0)
            {
                octaveTransposedKey += 12;
                transpositions++;
            }
            while (octaveTransposedKey >= 12)
            {
                octaveTransposedKey -= 12;
                transpositions--;
            }

            //get lowest denominator match for transposed key
            List<(int key, Fraction approximation)> candidates = new();
            foreach (var keyAndFraction in entry.Value)
            {
                if (octaveTransposedKey == keyAndFraction.key)
                {
                    Fraction transposedApproximation = keyAndFraction.approximation;
                    if (transpositions > 0)
                        transposedApproximation = new Fraction(
                            keyAndFraction.approximation.Numerator,
                            keyAndFraction.approximation.Denominator * BigInteger.Pow(2, Math.Abs(transpositions)));
                    if (transpositions < 0)
                        transposedApproximation = new Fraction(
                            keyAndFraction.approximation.Numerator * BigInteger.Pow(2, Math.Abs(transpositions)),
                            keyAndFraction.approximation.Denominator);

                    candidates.Add((key, transposedApproximation));
                }
            }
            if (candidates.Count > 0)
            {
                (int key, Fraction approximation) bestCandidate = candidates[0];
                foreach (var candidate in candidates)
                {
                    if (bestCandidate.approximation.Denominator > candidate.approximation.Denominator)
                    {
                        bestCandidate = candidate;
                    }
                }
                keysAndApproximations.Add(bestCandidate);
            }
            else
            {
                allKeysCompatibleWithPatternLength = false;
                break; //all keys must be compatible, no need to check the others.
            }
        }
        if (allKeysCompatibleWithPatternLength)
        {
            compatiblePatternLengths.Add((entry.Key, keysAndApproximations));
        }
    }
    return compatiblePatternLengths;
}