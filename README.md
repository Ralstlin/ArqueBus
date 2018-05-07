# ArqueBus
ArqueBus is a .Net core generic memory event bus.

| | |
| --- | --- |
| **Build** | [![Build status](https://ci.appveyor.com/api/projects/status/rhojxs4e9ukeyh7m/branch/master?svg=true)](https://ci.appveyor.com/project/Ralstlin/arquebus/branch/master) |
| **Tests** | [![Tests status](https://img.shields.io/appveyor/tests/Ralstlin/ArqueBus.svg)](https://img.shields.io/appveyor/tests/Ralstlin/ArqueBus.svg) |
| **Coverage** |[![SonarCloud Coverage](https://sonarcloud.io/api/project_badges/measure?project=arquebus&metric=coverage&)](https://sonarcloud.io/component_measures/metric/coverage/list?id=arquebus) | 
| **Quality** | [![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=arquebus&metric=alert_status)](https://sonarcloud.io/api/project_badges/measure?project=arquebus&metric=alert_status) [![SonarCloud Bugs](https://sonarcloud.io/api/project_badges/measure?project=arquebus&metric=bugs)](https://sonarcloud.io/component_measures/metric/reliability_rating/list?id=arquebus) [![SonarCloud Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=arquebus&metric=vulnerabilities)](https://sonarcloud.io/component_measures/metric/security_rating/list?id=arquebus) | 
| **Nuget** | [![Nuget](https://img.shields.io/nuget/v/arquebus.svg)](http://nuget.org/packages/arquebus) [![Nuget](https://buildstats.info/nuget/arquebus)](http://nuget.org/packages/arquebus)   |

# Usage
The way to create a bus is really simple, just `new EventBus<T, TModel>();` where

* `T` is the type used to find the subscription!
* `TModel` is the type for the objects passed by the publications, you can use `object`, a base class or implement an interface for your models.

## Simple Subscribe/Publish:
```csharp 

var eventBus = new EventBus<string, IModel>();

eventBus.Subscribe("test.first", (data) =>
{
  Console.WriteLine(data.StringValue);
});

underTest.Publish("test.first", new Model(){StringValue = "my.string"});

```

## Awaiting publications and receiving the models published:
```csharp
// Create new event bus
var eventBus = new EventBus<string, object>();

// Listen for publications
var options = new DataflowBlockOptions { CancellationToken = cancellationTokenSource.Token };
var listenedTask = eventBus.ListenAsync<string>("test.first", options);

// publish something
eventBus.Publish("test.first", "published-string-1");
eventBus.Publish("test.first", "published-string-2");

// Cancel listening for publications or awaiting listnedTask will hang forever.
cancellationTokenSource.Cancel();

//Get data
var result = await listenedTask;
Console.Writeline(result.Skip(0).First()) // published-string-1
Console.Writeline(result.Skip(1).First()) // published-string-2
```
