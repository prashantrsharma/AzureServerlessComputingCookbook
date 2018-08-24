using System;
using System.Collections.Generic;
using System.IO;
using ImageResizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace CH1.Functions
{
    public static class ResizeImage
    {
        [FunctionName("ResizeImage")]
        public static void Run(
            [BlobTrigger("userprofileimagecontainer/{name}")]Stream image,
            [Blob("userprofileimagecontainer-sm/{name}", FileAccess.Write)]Stream imageSmall,
            [Blob("userprofileimagecontainer-md/{name}", FileAccess.Write)]Stream imageMedium,
            TraceWriter log)  // output blobs
        {
            try
            {
                log.Info("ResizeImage function processed a request.");

                var imageBuilder = ImageResizer.ImageBuilder.Current;
                var size = imageDimensionsTable[ImageSize.Small];

                imageBuilder.Build(
                    image, imageSmall,
                    new ResizeSettings(size.Item1, size.Item2, FitMode.Max, null), false);

                image.Position = 0;
                size = imageDimensionsTable[ImageSize.Medium];

                imageBuilder.Build(
                    image, imageMedium,
                    new ResizeSettings(size.Item1, size.Item2, FitMode.Max, null), false);
            }
            catch (Exception ex)
            {
                log.Info($"ResizeImage function : Exception:{ ex.Message}");
                throw;
            }
            finally
            {
                log.Info("ResizeImage function has finished processing a request.");
            }
           
        }

        public enum ImageSize
        {
            ExtraSmall, Small, Medium
        }

        private static Dictionary<ImageSize, Tuple<int, int>> imageDimensionsTable = new Dictionary<ImageSize, Tuple<int, int>>()
        {
            { ImageSize.ExtraSmall, Tuple.Create(320, 200) },
            { ImageSize.Small,      Tuple.Create(640, 400) },
            { ImageSize.Medium,     Tuple.Create(800, 600) }
        };
    }
}
