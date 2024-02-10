// See https://aka.ms/new-console-template for more information
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using MusicTheory;

//MIDI standard: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
//Note time in MIDI is defined in header chunk as number of divisions of quarter beat, e.g. setting "division" to 12 means a quarter beat has 12 divisions.
//Tempo then decides length in seconds of divisions.
/**
Time and length is poorly explained in docs:
Time is when something happens, length is for how long:
    notes.Add(new Melanchall.DryWetMidi.Interaction.Note(NoteName.B, 4)
    {
        Velocity = (SevenBitNumber)64,
        Time = 96,
        Length = 192,
    });
This means "create scientific pitch note B4 at 96 ticks into the midi file, make it 192 ticks long".
**/

// TODO: Start work on beatbox to calculate rhythms based on "How Beat Perception Co-opts Motor Neurophysiology". This can be used to define rhythmic chunks.
// Calculate lcm for coprime numerators to get the "resolution" of a rhythmic chunk. Perhaps use prime factorization and put the primes into a set to get the lcm.
//https://github.com/melanchall/drywetmidi#getting-started
//https://melanchall.github.io/drywetmidi/

string folderPath = @"E:\Documents\Reaper Projects\Melodroid\MIDI_write_testing\";

var measure = new Measure(8);
measure.Notes[0] = new(true, 64);
measure.Notes[4] = NoteOnOff.NoteOff;

Console.Write(measure);
Console.Write(measure);

//PrintLengthOf(new MetricTimeSpan(hours: 0, minutes: 0, seconds: 10), 0, tempoMap);
//WriteMIDIWithTimedObjectManager(Path.Combine(folderPath, "midi test two notes velocity.mid"));

void PrintLengthOf(ITimeSpan length, long time, TempoMap tempo)
{
    Console.WriteLine($"tempo.TimeDivision: {tempo.TimeDivision}");
    Console.WriteLine($"{nameof(length)}: {length}");
    Console.WriteLine($"{nameof(time)}: {time}");
    Console.WriteLine($"tempo.GetTempoAtTime(length).BeatsPerMinute: {tempo.GetTempoAtTime(length).BeatsPerMinute}");
    Console.WriteLine($"LengthConverter.ConvertFrom(length, time, tempo): {LengthConverter.ConvertFrom(length, time, tempo)}");
}

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

    using (var tempoManager = new TempoMapManager())
    {
        tempoManager.SetTempo(0, Tempo.FromBeatsPerMinute(60));
        tempoManager.SetTempo(48, Tempo.FromBeatsPerMinute(120));
        tempoManager.SetTempo(144, Tempo.FromBeatsPerMinute(60));
        midiFile.ReplaceTempoMap(tempoManager.TempoMap);
    }

    TrackChunk trackChunk = new TrackChunk();
    using (TimedObjectsManager<Melanchall.DryWetMidi.Interaction.Note> notesManager = trackChunk.ManageNotes())
    {
        TimedObjectsCollection<Melanchall.DryWetMidi.Interaction.Note> notes = notesManager.Objects;

        notes.Add(new Melanchall.DryWetMidi.Interaction.Note(NoteName.A, 4)
        {
            Velocity = (SevenBitNumber)64,
            Time = 0,
            Length = 192,
        });
        notes.Add(new Melanchall.DryWetMidi.Interaction.Note(NoteName.B, 4)
        {
            Velocity = (SevenBitNumber)64,
            Time = 96,
            Length = 192,
        });
    }

    midiFile.Chunks.Add(trackChunk);
    midiFile.Write(fullWritePath, true);
}