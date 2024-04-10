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
        public Dictionary<int, List<Scale>> ScalesWithBase = new();

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
                    int baseValue = scale.GetBase();
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

        public List<List<Scale>> CalculateScaleSuperClasses(Scale scale)
        {
            List<List<Scale>> superClasses = new();
            for (int length = 12; length > scale.NumberOfKeys(); length--)
            {
                foreach (var scaleClass in ScaleClassesOfLength[length])
                {
                    if (scale.isSubClassTo(scaleClass.First())) //arbitrary member of the larger scale class
                        superClasses.Add(scaleClass);
                }
            }
            return superClasses;
        }

        //Chord progressions are basically "hidden keys" in a superclass to a scale, moving from larger to smaller bases in the superscale
        public List<List<(int keySteps, Scale legalKeys)>> CalculateChordProgressionsPerSuperClass(Scale chord)
        {
            List<int> LEGAL_BASES = new() { 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 24 };

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
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static List<int> Bit12IntToIntervals(Bit12Int binaryKeySet)
        {
            List<int> intervals = new();
            for (int i = 0; i < 12; i++)
            {
                if (((binaryKeySet >> i) & 1) == 1)
                    intervals.Add(i);
            }

            return intervals;
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

        public static bool operator !=(Bit12Int left, Bit12Int right)
        {
            return left._value != right._value;
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
                keysAsBinary += 1 << key;
            }
            BinaryRepresentation = new(keysAsBinary);
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
                new(9, 5), //perhaps it is 9/5, perhaps both? 9/5 produces more interesting results.
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

        public List<int> AsIntervals()
        {
            return Bit12Int.Bit12IntToIntervals(KeySet.BinaryRepresentation);
        }

        public int GetBase()
        {
            return GetBase(TET12_STANDARD_FRACTION_APPROXIMATIONS);
        }

        public int GetBase(Fraction[] keyFractionApproximations)
        {
            if (keyFractionApproximations.Length != 12) throw new ArgumentException("fraction approximations must equal number of keys in the scale");
            List<long> denominators = new();
            for (int i = 0; i < 12; i++)
            {
                if (((KeySet.BinaryRepresentation >> i) & 1) == 1)
                    denominators.Add((long)keyFractionApproximations[i].Denominator);
            }
            return (int)LCM(denominators.ToArray());
        }

        public int NumberOfKeys()
        {
            return KeySet.NumberOfKeys();
        }

        public override string ToString()
        {
            return Bit12Int.Bit12IntToIntervalString(KeySet.BinaryRepresentation);
        }

        public List<Scale> CalculateScaleClass()
        {
            List<Scale> scaleClass = new();
            for (int i = 0; i < 12; i++)
            {
                Tet12KeySet keySet = this >> i;
                if ((keySet.BinaryRepresentation & 1) == 1)
                    scaleClass.Add(new(keySet));
            }
            return scaleClass;
        }

        public bool isSubClassTo(Scale superClassScale)
        {
            List<Scale> superClass = superClassScale.CalculateScaleClass();

            if (superClass.Any(isSubScaleTo))
                return true;
            return false;
        }

        public bool isSubScaleTo(Scale superScale)
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
    }
}