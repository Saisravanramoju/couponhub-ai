package com.couponhub.ai

import retrofit2.http.*

val categories = listOf("Food", "Shopping", "Entertainment", "Travel", "Fashion", "Electronics", "Grocery", "Medicine", "Recharge", "Other")
val discountTypes = listOf("Percentage", "Flat", "Cashback", "FreeDelivery", "BuyOneGetOne", "Other")
data class Credentials(val email: String, val password: String)
data class LoginResult(val accessToken: String, val expiresAt: String, val id: String, val email: String, val isAdmin: Boolean)
data class Account(val id: String, val email: String, val isAdmin: Boolean, val categories: List<String>)
data class Preferences(val categories: List<String>)
data class Brand(val id: String, val name: String, val category: String)
data class Coupon(val id: String, val brandId: String, val brandName: String, val couponCode: String,
    val description: String, val category: String, val discountType: String, val discountValue: Double,
    val minimumOrderAmount: Double?, val maximumDiscount: Double?, val expiryDate: String?, val isActive: Boolean,
    val couponSource: String)
data class SearchResult(val total: Int, val page: Int, val pageSize: Int, val items: List<Coupon>)
data class RecommendationRequest(val query: String, val orderAmount: Double?, val category: String? = null,
    val limit: Int = 20, val useAi: Boolean)
data class Recommended(val coupon: Coupon, val estimatedSavings: Double?, val reason: String)
data class Recommendations(val mode: String, val items: List<Recommended>)
data class ImportText(val text: String, val consentToAi: Boolean)
data class Draft(val brandName: String?, val couponCode: String?, val description: String?, val category: String?,
    val discountType: String?, val discountValue: Double?, val minimumOrderAmount: Double?,
    val maximumDiscount: Double?, val expiryDate: String?)
data class Extraction(val draft: Draft, val requiresReview: Boolean)
data class CreateCoupon(val brandId: String, val couponCode: String, val description: String, val category: String,
    val discountType: String, val discountValue: Double, val minimumOrderAmount: Double?,
    val maximumDiscount: Double?, val expiryDate: String?, val couponSource: String)
data class CouponEvent(val kind: String)
data class ExpiryNotice(val id: String, val couponCode: String, val expiryDate: String, val message: String)
data class CreateBrand(val name: String, val category: String = "Other", val logoUrl: String? = null)
interface CouponApi {
    @POST("api/account/register") suspend fun register(@Body credentials: Credentials): LoginResult
    @POST("api/account/login") suspend fun login(@Body credentials: Credentials): LoginResult
    @POST("api/account/logout") suspend fun logout()
    @GET("api/account") suspend fun me(): Account
    @PUT("api/account/preferences") suspend fun preferences(@Body preferences: Preferences)
    @GET("api/brands") suspend fun brands(): List<Brand>
    @POST("api/brands") suspend fun createBrand(@Body brand: CreateBrand): Brand
    @GET("api/search") suspend fun search(@Query("q") q: String, @Query("category") category: String?, @Query("page") page: Int): SearchResult
    @POST("api/recommendations") suspend fun recommend(@Body request: RecommendationRequest): Recommendations
    @GET("api/saved") suspend fun saved(): List<Coupon>
    @PUT("api/saved/{id}") suspend fun save(@Path("id") id: String)
    @DELETE("api/saved/{id}") suspend fun unsave(@Path("id") id: String)
    @POST("api/imports/extract") suspend fun extract(@Body request: ImportText): Extraction
    @POST("api/coupons") suspend fun create(@Body request: CreateCoupon): Coupon
    @PUT("api/coupons/{id}") suspend fun update(@Path("id") id: String, @Body request: CreateCoupon): Coupon
    @DELETE("api/coupons/{id}") suspend fun delete(@Path("id") id: String)
    @POST("api/coupons/{id}/publish") suspend fun publish(@Path("id") id: String)
    @POST("api/coupons/{id}/events") suspend fun track(@Path("id") id: String, @Body event: CouponEvent)
    @GET("api/notifications") suspend fun notifications(): List<ExpiryNotice>
}
