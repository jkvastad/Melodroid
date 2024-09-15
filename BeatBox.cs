using Fractions;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MusicTheory;
using System;
using System.Linq;

public class BeatBox(IRhythmMeasureMaker rhythmMeasureMaker, IMeasureHarmonizer measureHarmonizer)
{
    public IMeasureHarmonizer MeasureHarmonizer = measureHarmonizer;
    public IRhythmMeasureMaker RhythmMeasureMaker = rhythmMeasureMaker;

    public List<Measure> MakeMeasures()
    {
        List<int?[]> velocities = RhythmMeasureMaker.MakeVelocities();
        return MeasureHarmonizer.MeasuresFromVelocities(velocities);
    }

    public void WriteMeasuresToMidi(List<Measure> measures, string folderPath, string fileName, bool overWrite = false)
    {

        /** Example Usage
            string folderPath = @"E:\Documents\Reaper Projects\Melodroid\MIDI_write_testing\";
            int timeDivision = 8;
            NoteValue?[] noteValues = new NoteValue?[timeDivision];
            noteValues[3] = new(NoteName.A, 4, 64);
            Measure measure = new(noteValues);
            List<Measure> measureList = new();
            measureList.Add(measure);
            WriteMeasuresToMidi(measureList, folderPath, "test", true);
        **/

        MidiFile midiFile = new MidiFile();

        //TODO set tempo

        TrackChunk trackChunk = new TrackChunk();
        using (TimedObjectsManager<Melanchall.DryWetMidi.Interaction.Note> notesManager = trackChunk.ManageNotes())
        {
            TimedObjectsCollection<Melanchall.DryWetMidi.Interaction.Note> notes = notesManager.Objects;
            NoteBuilder nb = new NoteBuilder(measures);
            midiFile.TimeDivision = new TicksPerQuarterNoteTimeDivision(nb.MidiTimeDivision);
            foreach (var note in nb.Notes)
            {
                notes.Add(note);
            }
        }

        midiFile.Chunks.Add(trackChunk);
        midiFile.Write(Path.Combine(folderPath, fileName + ".mid"), overWrite);
    }
}

//Decorates rhythms (lists with time division number of nullable velocities) with scientific pitch according to the rules of consonance 
public interface IMeasureHarmonizer
{
    //See Measure MIDIKeys
    // - array position is time division steps in measure, key is MIDI note, value is velocity
    public List<Measure> MeasuresFromVelocities(List<int?[]> velocityMeasures);
}

//Creates rhythms (lists with time division number of nullable velocities) according to some heuristic (e.g. simply isochrony or triplet swing time)
public interface IRhythmMeasureMaker
{
    //Velocity 0 is note off
    //Else note on
    //Cannot represent crescendo - how to differ between change of velocity for old note and new note with new velocity value?
    // - Midi has no explicit support for crescendo - must use expression/volume messages, so perhaps the problem is elsewhere any way.
    public List<int?[]> MakeVelocities();
}
