using Bogus;
using dtr_nne.Domain.Entities;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Fixtures.NewsArticleFixtures;

public class NewsArticleFixtureBase
{
    private static readonly Faker Faker = new();

    public static readonly List<List<NewsArticle>> Articles =
    [
        [
            new NewsArticle()
            {
                Id = Faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = Faker.Random.Int(600),
                    Body = Faker.Lorem.Paragraphs(1),
                    Copyright = [Faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = Faker.Lorem.Sentence(),
                        TranslatedHeadline = Faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(Faker.Internet.UrlWithPath())],
                    Source = Faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(Faker.Internet.UrlWithPath())
            }
        ],
        [
            new NewsArticle()
            {
                Id = Faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = Faker.Random.Int(600),
                    Body = Faker.Lorem.Paragraphs(1),
                    Copyright = [Faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = Faker.Lorem.Sentence(),
                        TranslatedHeadline = Faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(Faker.Internet.UrlWithPath())],
                    Source = Faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(Faker.Internet.UrlWithPath())
            },
            new NewsArticle()
            {
                Id = Faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = Faker.Random.Int(600),
                    Body = Faker.Lorem.Paragraphs(1),
                    Copyright = [Faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = Faker.Lorem.Sentence(),
                        TranslatedHeadline = Faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(Faker.Internet.UrlWithPath())],
                    Source = Faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(Faker.Internet.UrlWithPath())
            },
            new NewsArticle()
            {
                Id = Faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = Faker.Random.Int(600),
                    Body = Faker.Lorem.Paragraphs(1),
                    Copyright = [Faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = Faker.Lorem.Sentence(),
                        TranslatedHeadline = Faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(Faker.Internet.UrlWithPath())],
                    Source = Faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(Faker.Internet.UrlWithPath())
            },
        ]
    ];
}