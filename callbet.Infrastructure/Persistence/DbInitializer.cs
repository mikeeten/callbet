using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using callbet.Domain.Entities;

namespace callbet.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedLocationDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CallbetDbContext>();

            // Automatically apply any pending EF Core migrations
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            var locationData = new Dictionary<string, List<string>>
            {
                ["Addis Ketema"] = new()
                {
                    "Mercato Area (General)",
                    "Autobus Tera",
                    "Atekelet Tera",
                    "Berbere Berenda",
                    "Bomb Tera",
                    "Chew Berenda",
                    "Dubai Tera",
                    "Goma Tera",
                    "Kibe Tera",
                    "Menalesh Tera",
                    "Mesalemiya",
                    "Military Tera",
                    "Satin Tera",
                    "Shema Tera",
                    "Ayer Marefiya",
                    "Dildiyu",
                    "Enqulal Faberika",
                    "Gofa Sefer",
                    "Kolfe",
                    "Kuwas Meda",
                    "Sebategna"
                },
                ["Akaky Kaliti"] = new()
                {
                    "Kality Total",
                    "Kality Ring Road Area",
                    "Kality Prison Area",
                    "Drivers and Mechanics Training Center Area (Mekina Nediyoch)",
                    "Akaki Town",
                    "Berta",
                    "Furi",
                    "Tirunesh Beijing Hospital Area",
                    "Gelan Condominium",
                    "Koye Feche",
                    "Tulu Dimtu",
                    "Saris Abo",
                    "Gelangura"
                },
                ["Arada"] = new()
                {
                    "Arat Kilo",
                    "Amist Kilo",
                    "Sidist Kilo",
                    "Piassa (Piazza)",
                    "Eri Bekentu",
                    "Doro Manekiya",
                    "Dejazmach Wube Sefer",
                    "Sebara Babile",
                    "Mahmoud Musik Bet",
                    "Yohannes",
                    "Gedam Sefer",
                    "Habte Giorgis",
                    "Ras Mekonnen Sefer",
                    "Afincho Ber",
                    "Basha Wolde Chilot",
                    "Abakoran Sefer",
                    "Jan Meda"
                },
                ["Bole"] = new()
                {
                    "Bole Medhanialem",
                    "Atlas",
                    "Rwanda",
                    "Japan",
                    "Mickey Leland",
                    "Wolo Sefer",
                    "Haya Hulet Mazoria",
                    "Gerji",
                    "Gerji Mebrat Hail",
                    "Gerji Condominium",
                    "Imperial",
                    "Bole Bulbula",
                    "Bole Michael",
                    "Bole Homes",
                    "Bole Beshale",
                    "Kotebe"
                },
                ["Gullele"] = new()
                {
                    "Entoto (Intoto)",
                    "Shiro Meda",
                    "Kuskuam",
                    "Kechene",
                    "Addisu Gebeya",
                    "Semen Mazegia",
                    "Shegole",
                    "Asko",
                    "Gulele Bota",
                    "Rufael",
                    "Petros We Paulos",
                    "Minilik Hospital Area",
                    "Medhanialem (Gullele Medhanialem)"
                },
                ["Kirkos"] = new()
                {
                    "Kazanchis",
                    "Meskel Square / Estifanos",
                    "Legahar (La Gare)",
                    "Bambis",
                    "Beherawi",
                    "African Union (AU) Area",
                    "Gotera",
                    "Meshuwalekiya",
                    "Lancha",
                    "Bekelo Bet",
                    "Riche",
                    "Bulgariya Mazoriya",
                    "Cherkos",
                    "Kera",
                    "Sarbet",
                    "Wabi Shebele",
                    "Urael"
                },
                ["Kolfe Keranio"] = new()
                {
                    "Ayertena",
                    "Betel (Bethel)",
                    "Alem Bank",
                    "Keranio",
                    "Alert",
                    "Zenebework",
                    "Tor Hailoch",
                    "Kara Kore",
                    "Repi",
                    "Asko",
                    "Welete",
                    "Lukanda"
                },
                ["Lemi Kura"] = new()
                {
                    "Ayat",
                    "Summit",
                    "Arabsa",
                    "Hayat",
                    "Goro",
                    "Meri / Meri Luke",
                    "CMC",
                    "Tafo / Leg Taffo",
                    "Bole Beshale"
                },
                ["Lideta"] = new()
                {
                    "Mexico",
                    "Abnet",
                    "Tor Hailoch",
                    "Sengatera",
                    "Coca",
                    "Darmar",
                    "Geja Sefer",
                    "Molla Maru",
                    "Sost Kuter Mazoria",
                    "Lideta Condominium",
                    "Tsebel"
                },
                ["Nifas Silk-Lafto"] = new()
                {
                    "Lafto",
                    "Lebu",
                    "Lebu Mebrathayil",
                    "Lebu Muzika Bet",
                    "Jemo",
                    "Jemo 1",
                    "Jemo 2",
                    "Jemo 3",
                    "Jemo Michael",
                    "Mekanisa",
                    "Mekanisa Abo",
                    "Kore",
                    "Saris",
                    "Saris Abo",
                    "Saris Adey Abeba",
                    "Sarbet",
                    "Kera",
                    "Gofa (Gofa Sefer / Gofa Mebrat Hail)",
                    "Bisantion",
                    "Haile Garment",
                    "Vatican"
                },
                ["Yeka"] = new()
                {
                    "Megenagna",
                    "Shola",
                    "Urael",
                    "Haya Hulet Mazoria",
                    "Ferensay Legasion",
                    "Kebena",
                    "Aware",
                    "Signal",
                    "Beka",
                    "Kotebe",
                    "Kara Alo",
                    "Yeka Abado",
                    "CMC"
                }
            };

            foreach (var (subCityName, neighborhoods) in locationData)
            {
                var subCity = await context.SubCities
                    .Include(s => s.Neighborhoods)
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == subCityName.ToLower());

                if (subCity == null)
                {
                    subCity = new SubCity
                    {
                        Name = subCityName,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.SubCities.Add(subCity);
                    await context.SaveChangesAsync();
                }

                foreach (var name in neighborhoods)
                {
                    if (!subCity.Neighborhoods.Any(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Neighborhoods.Add(new Neighborhood
                        {
                            SubCityId = subCity.Id,
                            Name = name,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
