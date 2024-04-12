# SimpleEventBus
## Installation
Download the project and build it, then include the dll in your project.
## Usage
First you need to include the usaging statement: \
`using SimpleEventBus.Bus;` \
Then you have two options:
- either mark your methods with the SubscribeAttribute like so: 
```
[Subscribe("CallName")]
private static void HandleSomething(param 1, ..., param 16) {}
```
- or subscribe manually: 
``` 
Bus.GetInstance().Subscribe("CallName", HandleSomething);

private void HandleSomething(param 1, ..., param 16) {}
```

The call name can be any string, but be careful since its cace sensitive.

To publish an event you simply call: \
`Bus.GetInstance().Publish("CallName", object[] data);`

The bus will then invoke all methods with the same string you provided with the given data. 

To unsubscribe an method from an event you call: <br>
`Bus.GetInstance().Unsubscribe("CallName", Delegate method);`

## Tips
Be sure than when you publish an event, the given data fits in the parameters of all subscribed methods. 
For example: 
```
[Subscribe("TestName")]
void Test(string test) {}

Bus.GetInstance().Publish("TestName", ["Test"]) \\ this will work
Bus.GetInstance().Publish("TestName", ["Test1", "Test2"]) \\ this wont work
Bus.GetInstance().Publish("TestName", []) \\ this wont work
```

When you want a instance method to subscribe to an event, dont use the attribute since it will create a new instance of the object so it wont have the expected instance data.