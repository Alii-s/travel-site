using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Data.Sqlite;
using System.Data;
using Dapper;
using SQLitePCL;
using Microsoft.AspNetCore.Http;
using System.Text;
Batteries.Init();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAntiforgery();
var connectionString = "Data Source=database/travel-site.db";
builder.Services.AddSingleton<IDbConnection>(_ => new SqliteConnection(connectionString));
var app = builder.Build();
app.UseAntiforgery();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.MapGet("/admin", (HttpContext context, IAntiforgery antiforgery) =>
{

    var token = antiforgery.GetAndStoreTokens(context);
    string htmlContent = $@"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>admin</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet""
        integrity=""sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH"" crossorigin=""anonymous"">
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/unpoly@3.7.3/unpoly-bootstrap5.min.css"">
    <link rel=""stylesheet"" href=""index1.css"">
</head>

<body>
    <h2 style=""text-align: center;"" class=""my-3"">Add a item</h2>
    <div class=""container-fluid my-4 d-flex justify-content-center squareBox align-items-center"">

        <form action=""/api/insert"" method=""post"" enctype=""multipart/form-data"" class=""card p-4 needs-validation"">
            <input name=""{token.FormFieldName}"" type=""hidden"" value=""{token.RequestToken}"" />
            <div class=""mb-3"">
                <label for=""imageName"" class=""form-label"">Image Title</label>
                <input type=""text"" class=""form-control"" id=""imageName"" name=""name"" required>
                <div class=""nameValidation validation"">
                    Name is required.
                </div>
            </div>
            <div class=""mb-3"">
                <label for=""imgExtension"" class=""form-label"">Choose an image</label>
                <input type=""file"" accept=""image"" class=""form-control"" id=""imgExtension"" name=""img"" required>
                <div class=""imgValidation validation"">
                    Image is required. PNG or JPEG only.
                </div>
            </div>
            <button type=""submit"" class=""btn btn-dark"">Submit</button>
        </form>
    </div>
    <h2 style=""text-align: center;"" class=""my-3"">Remove an item</h2>
    <div class=""container-fluid my-4 d-flex justify-content-center squareBox align-items-center"">
        <form action=""/api/remove/"" method=""post"" enctype=""multipart/form-data"" class=""remove card p-4 needs-validation"">
            <div class=""mb-3"">
                <label for=""id"" class=""form-label"">Image Title</label>
                <select class=""form-select"" name=""idSelect"" id=""id"" up-target=""#id"">
                    <option value="""" disabled selected>Please select an item</option>
                </select>
            </div>
            <button type=""submit"" class=""btn btn-dark"">Submit</button>
        </form>
    </div>
    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js""
        integrity=""sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz""
        crossorigin=""anonymous""></script>
    <script src=""admin.js""></script>
</body>
</html>";
    return Results.Content(htmlContent, "text/html");
});
app.MapGet("/api/items", async (IDbConnection db) =>
{

    var items = await db.QueryAsync<Item>("SELECT ID,title FROM Items");
    if (items == null)
    {
        return Results.NotFound();
    }
    var selectOptions = items.Select(item => new { Id = item.ID, Title = item.Title }).ToList();
    return Results.Ok(selectOptions);

});

app.MapGet("/api/content", async (IDbConnection db) =>
{

    var items = await db.QueryAsync<Item>("SELECT * FROM Items");

    if (items == null)
    {
        return Results.NotFound();
    }

    // Generate HTML markup for the items
    StringBuilder htmlBuilder = new StringBuilder();

    foreach (var item in items)
    {
        // Assuming you have a method to convert byte array to base64 string
        string base64Image = Convert.ToBase64String(item.Image);
        htmlBuilder.AppendFormat(
            "<div class=\"item\">" +
            "<img src=\"data:image/jpeg;base64,{0}\" alt=\"{1}\">" +
            "<div class=\"mask\">" +
            "<h2>{2}</h2>" +
            "</div>" +
            "</div>",
            base64Image,
            item.Title,
            item.Title // Assuming item.Title corresponds to the title of the image
        );

    }

    // Return the HTML markup
    return Results.Content(htmlBuilder.ToString());
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
        var allowedExtensions = new[] { ".png", ".gif", ".jpeg",".jpg" };
        var fileExtension = Path.GetExtension(img.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return Results.BadRequest("Only PNG, GIF, and JPEG images are allowed.");
        }
    }
    var item = new Item
    {
        ID = Guid.NewGuid().ToString(),
        Title = name,
        Image = ConvertIFormFileToByteArray(img)
    };
    var sql = "INSERT INTO items (ID, Title, Image) VALUES (@ID, @Title, @Image)";
    var affectedRows = await db.ExecuteAsync(sql, item);
    if(affectedRows == 0)
    {
        return Results.BadRequest();
    }
    return Results.Redirect("/admin");

});

app.MapDelete("/api/remove/{id}", async (string id, IDbConnection db) =>
{

    var affectedRows = await db.ExecuteAsync($"DELETE FROM items WHERE ID = @Id", new { Id = id });
    if(affectedRows == 0)
    {
        return Results.NotFound();
    }
    return Results.Redirect("/admin");

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



