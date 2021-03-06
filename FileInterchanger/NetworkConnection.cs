﻿// From Luke Quinane (http://stackoverflow.com/a/1197430/1028452)
//
// Usage:
// using (NetworkConnection ncA = new NetworkConnection(@"\\server\read", readCredentials))
// using (NetworkConnection ncB = new NetworkConnection(@"\\server2\write", writeCredentials)) {
//    ncA.Connect();
//    ncB.Connect();
//    File.Copy(@"\\server\read\file", @"\\server2\write\file");
// }

using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

public class NetworkConnection : IDisposable
{
    public const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;

    bool _forceConnection;
    string _networkName;
    string _password;
    string _userName;

    public NetworkConnection(string networkName,
        NetworkCredential credentials)
    {
        _forceConnection = false;
        _networkName = networkName;

        _userName = string.IsNullOrEmpty(credentials.Domain)
            ? credentials.UserName
            : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);
        _password = credentials.Password;
    }

    /// <summary>
    /// Remove credential conflict when trying to connect to network resource.
    /// </summary>
    public bool ForceConnection
    {
        get { return _forceConnection; }
        set { _forceConnection = value; }
    }

    /// <summary>
    /// Connects to specified network resource using provided credentials.
    /// </summary>
    /// <exception cref="Win32Exception">Error connecting to network resource.</exception>
    public void Connect()
    {
        var netResource = new NetResource()
        {
            Scope = ResourceScope.GlobalNetwork,
            ResourceType = ResourceType.Disk,
            DisplayType = ResourceDisplaytype.Share,
            RemoteName = _networkName
        };

        var result = WNetAddConnection2(netResource, _password, _userName, 0);

        if (result == ERROR_SESSION_CREDENTIAL_CONFLICT && _forceConnection)
        {
            WNetCancelConnection2(_networkName, 0, true);
            result = WNetAddConnection2(netResource, _password, _userName, 0);
        }
        if (result != 0)
        {
            throw new Win32Exception(result);
        }
    }

    ~NetworkConnection()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        WNetCancelConnection2(_networkName, 0, true);
    }

    [DllImport("mpr.dll")]
    private static extern int WNetAddConnection2(NetResource netResource,
        string password, string username, int flags);

    [DllImport("mpr.dll")]
    private static extern int WNetCancelConnection2(string name, int flags,
        bool force);
}

[StructLayout(LayoutKind.Sequential)]
public class NetResource
{
    public ResourceScope Scope;
    public ResourceType ResourceType;
    public ResourceDisplaytype DisplayType;
    public int Usage;
    public string LocalName;
    public string RemoteName;
    public string Comment;
    public string Provider;
}

public enum ResourceScope : int
{
    Connected = 1,
    GlobalNetwork,
    Remembered,
    Recent,
    Context
};

public enum ResourceType : int
{
    Any = 0,
    Disk = 1,
    Print = 2,
    Reserved = 8,
}

public enum ResourceDisplaytype : int
{
    Generic = 0x0,
    Domain = 0x01,
    Server = 0x02,
    Share = 0x03,
    File = 0x04,
    Group = 0x05,
    Network = 0x06,
    Root = 0x07,
    Shareadmin = 0x08,
    Directory = 0x09,
    Tree = 0x0a,
    Ndscontainer = 0x0b
}