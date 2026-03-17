# Block Drag Slide Design

## Context
We need block dragging where blocks do not overlap. When dragging diagonally and the intended direction is blocked on one axis, the block should slide along the other axis if free. Movement is continuous while dragging and snaps to grid only on release.

## Goals
- Prevent blocks from overlapping while dragging.
- If diagonal movement is blocked on one axis, allow sliding along the other axis.
- Keep current free-drag feel; snap only on mouse release.

## Non-Goals
- Grid-based movement during drag.
- Physics-based collision responses between dynamic bodies.

## Behavior Summary
- During drag, compute desired target position from mouse.
- In physics step, attempt movement toward target.
- If diagonal movement is blocked, try axis-separated movement.
- If both axes are blocked, stop.
- On release, snap to grid.

## Approach Options
1. ColliderCast-based manual blocking (recommended)
   - Use `BoxCollider2D.Cast` to detect obstacles along movement vector.
   - If blocked on full vector, try X then Y (or Y then X per rule).
   - Deterministic and matches design.

2. Pure physics collision resolution
   - Rely on Rigidbody2D dynamics to block movement.
   - Less deterministic for axis-priority slide.

## Recommended Design
Use ColliderCast checks before applying movement:
- Compute `delta = targetPos - rb.position`.
- If `delta` is near zero, do nothing.
- If diagonal, attempt move along X first. If blocked, attempt Y.
  - This matches the rule: if right is blocked, move up if free.
- If single-axis movement, just check that axis.

## Data Flow
- `OnMouseDrag` updates `targetPos` from mouse.
- `FixedUpdate` computes `delta` and decides allowed movement.
- `MovePosition` applies the allowed movement.
- `OnMouseUp` snaps to grid.

## Edge Cases
- Very small deltas are ignored to avoid jitter.
- If both axes blocked, object remains in place.

## Testing Plan
- Drag into another block on X axis: should stop or slide on Y if free.
- Drag diagonally into a corner: should slide along free axis, or stop.
- Release after partial movement: should snap to grid.

## Rollout
- Update `BlockMove.cs` only.
- Manual playtest in editor.
