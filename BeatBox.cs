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
                //TODO pick harmonies from sets, based on relative periodicity as in Harmony Perception by Periodicity Detection (e.g. major triad has 4)
                noteValues[i] = new NoteValue(NoteName.A, 4, velocities[i]!.Value);
            }
        }
        return noteValues;
    }
    //Midi intervals based on relative periodicity:    
    // 1/1 : +0
    // 3/2 : +7
    // 4/3 : +5, 5/3 : +9
    // 5/4 : +4, 7/4 : +10
    // 6/5 : +3, 7/5 : + 6, 8/5 : +8, 9/5 : + 10
    // 7/6 : 
}
