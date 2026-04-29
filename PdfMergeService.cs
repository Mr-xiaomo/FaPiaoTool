using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace FaPiaoTool
{
    public enum LayoutMode
    {
        Single,
        Double,
        Quadruple
    }

    public class PdfMergeService
    {
        // A4 at 72 dpi in points
        private const double A4WidthPt = 595.28;
        private const double A4HeightPt = 841.89;

        // Render DPI for high-fidelity output (400 DPI for high clarity)
        private const int RenderDpi = 400;

        // Margin in points
        private const double MarginPt = 10;

        // Security limits
        public const long MaxFileBytes = 50 * 1024 * 1024; // 50MB per file
        public const int MaxTotalPages = 200;              // Max total pages to process

        /// <summary>
        /// Validate that a file is a real PDF by checking its header.
        /// </summary>
        public static bool IsValidPdf(string pdfPath)
        {
            try
            {
                if (!File.Exists(pdfPath)) return false;
                var fi = new FileInfo(pdfPath);
                if (fi.Length < 8) return false; // Too small to be a valid PDF

                byte[] header = new byte[5];
                using var fs = new FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Read(header, 0, 5) < 5) return false;
                // PDF files must start with %PDF-
                return header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get page count of a PDF file. Caller should ensure file is a valid PDF first.
        /// </summary>
        public static int GetPageCount(string pdfPath)
        {
            using var stream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return PDFtoImage.Conversion.GetPageCount(stream, leaveOpen: false, password: null);
        }

        /// <summary>
        /// Render a single PDF page to a SkiaSharp SKBitmap at the specified DPI.
        /// Accepts pre-loaded bytes to avoid redundant file reads.
        /// </summary>
        private static SkiaSharp.SKBitmap RenderPageToBitmap(byte[] pdfBytes, int pageIndex, int dpi)
        {
            var options = new PDFtoImage.RenderOptions
            (
                dpi,
                null,  // Width
                null,  // Height
                true,  // WithAnnotations — 保留图层（红章等）
                true,  // WithFormFill — 保留表单填充
                false, // WithAspectRatio
                PDFtoImage.PdfRotation.Rotate0,
                PDFtoImage.PdfAntiAliasing.All,
                null,  // BackgroundColor
                null,  // Bounds
                false, // UseTiling
                false, // DpiRelativeToBounds
                false  // Grayscale
            );
            return PDFtoImage.Conversion.ToImage(pdfBytes, page: pageIndex, password: null, options: options);
        }

        /// <summary>
        /// Main merge method. Returns a Task that completes when merging is done.
        /// Calls progressCallback with a value between 0 and 100.
        /// </summary>
        public static async Task MergeAsync(
            List<string> pdfPaths,
            LayoutMode layout,
            string outputPath,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                // Step 1: Validate all files and pre-load bytes once per file
                var fileDataList = new List<(string path, byte[] bytes, int pageCount)>();
                int totalPages = 0;

                foreach (var path in pdfPaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Validate PDF header
                    if (!IsValidPdf(path))
                        throw new InvalidOperationException($"文件不是有效的PDF格式: {Path.GetFileName(path)}");

                    // Check file size
                    var fi = new FileInfo(path);
                    if (fi.Length > MaxFileBytes)
                        throw new InvalidOperationException($"文件大小超过限制({MaxFileBytes / 1024 / 1024}MB): {Path.GetFileName(path)}");

                    // Read file bytes once, reuse for page count and rendering
                    var bytes = File.ReadAllBytes(path);
                    int pageCount = 0;
                    using (var ms = new MemoryStream(bytes))
                    {
                        pageCount = PDFtoImage.Conversion.GetPageCount(ms, leaveOpen: false, password: null);
                    }

                    totalPages += pageCount;

                    // Check total page limit
                    if (totalPages > MaxTotalPages)
                        throw new InvalidOperationException($"总页数超过限制({MaxTotalPages}页)，当前已累计{totalPages}页");

                    fileDataList.Add((path, bytes, pageCount));
                }

                // Step 2: Process pages incrementally — render and compose per sheet to minimize memory
                int pagesPerSheet = layout switch
                {
                    LayoutMode.Single => 1,
                    LayoutMode.Double => 2,
                    LayoutMode.Quadruple => 4,
                    _ => 2
                };

                int cols = layout switch
                {
                    LayoutMode.Single => 1,
                    LayoutMode.Double => 1,
                    LayoutMode.Quadruple => 2,
                    _ => 1
                };

                int rows = layout switch
                {
                    LayoutMode.Single => 1,
                    LayoutMode.Double => 2,
                    LayoutMode.Quadruple => 2,
                    _ => 2
                };

                double pageWidthPt = (layout == LayoutMode.Quadruple) ? A4HeightPt : A4WidthPt;
                double pageHeightPt = (layout == LayoutMode.Quadruple) ? A4WidthPt : A4HeightPt;

                int canvasWidthPx = (int)(pageWidthPt / 72.0 * RenderDpi);
                int canvasHeightPx = (int)(pageHeightPt / 72.0 * RenderDpi);

                // Build a flat list of (fileIndex, pageIndex) for all pages
                var pageSlots = new List<(int fileIdx, int pageIdx)>();
                for (int fi = 0; fi < fileDataList.Count; fi++)
                {
                    for (int pi = 0; pi < fileDataList[fi].pageCount; pi++)
                    {
                        pageSlots.Add((fi, pi));
                    }
                }

                int totalSheets = (pageSlots.Count + pagesPerSheet - 1) / pagesPerSheet;

                // Step 3: Create PDF document and compose sheets
                var document = new PdfDocument();
                try
                {
                    document.Info.Title = "合并发票";
                    document.Info.Author = "发票合并工具";
                    document.Info.Creator = "发票合并工具 v1.0";

                    int processedPages = 0;

                    for (int sheet = 0; sheet < totalSheets; sheet++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Render page bitmaps for this sheet only
                        var sheetBitmaps = new List<SkiaSharp.SKBitmap>();
                        try
                        {
                            for (int slot = 0; slot < pagesPerSheet; slot++)
                            {
                                int slotIdx = sheet * pagesPerSheet + slot;
                                if (slotIdx >= pageSlots.Count) break;

                                var (fileIdx, pageIdx) = pageSlots[slotIdx];
                                var bitmap = RenderPageToBitmap(fileDataList[fileIdx].bytes, pageIdx, RenderDpi);
                                sheetBitmaps.Add(bitmap);
                                processedPages++;
                                progress?.Report((int)(processedPages * 50.0 / totalPages));
                            }

                            // Compose sheet canvas
                            var canvas = new SkiaSharp.SKBitmap(canvasWidthPx, canvasHeightPx);
                            try
                            {
                                using (var canvasCanvas = new SkiaSharp.SKCanvas(canvas))
                                {
                                    canvasCanvas.Clear(SkiaSharp.SKColors.White);

                                    for (int slot = 0; slot < sheetBitmaps.Count; slot++)
                                    {
                                        var pageBitmap = sheetBitmaps[slot];

                                        double cellWidthPt = (pageWidthPt - (cols + 1) * MarginPt) / cols;
                                        double cellHeightPt = (pageHeightPt - (rows + 1) * MarginPt) / rows;

                                        int col = slot % cols;
                                        int row = slot / cols;

                                        double cellX = MarginPt + col * (cellWidthPt + MarginPt);
                                        double cellY = MarginPt + row * (cellHeightPt + MarginPt);

                                        double cellWidthPx = cellWidthPt / 72.0 * RenderDpi;
                                        double cellHeightPx = cellHeightPt / 72.0 * RenderDpi;

                                        double scaleX = cellWidthPx / pageBitmap.Width;
                                        double scaleY = cellHeightPx / pageBitmap.Height;
                                        double scale = Math.Min(scaleX, scaleY);

                                        int destW = (int)(pageBitmap.Width * scale);
                                        int destH = (int)(pageBitmap.Height * scale);

                                        double destX = cellX / 72.0 * RenderDpi + (cellWidthPx - destW) / 2.0;
                                        double destY = cellY / 72.0 * RenderDpi + (cellHeightPx - destH) / 2.0;

                                        var destRect = new SkiaSharp.SKRect(
                                            (float)destX,
                                            (float)destY,
                                            (float)(destX + destW),
                                            (float)(destY + destH));

                                        canvasCanvas.DrawBitmap(pageBitmap, destRect);
                                    }
                                }

                                // Add this sheet to the PDF document
                                var page = document.AddPage();
                                page.Width = XUnit.FromPoint(pageWidthPt);
                                page.Height = XUnit.FromPoint(pageHeightPt);

                                using (var gfx = XGraphics.FromPdfPage(page))
                                {
                                    // Use PNG for lossless quality (preserves stamps, QR codes, text edges)
                                    using var skImage = SkiaSharp.SKImage.FromBitmap(canvas);
                                    using var skData = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                                    using var ms = new MemoryStream();
                                    skData.SaveTo(ms);
                                    ms.Position = 0;

                                    using var xImage = XImage.FromStream(() => ms);
                                    gfx.DrawImage(xImage, 0, 0, pageWidthPt, pageHeightPt);
                                }
                            }
                            finally
                            {
                                canvas.Dispose();
                            }
                        }
                        finally
                        {
                            // Dispose sheet bitmaps immediately after use to free memory
                            foreach (var bmp in sheetBitmaps)
                                bmp.Dispose();
                        }

                        progress?.Report(50 + (int)((sheet + 1) * 50.0 / totalSheets));
                    }

                    document.Save(outputPath);
                }
                finally
                {
                    document.Dispose();
                }

                progress?.Report(100);

            }, cancellationToken);
        }
    }
}
