using HotelsWebApi;
using HotelsWebApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<HotelDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()){}
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository repository) => 
    Results.Extensions.Xml(await repository.GetHotelsAsync()))
    .Produces<List<Hotel>>()
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repository) =>
{
    var hotel = await repository.GetHotelByIdAsync(id);
    if (hotel == null)
    {
        Results.NotFound();
        return;
    }
    Results.Ok(hotel);
}).Produces<Hotel>()
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetHotelById")
    .WithTags("Getters");
    
app.MapPost("/hotels", async ([FromBody] Hotel hotel,
    IHotelRepository repository) =>
{
    await repository.InsertHotelAsync(hotel);
    await repository.SaveAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
})
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");

app.MapPut("/hotels", async ([FromBody]Hotel hotel, IHotelRepository repository) =>
{
    await repository.UpdateHotelAsync(hotel);

    await repository.SaveAsync();
    return Results.NoContent();
})
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status204NoContent)
    .WithName("UpdateHotel")
    .WithTags("Updaters");

app.MapDelete("hotels/{id}", async (int id, IHotelRepository repository) =>
{
    await repository.DeleteHotelAsync(id);
    await repository.SaveAsync();

    return Results.NoContent();
})
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.MapGet("/hotels/search/name/{query}",
        async (string query, IHotelRepository repository) =>
        {
            var hotels = await repository.GetHotelsByNameAsync(query);

            if (hotels.Any())
            {
                Results.Ok(hotels);
                return;
            }

            Results.NotFound(Array.Empty<Hotel>());
        }
    )
    .Produces<List<Hotel>>()
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotelsByName")
    .WithTags("Getters");

app.MapGet("/hotels/search/location/{coordinate}",
        async (Coordinate coordinate, IHotelRepository repository) =>
            await repository.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
                ? Results.Ok(hotels)
                : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>()
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotelsByCoordinates")
    .WithTags("Getters");

app.UseHttpsRedirection();

app.Run();