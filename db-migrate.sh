#!/usr/bin/env bash
# Uso: bash db-migrate.sh <NombreMigracion>
# Genera una migración en los 3 DbContexts. Si el modelo no cambió, EF produce
# migraciones vacías — elimínalas con:
#   dotnet ef migrations remove --project src/HungryUp.Persistence --startup-project src/HungryUp.Api --context <Context>

set -e

if [ -z "$1" ]; then
  echo "Uso: bash db-migrate.sh <NombreMigracion>"
  exit 1
fi

NAME="$1"
PERSISTENCE="src/HungryUp.Persistence"
STARTUP="src/HungryUp.Api"

echo "-> CatalogDbContext"
dotnet ef migrations add "$NAME" \
  --project "$PERSISTENCE" \
  --startup-project "$STARTUP" \
  --context CatalogDbContext \
  --output-dir Catalog/Migrations

echo "-> OrdersDbContext"
dotnet ef migrations add "$NAME" \
  --project "$PERSISTENCE" \
  --startup-project "$STARTUP" \
  --context OrdersDbContext \
  --output-dir Orders/Migrations

echo "-> BillingDbContext"
dotnet ef migrations add "$NAME" \
  --project "$PERSISTENCE" \
  --startup-project "$STARTUP" \
  --context BillingDbContext \
  --output-dir Billing/Migrations

echo "Migraciones '$NAME' creadas. Ejecuta 'dotnet run' para aplicarlas automaticamente."
