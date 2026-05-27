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


app.MapPost("/api/upload", async (HttpContext context) =>
{
    if (!context.Request.HasFormContentType)
    {
        return Results.BadRequest("SYS_ERR: INVALID_TRANSMISSION_PAYLOAD_TYPE");
    }

    // Capture the custom multi-segment boundary string passed down from the browser
    var boundary = context.Request.GetMultipartBoundary();
    if (string.IsNullOrEmpty(boundary))
    {
        return Results.BadRequest("SYS_ERR: MISSING_BOUNDARY_DESCRIPTOR");
    }

    var reader = new MultipartReader(boundary, context.Request.Body);
    var downloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wshare-downloads");

    if (!Directory.Exists(downloadDirectory))
    {
        Directory.CreateDirectory(downloadDirectory);
    }

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

// 5. Diagnostics Terminal Display Layer on Application Boot up
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
    Console.WriteLine("\nAcess local data files by routing your target browser to:\n");


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
    Console.WriteLine("  >> http://localhost:5050 (Local Dev Mode Only)");
    Console.ResetColor();
    Console.WriteLine("\n======================================================================\n");
});

app.Run();