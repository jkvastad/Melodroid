// See https://aka.ms/new-console-template for more information
using Fractions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melodroid.Harmonizers;
using MusicTheory;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using static MusicTheory.MusicTheoryUtils;
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

string folderPath = @"E:\Documents\Reaper Projects\Melodroid\MIDI_write_testing";

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(@"D:\Projects\Code\Melodroid\logs\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

ScaleCalculator scaleCalculator = new();


//Matrix<double> MajorChordMatrix = DenseMatrix.OfArray(new double[,] {
//        {0,8,5},
//        {4,0,9},
//        {7,3,0}});

//Matrix<double> MajorChordMatrix2 = DenseMatrix.OfArray(new double[,] {
//        {0,7,3},
//        {5,0,8},
//        {9,4,0}});

//Matrix<double> ReversedMajorChordMatrix = DenseMatrix.OfArray(new double[,] {
//        {7,3,0},
//        {4,0,9},
//        {0,8,5}});

//Matrix<double> Major7thChordMatrix = DenseMatrix.OfArray(new double[,] {
//        {0, 8,5,2},
//        {4, 0,9,6},
//        {7, 3,0,9},
//        {10,6,3,0}});

//Matrix<double> MinorChordMatrix = DenseMatrix.OfArray(new double[,] {
//        {0,9,5},
//        {3,0,8},
//        {7,4,0}});

//DenseVector MajorChordVector = DenseVector.OfArray([0, 4, 7]);
//DenseVector Major7thChordVector = DenseVector.OfArray([0, 4, 7, 10]);
//DenseVector MinorChordVector = DenseVector.OfArray([0, 3, 7]);


//var leftTerm = MajorChordMatrix2;
//var rightTerm = MajorChordMatrix;
//var result = leftTerm * rightTerm;
//result %= 12;
//Console.WriteLine(leftTerm);
//Console.WriteLine(rightTerm);
//Console.WriteLine(result);


//TODO add logger for scales used (and other random outcomes during generation)
////Select Rhythm Maker
int timeDivision = 16;
int numberOfMeasures = 32;
int beatsPerMeasure = 8;
int deviationsPerMeasure = 3;
//SimpleIsochronicRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure);
SimpleGrooveRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure, deviationsPerMeasure);
List<List<PatternBlock>> measurePatternBlocks = [
    [new("A", 8), new("B", 4), new("B", 4)],
    [new("A", 8), new("B", 4), new("C", 4)],
    [new("A", 8), new("B", 4), new("B", 4)],
    [new("D", 8), new("B", 4), new("E", 4)]
];
//SimpleMeasurePatternRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure, deviationsPerMeasure, measurePatternBlocks);


//Scale initialScale = new(new int[] { 0, 4, 7 });
//Scale majorChord = new(new int[] { 0, 4, 7 });
//Scale minorChord = new(new int[] { 0, 3, 7 });
//List<Scale> chordProgression = [majorChord, minorChord];

////Select harmonizer
//MelodicSupersetHarmonizerOneFundamentalPerMeasure harmonizer = new([0, 4, 7]);
//MelodicSupersetHarmonizerOddOrEvenBasePerMeasure harmonizer = new([0, 4, 7]);
MelodicSupersetHarmonizerOddOrEvenFixedFundamentalPerMeasure harmonizer = new([0, 4, 7]);
//ScaleClassRotationHarmonizer harmonizer = new(initialScale);
//ChordMeasureProgressionHarmonizer harmonizer = new(chordProgression);
//ScaleClassRotationTransposeHarmonizer harmonizer = new(initialScale);
//RandomKeyMultiplicityHarmonizer harmonizer = new();
//ChordPreferenceKeyMultiplicityHarmonizer harmonizer = new();
//MajorChordCollapsingKeyMultiplicityPhraseHarmonizer harmonizer = new();

//RandomNoteHarmonizer randomNoteHarmonizer = new();

//RandomChordNoteHarmonizer harmonizer = new(initialScale);

//RandomWalkMeasureHarmonizer measureHarmonizer = new(initialScale);
//PathWalkMeasureHarmonizer measureHarmonizer = new(initialScale, initialScale, 4);
//BeatBox beatBox = new BeatBox(rhythmMaker, measureHarmonizer);

//////Write MIDI files
//BeatBox beatBox = new BeatBox(rhythmMaker, harmonizer);

//List<Measure> melodyMeasures = beatBox.MakeMeasures();
//beatBox.WriteMeasuresToMidi(melodyMeasures, folderPath, "melodic_superset_test", true);

//ChordMeasureHarmonizer chordHarmonizer = new(harmonizer.ChordPerMeasure, 4);
//List<Measure> chordMeasures = chordHarmonizer.MeasuresFromVelocities(rhythmMaker.VelocityMeasures);
//beatBox.WriteMeasuresToMidi(chordMeasures, folderPath, "melodic_superset_chord_test", true);

//TODO: Are chord progressions based on finding intervals 3/2 and 5/4 (major chord)? Implies other intervals are renormalized and must fit in with the prioritized intervals
//TODO: Print LCM to simplify comparing interval matches, perhaps filter on max LCM.
while (true)
{
    //QueryRatioFundamentalOctaveSweep(maxDeviation: 0.02d);
    //QueryChordKeyMultiplicityPowerSets(scaleCalculator);
    //QueryFundamentalClassPerScale(scaleCalculator);
    //QueryChordProgressionFromMultiplicity(scaleCalculator);
    //QueryChordInKeySetTranslations();
    //QuerySubsetIntervalsLCMs();
    //QueryMelodicSubsetLCMs();

    //QueryIntervalChordProgressions();
    //QueryChordKeyMultiplicity(scaleCalculator);
    QueryChordPowerSetLCMs();
    QueryTonalSetsFundamentalOverlap();
    //QueryChordIntervalMultiplicity();
    //QueryLCMTonalSetsForFundamental();
    //QueryRealChordIntervalMultiplicity();
    //QueryMelodicSupersetLCMs();
    //QueryIntervalScaleOverlap();
}


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
// - 5/3 sounds very harsh and leaves only 1 compatible interpretation for max pattern length 24
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

////PrintScalesWithDesiredBase();
////WriteAllScaleClassesToMidi(folderPath);

//Scale chord = new(new int[] { 0, 4, 7 });
//List<List<(int keySteps, Scale legalBaseScale)>> chordProgressionsPerSuperClass = scaleCalculator.CalculateSuperClassProgressionsPerSuperClass(chord);
//int superClassIndex = 0;
//foreach (var superClass in chordProgressionsPerSuperClass)
//{
//    Console.WriteLine($"{superClassIndex++}");
//    foreach (var chordProgression in superClass)
//    {
//        Console.WriteLine($"{chordProgression.legalBaseScale}");
//    }
//}

//Order progressions by physical keys, noting the origin (scale and key steps causing the keys)
//Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> chordProgressionsAndOrigins = OrderChordProgressionsByPhysicalKeys(chordProgressionsPerSuperClass);
//PrintChordProgressionsAndOrigins(chord, chordProgressionsAndOrigins);

////Console.WriteLine();

// Collapse subscales into superscales if origin keys steps and base is preserved
//Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> chordProgressionsAndOriginsCollapsed = CollapseChordProgressionsByPhysicalKeys(chordProgressionsAndOrigins);
//PrintChordProgressionsAndOrigins(chord, chordProgressionsAndOriginsCollapsed);

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

//Order progressions by steps then by base then by scale size, keeping only unique supersets
//foreach (int keySteps in chordProgressionsPerKeyStep.Keys.OrderByDescending(keyStep => keyStep))
//{
//    Dictionary<int, HashSet<Scale>> largestProgressionScalesPerBase = new();
//    foreach (Scale scale in chordProgressionsPerKeyStep[keySteps])
//    {
//        int scaleBase = scale.CalculateBase();

//        if (!largestProgressionScalesPerBase.ContainsKey(scaleBase))
//            largestProgressionScalesPerBase[scaleBase] = new();

//        HashSet<Scale> largestScalesForBase = largestProgressionScalesPerBase[scaleBase];
//        bool isScaleUnique = true;
//        foreach (Scale largestScale in largestScalesForBase)
//        {
//            if (largestScale.IsSubScaleTo(scale))
//            {
//                largestScalesForBase.Remove(largestScale);
//                largestScalesForBase.Add(scale);
//                break;
//            }
//            if (scale.IsSubScaleTo(largestScale)) //Scales are subscales to themselves
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
//            Console.WriteLine($"{$"{scale.CalculateBase()}:".PadRight(3)}{scale}");
//        }
//    }
//}

//QueryChordInProgression(chordProgressionsAndOriginsCollapsed);

////Print all scaleclasses with multiple legal bases
//int scaleClassIndex = 0;
//foreach (int length in scaleCalculator.ScaleClassesOfLength.Keys.OrderByDescending(key => key))
//{
//    Console.WriteLine($"-- Scale Length {length} --");
//    foreach (var scaleClass in scaleCalculator.ScaleClassesOfLength[length])
//    {
//        Console.WriteLine($"- Scale Class #{scaleClassIndex}");
//        List<Scale> scalesWithLegalBase = new();
//        foreach (var scale in scaleClass)
//        {
//            if (ScaleCalculator.LEGAL_BASES.Contains(scale.GetBase()))
//                scalesWithLegalBase.Add(scale);
//        }
//        if (scalesWithLegalBase.Count > 1)
//        {
//            foreach (var scale in scalesWithLegalBase)
//            {
//                Console.WriteLine($"{scale} - {scale.GetBase()}");
//            }
//        }
//        scaleClassIndex++;
//    }
//}

//Show only keys resulting from collapsed scales:
//Deep copy
//Dictionary<int, HashSet<Scale>> keysWithBase = new();
//foreach (var key in scaleCalculator.ScalesWithBase.Keys)
//{
//    keysWithBase[key] = [.. scaleCalculator.ScalesWithBase[key]];
//}
////Remove subscales
//foreach (var key in scaleCalculator.ScalesWithBase.Keys)
//{
//    foreach (Scale value in scaleCalculator.ScalesWithBase[key])
//    {
//        if (keysWithBase[key].Any(scale => scale.Contains(value) && scale != value))
//        {
//            keysWithBase[key].Remove(value);
//        }
//    }
//}
////print results
//foreach (var baseSize in keysWithBase.Keys.OrderByDescending(key => key))
//{
//    Console.WriteLine($"Base size {baseSize}");
//    foreach (var scale in keysWithBase[baseSize].OrderByDescending(scale => scale.ToString()))
//    {
//        Console.WriteLine(scale);
//    }
//}

//Console.WriteLine();

//int scaleLength = 3;
//Console.WriteLine($"Scales classes of length {scaleLength}");
//foreach (var scaleClass in scaleCalculator.ScaleClassesOfLength[scaleLength].OrderByDescending(scaleClass => scaleClass.MinBy(scale => scale.CalculateBase()).CalculateBase()))
//{
//    int intervalOfInterest = 6;
//    bool intervalDetected = false;
//    foreach (var scale in scaleClass.OrderBy(scale => scale.CalculateBase()))
//    {
//        Console.Write($"{scale.CalculateBase(),-3}: {scale.ToString().PadRight(scaleLength * 3)} / ");
//        if (scale.ToIntervals().Contains(intervalOfInterest))
//            intervalDetected = true;
//    }
//    if (intervalDetected)
//        Console.Write($"<- {intervalOfInterest}");
//    Console.WriteLine();
//}
//Console.WriteLine();

//QueryScaleClassProgressionsFromScale(scaleCalculator);

//PrintScaleClassUniqueness(scaleCalculator);

//LogRelativePeriodicityForOctaveIntervals(16);

//TODO nånting händer med 0 1 8 och 0 7 11 - känns väldigt annorlunda om man inleder med 0 7 (11) eller 0 8 (1). Som om första intervallet 
//PrintScaleClassAmbiguity(scaleCalculator, true);

//QueryChordsInScale(scaleCalculator);
//QueryChordInKeySetTranslations();

//PrintFractionApproximations();
//PrintCumulativeFractionApproximations(15, 24, true);
//var fractionApproximations = ScaleCalculator.CalculateFractionsForApproximations(15);
//PrintRelativeDeviations(fractionApproximations, 12);
//PrintFractionClasses();
//PrintVirtualFundamentals();
//QueryFractionFundamentalClass();
//PrintFractionFundamentalClass(chord, toOctave: false);

static double[] ConstructTet12DoubleArray(int[] keys)
{
    double[] tet12Doubles = new double[keys.Length];
    for (int i = 0; i < keys.Length; i++)
        tet12Doubles[i] = Math.Pow(2, keys[i] / 12d);
    return tet12Doubles;
}

double[] ConstructTet12FractionFamily(int familyNumerator, int maxNumerator = 25)
{
    List<double> tet12Doubles = new();
    for (int i = 0; i < familyNumerator; i++)
    {
        Fraction currentFraction = new Fraction(i + familyNumerator, familyNumerator);
        if (currentFraction.Numerator <= maxNumerator)
            tet12Doubles.Add(currentFraction.ToDouble());
    }
    return tet12Doubles.ToArray();
}

//Scale chromaticScale = new(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
//Scale base24Scale = new(new int[] { 0, 2, 4, 5, 7, 9, 11 });
//Scale customScale0 = new(new int[] { 0, 2, 4, 5, 7, 8, 9, 11 });
//Scale majorChord = new(new int[] { 0, 4, 7 });
//Scale customScale1 = new(new int[] { 0, 2, 4, 5, 8, 9, 11 });
//Scale customScale2 = new(new int[] { 0, 2, 4, 5, 7, 8, 11 });
//Scale base15Scale = new(new int[] { 0, 1, 3, 5, 6, 8, 9, 10 });
//double[] tet12Base15Scale = ConstructTet12DoubleArray(new int[] { 0, 1, 3, 5, 6, 8, 9, 10 });
//double[] tet12Base20Scale = ConstructTet12DoubleArray(new int[] { 0, 3, 4, 6, 7, 8, 10 });
//double[] tet12Base24Scale = ConstructTet12DoubleArray(new int[] { 0, 2, 4, 5, 7, 9, 11 });
//Scale base20Scale = new(new int[] { 0, 3, 4, 6, 7, 8, 10 });
//double[] tet12MinorChord = ConstructTet12DoubleArray(new int[] { 0, 3, 7 });
//double[] tet12Custom0 = ConstructTet12DoubleArray(new int[] { 9, 10, 1, 4 });
//double[] tet12base7 = ConstructTet12FractionFamily(7);

//for (int i = 2; i <= 25; i++)
//{
//    Console.WriteLine($"Fraction family {i}:");
//    PrintSlidingFundamentalMatchingBetweenScales(ConstructTet12FractionFamily(i), chromaticScale, true);
//    Console.WriteLine("---");
//}


//Scale base15ScaleLeft = new(new int[] { 0, 1, 3, 5, 6, 8, 9 });
//Scale base15ScaleRight = new(new int[] { 0, 1, 3, 5, 6, 9, 10 });
//Scale base30Scale = new(new int[] { 0, 1, 3, 5, 6, 7, 8, 9, 10 });
//List<Fraction> base15FractionsFrom0 = base15Scale.ToFractions();
//List<Fraction> base15FractionsFrom1 = [.. base15FractionsFrom0.Select(fraction => (fraction * new Fraction(15, 16)).ToOctave())];

//PrintClosestFractionsBetweenScales(base24Scale, base24Scale);
//Console.WriteLine("---");
//PrintClosestDoublesBetweenScales(tet12Base24Scale, base24Scale);
//Console.WriteLine("---");
//PrintClosestFractionsBetweenScales(base15Scale, base24Scale);
//Console.WriteLine("---");
//PrintClosestDoublesBetweenScales(tet12Base15Scale, base24Scale);
//Console.WriteLine("---");
//PrintClosestFractionsBetweenScales(base20Scale, base24Scale);
//Console.WriteLine("---");
//PrintClosestDoublesBetweenScales(tet12Base20Scale, base24Scale);
//Console.WriteLine("---");
//PrintClosestDoublesBetweenScales(tet12Base15Scale, base24Scale);

//PrintSlidingFundamentalMatchingBetweenScales(ConstructTet12FractionFamily(24), chromaticScale, false);
//Console.WriteLine("---");
//PrintSlidingFundamentalMatchingBetweenScales(tet12Base15Scale, chromaticScale, true);
//Console.WriteLine("---");
//PrintSlidingFundamentalMatchingBetweenScales(tet12base7, chromaticScale, true);
//Console.WriteLine("---");
//PrintSlidingFundamentalMatchingBetweenScales(tet12base7, majorChord, false);
//Console.WriteLine("---");
//double[] myRatios = new double[] { 6 / 5d, 4 / 3d, 5 / 3d };
//double[] myRatios2 = new double[] { 5 / 4d };
//double[] myRatios3 = new double[] { 1, 16 / 15d };
//double[] myRatios4 = new double[] { 1, 15 / 18d };

//double[] cumulativeDScale6 = new double[] { 1, 7 / 6d, 8 / 6d, 9 / 6d, 10 / 6d, 11 / 6d };

//double[] fSharp = new double[] { 9 / 5d };
//double[] majorC = new double[] { 1, 5 / 4d, 3 / 2d };
//double[] tet12majorC = ConstructTet12DoubleArray(new int[] { 0, 4, 7 });
//double[] dimCSharp = new double[] { 16 / 15d, 5 / 4d, 3 / 2d };
//double[] dimC = new double[] { 1, 6 / 5d, 7 / 5d };
//double[] tet12dimC = ConstructTet12DoubleArray(new int[] { 0, 3, 6 });
//double[] majorD = new double[] { 9 / 8d, 7 / 5d, 5 / 3d };
//double[] tet12majorC7 = ConstructTet12DoubleArray(new int[] { 0, 4, 7, 10 });
//double[] majorC7_7_4 = new double[] { 1, 5 / 4d, 3 / 2d, 7 / 4d };
//double[] majorC7_9_5 = new double[] { 1, 5 / 4d, 3 / 2d, 9 / 5d };
//double[] majorC7_16_9 = new double[] { 1, 5 / 4d, 3 / 2d, 16 / 9d };
//double[] minorF = new double[] { 4 / 3d, 8 / 5d, 2 };
//double[] minorE = new double[] { 5 / 4d, 3 / 2d, 15 / 8d };
//double[] minorF4 = new double[] { 4 / 3d, 8 / 5d, 9 / 5d, 2 };
//double[] minorF7 = new double[] { 4 / 3d, 8 / 5d, 2, 6 / 5d };

//double[] pentatonic = new double[] { 1, 9 / 8d, 5 / 4d, 3 / 2d, 5 / 3d };

//PrintRatioFundamentalOctaveSweep(majorD.Union(minorF7).ToArray());
//PrintRatioFundamentalOctaveSweep(minorE.Union(fSharp).ToArray());
//PrintRatioFundamentalOctaveSweep(majorC.Union(myRatios).ToArray());

//TODO - when playing e.g. A# in C7, is it experienced purely as 9/5 or does it work as 7/4 and 16/9 as well?
//(seems probable given how base 6 scale is off by 0.3 and still approximates 5/3,4/3 etc.)
//if so, how to capture all the implications/harmonic dynamics of a single key? All values in an interval around key? 
//PrintRatioFundamentalOctaveSweep(majorC);
//Console.WriteLine($"---{nameof(majorC)}---");
//PrintRatioFundamentalOctaveSweep(tet12majorC);
//Console.WriteLine($"---{nameof(tet12majorC)}---");
//PrintRatioFundamentalOctaveSweep(dimC);
//Console.WriteLine($"---{nameof(dimC)}---");
//PrintRatioFundamentalOctaveSweep(tet12dimC);
//Console.WriteLine($"---{nameof(tet12dimC)}---");

//PrintRatioFundamentalOctaveSweep(tet12majorC7);
//Console.WriteLine($"---{nameof(tet12majorC7)}---");

//PrintRatioFundamentalOctaveSweep(majorC7_7_4);
//Console.WriteLine($"---{nameof(majorC7_7_4)}---");
//PrintRatioFundamentalOctaveSweep(majorC7_9_5);
//Console.WriteLine($"---{nameof(majorC7_9_5)}---");
//PrintRatioFundamentalOctaveSweep(majorC7_16_9);
//Console.WriteLine($"---{nameof(majorC7_16_9)}---");

//PrintRatioFundamentalOctaveSweep(myRatios3);
//Console.WriteLine("---");
//PrintRatioFundamentalOctaveSweep(myRatios4);


////Print all scales with superclasses (including self) of lesser/equal base
//Dictionary<Scale, List<Scale>> leqScalesPerScale = CalculateAllLEQScalesPerScale(scaleCalculator);

////print it
//int scaleLength = 1;
//foreach (var scale in leqScalesPerScale.Keys.OrderByDescending(key => key.NumberOfKeys()).ThenByDescending(key => key.ToString()))
//{
//    if (scale.NumberOfKeys() > scaleLength)
//    {
//        Console.WriteLine($"-- Scale Length {scaleLength} --");
//        scaleLength = scale.NumberOfKeys();
//    }

//    Console.WriteLine($"{scale.CalculateBase(),-2} - {scale}:");
//    foreach (var leqScale in leqScalesPerScale[scale].OrderByDescending(leqScale => leqScale.CalculateBase()).ThenByDescending(leqScale => leqScale.NumberOfKeys()))
//    {
//        //print which keys in the leq scale the original scale matches to, and the fundamental note shift
//        List<int> scaleIntervalsInLeqScale = new();
//        int fundamentalShift = 0;
//        for (int i = 0; i < 12; i++)
//        {
//            if (((leqScale.KeySet.BinaryRepresentation >> i) & scale.KeySet.BinaryRepresentation) == scale.KeySet.BinaryRepresentation)
//            {
//                scaleIntervalsInLeqScale = scale.ToIntervals().Select(interval => (interval + i) % 12).OrderBy(interval => interval).ToList();
//                fundamentalShift = (12 - i) % 12; //rotations of leq scale fundamental to match up with scale fundamental
//                break;
//            }
//        }
//        Console.WriteLine($" - {leqScale.CalculateBase(),-2} - {leqScale,-17}  -> {fundamentalShift} : {string.Join(" ", scaleIntervalsInLeqScale)}");
//    }
//}

//QueryLEQSuperclass(leqScalesPerScale);

//PrintAllSuperClassHierarchies(scaleCalculator);
//Scale chord = new(new int[] { 0, 2, 3, 7, 10 });
//PrintChordSuperClasses(scaleCalculator, chord);

//QueryKeySetCompatiblePatternLengths(40);

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

static void QueryTonalSetsFundamentalOverlap()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];

    while (true)
    {
        Console.WriteLine($"Input origin tonal set, or empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }

        string[] splitInput = input.Split(' ');
        List<string> options = splitInput.Where(chars => !int.TryParse(chars, out _)).ToList();
        bool lcmMatchOnly = false;
        bool lcmPartialMatch = false;
        foreach (string option in options)
        {
            switch (option)
            {
                case "m":
                    lcmMatchOnly = true;
                    break;
                case "p":
                    lcmPartialMatch = true;
                    break;
                default:
                    break;
            };
        }

        string[] keys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        int[] tet12Keys = Array.ConvertAll(keys, int.Parse);

        //Calculate Origin Data
        Dictionary<int, List<List<int>>> originCardinalSets = GetPowerSet(tet12Keys).GroupBy(set => set.Count).ToDictionary(
        group => group.Key,
        group => group.ToList());

        Dictionary<int, Dictionary<int, List<long>>> originLCMPerSubsetPerFundamentalPerCardinality = new();
        foreach (int cardinality in originCardinalSets.Keys)
        {
            if (cardinality < 2) continue; // not interested in single notes
            List<List<int>> cardinalSet = originCardinalSets[cardinality];
            Dictionary<int, List<long>> lcmPerSubsetPerFundamental = new();

            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                List<List<int>> renormalizedSets = cardinalSet.Select(
                    set => set.Select(key => (key - fundamental + 12) % 12).ToList())
                    .ToList();

                List<long> LcmPerSet = new();
                for (int i = 0; i < renormalizedSets.Count; i++)
                {
                    List<int> set = renormalizedSets[i];
                    if (set.Any(interval => standardFractions[interval] == 0))
                        LcmPerSet.Add(0); //0 to indicate invalid interval                    
                    else
                    {
                        long lcm = LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray());
                        if (8 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (10 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (24 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (15 % lcm == 0)// use base 15                                                    
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else
                            LcmPerSet.Add(0);
                    }
                }
                lcmPerSubsetPerFundamental[fundamental] = LcmPerSet;
            }
            originLCMPerSubsetPerFundamentalPerCardinality[cardinality] = lcmPerSubsetPerFundamental; //check cardinalSets[cardinality] for related sets
        }

        //Calculate Target Data
        Console.WriteLine($"Input target tonal set");
        input = Console.ReadLine();

        splitInput = input.Split(' ');
        keys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        tet12Keys = Array.ConvertAll(keys, int.Parse);

        Dictionary<int, List<List<int>>> targetCardinalSets = GetPowerSet(tet12Keys).GroupBy(set => set.Count).ToDictionary(
        group => group.Key,
        group => group.ToList());

        Dictionary<int, Dictionary<int, List<long>>> targetLCMPerSubsetPerFundamentalPerCardinality = new();
        foreach (int cardinality in targetCardinalSets.Keys)
        {
            if (cardinality < 2) continue; // not interested in single notes
            List<List<int>> cardinalSet = targetCardinalSets[cardinality];
            Dictionary<int, List<long>> lcmPerSubsetPerFundamental = new();

            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                List<List<int>> renormalizedSets = cardinalSet.Select(
                    set => set.Select(key => (key - fundamental + 12) % 12).ToList())
                    .ToList();

                List<long> LcmPerSet = new();
                for (int i = 0; i < renormalizedSets.Count; i++)
                {
                    List<int> set = renormalizedSets[i];
                    if (set.Any(interval => standardFractions[interval] == 0))
                        LcmPerSet.Add(0); //0 to indicate invalid interval                    
                    else
                    {
                        long lcm = LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray());
                        if (8 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (10 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (24 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (15 % lcm == 0)// use base 15                                                    
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else
                            LcmPerSet.Add(0);
                    }
                }
                lcmPerSubsetPerFundamental[fundamental] = LcmPerSet;
            }
            targetLCMPerSubsetPerFundamentalPerCardinality[cardinality] = lcmPerSubsetPerFundamental; //check cardinalSets[cardinality] for related sets
        }

        //Print data
        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            Console.WriteLine($"{fundamental}:");
            foreach (var originCardinality in originLCMPerSubsetPerFundamentalPerCardinality.Keys)
            {
                for (int originSubsetIndex = 0; originSubsetIndex < originLCMPerSubsetPerFundamentalPerCardinality[originCardinality][fundamental].Count; originSubsetIndex++)
                {
                    foreach (var targetCardinality in targetLCMPerSubsetPerFundamentalPerCardinality.Keys)
                    {
                        for (int targetSubsetIndex = 0; targetSubsetIndex < targetLCMPerSubsetPerFundamentalPerCardinality[targetCardinality][fundamental].Count; targetSubsetIndex++)
                        {
                            int originBase = (int)originLCMPerSubsetPerFundamentalPerCardinality[originCardinality][fundamental][originSubsetIndex];
                            int targetBase = (int)targetLCMPerSubsetPerFundamentalPerCardinality[targetCardinality][fundamental][targetSubsetIndex];
                            List<int> originSubset = originCardinalSets[originCardinality][originSubsetIndex];
                            List<int> targetSubset = targetCardinalSets[targetCardinality][targetSubsetIndex];

                            if (originBase > 0 && targetBase > 0)
                            {
                                if (lcmMatchOnly)
                                {
                                    if (originBase == targetBase)
                                        Console.WriteLine($"{originBase,-2} {targetBase,-2} ({string.Join(" ", originSubset)})({string.Join(" ", targetSubset)})");
                                }
                                else if (lcmPartialMatch)
                                {
                                    List<int> targetFactors = Factorize(targetBase);
                                    if (Factorize(originBase).Any(originFactor => targetFactors.Contains(originFactor)))
                                        Console.WriteLine($"{originBase,-2} {targetBase,-2} ({string.Join(" ", originSubset)})({string.Join(" ", targetSubset)})");
                                }
                                else
                                    Console.WriteLine($"{originBase,-2} {targetBase,-2} ({string.Join(" ", originSubset)})({string.Join(" ", targetSubset)})");
                            }
                        }
                    }
                }
            }
            Console.WriteLine();
        }
    }
}
static void QueryLCMTonalSetsForFundamental()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];

    while (true)
    {
        Console.WriteLine($"Input (optional) desired LCMs then fundamental for respective LCM tonal sets, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }

        string[] splitInput = input.Split(' ');

        int fundamental = int.Parse(splitInput.Last());

        List<int> desiredLCMs = new();
        if (splitInput.Count() > 1)
            desiredLCMs = [.. Array.ConvertAll(splitInput.SkipLast(1).ToArray(), int.Parse)];

        //Calculate Data
        Dictionary<int, Dictionary<int, List<List<int>>>> cardinalityPerLCMsPerTonalSets = new();
        for (int i = 1; i < Math.Pow(2, 12); i++) //all 12 key permutations
        {
            List<int> intervals = new();
            for (int j = 0; j < 12; j++) //add key j if j is in the tonal set
                if (((i >> j) & 1) == 1)
                    intervals.Add(j);

            var renormalizedIntervals = intervals.Select(key => (key - fundamental + 12) % 12);
            if (renormalizedIntervals.Any(key => standardFractions[key] == 0)) //skip tonal set if it contains illegal keys
                continue;

            int lcm = (int)LCM(renormalizedIntervals.Select(key => (long)standardFractions[key].Denominator).ToArray());

            if (desiredLCMs.Count > 0 && !desiredLCMs.Contains(lcm)) //possibly skip undesired LCM
                continue;

            //Possibly init dictionaries
            if (!cardinalityPerLCMsPerTonalSets.ContainsKey(intervals.Count))
                cardinalityPerLCMsPerTonalSets[intervals.Count] = new();
            if (!cardinalityPerLCMsPerTonalSets[intervals.Count].ContainsKey(lcm))
                cardinalityPerLCMsPerTonalSets[intervals.Count][lcm] = new();
            cardinalityPerLCMsPerTonalSets[intervals.Count][lcm].Add(intervals);
        }

        //Print Data
        for (int cardinality = 0; cardinality < 12; cardinality++)
        {
            if (!cardinalityPerLCMsPerTonalSets.ContainsKey(cardinality))
                continue;
            Console.WriteLine($"cardinality: {cardinality}");
            foreach (int lcm in cardinalityPerLCMsPerTonalSets[cardinality].Keys.OrderBy(lcm => lcm))
            {
                Console.WriteLine($" lcm: {lcm}");
                foreach (var tonalSet in cardinalityPerLCMsPerTonalSets[cardinality][lcm]
                    .OrderBy(tonalSet => string.Join(" ", tonalSet.OrderBy(key => key)))) //sort lexicographically
                {
                    //Console.WriteLine($"{$"{lcm}".PadRight(3)} - {string.Join(" ", tonalSet)}");
                    Console.WriteLine($"  {string.Join(" ", tonalSet)}");
                }
            }
        }
    }
}


//Returns all scales (as a tuple of fundamental and base) which contains the keys from each interval of the chord
Dictionary<(int, int), List<(int fundamental, int @base)>> GetIntervalScaleMatches(int[] tet12Keys)
{
    Dictionary<int, int[]> scalesPerBase = new();
    scalesPerBase[8] = [0, 2, 4, 7, 11];
    scalesPerBase[15] = [0, 1, 3, 5, 8, 9, 10];

    List<(int, int)> inputPairs = GetPowerSet(tet12Keys).Where(set => set.Count == 2).Select(pair => (pair.First(), pair.Last())).ToList();
    Dictionary<(int, int), List<(int fundamental, int @base)>> scalesMatchingPair = new();
    foreach (var pair in inputPairs)
    {
        scalesMatchingPair[pair] = new();
        foreach (var @base in scalesPerBase.Keys)
        {
            var scale = scalesPerBase[@base];
            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                var renormalizedScale = scale.Select(key => (key + fundamental) % 12).ToList();
                if (renormalizedScale.Intersect([pair.Item1, pair.Item2]).Count() == 2)
                    scalesMatchingPair[pair].Add((fundamental, @base));
            }
        }
    }
    return scalesMatchingPair;
}

List<(List<int> chord, (int, int) pair, int fundamental, int @base)> GetIntervalChordProgressions(
    Dictionary<(int, int), List<(int fundamental, int @base)>> intervalScaleMatches)
{
    List<List<int>> standardChords = [
        [0, 3, 7],
        [0, 3, 6],
        [0, 4, 7]];

    Dictionary<int, int[]> scalesPerBase = new();
    scalesPerBase[8] = [0, 2, 4, 7, 11];
    scalesPerBase[15] = [0, 1, 3, 5, 8, 9, 10];

    List<(List<int> chord, (int, int) pair, int fundamental, int @base)> ChordProgressions = new();
    foreach (var chord in standardChords)
    {
        foreach (var pair in intervalScaleMatches.Keys)
        {
            foreach (var fundamentalAndBase in intervalScaleMatches[pair])
            {
                var currentScale = scalesPerBase[fundamentalAndBase.@base].Select(key => (key + fundamentalAndBase.fundamental) % 12);
                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    var renormalizedChord = chord.Select(interval => (interval + fundamental) % 12).ToList();
                    if (currentScale.Intersect(renormalizedChord).Count() == renormalizedChord.Count())
                        ChordProgressions.Add((renormalizedChord, (pair.Item1, pair.Item2), fundamentalAndBase.fundamental, fundamentalAndBase.@base));
                }
            }
        }
    }
    return ChordProgressions;
}

//The intervals of a chord are subsets of scales which contain chords - thus defining chord progressions from intervals
void QueryIntervalChordProgressions()
{
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for interval chord progressions, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }

        string[] splitInput = input.Split(' ');
        List<string> options = splitInput.Where(chars => !int.TryParse(chars, out _)).ToList(); //Might add options later
        bool shortVersion = false;
        bool reverseLookup = false;
        foreach (string option in options)
        {
            switch (option)
            {
                case "r":
                    reverseLookup = true;
                    break;
                case "s":
                    shortVersion = true;
                    break;
                default:
                    break;
            };
        }

        string[] keys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        int[] tet12Keys = Array.ConvertAll(keys, int.Parse);

        //calculate data                
        Dictionary<(int, int), List<(int fundamental, int @base)>> intervalScaleMatches = new();
        List<(List<int> chord, (int, int) pair, int fundamental, int @base)> chordProgressions = new();
        if (reverseLookup)
        {
            List<List<int>> standardChords = [
                [0, 3, 7],
                [0, 3, 6],
                [0, 4, 7]];
            foreach (var chord in standardChords)
            {
                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    var currentChord = chord.Select(interval => (interval + fundamental) % 12).ToList();
                    var currentProgressions = GetIntervalChordProgressions(GetIntervalScaleMatches(currentChord.ToArray()));
                    var reverseProgressions = currentProgressions
                        .Where(progression => progression.chord.Intersect(tet12Keys).Count() == tet12Keys.Count()) //only keep chords pointing to target chord
                        .Select(progression => (currentChord, progression.pair, progression.fundamental, progression.@base)); //origin is reversed
                    chordProgressions.AddRange(reverseProgressions);
                }
            }
        }
        else
        {
            intervalScaleMatches = GetIntervalScaleMatches(tet12Keys);
            chordProgressions = GetIntervalChordProgressions(intervalScaleMatches);
        }

        //print data
        foreach (var chordGroup in chordProgressions.GroupBy(data => string.Join(" ", data.chord.OrderBy(key => key))).OrderBy(group => group.Key))
        {
            Console.WriteLine(chordGroup.Key);
            foreach (var pairGroup in chordGroup.GroupBy(data => data.pair))
            {
                Console.Write($"{pairGroup.Key}: ");
                foreach (var data in pairGroup.OrderBy(a => a.pair).ThenBy(a => a.@base).ThenBy(a => a.fundamental))
                {
                    Console.Write($"{$"{data.@base}@{data.fundamental}".PadRight(5)} ");
                }
                Console.WriteLine();
            }
        }

        //foreach (var pair in inputPairs)
        //{
        //    Console.WriteLine($"{pair}:");
        //    var bases = pairToBasesAtFundamentals[pair].GroupBy(data => data.@base).OrderBy(group => group.Key);
        //    foreach (var baseGroup in bases)
        //    {
        //        if (!shortVersion)
        //            Console.WriteLine($" Base: {baseGroup.Key}");
        //        var fundamentals = baseGroup.GroupBy(data => data.fundamental).OrderBy(group => group.Key);
        //        foreach (var fundamentalGroup in fundamentals)
        //        {
        //            if (!shortVersion)
        //                Console.WriteLine($"  Fundamental: {fundamentalGroup.Key}");
        //            var currentScale = scalesPerBase[baseGroup.Key].Select(key => (key + fundamentalGroup.Key) % 12);
        //            foreach (var chord in standardChords)
        //            {
        //                if (!shortVersion)
        //                {
        //                    Console.WriteLine($"   Chord: {string.Join(" ", chord)}");
        //                    Console.Write("    ");
        //                }
        //                for (int fundamental = 0; fundamental < 12; fundamental++) //check chord matches versus base at grouped fundamental
        //                {
        //                    var renormalizedChord = chord.Select(interval => (interval + fundamental) % 12).ToList();
        //                    if (currentScale.Intersect(renormalizedChord).Count() == renormalizedChord.Count())
        //                        Console.Write($"{string.Join(" ", renormalizedChord)} - ");
        //                }
        //                Console.WriteLine();
        //            }
        //        }
        //    }
        //}
    }
}

//Each interval in a chord has an lcm mapping to a scale, superimposing all these scales shows which keys best fit the chord for melody
void QueryIntervalScaleOverlap()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    Dictionary<int, int[]> scalesPerBase = new();
    scalesPerBase[3] = [0, 5, 9];
    //scalesPerBase[4] = [0, 4, 7];
    scalesPerBase[5] = [0, 3, 8, 10];
    scalesPerBase[8] = [0, 2, 4, 7, 11];
    //scalesPerBase[15] = [0, 1, 3, 5, 8, 9, 10];//too big scales? actually only smaller ones like 2, 3, 5, 8?   
    //scalesPerBase[20] = [0, 3, 4, 7, 8, 10];
    //scalesPerBase[24] = [0, 2, 4, 5, 7, 9, 11];

    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for interval scale superpositons, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }

        string[] splitInput = input.Split(' ');
        List<string> options = splitInput.Where(chars => !int.TryParse(chars, out _)).ToList();
        bool summary = false;
        foreach (string option in options)
        {
            switch (option)
            {
                case "s":
                    summary = true;
                    break;
                default:
                    break;
            };
        }

        string[] keys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        int[] tet12Keys = Array.ConvertAll(keys, int.Parse);

        //calculate data
        List<List<int>> inputPairs = GetPowerSet(tet12Keys).Where(set => set.Count == 2).ToList();
        Dictionary<int, List<long>> LcmPairsPerFundamental = new();
        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            List<List<int>> renormalizedPairs = inputPairs.Select(
                pairs => pairs.Select(key => (key - fundamental + 12) % 12).ToList())
                .ToList();

            List<long> LcmPerPair = new();
            foreach (List<int> pair in renormalizedPairs)
            {
                if (pair.Any(interval => interval == 6))
                    LcmPerPair.Add(0); //0 to indicate invalid interval, not using 7/5
                else
                    LcmPerPair.Add(LCM(pair.Select(key => (long)standardFractions[key].Denominator).ToArray()));
            }

            LcmPairsPerFundamental[fundamental] = LcmPerPair;
        }

        //print data
        Console.Write($" ".PadRight(4));
        for (int key = 0; key < 12; key++)
            Console.Write($"{key}".PadRight(3));
        Console.WriteLine();
        foreach (var fundamental in LcmPairsPerFundamental.Keys)
        {
            if (!tet12Keys.Contains(fundamental)) //real bases only
                continue;
            foreach (var lcmPerPair in LcmPairsPerFundamental[fundamental])
            {
                List<int[]> currentScales = [];
                if (lcmPerPair == 0) continue;
                foreach (var baseSize in scalesPerBase.Keys)
                    if (baseSize % lcmPerPair == 0)
                        currentScales.Add(scalesPerBase[baseSize]);

                if (currentScales.Count > 0)
                {
                    foreach (var currentScale in currentScales)
                    {
                        Console.Write($"{fundamental}:".PadRight(4));
                        for (int key = 0; key < 12; key++)
                        {
                            if (currentScale.Contains((key - fundamental + 12) % 12)) //check if key in renomarlized scale
                                Console.Write($"{lcmPerPair}".PadRight(3));
                            else
                                Console.Write($" ".PadRight(3));
                        }
                        Console.WriteLine();
                    }
                    //if (summary) //TODO: include summary?
                    //{                        
                    //    //summary
                    //    // - which bases match chord                        
                    //    foreach (var currentScale in currentScales)
                    //    {
                    //        for (int key = 0; key < 12; key++)
                    //        {
                    //            if (currentScale.Contains((key - fundamental + 12) % 12))
                    //                Console.Write($"{lcmPerPair}".PadRight(3));
                    //            else
                    //                Console.Write($" ".PadRight(3));
                    //        }
                    //        Console.WriteLine();
                    //    }
                    //}
                }
            }
        }
    }
}


//A chord can belong to many scales.
//e.g. for chord 0 4 7 and scale 0 2 4 5 7 9 11 the chord matches at positions 0, 5 and 7.
//e.g. for chord 0 4 7 and scale 0 1 3 5 6 8 9 the chord matches at positions 1, 5 and 8.
// - Given chord 0 4 7, it could belong to either of the above scales, and the scales' fundamentals could be at key 0, 7, 5 or key 11, 7, 4, respectively.
//This is called chord multiplicity: a chord can belong to different scales, but also belong to different positions in a scale.
// - Which different scales the chord belong to is called "chord scale multiplicity":
// -- given a chord, there is a set (a multiplicity) of different scales containing that chord.
// - Which positions the chord can hold in a scale is related to the "chord scale fundamental set":
// -- given a chord and scale, there is a set of fundamentals for that scale so that the scale contains the chord.
//Given a chord and scale, applying the chord scale fundamental set to the scale produces a key set for each fundamental.
// - each key thus belongs to a number (possibly 0) of fundamentals, this is called "chord key multiplicity" or simply key multiplicity
// -- chord key multiplicity tells us which keys imply which scales, possibly the basis for melody
void QueryChordKeyMultiplicity(ScaleCalculator scaleCalculator)
{
    List<Scale> scalesOfInterest = [
        //new([0, 7]), //base 2@0
        //new([0, 5, 9]), //base 3@0, 4@5
        new([0, 4, 7]), //base 4@0, 3@7
                        //new([0, 3, 8, 10]), //base 5@0, 6@3                
                        //new([0, 5, 7, 9]), //base 6@0                
        new([0, 2, 4, 7, 11]), //base 8@0, 10@4, 12@7
                               //new([0, 3, 7, 8, 10]), //base 10@0
                               //new([0, 4, 5, 7, 9]), //base 12@0
        new([0, 1, 3, 5, 8, 9, 10]), //base 15@0
                                     //new([0, 3, 4, 7, 8, 10]), //base 20@0
                                     //new([0, 3, 4, 7, 8, 10]), //full base 20        
                                     //new([0, 1, 3, 5, 9, 10]), //natural base 15 - no 8 as it collapses to 24 on 1, no 6 as 7 is bad numerator in 7/5
                                     //new([0, 3, 4, 7, 8, 10]), //full base 20                                                                          
                                     //new([0, 2, 4, 5, 7, 9, 11]),  //base 24@0 - 1, 9/8, 5/4, 5/4, 3/2, 5/3, 15/8
                                     //new([0, 1, 3, 5, 7, 8, 9, 10]), //base 30@0
    ];

    Console.WriteLine("Matching input against scales:");
    foreach (Scale scale in scalesOfInterest)
    {
        Console.WriteLine($"{scale.CalculateBase(),-2}: {scale}");
    }

    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for chord to calculate chord key multiplicity (see method comment for theory - empty input to exit)");
        string chordInput = Console.ReadLine();

        if (chordInput.Length == 0)
            return;
        if (chordInput == "clear")
        {
            Console.Clear();
            continue;
        }

        Tet12KeySet chord = new(Array.ConvertAll(chordInput.Split(' '), int.Parse));
        //print 12-TET keys
        Console.Write($" ".PadRight(4));
        for (int i = 0; i < 12; i++)
        {
            Console.Write($"{i}".PadRight(3));
        }
        Console.WriteLine();
        foreach (Scale scale in scalesOfInterest)
        {
            //Find all matches for chord per scale of interest
            List<int>[] chordKeyMultiplicity = scale.CalculateKeyMultiplicity(chord);
            //Print results
            Console.Write($"{scale.CalculateBase(),-2}: ");
            List<int> fundamentals = chordKeyMultiplicity.Aggregate((sum, next) => [.. sum, .. next]).Distinct().Order().ToList();
            for (int row = 0; row < fundamentals.Count(); row++)
            {
                if (row != 0)
                    Console.Write(" ".PadRight(4));
                for (int column = 0; column < 12; column++)
                {
                    //print scale root                    
                    if (chordKeyMultiplicity[column].Contains(fundamentals[row]))
                        Console.Write($"{fundamentals[row]}".PadRight(3));
                    else
                        Console.Write("   ");
                }
                Console.WriteLine();
            }
            if (fundamentals.Count() == 0) //extra line for pretty printing empty fundamentals
                Console.WriteLine();
            Console.WriteLine();
        }
    }
}


//Add an extra tone to the original set, e.g. the melody being played, display LCM for all fundamentals for all possible keys
static void QueryMelodicSupersetLCMs()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for melodic superset LCMs, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }
        long[] tet12Keys = Array.ConvertAll(input.Split(" "), long.Parse);

        List<List<long>> lcmsPerMelodyPerFundamental = new();

        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            lcmsPerMelodyPerFundamental.Add([]);
            for (int melodyKey = 0; melodyKey < 12; melodyKey++)
            {
                List<long> renormalizedKeys = tet12Keys.Select(key => (key - fundamental + 12) % 12).ToList();
                long renormalizedMelodyKey = (melodyKey - fundamental + 12) % 12;
                renormalizedKeys.Add(renormalizedMelodyKey);

                if (renormalizedKeys.Any(key => standardFractions[key] == 0))
                    lcmsPerMelodyPerFundamental[fundamental].Add(0); //0 for invalid key, no 7/5
                else
                {
                    lcmsPerMelodyPerFundamental[fundamental].Add(
                        LCM(renormalizedKeys.Select(
                            key => (long)standardFractions[key].Denominator).ToArray()));
                }
            }
        }

        //Print data
        Console.Write("".PadRight(4));
        for (int key = 0; key < 12; key++)
            Console.Write($"{key}".PadRight(4));

        Console.WriteLine();
        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            Console.Write($"{fundamental}:".PadRight(4));
            foreach (var lcm in lcmsPerMelodyPerFundamental[fundamental])
            {
                if (lcm != 0 && (24 % lcm == 0 || 15 % lcm == 0 || 20 % lcm == 0)) //only base 24, 20 or 15 lcm, else invalid
                    Console.Write($"{lcm}".PadRight(4));
                else
                    Console.Write($"".PadRight(4));
            }
            Console.WriteLine();
        }
    }
}

static void QueryMelodicSubsetLCMs()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for melodic subset LCMs, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }
        int[] tet12Keys = Array.ConvertAll(input.Split(" "), int.Parse);
        if (tet12Keys.Count() < 3)
        {
            Console.WriteLine("At least 3 keys for reduced LCMs");
            continue;
        }

        List<List<int>> reducedKeys = [tet12Keys.ToList()];
        for (int i = 0; i < tet12Keys.Length; i++)
        {
            List<int> reducedSet = tet12Keys.Where((_, index) => index != i).ToList();
            reducedKeys.Add(reducedSet);
        }

        List<List<long>> lcmPerFundamentalPerReducedSet = new();
        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            lcmPerFundamentalPerReducedSet.Add(new());
            foreach (var keys in reducedKeys)
            {
                List<List<int>> intervalPairs = GetPowerSet(keys.ToArray()).Where(set => set.Count == 2).ToList();
                List<List<int>> renormalizedPairs = intervalPairs.Select(
                    pairs => pairs.Select(key => (key - fundamental + 12) % 12).ToList())
                    .ToList();

                List<long> LcmPerPair = new();
                foreach (List<int> pair in renormalizedPairs)
                {
                    if (pair.Any(interval => interval == 6))
                        LcmPerPair.Add(0); //0 to indicate invalid interval, not using 7/5
                    else
                        LcmPerPair.Add(LCM(pair.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                }

                long totalLcm = 0;

                if (!LcmPerPair.Any(lcm => lcm == 0)) //only lcm from full match intervals
                    totalLcm = LCM(LcmPerPair.ToArray());

                lcmPerFundamentalPerReducedSet[fundamental].Add(totalLcm);
            }
        }

        //Print data
        Console.Write("".PadRight(4));
        Console.Write("-".PadRight(4));
        foreach (var key in tet12Keys) // printing removed key which produces the reduced set
            Console.Write($"{key}".PadRight(4));
        Console.WriteLine();
        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            Console.Write($"{fundamental}:".PadRight(4));
            foreach (var lcm in lcmPerFundamentalPerReducedSet[fundamental])
            {
                if (lcm != 0 && (24 % lcm == 0 || 15 % lcm == 0)) //only base 24 or 15 lcm, else invalid
                    Console.Write($"{lcm}".PadRight(4));
                else
                    Console.Write($"".PadRight(4));
            }
            Console.WriteLine();
        }
    }
}

//Show which base each key matches with for each interval of a chord e.g. 9 0 4 shows that key 1 matches with base 3 at 0 for interval 9 0
void QueryChordIntervalMultiplicity()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    //Fraction[] standardFractions = [new(1), new(16, 15), new(0), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(0), new(15, 8)];
    Dictionary<int, int[]> scalesPerBase = new();
    scalesPerBase[8] = [0, 2, 4, 7, 11];
    scalesPerBase[15] = [0, 1, 3, 5, 8, 9, 10];
    scalesPerBase[24] = [0, 2, 4, 5, 7, 9, 11];
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for chord interval multiplicity, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }

        string[] splitInput = input.Split(' ');
        bool printOneKeyOnly = false;
        bool printSideways = false;
        int melodyToShow = -1;
        if (splitInput.Last().Contains("m")) //end query with e.g. m=7 to see only output for key 7
        {
            printOneKeyOnly = true;
            melodyToShow = int.Parse(splitInput.Last().Split("=").Last());
        }
        if (splitInput.Last().Contains("s")) //end query with s to print columns side by side
        {
            printSideways = true;
        }

        string[] inputKeys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        int[] tet12Keys = Array.ConvertAll(inputKeys, int.Parse);

        List<List<int>> inputIntervals = GetPowerSet(tet12Keys).Where(set => set.Count == 2).Select(pair => new List<int>([pair.First(), pair.Last()])).ToList();

        //Calculate Data
        List<List<List<long>>> lcmPerMelodyPerFundamentalPerInterval = new();
        foreach (var interval in inputIntervals)
        {
            lcmPerMelodyPerFundamentalPerInterval.Add(new());
            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                lcmPerMelodyPerFundamentalPerInterval[^1].Add(new());
                for (int melody = 0; melody < 12; melody++)
                {
                    //calculate lcm for interval relative to current fundamental
                    long lcm = 0;
                    var renormalizedInterval = interval.Select(key => (key - fundamental + 12) % 12);
                    if (!renormalizedInterval.Any(key => standardFractions[key] == 0)) //no invalid interval
                        lcm = LCM(renormalizedInterval.Select(key => (long)standardFractions[key].Denominator).ToArray());
                    //check if melody + interval is in any scale                                        
                    if (lcm > 0)
                    {
                        bool isMatch = false;
                        List<int> keysToMatch = [melody, .. interval];
                        keysToMatch = keysToMatch.Distinct().ToList();
                        foreach (var @base in scalesPerBase.Keys)
                        {
                            var scale = scalesPerBase[@base];
                            var currentScale = scale.Select(key => (key + fundamental) % 12);
                            if (currentScale.Intersect(keysToMatch).Count() == keysToMatch.Count())
                            {
                                lcmPerMelodyPerFundamentalPerInterval[^1][^1].Add(@base);
                                isMatch = true;
                                break;
                            }

                        }
                        if (!isMatch)
                            lcmPerMelodyPerFundamentalPerInterval[^1][^1].Add(0); //no match                        
                    }
                    else
                        lcmPerMelodyPerFundamentalPerInterval[^1][^1].Add(0); //bad LCM
                }
            }
        }

        //Print Data
        if (printOneKeyOnly)
        {
            Console.Write(" ".PadRight(4));
            foreach (var interval in inputIntervals)
                foreach (var key in interval)
                    Console.Write($"{key}".PadRight(3));
            Console.WriteLine();
            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                Console.Write($"{$"{fundamental}".PadRight(2)}: ");
                for (int interval = 0; interval < lcmPerMelodyPerFundamentalPerInterval.Count; interval++)
                {
                    var lcm = lcmPerMelodyPerFundamentalPerInterval[interval][fundamental][melodyToShow];
                    if (lcm > 0)
                        Console.Write($"{lcmPerMelodyPerFundamentalPerInterval[interval][fundamental][melodyToShow]} ".PadRight(6));
                    else
                        Console.Write(" ".PadRight(6));
                }
                Console.WriteLine();
            }
        }
        else if (printSideways)
        {
            for (int interval = 0; interval < lcmPerMelodyPerFundamentalPerInterval.Count; interval++)
            {
                Console.Write(" ".PadRight(7));
                for (int key = 0; key < 12; key++)
                    Console.Write($"{key}".PadRight(3));
            }
            Console.WriteLine();
            for (int interval = 0; interval < lcmPerMelodyPerFundamentalPerInterval.Count; interval++)
            {
                Console.Write($"{string.Join(" ", inputIntervals[interval])}:".PadRight(12 * 3 + 7));
            }
            Console.WriteLine();
            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                for (int interval = 0; interval < lcmPerMelodyPerFundamentalPerInterval.Count; interval++)
                {
                    Console.Write($"   {$"{fundamental}".PadRight(2)}: ");
                    for (int melody = 0; melody < 12; melody++)
                    {
                        var lcm = lcmPerMelodyPerFundamentalPerInterval[interval][fundamental][melody];
                        if (lcm > 0)
                            Console.Write($"{lcmPerMelodyPerFundamentalPerInterval[interval][fundamental][melody]} ".PadRight(3));
                        else
                            Console.Write(" ".PadRight(3));
                    }
                }
                Console.WriteLine();
            }
        }
        else
        {
            Console.Write(" ".PadRight(7));
            for (int key = 0; key < 12; key++)
            {
                Console.Write($"{key}".PadRight(3));
            }
            Console.WriteLine();
            for (int interval = 0; interval < lcmPerMelodyPerFundamentalPerInterval.Count; interval++)
            {
                Console.WriteLine($"{string.Join(" ", inputIntervals[interval])}:");
                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    Console.Write($"   {$"{fundamental}".PadRight(2)}: ");
                    for (int melody = 0; melody < 12; melody++)
                    {
                        var lcm = lcmPerMelodyPerFundamentalPerInterval[interval][fundamental][melody];
                        if (lcm > 0)
                            Console.Write($"{lcmPerMelodyPerFundamentalPerInterval[interval][fundamental][melody]} ".PadRight(3));
                        else
                            Console.Write(" ".PadRight(3));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("---");
            }
        }
    }
}
//Show which real base each key matches to for each interval of a chord
void QueryRealChordIntervalMultiplicity()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    Dictionary<int, int[]> scalesPerBase = new();
    scalesPerBase[8] = [0, 2, 4, 7, 11];
    scalesPerBase[15] = [0, 1, 3, 5, 8, 9, 10];
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for real (non virtual) chord interval multiplicity, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }

        string[] splitInput = input.Split(' ');
        List<string> options = splitInput.Where(chars => !int.TryParse(chars, out _)).ToList();
        foreach (string option in options)
        {
            switch (option)
            {
                default:
                    break;
            };
        }

        string[] inputKeys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        int[] tet12Keys = Array.ConvertAll(inputKeys, int.Parse);

        //Calculate Data
        // - Calculate interval lcms
        List<(int, int)> inputIntervals = GetPowerSet(tet12Keys).Where(set => set.Count == 2).Select(pair => (pair.First(), pair.Last())).ToList();
        List<((int, int), (long, long))> intervalLcms = new();
        foreach (var interval in inputIntervals)
        {
            (int, int) renormalizedInterval1 = (0, (interval.Item2 - interval.Item1 + 12) % 12);
            (int, int) renormalizedInterval2 = ((interval.Item1 - interval.Item2 + 12) % 12, 0);
            //calculate LCM for valid intervals, else set lcm 0
            long lcm1 = 0;
            long lcm2 = 0;
            if (renormalizedInterval1.Item2 != 6)
                lcm1 = LCM([(long)standardFractions[renormalizedInterval1.Item1].Denominator, (long)standardFractions[renormalizedInterval1.Item2].Denominator]);
            if (renormalizedInterval1.Item1 != 6)
                lcm2 = LCM([(long)standardFractions[renormalizedInterval2.Item1].Denominator, (long)standardFractions[renormalizedInterval2.Item2].Denominator]);

            intervalLcms.Add(((interval.Item1, interval.Item2), (lcm1, lcm2)));
        }
        // - Calculate if key is in base corresponding to interval lcm
        List<List<List<long>>> lcmPerMelodyPerKeyPerInterval = new();
        foreach (var data in intervalLcms)
        {
            lcmPerMelodyPerKeyPerInterval.Add(new()); //add interval
            List<int> interval = [data.Item1.Item1, data.Item1.Item2];
            List<long> lcms = [data.Item2.Item1, data.Item2.Item2];
            for (int i = 0; i < interval.Count; i++)
            {
                int key = interval[i];
                lcmPerMelodyPerKeyPerInterval[^1].Add(new());
                for (int melody = 0; melody < 12; melody++)
                {
                    //check if melody is in lcm base at key as fundamental
                    int @base = 0;
                    if (8 % lcms[i] == 0) //use base 8
                        @base = 8;
                    else if (15 % lcms[i] == 0) //use base 15
                        @base = 15;
                    if (@base > 0)
                    {
                        var renormalizedScale = scalesPerBase[@base].Select(scaleKey => (scaleKey + key) % 12);
                        if (renormalizedScale.Contains(melody))
                            lcmPerMelodyPerKeyPerInterval[^1][^1].Add(lcms[i]);
                        else
                            lcmPerMelodyPerKeyPerInterval[^1][^1].Add(0);
                    }
                    else
                        lcmPerMelodyPerKeyPerInterval[^1][^1].Add(0);
                }
            }
        }

        //Print Data
        Console.Write(" ".PadRight(4));
        for (int key = 0; key < 12; key++)
        {
            Console.Write($"{key}".PadRight(3));
        }
        Console.WriteLine();
        for (int j = 0; j < intervalLcms.Count; j++)
        {
            var data = intervalLcms[j];
            List<int> interval = [data.Item1.Item1, data.Item1.Item2];
            List<long> lcms = [data.Item2.Item1, data.Item2.Item2];
            for (int i = 0; i < interval.Count; i++)
            {
                var intervalKey = interval[i];
                Console.Write($"{$"{intervalKey}".PadRight(2)}: ");
                for (int melody = 0; melody < 12; melody++)
                {
                    var output = lcmPerMelodyPerKeyPerInterval[j][i][melody];
                    if (output == 0)
                        Console.Write($" ".PadRight(3));
                    else
                        Console.Write($"{output}".PadRight(3));
                }
                Console.WriteLine();
            }
            Console.WriteLine("---");
        }
    }
}

//Query the lcms of a chords power set. Useful for finding lcms of subsets.
static void QueryChordPowerSetLCMs()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    //Fraction[] standardFractions = [new(1), new(16, 15), new(0), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(0), new(15, 8)];
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for power set LCMs, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }

        string[] splitInput = input.Split(' ');
        List<string> options = splitInput.Where(chars => !int.TryParse(chars, out _)).ToList();
        bool realBaseOnly = false;
        bool virtualBaseOnly = false;
        bool noCollapse = false;
        bool useMaxLCM = false;
        bool printCardinalComplement = false;
        int maxLCM = 12; //pure base 15 sounds bad, so does 20, 24 sounds okish (11 and 5 sounds bad), size 12 might be largest lcm and rest is superpositions
        foreach (string option in options)
        {
            switch (option)
            {
                case "c":
                    printCardinalComplement = true;
                    break;
                case "m":
                    useMaxLCM = true;
                    break;
                case "r":
                    realBaseOnly = true;
                    break;
                case "v":
                    virtualBaseOnly = true;
                    break;
                case "n": //noBase15Collapse (base 15 on fundamental excludes fundamental - 4)
                    noCollapse = true;
                    break;
                default:
                    break;
            };
        }

        string[] keys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        int[] tet12Keys = Array.ConvertAll(keys, int.Parse);

        //Calculate Data
        Dictionary<int, List<List<int>>> cardinalSets = GetPowerSet(tet12Keys).GroupBy(set => set.Count).ToDictionary(
            group => group.Key,
            group => group.ToList());

        Dictionary<int, Dictionary<int, List<long>>> lcmPerSubsetPerFundamentalPerCardinality = new();
        foreach (int cardinality in cardinalSets.Keys)
        {
            if (cardinality < 2) continue; // not interested in single notes
            List<List<int>> cardinalSet = cardinalSets[cardinality];
            Dictionary<int, List<long>> lcmPerSubsetPerFundamental = new();

            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                List<List<int>> renormalizedSets = cardinalSet.Select(
                    set => set.Select(key => (key - fundamental + 12) % 12).ToList())
                    .ToList();

                List<long> LcmPerSet = new();
                for (int i = 0; i < renormalizedSets.Count; i++)
                {
                    List<int> set = renormalizedSets[i];
                    if (set.Any(interval => standardFractions[interval] == 0))
                        LcmPerSet.Add(0); //0 to indicate invalid interval
                    else if (realBaseOnly && !cardinalSet[i].Contains(fundamental))
                        LcmPerSet.Add(0); //0 to indicate non real base                    
                    else if (virtualBaseOnly && cardinalSet[i].Contains(fundamental))
                        LcmPerSet.Add(0); //0 to indicate real base                    
                    else
                    {
                        long lcm = LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray());
                        if (useMaxLCM && lcm > maxLCM)
                            LcmPerSet.Add(0);
                        else if (8 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (10 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (24 % lcm == 0)
                            LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        else if (15 % lcm == 0)// use base 15
                        {
                            if (noCollapse && cardinalSet[i].Contains((fundamental + 8) % 12))
                                LcmPerSet.Add(0); //0 to indicate collapsing base 15 - note that collapse might not exist in this way, related to 0 4 7 1 sounding bad unless used as 15@7
                            else
                                LcmPerSet.Add(LCM(set.Select(key => (long)standardFractions[key].Denominator).ToArray()));
                        }
                        else
                            LcmPerSet.Add(0);
                    }
                }

                lcmPerSubsetPerFundamental[fundamental] = LcmPerSet;
            }
            lcmPerSubsetPerFundamentalPerCardinality[cardinality] = lcmPerSubsetPerFundamental; //check cardinalSets[cardinality] for related sets
        }

        //Print Data
        int consoleMargin = 20; //arbitrary console margin
        foreach (var cardinality in lcmPerSubsetPerFundamentalPerCardinality.Keys)
        {
            int currentCardinalSetIndex = 0;
            int currentPageCardinalSetIndexStart = 0;

            while (currentCardinalSetIndex < cardinalSets[cardinality].Count) //each loop is a page of output
            {
                int currentWidth = 0;
                currentPageCardinalSetIndexStart = currentCardinalSetIndex;

                Console.WriteLine($"{nameof(cardinality)}:{cardinality}");
                Dictionary<int, List<long>> lcmPerSubsetPerFundamental = lcmPerSubsetPerFundamentalPerCardinality[cardinality];
                Console.Write($" ".PadRight(4)); //chars to write e.g. "10:"            
                for (int i = currentCardinalSetIndex; i < cardinalSets[cardinality].Count; i++)
                {
                    List<int> currentSet = cardinalSets[cardinality][i];
                    List<int> setToWrite = currentSet;
                    if (printCardinalComplement)
                        setToWrite = cardinalSets[lcmPerSubsetPerFundamentalPerCardinality.Keys.Max()].First().Except(currentSet).ToList();

                    if (setToWrite.Count * 3 > Console.WindowWidth - consoleMargin) //check if set will overflow console width  
                    {
                        Console.WriteLine("Console window width too small to print cardinal set, trying next cardinality");
                        currentCardinalSetIndex = cardinalSets[cardinality].Count; //breaks the enclosing while loop
                        break;
                    }
                    if (currentWidth + setToWrite.Count * 3 > Console.WindowWidth - consoleMargin) //check if the set to write will overflow console width, try new page
                        break;

                    foreach (var key in setToWrite)
                        Console.Write($"{key,-2} ");
                    currentWidth += setToWrite.Count * 3;
                    currentCardinalSetIndex++;
                }
                Console.WriteLine();
                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    Console.Write($"{fundamental,-2}: ");
                    int lcmPadding = cardinality * 3; //max lcm is double digit + 1 space
                    if (printCardinalComplement)
                        lcmPadding = Math.Max(lcmPerSubsetPerFundamentalPerCardinality.Keys.Max() - cardinality, 1) * 3;

                    for (int i = currentPageCardinalSetIndexStart; i < currentCardinalSetIndex; i++) //needs to be per subset currently being written
                    {
                        long lcm = lcmPerSubsetPerFundamental[fundamental][i];
                        if (lcm == 0)
                            Console.Write($" ".PadRight(lcmPadding));
                        else
                            Console.Write($"{lcm}".PadRight(lcmPadding));
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}


//Query LCMs of all subsets of size 2
static void QuerySubsetIntervalsLCMs()
{
    Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for subset LCMs, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }
        int[] tet12Keys = Array.ConvertAll(input.Split(" "), int.Parse);

        List<List<int>> inputPairs = GetPowerSet(tet12Keys).Where(set => set.Count == 2).ToList();
        Dictionary<int, List<long>> LcmPairsPerFundamental = new();
        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            List<List<int>> renormalizedPairs = inputPairs.Select(
                pairs => pairs.Select(key => (key - fundamental + 12) % 12).ToList())
                .ToList();

            List<long> LcmPerPair = new();
            foreach (List<int> pair in renormalizedPairs)
            {
                if (pair.Any(interval => interval == 6))
                    LcmPerPair.Add(0); //0 to indicate invalid interval, not using 7/5
                else
                    LcmPerPair.Add(LCM(pair.Select(key => (long)standardFractions[key].Denominator).ToArray()));
            }

            LcmPairsPerFundamental[fundamental] = LcmPerPair;
        }
        Console.Write($" ".PadRight(4));
        foreach (var pair in inputPairs)
        {
            foreach (var key in pair)
                Console.Write($"{key,-2} ");
            Console.Write("  ");
        }
        Console.WriteLine("all");
        for (int fundamental = 0; fundamental < 12; fundamental++)
        {
            Console.Write($"{fundamental,-2}: ");
            foreach (var lcm in LcmPairsPerFundamental[fundamental])
            {
                if (lcm == 0)
                    Console.Write($" ".PadRight(8));
                else
                    Console.Write($"{lcm,-7} ");
            }
            long totalLcm = LCM(LcmPairsPerFundamental[fundamental].Where(lcm => lcm != 0).ToArray());
            Console.Write($"{totalLcm,-3} ");
            if (!LcmPairsPerFundamental[fundamental].Any(lcm => lcm == 0))
                Console.Write("!");
            Console.WriteLine();
        }
    }
}



static void QueryRatioFundamentalOctaveSweep(double maxDeviation = 0.010d)
{
    while (true)
    {
        bool fullMatchOnly = false;
        bool tet12Only = true;
        bool repeatFractions = false;
        Console.WriteLine($"Input space separated tet12 keys for octave sweep, empty input to exit");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        if (input == "clear")
        {
            Console.Clear();
            continue;
        }
        string[] splitInput = input.Split(' ');
        List<string> options = splitInput.Where(chars => !int.TryParse(chars, out _)).ToList();
        foreach (string option in options)
        {
            switch (option)
            {
                case "t": //allow non tet 12 keys
                    tet12Only = false;
                    break;
                case "f": //fullMatchOnly
                    fullMatchOnly = true;
                    break;
                case "r": //repeatFractions
                    repeatFractions = true;
                    break;
                default:
                    break;
            };
        }

        string[] keys = splitInput.Where(chars => int.TryParse(chars, out _)).ToArray();
        int[] tet12Keys = Array.ConvertAll(keys, int.Parse);

        PrintRatioFundamentalOctaveSweep(ConstructTet12DoubleArray(tet12Keys),
                                         maxDeviation: maxDeviation,
                                         fullMatchOnly: fullMatchOnly,
                                         repeatFractions: repeatFractions,
                                         tet12Only: tet12Only);
    }
}
//Default max set to catch certain scenarios
static void PrintRatioFundamentalOctaveSweep(double[] originalRatios,
                                             double stepSize = 0.01,
                                             double maxDeviation = 0.01d,
                                             bool fullMatchOnly = false,
                                             bool repeatFractions = true,
                                             bool tet12Only = true
                                             )
{
    //No 7/5? approximates sqrt 2, might be important even though big prime in numerator
    Fraction[] goodFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
    //Fraction[] goodFractions = [new(1), new(8, 7), new(9, 7), new(10, 7), new(11, 7), new(12, 7), new(13, 7)];
    double[] goodRatios = goodFractions.Select(fraction => fraction.ToDouble()).ToArray();
    double fundamental = 1;
    List<(double fundamental, double[] renormalizedRatios, Fraction[] goodFractionsFound)> dataPerStep = new();
    //Compute renormalized ratios and bin to good ratios during octave sweep
    while (fundamental < 2)
    {
        double[] renormalizedRatios = originalRatios.Select(ratio => (ratio / fundamental).ToOctave()).ToArray();
        Fraction[] goodFractionsFound = new Fraction[originalRatios.Length];

        //Match renormalized ratios to good fractions
        for (int i = 0; i < renormalizedRatios.Count(); i++)
        {
            double renormalizedRatio = renormalizedRatios[i];
            foreach (var goodFraction in goodFractions)
            {
                //proximity modulo 2 - check from both left and right of number line, go for smallest distance
                if (Math.MinMagnitude(Math.Abs((double)goodFraction - renormalizedRatio), Math.Abs(((double)goodFraction - 1) + (2 - renormalizedRatio))) < maxDeviation)
                    goodFractionsFound[i] = goodFraction;
            }
        }

        dataPerStep.Add((fundamental, renormalizedRatios, goodFractionsFound));

        fundamental += stepSize;
    }
    //Print data     

    Fraction[] previousGoodFractions = []; //used with repeatFractions


    var tet12Keys = CalculateTet12Values().ToList();
    tet12Keys.Add(2);
    foreach (var dataRow in dataPerStep)
    {
        Fraction[] goodFractionsFound = dataRow.goodFractionsFound;
        long rowLcm = 0;

        if (tet12Only)
        {
            bool match12Tet = false;
            foreach (var tet12Key in tet12Keys)
            {
                if (Math.MinMagnitude(
                    Math.Abs(dataRow.fundamental - tet12Key),
                    Math.Abs(dataRow.fundamental - 1) + (2 - tet12Key)) < 0.01)
                {
                    match12Tet = true;
                    break;
                }
            }
            if (!match12Tet)
                continue;
        }

        //abbreviated output
        if (goodFractionsFound.Any(goodFraction => goodFraction > 0))
        {
            rowLcm = LCM(dataRow.goodFractionsFound
                .Where(goodFraction => goodFraction > 0)
                .Select(fraction => (long)fraction.Denominator)
                .ToArray());
        }
        if (!repeatFractions)
        {
            if (Enumerable.SequenceEqual(goodFractionsFound, previousGoodFractions))
            {
                previousGoodFractions = goodFractionsFound;
                continue;
            }
            previousGoodFractions = goodFractionsFound;
        }

        //full match only output
        bool isRowFullMatch = goodFractionsFound.Where(goodFraction => goodFraction > 0).Count() == originalRatios.Length;
        if (fullMatchOnly && !isRowFullMatch)
            continue;

        //skip empty rows
        if (rowLcm == 0)
            continue;


        Console.Write($"{dataRow.fundamental:0.00}:");

        //Print renormalized ratios
        for (int i = 0; i < dataRow.renormalizedRatios.Count(); i++)
        {
            double renormalizedRatio = dataRow.renormalizedRatios[i];
            Console.Write($" {renormalizedRatio:0.00}");
        }

        //Pretty print good fractions
        if (rowLcm != 0)
        {
            Console.Write(" <--");
            for (int i = 0; i < dataRow.goodFractionsFound.Length; i++)
            {
                Fraction goodFraction = dataRow.goodFractionsFound[i];
                if (goodFraction > 0)
                    Console.Write($" {goodFraction}".PadRight(6));
                else
                    Console.Write("".PadRight(6));
            }
            if (isRowFullMatch)
                Console.Write(" !");
            else
                Console.Write("  ");
            Console.Write($" LCM:{rowLcm}");
        }

        Console.WriteLine();
    }
}

static void PrintClosestFractionsBetweenScales(Scale scaleToRotate, Scale scaleForReference)
{
    Scale currentScale = scaleToRotate;
    List<Fraction> currentScaleFractions = currentScale.ToFractions();
    foreach (Fraction fundamentalFraction in currentScaleFractions)
    {
        List<Fraction> renormalizedFractions = [.. currentScaleFractions.Select(fraction => (fraction / fundamentalFraction).ToOctave())];
        foreach (Fraction fraction in renormalizedFractions)
        {
            var bestFitFraction = scaleForReference.CalculateClosestFraction(fraction.ToDouble());

            Console.WriteLine(
                $"{bestFitFraction,-5} "
                + $"{Math.Abs(RelativeDeviation(bestFitFraction.ToDouble(), fraction.ToDouble())):0.00} ".PadRight(5)
                + $"({fraction})".PadRight(5));
        }
        Console.WriteLine();
    }
}

static void PrintClosestDoublesBetweenScales(double[] scaleToRotate, Scale scaleForReference)
{
    foreach (double fundamentalDouble in scaleToRotate)
    {
        List<double> renormalizedDoubles = [.. scaleToRotate.Select(scaleNote => (scaleNote / fundamentalDouble).ToOctave())];
        foreach (double renormalizedDouble in renormalizedDoubles)
        {
            var bestFitFraction = scaleForReference.CalculateClosestFraction(renormalizedDouble);

            Console.WriteLine(
                $"{bestFitFraction,-5} "
                + $"{Math.Abs(RelativeDeviation(bestFitFraction.ToDouble(), renormalizedDouble)):0.00} ".PadRight(5)
                + $"({renormalizedDouble:0.00})".PadRight(5));
        }
        Console.WriteLine();
    }
}

static void PrintSlidingFundamentalMatchingBetweenScales(double[] scaleToSlide, Scale scaleForReference, bool printMaxMatchOnly = false, double keyFitCriteria = 0.02d)
{
    double fundamental = 1;
    double stepSize = 0.01; //12 tet key diff is about 0.06    
    while (fundamental > 0.5)
    {
        List<double> renormalizedDoubles = [.. scaleToSlide.Select(scaleNote => (scaleNote * fundamental).ToOctave())];
        double errorSum = 0;
        int goodKeyFits = 0;
        List<string> output = new();
        foreach (double renormalizedDouble in renormalizedDoubles)
        {
            var bestFitFraction = scaleForReference.CalculateClosestFraction(renormalizedDouble);
            double relativeDeviation = Math.Abs(RelativeDeviation(bestFitFraction.ToDouble(), renormalizedDouble));
            errorSum += relativeDeviation;
            if (relativeDeviation < keyFitCriteria)
                goodKeyFits++;

            output.Add(
                $"{bestFitFraction,-5} "
                + $"{relativeDeviation:0.00} ".PadRight(5)
                + $"({renormalizedDouble:0.00})".PadRight(5));
        }
        if (printMaxMatchOnly)
        {
            if (goodKeyFits == scaleToSlide.Length)
            {
                foreach (var line in output)
                    Console.WriteLine(line);
                Console.WriteLine($"{fundamental} ({fundamental.ToOctave():0.00}) - {errorSum} : {goodKeyFits}");
            }
        }
        else
        {
            foreach (var line in output)
                Console.WriteLine(line);
            Console.WriteLine($"{fundamental} ({fundamental.ToOctave():0.00}) - {errorSum} : {goodKeyFits}");
        }

        fundamental -= stepSize;
    }
}

//Ambiguity is the number of fundamental note placements in the scaleclass producing identical scales - e.g. 0 3 6 9 has ambiguity 4, while 0 4 7 has ambiguity 1
void PrintScaleClassAmbiguity(ScaleCalculator scaleCalculator, bool printBase = true)
{
    Dictionary<int, List<HashSet<Scale>>> scaleClassesByAmbiguity = new();
    foreach (var scaleClass in scaleCalculator.ScaleClasses)
    {
        HashSet<Scale> uniqueScales = [.. scaleClass];
        int ambiguity = 1 + scaleClass.First().NumberOfKeys() - uniqueScales.Count;

        if (!scaleClassesByAmbiguity.ContainsKey(ambiguity))
            scaleClassesByAmbiguity[ambiguity] = new();
        scaleClassesByAmbiguity[ambiguity].Add(uniqueScales);
    }
    foreach (int ambiguity in scaleClassesByAmbiguity.Keys.OrderByDescending(key => key))
    {
        Console.WriteLine($"Ambiguity {ambiguity} ({scaleClassesByAmbiguity[ambiguity].Count}): ");
        foreach (var uniqueScales in scaleClassesByAmbiguity[ambiguity].OrderByDescending(scaleClass => scaleClass.First().NumberOfKeys()))
        {
            Console.WriteLine();
            foreach (var uniqueScale in uniqueScales)
            {
                if (printBase)
                    Console.Write($"{uniqueScale.CalculateBase(),-4} ");
                Console.WriteLine(uniqueScale);
            }
        }
    }
}
//Uniqueness is the number of distinct scales in a scale class, e.g. 0 3 6 9 has uniqueness 1 despite having 4 rotations.
void PrintScaleClassUniqueness(ScaleCalculator scaleCalculator, bool printBase = true)
{
    Dictionary<int, List<HashSet<Scale>>> scaleClassByUniqueness = new();
    foreach (var scaleClass in scaleCalculator.ScaleClasses)
    {
        HashSet<Scale> uniqueScales = [.. scaleClass];

        if (!scaleClassByUniqueness.ContainsKey(uniqueScales.Count))
            scaleClassByUniqueness[uniqueScales.Count] = new();
        scaleClassByUniqueness[uniqueScales.Count].Add(uniqueScales);
    }
    foreach (int uniqueness in scaleClassByUniqueness.Keys.OrderByDescending(key => key))
    {
        Console.WriteLine($"Uniqueness {uniqueness}:");
        foreach (var uniqueScales in scaleClassByUniqueness[uniqueness].OrderByDescending(scaleClass => scaleClass.First().NumberOfKeys()))
        {
            Console.WriteLine();
            foreach (var uniqueScale in uniqueScales)
            {
                if (printBase)
                    Console.Write($"{uniqueScale.CalculateBase(),-4} ");
                Console.WriteLine(uniqueScale);
            }
        }
    }
}

void PrintAllSymmetricScaleClasses(ScaleCalculator scaleCalculator)
{
    foreach (var scaleClass in scaleCalculator.ScaleClasses)
    {
        Scale firstScale = scaleClass.First();
        bool isSymmetric = true;
        foreach (var scale in scaleClass)
        {
            if (firstScale != scale)
            {
                isSymmetric = false;
                break;
            }
        }
        if (isSymmetric)
        {
            Console.WriteLine(firstScale);
        }
    }
}

//Check if a chord is a subset of any key set translation in the octave
void QueryChordInKeySetTranslations()
{
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for chord");
        string input = Console.ReadLine();

        Scale chord = new(Array.ConvertAll(input.Split(' '), int.Parse));

        Console.WriteLine($"Input space separated tet12 keys for key set to find chord in. (empty input to exit)");
        input = Console.ReadLine();

        if (input.Length == 0) return;
        Tet12KeySet inputKeys = new(Array.ConvertAll(input.Split(' '), int.Parse));

        for (int i = 0; i < 12; i++)
        {
            Tet12KeySet translatedKeySet = inputKeys >> i;
            if ((translatedKeySet.BinaryRepresentation & chord.KeySet.BinaryRepresentation) == chord.KeySet.BinaryRepresentation)
                Console.WriteLine($"{i,-2}: {translatedKeySet.ToIntervalString()}");
        }
    }
}

void QueryChordKeyMultiplicityPowerSets(ScaleCalculator scaleCalculator, int minSubsetLength = 2)
{
    List<Scale> scalesOfInterest = [
        //new([0, 1, 3, 5, 6, 8, 9, 10]), //full base 15
        //new([0, 1, 3, 5, 6, 9, 10]), //base 15 - seems like the full base 15 collapses to base 24 due to the 8/5 creating base24 at C or G for base 15 at B
        new([0, 1, 3, 5, 9, 10]), //natural base 15 - no 8 as it collapses to 24 on 1, no 6 as 7 is bad numerator in 7/5
                                  //new([0, 3, 4, 6, 7, 8, 10]), //full base 20
                                  //new([0, 2, 4, 6, 8, 9]), //full base 7
                                  //new([0, 2, 4, 7, 11]),  //base 8
        new([0, 2, 4, 5, 7, 9, 11])  //base 24 - 1, 9/8, 5/4, 5/4, 3/2, 5/3, 15/8
        ];

    Console.WriteLine("Matching input against scales:");
    foreach (Scale scale in scalesOfInterest)
    {
        Console.WriteLine($"{scale.CalculateBase(),-2}: {scale}");
    }

    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for chord to calculate chord key multiplicity power sets - empty input to exit");
        string chordInput = Console.ReadLine();

        if (chordInput.Length == 0)
            return;

        bool isTargetChordQueried = false;
        Tet12KeySet targetChord = new();
        int[] keys;
        int localMinSubsetLength = minSubsetLength;
        if (chordInput.Contains(":"))
        {
            isTargetChordQueried = true;

            string[] splitInput = chordInput.Split(":").Select(split => split.Trim(' ')).ToArray();
            int[] targetChordKeys = Array.ConvertAll(splitInput[1].Split(' '), int.Parse);
            targetChord = new(targetChordKeys);
            keys = [.. Array.ConvertAll(splitInput[0].Split(' '), int.Parse), .. targetChordKeys];
            //localMinSubsetLength += targetChordKeys.Count();
        }
        else
            keys = Array.ConvertAll(chordInput.Split(' '), int.Parse);

        List<List<int>> powerSetOfChords = GetPowerSet(keys)
            .Where(set => set.Count >= localMinSubsetLength)
            .OrderBy(set => set.Count).ToList();

        //Find all matches for chord per scale of interest
        int setIndex = 0;
        while (setIndex < powerSetOfChords.Count)
        {
            //Calculate data
            Tet12KeySet chord = new(powerSetOfChords[setIndex].ToArray());
            if (isTargetChordQueried && !targetChord.IsSubsetTo(chord))
            {
                setIndex++;
                continue;
            }

            List<List<int>[]> chordKeyMultiplicities = new();
            foreach (Scale scale in scalesOfInterest)
                chordKeyMultiplicities.Add(scale.CalculateKeyMultiplicity(chord));

            //Print data             
            if (chordKeyMultiplicities.Any(multiplicity => multiplicity.Any(fundamentals => fundamentals.Count > 0)))
            {
                Console.WriteLine(chord.ToIntervalString());
                for (int i = 0; i < scalesOfInterest.Count; i++)
                {
                    int maxFundamentals = chordKeyMultiplicities[i].Max(fundamentals => fundamentals.Count); //get all possible fundamentals - max includes all possibilities
                    Console.WriteLine($"  {string.Join(" ", chordKeyMultiplicities[i].First(fundamentals => fundamentals.Count == maxFundamentals))}");
                }
            }
            setIndex++;
        }
    }
}

void QueryChordProgressionFromMultiplicity(ScaleCalculator scaleCalculator)
{
    List<Scale> scalesOfInterest = [
        new([0, 1, 3, 5, 6, 8, 9, 10]), //base 15
        new([0, 2, 4, 5, 7, 9, 11])  //base 24
        ];

    Console.WriteLine("Matching input against scales:");
    foreach (Scale scale in scalesOfInterest)
    {
        Console.WriteLine($"{scale.CalculateBase(),-2}: {scale}");
    }

    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for current chord. Empty input to exit.");
        string chordInput = Console.ReadLine();

        if (chordInput.Length == 0)
            return;
        //TODO remake progressions to work from previosu chords
        Tet12KeySet currentChord = new(Array.ConvertAll(chordInput.Split(' '), int.Parse));

        Console.WriteLine($"Input space separated tet12 keys for previous chord.");
        chordInput = Console.ReadLine();

        Scale previousChord = new(Array.ConvertAll(chordInput.Split(' '), int.Parse));
        foreach (Scale scale in scalesOfInterest)
        {
            //calculate multiplicity to get all scale fundamental shifts
            var multiplicity = scale.CalculateKeyMultiplicity(currentChord);
            //Combine with all occurences of the progression chord in the scale for all progressions root positions
            List<int> chordPositionsInScale = new();
            for (int i = 0; i < 12; i++)
            {
                //chord in scale rotation?
                if (((scale >> i).BinaryRepresentation & previousChord.KeySet.BinaryRepresentation) == previousChord.KeySet.BinaryRepresentation)
                {
                    chordPositionsInScale.Add(i);
                }
            }
            HashSet<int> chordPositions = new();
            foreach (int fundamentalShift in multiplicity[0])
            {
                foreach (int chordPositionInScale in chordPositionsInScale)
                {
                    chordPositions.Add((fundamentalShift + chordPositionInScale) % 12);
                }
            }
            Console.WriteLine($"{string.Join(" ", chordPositions.OrderBy(position => position)),-25} ({string.Join(" ", chordPositionsInScale.OrderBy(position => position))})");
        }
    }
}

void QueryChordsInScale(ScaleCalculator scaleCalculator)
{
    List<Scale> scalesOfInterest = [new([0, 1, 3, 5, 6, 8, 9]), new([0, 2, 4, 5, 7, 9, 11])];
    Console.WriteLine("Matching input against scales:");
    foreach (Scale scale in scalesOfInterest)
    {
        Console.WriteLine($"{scale.CalculateBase(),-2}: {scale}");
    }

    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for chord progression source. (empty input to exit)");
        string sourceChordInput = Console.ReadLine();

        if (sourceChordInput.Length == 0)
            return;

        Scale sourceChord = new(Array.ConvertAll(sourceChordInput.Split(' '), int.Parse));

        Console.WriteLine($"Input space separated tet12 keys for chord progression target. (empty input to exit)");
        string targetChordInput = Console.ReadLine();

        if (targetChordInput.Length == 0)
            return;

        Scale targetChord = new(Array.ConvertAll(targetChordInput.Split(' '), int.Parse));

        foreach (Scale scale in scalesOfInterest)
        {
            for (int i = 0; i < 12; i++)
            {
                //Check if rotated scale is still a scale
                Tet12KeySet rotatedKeys = scale >> i;
                if ((rotatedKeys.BinaryRepresentation & 1) == 1)
                {
                    Scale rotatedScale = new(rotatedKeys);
                    if (rotatedScale.Contains(sourceChord))
                    {
                        Console.Write($"{i} (");
                        StringBuilder sb = new();
                        //look for targets
                        for (int j = 0; j < 12; j++)
                        {
                            Tet12KeySet rotatedKeys2 = rotatedScale >> j;
                            if ((rotatedKeys2.BinaryRepresentation & 1) == 1)
                            {
                                Scale rotatedScale2 = new(rotatedKeys2);
                                if (rotatedScale2.Contains(targetChord))
                                {
                                    sb.Append($"{j} ");
                                }
                            }
                        }
                        sb.Length--;
                        Console.Write(sb.ToString() + ") ");
                    }
                }
            }
            Console.WriteLine();
        }
    }
}
void QueryScaleClassProgressionsFromScale(ScaleCalculator scaleCalculator)
{
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for scale class progressions. (empty input to exit)");
        string input = Console.ReadLine();

        if (input.Length == 0)
            return;

        Scale inputScale = new(Array.ConvertAll(input.Split(' '), int.Parse));
        int formatLength = inputScale.NumberOfKeys() * 3;
        //Get scale class of input scale with fundamental shifts
        IEnumerable<(int shift, Scale scale)> scaleClass =
            inputScale.KeySet.CalculateFundamentalClass().Where(shiftAndScale => shiftAndScale.scale.NumberOfKeys() == inputScale.NumberOfKeys());
        foreach (var outerScale in scaleClass)
        {
            Console.Write($"{outerScale.shift,-2}{$"({outerScale.scale.CalculateBase()})",-4}: {outerScale.scale.ToString().PadRight(formatLength)} - ");
            foreach (var innerScale in scaleClass)
            {
                if (outerScale == innerScale)
                    continue;
                //inner scale progression from outer scale expressed as input scale
                int scaleShift = (12 + (outerScale.shift - innerScale.shift)) % 12;
                Console.Write($"{scaleShift,-2}{$"({innerScale.scale.CalculateBase()})",-4}: {innerScale.scale.ToString().PadRight(formatLength)} / ");
            }
            Console.WriteLine();
        }
    }
}

void QueryFundamentalClassPerScale(ScaleCalculator scaleCalculator)
{
    //Calculate all fundamental classes for all scales
    Dictionary<Scale, List<(int shift, Scale scale)>> FundamentalClassForScale = new();
    foreach (List<Scale> scaleClass in scaleCalculator.ScaleClasses)
    {
        foreach (Scale scale in scaleClass)
        {
            FundamentalClassForScale[scale] = scale.KeySet.CalculateFundamentalClass().
                OrderByDescending(item => item.scale.CalculateBase()).ThenByDescending(item => item.scale.ToString()).ToList();
        }
    }
    //Read input
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for fundamental class. (empty input to exit)");
        string input = Console.ReadLine();

        if (input.Length == 0)
            return;
        //Reverse lookup - print all scale classes containing input keys
        else if (input.Split(' ').First() == "r")
        {
            Scale inputKeys = new(Array.ConvertAll(input.Split(' ')[1..], int.Parse));
            foreach (List<(int shift, Scale scale)> scaleClass in FundamentalClassForScale.Values)
            {
                if (scaleClass.Select(item => item.scale).Contains(inputKeys))
                {
                    Console.WriteLine($"Fundamental class containing {inputKeys} found");
                    foreach (var item in scaleClass)
                    {
                        Console.WriteLine($"{item.scale.CalculateBase(),-3} - {(item.shift.ToString() + ":"),-3} {item.scale}");
                    }
                }
            }
        }
        //ordinary lookup, print the fundamental class for input keys
        else
        {
            Scale inputKeys = new(Array.ConvertAll(input.Split(' '), int.Parse));
            //sort fundamental class by scale classes
            Dictionary<HashSet<Scale>, HashSet<(int shift, Scale scale)>> scalesByScaleClass = new(HashSet<Scale>.CreateSetComparer());
            foreach ((int shift, Scale scale) item in FundamentalClassForScale[inputKeys])
            {
                var scaleClass = item.scale.CalculateScaleClass();
                if (!scalesByScaleClass.ContainsKey(scaleClass))
                    scalesByScaleClass[scaleClass] = new();
                scalesByScaleClass[scaleClass].Add(item);
            }
            //print scales grouped by scale classes            
            foreach (HashSet<(int shift, Scale scale)>? scaleClass in
                scalesByScaleClass.Values.OrderByDescending(item => item.First().scale.NumberOfKeys()))
            {
                foreach ((int shift, Scale scale) item in
                    scaleClass.OrderByDescending(item => item.scale.CalculateBase()).ThenByDescending(item => item.scale.ToString()).ToList())
                {
                    Console.WriteLine($"" +
                        $"{item.scale.CalculateBase(),-3} - " +
                        $"{item.shift.ToString() + ":",-3} " +
                        $"{item.scale.ToString().PadRight(3 * item.scale.NumberOfKeys())} - " +
                        $"{item.scale.Transpose().CalculateBase(),-3} " +
                        $"{item.scale.Transpose().ToString().PadRight(3 * item.scale.NumberOfKeys())} " +
                        $"({$"{new Tet12KeySet(item.scale.Transpose().ToIntervals().Select(interval => (interval + item.shift) % 12).ToArray())
                        .ToIntervalString()})"
                        .PadRight(3 * item.scale.NumberOfKeys())}");
                }
                Console.WriteLine();
            }
        }
    }
}

void QueryLEQSuperclass(Dictionary<Scale, List<Scale>> leqScalesPerScale)
{
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for all LEQ occurences. (empty input to exit)");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        Tet12KeySet inputKeys = new(Array.ConvertAll(input.Split(' '), int.Parse));

        foreach (Scale scale in leqScalesPerScale.Keys.OrderByDescending(key => key.NumberOfKeys()).ThenByDescending(key => key.ToString()))
        {
            if (scale.Contains(inputKeys) || leqScalesPerScale[scale].Any(leqScale => leqScale.Contains(inputKeys)))
            {
                Console.WriteLine($"{scale.CalculateBase(),-2} - {scale}:");
                foreach (var leqScale in leqScalesPerScale[scale].OrderByDescending(leqScale => leqScale.CalculateBase()).ThenByDescending(leqScale => leqScale.NumberOfKeys()))
                {
                    //print which keys in the leq scale the original scale matches to, and the fundamental note shift
                    List<int> scaleIntervalsInLeqScale = new();
                    int fundamentalShift = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        if (((leqScale.KeySet.BinaryRepresentation >> i) & scale.KeySet.BinaryRepresentation) == scale.KeySet.BinaryRepresentation)
                        {
                            scaleIntervalsInLeqScale = scale.ToIntervals().Select(interval => (interval + i) % 12).OrderBy(interval => interval).ToList();
                            fundamentalShift = (12 - i) % 12; //rotations of leq scale fundamental to match up with scale fundamental
                            break;
                        }
                    }
                    Console.WriteLine($" - {leqScale.CalculateBase(),-2} - {leqScale,-17}  -> {fundamentalShift} : {string.Join(" ", scaleIntervalsInLeqScale)}");
                }
            }
        }
    }
}

//Use with printing all chord progressions and origins for faster than manual lookup
void QueryChordInProgression(Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> chordProgressionsAndOrigins)
{
    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for progression containing chord. (empty input to exit)");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        Tet12KeySet inputKeys = new(Array.ConvertAll(input.Split(' '), int.Parse));

        foreach (Tet12KeySet progression in chordProgressionsAndOrigins.Keys)
        {
            if (inputKeys.IsSubsetTo(progression))
            {
                Console.Write($"{$"{progression.ToIntervalString()}",-20} : ");
                foreach ((int keySteps, Scale legalBaseScale) origin in chordProgressionsAndOrigins[progression])
                {
                    Console.Write($"{$"({origin.legalBaseScale.CalculateBase()}, {origin.keySteps}, {origin.legalBaseScale})",-25} - ");
                }
                Console.WriteLine();
            }
        }
    }
}

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
        WriteMeasuresToMidi(measureList, folderPath, $"base_{scale.CalculateBase()}_keys_{scale.NumberOfKeys()}_number_{i}", true);
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
                int baseValue = scale.CalculateBase();
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
            Console.WriteLine($"{scale}:{scale.CalculateBase()}");
        }
        Console.WriteLine();
    }
}

void PrintAllSuperClassHierarchies(ScaleCalculator scaleCalculator)
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
            if (scaleClasses[previousIndex].Any(scale => scaleClass[0].IsSubClassTo(scale) && scale.CalculateBase() <= 24))
            {
                superClassIndex = previousIndex;
                break;
            }
        }

        if (superClassIndex != null)
            Console.WriteLine($"-Scale index: {scaleClassIndex} <- {superClassIndex}");
        else
            Console.WriteLine($"-Scale index: {scaleClassIndex}");

        if (scaleClass.Any(scale => scale.CalculateBase() <= 24
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
                Console.WriteLine($"{scale} - {scale.CalculateBase()}");
            }
        }

        scaleClassIndex++;

    }
}

void PrintChordSuperClasses(ScaleCalculator scaleCalculator, Scale chord, int maxBase = 24, int minBase = 0)
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

        if (scaleClass.Any(scale => scale.CalculateBase() <= maxBase && scale.CalculateBase() >= minBase
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
                int baseValue = scale.CalculateBase();
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
                    Console.WriteLine($"{scale.KeySet.ToIntervalString()} - {baseValue} <--");
                else
                    Console.WriteLine($"{scale.KeySet.ToIntervalString()} - {baseValue}");
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
                        scale => illegalBases.Contains(scale.CalculateBase())
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
                Console.WriteLine($"{scale} - {scale.CalculateBase()}");
            }
            Console.WriteLine("---");
        }
    }
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

Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> CollapseChordProgressionsByPhysicalKeys(Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> chordProgressionsAndOrigins)
{
    // Clone original list
    Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> collapsedChordProgressions = new();
    foreach (Tet12KeySet keySet in chordProgressionsAndOrigins.Keys)
    {
        collapsedChordProgressions[keySet] = [.. chordProgressionsAndOrigins[keySet]]; //spread element syntx for collection expression cloning
    }
    // Remove redundant entries from clone
    foreach (Tet12KeySet currentKeySet in chordProgressionsAndOrigins.Keys)
    {
        foreach (Tet12KeySet referenceKeySet in chordProgressionsAndOrigins.Keys)
        {
            if (collapsedChordProgressions.ContainsKey(referenceKeySet))
            {
                if (referenceKeySet.IsSubsetTo(currentKeySet) && referenceKeySet != currentKeySet) // Do not remove self
                {
                    var currentOrigins = chordProgressionsAndOrigins[currentKeySet];
                    var referenceOrigins = chordProgressionsAndOrigins[referenceKeySet];
                    if (currentOrigins.Count != referenceOrigins.Count)
                        continue; // different origins, do not collapse
                    bool isBaseAndKeyStepsSame = true;
                    for (int i = 0; i < currentOrigins.Count; i++)
                    {
                        if (currentOrigins[i].keySteps != referenceOrigins[i].keySteps)
                        {
                            isBaseAndKeyStepsSame = false;
                            break;
                        }
                        if (currentOrigins[i].legalBaseScale.CalculateBase() != referenceOrigins[i].legalBaseScale.CalculateBase())
                        {
                            isBaseAndKeyStepsSame = false;
                            break;
                        }
                    }
                    if (isBaseAndKeyStepsSame)
                    {
                        collapsedChordProgressions.Remove(referenceKeySet); // Collapse into current key set
                    }
                }
            }
        }
    }
    return collapsedChordProgressions;
}

static void PrintChordProgressionsAndOrigins(Scale chord, Dictionary<Tet12KeySet, List<(int keySteps, Scale legalBaseScale)>> chordProgressionsAndOrigins)
{
    Console.WriteLine($"Original chord: {chord}");
    Console.WriteLine($"Number of progressions: {chordProgressionsAndOrigins.Keys.Count}");
    //foreach (Tet12KeySet chordProgression in chordProgressionsAndOrigins.Keys.OrderByDescending(cp => cp.ToIntervalString()).ThenByDescending(cp => cp.NumberOfKeys()))
    foreach (Tet12KeySet chordProgression in chordProgressionsAndOrigins.Keys.
        OrderByDescending(cp => chordProgressionsAndOrigins[cp].OrderBy(origin => origin.legalBaseScale.CalculateBase()).First().legalBaseScale.CalculateBase()).
        ThenByDescending(cp => cp.NumberOfKeys()))
    {
        Console.Write($"{$"{chordProgression.ToIntervalString()}",-20} : ");
        foreach ((int keySteps, Scale legalBaseScale) origin in chordProgressionsAndOrigins[chordProgression].OrderBy(origin => origin.legalBaseScale.CalculateBase()))
        {
            Console.Write($"{$"({origin.legalBaseScale.CalculateBase()}, {origin.keySteps}, {origin.legalBaseScale})",-25} - ");
        }
        Console.WriteLine();
    }
}

static Dictionary<Scale, List<Scale>> CalculateAllLEQScalesPerScale(ScaleCalculator scaleCalculator)
{
    Dictionary<Scale, List<Scale>> leqScalesPerScale = new();
    foreach (int length in scaleCalculator.ScaleClassesOfLength.Keys.OrderByDescending(key => key))
    {
        foreach (var scaleClass in scaleCalculator.ScaleClassesOfLength[length])
        {
            //Console.WriteLine($"- Scale Class #{scaleClassIndex}");        
            foreach (var scale in scaleClass)
            {
                int scaleBase = scale.CalculateBase();
                //start out with scaleclass legal bases less or equal to current base
                List<Scale> leqBaseScales = scaleClass.Where(otherScale =>
                    ScaleCalculator.LEGAL_BASES.Contains(otherScale.CalculateBase()) &&
                    otherScale.CalculateBase() <= scaleBase &&
                    otherScale != scale).ToList();
                //find all superclasses' legal bases less or equal to current base
                foreach (var superClass in scaleCalculator.CalculateScaleSuperClasses(scale))
                {
                    foreach (var superScale in superClass)
                    {
                        if (ScaleCalculator.LEGAL_BASES.Contains(superScale.CalculateBase()) && superScale.CalculateBase() <= scaleBase)
                            leqBaseScales.Add(superScale);
                    }
                }
                //If we got something, filter it
                if (leqBaseScales.Count > 0)
                {
                    //only save the largest superscale per base
                    List<Scale> noSubscales = new();
                    Dictionary<int, List<Scale>> baseAndScales = new();
                    foreach (var leqScale in leqBaseScales)
                    {
                        var leqBase = leqScale.CalculateBase();
                        if (!baseAndScales.ContainsKey(leqBase))
                            baseAndScales[leqBase] = new();
                        baseAndScales[leqBase].Add(leqScale);
                    }
                    foreach (var baseValue in baseAndScales.Keys)
                    {
                        foreach (var leqScale in baseAndScales[baseValue])
                        {
                            if (!baseAndScales[baseValue].Any(otherScale => otherScale != leqScale && leqScale.IsSubScaleTo(otherScale)))
                                noSubscales.Add(leqScale);
                        }
                    }
                    //Store superscales                
                    leqScalesPerScale[scale] = noSubscales;
                }
            }
        }
    }

    return leqScalesPerScale;
}

static void PrintFractionApproximations(int maxDenominator = 15)
{
    int columnSpacing = 6;
    var fractionApproximations = ScaleCalculator.CalculateFractionsForApproximations(maxDenominator)
        .ToDictionary(old => old.Key, old => old.Value.OrderBy(item => item).ToList());

    foreach (var key in fractionApproximations.Keys)
        Console.Write($"{key}".PadRight(columnSpacing));
    Console.WriteLine();

    for (int row = 0; row < fractionApproximations.Values.Max(column => column.Count); row++)
    {
        foreach (var column in fractionApproximations.Keys)
        {
            if (row < fractionApproximations[column].Count)
            {
                Console.Write($"{fractionApproximations[column][row]}".PadRight(columnSpacing));
            }
            else
                Console.Write("".PadRight(columnSpacing));
        }
        Console.WriteLine();
    }
}
static void PrintCumulativeFractionApproximations(int minDenominator = 1, int maxDenominator = 15, bool legalNumeratorsOnly = false)
{
    int columnSpacing = 10;
    var fractionApproximations = ScaleCalculator.CalculateFractionsForApproximations(maxDenominator);

    for (int denominator = minDenominator; denominator <= maxDenominator; denominator++)
        Console.Write($"{denominator}".PadRight(columnSpacing));
    Console.WriteLine();

    //Create cumulative approximations
    Dictionary<int, List<Fraction>> cumulativeApproximations = new();
    foreach (var key in fractionApproximations.Keys.OrderBy(key => key))
    {
        cumulativeApproximations[key] = [.. fractionApproximations[key]];
        for (int denominator = 2; denominator < key; denominator++)
        {
            if (key % denominator == 0)
                cumulativeApproximations[key].AddRange([.. fractionApproximations[denominator]]);
        }
    }

    if (legalNumeratorsOnly)
    {
        //if key in dict then legal, value at key is max number of occurences
        Dictionary<int, int> legalPrimes = new() { { 2, 4 }, { 3, 2 }, { 5, 1 } };
        foreach (var key in cumulativeApproximations.Keys)
        {
            cumulativeApproximations[key] = cumulativeApproximations[key].Where(fraction =>
                {
                    if (fraction == new Fraction(7, 5)) //sqrt 2 special case
                        return true;
                    var factors = Factorize((int)fraction.Numerator);
                    foreach (var factor in factors)
                    {
                        //numerator must be small enough
                        if (fraction.Numerator > 30)
                            return false;
                        //factor must be legal
                        if (!legalPrimes.ContainsKey(factor))
                            return false;
                        //factor duplicates cannot exceed max allowed occurences
                        if (factors.Where(otherFactor => otherFactor == factor).Count() > legalPrimes[factor])
                            return false;
                    }
                    return true;
                }
            ).ToList();
        }
    }

    cumulativeApproximations = cumulativeApproximations
        .Where(kv => kv.Key >= minDenominator)
        .ToDictionary(old => old.Key, old => old.Value.OrderBy(item => item).ToList());

    for (int row = 0; row < cumulativeApproximations.Values.Max(column => column.Count); row++)
    {
        foreach (var column in cumulativeApproximations.Keys)
        {
            if (row < cumulativeApproximations[column].Count)
            {
                Console.Write(($"{cumulativeApproximations[column][row]}".PadRight(5) + $"({cumulativeApproximations[column][row].ToDouble().ClosestKey()})").PadRight(columnSpacing));
            }
            else
                Console.Write("".PadRight(columnSpacing));
        }
        Console.WriteLine();
    }
}
static void PrintRelativeDeviations(Dictionary<int, HashSet<Fraction>> fractionApproximations, int keysInTonalSystem)
{
    int columnSpacing = 14;
    Dictionary<int, List<(Fraction approximation, double relativeDeviation)>> relativeDeviationsPerKeyAndApproximation
        = ScaleCalculator.CalculateRelativeDeviationsForEqualToneSystem(fractionApproximations, keysInTonalSystem);

    //Sort approximations by relative deviations
    foreach (var key in relativeDeviationsPerKeyAndApproximation.Keys)
        relativeDeviationsPerKeyAndApproximation[key] = [.. relativeDeviationsPerKeyAndApproximation[key].OrderBy(item => Math.Abs(item.relativeDeviation))];

    foreach (var key in relativeDeviationsPerKeyAndApproximation.Keys.OrderBy(key => key))
        Console.Write($"{key}".PadRight(columnSpacing));
    Console.WriteLine();

    for (int row = 0; row < relativeDeviationsPerKeyAndApproximation.Values.Max(column => column.Count); row++)
    {
        foreach (var column in relativeDeviationsPerKeyAndApproximation.Keys.OrderBy(key => key))
        {
            if (row < relativeDeviationsPerKeyAndApproximation[column].Count)
            {
                Console.Write(
                    (
                    $"{relativeDeviationsPerKeyAndApproximation[column][row].approximation}" +
                    $"({relativeDeviationsPerKeyAndApproximation[column][row].relativeDeviation:0.00})"
                    ).PadRight(columnSpacing));
            }
            else
                Console.Write(" ".PadRight(columnSpacing));
        }
        Console.WriteLine();
    }
}

static void PrintFractionClasses(int maxDenominator = 15)
{
    int columnSpacing = 6;
    var fractionApproximations = ScaleCalculator.CalculateFractionsForApproximations(maxDenominator)
        .ToDictionary(old => old.Key, old => old.Value.OrderBy(item => item).ToList());

    foreach (var denominator in fractionApproximations.Keys)
    {
        Console.WriteLine($"{denominator}:");
        HashSet<Fraction> fractionsWithDenominator = fractionApproximations[denominator].ToHashSet();
        fractionsWithDenominator.Add(1);
        List<HashSet<Fraction>> fractionClasses = ScaleCalculator.CalculateFractionClasses(fractionsWithDenominator);
        foreach (var fractionClass in fractionClasses)
        {
            foreach (var fraction in fractionClass)
                Console.Write($"{fraction}".PadRight(columnSpacing));
            Console.WriteLine();
        }
    }
}

static void PrintVirtualFundamentals(int maxPacketLength = 15)
{
    var virtualFundamentals = ScaleCalculator.CalculateVirtualFundamentals(maxPacketLength);

    foreach (var fundamental in virtualFundamentals.OrderBy(fraction => fraction))
    {
        Console.WriteLine(fundamental);
    }
}

static void QueryFractionFundamentalClass(int maxPacketLength = 15, bool toOctave = true)
{
    while (true)
    {
        Console.WriteLine($"Input space separated fractions for fraction fundamental class up to {maxPacketLength} denominator. (empty input to exit)");
        string input = Console.ReadLine();

        if (input.Length == 0) return;
        Fraction[] inputKeys = Array.ConvertAll(input.Split(' '), split =>
        {
            if (split.Contains('/'))
            {
                string[] inputSplit = split.Split('/');
                return new(int.Parse(inputSplit[0]), int.Parse(inputSplit[1]));
            }
            else
                return new Fraction(1);
        });
        Dictionary<Fraction, HashSet<Fraction>> fractionFundamentalClass = ScaleCalculator.CalculateFractionFundamentalClass(inputKeys.ToHashSet(), maxPacketLength);
        if (toOctave == true)
        {
            //remove doubles from octave transposition
            fractionFundamentalClass = fractionFundamentalClass.GroupBy(
                kv => kv.Key.ToOctave(),
                kv => kv.Value,
                (key, values) => new { fraction = key, fractionClass = values.First() }
                ).ToDictionary(kv => kv.fraction, kv => kv.fractionClass);
            foreach (var fundamental in fractionFundamentalClass.Keys
                .OrderBy(key => LCM(fractionFundamentalClass[key].Select(fraction => (long)fraction.ToOctave().Denominator).ToArray()))
                .ThenBy(key => key.ToOctave()))
            {
                var lcmForFundamental = LCM(fractionFundamentalClass[fundamental].Select(fraction => (long)fraction.ToOctave().Denominator).ToArray());
                Console.WriteLine($"{fundamental.ToOctave(),-5}: " +
                    $"{lcmForFundamental} - " +
                    $"{string.Join(" ", fractionFundamentalClass[fundamental].Select(fraction => fraction.ToOctave()))}");
            }
        }
        else
        {
            foreach (var fundamental in fractionFundamentalClass.Keys
                .OrderBy(key => LCM(fractionFundamentalClass[key].Select(fraction => (long)fraction.Denominator).ToArray()))
                .ThenBy(key => key))
            {
                var lcmForFundamental = LCM(fractionFundamentalClass[fundamental].Select(fraction => (long)fraction.Denominator).ToArray());
                Console.WriteLine($"{fundamental,-5}: " +
                    $"{lcmForFundamental} - " +
                    $"{string.Join(" ", fractionFundamentalClass[fundamental])}");
            }
        }
    }
}