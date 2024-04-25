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
    return Results.NoContent();

});

app.MapDelete("/api/remove/{id}", async (string id, IDbConnection db) =>
{

    var affectedRows = await db.ExecuteAsync($"DELETE FROM items WHERE ID = @Id", new { Id = id });
    if(affectedRows == 0)
    {
        return Results.NotFound();
    }
    return Results.Ok();

});

app.MapGet("/", (HttpContext context, IAntiforgery antiforgery) =>
{
    var token = antiforgery.GetAndStoreTokens(context);
    string htmlContent = $@"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>BonVoyage</title>
    <link rel=""icon"" href=""assets/software-engineer.png"" sizes=""64x64"">
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet""
        integrity=""sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH"" crossorigin=""anonymous"">
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
    <link href=""https://fonts.googleapis.com/css2?family=Caveat:wght@400..700&family=Patrick+Hand&display=swap""
        rel=""stylesheet"">
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/OwlCarousel2/2.3.4/assets/owl.carousel.min.css"">
    <link rel=""stylesheet""
        href=""https://cdnjs.cloudflare.com/ajax/libs/OwlCarousel2/2.3.4/assets/owl.theme.default.min.css"">
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js""></script>
    <link rel=""stylesheet"" href=""index.css"">
</head>

<body>
    <nav id=""mainNavbar"" class=""navbar navbar-expand-lg fs-5 sticky-top"">
        <a class=""navbar-brand"" href=""#Intro"">
            <img src=""assets/voyage.png"" alt=""logo"" width=""70"" height=""70"">
            <span class=""brand"">Bon<span style=""color: #BBBE64;"">Voyage</span></span>
        </a>
        <div class=""container-fluid justify-content-start justify-content-lg-center"">
            <button class=""navbar-toggler mb-3 mt-3"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#navbarNav""
                aria-controls=""navbarNav"" aria-expanded=""false"" aria-label=""Toggle navigation"">
                <span class=""navbar-toggler-icon""></span>
            </button>
            <div class=""collapse navbar-collapse flex-grow-0"" id=""navbarNav"">
                <ul class=""navbar-nav text-start text-lg-center"">
                    <li class=""nav-item"">
                        <a class=""nav-link"" href=""#Intro"">
                            Home
                        </a>
                    </li>
                    <li class=""nav-item"">
                        <a class=""nav-link"" href="""">
                            Destinations
                        </a>
                    </li>
                    <li class=""nav-item"">
                        <a class=""nav-link"" href="""">
                            Discover
                        </a>
                    </li>
                    <li class=""nav-item"">
                        <a class=""nav-link"" href="""">
                            About
                        </a>
                    </li>
                    <li class=""nav-item"">
                        <a class=""nav-link"" href="""">
                            Contact
                        </a>
                    </li>
                </ul>

            </div>
        </div>
        <aside class=""d-flex search-items"" role=""search"">
            <button class=""contact mt-3 mb-3"" type=""search"" placeholder=""Search"" aria-label=""Search"">
                <img src=""assets/magnifier.png"" width=""35"" height=""35"" alt="""">
            </button>
            <button class="" contact contactBar m-3"" type=""submit"">Contact</button>
        </aside>
    </nav>

    <!-- NAVBAR END -->

    <!-- INTRO -->
    <div class=""image-container mb-4"">
        <div class=""image-overlay"">
            <div class=""centered-content"">
                <span class=""headText"">Discover The World</span>
                <h3>Enjoy a wide variety of travelling services planned by experts</h3>
                <button class=""actioButton"">Find Your Adventure</button>
            </div>
        </div>
    </div>
    <!-- INTRO END -->
    <div class=""row container-fluid"">
        <div class=""col-md-12 text-center mb-4"">
            <h1 style=""font-size: 4em;"">How It Works</h1>
        </div>
    </div>

    <div class=""row container-fluid"">
        <div class=""col-md-4"">
            <div class=""text-center"">
                <img src=""assets/checklist.png"" alt=""Head Image 1"" class=""img-fluid mb-4"">
                <h2>Fill Out a Survey</h2>
                <p>Answer a few quick questions about your interests + travel history. Give us all the details!</p>
            </div>
        </div>
        <div class=""col-md-4"">
            <div class=""text-center"">
                <img src=""assets/budget.png"" alt=""Head Image 2"" class=""img-fluid mb-4"">
                <h2>Calculate Your Budget</h2>
                <p>Tell us how much you want to spend. We’ll plan a curated Surprise Trip just for you!</p>
            </div>
        </div>
        <div class=""col-md-4"">
            <div class=""text-center"">
                <img src=""assets/travel-luggage.png"" alt=""Head Image 2"" class=""img-fluid mb-4"">
                <h2>Pack Your Bags</h2>
                <p>Ready to go? After your survey and budget, we'll handle the rest. Get set for your adventure!</p>
            </div>
        </div>
    </div>
    <!-- TOP DESTINATIONS -->
    <div class=""row container-fluid"">
        <div class=""col-md-12 text-center mb-4"">
            <h1 style=""font-size: 4em;"">Top Destinations</h1>
        </div>
    </div>
    <div class=""row container-fluid carouselContainer"" hx-get=""/api/content"" hx-trigger=""load"" hx-target="".owl"">
        <div class=""owl-carousel owl owl-theme"">
            <!-- -->
        </div>
    </div>
    <!-- TOP DESTINATIONS -->

    <!-- REVIEWS -->
    <div class=""row container-fluid"">
        <div class=""owl-carousel owl1"">
            <div class=""review-item text-center"">
                <h2 style=""font-size: 3em;"">“1st Trip with BonVoyage”
                </h2>
                <p class=""reviewText"">First trip with BonVoyage was beyond expectations! From seamless planning to
                    unforgettable experiences, they truly know how to make every moment special. Can't wait for the
                    next
                    adventure!</p>
                <div class=""star-icons"">
                    <!-- Embedding 5-star icons -->
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                </div>
                <span>John Doe</span>
                <p>20th April, 202X</p>
            </div>

            <div class=""review-item text-center"">
                <h2 style=""font-size: 3em;"">“2nd Trip with BonVoyage”
                </h2>
                <p class=""reviewText"">First trip with BonVoyage was beyond expectations! From seamless planning to
                    unforgettable experiences, they truly know how to make every moment special. Can't wait for the
                    next
                    adventure!</p>
                <div class=""star-icons"">
                    <!-- Embedding 5-star icons -->
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                    <img src=""assets/star.png"" class=""star-icon"" alt=""Star"">
                </div>
                <span>John Doe</span>
                <p>20th April, 202X</p>
            </div>
            <!-- Add more review items as needed -->
        </div>
    </div>
    <!-- REVIEWS END -->

    <!-- ADMIN -->
    <div class=""accordion"" id=""accordionExample"">
        <div class=""accordion-item"">
          <div class=""accordion-header d-flex justify-content-center"" id=""headingOne"">
            <button style=""font-size: 1.8rem;"" class=""accordion-button collapsed"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#collapseOne"" aria-expanded=""true"" aria-controls=""collapseOne"">
              Admin
            </button>
          </div>
          <div id=""collapseOne"" class=""accordion-collapse collapse"" aria-labelledby=""headingOne"" data-bs-parent=""#accordionExample"">
            <div class=""accordion-body"">
              <div class=""card-group"">
                <div class=""card"">
                  <div class=""card-body"">
                    <h5 class=""card-title text-center"">Add Item</h5>
                    <form action=""/api/insert"" method=""post"" enctype=""multipart/form-data"" class=""p-4 needs-validation"">
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
                </div>


                <div class=""card"">
                  <div class=""card-body"">
                    <h5 class=""card-title text-center"">Remove Item</h5>
                    <form action=""/api/remove/"" method=""post"" enctype=""multipart/form-data"" class=""remove p-4 needs-validation"" hx-get=""/api/items"" hx-target=""#id"" hx-trigger=""mouseenter"">
                        <div class=""mb-3"">
                            <label for=""id"" class=""form-label"">Image Title</label>
                            <select class=""form-select"" name=""idSelect"" id=""id"">
                                <option value="""" disabled selected>Please select an item</option>
                            </select>
                        </div>
                        <button type=""submit"" class=""btn btn-dark"">Submit</button>
                    </form>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    <!-- FOOTER -->
    <footer>
        <div class=""footer"" id=""footer"">
        </div>
        <div class=""footerBottom d-flex justify-content-center"">
            <p>Copyright &copy;2024 Designed by <span class=""designer"">ALI ABDELGHANI</span> </p>
        </div>
    </footer>
    </div>

    <!-- scripts -->
    <script src=""https://kit.fontawesome.com/758f51ee4f.js"" crossorigin=""anonymous""></script>
    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js""
        integrity=""sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz""
        crossorigin=""anonymous""></script>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/OwlCarousel2/2.3.4/owl.carousel.min.js""></script>
    <script src=""https://unpkg.com/htmx.org@1.9.12""
        integrity=""sha384-ujb1lZYygJmzgSwoxRggbCHcjc0rB2XoQrxeTUQyRjrOnlCoYta87iKBWq3EsdM2""
        crossorigin=""anonymous""></script>
    <script src=""index.js""></script>
</body>

</html>
    ";
    return Results.Content(htmlContent, "text/html");
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



