// See https://aka.ms/new-console-template for more information
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

//MIDI standard: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
//Note time in MIDI is defined in header chunk as number of divisions of quarter beat, e.g. setting "division" to 12 means a quarter beat has 12 divisions.
//Tempo then decides length in seconds of divisions.
//TODO: write any MIDI file using DryWetMidi and run it in reaper
//https://github.com/melanchall/drywetmidi#getting-started
//https://melanchall.github.io/drywetmidi/

//TODO: continue reading time https://melanchall.github.io/drywetmidi/articles/high-level-managing/Time-and-length.html

string folderPath = @"E:\Documents\Reaper Projects\Melodroid\MIDI_write_testing\";

//WriteMIDIWithTimedObjectManager(Path.Combine(folderPath, "midi test start time 2.mid"));

void WriteMIDIWithMidiEvents(string fullWritePath)
{
    MidiFile midiFile = new MidiFile();
    new TrackChunk(new SetTempoEvent(500000));
    //... etc., seems clunky
    midiFile.Write(fullWritePath);
}

void WriteMIDIWithTimedObjectManager(string fullWritePath)
{
    MidiFile midiFile = new MidiFile();
    TempoMap tempoMap = midiFile.GetTempoMap();

    TrackChunk trackChunk = new TrackChunk();
    using (TimedObjectsManager<Melanchall.DryWetMidi.Interaction.Note> notesManager = trackChunk.ManageNotes())
    {
        TimedObjectsCollection<Melanchall.DryWetMidi.Interaction.Note> notes = notesManager.Objects;
        notes.Add(new Melanchall.DryWetMidi.Interaction.Note(
            NoteName.A,
            4,
            LengthConverter.ConvertFrom(
                new MetricTimeSpan(hours: 0, minutes: 0, seconds: 10),
                0,
                tempoMap)));
    }
    midiFile.Chunks.Add(trackChunk);    
    midiFile.Write(fullWritePath);
}