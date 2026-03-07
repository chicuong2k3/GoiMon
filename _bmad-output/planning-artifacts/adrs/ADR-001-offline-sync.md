# ADR-001: Offline Sync & Idempotency Protocol

## Status
Accepted (Sprint 1)

## Context
GoiMon must work in environments with intermittent or no connectivity. Staff must be able to record orders, take payments, and print receipts without waiting for a server response. When connectivity returns, all locally queued operations must be synchronized to the cloud without data loss or duplication.

## Decision
We adopt a **Local-First with Background Sync Queue** architecture.

### Key Components:
1.  **Idempotency**: All operations (CreateOrder, MarkPaid, etc.) are assigned a Client-Generated UUID (`operationId`) before being saved locally. The server MUST deduplicate operations based on this `operationId` + `clientId`.
2.  **Vector/Sequence Clock**: Each client maintains a local sequence number (`localSeq`) to preserve order of operations during replay on the server.
3.  **Sync Protocol**: Clients upload batches of operations. The server processes them in a transition-safe manner.
4.  **Conflict Resolution**: 
    -   **Orders**: State-machine based. If an order is already "Paid" on the server, a client update attempting to "Cancel" it will be rejected as a conflict.
    -   **Shared Data (Menu)**: Last-Write-Wins (LWW) based on client-supplied timestamps.

## Consequences
-   **Positive**: Near-zero UI latency, works in 100% offline modes, resilient to flaky networks.
-   **Negative**: Server complexity increases for deduplication logic; 30-day retention of operation IDs required for reliable replay detection.
-   **Risk**: Potential for merge conflicts if multiple devices modify the same order simultaneously; mitigated by assigning specific tables to specific devices if possible.
