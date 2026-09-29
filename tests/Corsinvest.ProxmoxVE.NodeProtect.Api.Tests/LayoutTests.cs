/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

namespace Corsinvest.ProxmoxVE.NodeProtect.Api.Tests;

public class LayoutTests
{
    [Fact]
    public void GetArchiveFileName_HostName()
        => Assert.Equal("pve01-config.tar.gz", ProtectHelper.GetArchiveFileName("pve01"));

    [Fact]
    public void GetArchiveFileName_IPv6_ValidOnThisSystem()
    {
        var name = ProtectHelper.GetArchiveFileName("fe80::1");

        Assert.EndsWith(ProtectEngine.FileNameSuffix, name);
        Assert.DoesNotContain(name, c => Path.GetInvalidFileNameChars().Contains(c));
        Assert.Equal(OperatingSystem.IsWindows() ? "fe80__1-config.tar.gz" : "fe80::1-config.tar.gz", name);
    }

    [Theory]
    [InlineData("2026-09-29-03-00-01", true)]
    [InlineData("2026-09-29", false)]
    [InlineData("2026-09-29-03-00-01-old", false)]
    [InlineData("notes", false)]
    public void IsBackupDirectoryName(string name, bool expected)
        => Assert.Equal(expected, ProtectHelper.IsBackupDirectoryName(name));

    [Fact]
    public void IsBackupDirectoryName_MatchesDateFormat()
        => Assert.True(ProtectHelper.IsBackupDirectoryName(new DateTime(2026, 9, 29, 3, 0, 1).ToString(ProtectEngine.DateFormat)));

    [Fact]
    public void GetBackupsToDelete_KeepsNewestDatedFolders()
    {
        string[] directories =
        [
            Path.Combine("work", "2026-09-27-03-00-01"),
            Path.Combine("work", "2026-09-29-03-00-01"),
            Path.Combine("work", "notes"),
            Path.Combine("work", "2026-09-28-03-00-01"),
            Path.Combine("work", "2025-12-31-23-59-59"),
        ];

        var toDelete = ProtectHelper.GetBackupsToDelete(directories, 2);

        Assert.Equal([Path.Combine("work", "2026-09-27-03-00-01"), Path.Combine("work", "2025-12-31-23-59-59")], toDelete);
    }

    [Fact]
    public void GetBackupsToDelete_FewerThanKeep_DeletesNothing()
        => Assert.Empty(ProtectHelper.GetBackupsToDelete(["2026-09-29-03-00-01", "other"], 7));
}
