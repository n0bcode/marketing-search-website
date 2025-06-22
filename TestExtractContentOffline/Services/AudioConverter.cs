using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFMpegCore;

namespace VideoSubtitleExtractor.Services
{
    public static class AudioConverter
    {
        public static async Task ConvertToWavWithFFMpegCore(string inputPath, string outputPath)
        {
            Console.WriteLine("Checking input format before converting to WAV...");
            string tempMp4 = "temp.mp4";
            await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(tempMp4, true, options => options
                    .WithVideoCodec("copy")
                    .WithAudioCodec("aac"))
                .ProcessAsynchronously();
            Console.WriteLine("Converting to WAV using FFMpegCore...");
            await FFMpegArguments
                .FromFileInput(tempMp4)
                .OutputToFile(outputPath, true, options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 16000 -ac 1"))
                .ProcessAsynchronously();
            if (File.Exists(tempMp4)) File.Delete(tempMp4);
        }

    }
}