/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

namespace Corsinvest.ProxmoxVE.NodeProtect.Api.Tests;

public class ParseHostsTests
{
    [Theory]
    [InlineData("pve01", "pve01", 22)]
    [InlineData("pve01:2222", "pve01", 2222)]
    [InlineData("192.168.0.1", "192.168.0.1", 22)]
    [InlineData("192.168.0.1:2222", "192.168.0.1", 2222)]
    [InlineData("fe80::1", "fe80::1", 22)]
    [InlineData("[fe80::1]", "fe80::1", 22)]
    [InlineData("[fe80::1]:2222", "fe80::1", 2222)]
    [InlineData("[::1]:22", "::1", 22)]
    public void ParseHostAndPort(string input, string host, int port)
        => Assert.Equal((host, port), ProtectHelper.ParseHostAndPort(input));

    [Fact]
    public void ParseHostAndPort_PortNotANumber_UsesDefaultPort()
        => Assert.Equal(("pve01", 22), ProtectHelper.ParseHostAndPort("pve01:ssh"));

    [Theory]
    [InlineData("[fe80::1")]
    [InlineData("[fe80::1]2222")]
    [InlineData("[fe80::1]:ssh")]
    public void ParseHostAndPort_InvalidIPv6_Throws(string input)
        => Assert.Throws<ArgumentException>(() => ProtectHelper.ParseHostAndPort(input));

    [Fact]
    public void ParseHosts_KeepsOrderAndSkipsEmptyEntries()
    {
        var hosts = ProtectHelper.ParseHosts(" pve02 , [::1]:22,,pve01:2222 ");

        Assert.Equal([("pve02", 22), ("::1", 22), ("pve01", 2222)], hosts);
    }
}
