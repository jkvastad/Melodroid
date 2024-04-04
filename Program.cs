// See https://aka.ms/new-console-template for more information
using Fractions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using static MusicTheory.MusicTheoryUtils;
using static System.Formats.Asn1.AsnWriter;
using Scale = MusicTheory.Scale;

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

string folderPath = @"E:\Documents\Reaper Projects\Melodroid\MIDI_write_testing\ScalesByBase";

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

//BeatBox beatBox = new BeatBox();
//List<int> primes = new() { 2, 2, 2, 2, 3, 3, 3, 5, 5, 7 };
//PrintTet12FractionApproximations(primes);

//TODO:
// look at base15: 0 1 3 6 8 10 vs. 0 1 3 6 8 9 10 and 0 1 3 5 6 8 10
// - 5/3 sounds very harsh and leaves only 1 compatible interpretation for pax pattern length 24
// - 4/3 "only" removes 4 interpretations, and only "blacks out" the base 7 scale (and also itself)
//TODO: Check translation + rotation - e.g. rotate scale from base 15 to 12, then translate 12 back to 15 using 12/15=4/5.
// - something interesting happens when the base of the rotation (e.g. base 5 for key 4 in 0 4 7) is rotated to match the fundamental (e.g. multiply with 5/4 for 0 4 7) but the rotated pattern is kept
//TODO: - also interesting results if the rotated pattern is translted to match other keys in the original pattern? e.g. in 0 4 7 base 6 moves to 5 using 6/5? (e.g. C -> A)
//TODO: look into progression 0 2 4 6 7 9 11 -> 0 2 4 5 7 9 10 -> 0 2 4 5 7 9 11 -> 0 1 4 5 7 8 10 (e.g. C -> D -> G -> B -> C)
//TODO: something about subsets in larger sets? e.g. base 15: 0 1 3 5 6 9 10 contains a base 4 subset of 5 9 0
//TODO: In C D G B the B with scale 0 1 4 5 7 8 10 rotates to 7 with base 15: 0 1 3 5 6 9 10 but in the B adding key 3 or 6 sounds bad, even though it should be 6/5 or 7/5 and compatible with 15.
//  - Why? In isolation 0 1 3 4 5 7 8 10 sounds ok(? kinda bad at key 4) and indeed still has base 15, but when coming from G -> B it sounds bad.
//  - - Is it because 0 1 3 4 5 7 8 10 is almost isomorphic with 0 2 4 6 7 9 11 (lacks 3)? 0 2 3 4 6 7 9 11 rotates at 6 to base 15: 0 1 3 5 6 8 9 10 which sounds bad at key 9.
//  - - - Same problem as 15: 0 1 3 6 8 10 - can add key 5 (4/3 - sounds a bit weak) but not key 9 (5/3) even though compatible with 15
//  - - - Can alternatively play 15: 0 1 3 5 6 9 10 but not add key 8 - seems 8/5 interferes with 5 and specially 9.
//  - - - Can not add any of 2, 4 or 7 - as expected, would increase pattern length to 30+
//TODO: When a note seems like it can be added but cannot, perhaps the current scale is simply assumed incorrectly and instead a rotation is the correct scale?
//  - This would cause a compatible tone to actually be incompatible e.g. in 15: 0 1 3 6 8 10 key 9 cannot be added
//  - - It is perhaps in fact 24: 0 2 5 7 9 11 and adding key "9" (5/3 compatible with 15) actually adds key 1 (16/15 incompatible with 24; it would cause pattern length 120)
//  - - - TODO: How to calculate the correct scale?
//  - - - - Rotation 0 with 15: 0 1 3 6 8 10 explains 4/6; external bad notes 2 (9/8 -> 120), 4 (5/4 -> 60), 7 (3/2 -> 30), 9 (5/3), 11 (15/8 -> 120) - good notes 5 (4/3).
//  - - - - Rotation 1 with 24: 0 2 5 7 9 11 explains 6/6; external bad notes 1 (16/15 -> 120), 3 (6/5 -> 120), 6 (7/5 -> 120), 8 (8/5 -> 120), 10 (9/5 -> 120) - good notes 4 (5/4).
//  - - - - Rotation 2 with 24: 0 3 5 7 9 10 explains 3/6; external bad notes 11 (15/8), 1 (16/15 -> 120), 4 (5/4), 6 (7/5 -> 120), 8 (8/5 -> 120) - good notes 2 (9/8).
//  - - - - Rotation 3 with 14: 0 3 5 7 9 10 explains 5/6; external bad notes 8 (8/5 -> 70), 10 (9/5 -> 70), 1 (16/15 -> 105), 3 (6/5 -> 70), 5 (4/3 -> 21) - good notes 11 (15/8 -> 56).
//  - - - - Rotation 4 with 24: 0 2 4 5 7 10 explains 5/6; external bad notes 6 (7/5 -> 120), 8 (8/5 -> 120), 11 (15/8), 1 (16/15 -> 120), 3 (6/5 -> 120) - good notes 9 (5/3).
//  - - - - Rotation 5 with 18: 0 2 3 5 8 10 explains 4/6; external bad notes 4 (5/4), 6 (7/5 -> 90), 9 (5/3), 11 (15/8 -> 72), 1 (6/5 -> 90) - good notes 7 (3/2).
//  - - - TODO: Only 1 valid rotation. Why? When adding keys to a set of keys, at what point does it become a specific base 24 scale? What about base 15 scales? (or 7s?)
//  - - - - Major scale is base 24: 0 2 4 5 7 9 11, what base is minor scale? 0 2 3 5 7 8 10 rotates to major.
//  - - - - 15: 0 1 3 5 6 9 10 has only the one rotation (dim 7 with all base 5 and 3 notes). dim7 is rotationally symmetrical, should have interesting properties
//  - - - - 20: 0 3 4 6 7 8 10 - Aug chord. Rotationally symmetrical. Gives a characterstic bluesy sound with 0 2 3 4 6 7 10 when a major chord is sounded
//  - - - - - for some reason 8 sounds wrong when voiced with major chord. Voicing seems to matter a lot (e.g. minor vs major vs aug vs dim).

//PrintScalesWithDesiredBase();
//WriteAllScaleClassesToMidi(folderPath);
ScaleCalculator scaleCalculator = new();

Scale chord = new(new int[] { 0, 4, 7 });
List<List<(int keySteps, Scale legalBaseScale)>> chordProgressionsPerSuperClass = CalculateChordProgressionsPerSuperClass(scaleCalculator, chord);

//Order progressions by physical keys, noting the origin (scale and key steps causing the keys)
Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> chordProgressionsAndOrigins = OrderChordProgressionsByPhysicalKeys(chordProgressionsPerSuperClass);
//TODO: Collapse subscales into superscales if origin is preserved
Console.WriteLine($"Original chord: {chord}");
Console.WriteLine($"Number of progressions: {chordProgressionsAndOrigins.Keys.Count}");
foreach (Tet12KeySet chordProgression in chordProgressionsAndOrigins.Keys.OrderByDescending(cp => cp.ToIntervalString()).ThenByDescending(cp => cp.NumberOfKeys()))
{
    Console.Write($"{$"{chordProgression.ToIntervalString()}",-20} : ");
    foreach ((int keySteps, Scale legalBaseScale) origin in chordProgressionsAndOrigins[chordProgression])
    {
        Console.Write($"{$"({origin.keySteps}, {origin.legalBaseScale.GetBase()}, {origin.legalBaseScale})",-25} - ");
    }
    Console.WriteLine();
}

//// Order progressions by key step length
//Dictionary<int, HashSet<Scale>> chordProgressionsPerKeyStep = new();
//foreach (List<(int keySteps, Scale legalKeys)> chordProgressions in chordProgressionsPerSuperClass)
//{
//    foreach ((int keySteps, Scale legalKeys) chordProgression in chordProgressions)
//    {
//        if (!chordProgressionsPerKeyStep.ContainsKey(chordProgression.keySteps))
//            chordProgressionsPerKeyStep[chordProgression.keySteps] = new();
//        chordProgressionsPerKeyStep[chordProgression.keySteps].Add(chordProgression.legalKeys);
//        //Console.WriteLine($"{$"{(chordProgression.keySteps + 12) % 12}".PadRight(3)} - {$"{chordProgression.legalKeys.GetBase()}:".PadRight(3)}{chordProgression.legalKeys}");
//    }
//}

//// Order progressions by steps then by base then by alphetical size
//foreach (int keySteps in chordProgressionsPerKeyStep.Keys.OrderByDescending(keyStep => keyStep))
//{
//    Console.WriteLine($"- {keySteps} -");
//    foreach (Scale scale in chordProgressionsPerKeyStep[keySteps].OrderByDescending(Scale => Scale.GetBase()).ThenByDescending(Scale => Scale.ToString()))
//    {
//        Console.WriteLine($"{$"{scale.GetBase()}:".PadRight(3)}{scale}");
//    }
//}

////Order progressions by steps then by base then by scale size, keeping only unique supersets
//foreach (int keySteps in chordProgressionsPerKeyStep.Keys.OrderByDescending(keyStep => keyStep))
//{
//    Dictionary<int, HashSet<Scale>> largestProgressionScalesPerBase = new();
//    foreach (Scale scale in chordProgressionsPerKeyStep[keySteps])
//    {
//        int scaleBase = scale.GetBase();

//        if (!largestProgressionScalesPerBase.ContainsKey(scaleBase))
//            largestProgressionScalesPerBase[scaleBase] = new();

//        HashSet<Scale> largestScalesForBase = largestProgressionScalesPerBase[scaleBase];
//        bool isScaleUnique = true;
//        foreach (Scale largestScale in largestScalesForBase)
//        {
//            if (largestScale.isSubscaleTo(scale))
//            {
//                largestScalesForBase.Remove(largestScale);
//                largestScalesForBase.Add(scale);
//                break;
//            }
//            if (scale.isSubscaleTo(largestScale)) //Scales are subscales to themselves
//                isScaleUnique = false;
//        }
//        if (isScaleUnique)
//            largestScalesForBase.Add(scale);
//    }
//    Console.WriteLine($"- {keySteps} -");
//    foreach (int scaleBase in largestProgressionScalesPerBase.Keys.OrderByDescending(scaleBase => scaleBase))
//    {
//        foreach (Scale scale in largestProgressionScalesPerBase[scaleBase])
//        {
//            Console.WriteLine($"{$"{scale.GetBase()}:".PadRight(3)}{scale}");
//        }
//    }
//}

//PrintAllSuperClassHierarchies();
//Scale chord = new(new int[] { 0, 4, 7 });
//PrintChordSuperClasses(chord);

//TODO: Method to find superclass containing specific base? E.g. if I want to play anything but keep base 8, what are my options?

//PrintDissonantSets(scaleCalculator);

QueryKeySetCompatiblePatternLengths(30);

//Print all fractions of interest
//HashSet<Fraction> fractions = new HashSet<Fraction>();
//for (int denominator = 1; denominator <= 24; denominator++)
//{
//    for (int numerator = denominator; numerator <= 2 * denominator; numerator++)
//    {
//        Fraction fraction = new(numerator, denominator);
//        var numeratorFactors = Factorize((int)fraction.Numerator);
//        var denominatorFactors = Factorize((int)fraction.Denominator);
//        if (numeratorFactors.Any(factor => factor >= 7) || denominatorFactors.Any(factor => factor >= 7))
//            continue;
//        fractions.Add(fraction);
//    }
//}
//List<Fraction> sortedFractions = fractions.OrderBy(fraction => fraction.Denominator).ToList();
//foreach (Fraction fraction in sortedFractions)
//{
//    Console.WriteLine($"{fraction.Numerator}/{fraction.Denominator}");
//}


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
                if (pattern.keysAndApproximations.Any(keyAndApproximation =>
                {
                    var octaveTransposedApproximation = keyAndApproximation.approximation.OctaveTransposed();
                    if (
                    octaveTransposedApproximation.Numerator == 21 ||
                    octaveTransposedApproximation.Numerator == 14 ||
                    octaveTransposedApproximation.Numerator == 25 ||
                    octaveTransposedApproximation.Numerator == 28 ||
                    octaveTransposedApproximation.Numerator == 35 ||
                    octaveTransposedApproximation.Numerator == 27)
                        return true;
                    return false;

                }))
                    continue;
                Console.WriteLine($"\t{pattern.patternLength}: " +
                    $"{string.Join(" ", pattern.keysAndApproximations.Select(keyAndApproximation => keyAndApproximation.approximation))} " +
                    $"({string.Join(" ", pattern.keysAndApproximations.Select(keyAndApproximation => keyAndApproximation.approximation.OctaveTransposed()))})");
            }
        }
    }
}
void WriteMeasuresToMidi(List<Measure> measures, string folderPath, string fileName, bool overWrite = false)
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

void WriteScalesOfBaseToMidi(List<Scale> scales, string folderPath)
{
    /** Example usage
    ScaleCalculator scaleCalculator = new();
    foreach (var baseValue in scaleCalculator.ScalesWithBase.Keys.Order())
    {
        Console.WriteLine($"Base: {baseValue}");
        var scales = scaleCalculator.ScalesWithBase[baseValue].OrderBy(scale => scale.NumberOfKeys());
        WriteScalesOfBaseToMidi(scales.ToList(), folderPath);
        foreach (var scale in scales)
        {
            Console.WriteLine($"{scale}");
        }
    } 
     **/

    for (int i = 0; i < scales.Count; i++)
    {
        Scale scale = scales[i];
        NoteValue?[] noteValues = ScaleCalculator.ScaleToNoteValues(scale);
        Measure measure = new(noteValues);
        List<Measure> measureList = [measure];
        WriteMeasuresToMidi(measureList, folderPath, $"base_{scale.GetBase()}_keys_{scale.NumberOfKeys()}_number_{i}", true);
    }
}
void WriteAllScaleClassesToMidi(string folderPath)
{
    ScaleCalculator scaleCalculator = new ScaleCalculator();
    Console.WriteLine($"Number of scales: {scaleCalculator.ScaleClassForScale.Keys.Count}");
    Console.WriteLine($"Number of scale classes: {scaleCalculator.ScaleClasses.Count}");
    foreach (var length in scaleCalculator.ScaleClassesOfLength.Keys)
        Console.WriteLine($"Scale classes of length {length}: {scaleCalculator.ScaleClassesOfLength[length].Count}");
    foreach (var length in scaleCalculator.ScaleClassesOfLength.Keys)
    {
        int scaleClassIndex = 0;
        foreach (var scaleClass in scaleCalculator.ScaleClassesOfLength[length])
        {
            int scaleIndex = 0;
            Console.WriteLine($"Scales in scale class {length}:{scaleClassIndex}");
            Log.Information($"Scales in scale class {length}:{scaleClassIndex}");
            foreach (var scale in scaleClass)
            {
                Console.WriteLine($"{scale} : {length}_{scaleClassIndex}_{scaleIndex}");
                Log.Information($"{scale} : {length}_{scaleClassIndex}_{scaleIndex}");

                NoteValue?[] noteValues = ScaleCalculator.ScaleToNoteValues(scale);
                Measure measure = new(noteValues);
                List<Measure> measureList = [measure];
                WriteMeasuresToMidi(measureList, folderPath, $"{length}_{scaleClassIndex}_{scaleIndex}", true);
                scaleIndex++;
            }
            scaleClassIndex++;
        }
    }
}

static void PrintScalesWithUpperLimitOnBase()
{
    ScaleCalculator scaleCalculator = new ScaleCalculator();
    List<List<Scale>> scaleClassesWithDesiredBases = new();
    int maxBaseValue = 24;
    foreach (var length in scaleCalculator.ScaleClassesOfLength.Keys)
    {
        int scaleClassIndex = 0;
        foreach (var scaleClass in scaleCalculator.ScaleClassesOfLength[length])
        {
            int scaleIndex = 0;
            bool scaleClassGood = true;
            foreach (var scale in scaleClass)
            {
                int baseValue = scale.GetBase();
                Console.WriteLine($"{length}_{scaleClassIndex}_{scaleIndex}: {baseValue}");
                if (baseValue > maxBaseValue)
                    scaleClassGood = false;
                scaleIndex++;
            }
            if (scaleClassGood)
                scaleClassesWithDesiredBases.Add(scaleClass);
            Console.WriteLine();
            scaleClassIndex++;
        }
    }
    Console.WriteLine($"Scale classes with bases less than {maxBaseValue}: {scaleClassesWithDesiredBases.Count}");
    scaleClassesWithDesiredBases.Sort((scaleClass1, scaleClass2) => scaleClass1[0].NumberOfKeys().CompareTo(scaleClass2[0].NumberOfKeys()));
    foreach (var scaleClass in scaleClassesWithDesiredBases)
    {
        foreach (var scale in scaleClass)
        {
            Console.WriteLine($"{scale}:{scale.GetBase()}");
        }
        Console.WriteLine();
    }
}

void PrintAllSuperClassHierarchies()
{
    int scaleClassIndex = 0;
    int oldScaleLength = 0;
    List<List<Scale>> scaleClasses = scaleCalculator.ScaleClasses.OrderBy(scaleClass => scaleClass[0].NumberOfKeys()).Reverse().ToList();
    foreach (List<Scale> scaleClass in scaleClasses)
    {
        if (scaleClass[0].NumberOfKeys() != oldScaleLength)
        {
            Console.WriteLine($"-- Scale lengths: {scaleClass[0].NumberOfKeys()} --");
            oldScaleLength = scaleClass[0].NumberOfKeys();
        }

        int? superClassIndex = null;
        for (int previousIndex = 0; previousIndex < scaleClassIndex; previousIndex++)
        {
            if (scaleClasses[previousIndex].Any(scale => scaleClass[0].isSubClassTo(scale) && scale.GetBase() <= 24))
            {
                superClassIndex = previousIndex;
                break;
            }
        }

        if (superClassIndex != null)
            Console.WriteLine($"-Scale index: {scaleClassIndex} <- {superClassIndex}");
        else
            Console.WriteLine($"-Scale index: {scaleClassIndex}");

        if (scaleClass.Any(scale => scale.GetBase() <= 24
                //&& superClassIndex == null
                //&& scale.GetBase() != 24
                //&& scale.GetBase() != 20
                //&& scale.GetBase() != 15
                //&& scale.GetBase() != 12
                //&& scale.GetBase() != 10
                //&& scale.GetBase() != 8
                //&& scale.GetBase() != 6
                //&& scale.GetBase() != 5
                )
            )
        {
            foreach (Scale scale in scaleClass)
            {
                Console.WriteLine($"{scale} - {scale.GetBase()}");
            }
        }

        scaleClassIndex++;

    }
}

void PrintChordSuperClasses(Scale chord, int maxBase = 24, int minBase = 0)
{
    List<int> availableBases = new();
    Dictionary<int, int> baseClassIndexLongest = new();
    Dictionary<int, int> baseLengthLongest = new();
    Dictionary<int, int> baseClassIndexShortest = new();
    Dictionary<int, int> baseLengthShortest = new();
    int scaleClassIndex = 0;
    int oldScaleLength = 0;
    foreach (List<Scale> scaleClass in scaleCalculator.CalculateScaleSuperClasses(chord).OrderBy(scaleClass => scaleClass[0].NumberOfKeys()).Reverse())
    {
        if (scaleClass[0].NumberOfKeys() != oldScaleLength)
        {
            Console.WriteLine($"-- Scale lengths: {scaleClass[0].NumberOfKeys()} --");
            oldScaleLength = scaleClass[0].NumberOfKeys();
        }
        Console.WriteLine($"Scale class index: {scaleClassIndex}");

        if (scaleClass.Any(scale => scale.GetBase() <= maxBase && scale.GetBase() >= minBase
            //&& superClassIndex == null
            //&& scale.GetBase() != 24
            //&& scale.GetBase() != 20
            //&& scale.GetBase() != 15
            //&& scale.GetBase() != 12
            //&& scale.GetBase() != 10
            //&& scale.GetBase() != 8
            //&& scale.GetBase() != 6
            //&& scale.GetBase() != 5
            )
        )
        {
            foreach (Scale scale in scaleClass)
            {
                int baseValue = scale.GetBase();
                if (!availableBases.Contains(baseValue))
                {
                    availableBases.Add(baseValue);
                    baseClassIndexLongest[baseValue] = scaleClassIndex;
                    baseLengthLongest[baseValue] = oldScaleLength;
                }
                else if (availableBases.Contains(baseValue))
                {
                    baseClassIndexShortest[baseValue] = scaleClassIndex;
                    baseLengthShortest[baseValue] = oldScaleLength;
                }

                if ((scale.KeySet.BinaryRepresentation & chord.KeySet.BinaryRepresentation) == chord.KeySet.BinaryRepresentation)
                    Console.WriteLine($"{scale.KeySet} - {baseValue} <--");
                else
                    Console.WriteLine($"{scale.KeySet} - {baseValue}");
            }
        }
        scaleClassIndex++;
    }

    Console.WriteLine($"Available bases (index for first found largest key set containing the base):");
    foreach (var baseValue in availableBases.OrderBy(value => value))
    {
        Console.WriteLine($"{baseValue} at {baseClassIndexLongest[baseValue]} length {baseLengthLongest[baseValue]} and " +
            $"at {baseClassIndexShortest.ElementAtOrDefault(baseValue)} length {baseLengthShortest.ElementAtOrDefault(baseValue)}");
    }
}

//Calculate all minimal dissonant sets, i.e. sets of tones which lack a superclass containing a legal base.
void PrintDissonantSets(ScaleCalculator scaleCalculator)
{
    List<int> illegalBases = new() { 24, 20, 15, 12, 10, 8, 5, 4, 3, 2, 1 };
    foreach (int length in scaleCalculator.ScaleClassesOfLength.Keys.OrderBy(key => key).Reverse())
    {
        List<List<Scale>> currentScaleClasses = scaleCalculator.ScaleClassesOfLength[length];
        List<List<Scale>> filteredClasses = new();
        foreach (var scaleClass in currentScaleClasses)
        {
            List<List<Scale>> superClasses = scaleCalculator.CalculateScaleSuperClasses(scaleClass[0]);
            //if (scaleClass.Any(scale => illegalBases.Contains(scale.GetBase())))
            if (superClasses.Any(
                    superClass => superClass.Any(
                        scale => illegalBases.Contains(scale.GetBase())
                    )
                )
            )
            {
                continue;
            }

            filteredClasses.Add(scaleClass);
        }

        Console.WriteLine($"Scale classes of length {length}");

        foreach (var scaleClass in filteredClasses)
        {
            foreach (Scale scale in scaleClass)
            {
                Console.WriteLine($"{scale} - {scale.GetBase()}");
            }
            Console.WriteLine("---");
        }
    }
}

//Chord progressions are basically "hidden keys" in a superclass to a scale, moving from larger to smaller bases in the superscale
static List<List<(int keySteps, Scale legalKeys)>> CalculateChordProgressionsPerSuperClass(ScaleCalculator scaleCalculator, Scale chord)
{
    List<int> LEGAL_BASES = new() { 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 24 };

    List<List<Scale>> superClasses = scaleCalculator.CalculateScaleSuperClasses(chord);
    List<List<(int keySteps, Scale legalBaseScale)>> chordProgressionsPerSuperClass = new();
    foreach (List<Scale> superclass in superClasses)
    {
        Scale referenceScale = superclass.First(); // Arbitrary reference point in the superclass
        List<int> superScaleRotations = new(); // The rotations producing superscales to our chord from an arbitrary reference point in the superclass
        List<(int rotations, Scale scale)> legalBasesAndRotations = new(); // The rotations producing legal bases in the superclass

        for (int rotations = 0; rotations < 12; rotations++)
        {
            Tet12KeySet scaleKeys = referenceScale >> rotations;
            if ((scaleKeys.BinaryRepresentation & 1) != 1)
                continue; // Scales must have a fundamental - also implies that the resulting scale is in the superclass            

            Scale rotatedScale = new(scaleKeys);

            if (chord.isSubScaleTo(rotatedScale))
                superScaleRotations.Add(rotations); // Found superscale to our chord at current rotations

            if (LEGAL_BASES.Contains(rotatedScale.GetBase()))
                legalBasesAndRotations.Add((rotations, rotatedScale)); // Found legal base in superclass
        }

        //Rotation diff between all chord superscales and legal bases in the superclass
        List<(int keySteps, Scale legalKeys)> chordProgressions = new();
        foreach (int chordSuperScaleRotation in superScaleRotations)
        {
            foreach ((int rotations, Scale scale) legalBase in legalBasesAndRotations)
            {
                // From chordSuperscaleRotation, go keySteps rotations to the right and play any keys from legalBase.Scale
                int keySteps = ((legalBase.rotations - chordSuperScaleRotation) + 12) % 12;
                chordProgressions.Add((keySteps, legalBase.scale));
            }
        }
        chordProgressionsPerSuperClass.Add(chordProgressions);
    }

    return chordProgressionsPerSuperClass;
}

static Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> OrderChordProgressionsByPhysicalKeys(List<List<(int keySteps, Scale legalBaseScale)>> chordProgressionsPerSuperClass)
{
    Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> chordProgressionsAndOrigins = new();
    foreach (var superClass in chordProgressionsPerSuperClass)
    {
        foreach ((int keySteps, Scale legalBaseScale) origin in superClass)
        {
            Tet12KeySet keys = origin.legalBaseScale << origin.keySteps; //scale in intervals relative to chord root
            if (!chordProgressionsAndOrigins.ContainsKey(keys))
                chordProgressionsAndOrigins[keys] = new();

            chordProgressionsAndOrigins[keys].Add(origin);
        }
    }

    return chordProgressionsAndOrigins;
}