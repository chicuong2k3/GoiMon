#!/bin/bash
# Sync Failure Simulator (S1-06 Tooling Baseline)
# Usage: ./scripts/sync-simulator.sh --scenario [offline|conflict|duplicate|corrupt]

echo "[Sync Simulator] Target: http://localhost:5003/api/sync/batch"
echo "[Sync Simulator] Mode: $1"

case $1 in
  "offline")
    echo "Simulating connectivity loss (dropping packets to :5003)..."
    echo "(Action: iptables or server shutdown)"
    ;;
  "duplicate")
    echo "Simulating duplicate batch replay..."
    echo "Sending Batch ID 123 to server twice."
    # curl -X POST ... (command mocked here for MVP)
    ;;
  "conflict")
    echo "Simulating concurrent modification conflict..."
    echo "Sending Update to Order-1 from two separate Client-IDs."
    ;;
  "corrupt")
    echo "Simulating malformed JSON payload rejection..."
    ;;
  *)
    echo "Usage: ./scripts/sync-simulator.sh --scenario [offline|conflict|duplicate|corrupt]"
    ;;
esac
