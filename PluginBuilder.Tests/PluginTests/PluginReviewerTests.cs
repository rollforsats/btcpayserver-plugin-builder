using Dapper;
using PluginBuilder.Services;
using PluginBuilder.Util.Extensions;
using PluginBuilder.ViewModels.Admin;
using Xunit;
using Xunit.Abstractions;

namespace PluginBuilder.Tests.PluginTests;

public class PluginReviewerTests(ITestOutputHelper logs) : UnitTestBase(logs)
{
    [Fact]
    public async Task UnlinkedReviewerCanStillBeClaimedByProfileUrl()
    {
        await using var tester = Create();
        tester.ReuseDatabase = false;
        await tester.Start();
        await using var conn = await tester.GetService<DBConnectionFactory>().Open();

        const string externalProfileUrl = "https://github.com/external-reviewer";
        var externalReviewerId = await conn.CreateOrUpdatePluginReviewer(new ImportReviewViewModel
        {
            ReviewerName = "external-reviewer",
            ReviewerProfileUrl = externalProfileUrl,
            Source = ImportReviewViewModel.ImportReviewSourceEnum.Github
        });

        const string claimUserId = "claim-user";
        var claimedReviewerId = await conn.CreateOrUpdatePluginReviewer(new ImportReviewViewModel
        {
            LinkExistingUser = true,
            SelectedUserId = claimUserId,
            ReviewerName = "external-reviewer",
            ReviewerProfileUrl = externalProfileUrl
        });

        Assert.Equal(externalReviewerId, claimedReviewerId);
        Assert.Equal(claimUserId, await conn.QuerySingleAsync<string>(
            "SELECT user_id FROM plugin_reviewers WHERE id = @ReviewerId",
            new { ReviewerId = externalReviewerId }));
        Assert.Equal(1, await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM plugin_reviewers WHERE profile_url = @ProfileUrl",
            new { ProfileUrl = externalProfileUrl }));
    }
}
