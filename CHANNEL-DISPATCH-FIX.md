# Channel callback dispatch in 1.0.1

`Channel` needs to support subscription changes while notifications arrive. A form can open, close, or transfer its subscriptions while the receiver processes a notification on another thread. A callback can also change subscriptions itself.

In 1.0.0, `Bind`, `Unbind`, and `UnbindAll` modify the same `List(Of Event)` that `MessageReceived` traverses with `List.ForEach`. Changing that list invalidates the traversal. The next version check can throw `InvalidOperationException`, and remaining callbacks miss the notification.

Both `WebSocketListener.Watch` and `SSEListener.Watch` call this dispatch path inside their receive loops. Their exception handlers report the error and exit `Watch`. As a result, this subscription race can stop later notifications from being processed as well. A lock in GTDEV cannot protect a traversal inside Pusher that does not take that lock. The fix must therefore ship in the Pusher package.

## Why the implementation changes

All callback-list mutations now use the same private lock. `MessageReceived` takes that lock only long enough to copy the matching registrations to an array. The array is independent of subsequent list changes.

The dispatcher releases the lock before it invokes callbacks. Holding the lock during callbacks would block subscription changes for the duration of application code. It could also deadlock if a callback waited for another thread to bind or unbind.

The package version changes to 1.0.1 so consumers can install the fix through NuGet. The target framework and public API stay unchanged. GTDEV does not need a source checkout or a project reference to Pusher.

## Callback behavior

Callbacks captured for a notification run in registration order, including duplicate registrations. Unbinding after the snapshot does not cancel a callback already captured for that notification. A callback bound after the snapshot first becomes eligible for the next notification.

Exceptions thrown by application callbacks still propagate as before. This change protects the callback list. It does not add notification persistence, replay, reconnection, or synchronization for the receiver's separate channel collection.

## Verification

The regression tests cover concurrent unbinding, subscription changes inside a callback, event filtering, registration order, duplicate registrations, and a callback that waits for another thread to bind.

Run from the Pusher repository root:

```powershell
dotnet test DeveloperCore.Pusher.Tests/DeveloperCore.Pusher.Tests.vbproj
```

A coordinated test pauses the first callback, changes the registrations, then resumes dispatch. The 1.0.0 implementation throws during this sequence. The snapshot implementation completes the captured callbacks and uses the new registrations for the next notification.
