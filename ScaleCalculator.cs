using Fractions;
using Melanchall.DryWetMidi.MusicTheory;
using System.Numerics;
using System.Text;
using static MusicTheory.MusicTheoryUtils;

namespace MusicTheory
{
    public class ScaleCalculator
    {
        public HashSet<List<Scale>> ScaleClasses = new();
        public Dictionary<Scale, List<Scale>> ScaleClassForScale = new();
        public Dictionary<int, List<List<Scale>>> ScaleClassesOfLength = new();
        public Dictionary<int, HashSet<Scale>> ScalesWithBase = new();

        public static List<int> LEGAL_BASES = new() { 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 24, 30 };
        //public static List<int> LEGAL_BASES = new() { 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 24 };

        public ScaleCalculator()
        {
            InitScaleClassForScale();
            InitScaleClasses();
            InitScaleClassesOfLength();
            InitScalesWithBase();
        }

        private void InitScalesWithBase()
        {
            foreach (var scaleClass in ScaleClasses)
            {
                foreach (var scale in scaleClass)
                {
                    int baseValue = scale.CalculateBase();
                    if (!ScalesWithBase.ContainsKey(baseValue))
                        ScalesWithBase[baseValue] = new();
                    ScalesWithBase[baseValue].Add(scale);
                }
            }
        }

        private void InitScaleClassesOfLength()
        {
            foreach (var scaleClass in ScaleClasses)
            {
                int length = scaleClass[0].KeySet.NumberOfKeys();

                if (!ScaleClassesOfLength.ContainsKey(length))
                    ScaleClassesOfLength[length] = new();

                ScaleClassesOfLength[length].Add(scaleClass);
            }
        }

        private void InitScaleClasses()
        {
            foreach (var rotationClass in ScaleClassForScale.Values)
            {
                ScaleClasses.Add(rotationClass);
            }
        }

        private void InitScaleClassForScale()
        {
            for (int combination = 1; combination < BigInteger.Pow(2, 12); combination++)
            {
                Tet12KeySet keySet = new(combination);
                List<Scale> rotationClass = new();
                for (int i = 0; i < 12; i++)
                {
                    keySet = keySet.RotateBinaryRight();

                    if ((keySet.BinaryRepresentation & 1) == 0) continue; //only keep scales (key sets with fundamentals - i.e. first bit set)

                    Scale scale = new(keySet);
                    if (ScaleClassForScale.ContainsKey(scale)) //Try next combination - rotation class already registered
                        break;
                    else //implies no rotation of the scale has yet been registered
                    {
                        rotationClass.Add(scale);
                        ScaleClassForScale[scale] = rotationClass;
                    }
                }
            }
        }

        public Scale ScaleFromChords(List<(int fundamental, Scale scale)> chords)
        {
            HashSet<int> intervals = new();
            foreach (var chord in chords)
            {
                foreach (var interval in chord.scale.ToIntervals())
                {
                    intervals.Add((interval + chord.fundamental) % 12);
                }
            }
            return new(intervals.ToArray());
        }

        public List<(int, Scale)> ChordsInScale(Scale scale, List<Scale> chords)
        {
            List<(int, Scale)> chordsInScale = new();
            for (int i = 0; i < 12; i++)
            {
                foreach (var chord in chords)
                {
                    if (((scale.KeySet.BinaryRepresentation >> i) & chord.KeySet.BinaryRepresentation) == chord.KeySet.BinaryRepresentation)
                        chordsInScale.Add((i, chord));
                }
            }
            return chordsInScale;
        }

        public List<List<Scale>> CalculateScaleSuperClasses(Scale scale)
        {
            List<List<Scale>> superClasses = new();
            for (int length = 12; length > scale.NumberOfKeys(); length--)
            {
                foreach (var scaleClass in ScaleClassesOfLength[length])
                {
                    if (scale.IsSubClassTo(scaleClass.First())) //arbitrary member of the larger scale class
                        superClasses.Add(scaleClass);
                }
            }
            return superClasses;
        }

        //Chord progressions may be "hidden keys" in a superclass to a scale, moving from larger to smaller bases in the superscale
        //A superclass progression is a chord progression to a superscale in a superclass
        public List<List<(int keySteps, Scale legalKeys)>> CalculateSuperClassProgressionsPerSuperClass(Scale chord)
        {
            List<List<Scale>> superClasses = CalculateScaleSuperClasses(chord);
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

                    if (chord.IsSubScaleTo(rotatedScale))
                        superScaleRotations.Add(rotations); // Found superscale to our chord at current rotations

                    if (LEGAL_BASES.Contains(rotatedScale.CalculateBase()))
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

        public static NoteValue?[] ScaleToNoteValues(Scale scale)
        {
            /** Example usage
                NoteValue?[] noteValues = ScaleCalculator.ScaleToNoteValues(scale);
                Measure measure = new(noteValues);
                List<Measure> measureList = [measure];
                WriteMeasuresToMidi(measureList, folderPath, $"file_name", true); 
             **/
            int timeDivision = 12;
            NoteValue?[] noteValues = new NoteValue?[timeDivision];
            for (int i = 0; i < timeDivision; i++)
            {
                if (((scale.KeySet.BinaryRepresentation >> i) & 1) == 1)
                {
                    NoteValue value = new(NoteName.C, 4, 64);
                    value += i;
                    noteValues[i] = value;
                    if (i + 1 < timeDivision)
                        noteValues[i + 1] = NoteValue.SilentNote;
                }
            }
            return noteValues;
        }

        // Fractions used for fraction approximations are on the interval [1,2) and have a maximal denominator of roughly 5-50
        // The interval is due to musical sounds empiricially behaving cyclically on such an interval
        // The maximal denominator is due to the hearings lower bound on 20hz, meaning a sound packet can not exceed 50ms
        // - with musical sounds roughly being around 100-1000hz, the maximal (composite) denominator for a sound pattern becomes 5-50
        public static Dictionary<int, HashSet<Fraction>> CalculateFractionsForApproximations(int maxDenominator)
        {
            Dictionary<int, HashSet<Fraction>> denominatorsAndFractions = new();
            for (int denominator = 1; denominator <= maxDenominator; denominator++)
            {
                for (int numerator = denominator; numerator < 2 * maxDenominator; numerator++)
                {
                    Fraction fraction = new(numerator, denominator);
                    if (!denominatorsAndFractions.ContainsKey((int)fraction.Denominator))
                        denominatorsAndFractions[(int)fraction.Denominator] = new();
                    denominatorsAndFractions[(int)fraction.Denominator].Add(fraction);
                }
            }
            return denominatorsAndFractions;
        }

        //Fraction approximations can deviate from equal tone system keys
        //Tone systems generally produce real value tones which the brain parses to rational value tones
        //Relative deviation of fraction approximations to tone system tones is likely an important factor in which tone gets approximated with what
        //TODO what if two deviations are very similar?
        public static Dictionary<int, List<(Fraction approximation, double relativeDeviation)>> CalculateRelativeDeviationsForEqualToneSystem(
            Dictionary<int, HashSet<Fraction>> fractionApproximations, int keysInTonalSystem)
        {
            Dictionary<int, List<(Fraction approximation, double relativeDeviation)>> relativeErrorForFractionApproximationPerKey = new();
            Dictionary<int, double> relativeHzPerKey = new();
            for (int key = 0; key < keysInTonalSystem; key++)
                relativeHzPerKey[key] = Math.Pow(2, key / (double)keysInTonalSystem);
            foreach (Fraction fractionApproximation in fractionApproximations.SelectMany(item => item.Value))
            {
                double smallestDeviation = 100; //some large initial error                
                int key = -1;
                foreach (var relativeHz in relativeHzPerKey)
                {
                    double relativeDeviation = (1 - (double)fractionApproximation / relativeHz.Value);
                    if (Math.Abs(relativeDeviation) < Math.Abs(smallestDeviation))
                    {
                        smallestDeviation = relativeDeviation;
                        key = relativeHz.Key;
                    }
                }
                if (!relativeErrorForFractionApproximationPerKey.ContainsKey(key))
                    relativeErrorForFractionApproximationPerKey[key] = new();
                relativeErrorForFractionApproximationPerKey[key].Add((fractionApproximation, smallestDeviation));
            }
            return relativeErrorForFractionApproximationPerKey;
        }
    }


    public struct Bit12Int
    {
        private const int _bitSize = 12;
        private const int _maxValue = (1 << _bitSize) - 1;

        private int _value;

        public Bit12Int(int initialValue)
        {
            _value = initialValue & _maxValue;
        }

        public static Bit12Int operator <<(Bit12Int left, int rotations)
        {
            rotations %= _bitSize;
            return new((left._value << rotations | left._value >> (_bitSize - rotations)) & _maxValue);
        }
        public static Bit12Int operator >>(Bit12Int left, int rotations)
        {
            rotations %= _bitSize;
            return new((left._value >> rotations | left._value << (_bitSize - rotations)) & _maxValue);
        }

        public int GetValue()
        {
            return _value;
        }

        public static string Bit12IntToIntervalString(Bit12Int binaryKeySet)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 12; i++)
            {
                if (((binaryKeySet >> i) & 1) == 1)
                    sb.Append($"{i} ");
            }
            if (sb.Length > 1)
                sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static HashSet<int> Bit12IntToIntervals(Bit12Int binaryKeySet)
        {
            HashSet<int> intervals = new();
            for (int i = 0; i < 12; i++)
            {
                if (((binaryKeySet >> i) & 1) == 1)
                    intervals.Add(i);
            }

            return intervals;
        }

        public static int operator |(Bit12Int left, int right)
        {
            return left._value | right;
        }

        public static Bit12Int operator |(Bit12Int left, Bit12Int right)
        {
            return new Bit12Int(left._value | (int)right);
        }

        public static int operator &(Bit12Int left, int right)
        {
            return left._value & right;
        }

        public static Bit12Int operator &(Bit12Int left, Bit12Int right)
        {
            return new Bit12Int(left._value & (int)right);
        }

        public static explicit operator Bit12Int(int value)
        {
            return new Bit12Int(value);
        }

        public static explicit operator int(Bit12Int value)
        {
            return value._value;
        }

        public static bool operator ==(Bit12Int left, Bit12Int right)
        {
            return left._value == right._value;
        }

        public static bool operator ==(Bit12Int left, int right)
        {
            return left._value == right;
        }

        public static bool operator !=(Bit12Int left, Bit12Int right)
        {
            return left._value != right._value;
        }

        public static bool operator !=(Bit12Int left, int right)
        {
            return left._value != right;
        }

        public override string ToString()
        {
            return Convert.ToString(_value, 2).PadLeft(12, '0');
        }
    }

    public struct Tet12KeySet
    {
        public Bit12Int BinaryRepresentation;

        public Tet12KeySet(string keySetAsBinaryString)
        {
            if (keySetAsBinaryString.Length < 12)
                keySetAsBinaryString = keySetAsBinaryString.PadLeft(12, '0');

            if (keySetAsBinaryString.Length != 12) throw new ArgumentException("12TET key set must consist of exactly 12 keys");
            if (keySetAsBinaryString.Any(key => key != '0' && key != '1')) throw new ArgumentException("12TET key set keys must be either 1 (included in scale) or 0 (excluded from scale)");

            BinaryRepresentation = (Bit12Int)Convert.ToInt32(keySetAsBinaryString, 2);
        }

        public Tet12KeySet(Bit12Int keySet)
        {
            BinaryRepresentation = keySet;
        }

        public Tet12KeySet(int keySetAsInt)
        {
            BinaryRepresentation = new(keySetAsInt);
        }

        public Tet12KeySet(int[] keysAsIntervals)
        {
            int keysAsBinary = 0;
            foreach (int key in keysAsIntervals)
            {
                int transposedKey = key;
                while (transposedKey < 0)
                    transposedKey += 12;
                while (transposedKey > 12)
                    transposedKey -= 12;

                keysAsBinary += 1 << transposedKey;
            }
            BinaryRepresentation = new(keysAsBinary);
        }

        //calculates scale and fundamental shift
        public List<(int shift, Scale scale)> CalculateFundamentalClass()
        {
            List<(int shift, Scale scale)> fundamentalClass = new();
            for (int i = 0; i < 12; i++)
            {
                Tet12KeySet keySet = new Tet12KeySet((BinaryRepresentation >> i) | 1); //make sure there is a (virtual) fundamental for the scale

                fundamentalClass.Add((i, new(keySet)));
            }
            return fundamentalClass;
        }

        public int CalculateBase()
        {
            return CalculateBase(Scale.TET12_STANDARD_FRACTION_APPROXIMATIONS);
        }

        public int CalculateBase(Fraction[] keyFractionApproximations)
        {
            if (keyFractionApproximations.Length != 12)
                throw new ArgumentException("fraction approximations must equal number of keys in the scale");

            Bit12Int binaryRepresentation = new(BinaryRepresentation | 1); //set first bit to represent (possibly virtual) fundamental
            List<long> denominators = new();
            for (int i = 0; i < 12; i++)
            {
                if (((binaryRepresentation >> i) & 1) == 1)
                    denominators.Add((long)keyFractionApproximations[i].Denominator);
            }
            return (int)LCM(denominators.ToArray());
        }

        public Tet12KeySet RotateBinaryLeft()
        {
            return new Tet12KeySet(BinaryRepresentation << 1);
        }

        public Tet12KeySet RotateBinaryRight()
        {
            return new Tet12KeySet(BinaryRepresentation >> 1);
        }

        public int NumberOfKeys()
        {
            int keys = 0;
            int value = (int)BinaryRepresentation;
            while (value > 0)
            {
                if ((value & 1) == 1)
                    keys++;
                value >>= 1;
            }
            return keys;
        }

        public static Tet12KeySet operator >>(Tet12KeySet left, int right)
        {
            return new(left.BinaryRepresentation >> right);
        }
        public static Tet12KeySet operator <<(Tet12KeySet left, int right)
        {
            return new(left.BinaryRepresentation << right);
        }

        public static bool operator ==(Tet12KeySet left, Tet12KeySet right)
        {

            return left.BinaryRepresentation == right.BinaryRepresentation;
        }

        public static bool operator !=(Tet12KeySet left, Tet12KeySet right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is Tet12KeySet other && Equals(other);
        }

        public bool Equals(Tet12KeySet other)
        {
            return BinaryRepresentation == other.BinaryRepresentation;
        }

        public override int GetHashCode()
        {
            return (int)BinaryRepresentation;
        }

        public override string ToString()
        {
            return BinaryRepresentation.ToString();
        }

        public string ToIntervalString()
        {
            return Bit12Int.Bit12IntToIntervalString(BinaryRepresentation);
        }

        public HashSet<int> ToIntervals()
        {
            return Bit12Int.Bit12IntToIntervals(BinaryRepresentation);
        }

        internal bool IsSubsetTo(Tet12KeySet superSet)
        {
            return (BinaryRepresentation & superSet.BinaryRepresentation) == BinaryRepresentation;
        }
    }

    public struct Scale
    {
        public Tet12KeySet KeySet;
        public static Fraction[] TET12_STANDARD_FRACTION_APPROXIMATIONS =
            [
                new(1),
                new(16, 15),
                new(9, 8),
                new(6, 5),
                new(5, 4),
                new(4, 3),
                new(7, 5),
                new(3, 2),
                new(8, 5),
                new(5, 3),
                new(16, 9), //9/5, 7/4, 16/9? 9/5 seems good but some inconsistencies, e.g. 0 4 5 7 8 9 11 - seems like either 9 or 7 works but not both. 16/9 has symmetry with 9/8.
                new(15, 8)
            ];

        public Scale(Tet12KeySet keySet)
        {
            if ((keySet.BinaryRepresentation & 1) != 1) throw new ArgumentException($"A scale must have a fundamental - {nameof(Tet12KeySet)} did not set first bit ");
            KeySet = keySet;
        }

        public Scale(string keySetAsBinaryString)
        {
            KeySet = new Tet12KeySet(keySetAsBinaryString);
        }

        public Scale(int[] tet12Keys)
        {
            KeySet = new Tet12KeySet(tet12Keys);
        }

        public bool Contains(Tet12KeySet keySet)
        {
            return (KeySet.BinaryRepresentation & keySet.BinaryRepresentation) == keySet.BinaryRepresentation;
        }

        public bool Contains(Scale scale)
        {
            return Contains(scale.KeySet);
        }

        public override string ToString()
        {
            return Bit12Int.Bit12IntToIntervalString(KeySet.BinaryRepresentation);
        }

        public HashSet<int> ToIntervals()
        {
            return Bit12Int.Bit12IntToIntervals(KeySet.BinaryRepresentation);
        }

        public int CalculateBase()
        {
            return CalculateBase(TET12_STANDARD_FRACTION_APPROXIMATIONS);
        }

        public int CalculateBase(Fraction[] keyFractionApproximations)
        {
            return KeySet.CalculateBase(keyFractionApproximations);
        }

        public int NumberOfKeys()
        {
            return KeySet.NumberOfKeys();
        }

        public Scale Transpose()
        {
            return new(ToIntervals().Select(interval => -interval).ToArray());
        }

        public HashSet<Scale> CalculateScaleClass()
        {
            HashSet<Scale> scaleClass = new();
            for (int i = 0; i < 12; i++)
            {
                Tet12KeySet keySet = this >> i;
                if ((keySet.BinaryRepresentation & 1) == 1)
                    scaleClass.Add(new(keySet));
            }
            return scaleClass;
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
        public List<int>[] CalculateKeyMultiplicity(Scale chord)
        {
            Dictionary<int, Scale> leftTranslatedScales = new();
            for (int i = 0; i < 12; i++)
            {
                //Check if rotated scale is still a scale
                Tet12KeySet rotatedKeys = this >> i;
                if ((rotatedKeys.BinaryRepresentation & 1) == 1)
                {
                    //Record if scale contains chord after rotation
                    // - rotating the scale's binary representation right is equivalent to offseting the scale left via translation
                    Scale rotatedScale = new(rotatedKeys);
                    if (rotatedScale.Contains(chord))
                        leftTranslatedScales[i] = rotatedScale;
                }
            }

            //Find how many scales each key can belong to
            List<int>[] keyMultiplicity = new List<int>[12];
            for (int i = 0; i < keyMultiplicity.Length; i++)
            {
                keyMultiplicity[i] = new();
            }

            for (int i = 0; i < 12; i++)
            {
                foreach (var translationAndScale in leftTranslatedScales)
                {
                    //Check if key is in the translated scale - if yes add the fundamental
                    if (((translationAndScale.Value >> i).BinaryRepresentation & 1) == 1)
                        keyMultiplicity[i].Add((12 - translationAndScale.Key) % 12);
                }
            }
            return keyMultiplicity;
        }

        public bool IsSubClassTo(Scale superClassScale)
        {
            HashSet<Scale> superClass = superClassScale.CalculateScaleClass();

            if (superClass.Any(IsSubScaleTo))
                return true;
            return false;
        }

        public bool IsSubScaleTo(Scale superScale)
        {
            return (this & superScale) == KeySet;
        }

        public static Tet12KeySet operator <<(Scale left, int right)
        {
            return new Tet12KeySet(left.KeySet.BinaryRepresentation << right);
        }

        public static Tet12KeySet operator >>(Scale left, int right)
        {
            return new Tet12KeySet(left.KeySet.BinaryRepresentation >> right);
        }

        public static Tet12KeySet operator &(Scale left, Scale right)
        {
            return new Tet12KeySet(left.KeySet.BinaryRepresentation & right.KeySet.BinaryRepresentation);
        }

        public static bool operator ==(Scale left, Scale right)
        {
            return left.KeySet.BinaryRepresentation == right.KeySet.BinaryRepresentation;
        }

        public static bool operator !=(Scale left, Scale right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is Scale other && Equals(other);
        }

        public bool Equals(Scale other)
        {
            return KeySet == other.KeySet;
        }
    }

    //Node used to represent a scale in the chord progression graph
    public class ScaleNode
    {
        public Scale Scale;
        public int Base;
        public Dictionary<Scale, int> Edges = new();
        public ScaleNode(Scale scale)
        {
            Scale = scale;
            Base = scale.CalculateBase();
        }
    }

    //Working hypothesis - rules of progression:
    //1. Can move to any superscale if the superclass has a legal base
    //1.1. Strict superscales (no rotation) have edge weight 0
    //1.2. Rotated superscales have edge weight r % 12 with r = #rotations (take care with rotation sign)
    //2. Can move to any subscale if the subclass has a legal base (sufficient that there exists a superclass to the subclass with a legal base?)
    //2.1. Strict subscales (no rotation) have edge weight 0
    //2.2. Subscales from a rotated original scale have edge weight r % 12 with r = #rotations (take care with rotation sign)
    public class ChordProgressionGraph
    {
        public Dictionary<Scale, ScaleNode> Nodes = new();
        public ChordProgressionGraph(ScaleCalculator scaleCalculator)
        {
            //Create all possible scales. Keep the ones with legal base.
            //Only odd numbers represent scales since the least significant bit must be set for a fundamental to exist
            //Skip the single key scale - thus start at i = 3
            for (int i = 3; i < BigInteger.Pow(2, 12); i += 2)
            {
                ScaleNode node = new(new Scale(new Tet12KeySet(i)));
                if (ScaleCalculator.LEGAL_BASES.Contains(node.Base))
                    Nodes[node.Scale] = node;
            }
            //Create all edges
            foreach (ScaleNode node in Nodes.Values)
            {
                List<List<(int keySteps, Scale legalKeys)>> superClasses = scaleCalculator.CalculateSuperClassProgressionsPerSuperClass(node.Scale);
                foreach (List<(int keySteps, Scale legalKeys)> chordProgressions in superClasses)
                {
                    foreach ((int keySteps, Scale legalKeys) chordProgression in chordProgressions)
                    {
                        node.Edges[chordProgression.legalKeys] = chordProgression.keySteps; //Edge to superclass
                        Nodes[chordProgression.legalKeys].Edges[node.Scale] = (-chordProgression.keySteps + 12) % 12; //Reverse edge is same as edge to subclass
                    }
                }
            }
        }
        public ScaleNode this[Scale key]
        {
            get => Nodes[key];
            set => Nodes[key] = value;
        }
    }

    public class ChordProgressionPathFinder
    {
        ChordProgressionGraph _progressionGraph;
        public ChordProgressionPathFinder(ChordProgressionGraph chordProgressionGraph)
        {
            _progressionGraph = chordProgressionGraph;
        }

        public Queue<ChordPath> FindPathsFrom(Scale chord, int pathLength)
        {
            if (pathLength < 2)
                throw new ArgumentException("Path must be at least 2 long - a start and an end is required");

            Queue<ChordPath> currentQueue = new();
            Queue<ChordPath> nextQueue = new();

            ChordPath origin = new();
            origin.Add(_progressionGraph[chord], 0);
            currentQueue.Enqueue(origin);

            for (int i = 1; i < pathLength; i++) //start at 1 - step 0 is adding origin node
            {
                while (currentQueue.Count > 0)
                {
                    ChordPath currentPath = currentQueue.Dequeue();
                    foreach (KeyValuePair<Scale, int> edge in currentPath.Nodes.Last().Edges)
                    {
                        ScaleNode nextNode = _progressionGraph[edge.Key];
                        if (currentPath.Nodes.Count < 2) { }
                        else if (currentPath.Nodes[^2] == nextNode) //skip direct backtracking, if desired can be inferred from the path by bouncing between adjacent nodes.
                        {
                            continue;
                        }
                        ChordPath nextPath = currentPath.Clone();
                        nextPath.Add(nextNode, edge.Value);
                        nextQueue.Enqueue(nextPath);
                    }
                }

                currentQueue = nextQueue;
                nextQueue = new();
            }
            return currentQueue;
        }
    }
    public class ChordPath
    {
        public List<ScaleNode> Nodes = new();
        public List<int> PathSteps = new();
        public void Add(ScaleNode scale, int keySteps)
        {
            Nodes.Add(scale);
            PathSteps.Add(keySteps);
        }

        //Note that ScaleNodes are shallow copies
        public ChordPath Clone()
        {
            return new ChordPath() { Nodes = new(Nodes), PathSteps = new(PathSteps) };
        }
    }
}