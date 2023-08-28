using Casbin;
using Redis.Casbin.NET;

//************************************************
// For this sample you can use a local Redis server installed to WSL
// https://redis.io/docs/getting-started/installation/install-redis-on-windows/
//
// With the Redis CLI you can check if a message is sent when the 'SavePolicy' is called
// Bash command:
// > redis-cli PSUBSCRIBE '*'
//
// > redis-cli PUBLISH CasbinUpdate test
//************************************************

// Initialize the watcher.
// Use the Redis host as parameter.
var watcher = new RedisWatcher("127.0.0.1:6379");

watcher.SetUpdateCallback(() =>
{
    Console.WriteLine("This instance has been notified from an external publisher");
});

// Initialize the enforcer.
var _enforcer = new Enforcer("assets/rbac_model.conf", "assets/rbac_policy.csv");

// Set the watcher for the enforcer.
_enforcer.SetWatcher(watcher);

// Update the policy to test the effect.
_enforcer.SavePolicy();

Console.WriteLine("Press Any key to exit...");
Console.ReadLine();

