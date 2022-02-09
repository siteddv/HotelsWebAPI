using HotelsWebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

var app = builder.Build();

if (app.Environment.IsDevelopment()){}

{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}

var hotels = new List<Hotel>();

app.MapGet("/hotels", async (HotelDbContext dbContext) => await dbContext.Hotels.ToListAsync());

app.MapGet("/hotels/{id}", async (int id, HotelDbContext db) => 
    await db.Hotels.FirstOrDefaultAsync(h => h.Id == id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound());

app.MapPost("/hotels", async ([FromBody] Hotel hotel,
    HotelDbContext dbContext) =>
{
    dbContext.Hotels.Add(hotel);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
});

app.MapPut("/hotels", async ([FromBody]Hotel hotel, HotelDbContext dbContext) =>
{
    var hotelFromDb = await dbContext.Hotels.FindAsync(new object[] {hotel.Id});
    if (hotelFromDb == null) return Results.NotFound();

    hotelFromDb.Latitude = hotel.Latitude;
    hotelFromDb.Longitude = hotel.Longitude;
    hotelFromDb.Name = hotel.Name;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("hotels/{id}", async (int id, HotelDbContext dbContext) =>
{
    var hotelFromDb = await dbContext.Hotels.FindAsync(new object[] {id});
    if (hotelFromDb == null) return Results.NotFound();

    dbContext.Hotels.Remove(hotelFromDb);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();