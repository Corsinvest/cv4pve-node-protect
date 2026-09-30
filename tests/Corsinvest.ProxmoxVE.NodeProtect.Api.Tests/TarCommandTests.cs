/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

namespace Corsinvest.ProxmoxVE.NodeProtect.Api.Tests;

public class TarCommandTests
{
    [Fact]
    public void BuildTarCommand_QuotesEveryPath()
        => Assert.Equal("tar --one-file-system --ignore-failed-read -czPf - -- '/etc/.' '/etc/pve/.' '/root/my scripts'",
                        ProtectEngine.BuildTarCommand(["/etc/.", "/etc/pve/.", "/root/my scripts"]));

    [Theory]
    [InlineData("/etc/$(reboot)")]
    [InlineData("/etc; rm -rf /")]
    [InlineData("/etc/`id`")]
    public void BuildTarCommand_ShellCharactersStayInsideQuotes(string path)
        => Assert.EndsWith($" '{path}'", ProtectEngine.BuildTarCommand([path]));

    [Theory]
    [InlineData("--checkpoint-action=exec=id")]
    [InlineData("-C")]
    public void BuildTarCommand_PathStartingWithDash_IsNotAnOption(string path)
        => Assert.EndsWith($" -- '{path}'", ProtectEngine.BuildTarCommand([path]));

    [Fact]
    public void BuildTarCommand_SingleQuote_Throws()
        => Assert.Throws<ArgumentException>(() => ProtectEngine.BuildTarCommand(["/root/it's"]));
}
