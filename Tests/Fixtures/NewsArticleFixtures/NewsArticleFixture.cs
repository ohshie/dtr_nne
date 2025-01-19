using dtr_nne.Domain.Entities;

namespace Tests.Fixtures.NewsArticleFixtures;

public class NewsArticleFixture : TheoryData<List<NewsArticle>>
{
    public NewsArticleFixture()
    {
        Add(NewsArticleFixtureBase.Articles[0]);
        Add(NewsArticleFixtureBase.Articles[1]);
    }
}