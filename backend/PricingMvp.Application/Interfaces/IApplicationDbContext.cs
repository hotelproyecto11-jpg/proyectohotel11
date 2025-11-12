using Microsoft.EntityFrameworkCore;
using PricingMvp.Domain.Entities;

namespace PricingMvp.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Hotel> Hotels { get; }
        DbSet<Room> Rooms { get; }
        DbSet<PriceHistory> PriceHistories { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}