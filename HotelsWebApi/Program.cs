using HotelsWebApi;
using HotelsWebApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()){}

{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository repository) => await repository.GetHotelsAsync());

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repository) => 
    await repository.GetHotelByIdAsync(id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound());

app.MapPost("/hotels", async ([FromBody] Hotel hotel,
    IHotelRepository repository) =>
{
    await repository.InsertHotelAsync(hotel);
    await repository.SaveAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
});

app.MapPut("/hotels", async ([FromBody]Hotel hotel, IHotelRepository repository) =>
{
    await repository.UpdateHotelAsync(hotel);

    await repository.SaveAsync();
    return Results.NoContent();
});

app.MapDelete("hotels/{id}", async (int id, IHotelRepository repository) =>
{
    await repository.DeleteHotelAsync(id);
    await repository.SaveAsync();

    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();