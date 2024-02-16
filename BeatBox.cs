using Fractions;
using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;
using static MusicTheory.MusicTheoryUtils;

public class BeatBox
{
    //Key A is compatible with denominator D if key A has a fraction approximaton whose denominator divides D.
    Dictionary<int, HashSet<int>> _allKeysCompatibleWithDenominator = new();
    Dictionary<int, List<Fraction>> _tet12FractionApproximations;
    public BeatBox()
    {
        _tet12FractionApproximations = Calculate12TetFractionApproximations(standardPrimes);

        //init _allKeys
        foreach (var entry in _tet12FractionApproximations)
        {
            foreach (var fraction in entry.Value)
            {
                int denominator = (int)fraction.Denominator;
                if (!_allKeysCompatibleWithDenominator.ContainsKey(denominator))
                    _allKeysCompatibleWithDenominator[denominator] = new();
            }
        }
        //populate _allKeys
        foreach (var approximations in _tet12FractionApproximations)
        {
            foreach (var fraction in approximations.Value)
            {
                foreach (var denominator in _allKeysCompatibleWithDenominator.Keys)
                {
                    if (denominator % fraction.Denominator == 0)
                        _allKeysCompatibleWithDenominator[denominator].Add(approximations.Key);
                }
            }
        }

        foreach (var entry in new SortedDictionary<int, HashSet<int>>(_allKeysCompatibleWithDenominator))
        {
            Console.Write($"Denominator: {entry.Key} - Keys: ");
            foreach (var tet12Key in entry.Value)
            {
                Console.Write($"{tet12Key}, ");
            }
            Console.WriteLine();
        }
    }

    public Phrase TestPhrase()
    {
        List<Measure> measures = new List<Measure>();

        int measureTimeDivision = 8;
        int measuresInPhrase = 4;
        for (int measureIndex = 0; measureIndex < measuresInPhrase; measureIndex++)
        {
            int?[] velocities = GetRhythm(measureTimeDivision);

            NoteValue?[] noteValues = GetHarmony(velocities);
            measures.Add(new(noteValues));
        }

        return new Phrase(measures);
    }

    private int?[] GetRhythm(int measureTimeDivision)
    {
        Random random = new Random();
        int?[] velocities = new int?[measureTimeDivision];
        for (int i = 0; i < velocities.Length; i++)
        {
            //TODO generate better rhythms, perhaps things which the brain can easily chunk (based on cerebullum and rhythm - e.g. learning 2 + 3 count)
            if (random.Next(measureTimeDivision) > measureTimeDivision / 4)
            {
                if (random.Next(6) > 0)
                    velocities[i] = random.Next(64, 97);
                else
                    velocities[i] = 0;
            }
        }
        return velocities;
    }

    private NoteValue?[] GetHarmony(int?[] velocities)
    {
        NoteValue?[] noteValues = new NoteValue?[velocities.Length];
        for (int i = 0; i < velocities.Length; i++)
        {
            if (velocities[i].HasValue)
            {
                noteValues[i] = GetNoteValuePreservingLcm(15, new NoteValue(NoteName.A, 4, velocities[i]!.Value));
            }
        }
        return noteValues;
    }

    private NoteValue GetNoteValuePreservingLcm(int lcm, NoteValue fundamentalNote)
    {
        Random random = new();
        //TODO print which fractions are selected for which intervals
        var intervals = MidiIntervalsPreservingLcm(lcm);
        if (intervals.Count == 0)
            throw new ArgumentException($"No interval can preserve lcm {lcm}");
        var interval = intervals[random.Next(intervals.Count)];
        return fundamentalNote + interval;
    }
}