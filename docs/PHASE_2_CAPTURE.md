# Phase 2 cross-app coupon capture

## Implemented vertical slice

The first Phase 2 slice accepts content only after a user action:

- Android Sharesheet `ACTION_SEND` for `text/plain` and `image/*`;
- Android Sharesheet `ACTION_SEND_MULTIPLE` for `image/*`;
- image selection from the visible Import screen;
- clipboard text read only when the user taps **Paste**.

Shared and selected images are passed to ML Kit on the device. CouponHub keeps the recognized text, not the image, and does not send an image to the API or OpenAI.

## Data flow

1. `MainActivity` parses a supported share intent.
2. The signed-in app opens Import and sends image URIs to on-device OCR.
3. Recognized/shared text is capped at 12,000 characters and stored in the account's local Room draft table.
4. The user can edit the text, save it for later, or explicitly consent to AI extraction.
5. AI output remains a draft. Missing required fields are recorded and shown in **Incomplete coupons**.
6. Saving through the existing coupon endpoint removes the local draft. Discard and sign-out also remove applicable local draft data.

Draft states:

| State | Meaning |
| --- | --- |
| `CAPTURED` | Text exists but AI extraction has not produced structured details |
| `NEEDS_DETAILS` | Structured details exist but one or more required fields are missing |
| `READY_FOR_REVIEW` | Required draft fields are present; final user review and save are still required |

## Privacy and platform boundaries

- No Accessibility Service.
- No notification-listener capture.
- No background clipboard monitoring.
- No silent or continuous screenshots.
- No attempt to bypass `FLAG_SECURE`.
- No direct interception of Circle to Search.
- No AI call without the existing consent checkbox.
- No source image upload to the backend or model provider.

Circle to Search remains compatible through its user-visible **Copy** action followed by CouponHub's explicit **Paste** action.

## Manual verification

| Scenario | Expected result |
| --- | --- |
| Share coupon text while signed in | Import opens, text is editable, one incomplete draft appears |
| Share one screenshot | Local OCR populates text; the image is not uploaded |
| Share multiple screenshots | OCR text is combined in share order and capped at 12,000 characters |
| Tap Paste with coupon text | Clipboard is read once and text is saved as an incomplete draft |
| Tap Paste with no text | A non-destructive message is shown |
| AI returns partial fields | Draft status is `NEEDS_DETAILS` with missing fields listed |
| AI returns HTTP 503 | Captured text remains available for manual entry |
| Save a valid private coupon | The corresponding incomplete draft is removed |
| Discard an incomplete coupon | Only that local draft is removed |
| Sign out | Session, cache, reminders, and that account's local incomplete drafts are cleared |
| Receive a share while signed out | Content waits in the activity until successful sign-in; it is not stored under another account |

## Deferred work

A one-shot screen capture tile using Android MediaProjection is intentionally deferred. If added, it must request Android consent for every session, capture a single frame, stop projection immediately, run OCR locally, show a foreground notification only while projection is active, and respect protected screens.
