using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LinkDotNet.Blog.Domain;
using LinkDotNet.Blog.Web.Features.Home.Components;
using Xunit.Abstractions;
using Xunit.Sdk;
using IXunitSerializable = Xunit.Sdk.IXunitSerializable;
using IXunitSerializationInfo = Xunit.Sdk.IXunitSerializationInfo;

namespace LinkDotNet.Blog.UnitTests.Web.Features.Home.Components;

public class SocialAccountTests : BunitContext
{
    [Theory]
    [ClassData(typeof(SocialAccountTheoryData))]
    public void ShouldDisplaySocialAccountsOnlyWhenConfigured(
        SerializableSocial serializableSocial,
        ExpectedVisibility expectedVisibility)
    {
        var cut = Render<SocialAccounts>(s => s.Add(p => p.Social, serializableSocial.ToSocial()));

        foreach (var (iconId, shouldBeVisible) in expectedVisibility.Values)
        {
            cut.FindAll($"#{iconId}").Any().ShouldBe(
                shouldBeVisible,
                $"Social icon #{iconId} visibility should be {shouldBeVisible}");
        }
    }

    private record SocialTestCase(
        string Name,
        string? Url,
        string IconId);

    public class ExpectedVisibility : IXunitSerializable
    {
        public Dictionary<string, bool> Values { get; private set; } = [];

        // Required parameterless constructor for deserialization
        public ExpectedVisibility() { }

        public ExpectedVisibility(Dictionary<string, bool> values) => Values = values;

        public void Deserialize(IXunitSerializationInfo info)
        {
            var keys = info.GetValue<string[]>("keys");
            var values = info.GetValue<bool[]>("values");

            if (keys != null && values != null)
            {
                Values = keys.Zip(values, (k, v) => new { k, v })
                            .ToDictionary(x => x.k, x => x.v);
            }
            else
            {
                Values = [];
            }
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("keys", Values.Keys.ToArray());
            info.AddValue("values", Values.Values.ToArray());
        }
    }

    public class SerializableSocial : IXunitSerializable
    {
        public string? LinkedInAccountUrl { get; set; }
        public string? GithubAccountUrl { get; set; }
        public string? TwitterAccountUrl { get; set; }
        public string? YoutubeAccountUrl { get; set; }
        public string? BlueSkyHandle { get; set; }

        // Required parameterless constructor for deserialization
        public SerializableSocial() { }

        public SerializableSocial(Social social)
        {
            LinkedInAccountUrl = social.LinkedInAccountUrl;
            GithubAccountUrl = social.GithubAccountUrl;
            TwitterAccountUrl = social.TwitterAccountUrl;
            YoutubeAccountUrl = social.YoutubeAccountUrl;
            BlueSkyHandle = social.BlueSkyHandle;
        }

        public Social ToSocial() => new()
        {
            LinkedInAccountUrl = LinkedInAccountUrl,
            GithubAccountUrl = GithubAccountUrl,
            TwitterAccountUrl = TwitterAccountUrl,
            YoutubeAccountUrl = YoutubeAccountUrl,
            BlueSkyHandle = BlueSkyHandle
        };

        public void Deserialize(IXunitSerializationInfo info)
        {
            LinkedInAccountUrl = info.GetValue<string?>("LinkedInAccountUrl");
            GithubAccountUrl = info.GetValue<string?>("GithubAccountUrl");
            TwitterAccountUrl = info.GetValue<string?>("TwitterAccountUrl");
            YoutubeAccountUrl = info.GetValue<string?>("YoutubeAccountUrl");
            BlueSkyHandle = info.GetValue<string?>("BlueSkyHandle");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("LinkedInAccountUrl", LinkedInAccountUrl);
            info.AddValue("GithubAccountUrl", GithubAccountUrl);
            info.AddValue("TwitterAccountUrl", TwitterAccountUrl);
            info.AddValue("YoutubeAccountUrl", YoutubeAccountUrl);
            info.AddValue("BlueSkyHandle", BlueSkyHandle);
        }
    }

    private class SocialAccountTheoryData : TheoryData<SerializableSocial, ExpectedVisibility>
    {
        [SuppressMessage("Design", "S1144: Unused private types or members should be removed", Justification = "Used by xUnit")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Needed to justify xUnit")]
        public SocialAccountTheoryData()
        {
            var testCases = new[]
            {
                new SocialTestCase("LinkedIn", "https://linkedin.com", "linkedin"),
                new SocialTestCase("GitHub", "https://github.com", "github"),
                new SocialTestCase("Twitter", "https://twitter.com", "twitter"),
                new SocialTestCase("YouTube", "https://youtube.com", "youtube"),
                new SocialTestCase("BlueSky", "https://bsky.app", "bluesky")
            };

            Add(new SerializableSocial(new Social()), new ExpectedVisibility(CreateExpectedResults(testCases, null)));

            foreach (var testCase in testCases)
            {
                Add(
                    new SerializableSocial(CreateSocial(testCase)),
                    new ExpectedVisibility(CreateExpectedResults(testCases, testCase.Name)));
            }
        }

        private static Social CreateSocial(SocialTestCase activeAccount) => new()
        {
            LinkedInAccountUrl = activeAccount.Name == "LinkedIn" ? activeAccount.Url : null,
            GithubAccountUrl = activeAccount.Name == "GitHub" ? activeAccount.Url : null,
            TwitterAccountUrl = activeAccount.Name == "Twitter" ? activeAccount.Url : null,
            YoutubeAccountUrl = activeAccount.Name == "YouTube" ? activeAccount.Url : null,
            BlueSkyHandle = activeAccount.Name == "BlueSky" ? activeAccount.Url : null
        };

        private static Dictionary<string, bool> CreateExpectedResults(
            IEnumerable<SocialTestCase> allCases,
            string? activeAccountName) => allCases.ToDictionary(
            tc => tc.IconId,
            tc => tc.Name == activeAccountName);
    }
}
