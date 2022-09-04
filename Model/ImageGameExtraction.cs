using IronOcr;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace MrMeeseeks.NonogramSolver.Model
{
    public interface IFromImageGameEditor
    {
        Task OCR(string path);
    }

    internal class FromImageGameEditor : IFromImageGameEditor
    {
        public async Task OCR(string path)
        {
            var image = await Image.LoadAsync(path)
                .ConfigureAwait(false);
            image.Mutate(ipc => ipc.ProcessPixelRowsAsVector4(row =>
            {
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] = Vector4.UnitW;
                }
            }));
            var fileInfo = new FileInfo(path);
            var savePath = $"{fileInfo.Directory?.Name}{Path.DirectorySeparatorChar}asdf{fileInfo.Name}";
            await image.SaveAsPngAsync(savePath)
                .ConfigureAwait(false);
            var result = new IronTesseract(new TesseractConfiguration
            {
                WhiteListCharacters = "0123456789",
                PageSegmentationMode = TesseractPageSegmentationMode.SparseTextOsd
            }).Read(path);
            foreach (OcrResult.Block resultBlock in result.Blocks)
            {
                
            }
        }
    }
}