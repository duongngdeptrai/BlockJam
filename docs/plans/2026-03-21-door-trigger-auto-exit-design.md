# Door Trigger Auto-Exit Design

## Summary
Add a trigger child inside each Door to detect the correct-color Block passing through. When triggered, spawn an effect at the trigger position, temporarily disable block dragging, move the block outwards along the Door `direction` at a fixed speed, then destroy the block after it exits.

## Goals
- Trigger success when a Block of matching ColorType passes the Door.
- Spawn an effect at the Door trigger position.
- Auto-move the Block outward based on Door `direction` at a fixed speed.
- Destroy the Block after it has fully exited.
- Allow the player to keep controlling other blocks while the exiting block moves.

## Non-Goals
- Changing Door main collider behavior (Door remains non-trigger for normal collisions).
- Adding new gameplay rules beyond correct-color exit behavior.

## Approach Options Considered
1. Child trigger in Door (chosen): clean separation of collision and trigger logic.
2. Toggle Door collider to trigger on match: mixes collision/trigger paths and can be brittle.
3. Check for Door only on release: less reliable for continuous pass-through.

## Proposed Design
### Components
- `Door` prefab
  - Existing collider remains non-trigger.
  - Add child GameObject `DoorTrigger` with `Collider2D` set to `isTrigger = true`.
- New script `DoorTrigger.cs`
  - OnTriggerEnter2D: verify Block color matches Door color.
  - Spawn effect prefab at trigger transform (optional if prefab is null).
  - Call `BlockMove.StartAutoExit(Direction direction)`.
- `BlockMove` updates
  - Add auto-exit state: disable dragging while auto-exiting.
  - Move block along direction at a fixed speed each FixedUpdate.
  - Destroy block after it moves beyond `exitDistance` from the trigger entry point.

### Data Flow
1. Block enters `DoorTrigger`.
2. `DoorTrigger` gets Door reference and checks `Block.ColorType` == `Door.ColorType`.
3. If match:
   - Spawn effect prefab at trigger position.
   - Call `BlockMove.StartAutoExit(door.Direction)`.
4. Block moves outward at `exitSpeed` until distance >= `exitDistance` then destroy.

### Parameters
- `exitSpeed` (float, serialized in BlockMove).
- `exitDistance` (float, serialized in BlockMove).

## Edge Cases
- If Block is already auto-exiting, ignore repeat triggers.
- If effect prefab is null, skip spawn but still auto-exit.
- If Block color does not match, do nothing.

## Testing Plan
- Correct-color Block triggers effect, auto-exits in correct direction, then destroy.
- Wrong-color Block does nothing on trigger.
- While a Block auto-exits, the player can drag a different Block normally.
