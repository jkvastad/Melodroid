using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;

public class BeatBox
{
    public BeatBox()
    {
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
            //arbitrarily play a note with 1/4 probability
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
                noteValues[i] = new NoteValue(NoteName.A, 4, velocities[i]!.Value); //TODO pick harmonies from sets
            }
        }
        return noteValues;
    }
}
