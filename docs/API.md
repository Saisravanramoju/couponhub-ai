# API guide

Except registration, login and `/health/*`, requests require `Authorization: Bearer <accessToken>`. Sessions are opaque tokens, not JWTs. Revoke the current token with logout. Passwords require 12–128 characters. Authentication is rate-limited to 10 requests per minute per source IP; AI routes to 10 per minute per user, per API instance.

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/api/account/register` | `{ "email": "...", "password": "..." }` → session |
| POST | `/api/account/login` | Credentials → session |
| POST | `/api/account/logout` | Revoke current session |
| GET | `/api/account` | Current account and preferences |
| PUT | `/api/account/preferences` | `{ "categories": ["Food", "Shopping"] }` |
| GET / POST | `/api/brands` | Read brands / admin creates brand |
| GET | `/api/brands/{id}` | Brand details |
| GET / POST | `/api/coupons` | Visible coupons / create private coupon |
| GET / PUT / DELETE | `/api/coupons/{id}` | Read / owner or admin update/delete |
| POST | `/api/coupons/{id}/publish` | Admin publishes an owned coupon to everyone |
| GET | `/api/search?q=&category=Food&page=1&pageSize=30` | Paginated active catalog |
| POST | `/api/recommendations` | Ranked eligible offers |
| POST | `/api/imports/extract` | Consent-gated AI draft, never automatically stored |
| GET | `/api/saved` | Current user's saved coupons (up to 500) |
| PUT / DELETE | `/api/saved/{id}` | Idempotent save / unsave |
| POST | `/api/coupons/{id}/events` | `{ "kind": "copy" }` or `{ "kind": "redeem" }` |
| GET | `/api/notifications` | Saved coupons expiring within 3 days |

Create/update coupon payload:

```json
{
  "brandId": "REPLACE_WITH_BRAND_UUID",
  "couponCode": "EXAMPLE20",
  "description": "Example only; replace with actual merchant terms",
  "category": "Food",
  "discountType": "Percentage",
  "discountValue": 20,
  "minimumOrderAmount": 500,
  "maximumDiscount": 100,
  "expiryDate": null,
  "couponSource": "Manual"
}
```

Choose an actual future UTC expiry when known. Percentage offers require a positive maximum discount; flat offers must omit the maximum discount. Delivery and BOGO offers use value 0 and require a positive minimum order. Category/source/type values use the domain enum names. Codes are trimmed and uppercased; uniqueness is enforced per brand and owner scope. Public legacy coupons remain public.

Recommendation payload:

```json
{ "query": "dinner delivery", "orderAmount": 800, "category": "Food", "limit": 10, "useAi": true }
```

Response contains `mode` (`rules`, `ai`, or `rules-fallback`) and `items`, each with `coupon`, nullable `estimatedSavings` and an authoritative rule-based `reason`. AI reorders candidates; it does not generate merchant terms. `orderAmount` is optional; without it, minimum-order eligibility cannot be confirmed and savings remain null.

Extraction payload:

```json
{ "text": "User-selected offer text", "consentToAi": true }
```

Response: `draft` with nullable extracted fields, plus `requiresReview: true`. Pick the matching brand ID and correct unknown/invalid fields before using POST `/api/coupons`. Merely extracting does not grant permission to share the coupon publicly.
