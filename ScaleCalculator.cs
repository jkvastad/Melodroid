using Fractions;
using Melanchall.DryWetMidi.MusicTheory;
using System.Numerics;
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
            foreach (var rotationClass in ScaleClasses)
            {
                int length = rotationClass[0].KeySet.NumberOfKeys();

                if (!ScaleClassesOfLength.ContainsKey(length))
                    ScaleClassesOfLength[length] = new();

                ScaleClassesOfLength[length].Add(rotationClass);
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
                    keySet = keySet.RotateBinaryLeft();

                    if ((keySet.binaryRepresentation & 1) == 0) continue; //only keep scales (key sets with fundamentals - i.e. first bit set)

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
                foreach (var superClass in ScaleClassesOfLength[length])
                {
                    if (scale.isSubscale(superClass[0]))
                        superClasses.Add(superClass);
                }
            }
            return superClasses;
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
                if ((scale.KeySet.binaryRepresentation.RotateRight(i) & 1) == 1)
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

        public Bit12Int RotateLeft(int positions)
        {
            positions %= _bitSize;
            return new((_value << positions | _value >> (_bitSize - positions)) & _maxValue);
        }

        public Bit12Int RotateRight(int positions)
        {
            positions %= _bitSize;
            return new((_value >> positions | _value << (_bitSize - positions)) & _maxValue);
        }

        public int GetValue()
        {
            return _value;
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
        public Bit12Int binaryRepresentation;

        public Tet12KeySet(string keySetAsBinaryString)
        {
            if (keySetAsBinaryString.Length < 12)
                keySetAsBinaryString = keySetAsBinaryString.PadLeft(12, '0');

            if (keySetAsBinaryString.Length != 12) throw new ArgumentException("12TET key set must consist of exactly 12 keys");
            if (keySetAsBinaryString.Any(key => key != '0' && key != '1')) throw new ArgumentException("12TET key set keys must be either 1 (included in scale) or 0 (excluded from scale)");

            binaryRepresentation = (Bit12Int)Convert.ToInt32(keySetAsBinaryString, 2);
        }

        public Tet12KeySet(Bit12Int keySet)
        {
            binaryRepresentation = keySet;
        }

        public Tet12KeySet(int keySetAsInt)
        {
            binaryRepresentation = new(keySetAsInt);
        }

        public Tet12KeySet RotateBinaryLeft()
        {
            return new Tet12KeySet(binaryRepresentation.RotateLeft(1));
        }

        public int NumberOfKeys()
        {
            int keys = 0;
            int value = (int)binaryRepresentation;
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

            return left.binaryRepresentation == right.binaryRepresentation;
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
            return binaryRepresentation == other.binaryRepresentation;
        }

        public override int GetHashCode()
        {
            return (int)binaryRepresentation;
        }

        public override string ToString()
        {
            return binaryRepresentation.ToString();
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
            if ((keySet.binaryRepresentation & 1) != 1) throw new ArgumentException($"A scale must have a fundamental - {nameof(Tet12KeySet)} did not set first bit ");
            KeySet = keySet;
        }

        public Scale(string keySetAsBinaryString)
        {
            KeySet = new Tet12KeySet(keySetAsBinaryString);
        }

        public Scale(int[] tet12Keys)
        {
            int keysAsBinary = 0;
            foreach (var key in tet12Keys)
            {
                keysAsBinary += 1 << key;
            }

            KeySet = new Tet12KeySet(keysAsBinary);
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
                if ((KeySet.binaryRepresentation.RotateRight(i).GetValue() & 1) == 1)
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
            return KeySet.ToString();
        }

        public bool isSubscale(Scale superScale)
        {
            Bit12Int superBinaryScale = superScale.KeySet.binaryRepresentation;
            for (int i = 0; i < 12; i++)
            {
                if ((superBinaryScale & KeySet.binaryRepresentation) == KeySet.binaryRepresentation)
                    return true;
                superBinaryScale = superBinaryScale.RotateLeft(1);
            }
            return false;
        }
    }
}