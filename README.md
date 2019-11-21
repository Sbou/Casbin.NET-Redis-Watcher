Redis Watcher
====

Redis Watcher is a [Redis](http://redis.io) watcher for [Casbin](https://github.com/casbin/casbin).

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

- [Casbin](https://github.com/casbin/casbin)