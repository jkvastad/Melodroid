﻿using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;
using Scale = MusicTheory.Scale;

public class ScaleClassRotationHarmonizer(Scale initialChord) : IMeasureHarmonizer
{
    public Scale CurrentChord = initialChord;
    public int CurrentFundamental = 0;
    public int CurrentOctave = 4;

    public List<(int fundamentalNoteNumber, Scale scale)> ChordPerMeasure = new();

    //TODO use relative scale class rotations as well - e.g. for 0 4 7 there is 4 8 11, i.e. 0 4 7 from 0 3 8.
    //TODO create class for transpose motion (e.g. 0 4 7 has transpose 0 5 8). Also use relative transpose similar to relative scale class rotations.
    public List<Measure> MeasuresFromVelocities(List<int?[]> velocityMeasures)
    {
        ChordPerMeasure.Clear();

        List<Measure> measures = new();
        int measureIndex = 0;
        //get all scale translations, same as the set of all interval values for the scale class
        HashSet<int> scaleTranslations = CurrentChord.CalculateScaleClass().SelectMany(scale => scale.ToIntervals()).ToHashSet();
        scaleTranslations.Remove(0); // no self movement - boring

        HashSet<int> currentIntervals = CurrentChord.ToIntervals();

        foreach (var velocityMeasure in velocityMeasures)
        {
            ChordPerMeasure.Add((CurrentFundamental, CurrentChord)); //used by chord harmonizers

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

            for (int i = 0; i < velocityMeasure.Length; i++)
            {
                if (velocityMeasure[i] == null)
                    continue;

                if (measureNoteValues[i] == null) //might be created earlier for i = 0 when closing old notes
                    measureNoteValues[i] = new();

                //play random interval from current intervals and fundamental 
                int noteNameNumber = ((int)(CurrentFundamental + currentIntervals.TakeRandom())) % 12; //C is 0
                int noteNumber = ((CurrentOctave + 1) * 12) + noteNameNumber;
                int velocity = (int)velocityMeasure[i]!; //checked for null on the lines above                
                measureNoteValues[i]![noteNumber] = velocity; //created the dictionary on the lines above
            }
            Measure measure = new(measureNoteValues);
            measures.Add(measure);
            measureIndex++;

            //Update current chord and fundamental
            //Chord to any other scale class chord
            //Normalize chord by moving fundamental - chord stays the same, fundamental changes            
            CurrentFundamental = (CurrentFundamental + scaleTranslations.TakeRandom()) % 12;
        }
        return measures;
    }
}

