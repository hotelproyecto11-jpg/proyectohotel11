using Microsoft.EntityFrameworkCore;
using PricingMvp.Application.Interfaces;
using PricingMvp.Domain.Entities;
using PricingMvp.Domain.Common;

namespace PricingMvp.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }
        
        public DbSet<User> Users => Set<User>();
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar relaciones
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<PriceHistory>()
                .HasOne(p => p.Room)
                .WithMany(r => r.PriceHistories)
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configurar precisión de decimales para precios
            modelBuilder.Entity<Room>()
                .Property(r => r.BasePrice)
                .HasPrecision(18, 2);
                
            modelBuilder.Entity<PriceHistory>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Evitar truncamiento silencioso para PredictedPrice (decimal nullable para ML)
            modelBuilder.Entity<PriceHistory>()
                .Property(p => p.PredictedPrice)
                .HasPrecision(18, 2);
                
            // Índices para mejorar búsquedas
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            modelBuilder.Entity<PriceHistory>()
                .HasIndex(p => p.Date);
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Actualizar timestamps automáticamente
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);
                
            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity entity)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}