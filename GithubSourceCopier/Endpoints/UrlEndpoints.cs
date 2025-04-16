using AngleSharp;
using AngleSharp.Dom;
using GithubSourceCopier.Helpers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
                try { html = await client.GetStringAsync(url);}
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

