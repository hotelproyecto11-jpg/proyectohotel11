using PricingMvp.Domain.Entities;
using PricingMvp.Domain.Enums;

namespace PricingMvp.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Verificar si ya hay datos
            if (context.Users.Any())
                return; // Ya hay datos, no hacer nada
            
            // 1. Crear usuarios
            var adminUser = new User
            {
                Email = "admin@pricingmvp.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FullName = "Administrador Principal",
                Role = UserRole.Admin,
                IsActive = true
            };
            
            var revenueUser = new User
            {
                Email = "revenue@pricingmvp.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Revenue123!"),
                FullName = "Manager de Revenue",
                Role = UserRole.RevenueManager,
                IsActive = true
            };
            
            context.Users.AddRange(adminUser, revenueUser);
            await context.SaveChangesAsync();
            
            // 2. Crear hotel
            var hotel = new Hotel
            {
                Name = "Hotel Paradise Cancún",
                City = "Cancún",
                State = "Quintana Roo",
                Address = "Blvd. Kukulcán Km 12.5, Zona Hotelera",
                Stars = 5,
                Description = "Hotel de lujo frente al mar"
            };
            
            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();
            
            // 3. Crear habitaciones
            var room1 = new Room
            {
                HotelId = hotel.Id,
                RoomNumber = "101",
                Type = RoomType.Single,
                BasePrice = 1500m,
                Capacity = 2,
                Quantity = 10,
                HasBalcony = true,
                HasSeaView = false,
                SquareMeters = 25
            };
            
            var room2 = new Room
            {
                HotelId = hotel.Id,
                RoomNumber = "201",
                Type = RoomType.Double,
                BasePrice = 2500m,
                Capacity = 4,
                Quantity = 15,
                HasBalcony = true,
                HasSeaView = true,
                SquareMeters = 40
            };
            
            var room3 = new Room
            {
                HotelId = hotel.Id,
                RoomNumber = "301",
                Type = RoomType.Suite,
                BasePrice = 5000m,
                Capacity = 6,
                Quantity = 5,
                HasBalcony = true,
                HasSeaView = true,
                SquareMeters = 80
            };
            
            context.Rooms.AddRange(room1, room2, room3);
            await context.SaveChangesAsync();
            
            // 4. Crear historial de precios (últimos 30 días)
            var random = new Random();
            var priceHistories = new List<PriceHistory>();
            
            foreach (var room in new[] { room1, room2, room3 })
            {
                for (int i = 30; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.Date.AddDays(-i);
                    var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || 
                                   date.DayOfWeek == DayOfWeek.Sunday;
                    
                    var occupancy = isWeekend 
                        ? random.Next(70, 95) 
                        : random.Next(50, 80);
                    
                    var price = room.BasePrice;
                    if (occupancy > 80)
                        price *= 1.10m;
                    else if (occupancy < 60)
                        price *= 0.95m;
                    
                    priceHistories.Add(new PriceHistory
                    {
                        RoomId = room.Id,
                        Date = date,
                        Price = Math.Round(price, 2),
                        OccupancyPercent = occupancy,
                        WasWeekend = isWeekend,
                        HadEvent = random.Next(0, 10) > 8 // 20% probabilidad de evento
                    });
                }
            }
            
            context.PriceHistories.AddRange(priceHistories);
            await context.SaveChangesAsync();
            
            Console.WriteLine("✅ Datos de prueba creados exitosamente");
            Console.WriteLine($"   - Usuarios: 2 (admin@pricingmvp.com / revenue@pricingmvp.com)");
            Console.WriteLine($"   - Hoteles: 1");
            Console.WriteLine($"   - Habitaciones: 3");
            Console.WriteLine($"   - Registros históricos: {priceHistories.Count}");
        }
    }
}