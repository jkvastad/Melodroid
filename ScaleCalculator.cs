using Fractions;
using System.Numerics;
using static MusicTheory.MusicTheoryUtils;

namespace MusicTheory
{
    public class ScaleCalculator
    {
        public Dictionary<Scale, List<Scale>> RotationClassForScale = new();
        public Dictionary<int, List<List<Scale>>> RotationClassesOfLength = new();

        public void CalculateAllRotationClasses()
        {
            List<Scale> allScales = new();
            foreach (Scale scale in allScales)
            {
                if (RotationClassForScale.ContainsKey(scale))
                    continue;
            }
        }

        //Binary Counter method - the binary representation of a number is the scale.
        //  E.g. major chord is 2^0 + 2^4 + 2^7 = 145 = 10010001
        public void CalculateAllScales(int length)
        {
            List<int[]> scales = new();

            int combinations = (int)BigInteger.Pow(2, length);

            for (int i = 0; i < combinations; i++)
            {
                int[] scale = new int[length];
                for (int j = 0; j < length; j++)
                {
                    scale[j] = (i >> j) & 1;
                }
                scales[i] = scale;
            }
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

    public struct Scale
    {
        public Bit12Int binaryScale;
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
                new(9, 5),
                new(15, 8)
            ];

        public Scale(string scaleAsBinary)
        {
            if (scaleAsBinary.Length < 12)
                scaleAsBinary = scaleAsBinary.PadLeft(12, '0');

            if (scaleAsBinary.Length != 12) throw new ArgumentException("12TET Scale must consist of exactly 12 keys");
            if (scaleAsBinary.Any(key => key != '0' && key != '1')) throw new ArgumentException("12TET Scale keys must be either 1 (included in scale) or 0 (excluded from scale)");

            binaryScale = (Bit12Int)Convert.ToInt32(scaleAsBinary, 2);
        }

        public Scale(Bit12Int scale)
        {
            binaryScale = scale;
        }

        public Scale Rotated()
        {
            return new Scale(binaryScale.RotateLeft(1));
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
                if ((binaryScale.RotateRight(i).GetValue() & 1) == 1)
                    denominators.Add((long)keyFractionApproximations[i].Denominator);
            }
            return (int)LCM(denominators.ToArray());
        }

        public static bool operator ==(Scale left, Scale right)
        {

            return left.binaryScale == right.binaryScale;
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
            return binaryScale == other.binaryScale;
        }

        public override int GetHashCode()
        {
            return (int)binaryScale;
        }
    }
}