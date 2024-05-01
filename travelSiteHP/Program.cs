using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Data.Sqlite;
using System.Data;
using Dapper;
using SQLitePCL;
using Microsoft.AspNetCore.Http;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Builder;
using System.Xml.Linq;

Batteries.Init();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAntiforgery();
var connectionString = "Data Source=wwwroot/database/travel-site.db";
builder.Services.AddSingleton<IDbConnection>(_ => new SqliteConnection(connectionString));
var app = builder.Build();
app.MapFallbackToFile("/index.html");
app.UseAntiforgery();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.MapGet("/api/items", async (IDbConnection db) =>
{
    var items = await db.QueryAsync<Item>("SELECT ID, title FROM Items");
    if (items == null)
    {
        return Results.NotFound();
    }
    var selectOptions = new StringBuilder();
    selectOptions.Append("<option disabled selected>Please select an item</option>");
    foreach (var item in items)
    {
        selectOptions.Append($"<option value={item.ID}>{item.Title}</option>");
    }
    return Results.Ok(selectOptions.ToString());
});

app.MapGet("/api/content", async (IDbConnection db) =>
{
    var items = await db.QueryAsync<Item>("SELECT * FROM Items");

    if (items == null)
    {
        return Results.NotFound();
    }

    StringBuilder htmlBuilder = new StringBuilder();
    htmlBuilder.Append("""<div class="owl-carousel owl owl-theme">""");

    foreach (var item in items)
    {
        string base64Image = Convert.ToBase64String(item.Image);
        htmlBuilder.AppendFormat(
            "<div class=\"item\">" +
            "<img src=\"data:image/webp;base64,{0}\" alt=\"{1}\">" +
            "<div class=\"mask\">" +
            "<h2>{2}</h2>" +
            "</div>" +
            "</div>",
            base64Image,
            item.Title,
            item.Title
        );
    }
    htmlBuilder.Append("</div>");
    return Results.Content(htmlBuilder.ToString());
});
app.MapGet("/api/image/{id}", async (string id, IDbConnection db) =>
{
fetch:
byte[]? image;
try
{
    image = db.QueryFirstOrDefault<byte[]>("SELECT image FROM Items WHERE ID=@id", new { id });
}
catch
{
    goto fetch;
}
if (image == null)
{
    return Results.NotFound();
}
return Results.File(image, "image/webp");
});

app.MapPost("/api/insert", async (IFormFile img, [FromForm] string name, IDbConnection db, IAntiforgery antiforgery, HttpContext context) =>
{
    await antiforgery.ValidateRequestAsync(context);
    if (string.IsNullOrWhiteSpace(name) || img == null)
    {
        return Results.BadRequest("Name and Image are required.");
    }

    if (img != null)
    {
        var allowedExtensions = new[] { ".png", ".gif", ".jpeg", ".jpg",".webp" };
        var fileExtension = Path.GetExtension(img.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return Results.BadRequest("Only PNG, GIF, and JPEG images are allowed.");
        }
    }
    var file = ConvertIFormFileToByteArray(img!);
    using Image image = Image.Load(file);
    using MemoryStream stream = new();
    image.SaveAsWebp(stream, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 70 });
    var item = new Item
    {
        ID = Guid.NewGuid().ToString(),
        Title = name,
        Image = stream.ToArray()
    };
    var sql = "INSERT INTO items (ID, Title, Image) VALUES (@ID, @Title, @Image)";
    var affectedRows = await db.ExecuteAsync(sql, item);
    if (affectedRows == 0)
    {
        return Results.BadRequest();
    }
    return Results.NoContent();
});

app.MapDelete("/api/remove/{id}", async (string id, IDbConnection db) =>
{
    var affectedRows = await db.ExecuteAsync($"DELETE FROM items WHERE ID = @Id", new { Id = id });
    if (affectedRows == 0)
    {
        return Results.NotFound();
    }
    return Results.Ok();
});
app.MapGet("/api/tokens", (HttpContext context, IAntiforgery antiforgery) =>
{
    var token = antiforgery.GetAndStoreTokens(context);
    string html = $"""<input name = "{token.FormFieldName}" type = "hidden" value = "{token.RequestToken}"/>""";
    return Results.Content(html,"text/html");
});
app.Run();
byte[] ConvertIFormFileToByteArray(IFormFile file)
{
    using (MemoryStream memoryStream = new MemoryStream())
    {
        // Copy the file stream to the memory stream
        file.CopyTo(memoryStream);

        // Convert the memory stream to a byte array
        return memoryStream.ToArray();
    }

}

public class Item
{
    public string ID { get; set; }
    public string Title { get; set; }
    public byte[] Image { get; set; }
}



