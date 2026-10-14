using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Portfolio.E2E.Tests;

[NonParallelizable]
public sealed class PortfolioFlowTests : PageTest
{
    private static E2EApplicationHost? _application;

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = E2EApplicationHost.ClientBaseUrl,
        IgnoreHTTPSErrors = true
    };

    [OneTimeSetUp]
    public async Task StartApplication()
    {
        _application = new E2EApplicationHost();
        try
        {
            await _application.StartAsync();
        }
        catch
        {
            await _application.DisposeAsync();
            throw;
        }
    }

    [OneTimeTearDown]
    public async Task StopApplication()
    {
        if (_application is not null)
        {
            await _application.DisposeAsync();
            _application = null;
        }
    }

    [Test]
    public async Task Authenticated_user_can_manage_data_but_anonymous_and_other_users_cannot()
    {
        var api = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = E2EApplicationHost.ApiBaseUrl,
            IgnoreHTTPSErrors = true
        });

        try
        {
            Assert.That((await api.GetAsync("/api/portfolio-users")).Status, Is.EqualTo(401));
            Assert.That((await api.PostAsync("/api/projects?portfolioUserId=1", new APIRequestContextOptions
            {
                DataObject = ProjectBody("Anonymous project")
            })).Status, Is.EqualTo(401));
            Assert.That((await api.PutAsync("/api/projects/1", new APIRequestContextOptions
            {
                DataObject = ProjectBody("Anonymous edit")
            })).Status, Is.EqualTo(401));
            Assert.That((await api.DeleteAsync("/api/projects/1")).Status, Is.EqualTo(401));

            var email = $"playwright-{Guid.NewGuid():N}@example.test";
            const string password = "PlaywrightPass123";

            await Page.GotoAsync("/");
            await Page.GetByLabel("EMAIL").FillAsync(email);
            await Page.GetByLabel("PASSWORD").FillAsync(password);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();

            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Sign out" })).ToBeVisibleAsync();
            await Expect(Page.Locator(".signed-in-email")).ToHaveTextAsync(email);

            await Page.GetByRole(AriaRole.Button, new() { Name = "Create portfolio" }).ClickAsync();
            await Page.GetByLabel("FULL NAME").FillAsync("Playwright Owner");
            await Page.GetByLabel("EMAIL").FillAsync($"portfolio-{Guid.NewGuid():N}@example.test");
            await Page.GetByLabel("SKILLS COMMA SEPARATED").FillAsync("Playwright, SQLite");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save profile" }).ClickAsync();

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Playwright Owner" })).ToBeVisibleAsync();
            await Expect(Page.Locator(".skill-tag")).ToHaveTextAsync(new[] { "Playwright", "SQLite" });

            await Page.GetByRole(AriaRole.Button, new() { Name = "New project" }).ClickAsync();
            await Page.GetByLabel("PROJECT TITLE").FillAsync("NUnit E2E project");
            await Page.GetByLabel("DESCRIPTION").FillAsync("Created by the Playwright .NET test");
            await Page.GetByLabel("PROJECT URL").FillAsync("https://example.test/nunit-project");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save project" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "NUnit E2E project" })).ToBeVisibleAsync();

            var token = await Page.EvaluateAsync<string?>("() => sessionStorage.getItem('portfolio.access-token')");
            Assert.That(token, Is.Not.Null.And.Not.Empty);
            var ownerHeaders = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };
            var profileResponse = await api.GetAsync("/api/portfolio-users", new APIRequestContextOptions { Headers = ownerHeaders });
            Assert.That(profileResponse.Ok, Is.True);
            using var profileJson = JsonDocument.Parse(await profileResponse.TextAsync());
            var profileId = profileJson.RootElement[0].GetProperty("id").GetInt32();

            var projectsResponse = await api.GetAsync(
                $"/api/projects?portfolioUserId={profileId}",
                new APIRequestContextOptions { Headers = ownerHeaders });
            Assert.That(projectsResponse.Ok, Is.True);
            using var projectsJson = JsonDocument.Parse(await projectsResponse.TextAsync());
            var projectId = projectsJson.RootElement[0].GetProperty("id").GetInt32();

            await Page.GetByRole(AriaRole.Button, new() { Name = "Edit NUnit E2E project" }).ClickAsync();
            await Page.GetByLabel("PROJECT TITLE").FillAsync("Updated NUnit E2E project");
            await Page.GetByLabel("STATUS").SelectOptionAsync(new SelectOptionValue { Label = "Completed" });
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save project" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Updated NUnit E2E project" })).ToBeVisibleAsync();
            await Expect(Page.GetByText("Completed", new() { Exact = true })).ToBeVisibleAsync();

            await Page.GetByRole(AriaRole.Button, new() { Name = "Edit profile" }).ClickAsync();
            await Page.GetByLabel("SKILLS COMMA SEPARATED").FillAsync("Playwright");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save profile" }).ClickAsync();
            await Expect(Page.Locator(".skill-tag")).ToHaveTextAsync(new[] { "Playwright" });

            var otherEmail = $"playwright-other-{Guid.NewGuid():N}@example.test";
            var otherRegistration = await api.PostAsync("/api/identity/register", new APIRequestContextOptions
            {
                DataObject = new { email = otherEmail, password = "OtherPlaywright123" }
            });
            Assert.That(otherRegistration.Status, Is.EqualTo(201));
            using var otherJson = JsonDocument.Parse(await otherRegistration.TextAsync());
            var otherToken = otherJson.RootElement.GetProperty("accessToken").GetString();
            var otherHeaders = new Dictionary<string, string> { ["Authorization"] = $"Bearer {otherToken}" };

            var otherProfiles = await api.GetAsync(
                "/api/portfolio-users",
                new APIRequestContextOptions { Headers = otherHeaders });
            Assert.That(await otherProfiles.TextAsync(), Is.EqualTo("[]"));

            var crossAccountUpdate = await api.PutAsync($"/api/projects/{projectId}", new APIRequestContextOptions
            {
                Headers = otherHeaders,
                DataObject = ProjectBody("Cross-account edit")
            });
            Assert.That(crossAccountUpdate.Status, Is.EqualTo(404));

            var crossAccountDelete = await api.DeleteAsync($"/api/projects/{projectId}", new APIRequestContextOptions
            {
                Headers = otherHeaders
            });
            Assert.That(crossAccountDelete.Status, Is.EqualTo(404));

            Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Updated NUnit E2E project" }).ClickAsync();
            await Expect(Page.GetByText("Project deleted.")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Updated NUnit E2E project" })).Not.ToBeVisibleAsync();

            await Page.GetByRole(AriaRole.Button, new() { Name = "Sign out" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create account" })).ToBeVisibleAsync();
            Assert.That((await api.DeleteAsync($"/api/projects/{projectId}")).Status, Is.EqualTo(401));

            var anonymousCreate = await api.PostAsync($"/api/projects?portfolioUserId={profileId}", new APIRequestContextOptions
            {
                DataObject = ProjectBody("Anonymous create")
            });
            Assert.That(anonymousCreate.Status, Is.EqualTo(401));

            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            await Page.GetByLabel("EMAIL").FillAsync(email);
            await Page.GetByLabel("PASSWORD").FillAsync(password);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
            await Expect(Page.Locator(".signed-in-email")).ToHaveTextAsync(email);
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Playwright Owner" })).ToBeVisibleAsync();
        }
        finally
        {
            await api.DisposeAsync();
        }
    }

    private static object ProjectBody(string title) => new
    {
        title,
        description = "",
        projectUrl = "https://example.test/authorized-project",
        status = "Planned"
    };
}