// See https://aka.ms/new-console-template for more information
using Fractions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melodroid.Harmonizers;
using MusicTheory;
using Serilog;
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
int deviationsPerMeasure = 2;
SimpleIsochronicRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure);
//SimpleGrooveRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure, deviationsPerMeasure);
//List<List<PatternBlock>> measurePatternBlocks = [
//    [new("A", 8), new("B", 4), new("B", 4)],
//    [new("A", 8), new("B", 4), new("C", 4)],
//    [new("A", 8), new("B", 4), new("B", 4)],
//    [new("D", 8), new("B", 4), new("E", 4)]
//];
//SimpleMeasurePatternRhythmMaker rhythmMaker = new(timeDivision, numberOfMeasures, beatsPerMeasure, deviationsPerMeasure, measurePatternBlocks);


//Scale initialScale = new(new int[] { 0, 4, 7 });
//Scale majorChord = new(new int[] { 0, 4, 7 });
//Scale minorChord = new(new int[] { 0, 3, 7 });
//List<Scale> chordProgression = [majorChord, minorChord];

////Select harmonizer
//ScaleClassRotationHarmonizer harmonizer = new(initialScale);
//ChordMeasureProgressionHarmonizer harmonizer = new(chordProgression);
//ScaleClassRotationTransposeHarmonizer harmonizer = new(initialScale);
//RandomKeyMultiplicityHarmonizer harmonizer = new();
//ChordPreferenceKeyMultiplicityHarmonizer harmonizer = new();
ChordPreferenceKeyMultiplicityPhraseHarmonizer harmonizer = new();

//RandomNoteHarmonizer randomNoteHarmonizer = new();

//RandomChordNoteHarmonizer harmonizer = new(initialScale);

//RandomWalkMeasureHarmonizer measureHarmonizer = new(initialScale);
//PathWalkMeasureHarmonizer measureHarmonizer = new(initialScale, initialScale, 4);
//BeatBox beatBox = new BeatBox(rhythmMaker, measureHarmonizer);

//Write MIDI files
//BeatBox beatBox = new BeatBox(rhythmMaker, harmonizer);

//List<Measure> melodyMeasures = beatBox.MakeMeasures();
//beatBox.WriteMeasuresToMidi(melodyMeasures, folderPath, "key_multiplicity_test", true);

//ChordMeasureHarmonizer chordHarmonizer = new(harmonizer.ChordPerMeasure, 4);
//List<Measure> chordMeasures = chordHarmonizer.MeasuresFromVelocities(rhythmMaker.VelocityMeasures);
//beatBox.WriteMeasuresToMidi(chordMeasures, folderPath, "key_multiplicity_chord_test", true);


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

//Scale base24Scale = new(new int[] { 0, 2, 4, 5, 7, 9, 11 });
//Scale base15Scale = new(new int[] { 0, 1, 3, 5, 6, 8, 9, 10 });
//Scale base15ScaleLeft = new(new int[] { 0, 1, 3, 5, 6, 8, 9 });
//Scale base15ScaleRight = new(new int[] { 0, 1, 3, 5, 6, 9, 10 });
//Scale base30Scale = new(new int[] { 0, 1, 3, 5, 6, 7, 8, 9, 10 });
//List<Fraction> base15FractionsFrom0 = base15Scale.ToFractions();
//List<Fraction> base15FractionsFrom1 = [.. base15FractionsFrom0.Select(fraction => (fraction * new Fraction(15, 16)).ToOctave())];

//PrintClosestFractionsBetweenScales(base15Scale, base24Scale);
//Console.WriteLine("---");
//PrintClosestFractionsBetweenScales(base15ScaleLeft, base24Scale);
//Console.WriteLine("---");
//PrintClosestFractionsBetweenScales(base15ScaleRight, base24Scale);

QueryChordKeyMultiplicity(scaleCalculator);
//QueryFundamentalClassPerScale(scaleCalculator);
//QueryChordProgressionFromMultiplicity(scaleCalculator);
//QueryChordInKeySetTranslations();





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
        //new([0, 1, 3, 5, 6, 9, 10]), //base 15 - seems like the full base 15 collapses to base 24 due to the 8/5 creating base24 at C or G for base 15 at B
        new([0, 1, 3, 5, 6, 8, 9, 10]), //full base 15
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
        Console.WriteLine($"Input space separated tet12 keys for chord to calculate chord key multiplicity (see method comment for theory - empty input to exit)");
        string chordInput = Console.ReadLine();

        if (chordInput.Length == 0)
            return;

        Scale chord = new(Array.ConvertAll(chordInput.Split(' '), int.Parse));
        //Find all matches for chord per scale of interest
        foreach (Scale scale in scalesOfInterest)
        {
            List<int>[] chordKeyMultiplicity = scale.CalculateKeyMultiplicity(chord);
            //Print results
            for (int i = 0; i < 12; i++)
            {
                Console.Write($"{i}".PadRight(3));
            }
            Console.WriteLine();
            Console.WriteLine();
            for (int row = 0; row < chordKeyMultiplicity.Max(columnValues => columnValues.Count); row++)
            {
                for (int column = 0; column < 12; column++)
                {
                    //print scale root
                    if (row < chordKeyMultiplicity[column].Count)
                        Console.Write($"{chordKeyMultiplicity[column][row]}".PadRight(3));
                    else
                        Console.Write("   ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}

void QueryChordProgressionFromMultiplicity(ScaleCalculator scaleCalculator)
{
    List<Scale> scalesOfInterest = [
        new([0, 1, 3, 5, 6, 8, 9]),  //base 15 - 1, 16/15, 4/3, 6/5, 7/5(sqrt(2)), 8/5, 5/3 (if both 8 and 10 then becomes base 24 at 1)
        new([0, 1, 3, 5, 6, 9, 10]), //base 15 - 1, 16/15, 6/5, 4/3, 7/5(sqrt(2)), 5/3, 9/5 (if both 8 and 10 then becomes base 24 at 1)        
        new([0, 2, 4, 7, 11]),  //base 8
        new([0, 2, 4, 5, 7, 9, 11])  //base 24 - 1, 9/8, 5/4, 5/4, 3/2, 5/3, 15/8
        ];

    Console.WriteLine("Matching input against scales:");
    foreach (Scale scale in scalesOfInterest)
    {
        Console.WriteLine($"{scale.CalculateBase(),-2}: {scale}");
    }

    while (true)
    {
        Console.WriteLine($"Input space separated tet12 keys for chord current chord");
        string chordInput = Console.ReadLine();

        Scale currentChord = new(Array.ConvertAll(chordInput.Split(' '), int.Parse));

        Console.WriteLine($"Input space separated tet12 keys for chord to progress to. Empty input to exit");
        chordInput = Console.ReadLine();

        if (chordInput.Length == 0)
            return;

        Scale progressionChord = new(Array.ConvertAll(chordInput.Split(' '), int.Parse));
        foreach (Scale scale in scalesOfInterest)
        {
            //calculate multiplicity to get all scale fundamental shifts
            var multiplicity = scale.CalculateKeyMultiplicity(currentChord);
            //Combine with all occurences of the progression chord in the scale for all progressions root positions
            List<int> chordPositionsInScale = new();
            for (int i = 0; i < 12; i++)
            {
                //chord in scale rotation?
                if (((scale >> i).BinaryRepresentation & progressionChord.KeySet.BinaryRepresentation) == progressionChord.KeySet.BinaryRepresentation)
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
