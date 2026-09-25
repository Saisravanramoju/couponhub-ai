package com.couponhub.ai

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class CaptureIntakeTest {
    @Test
    fun completeDraftWithKnownBrandHasNoMissingFields() {
        val draft = Draft("Swiggy", "SAVE20", "20% off", "Food", "Percentage", 20.0, 199.0, 100.0, null)
        assertTrue(missingCouponFields(draft, listOf("Swiggy")).isEmpty())
    }

    @Test
    fun percentageDraftReportsReviewFields() {
        val draft = Draft("Unknown shop", null, "Offer", "Food", "Percentage", 20.0, null, null, null)
        assertEquals(listOf("brand", "coupon code", "maximum discount"), missingCouponFields(draft, listOf("Swiggy")))
    }

    @Test
    fun capturedTextWithoutExtractionNeedsOfferDetails() {
        assertEquals(listOf("offer details"), missingCouponFields(null, emptyList()))
    }
}
