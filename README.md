# utils4u

Various utilities for Unity game engine. There are not many interdependencies here, so in most cases one can simply copypaste only needed file(s)/function(s) and make minor modifications to suit project's specific needs.

## Pathfinding

#### Paths.BitonicTour

Computes a [bitonic tour](https://en.wikipedia.org/wiki/Bitonic_tour) for given array of vertices. Complexity: `O(n^2)`. Bitonic tours are proven to find less optimal solutions than [PTAS](https://en.wikipedia.org/wiki/Polynomial-time_approximation_scheme) under certain conditions, but are still useful in cases you want a [simple polygon](https://en.wikipedia.org/wiki/Simple_polygon) (for example, this is more intuitive route shape for flying objects). Notice, this function will return an array, where the first element is not strictly leftmost point of route. If you have an usecase where this really matters, please create an issue.

## Infrastructural

#### Factory

Provides a way to instantiate objects and set their specific fields to random values from numeric range or a list of variants without forcing you to keep that data inside every instance (as it becomes useless after instantiation).

![factory](https://raw.githubusercontent.com/aemxdp/utils4u/master/factory.png)

#### EventLogger

Generic event looger. After binding, makes a `Debug.Log` calls printing names and args on every fired event of all the components of some gameobject and all of its children (at any hierarchical depth). Looks like this:

![eventlogger](https://raw.githubusercontent.com/aemxdp/utils4u/master/eventlogger.png)

**Usage:** You can call `EventLogger.LogAllEvents(rootGameObjectInHierarchy)` on your object of interest,
or you can add `EventLogger` as a component and it will bind automatically on `Await`.

**Protip:** Prefer using custom delegates instead of Func/Action ones for your events which have more than zero parameters
because they allow you to specify parameter names so that EventLogger can use them for prettier log printing:

```C#
public delegate void DamageHandler(float amount);

public event Action Death = delegate { };
public event DamageHandler Damage = delegate { };
public event DamageHandler DamageShield = delegate { };
public event DamageHandler DamageHealth = delegate { };
```