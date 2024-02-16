using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System.Text;

using Fractions;
using Serilog;
using MoreLinq;

namespace MusicTheory
{
    public class Phrase
    {
        public List<Measure> Measures { get; }
        public Phrase(List<Measure> measures)
        {
            Measures = measures;
        }
    }

    public class Measure
    {
        public int TimeDivision { get; }
        public NoteValue?[] NoteValues { get; }

        public Measure(NoteValue?[] noteValues)
        {
            NoteValues = noteValues;
            TimeDivision = noteValues.Length;
        }

        int stringPadding = 3; //max value 127

        public string NoteVelocitiesString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NoteValues.Length; i++)
            {
                int? noteVelocity = NoteValues[i]?.Velocity;
                if (noteVelocity == null)
                {
                    sb.Append("-".PadRight(stringPadding));
                    continue;
                }
                else
                {
                    sb.Append($"{noteVelocity}".PadRight(stringPadding));
                }
            }
            return sb.ToString();
        }

        public string NoteValuesString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NoteValues.Length; i++)
            {
                NoteValue? noteValue = NoteValues[i];
                if (noteValue is NoteValue valueOfNote)
                {
                    sb.Append($"{valueOfNote.Name}{valueOfNote.Octave}".PadRight(stringPadding));
                }
                else
                {
                    sb.Append("-".PadRight(stringPadding));

                    continue;
                }
            }
            return sb.ToString();
        }
    }

    public readonly struct NoteValue
    {
        public NoteName Name { get; }
        public int Octave { get; }
        public int Velocity { get; }

        public static NoteValue SilentNote = new NoteValue(NoteName.A, 4, 0);
        public NoteValue(NoteName name, int octave, int velocity)
        {
            Name = name;
            Octave = octave;
            Velocity = velocity;
        }

        public static NoteValue operator +(NoteValue a, int midiInterval)
        {
            SevenBitNumber noteNumber = (SevenBitNumber)(NoteUtilities.GetNoteNumber(a.Name, a.Octave) + midiInterval);
            return new(NoteUtilities.GetNoteName(noteNumber), NoteUtilities.GetNoteOctave(noteNumber), a.Velocity);
        }

        public static bool operator ==(NoteValue a, NoteValue b)
        {
            if (a.Name == b.Name && a.Octave == b.Octave && a.Velocity == b.Velocity)
                return true;
            return false;
        }

        public static bool operator !=(NoteValue a, NoteValue b) => !(a == b);
    }

    public class NoteBuilder
    {
        List<Measure> _measures { get; }
        public short MidiTimeDivision { get; }
        public List<Melanchall.DryWetMidi.Interaction.Note> Notes { get; } = new();

        public NoteBuilder(List<Measure> measures)
        {
            _measures = measures;
            MidiTimeDivision = ParseNotes();
        }

        short ParseNotes()
        {
            short songTimeDivision = (short)MusicTheoryUtils.LCM(_measures.Select((measure, index) => (long)measure.TimeDivision).ToArray());
            int totalMidiTicks = 0;
            NoteValue currentNoteValue = NoteValue.SilentNote;
            int currentNoteStart = totalMidiTicks;

            foreach (var measure in _measures)
            {
                foreach (NoteValue? noteValue in measure.NoteValues)
                {
                    if (noteValue != null)
                    {
                        if (currentNoteValue.Velocity > 0)
                            AddCurrentNote();
                        //Begin building next note
                        currentNoteValue = noteValue.Value;
                        currentNoteStart = totalMidiTicks;
                    }
                    //midi time division is per quarter note rather than per measure, thus 4 times higher resolution
                    int midiTicks = 4 * (songTimeDivision / measure.TimeDivision);
                    totalMidiTicks += midiTicks;
                }
            }
            //Add final note
            AddCurrentNote();
            //Pad with one division of silence, otherwise drywetmidi clips the last midi tick.
            if (currentNoteValue.Velocity > 0)
                Notes.Add(new(NoteName.A, 4)
                {
                    Time = totalMidiTicks,
                    Length = songTimeDivision,
                    Velocity = (SevenBitNumber)0
                });

            return songTimeDivision;

            void AddCurrentNote()
            {
                Notes.Add(new(currentNoteValue.Name, currentNoteValue.Octave)
                {
                    Time = currentNoteStart,
                    Length = totalMidiTicks - currentNoteStart,
                    Velocity = (SevenBitNumber)currentNoteValue.Velocity
                });
            }
        }
    }

    public class MusicTheoryUtils
    {
        /* Rational tuning (see fig 6 https://arxiv.org/pdf/1306.6458.pdf):
         * Choosing values where both denominator and numerator consist of small (max value 7) primes. Noting if deviation from 12TET is substantially above 1%
         * TODO Double check procedure for generating the figure
         * 0 - 1
         * 1 - 16/15 or 15/14
         * 2 - 9/8 or 10/9 or 8/7(1.9% out of tune)
         * 3 - 6/5
         * 4 - 5/4
         * 5 - 4/3
         * 6 - 7/5 or 10/7
         * 7 - 3/2
         * 8 - 8/5 or 25/16(1.6%)
         * 9 - 5/3
         * 10 - 25/14 or 16/9 or 9/5 or 7/4(1.8%)
         * 11 - 15/8 
         * 12 - 2
         */

        public static readonly double[] TET12 = [
            1,
            1.0594630943592953,
            1.122462048309373,
            1.189207115002721,
            1.2599210498948732,
            1.3348398541700344,
            1.4142135623730951,
            1.4983070768766815,
            1.5874010519681994,
            1.681792830507429,
            1.7817974362806785,
            1.887748625363387];

        public static void PrintTet12FractionApproximations(int maxFactors = 4, int maxPatternLength = 50)
        {
            List<int> primes = new() { 2, 2, 2, 2, 3, 3, 3, 5, 5, 7, 7 };
            Dictionary<int, List<Fraction>> keyApproximations = Caluclate12TetFractionApproximations(primes, maxFactors, maxPatternLength);

            //print bins
            foreach (var entry in new SortedDictionary<int, List<Fraction>>(keyApproximations))
            {
                Console.Write($"{entry.Key}: ");
                Console.WriteLine();
                foreach (var fraction in entry.Value)
                {
                    Console.Write($"{fraction.Numerator}/{fraction.Denominator}");
                    Console.Write(" - ");
                    Console.Write($"[{string.Join(",", Factorize((int)fraction.Numerator))}]/[{string.Join(",", Factorize((int)fraction.Denominator))}] - ");
                    Console.WriteLine($"({(Math.Abs(((double)fraction - TET12[entry.Key]) / TET12[entry.Key]) * 100).ToString("0.0")})");
                }
            }
        }

        private static Dictionary<int, List<Fraction>> Caluclate12TetFractionApproximations(List<int> primes, int maxFactors = 4, int maxPatternLength = 50)
        {
            //Get all interesting numbers - combos of up to maxFactors primes
            Dictionary<int, IList<int>> lcmFacotorisations = LcmFactorisationsForCombinationsOfPrimes(primes, maxFactors);

            //Keep only fractions < maxPatternLength, since this decides pattern length.
            //I conjecture that max pattern length arises from mechanisms similar to how a beat turns into a sound around 20hz.
            lcmFacotorisations = lcmFacotorisations.Where(entry => entry.Key < maxPatternLength).ToDictionary();

            //Create all fractions from possible lcm combinations
            var fractions = FractionsFromIntegers(lcmFacotorisations.Keys.ToList()).Order();

            //Filter out fractions outside octave -
            //Since proximity is calculated as percentage, approximations are as close to a 12Tet tone in one octave as their octave transposed counterpart in another
            List<Fraction> fractionsInsideOctave = new();
            foreach (var fraction in fractions)
            {
                if (1 < fraction && fraction < 2)
                    fractionsInsideOctave.Add(fraction);
            }

            //Bin fractions to closest 12 tet key
            Dictionary<int, List<Fraction>> keyApproximations = new();
            for (int i = 0; i < 12; i++)
            {
                keyApproximations[i] = new();
            }
            foreach (var fraction in fractionsInsideOctave)
            {
                double maxDiff = 1;
                int bestKey = 0;
                for (int key = 0; key < TET12.Length; key++)
                {
                    var keyDiff = Math.Abs(((double)fraction - TET12[key]) / TET12[key]); //diff in percentage of 12tet key
                    if (keyDiff < maxDiff)
                    {
                        maxDiff = keyDiff;
                        bestKey = key;
                    }
                }
                keyApproximations[bestKey].Add(fraction);
            }

            return keyApproximations;
        }

        public static List<int> MidiIntervalsPreservingLcm(int lcm, int maximumPeriodicity = 16)
        {
            List<int> intervals = new();
            var approximations = RatiosClosestTo12TetKeys(maximumPeriodicity);
            Log.Information($"intervals preserving lcm {lcm} using fraction(s)");
            for (int interval = 0; interval < approximations.Count; interval++)
            {
                if (approximations[interval].Any(fraction => lcm % fraction.Denominator == 0))
                {
                    //TODO write to log instead of console                    
                    StringBuilder sb = new();
                    sb.Append(interval.ToString() + ": ");
                    foreach (Fraction fraction in approximations[interval])
                    {
                        if (lcm % fraction.Denominator == 0)
                        {
                            sb.Append(fraction.ToString() + ", ");
                        }
                    }
                    Log.Information(sb.ToString());
                    intervals.Add(interval);
                }
            }
            return intervals;
        }
        public static void PrintRelativePeriodicityForOctaveIntervals(int maximumPeriodicity = 16)
        {
            List<HashSet<Fraction>> approximations = RatiosClosestTo12TetKeys(maximumPeriodicity);
            Console.WriteLine("Midi note diff: closest frequency ratio");

            for (int i = 0; i < approximations.Count; i++)
            {
                Log.Information($"{i}: " + string.Join(", ", approximations[i]));
            }
        }

        private static List<HashSet<Fraction>> RatiosClosestTo12TetKeys(int maximumPeriodicity)
        {
            double[] tet12 = CalculateTet12Values();

            List<HashSet<Fraction>> approximations = new();
            for (int i = 0; i < 12; i++)
            {
                approximations.Add(new());
            }

            for (int i = 1; i < maximumPeriodicity + 1; i++)
            {
                for (int j = i + 1; j < i * 2; j++)
                {
                    double ratio = j / (double)i;
                    double bestDiff = 1;
                    int best12TetKey = 0;
                    for (int k = 0; k < tet12.Length; k++)
                    {
                        var diff = Math.Abs(ratio - tet12[k]);
                        if (diff < bestDiff)
                        {
                            bestDiff = diff;
                            best12TetKey = k;
                        }
                    }
                    approximations[best12TetKey].Add(new(i, j));
                }
            }

            return approximations;
        }

        public static double[] CalculateTet12Values()
        {
            double[] Tet12 = new double[12];
            for (int i = 0; i < Tet12.Length; i++)
            {
                Tet12[i] = Math.Pow(2, i / (double)12);
            }

            return Tet12;
        }

        //Thanks stack overflow https://stackoverflow.com/questions/147515/least-common-multiple-for-3-or-more-numbers/29717490#29717490
        public static long LCM(long[] numbers)
        {
            long LcmResult = numbers.Aggregate(lcm);
            if (LcmResult > short.MaxValue)
                throw new ArgumentException($"Lcm result {LcmResult} is larger than short.MaxValue: This exceeds maximum midi time division");
            return LcmResult;
        }
        public static long lcm(long a, long b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }
        public static long GCD(long a, long b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }

        /*    
     *    lcm vs numerator prime factors
        2: 2
        3: 3
        4: 2,2
        5: 5
        6: 2,3
        8: 2,2,2
        9: 3,3
        10: 2,5
        12: 2,2,3
        15: 3,5
        16: 2,2,2,2
        18: 2,3,3
        20: 2,2,5
        24: 2,2,2,3
        30: 2,3,5
        36: 2,2,3,3
        40: 2,2,2,5
        45: 3,3,5
        60: 2,2,3,5
        90: 2,3,3,5
     */
        public static Dictionary<int, IList<int>> LcmFactorisationsForCombinationsOfPrimes(List<int> primes, int maxCombinations)
        {
            //print all lcms for all 4-combinations of primes:
            //List<int> primes = new() { 2, 2, 2, 2, 3, 3, 3, 5, 5, 7, 7 };
            //var lcmFacotorisations = LcmFactorisationsForCombinationsOfPrimes(primes, 4);
            //foreach (var lcm in lcmFacotorisations.Keys.Order())
            //{
            //    if (lcm < 100)
            //        Console.WriteLine($"{lcm}: {string.Join(",", lcmFacotorisations[lcm])}");
            //}
            Dictionary<int, IList<int>> lcmSets = new();
            for (int i = 1; i < maxCombinations + 1; i++)
            {
                IEnumerable<IList<int>> sets = primes.Subsets(i);
                foreach (var set in sets)
                {
                    int lcm = set.Aggregate((prev, next) => prev * next);
                    if (!lcmSets.ContainsKey(lcm)) lcmSets[lcm] = set;
                }
            }
            return lcmSets;
        }

        public static List<Fraction> FractionsFromIntegers(List<int> integers)
        {
            HashSet<Fraction> fractions = new();
            foreach (int i in integers)
            {
                foreach (int j in integers)
                {
                    Fraction fraction = new(i, j);
                    if (!fractions.Contains(fraction))
                        fractions.Add(fraction);
                }
            }
            return fractions.ToList();
        }

        public static List<int> Factorize(int integer, int maxLoops = 100)
        {
            List<int> factors = new();
            int factor = 2;
            int loops = 0;
            while (integer != 1)
            {
                if (integer % factor == 0)
                {
                    factors.Add(factor);
                    integer /= factor;
                }
                else
                {
                    factor += 1;
                }
                loops++;
                if (loops > maxLoops) throw new ArgumentException($"Factorization failed - exceeded maxLoops {maxLoops}");
            }
            return factors.Count == 0 ? new() { 1 } : factors;
        }
    }
}