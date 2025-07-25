﻿using Fractions;
using MusicTheory;
using System.Linq;
using static MusicTheory.MusicTheoryUtils;

namespace Melodroid.Harmonizers
{
    public class MelodicSupersetHarmonizerOddOrEvenBasePerMeasure(int[] chord) : IMeasureHarmonizer
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
                Dictionary<int, List<int>> properKeysPerFundamental = new();
                properKeysPerFundamental[0] = new(); //base 24
                properKeysPerFundamental[1] = new(); //base 15
                for (int fundamental = 0; fundamental < 12; fundamental++)
                {
                    for (int key = 0; key < 12; key++)
                    {
                        var lcm = properMelodicSuperset[fundamental][key];
                        if (lcm != 0)
                        {
                            if (24 % lcm == 0)
                                properKeysPerFundamental[0].Add(key);
                            else
                                properKeysPerFundamental[1].Add(key);
                        }
                    }
                }
                properKeysPerFundamental[0] = properKeysPerFundamental[0].Distinct().ToList();
                properKeysPerFundamental[1] = properKeysPerFundamental[1].Distinct().ToList();


                //Play random notes from proper melodic superset
                //TODO: Needs to respect rhythmic chunking, to many changes within a chunk sounds chaotic.
                int previousNoteNumber = 0;
                int melodyRoot = properKeysPerFundamental.Keys.Where(key => properKeysPerFundamental[key].Count() > 0).TakeRandom(); //select a fundamental and stick with it through the measure
                for (int i = 0; i < velocityMeasure.Length; i++)
                {
                    if (velocityMeasure[i] == null)
                        continue;

                    if (measureNoteValues[i] == null) //might have been created earlier for i = 0 when closing old notes
                        measureNoteValues[i] = new();

                    //Play random note from proper melodic superset                    
                    int noteNameNumber = properKeysPerFundamental[melodyRoot].TakeRandom(); //C is 0
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
                _currentChord = MelodicSupersetHarmonizer.GetChordProgression(_currentChord);
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
