using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoLibrary;
using Whisper.net;

namespace Api.Services.VideoServices.Sub
{
    public static class WhisperTranscriber
    {
        public static async Task TranscribeAsync(string audioFile, string transcriptFile)
        {
            using var whisperFactory = WhisperFactory.FromPath("./whisper-models/ggml-base.bin");
            using var processor = whisperFactory.CreateBuilder().WithLanguage("vi").Build();

            using (var fileStream = File.OpenRead(audioFile))
            {
                byte[] header = new byte[4];
                fileStream.Read(header, 0, 4);
                string riffHeader = System.Text.Encoding.ASCII.GetString(header);
                if (riffHeader != "RIFF")
                    throw new InvalidDataException("File is not a valid RIFF WAV file.");

                fileStream.Position = 0;
                var segments = new List<SegmentData>();
                await foreach (var segment in processor.ProcessAsync(fileStream))
                {
                    segments.Add(segment);
                }
                string transcript = string.Join(Environment.NewLine, segments.Select(s => s.Text));
                File.WriteAllText(transcriptFile, transcript);

                Console.WriteLine("\n=== Transcription Result ===\n");
                Console.WriteLine(transcript);
            }

        }
    }
}