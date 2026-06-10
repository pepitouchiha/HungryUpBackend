#!/bin/bash
# Verifica las reglas de arquitectura estrictas definidas en backend-spec.md

PASS=0
FAIL=0

section() { echo ""; echo "── $1 ──────────────────────────────────────────"; }
ok()   { echo "  ✓ $1"; ((PASS++)); }
fail() { echo "  ✗ $1"; ((FAIL++)); }

# ── 1. Prohibición de float / double ─────────────────────────────────────────
section "Regla: sin float ni double en Modules/"

HITS=$(grep -rn --include="*.cs" -E "\b(float|double)\b" Modules/ 2>/dev/null)
if [ -z "$HITS" ]; then
    ok "Ningún uso de float o double detectado."
else
    while IFS= read -r line; do fail "$line"; done <<< "$HITS"
fi

# ── 2. Propiedades decimal con precisión configurada en DbContexts ────────────
section "Regla: decimal(18,2) — HasPrecision en cada DbContext"

for ctx in Modules/Catalog/Entities/CatalogDbContext.cs \
           Modules/Orders/Entities/OrdersDbContext.cs \
           Modules/Billing/Entities/BillingDbContext.cs; do
    if grep -q "HasPrecision" "$ctx" 2>/dev/null; then
        ok "$ctx → HasPrecision encontrado"
    else
        fail "$ctx → HasPrecision NO encontrado"
    fi
done

# ── 3. DbContexts aislados (ningún módulo importa el DbContext de otro) ───────
section "Regla: aislamiento de DbContexts entre módulos"

check_cross() {
    local module=$1; local forbidden=$2
    local hits
    hits=$(grep -rn --include="*.cs" "$forbidden" "Modules/$module/" 2>/dev/null | grep -v "\.Designer\." | grep -v "Snapshot")
    if [ -z "$hits" ]; then
        ok "Modules/$module/ no referencia $forbidden"
    else
        while IFS= read -r line; do fail "$line"; done <<< "$hits"
    fi
}

check_cross "Catalog"  "OrdersDbContext\|BillingDbContext"
check_cross "Orders"   "CatalogDbContext\|BillingDbContext"
check_cross "Billing"  "CatalogDbContext\|OrdersDbContext"
check_cross "Analytics" "CatalogDbContext\|OrdersDbContext\|BillingDbContext"

# ── 4. Comunicación inter-módulos vía interfaces ──────────────────────────────
section "Regla: inter-módulo solo por interfaces (IXxxService)"

check_concrete() {
    local module=$1; local forbidden_class=$2
    local hits
    hits=$(grep -rn --include="*.cs" "new $forbidden_class\b\|: $forbidden_class\b" "Modules/$module/" 2>/dev/null)
    if [ -z "$hits" ]; then
        ok "Modules/$module/ no instancia directamente $forbidden_class"
    else
        while IFS= read -r line; do fail "$line"; done <<< "$hits"
    fi
}

check_concrete "Billing"  "OrdersService"
check_concrete "Analytics" "BillingService"
check_concrete "Orders"   "CatalogService"

# ── Resultado ─────────────────────────────────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════"
echo "  Resultado: $PASS reglas OK  |  $FAIL violaciones"
echo "═══════════════════════════════════════════════════════"
[ $FAIL -eq 0 ] && exit 0 || exit 1
