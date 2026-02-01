using Microsoft.EntityFrameworkCore;
using SimpleBlog.Blog.Services;
using SimpleBlog.ApiService;

namespace SimpleBlog.ApiService.Seeding;

public static class BlogSeeder
{
    public static async Task SeedAsync(BlogDbContext db, ILogger logger)
    {
        logger.LogInformation("Seeding blog data...");

        // Seed posts if none exist
        if (!await db.Posts.AnyAsync())
        {
            var posts = new[]
        {
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Witaj w naszej pracowni!",
                Content = "Witaj w SimpleBlog - miejscu, gdzie ręczna praca spotyka się z pasją do mody. Każdy element naszej kolekcji jest szyty ręcznie z najwyższą starannością. Zapraszamy do odkrywania naszych unikalnych kreacji!",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = "user",
                        Content = "Piękne ubrania! Czy macie też sukienki letnie?",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-9)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = SeedDataConstants.AdminUsername,
                        Content = "Dziękujemy! Tak, nasza kolekcja letnich sukienek jest już dostępna w sklepie.",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-8)
                    }
                }
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Filozofia ręcznej roboty",
                Content = "Każdy element naszej odzieży powstaje ręcznie - od doboru materiałów, przez projektowanie, aż po ostatni ścieg. Wierzymy, że ręcznie robione ubrania mają duszę i charakteryzują się niepowtarzalną jakością, której nie da się zastąpić masową produkcją.",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = "user",
                        Content = "Z jakich materiałów szyjecie?",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-6)
                    }
                }
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Wybór materiałów - bawełna i len",
                Content = "Stawiamy na naturalne materiały najwyższej jakości. Nasza kolekcja to głównie bawełna organiczna i len - materiały przyjazne dla skóry, oddychające i trwałe. Wszystkie tkaniny kupujemy od sprawdzonych dostawców z certyfikatami.",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>()
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Proces powstawania sukienki",
                Content = "Zapraszamy za kulisy! Każda sukienka przechodzi przez kilka etapów: projektowanie, dobór materiału, wstępny wykrój, szycie, przymiarki i finalne wykończenie. Cały proces trwa średnio 2-3 tygodnie, aby zapewnić najwyższą jakość.",
                Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = "user",
                        Content = "Czy możliwe są indywidualne zamówienia?",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = SeedDataConstants.AdminUsername,
                        Content = "Tak! Chętnie realizujemy zamówienia szyte na miarę. Skontaktuj się z nami przez formularz.",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                    }
                }
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Trendy wiosna/lato 2026",
                Content = "W nadchodzącym sezonie stawiamy na lekkość i naturalność. W naszej kolekcji dominują pastelowe kolory, luźne kroje i przewiewne tkaniny. Przygotowaliśmy dla Was unikalne sukienki maxi, zwiewne spódnice i bawełniane topy.",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>()
            }
        };

            await db.Posts.AddRangeAsync(posts);
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} blog posts with comments", posts.Length);
        }

        // Seed AboutMe if missing
        if (!await db.AboutMe.AnyAsync())
        {
            var about = new AboutMeEntity
            {
                Id = Guid.NewGuid(),
                Content = "Witaj w SimpleBlog! Jestem pasjonatką mody i szycia. Od lat tworzę unikalne, ręcznie szyte ubrania, które łączą wygodę, styl i wysoką jakość. Każda kreacja jest tworzona z myślą o Tobie - indywidualnie, z pasją i dbałością o każdy detal. Zapraszam do odkrywania mojej kolekcji!",
                ImageUrl = null,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = SeedDataConstants.SystemUsername
            };

            await db.AboutMe.AddAsync(about);
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded AboutMe content");
        }

        // Seed SiteSettings if missing
        if (!await db.SiteSettings.AnyAsync())
        {
            var settings = new SiteSettingsEntity
            {
                Id = Guid.NewGuid(),
                Theme = "light",
                LogoUrl = null,
                ContactText = "Masz pytania o ręcznie robione ubrania? Napisz do nas — odpowiadamy szybko i z przyjemnością doradzimy.",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = SeedDataConstants.SystemUsername
            };

            await db.SiteSettings.AddAsync(settings);
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded SiteSettings with default theme");
        }
    }
}
