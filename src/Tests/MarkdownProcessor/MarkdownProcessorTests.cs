﻿using System.Collections.Generic;
using System.IO;
using MarkdownSnippets;
using Xunit;
using Xunit.Abstractions;

public class MarkdownProcessorTests :
    TestBase
{
    [Fact]
    public void Simple()
    {
        var availableSnippets = new List<Snippet>
        {
            SnippetBuild(
                language: "cs",
                key: "snippet1"
            ),
            SnippetBuild(
                language: "cs",
                key: "snippet2"
            )
        };
        var markdownContent = @"
snippet: snippet1

some text

snippet: snippet2

some other text

snippet: FileToUseAsSnippet.txt

some other text

snippet: /FileToUseAsSnippet.txt

";
        SnippetVerifier.Verify(
            markdownContent,
            availableSnippets,
            new List<string>
            {
                Path.Combine(GitRepoDirectoryFinder.FindForFilePath(), "src/Tests/FileToUseAsSnippet.txt")
            });
    }

    Snippet SnippetBuild(string language, string key)
    {
        return Snippet.Build(
            language: language,
            startLine: 1,
            endLine: 2,
            value: "Snippet",
            key: key,
            path: "thePath");
    }

    public MarkdownProcessorTests(ITestOutputHelper output) :
        base(output)
    {
    }
}