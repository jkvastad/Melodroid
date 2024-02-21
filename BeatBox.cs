using Fractions;
using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;
using Serilog;
using static MusicTheory.MusicTheoryUtils;

public class BeatBox
{
    //Key A is compatible with denominator D if key A has a fraction approximaton whose denominator divides D.
    Dictionary<int, HashSet<(int key, Fraction approximation)>> _allKeysCompatibleWithDenominator = new();
    Dictionary<int, List<Fraction>> _tet12FractionApproximations;
    Random _random = new Random();
    public BeatBox(int maxFactors = 4, int maxPatternLength = 50)
    {

        _allKeysCompatibleWithDenominator = CalculateKeysCompatibleWithDenominators(maxFactors, maxPatternLength);

        Console.WriteLine($"Denominators of 12TET fraction approximations for pattern length {maxPatternLength} and primes {string.Join(",", standardPrimes)} with compatible keys:");
        foreach (var entry in new SortedDictionary<int, HashSet<(int key, Fraction approximation)>>(_allKeysCompatibleWithDenominator))
        {
            Console.Write($"Denominator: {entry.Key} - Keys: ");
            foreach (var keyAndApproximation in entry.Value)
            {
                //Console.Write($"{keyAndApproximation.key} ({keyAndApproximation.approximation.Numerator}/{keyAndApproximation.approximation.Denominator}), ");
                Console.Write($"{keyAndApproximation.key}, ");
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Denominators of 12TET fraction approximations for pattern length {maxPatternLength} and primes {string.Join(",", standardPrimes)} with compatible keys:");
        foreach (var entry in new SortedDictionary<int, HashSet<(int key, Fraction approximation)>>(_allKeysCompatibleWithDenominator))
        {
            Console.Write($"Denominator: {entry.Key} - Keys: ");
            foreach (var keyAndApproximation in entry.Value)
            {
                Console.Write($"{keyAndApproximation.key} ({keyAndApproximation.approximation.Numerator}/{keyAndApproximation.approximation.Denominator}), ");
                //Console.Write($"{keyAndApproximation.key}, ");
            }
            Console.WriteLine();
        }
    }

    public Phrase TestPhrase()
    {
        List<Measure> measures = new List<Measure>();

        int measureTimeDivision = 8;
        int measuresInPhrase = 4;
        int firstKey = _random.Next(12);
        int firstDenominator = (int)_tet12FractionApproximations[firstKey].TakeRandom().Denominator;
        for (int measureIndex = 0; measureIndex < measuresInPhrase; measureIndex++)
        {
            int?[] velocities = GetRhythm(measureTimeDivision);

            NoteValue?[] noteValues = GetHarmony(velocities, firstDenominator);
            measures.Add(new(noteValues));
        }

        return new Phrase(measures);
    }

    private int?[] GetRhythm(int measureTimeDivision)
    {
        int?[] velocities = new int?[measureTimeDivision];
        for (int i = 0; i < velocities.Length; i++)
        {
            //TODO generate better rhythms, perhaps things which the brain can easily chunk (based on cerebullum and rhythm - e.g. learning 2 + 3 count)
            if (_random.Next(measureTimeDivision) > measureTimeDivision / 4)
            {
                if (_random.Next(6) > 0)
                    velocities[i] = _random.Next(64, 97);
                else
                    velocities[i] = 0;
            }
        }
        return velocities;
    }

    private NoteValue?[] GetHarmony(int?[] velocities, int firstDenominator)
    {
        NoteValue?[] noteValues = new NoteValue?[velocities.Length];
        NoteValue fundamentalNote = new NoteValue(NoteName.C, 4, 64);
        Log.Information($"denominators dividing {firstDenominator}");
        for (int i = 0; i < velocities.Length; i++)
        {
            if (velocities[i].HasValue)
            {
                noteValues[i] = fundamentalNote + _allKeysCompatibleWithDenominator[firstDenominator].TakeRandom().key;
            }
        }
        return noteValues;
    }

    private NoteValue GetNoteValuePreservingLcm(int lcm, NoteValue fundamentalNote)
    {
        //TODO Still necessary? Cleanup/Deprecated?
        var intervals = MidiIntervalsPreservingLcm(lcm);
        if (intervals.Count == 0)
            throw new ArgumentException($"No interval can preserve lcm {lcm}");
        var interval = intervals[_random.Next(intervals.Count)];
        return fundamentalNote + interval;
    }
}