using System.Collections.Immutable;
using System.Runtime.InteropServices.ComTypes;
using GitHubMilestoneCleaner.Engines;
using Octokit;
using Shouldly;

namespace GitHubMilestoneCleaner.Tests;

public class IssueGroupEngineTests
{
    public class MockIssueWrapper(string title, int number) : IssueGroupEngine.IIssueWrapper
    {
        public string Title { get; } = title;
        public long Number { get; } = number;
        public Issue BackingIssue { get; } = new();
    }

    [Theory]
    [InlineData("Bump coverlet.msbuild from 3.1.1 to 3.1.2", "Bump coverlet.msbuild from 3.1.0 to 3.1.1")]
    [InlineData("Update github/codeql-action digest to 6a28655", "Update github/codeql-action digest to ddccb87")]
    [InlineData("Bump coverlet.msbuild from 3.1.1 to 3.1.2", "Bump coverlet.msbuild digest to ddccb87")]
    [InlineData("Bump actions/setup-dotnet from 1.9.0 to 1.9.1", "Bump actions/setup-dotnet from 1.9.1 to 2")]
    [InlineData("Bump github/codeql-action from 1 to 2", "Bump github/codeql-action from 2 to 3")]
    public void Should_group_two_bumps_of_the_same_package_into_one(string lhs, string rhs)
    {
        // given
        var sut = new IssueGroupEngine();
        var issues = new[] { lhs, rhs }
            .Select(x =>
                new MockIssueWrapper(x, 1));

        // when
        var f = sut.GroupIssues(issues).ToList();

        // then
        f.Count.ShouldBe(1);
        f[0].SubIssues.Count().ShouldBe(1);
    }

    [Theory]
    [InlineData("Bump coverlet.msbuild from 3.1.1 to 3.1.2", "Bump coverlet.msbuild to nothing at all")]
    [InlineData("Bump coverlet.msbuild from 3.1.1 to 3.1.2", "Update coverlet.msbuild from 3.1.1 to 3.1.2")]
    public void Should_NOT_group_two_bumps_of_different_packages_into_one(string lhs, string rhs)
    {
        // given
        var sut = new IssueGroupEngine();
        var issues = new[] { lhs, rhs }
            .Select(x =>
                new MockIssueWrapper(x, 1));
    
        // when
        var f = sut.GroupIssues(issues).ToList();
    
        // then
        f.Count.ShouldBe(2);
        f[0].SubIssues.Count().ShouldBe(0);
        f[1].SubIssues.Count().ShouldBe(0);
    }
}