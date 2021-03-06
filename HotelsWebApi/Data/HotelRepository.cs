namespace HotelsWebApi.Data;

public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public HotelRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<Hotel>> GetHotelsAsync() => await _context.Hotels.ToListAsync();

    public async Task<List<Hotel>> GetHotelsByNameAsync(string name) =>
        await _context.Hotels.Where(hotel => hotel.Name == name).ToListAsync();

    public async Task<List<Hotel>> GetHotelsAsync(Coordinate coordinate) =>
        await _context.Hotels.Where(hotel =>
            hotel.Latitude > coordinate.Latitude - 1 &&
            hotel.Latitude < coordinate.Latitude + 1 &&
            hotel.Longitude > coordinate.Longitude - 1 &&
            hotel.Longitude < coordinate.Longitude + 1
        ).ToListAsync();
    
    public async Task<Hotel> GetHotelByIdAsync(int hotelId) => await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId);

    public Task InsertHotelAsync(Hotel hotel) => Task.FromResult(_context.Hotels.Add(hotel));

    public async Task UpdateHotelAsync(Hotel hotel)
    {
        var hotelFromDb = await _context.Hotels.FindAsync(new object[] { hotel.Id });
        if (hotelFromDb == null) return;

        hotelFromDb.Latitude = hotel.Latitude;
        hotelFromDb.Longitude = hotel.Longitude;
        hotelFromDb.Name = hotel.Name;
    }

    public async Task DeleteHotelAsync(int hotelId)
    {
        var hotelFromDb = await _context.Hotels.FindAsync(new object[] { hotelId });
        if (hotelFromDb == null) return;

        _context.Hotels.Remove(hotelFromDb);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();

    private bool _isDisposed = false;
    public virtual void Dispose(bool disposing)
    {
        if (!_isDisposed && disposing)
            _context.Dispose();

        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}