using Fractions;
using MusicTheory;
using System.Linq;
using static MusicTheory.MusicTheoryUtils;

namespace Melodroid.Harmonizers
{
    public class MelodicSupersetHarmonizer(int[] chord) : IMeasureHarmonizer
    {
        private int[] _currentChord = chord;
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

                //Close notes from previous measure
                measureNoteValues[0] = new();
                if (measureIndex > 0)
                {
                    foreach (int noteNumber in measures[^1].MIDIKeys.Where(item => item != null).SelectMany(item => item!.Keys).ToHashSet())
                    {
                        measureNoteValues[0]![noteNumber] = 0;
                    }
                }

                //Generate proper melodic superset for playing melody with chord
                List<List<long>> melodicSuperset = GetMelodicSuperset(_currentChord);
                List<List<long>> properMelodicSuperset = GetProperMelodicSuperset(melodicSuperset, _currentChord);

                List<int> properKeys = new();
                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    for (int key = 0; key < 12; key++)
                    {
                        if (properMelodicSuperset[fundamental][key] != 0)
                            properKeys.Add(key);
                    }
                }
                properKeys = properKeys.Distinct().ToList(); //no need for duplicates

                //Play random notes from proper melodic superset                
                int previousNoteNumber = 0;
                for (int i = 0; i < velocityMeasure.Length; i++)
                {
                    if (velocityMeasure[i] == null)
                        continue;

                    if (measureNoteValues[i] == null) //might have been created earlier for i = 0 when closing old notes
                        measureNoteValues[i] = new();

                    //Play random note from proper melodic superset
                    int noteNameNumber = properKeys.TakeRandom(); //C is 0
                    int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
                    if (previousNoteNumber != noteNumber) //close old note - melodic superset is only made for one extra note at a time
                    {
                        measureNoteValues[i]![previousNoteNumber] = 0;
                    }
                    previousNoteNumber = noteNumber;

                    int velocity = (int)velocityMeasure[i]!; //checked for null on the lines above                
                    measureNoteValues[i]![noteNumber] = velocity; //created the dictionary on the lines above                    
                }
                Measure measure = new(measureNoteValues);
                measures.Add(measure);
                measureIndex++;

                //Save chord for chord harmonizers
                int chordFundamental = _currentChord.Min(); //arbitrary fundamental - chord inversions to match
                Scale chord = new Scale(_currentChord.Select(key => key - chordFundamental).ToArray());
                ChordPerMeasure.Add((chordFundamental, chord));

                //Select new chord
                _currentChord = GetChordProgression(_currentChord);
                //Console.WriteLine(string.Join(" ", _currentChord));
            }
            return measures;
        }
        //Melodic Superset consists of all complete LCM values for each possible tone added to the current chord for each possible fundamental position
        //LCM 0 is returned for keys and fundamentals with no valid LCM (i.e. using key 6 relative to fundamental)
        public static List<List<long>> GetMelodicSuperset(int[] tet12Keys)
        {
            List<List<long>> lcmsPerMelodyPerFundamental = new();

            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                lcmsPerMelodyPerFundamental.Add([]);
                for (int melodyKey = 0; melodyKey < 12; melodyKey++)
                {
                    List<int> renormalizedKeys = tet12Keys.Select(key => (key - fundamental + 12) % 12).ToList();
                    int renormalizedMelodyKey = (melodyKey - fundamental + 12) % 12;
                    renormalizedKeys.Add(renormalizedMelodyKey);

                    if (renormalizedKeys.Any(key => StandardFractions[key] == 0))
                        lcmsPerMelodyPerFundamental[fundamental].Add(0); //0 for invalid key, no 7/5
                    else
                    {
                        lcmsPerMelodyPerFundamental[fundamental].Add(
                            LCM(renormalizedKeys.Select(
                                key => (long)StandardFractions[key].Denominator).ToArray()));
                    }
                }
            }
            return lcmsPerMelodyPerFundamental;
        }
        //A proper melodic superset has additional requirements to a melodic superset:
        // - all LCMs not dividing 15 or 24 set to 0 (invalid)
        // - only real bases
        // - no base 15 collapse (the fundamental for base 15 does not contain the interval 8/5)        
        public static List<List<long>> GetProperMelodicSuperset(List<List<long>> melodicSuperset, int[] chord)
        {
            List<List<long>> properMelodicSuperset = new();
            for (int fundamental = 0; fundamental < melodicSuperset.Count; fundamental++)
            {
                properMelodicSuperset.Add([]);
                List<long> lcmPerKey = melodicSuperset[fundamental];
                for (int key = 0; key < lcmPerKey.Count; key++)
                {
                    long lcm = 0;
                    List<int> chordAndKey = [.. chord, key];
                    if (!chordAndKey.Contains(fundamental))//only real bases 
                    {
                        properMelodicSuperset[fundamental].Add(lcm);
                        continue;
                    }

                    if (lcmPerKey[key] == 0)
                        ;
                    else if (24 % lcmPerKey[key] == 0)
                        lcm = lcmPerKey[key];
                    else if (15 % lcmPerKey[key] == 0)
                    {
                        lcm = lcmPerKey[key];
                        if (chordAndKey.Any(note => (note - fundamental + 12) % 12 == 8)) //check for collapsing base 15 - no 8/5 interval
                            lcm = 0;
                    }
                    properMelodicSuperset[fundamental].Add(lcm);
                }
            }
            return properMelodicSuperset;
        }

        //Gets all LCMs per fundamental in 12 tet for a chord.
        public static List<long> GetChordLCMs(int[] chord)
        {
            List<long> chordLcms = new();
            for (int fundamental = 0; fundamental < 12; fundamental++)
            {
                List<int> renormalizedChord = chord.Select(key => (key - fundamental + 12) % 12).ToList();
                List<Fraction> chordFractions = renormalizedChord.Select(key => StandardFractions[key]).ToList();

                long chordLcm = 0;

                if (chordFractions.Any(fraction => fraction == 0))
                    ; //invalid fraction - no lcm possible
                else
                    chordLcm = LCM(chordFractions.Select(fraction => (long)fraction.Denominator).ToArray());

                chordLcms.Add(chordLcm);
            }
            return chordLcms;
        }

        public static int[] GetChordProgression(int[] originalChord)
        {
            int[] progression = [];
            bool progressionFound = false;
            List<long> originalLcms = GetChordLCMs(originalChord);
            List<int> tet12 = new List<int>();
            for (int i = 0; i < 12; i++)
                tet12.Add(i);

            while (!progressionFound)
            {
                var candidateChord = tet12.TakeRandom(3).Order().ToArray();
                var candidateLcms = GetChordLCMs(candidateChord);
                if (HasSemitoneOrTritone(candidateChord))
                    continue;
                //Check for at least one lcm match at any fundamental
                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    if (originalLcms[fundamental] == 0 || candidateLcms[fundamental] == 0) //avoid modulo 0
                        continue;
                    if ((24 % originalLcms[fundamental] == 0) && (24 % candidateLcms[fundamental] == 0))
                        return candidateChord;
                    if ((15 % originalLcms[fundamental] == 0) && (15 % candidateLcms[fundamental] == 0))
                        return candidateChord;
                }
            }
            return progression;
        }

        private static bool HasSemitoneOrTritone(int[] candidateChord)
        {
            for (int i = 0; i < candidateChord.Length; i++)
            {
                var keyDiff = (candidateChord[(i + 1) % candidateChord.Length] - candidateChord[i] + 12) % 12;
                //Check for no semitones and no tritones
                if (keyDiff == 1 || keyDiff == 6) return true;
            }
            return false;
        }
    }
}
