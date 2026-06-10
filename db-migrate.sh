#!/bin/bash
# Genera nuevas migraciones para los tres módulos.
# Uso: ./db-migrate.sh <NombreMigracion>
# Ejemplo: ./db-migrate.sh AddCategoriaActivo

NAME=${1:-"NewMigration"}

dotnet ef migrations add "$NAME" \
  --context CatalogDbContext \
  --output-dir Modules/Catalog/Migrations

dotnet ef migrations add "$NAME" \
  --context OrdersDbContext \
  --output-dir Modules/Orders/Migrations

dotnet ef migrations add "$NAME" \
  --context BillingDbContext \
  --output-dir Modules/Billing/Migrations

echo "Migraciones '$NAME' creadas. Ejecuta 'dotnet run' para aplicarlas automáticamente."
