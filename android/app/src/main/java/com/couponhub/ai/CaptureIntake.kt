package com.couponhub.ai

import android.content.ContentResolver
import android.content.Intent
import android.net.Uri

data class SharedCouponPayload(val text: String?, val imageUris: List<Uri>)

object SharedCouponIntentParser {
    @Suppress("DEPRECATION")
    fun parse(intent: Intent?): SharedCouponPayload? {
        val action = intent?.action
        if (action != Intent.ACTION_SEND && action != Intent.ACTION_SEND_MULTIPLE) return null

        val text = intent.getCharSequenceExtra(Intent.EXTRA_TEXT)?.toString()?.trim()
            ?.take(12_000)?.takeIf { it.isNotBlank() }
        val imageUris = linkedSetOf<Uri>()
        if (intent.type?.startsWith("image/") == true) {
            if (action == Intent.ACTION_SEND) {
                (intent.getParcelableExtra(Intent.EXTRA_STREAM) as? Uri)?.let(imageUris::add)
            } else {
                intent.getParcelableArrayListExtra<Uri>(Intent.EXTRA_STREAM)?.let(imageUris::addAll)
            }
            intent.clipData?.let { clip ->
                repeat(clip.itemCount) { index -> clip.getItemAt(index).uri?.let(imageUris::add) }
            }
        }

        val readableImages = imageUris.filter { it.scheme == ContentResolver.SCHEME_CONTENT }.take(10)
        return if (text == null && readableImages.isEmpty()) null else SharedCouponPayload(text, readableImages)
    }
}

fun missingCouponFields(draft: Draft?, knownBrandNames: Collection<String>): List<String> {
    if (draft == null) return listOf("offer details")
    val knownBrands = knownBrandNames.map { it.trim().lowercase() }.toSet()
    return buildList {
        if (draft.brandName.isNullOrBlank() || draft.brandName.trim().lowercase() !in knownBrands) add("brand")
        if (draft.couponCode.isNullOrBlank()) add("coupon code")
        if (draft.description.isNullOrBlank()) add("description")
        if (draft.category.isNullOrBlank()) add("category")
        if (draft.discountType.isNullOrBlank()) add("discount type")
        if (draft.discountValue == null) add("discount value")
        if (draft.discountType == "Percentage" && draft.maximumDiscount == null) add("maximum discount")
    }
}
