using Microsoft.Playwright;
using TiktokUploader;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.ConnectOverCDPAsync("http://localhost:9222");
var context = browser.Contexts[0];

var bot = new TiktokBot(context);
await bot.Upload(1993, 31, 35);