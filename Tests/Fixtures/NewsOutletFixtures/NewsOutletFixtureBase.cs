using Bogus;
using dtr_nne.Domain.Entities;

namespace Tests.Fixtures.NewsOutletFixtures;

public static class NewsOutletFixtureBase
{
    private static readonly Faker Faker = new();
    
    public static readonly List<List<NewsOutlet>> Outlets = 
    [
        [
            new NewsOutlet
                {
                    Id = Faker.Random.Int(600),
                    Name = "arkeonews.net",
                    Website = new Uri("https://arkeonews.net/"),
                    MainPagePassword = "{ \"links\": \"li div.left a @href\" }",
                    NewsPassword =
                        "{ \"header\": \"h1.article-title\", \"body\": \"div.clearfix > p\", \"images\": \"img.attachment-post-thumbnail @src\", \"copyright\": \"\", \"source\": \"\" }",
                    AlwaysJs = Faker.Random.Bool(),
                    InUse = Faker.Random.Bool(),
                    Themes = []
                }
        ], 
        [
            new NewsOutlet
                {
                    Id = Faker.Random.Int(600),
                    Name = "arkeonews.net",
                    Website = new Uri("https://arkeonews.net/"),
                    MainPagePassword = "{ \"links\": \"li div.left a @href\" }",
                    NewsPassword =
                        "{ \"header\": \"h1.article-title\", \"body\": \"div.clearfix > p\", \"images\": \"img.attachment-post-thumbnail @src\", \"copyright\": \"\", \"source\": \"\" }",
                    AlwaysJs = Faker.Random.Bool(),
                    InUse = true,
                    Themes = []
                },

            new NewsOutlet
                {
                    Id = Faker.Random.Int(600),
                    Name = "eurekalert.org",
                    Website = new Uri("https://eurekalert.org/"),
                    MainPagePassword = "{ \"links\": \"article.trending a @href\" }",
                    NewsPassword =
                        "{ \"header\": \"h1.page_title\", \"body\": \"div.entry > p\", \"images\": \"div.img-wrapper img @src\", \"copyright\": \"p.credit\", \"source\": \"div.well a @href\" }",
                    AlwaysJs = Faker.Random.Bool(),
                    InUse = Faker.Random.Bool(),
                    Themes = []
                },

            new NewsOutlet
                {
                    Id = Faker.Random.Int(600),
                    Name = "techxplore.com/sort/date/12h/",
                    Website = new Uri("https://techxplore.com/sort/date/12h/"),
                    MainPagePassword = "{ \"links\": \"a.news-link @href\" }",
                    NewsPassword =
                        "{n  \"header\": \"h1.text-extra-large\",n  \"body\": \".article-main > p:not(.article-main__more)\",n  \"images\": \"figure.article-img img @src\",n  \"copyright\": \"figure.article-img figcaption\",n  \"source\": \"p.article-main__note a @href\"n}",
                    AlwaysJs = Faker.Random.Bool(),
                    InUse = Faker.Random.Bool(),
                    Themes = []
                }
        ]
    ];
}