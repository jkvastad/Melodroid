using Fractions;
using MusicTheory;
using static MusicTheory.MusicTheoryUtils;

namespace Melodroid.Harmonizers
{
    public class MelodicSupersetHarmonizer(Scale chord) : IMeasureHarmonizer
    {
        private Scale _currentChord = chord;
        public int CurrentFundamental = 0;
        public int CurrentOctave = 4;
        private Random random = new();

        public List<(int fundamentalNoteNumber, Scale scale)> ChordPerMeasure = new();
        public List<Measure> MeasuresFromVelocities(List<int?[]> velocityMeasures)
        {
            ChordPerMeasure.Clear();

            List<Measure> measures = new();
            int measureIndex = 0;

            foreach (var velocityMeasure in velocityMeasures)
            {
                Dictionary<int, int>?[] measureNoteValues = new Dictionary<int, int>?[velocityMeasure.Length];

                List<List<long>> melodicSuperset = GetMelodicSuperset(_currentChord.ToIntervals().Select(integer => (long)integer).ToArray());

                HashSet<int> currentIntervals = _currentChord.ToIntervals().ToHashSet();

                //Close notes from previous measure
                measureNoteValues[0] = new();
                if (measureIndex > 0)
                {
                    foreach (int noteNumber in measures[^1].MIDIKeys.Where(item => item != null).SelectMany(item => item!.Keys).ToHashSet())
                    {
                        measureNoteValues[0]![noteNumber] = 0;
                    }
                }

                //Play random notes from current allowed intervals                
                for (int i = 0; i < velocityMeasure.Length; i++)
                {
                    if (velocityMeasure[i] == null)
                        continue;

                    if (measureNoteValues[i] == null) //might be created earlier for i = 0 when closing old notes
                        measureNoteValues[i] = new();

                    //play random interval from current intervals and fundamental 
                    int noteNameNumber = (CurrentFundamental + currentIntervals.TakeRandom()) % 12; //C is 0
                    int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
                    int velocity = (int)velocityMeasure[i]!; //checked for null on the lines above                
                    measureNoteValues[i]![noteNumber] = velocity; //created the dictionary on the lines above                    
                }
                Measure measure = new(measureNoteValues);
                measures.Add(measure);
                measureIndex++;

                ChordPerMeasure.Add((CurrentFundamental, _currentChord)); //used by chord harmonizers

                CurrentFundamental = random.Next(12);
            }
            return measures;
        }

        //Melodic Superset consists of all complete LCM values for each possible added tone for each possible fundamental position
        //LCM 0 is returned for keys and fundamentals with no valid LCM (i.e. using key 6 relative to fundamental)
        public static List<List<long>> GetMelodicSuperset(long[] tet12Keys)
        {
            Fraction[] standardFractions = [new(1), new(16, 15), new(9, 8), new(6, 5), new(5, 4), new(4, 3), new(0), new(3, 2), new(8, 5), new(5, 3), new(9, 5), new(15, 8)];
            List<List<long>> lcmsPerMelodyPerFundamental = new();

            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                lcmsPerMelodyPerFundamental.Add([]);
                for (int melodyKey = 0; melodyKey < 12; melodyKey++)
                {
                    List<long> renormalizedKeys = tet12Keys.Select(key => (key - fundamental + 12) % 12).ToList();
                    long renormalizedMelodyKey = (melodyKey - fundamental + 12) % 12;
                    renormalizedKeys.Add(renormalizedMelodyKey);

                    if (renormalizedKeys.Any(key => standardFractions[key] == 0))
                        lcmsPerMelodyPerFundamental[fundamental].Add(0); //0 for invalid key, no 7/5
                    else
                    {
                        lcmsPerMelodyPerFundamental[fundamental].Add(
                            LCM(renormalizedKeys.Select(
                                key => (long)standardFractions[key].Denominator).ToArray()));
                    }
                }
            }
            return lcmsPerMelodyPerFundamental;
        }
        //A pruned melodic superset has all LCMs not dividing 15 or 24 set to 0, as well as base 15 
        //public static List<List<long>> GetPrunedMelodicSuperset(List<List<long>> melodicSuperset, long[] chord)
        //{
        //    List<List<long>> prunedMelodicSuperset = new();
        //    for (int fundamental = 0; fundamental < melodicSuperset.Count; fundamental++)
        //    {                
        //        prunedMelodicSuperset.Add([]);
        //        List<long> lcmsPerKey = melodicSuperset[fundamental];
        //        for (int key = 0; key < lcmsPerKey.Count; key++)
        //        {
        //            long lcm = 0;
        //            if (24 % lcmsPerKey[key] == 0)
        //                lcm = lcmsPerKey[key];
        //            else if (15 % lcmsPerKey[key] == 0)
        //            {
        //                if (!) //check for collapsing base 15
        //                    lcm = lcmsPerKey[key];
        //            }
        //            prunedMelodicSuperset[fundamental].Add(lcm);
        //        }
        //    }
        //}
    }
}
