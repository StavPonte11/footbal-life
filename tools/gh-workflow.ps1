# ==============================================================================
# Football Life - GitHub Automation CLI for Agents & CI
# ==============================================================================
# Provides automated GitHub operations (Issues, PRs, Code Reviews, Merges)
# using token from GITHUB_TOKEN, GITHUB_PERSONAL_ACCESS_TOKEN, or Git Credential Manager.
# ==============================================================================

[CmdletBinding()]
param(
    [Parameter(Position=0, Mandatory=$true)]
    [ValidateSet("issue-create", "issue-list", "issue-get", "issue-close", "pr-create", "pr-list", "pr-get", "pr-update", "pr-diff", "pr-review", "pr-comments", "pr-reviews", "pr-merge")]
    [string]$Command,

    [Parameter(Mandatory=$false)] [string]$Title,
    [Parameter(Mandatory=$false)] [string]$Body,
    [Parameter(Mandatory=$false)] [string[]]$Labels,
    [Parameter(Mandatory=$false)] [int]$Id,
    [Parameter(Mandatory=$false)] [string]$Head,
    [Parameter(Mandatory=$false)] [string]$Base = "main",
    [Parameter(Mandatory=$false)] [ValidateSet("APPROVE", "REQUEST_CHANGES", "COMMENT")] [string]$Event = "COMMENT",
    [Parameter(Mandatory=$false)] [ValidateSet("squash", "merge", "rebase")] [string]$Method = "squash",
    [Parameter(Mandatory=$false)] [string]$State = "open"
)

function Get-GitHubToken {
    if ($env:GITHUB_TOKEN) { return $env:GITHUB_TOKEN }
    if ($env:GITHUB_PERSONAL_ACCESS_TOKEN) { return $env:GITHUB_PERSONAL_ACCESS_TOKEN }
    
    $userToken = [System.Environment]::GetEnvironmentVariable("GITHUB_TOKEN", "User")
    if ($userToken) { return $userToken }

    # Fallback to Git Credential Manager
    try {
        $cred = "protocol=https`nhost=github.com`n" | git credential fill 2>$null
        $tokenMatch = ($cred | Select-String "password=(.*)").Matches.Groups[1].Value
        if ($tokenMatch) { return $tokenMatch }
    } catch {}

    throw "GitHub token could not be resolved from environment or Git Credential Manager."
}

function Get-RepoSlug {
    try {
        $url = git remote get-url origin 2>$null
        if ($url -match "github\.com[:/]([^/]+)/([^/\.]+)") {
            return "$($Matches[1])/$($Matches[2])"
        }
    } catch {}
    return "StavPonte11/footbal-life"
}

$token = Get-GitHubToken
$repo = Get-RepoSlug
$headers = @{
    "Authorization" = "Bearer $token"
    "User-Agent"    = "FootballLife-AgentWorkflow"
    "Accept"        = "application/vnd.github.v3+json"
}

switch ($Command) {
    "issue-create" {
        if (-not $Title) { throw "Title is required for issue-create." }
        $payload = @{
            title = $Title
            body  = $Body
        }
        if ($Labels) { $payload["labels"] = $Labels }
        $json = $payload | ConvertTo-Json -Depth 5
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/issues" -Method Post -Headers $headers -Body $json -ContentType "application/json; charset=utf-8" -UseBasicParsing
        [PSCustomObject]@{
            Id    = $res.number
            Title = $res.title
            Url   = $res.html_url
            State = $res.state
        } | Format-List
    }

    "issue-list" {
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/issues?state=$State" -Method Get -Headers $headers -UseBasicParsing
        $res | Where-Object { -not $_.pull_request } | Select-Object number, title, state, @{Name="labels"; Expression={($_.labels | ForEach-Object {$_.name}) -join ", "}}, html_url | Format-Table -AutoSize
    }

    "issue-get" {
        if (-not $Id) { throw "Id is required for issue-get." }
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/issues/$Id" -Method Get -Headers $headers -UseBasicParsing
        [PSCustomObject]@{
            Id     = $res.number
            Title  = $res.title
            State  = $res.state
            Labels = ($res.labels | ForEach-Object { $_.name }) -join ", "
            Url    = $res.html_url
            Body   = $res.body
        } | Format-List
    }

    "issue-close" {
        if (-not $Id) { throw "Id is required for issue-close." }
        if ($Body) {
            $commentPayload = @{ body = $Body } | ConvertTo-Json
            Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/issues/$Id/comments" -Method Post -Headers $headers -Body $commentPayload -ContentType "application/json; charset=utf-8" -UseBasicParsing | Out-Null
        }
        $payload = @{ state = "closed"; state_reason = "completed" } | ConvertTo-Json
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/issues/$Id" -Method Patch -Headers $headers -Body $payload -ContentType "application/json; charset=utf-8" -UseBasicParsing
        Write-Host "Issue #$Id successfully closed." -ForegroundColor Green
    }

    "pr-create" {
        if (-not $Title -or -not $Head) { throw "Title and Head are required for pr-create." }
        $payload = @{
            title = $Title
            head  = $Head
            base  = $Base
            body  = $Body
        } | ConvertTo-Json -Depth 5
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls" -Method Post -Headers $headers -Body $payload -ContentType "application/json; charset=utf-8" -UseBasicParsing
        [PSCustomObject]@{
            Number = $res.number
            Title  = $res.title
            Head   = $res.head.ref
            Base   = $res.base.ref
            Url    = $res.html_url
            State  = $res.state
        } | Format-List
    }

    "pr-list" {
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls?state=$State" -Method Get -Headers $headers -UseBasicParsing
        $res | Select-Object number, title, state, @{Name="head"; Expression={$_.head.ref}}, @{Name="base"; Expression={$_.base.ref}}, html_url | Format-Table -AutoSize
    }

    "pr-get" {
        if (-not $Id) { throw "Id is required for pr-get." }
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls/$Id" -Method Get -Headers $headers -UseBasicParsing
        [PSCustomObject]@{
            Number     = $res.number
            Title      = $res.title
            State      = $res.state
            Mergeable  = $res.mergeable
            Additions  = $res.additions
            Deletions  = $res.deletions
            Url        = $res.html_url
            Body       = $res.body
        } | Format-List
    }

    "pr-update" {
        if (-not $Id) { throw "Id is required for pr-update." }
        $payload = @{}
        if ($Title) { $payload["title"] = $Title }
        if ($Body) { $payload["body"] = $Body }
        $json = $payload | ConvertTo-Json -Depth 5
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls/$Id" -Method Patch -Headers $headers -Body $json -ContentType "application/json; charset=utf-8" -UseBasicParsing
        Write-Host "PR #$Id updated successfully." -ForegroundColor Green
    }

    "pr-diff" {
        if (-not $Id) { throw "Id is required for pr-diff." }
        $diffHeaders = @{
            "Authorization" = "Bearer $token"
            "User-Agent"    = "FootballLife-AgentWorkflow"
            "Accept"        = "application/vnd.github.v3.diff"
        }
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls/$Id" -Method Get -Headers $diffHeaders -UseBasicParsing
        Write-Output $res
    }

    "pr-review" {
        if (-not $Id -or -not $Body) { throw "Id and Body are required for pr-review." }
        $payload = @{
            body  = $Body
            event = $Event
        } | ConvertTo-Json -Depth 5
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls/$Id/reviews" -Method Post -Headers $headers -Body $payload -ContentType "application/json; charset=utf-8" -UseBasicParsing
        Write-Host "Submitted review ($Event) on PR #$Id." -ForegroundColor Green
    }

    "pr-comments" {
        if (-not $Id) { throw "Id is required for pr-comments." }
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/issues/$Id/comments" -Method Get -Headers $headers -UseBasicParsing
        foreach ($c in $res) {
            Write-Host "==================================================" -ForegroundColor Cyan
            Write-Host "Author: $($c.user.login) | Created: $($c.created_at)" -ForegroundColor Yellow
            Write-Host "==================================================" -ForegroundColor Cyan
            Write-Output $c.body
        }
    }

    "pr-reviews" {
        if (-not $Id) { throw "Id is required for pr-reviews." }
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls/$Id/reviews" -Method Get -Headers $headers -UseBasicParsing
        $res | ForEach-Object {
            [PSCustomObject]@{
                Id        = $_.id
                Author    = $_.user.login
                State     = $_.state
                Submitted = $_.submitted_at
                Body      = if ($_.body.Length -gt 300) { $_.body.Substring(0, 300) + "..." } else { $_.body }
            }
        } | Format-Table -Wrap
    }

    "pr-merge" {
        if (-not $Id) { throw "Id is required for pr-merge." }
        $payload = @{
            merge_method = $Method
        }
        if ($Title) { $payload["commit_title"] = $Title }
        if ($Body) { $payload["commit_message"] = $Body }
        $json = $payload | ConvertTo-Json -Depth 5
        $res = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/pulls/$Id/merge" -Method Put -Headers $headers -Body $json -ContentType "application/json; charset=utf-8" -UseBasicParsing
        Write-Host "PR #$Id merged successfully ($Method): $($res.sha)" -ForegroundColor Green
    }
}
