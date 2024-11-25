
using MusicTheory;

public class PrincipleBasedMeasureMaker
{
    public List<Measure> MakeMeasures(int numberOfMeasures = 4, int measureTimeDivision = 12)
    {
        List<Measure> measures = new();
        List<int> startingIntervals = new() { 0, 4, 7 };

        for (int i = 0; i < numberOfMeasures; i++)
        {
            Dictionary<int, int>?[] noteValues = new Dictionary<int, int>?[measureTimeDivision];
            for (int j = 0; j < measureTimeDivision; j++)
            {
                if (decideNote())
                {
                    //select which note
                    //calculate tonal coverage: a fundamental with bases sharing a prime factor which contain all notes of the tonal set.
                }
                
            }
            //close old notes
        }

        return measures;
    }

    private bool decideNote()
    {
        return true;
    }
}

