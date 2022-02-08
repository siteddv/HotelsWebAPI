var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));

var app = builder.Build();

if (app.Environment.IsDevelopment()){}

{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}

var hotels = new List<Hotel>();

app.MapGet("/hotels", async (HotelDbContext dbContext) => await dbContext.Hotels.To);
app.MapGet("/hotels/{id}", (int id) => hotels.FirstOrDefault(h => h.Id == id));
app.MapPost("/hotels", (Hotel hotel) => hotels.Add(hotel));
app.MapPut("/hotels", (Hotel hotel) =>
{
    var index = hotels.FindIndex(h => h.Id == hotel.Id);
    if (index < 0)
        throw new Exception("Not found");

    hotels[index] = hotel;
});
app.MapDelete("hotels/{id}", (int id) =>
{
    var index = hotels.FindIndex(h => h.Id == id);
    if (index < 0)
        throw new Exception("Not found");

    hotels.RemoveAt(index);
});
app.Run();

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) {}
    public DbSet<Hotel> Hotels => Set<Hotel>();
}

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}