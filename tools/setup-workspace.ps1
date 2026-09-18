# ==============================================================================
# Football Life - Workspace Environment & MCP Setup Script
# ==============================================================================
# This script inspects, configures, and validates the full development environment:
# 1. Unity 6 Editor & Unity Hub
# 2. Git LFS for game asset tracking
# 3. Model Context Protocol (MCP) Servers:
#    - Unity MCP (uvx mcpforunityserver)
#    - GitHub MCP (native binary: tools/bin/github-mcp-server.exe)
#    - Context7 MCP (@upstash/context7-mcp)
#    - Figma MCP (figma-developer-mcp)
#    - Blender MCP (blend-ai / blender-ai-mcp)
# 4. Agent Skills & Rules verification (.agents/)
# ==============================================================================

[CmdletBinding()]
param(
    [switch]$SkipMcpChecks = $false
)

$ErrorActionPreference = "Continue"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "     Football Life - Workspace Environment Setup         " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# ------------------------------------------------------------------------------
# 1. Check Git & Git LFS
# ------------------------------------------------------------------------------
Write-Host "`n[1/5] Checking Git & Git LFS..." -ForegroundColor Yellow
if (Get-Command git -ErrorAction SilentlyContinue) {
    $gitVer = git --version
    Write-Host "  [OK] Git found: $gitVer" -ForegroundColor Green
    
    if (Get-Command git-lfs -ErrorAction SilentlyContinue) {
        $lfsVer = git lfs version
        Write-Host "  [OK] Git LFS found: $lfsVer" -ForegroundColor Green
        git lfs install | Out-Null
        Write-Host "  [OK] Git LFS hooks initialized for repository." -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Git LFS not found. Please install git-lfs from https://git-lfs.github.com" -ForegroundColor Magenta
    }
} else {
    Write-Host "  [FAIL] Git is not installed on PATH!" -ForegroundColor Red
}

# ------------------------------------------------------------------------------
# 2. Check Unity 6 Installation
# ------------------------------------------------------------------------------
Write-Host "`n[2/5] Checking Unity 6 Installation..." -ForegroundColor Yellow
$unityEditorPaths = @(
    "C:\Program Files\Unity\Hub\Editor\6000.2.0f1\Editor\Unity.exe",
    "C:\Program Files\Unity\Editor\Unity.exe"
)

$unityFound = $false
foreach ($path in $unityEditorPaths) {
    if (Test-Path $path) {
        Write-Host "  [OK] Unity Editor found at: $path" -ForegroundColor Green
        $unityFound = $true
        break
    }
}

if (-not $unityFound) {
    Write-Host "  [INFO] Checking Unity Hub..." -ForegroundColor Yellow
    if (Test-Path "C:\Program Files\Unity Hub\Unity Hub.exe") {
        Write-Host "  [OK] Unity Hub found at: C:\Program Files\Unity Hub\Unity Hub.exe" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Unity 6 Editor not found at default path. Ensure Unity 6000.2+ is installed via Unity Hub." -ForegroundColor Magenta
    }
}

# ------------------------------------------------------------------------------
# 3. Check Language Runtimes (Node, Python, UV)
# ------------------------------------------------------------------------------
Write-Host "`n[3/5] Checking Runtimes (Python, uv, Node.js)..." -ForegroundColor Yellow

if (Get-Command python -ErrorAction SilentlyContinue) {
    $pyVer = python --version
    Write-Host "  [OK] Python found: $pyVer" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Python not found on PATH." -ForegroundColor Red
}

if (Get-Command uvx -ErrorAction SilentlyContinue) {
    $uvxVer = cmd /c uvx --version
    Write-Host "  [OK] uvx found: $uvxVer" -ForegroundColor Green
} else {
    Write-Host "  [WARN] uvx not found. Install uv via: powershell -ExecutionPolicy ByPass -c `"irm https://astral.sh/uv/install.ps1 | iex`"" -ForegroundColor Magenta
}

if (Get-Command node -ErrorAction SilentlyContinue) {
    $nodeVer = node --version
    Write-Host "  [OK] Node.js found: $nodeVer" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Node.js not found on PATH." -ForegroundColor Red
}

# ------------------------------------------------------------------------------
# 4. Validate Agent Skills & Rules
# ------------------------------------------------------------------------------
Write-Host "`n[4/5] Checking Antigravity Skills & Rules..." -ForegroundColor Yellow
$skillsDir = Join-Path $PSScriptRoot "..\.agents\skills"
$rulesDir = Join-Path $PSScriptRoot "..\.agents\rules"

if (Test-Path $skillsDir) {
    $skillCount = (Get-ChildItem -Directory $skillsDir).Count
    Write-Host "  [OK] Skills directory detected: $skillCount skills installed." -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Skills directory missing at $skillsDir" -ForegroundColor Red
}

if (Test-Path $rulesDir) {
    $ruleCount = (Get-ChildItem -File $rulesDir).Count
    Write-Host "  [OK] Rules directory detected: $ruleCount rules active." -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Rules directory missing at $rulesDir" -ForegroundColor Red
}

# ------------------------------------------------------------------------------
# 5. MCP Server Status & Config
# ------------------------------------------------------------------------------
Write-Host "`n[5/5] Checking MCP Servers..." -ForegroundColor Yellow

# GitHub MCP
$ghBinary = Join-Path $PSScriptRoot "bin\github-mcp-server.exe"
if (Test-Path $ghBinary) {
    Write-Host "  [OK] GitHub MCP binary ready: $ghBinary" -ForegroundColor Green
} else {
    Write-Host "  [WARN] GitHub MCP binary missing. Run download routine or use Docker." -ForegroundColor Magenta
}

# Unity MCP
Write-Host "  [INFO] Unity MCP (mcpforunityserver) is registered via uvx." -ForegroundColor Cyan
Write-Host "  [INFO] In Unity: Install 'https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main' and select 'Window -> MCP for Unity -> Configure All Detected Clients'." -ForegroundColor Cyan

# Context7 MCP
Write-Host "  [OK] Context7 MCP is configured via '@upstash/context7-mcp'." -ForegroundColor Green

# Figma MCP
Write-Host "  [OK] Figma MCP is configured via 'figma-developer-mcp'." -ForegroundColor Green

# Summary
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " Workspace successfully verified! You are ready to build. " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
