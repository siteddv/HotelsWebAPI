namespace HotelsWebApi.Data
{
    public interface IHotelRepository : IDisposable
    {
        Task<List<Hotel>> GetHotelsAsync();
        Task<List<Hotel>> GetHotelsByNameAsync(string name);
        Task<List<Hotel>> GetHotelsAsync(Coordinate coordinate);
        Task<Hotel> GetHotelByIdAsync(int hotelId);
        Task InsertHotelAsync(Hotel hotel);
        Task UpdateHotelAsync(Hotel hotel);
        Task DeleteHotelAsync(int hotelId);
        Task SaveAsync();
    }
}
