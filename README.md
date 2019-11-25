# Redis Watcher

[![Build status](https://ci.appveyor.com/api/projects/status/wqq4to1ihyabhdmm?svg=true)](https://ci.appveyor.com/project/Sbou/casbin-net-redis-watcher)
[![Coverage Status](https://coveralls.io/repos/github/Sbou/Casbin.NET-Redis-Watcher/badge.svg?branch=master)](https://coveralls.io/github/Sbou/Casbin.NET-Redis-Watcher?branch=master)
[![Nuget](https://img.shields.io/nuget/v/Casbin.NET.Watcher.Redis.svg)](https://www.nuget.org/packages/Casbin.NET.Watcher.Redis/)
[![Release](https://img.shields.io/github/release/Sbou/Casbin.NET-Redis-Watcher.svg)](https://github.com/Sbou/Casbin.NET-Redis-Watcher/releases/latest)
[![Nuget](https://img.shields.io/nuget/dt/Casbin.NET.Watcher.Redis.svg)](https://www.nuget.org/packages/Casbin.NET.Watcher.Redis/)

Redis Watcher is a [Redis](http://redis.io) watcher for [Casbin.NET](https://github.com/casbin/Casbin.NET).

## Installation

    dotnet add package Casbin.NET.Watcher.Redis

## Simple Example

```csharp

using NetCasbin;
using Redis.Casbin.NET;

public class Program
{
    public static void Main(string[] args)
    {
        // Initialize the watcher.
        // Use the Redis host as parameter.
        var watcher = new RedisWatcher("127.0.0.1:6379";

        // Initialize the enforcer.
        var _enforcer = new Enforcer("examples/rbac_model.conf", "examples/rbac_policy.csv");

        // Set the watcher for the enforcer.
        _enforcer.SetWatcher(watcher);

        // Update the policy to test the effect.
        _enforcer.SavePolicy();
    }
}
```

## Getting Help

- [Casbin.NET](https://github.com/casbin/Casbin.NET)