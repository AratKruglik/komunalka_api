using KomunalkaAPI.Models;

namespace KomunalkaAPI.Data;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed data for Ukrainian regions
        if (!context.Regions.Any())
        {
            var regions = new List<Region>
            {
                new() { Name = "Вінницька область" },
                new() { Name = "Волинська область" },
                new() { Name = "Дніпропетровська область" },
                new() { Name = "Донецька область" },
                new() { Name = "Житомирська область" },
                new() { Name = "Закарпатська область" },
                new() { Name = "Запорізька область" },
                new() { Name = "Івано-Франківська область" },
                new() { Name = "Київська область" },
                new() { Name = "Кіровоградська область" },
                new() { Name = "Луганська область" },
                new() { Name = "Львівська область" },
                new() { Name = "Миколаївська область" },
                new() { Name = "Одеська область" },
                new() { Name = "Полтавська область" },
                new() { Name = "Рівненська область" },
                new() { Name = "Сумська область" },
                new() { Name = "Тернопільська область" },
                new() { Name = "Харківська область" },
                new() { Name = "Херсонська область" },
                new() { Name = "Хмельницька область" },
                new() { Name = "Черкаська область" },
                new() { Name = "Чернівецька область" },
                new() { Name = "Чернігівська область" },
                new() { Name = "Автономна Республіка Крим" },
                new() { Name = "м. Київ" }
            };

            await context.Regions.AddRangeAsync(regions);
        }

        // Seed data for address types
        if (!context.AddressTypes.Any())
        {
            var addressTypes = new List<AddressType>
            {
                new() { Name = "Квартира", Description = "Багатоквартирний будинок у місті", Icon = "BuildingOffice2Icon" },
                new() { Name = "Приватний будинок", Description = "Окрема садиба або дача", Icon = "HomeIcon" },
                new() { Name = "Офіс", Description = "Комерційне або офісне приміщення", Icon = "BuildingOfficeIcon" }
            };

            await context.AddressTypes.AddRangeAsync(addressTypes);
        }

        await context.SaveChangesAsync();
    }
}
