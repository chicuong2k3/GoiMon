-- Shared-schema multi-tenant DDL (Postgres syntax examples)
-- Use a `tenant_id` column on all business tables. tenant_id can be UUID or bigint.

-- Tenants
CREATE TABLE tenants (
  tenant_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT NOT NULL,
  plan TEXT,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

-- Devices (registered devices for sync)
CREATE TABLE devices (
  device_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
  name TEXT,
  sync_key TEXT, -- opaque sync token
  last_seen TIMESTAMP WITH TIME ZONE
);

-- Menu items / products
CREATE TABLE items (
  item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL,
  name TEXT NOT NULL,
  price_cents INTEGER NOT NULL,
  category TEXT,
  is_active BOOLEAN DEFAULT TRUE,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
  CONSTRAINT items_tenant_fk FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE CASCADE
);
CREATE INDEX idx_items_tenant_id ON items(tenant_id);

-- Orders
CREATE TABLE orders (
  order_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL,
  external_id TEXT, -- client-generated id (for mapping temporary ids)
  device_id UUID,
  total_cents INTEGER NOT NULL,
  status TEXT NOT NULL DEFAULT 'open', -- open/paid/cancelled
  payment_method TEXT,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
  CONSTRAINT orders_tenant_fk FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE CASCADE
);
CREATE INDEX idx_orders_tenant_id_created_at ON orders(tenant_id, created_at DESC);

-- Order items
CREATE TABLE order_items (
  order_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  order_id UUID NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
  tenant_id UUID NOT NULL,
  item_id UUID NOT NULL,
  qty INTEGER NOT NULL DEFAULT 1,
  unit_price_cents INTEGER NOT NULL,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
  CONSTRAINT order_items_tenant_fk FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE CASCADE
);
CREATE INDEX idx_order_items_tenant_order ON order_items(tenant_id, order_id);

-- Sync queue (server-side canonical queue for incoming device operations)
CREATE TABLE sync_queue (
  sync_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL,
  device_id UUID,
  operation_id UUID NOT NULL, -- client-generated id for idempotency
  operation_type TEXT NOT NULL, -- e.g., 'order:create','order:update'
  payload JSONB NOT NULL,
  received_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
  processed_at TIMESTAMP WITH TIME ZONE NULL,
  status TEXT DEFAULT 'pending'
);
CREATE INDEX idx_sync_queue_tenant_pending ON sync_queue(tenant_id) WHERE status = 'pending';

-- Remarks
-- 1) Add tenant_id to every table that contains merchant data. Consider partial indexes per-tenant for very large tenants.
-- 2) Use composite unique constraints where appropriate, e.g. (tenant_id, external_id) to map client-side IDs.
-- 3) Enforce access control in application layer and guard queries with tenant_id in WHERE clauses.
