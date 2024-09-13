using dtr_nne.Domain.Entities;
using dtr_nne.Domain.UnitOfWork;
using dtr_nne.Infrastructure.Context;
using dtr_nne.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Fixtures;

internal class NewsOutletFixture
{
    public static List<NewsOutlet> GetTestNewsOutlet() =>
    [
        new NewsOutlet
        {
            Id = Faker.RandomNumber.Next(300),
            Name = "arkeonews.net",
            Website = new Uri("https://arkeonews.net/"),
            MainPagePassword = "{ \"links\": \"li div.left a @href\" }",
            NewsPassword =
                "{ \"header\": \"h1.article-title\", \"body\": \"div.clearfix > p\", \"images\": \"img.attachment-post-thumbnail @src\", \"copyright\": \"\", \"source\": \"\" }",
            AlwaysJs = Faker.Boolean.Random(),
            InUse = Faker.Boolean.Random()
        },

        new NewsOutlet
        {
            Id = Faker.RandomNumber.Next(300),
            Name = "eurekalert.org",
            Website = new Uri("https://eurekalert.org/"),
            MainPagePassword = "{ \"links\": \"article.trending a @href\" }",
            NewsPassword =
                "{ \"header\": \"h1.page_title\", \"body\": \"div.entry > p\", \"images\": \"div.img-wrapper img @src\", \"copyright\": \"p.credit\", \"source\": \"div.well a @href\" }",
            AlwaysJs = Faker.Boolean.Random(),
            InUse = Faker.Boolean.Random()
        },

        new NewsOutlet
        {
            Id = Faker.RandomNumber.Next(300),
            Name = "techxplore.com/sort/date/12h/",
            Website = new Uri("https://techxplore.com/sort/date/12h/"),
            MainPagePassword = "{ \"links\": \"a.news-link @href\" }",
            NewsPassword =
                "{n  \"header\": \"h1.text-extra-large\",n  \"body\": \".article-main > p:not(.article-main__more)\",n  \"images\": \"figure.article-img img @src\",n  \"copyright\": \"figure.article-img figcaption\",n  \"source\": \"p.article-main__note a @href\"n}",
            AlwaysJs = Faker.Boolean.Random(),
            InUse = Faker.Boolean.Random()
        }

    ];

    public (GenericRepository<NewsOutlet, NneDbContext>, NneDbContext) ProvideEmptyRepository()
    {
        var options = new DbContextOptionsBuilder<NneDbContext>()
            .UseInMemoryDatabase(databaseName: "testDb")
            .Options;
        var context = new NneDbContext(options);
        
        var mockLogger = new Mock<ILogger<GenericRepository<NewsOutlet, NneDbContext>>>();
        
        var mockUoW = new Mock<IUnitOfWork<NneDbContext>>();
        mockUoW.Setup(uow => uow.Context).Returns(context);
        
        var newsOutletRepository = new GenericRepository<NewsOutlet, NneDbContext>
        (
            logger: mockLogger.Object, 
            unitOfWork: mockUoW.Object
        );

        return (newsOutletRepository, context);
    }
}