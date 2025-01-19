using Bogus;
using dtr_nne.Domain.Entities;
using Tests.Fixtures.NewsOutletFixtures;

namespace Tests.Fixtures.NewsArticleFixtures;

public class NewsArticleFixtureBase
{
    private static readonly Faker _faker = new();

    public static readonly List<List<NewsArticle>> Articles =
    [
        [
            new NewsArticle()
            {
                Id = _faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = _faker.Random.Int(600),
                    Body = _faker.Lorem.Paragraphs(1),
                    Copyright = [_faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = _faker.Lorem.Sentence(),
                        TranslatedHeadline = _faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(_faker.Internet.UrlWithPath())],
                    Source = _faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(_faker.Internet.UrlWithPath())
            }
        ],
        [
            new NewsArticle()
            {
                Id = _faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = _faker.Random.Int(600),
                    Body = _faker.Lorem.Paragraphs(1),
                    Copyright = [_faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = _faker.Lorem.Sentence(),
                        TranslatedHeadline = _faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(_faker.Internet.UrlWithPath())],
                    Source = _faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(_faker.Internet.UrlWithPath())
            },
            new NewsArticle()
            {
                Id = _faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = _faker.Random.Int(600),
                    Body = _faker.Lorem.Paragraphs(1),
                    Copyright = [_faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = _faker.Lorem.Sentence(),
                        TranslatedHeadline = _faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(_faker.Internet.UrlWithPath())],
                    Source = _faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(_faker.Internet.UrlWithPath())
            },
            new NewsArticle()
            {
                Id = _faker.Random.Int(600),
                ArticleContent = new ArticleContent()
                {
                    Id = _faker.Random.Int(600),
                    Body = _faker.Lorem.Paragraphs(1),
                    Copyright = [_faker.Lorem.Sentence()],
                    Headline = new Headline()
                    {
                        OriginalHeadline = _faker.Lorem.Sentence(),
                        TranslatedHeadline = _faker.Lorem.Sentence(),
                    },
                    Images = [new Uri(_faker.Internet.UrlWithPath())],
                    Source = _faker.Lorem.Sentence()
                },
                Error = "",
                NewsOutlet = NewsOutletFixtureBase.Outlets[0][0],
                Themes = NewsOutletFixtureBase.Outlets[0][0].Themes,
                Uri = new Uri(_faker.Internet.UrlWithPath())
            },
        ]
    ];
}