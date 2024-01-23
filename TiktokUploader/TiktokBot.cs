using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Playwright;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TiktokUploader
{
    internal class TiktokBot
    {
        static string _homeUrl = "https://www.tiktok.com/";
        static string _uploadUrl = "https://www.tiktok.com/creator-center/upload";
        private readonly IBrowserContext _browser;
        private IPage? _page;

        public TiktokBot(IBrowserContext browser)
        {
            _browser = browser;
            _page = browser.Pages.FirstOrDefault(x => x.Url.Contains(_homeUrl));
        }

        public async Task Upload(int year, int start, int end)
        {
            var filePath = @$"F:\TikTokContentBot\Suomen top\{year} {start}-{end}.mp4";
            if(!File.Exists(filePath))
            {
                Console.WriteLine("Video doesn't exist");
                return;
            }
            var videos = GetVideoMeta().Where(x => x.Year == year && x.Ranking >= start && x.Ranking <= end)
                .OrderBy(x => x.Ranking)
                .ToArray();

            var text = string.Join(Environment.NewLine, videos.Select(x => $"{x.Ranking}. {x.Artist} - {x.Title}"));
            var tags = $"#suomi #top50 #{year}";
            foreach(var video in videos)
            {
                var artistTag = new string(video.Artist.ToLower().Where(x => char.IsLetterOrDigit(x)).ToArray());
                tags += $" #{artistTag}";
            }
            text += Environment.NewLine + tags;

            await Upload(filePath, text);
        }
        private async Task Upload(string filePath, string caption)
        {
            await Navigate(_uploadUrl);
            Thread.Sleep(500);
            var frame = _page!.Frames.First(x => x.Url.Contains("scene=creator_center"));
            
            var fileChooser = await _page.RunAndWaitForFileChooserAsync(async () =>
            {
                await frame.ClickAsync(".file-select-button");
            });
            await fileChooser.SetFilesAsync(filePath);
            
            await frame.WaitForSelectorAsync("img.candidate-v2");

            var captionEditor = (await frame.QuerySelectorAsync(".DraftEditor-editorContainer"))!;

            Thread.Sleep(500);
            await captionEditor.ClickAsync();

            Thread.Sleep(200);
            await captionEditor.PressAsync("Control+A");

            Thread.Sleep(200);
            await captionEditor.PressAsync("Backspace");

            Thread.Sleep(200);
            await _page.Keyboard.TypeAsync(caption);

            Thread.Sleep(1000);
            await frame.ClickAsync(".btn-post");
        }

        private async Task Navigate(string url)
        {
            _page = _browser.Pages.FirstOrDefault(x => x.Url.Contains("tiktok.com"));
            if (_page == null)
            {
                _page = await _browser.NewPageAsync();
                
            }
            await _page.GotoAsync(url);
        }

        private Video[] GetVideoMeta()
        {
            var excelPath = @$"F:\TikTokContentBot\Suomen top\top75.xlsx";
            // read excel file closedxml
            var workbook = new XLWorkbook(excelPath);
            var worksheet = workbook.Worksheets.First();
            var rows = worksheet.RowsUsed().Skip(1);
            var videos = new List<Video>();
            foreach(var row in rows)
            {
                try
                {
                    var video = new Video
                    {
                        Year = (int)row.Cell("A").Value.GetNumber(),
                        Ranking = (int)row.Cell("B").Value.GetNumber(),
                        Artist = row.Cell("C").GetString(),
                        Title = row.Cell("D").GetString()
                    };

                    videos.Add(video);
                }
                catch
                {
                    ;
                }
            }
            return videos.ToArray();
        }
   
        private class Video
        {
            public int Year { get; set; }
            public int Ranking { get; set; }
            public string Artist { get; set; }
            public string Title { get; set; }
        }
    }
}
