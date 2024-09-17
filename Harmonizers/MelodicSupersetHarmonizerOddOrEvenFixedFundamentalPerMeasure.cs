using Fractions;
using MusicTheory;
using System.Linq;
using static MusicTheory.MusicTheoryUtils;

namespace Melodroid.Harmonizers
{
    public class MelodicSupersetHarmonizerOddOrEvenFixedFundamentalPerMeasure(int[] chord) : IMeasureHarmonizer
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
                List<List<long>> properMelodicSuperset = MelodicSupersetHarmonizer.GetProperMelodicSuperset(melodicSuperset, _currentChord);

                //Sort keys by odd or even (15 vs 24) only
                //TODO seems mostly superflous, but e.g. 0 4 7 with fundamental 7 has mixed even/odd.                
                Dictionary<int, List<List<int>>> properKeysPerFundamental = new();

                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    properKeysPerFundamental[fundamental] = [[], []]; //base 24, base 15                    
                    for (int key = 0; key < 12; key++)
                    {
                        var lcm = properMelodicSuperset[fundamental][key];
                        if (lcm != 0)
                        {
                            if (24 % lcm == 0)
                                properKeysPerFundamental[fundamental][0].Add(key);
                            if (15 % lcm == 0)
                                properKeysPerFundamental[fundamental][1].Add(key);
                        }
                    }
                    properKeysPerFundamental[fundamental] = properKeysPerFundamental[fundamental].Distinct().ToList();
                    properKeysPerFundamental[fundamental] = properKeysPerFundamental[fundamental].Distinct().ToList();
                }


                //Play random notes from proper melodic superset                
                int previousNoteNumber = 0;
                //select a fundamental having proper keys and stick with it through the measure
                //TODO crashes on e.g. 0 4 8 as no real bases are present - how to play melody to chords without real bases?
                int melodyRoot = properKeysPerFundamental.Keys
                    .Where(fundamental => properKeysPerFundamental[fundamental]
                    .Any(oddOrEven => oddOrEven.Count > 0)).TakeRandom();
                int isEven = properKeysPerFundamental[melodyRoot].Select((keys, index) => //only use polarity with actual corresponding keys
                {
                    if (keys.Count > 0)
                        return index;
                    else
                        return -1;
                }).Where(index => index >= 0).TakeRandom();

                for (int i = 0; i < velocityMeasure.Length; i++)
                {
                    if (velocityMeasure[i] == null)
                        continue;

                    if (measureNoteValues[i] == null) //might have been created earlier for i = 0 when closing old notes
                        measureNoteValues[i] = new();

                    //Play random note from proper melodic superset                    
                    int noteNameNumber = properKeysPerFundamental[melodyRoot][isEven].TakeRandom(); //C is 0
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
                _currentChord = MelodicSupersetHarmonizer.GetProperChordProgression(_currentChord);
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
    }
}
