# Sync Protocol: Contract Test Scenarios

These scenarios must pass to ensure the sync protocol is compliant with the "Offline-First" requirements.

## Scenario 1: Basic Idempotent Replay
**Given**: A client sends an `OrderCreate` operation (`op-123`).
**When**: The server receives the same `op-123` twice (simulating retry).
**Then**: The second response MUST match the first AND no duplicate order is created on the server.

## Scenario 2: Causal Consistency (Strict Ordering)
**Given**: Operations `op-1` (CreateOrder) and `op-2` (AssignTable) are queued.
**When**: `op-1` fails but `op-2` is sent.
**Then**: The server SHOULD reject `op-2` if it depends on a resource created in `op-1` until `op-1` is successfully processed.

## Scenario 3: Version Conflict (Pre-condition Failed)
**Given**: Client A and Client B both update `Order-1`.
**When**: Client A's sync is processed first.
**Then**: Client B's sync MUST return `status: conflict` with `serverState` containing Client A's updates.

## Scenario 4: Dead-Lettering
**Given**: An operation `op-fail` is sent 5 times and results in a permanent `400 Bad Request`.
**When**: Retry threshold is hit.
**Then**: Client MUST move `op-fail` to `Dead-Letter` and alert the merchant (AC for S1-05).
