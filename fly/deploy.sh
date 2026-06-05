#!/bin/bash
set -e

# ─────────────────────────────────────────────
#  MicroservicesSolution — Fly.io Deploy Script
#  Run from the root of the repository:
#    chmod +x fly/deploy.sh && ./fly/deploy.sh
# ─────────────────────────────────────────────

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; BLUE='\033[0;34m'; NC='\033[0m'

log()  { echo -e "${BLUE}[INFO]${NC} $1"; }
ok()   { echo -e "${GREEN}[OK]${NC}   $1"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
err()  { echo -e "${RED}[ERR]${NC}  $1"; exit 1; }

# ── CloudAMQP credentials (from cloudamqp.com) ──
RABBIT_HOST="gorilla.lmq.cloudamqp.com"
RABBIT_USER="iufhqqrh"
RABBIT_PASS="ONuVfwVx3ZiFGXPjR9l3CUCAuJ7anN1X"
RABBIT_VHOST="iufhqqrh"

REGION="iad"

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║   MicroservicesSolution — Fly.io Deploy      ║"
echo "╚══════════════════════════════════════════════╝"
echo ""

# ── Check flyctl ──
command -v fly &>/dev/null || err "flyctl not installed. Run: brew install flyctl"
fly auth whoami &>/dev/null  || err "Not logged in. Run: fly auth login"
ok "flyctl ready — $(fly auth whoami)"

# ── Create apps ──
log "Creating Fly.io apps..."
for app in ms-auth-service ms-orders-service ms-products-service ms-payments-service ms-api-gateway; do
  if fly apps list | grep -q "^$app"; then
    warn "App $app already exists — skipping"
  else
    fly apps create "$app" --org personal
    ok "Created $app"
  fi
done

# ── Create Postgres databases ──
log "Creating PostgreSQL databases..."
for db in ms-auth-db ms-orders-db ms-products-db ms-payments-db; do
  if fly apps list | grep -q "^$db"; then
    warn "DB $db already exists — skipping"
  else
    fly postgres create \
      --name "$db" \
      --region "$REGION" \
      --initial-cluster-size 1 \
      --vm-size shared-cpu-1x \
      --volume-size 1 \
      --org personal
    ok "Created $db"
  fi
done

# ── Attach databases ──
log "Attaching databases to services..."
fly postgres attach ms-auth-db     --app ms-auth-service     2>/dev/null || warn "ms-auth-db already attached"
fly postgres attach ms-orders-db   --app ms-orders-service   2>/dev/null || warn "ms-orders-db already attached"
fly postgres attach ms-products-db --app ms-products-service 2>/dev/null || warn "ms-products-db already attached"
fly postgres attach ms-payments-db --app ms-payments-service 2>/dev/null || warn "ms-payments-db already attached"

# ── Set secrets ──
log "Setting environment variables..."

# AUTH
fly secrets set --app ms-auth-service \
  ASPNETCORE_ENVIRONMENT=Production \
  Jwt__Issuer=auth-service \
  Jwt__Audience=microservices \
  Jwt__AccessTokenExpiryMinutes=15 \
  Jwt__RefreshTokenExpiryDays=7
ok "Auth secrets set"

# PRODUCTS
fly secrets set --app ms-products-service \
  ASPNETCORE_ENVIRONMENT=Production \
  Auth__Issuer=auth-service \
  Auth__Audience=microservices \
  "Auth__JwksUri=http://ms-auth-service.internal:8080/.well-known/jwks.json" \
  "RabbitMQ__Host=$RABBIT_HOST" \
  "RabbitMQ__Username=$RABBIT_USER" \
  "RabbitMQ__Password=$RABBIT_PASS" \
  "RabbitMQ__VirtualHost=$RABBIT_VHOST"
ok "Products secrets set"

# ORDERS
fly secrets set --app ms-orders-service \
  ASPNETCORE_ENVIRONMENT=Production \
  Auth__Issuer=auth-service \
  Auth__Audience=microservices \
  "Auth__JwksUri=http://ms-auth-service.internal:8080/.well-known/jwks.json" \
  "GrpcClients__Products=http://ms-products-service.internal:8080" \
  "RabbitMQ__Host=$RABBIT_HOST" \
  "RabbitMQ__Username=$RABBIT_USER" \
  "RabbitMQ__Password=$RABBIT_PASS" \
  "RabbitMQ__VirtualHost=$RABBIT_VHOST"
ok "Orders secrets set"

# PAYMENTS
fly secrets set --app ms-payments-service \
  ASPNETCORE_ENVIRONMENT=Production \
  "RabbitMQ__Host=$RABBIT_HOST" \
  "RabbitMQ__Username=$RABBIT_USER" \
  "RabbitMQ__Password=$RABBIT_PASS" \
  "RabbitMQ__VirtualHost=$RABBIT_VHOST"
ok "Payments secrets set"

# GATEWAY
fly secrets set --app ms-api-gateway \
  ASPNETCORE_ENVIRONMENT=Production \
  "ReverseProxy__Clusters__auth-cluster__Destinations__auth-service__Address=http://ms-auth-service.internal:8080/" \
  "ReverseProxy__Clusters__orders-cluster__Destinations__orders-service__Address=http://ms-orders-service.internal:8080/" \
  "ReverseProxy__Clusters__products-cluster__Destinations__products-service__Address=http://ms-products-service.internal:8080/" \
  "ReverseProxy__Clusters__payments-cluster__Destinations__payments-service__Address=http://ms-payments-service.internal:8080/"
ok "Gateway secrets set"

# ── Deploy services ──
log "Deploying services (this takes ~5 min)..."

fly deploy --config fly/auth.toml     --app ms-auth-service     --remote-only
ok "Auth deployed"

fly deploy --config fly/products.toml --app ms-products-service --remote-only
ok "Products deployed"

fly deploy --config fly/orders.toml   --app ms-orders-service   --remote-only
ok "Orders deployed"

fly deploy --config fly/payments.toml --app ms-payments-service --remote-only
ok "Payments deployed"

fly deploy --config fly/gateway.toml  --app ms-api-gateway      --remote-only
ok "Gateway deployed"

# ── Done ──
echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║  ✅  Deploy complete!                                    ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""
echo "  🌐 Portal:    https://ms-api-gateway.fly.dev"
echo "  🔐 Auth:      https://ms-auth-service.fly.dev/swagger"
echo "  📦 Products:  https://ms-products-service.fly.dev/swagger"
echo "  📋 Orders:    https://ms-orders-service.fly.dev/swagger"
echo "  💳 Payments:  https://ms-payments-service.fly.dev/swagger"
echo ""
