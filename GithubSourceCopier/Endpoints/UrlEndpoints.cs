using AngleSharp;
using AngleSharp.Dom;
using GithubSourceCopier.Helpers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IO.Compression;

namespace GithubSourceCopier.Endpoints;

public static class UrlEndpoints
{
    public static void MapURLEndpoints(this WebApplication app)
    {
        app.MapGet("/generate-doc-stream", async (HttpContext http,
            [FromQuery, SwaggerParameter("Fırlanacaq saytın əsas URL-i, məsələn: https://muratdincc.github.io/interview-topics-turkish/", Required = true)]
                    string baseUrl) =>
        {
            Console.WriteLine($"[INFO] İstək alındı. baseUrl: {baseUrl}");

            // Parametr yoxlaması
            if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
            {
                Console.WriteLine("[ERROR] BaseUrl parametri düzgün deyil.");
                return Results.BadRequest(new { Error = "Düzgün baseUrl query parametri daxil edilməlidir." });
            }

            http.Response.ContentType = "text/event-stream";
            http.Response.Headers.Add("Cache-Control", "no-cache");

            var client = new HttpClient();
            var config = Configuration.Default;
            var ctx = BrowsingContext.New(config);

            Console.WriteLine("[INFO] URL'lərin axtarış prosesi başlatılır.");
            var urls = await CrawlAllInternalUrls(baseUrl, client, ctx);
            Console.WriteLine($"[INFO] Axtarış tamamlandı. Toplam tapılan URL sayı: {urls.Count}");

            var elements = new List<(string Type, string Text)>();
            int total = urls.Count, i = 0;

            foreach (var url in urls)
            {
                Console.WriteLine($"[INFO] ({i + 1}/{total}) - İşlənən URL: {url}");

                string html;
                try { html = await client.GetStringAsync(url); }
                catch (Exception ex) { Console.WriteLine($"[ERROR] {url} ünvanından HTML əldə edilərkən xəta: {ex.Message}"); continue; }

                var docu = await ctx.OpenAsync(req => req.Content(html));

                // Elementləri topla
                if (docu.QuerySelector("h1") is IElement h1)
                    elements.Add(("h1", h1.TextContent.Trim()));

                var h2Elements = docu.QuerySelectorAll("h2").ToList();
                if (h2Elements.Any())
                    foreach (var h2 in h2Elements)
                        elements.Add(("h2", h2.TextContent.Trim()));

                var pElements = docu.QuerySelectorAll("p").ToList();
                if (pElements.Any())
                    foreach (var p in pElements)
                        elements.Add(("p", p.TextContent.Trim()));

                var codeElements = docu.QuerySelectorAll("pre > code").ToList();
                if (codeElements.Any())
                    foreach (var code in codeElements)
                        elements.Add(("code", code.TextContent.Trim()));

                // Progress göndər
                i++;
                var percent = (int)(i * 100.0 / total);
                Console.WriteLine($"[INFO] Progress: {percent}%");
                await http.Response.WriteAsync($"event: progress\ndata: {percent}\n\n");
                await http.Response.Body.FlushAsync();
            }

            // İş bitdikdə sənədi yarat və Base64 şəklində göndər
            Console.WriteLine("[INFO] Bütün URL-lər işlənildi. Document yaradılır...");
            using var ms = new MemoryStream();
            DocxHelper.CreateDocx(ms, elements);
            var b64 = Convert.ToBase64String(ms.ToArray());
            Console.WriteLine("[INFO] .docx sənədi yaradıldı və Base64 şəklində göndərilir.");
            await http.Response.WriteAsync($"event: done\ndata: {b64}\n\n");
            await http.Response.Body.FlushAsync();

            return Results.Empty;
        })
        .WithName("GenerateDocument")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "Dinamik .docx generasiya et",
            description: "Saytı fırlanaraq bütün başlıqları, mətnləri və kod nümunələrini çəkir və .docx faylını Base64 şəklində qaytarır."
        ));


        app.MapGet("/mirror-site-stream", async (HttpContext http,
            [FromQuery, SwaggerParameter("Mirror ediləcək saytın əsas URL-i, məsələn: https://tdvtsa.hilalkurslari.az/", Required = true)]
            string baseUrl) =>
        {
            Console.WriteLine($"[INFO] Mirror-stream istəyi alındı. baseUrl: {baseUrl}");

            // 1) Parametr yoxlaması
            if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
            {
                Console.WriteLine("[ERROR] baseUrl parametri düzgün deyil.");
                return Results.BadRequest(new { Error = "Düzgün baseUrl daxil edilməlidir." });
            }

            // 2) SSE cavab başlıqları
            http.Response.ContentType = "text/event-stream";
            http.Response.Headers.Add("Cache-Control", "no-cache");

            // 3) Hazırlıqlar: HTTP client, AngleSharp konteksti, diskdə qovluq
            var client = new HttpClient();
            var ctx = BrowsingContext.New(Configuration.Default);
            var host = new Uri(baseUrl).Host;
            var output = Path.Combine("MirroredSites", host);
            Directory.CreateDirectory(output);

            // 4) Crawl: bütün daxili URL-ləri topla
            Console.WriteLine("[INFO] Crawling prosesi başlayır...");
            var urls = await CrawlAllInternalUrls(baseUrl, client, ctx);
            Console.WriteLine($"[INFO] Tapılan URL sayı: {urls.Count}");

            // 5) Hər URL üçün yükləmə və proqres
            int idx = 0, total = urls.Count;
            foreach (var url in urls)
            {
                idx++;
                Console.WriteLine($"[INFO] ({idx}/{total}) yüklənir: {url}");
                string html;
                try
                {
                    html = await client.GetStringAsync(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] HTML alınmadı: {url} – {ex.Message}");
                    continue;
                }

                // Diskə HTML yaz
                var doc = await ctx.OpenAsync(req => req.Content(html));
                var uri = new Uri(url);
                var rel = string.IsNullOrEmpty(uri.AbsolutePath.Trim('/')) ? "index" : uri.AbsolutePath.Trim('/');
                var name = rel.Replace("/", "_") + ".html";
                var path = Path.Combine(output, name);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllTextAsync(path, html);
                Console.WriteLine($"[INFO] HTML saxlanıldı: {path}");

                // Statik resursları filtrlə və yüklə
                var resourceUrls = doc.QuerySelectorAll("link[href], script[src], img[src]")
                    .Select(el => el.GetAttribute("href") ?? el.GetAttribute("src"))
                    .Where(src => !string.IsNullOrWhiteSpace(src))
                    .Select(src => new Uri(uri, src))
                    .Where(r =>
                    {
                        var ext = Path.GetExtension(r.LocalPath);
                        var allowed = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg" };
                        return !string.IsNullOrEmpty(ext)
                            && allowed.Contains(ext, StringComparer.OrdinalIgnoreCase)
                            && r.Host.Equals(uri.Host, StringComparison.OrdinalIgnoreCase);
                    })
                    .Select(r => r.AbsoluteUri)
                    .Distinct();

                foreach (var res in resourceUrls)
                {
                    try
                    {
                        var data = await client.GetByteArrayAsync(res);
                        var ru = new Uri(res);
                        var rpath = Path.Combine(output, ru.Host, ru.AbsolutePath.TrimStart('/'));
                        Directory.CreateDirectory(Path.GetDirectoryName(rpath)!);
                        await File.WriteAllBytesAsync(rpath, data);
                        Console.WriteLine($"[INFO] Resurs saxlanıldı: {rpath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Resurs xətası: {res} – {ex.Message}");
                    }
                }

                // 6) Proqres eventini göndər və flush et
                var percent = (int)(idx * 100.0 / total);
                await http.Response.WriteAsync($"event: progress\ndata: {percent}\n\n");
                await http.Response.Body.FlushAsync();
            }

            // 7) Yaddaşda zip yarat → zipBase64 → done event
            Console.WriteLine("[INFO] Zip yaradılır...");
            using var zipMs = new MemoryStream();
            using (var archive = new ZipArchive(zipMs, ZipArchiveMode.Create, true))
            {
                foreach (var file in Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories))
                {
                    var entryName = Path.GetRelativePath(output, file);
                    var e = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                    using var fs = File.OpenRead(file);
                    using var es = e.Open();
                    await fs.CopyToAsync(es);
                }
            }
            zipMs.Position = 0;
            var zipBase64 = Convert.ToBase64String(zipMs.ToArray());

            Console.WriteLine("[INFO] Zip Base64-ə çevrildi, göndərilir.");
            await http.Response.WriteAsync($"event: done\ndata: {zipBase64}\n\n");
            await http.Response.Body.FlushAsync();

            return Results.Empty;
        })
        .WithName("MirrorSiteStream")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "Saytın statik nüsxəsini SSE ilə arxivləşdir və Base64 göndər",
            description: "Verilən URL-in bütün səhifələrini və statik resursları yükləyib zip edib Base64-ə çevirir, progress və done eventləri ilə SSE axını yaradır."
        ));

    }

    // Sadə breadth‑first crawling nümunəsi
    private static async Task<List<string>> CrawlAllInternalUrls(string baseUrl, HttpClient http, IBrowsingContext ctx)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var q = new Queue<string>();
        visited.Add(baseUrl); q.Enqueue(baseUrl);

        Console.WriteLine("[INFO] Crawling prosesi başlayır...");
        while (q.Count > 0)
        {
            Console.WriteLine($"[INFO] Say: {q.Count}");
            var u = q.Dequeue();
            string html;
            try { html = await http.GetStringAsync(u); }
            catch (Exception ex) { Console.WriteLine($"[ERROR] {u} ünvanından HTML əldə edilərkən xəta: {ex.Message}"); continue; }
            var doc = await ctx.OpenAsync(req => req.Content(html));
            foreach (var a in doc.QuerySelectorAll("a"))
            {
                var href = a.GetAttribute("href");
                if (string.IsNullOrWhiteSpace(href)) continue;
                var abs = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? href
                    : new Uri(new Uri(baseUrl), href).AbsoluteUri;
                if (abs.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase) && visited.Add(abs))
                    q.Enqueue(abs);
            }
        }
        Console.WriteLine("[INFO] Crawling prosesi tamamlandı.");
        return visited.ToList();
    }

}

