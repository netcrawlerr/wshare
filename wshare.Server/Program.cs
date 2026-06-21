using System.Net;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5050);

    // no capping limit (it ain't Ethio-Telecom)
    options.Limits.MaxRequestBodySize = null;
});

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = long.MaxValue; });

// cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

var downloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
if (!Directory.Exists(downloadDirectory))
{
    Directory.CreateDirectory(downloadDirectory);
}

// List all files in wshare-downloads directory
app.MapGet("/api/files", () =>
{
    if (!Directory.Exists(downloadDirectory)) return Results.Ok(Array.Empty<object>());

    var files = Directory.GetFiles(downloadDirectory, "*.*", SearchOption.AllDirectories)
        .Select(filePath =>
        {
            var fileInfo = new FileInfo(filePath);
            var relativePath = Path.GetRelativePath(downloadDirectory, filePath).Replace("\\", "/");
            return new
            {
                name = fileInfo.Name,
                size = FormatBytes(fileInfo.Length),
                path = relativePath
            };
        });

    return Results.Ok(files);
});

// download z specific file
app.MapGet("/api/download", (string path) =>
{
    var safePath = Path.GetFullPath(Path.Combine(downloadDirectory, path));
    if (!safePath.StartsWith(downloadDirectory, StringComparison.OrdinalIgnoreCase) || !File.Exists(safePath))
    {
        return Results.NotFound("SYS_ERR: FILE_NOT_FOUND");
    }

    return Results.File(safePath, "application/octet-stream", Path.GetFileName(safePath));
});

app.MapPost("/api/upload", async (HttpContext context) =>
{
    if (!context.Request.HasFormContentType)
    {
        return Results.BadRequest("SYS_ERR: INVALID_TRANSMISSION_PAYLOAD_TYPE");
    }

    var boundary = context.Request.GetMultipartBoundary();
    if (string.IsNullOrEmpty(boundary))
    {
        return Results.BadRequest("SYS_ERR: MISSING_BOUNDARY_DESCRIPTOR");
    }

    var reader = new MultipartReader(boundary, context.Request.Body);

    int filesProcessed = 0;
    MultipartSection? section;


    while ((section = await reader.ReadNextSectionAsync()) != null)
    {
        if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
        {
            if (contentDisposition.DispositionType.Equals("form-data"))
            {
                var fileName = contentDisposition.FileName.Value;
                if (string.IsNullOrEmpty(fileName)) continue;


                var sanitizedPath = fileName.Replace("\\", "/");
                var absoluteTargetDestination = Path.Combine(downloadDirectory, sanitizedPath);


                var parentDirectory = Path.GetDirectoryName(absoluteTargetDestination);
                if (!string.IsNullOrEmpty(parentDirectory) && !Directory.Exists(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }


                using var fileStream = File.Create(absoluteTargetDestination);
                await section.Body.CopyToAsync(fileStream);
                filesProcessed++;

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[STREAM_PROGRESS] -> Captured: {sanitizedPath}");
                Console.ResetColor();
            }
        }
    }

    return filesProcessed > 0
        ? Results.Ok(new { message = $"SYS_MSG: SUCCESSFUL_ENTRY_OF_{filesProcessed}_OBJECTS" })
        : Results.BadRequest("SYS_ERR: EMPTY_PAYLOAD");
});

// diagnostics Terminal Display (thanks to GPT)
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("======================================================");
    Console.WriteLine("  ██╗    ██╗███████╗██╗  ██╗ █████╗ ██████╗ ███████╗   ");
    Console.WriteLine("  ██║    ██║██╔════╝██║  ██║██╔══██╗██╔══██╗██╔════╝   ");
    Console.WriteLine("  ██║ █╗ ██║███████╗███████║███████║██████╔╝█████╗     ");
    Console.WriteLine("  ██║███╗██║╚════██║██╔══██║██╔══██║██╔══██╗██╔══╝     ");
    Console.WriteLine("  ╚███╔███╔╝███████║██║  ██║██║  ██║██║  ██║███████╗   ");
    Console.WriteLine("   ╚══╝╚══╝ ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝   ");
    Console.WriteLine("======================================================");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(" [SYS_STATUS] PROTOCOL ACTIVE. SERVER RUNNING READY TO ACCEPT STREAMS  ");
    Console.WriteLine("======================================================================");
    Console.ResetColor();
    Console.WriteLine("\nAccess local data files by routing your target browser to:\n");


    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        {
            var props = ni.GetIPProperties();
            foreach (var ip in props.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("  >> ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"http://{ip.Address}:5050 (From Any Other Device)");
                }
            }
        }
    }

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  >> http://localhost:5050 (local Dev Mode Only)");
    Console.ResetColor();
    Console.WriteLine("\n======================================================================\n");
});

app.Run();

static string FormatBytes(long bytes)
{
    string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
    int index = 0;
    double number = bytes;
    while (number >= 1024 && index < suffixes.Length - 1)
    {
        number /= 1024;
        index++;
    }
    return $"{number:0.#} {suffixes[index]}";
}
