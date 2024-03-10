namespace MusicTheory
{
    public class ScaleCalculator
    {
        public Dictionary<Scale, List<Scale>> RotationClass = new();
    }

    public struct Scale
    {
        public int[] Tet12Keys;

        public Scale(int[] tet12Keys)
        {
            if (tet12Keys.Length != 12) throw new ArgumentException("12TET Scale must consist of exactly 12 keys");
            if (tet12Keys.Any(key => key < 0 || key > 1)) throw new ArgumentException("12TET Scale keys must be either 1 (included in scale) or 0 (excluded from scale)");

            Tet12Keys = tet12Keys;
        }

        public Scale Rotated()
        {
            var scale = new int[Tet12Keys.Length];
            for (int i = 0; i < scale.Length; i++)
            {
                scale[i] = Tet12Keys[(i + 1) % scale.Length];
            }
            return new Scale(scale);
        }

        public static bool operator ==(Scale left, Scale right)
        {
            if (left.Tet12Keys.Length != right.Tet12Keys.Length) return false;
            for (int i = 0; i < left.Tet12Keys.Length; i++)
            {
                if (left.Tet12Keys[i] != right.Tet12Keys[i]) return false;
            }
            return true;
        }

        public static bool operator !=(Scale left, Scale right)
        {
            return !(left == right);
        }
    }
}

