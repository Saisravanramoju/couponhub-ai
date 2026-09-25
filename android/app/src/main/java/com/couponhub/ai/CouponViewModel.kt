package com.couponhub.ai

import android.app.NotificationManager
import android.net.Uri
import androidx.compose.runtime.*
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import androidx.work.*
import com.google.gson.Gson
import com.google.mlkit.vision.common.InputImage
import com.google.mlkit.vision.text.TextRecognition
import com.google.mlkit.vision.text.latin.TextRecognizerOptions
import kotlinx.coroutines.launch
import kotlinx.coroutines.tasks.await
import retrofit2.HttpException
import java.util.concurrent.TimeUnit

class CouponViewModel(private val app: CouponHubApp) : AndroidViewModel(app) {
    var loggedIn by mutableStateOf(app.session.session != null); private set
    var busy by mutableStateOf(false); private set
    var message by mutableStateOf(""); private set
    var account by mutableStateOf<Account?>(null); private set
    var coupons by mutableStateOf<List<Coupon>>(emptyList()); private set
    var saved by mutableStateOf<List<Coupon>>(emptyList()); private set
    var brands by mutableStateOf<List<Brand>>(emptyList()); private set
    var recommendations by mutableStateOf<Recommendations?>(null); private set
    var draft by mutableStateOf<Draft?>(null); private set
    var importText by mutableStateOf("")
    var importSource by mutableStateOf("Manual")
    var query by mutableStateOf("")
    var category by mutableStateOf<String?>(null)
    var page by mutableStateOf(1); private set
    var total by mutableStateOf(0); private set
    private val gson = Gson()
    init { if (loggedIn) refresh() }
    private fun action(block: suspend () -> Unit) {
        if (busy) return
        viewModelScope.launch {
            busy = true; message = ""
            try { block() }
            catch (ex: HttpException) {
                if (ex.code() == 401) { clearSession(); message = "Please sign in again." }
                else message = when(ex.code()) {
                    400 -> "Check all fields and coupon terms. Percentage offers need a maximum discount."
                    403 -> "You can only change your own coupons. Admin access is required to publish."
                    409 -> "This record already exists."
                    429 -> "Too many requests. Please wait a minute."
                    503 -> "AI is unavailable. You can still add coupons manually."
                    else -> "Request failed (${ex.code()}). Please retry."
                }
            } catch (ex: kotlinx.coroutines.CancellationException) { throw ex }
            catch (ex: Exception) { message = "Could not complete the request. Check your connection and entered values." }
            finally { busy = false }
        }
    }
    fun login(email: String, password: String, register: Boolean) = action {
        val result = if (register) app.api.register(Credentials(email, password)) else app.api.login(Credentials(email, password))
        app.session.save(result); loggedIn = true
        load()
    }
    fun refresh() = action { load() }
    private suspend fun load() {
        loadSaved()
        account = app.api.me()
        brands = app.api.brands()
        searchInternal(false)
    }
    private suspend fun loadSaved() {
        val key = "${app.session.session!!.id}:saved"
        try {
            saved = app.api.saved()
            app.db.cache().put(CacheEntry(key, gson.toJson(saved), System.currentTimeMillis()))
        } catch (ex: java.io.IOException) {
            val cached = app.db.cache().get(key)
            saved = cached?.let { gson.fromJson(it.json, Array<Coupon>::class.java).toList() } ?: emptyList()
            message = "Offline: saved offers may be out of date. Verify before using."
        }
    }
    fun search(next: Boolean = false) = action { searchInternal(next) }
    private suspend fun searchInternal(next: Boolean) {
        val nextPage = if (next) page + 1 else 1
        val result = app.api.search(query, category, nextPage)
        coupons = if(next) coupons + result.items else result.items
        page = nextPage; total = result.total
    }
    fun toggleSave(coupon: Coupon) = action {
        if (saved.any { it.id == coupon.id }) app.api.unsave(coupon.id) else app.api.save(coupon.id)
        loadSaved()
    }
    fun recommend(intent: String, amount: String, useAi: Boolean) = action {
        val number = amount.takeIf { it.isNotBlank() }?.toDouble()
        require(number == null || number.isFinite() && number >= 0)
        recommendations = app.api.recommend(RecommendationRequest(intent, number, useAi = useAi))
    }
    fun extract(consent: Boolean) = action {
        draft = app.api.extract(ImportText(importText, consent)).draft
        message = "Review every field and select the correct brand before saving."
    }
    fun ocr(uri: Uri) = action {
        val recognizer = TextRecognition.getClient(TextRecognizerOptions.DEFAULT_OPTIONS)
        try {
            importText = recognizer.process(InputImage.fromFilePath(app, uri)).await().text.take(12000)
            importSource = "OCR"
            message = "Text recognized on your device. Review it before AI extraction."
        } finally { recognizer.close() }
    }
    fun saveCoupon(request: CreateCoupon, editId: String? = null) = action {
        if (editId == null) app.api.create(request) else app.api.update(editId, request)
        draft = null; importText = ""; importSource = "Manual"
        searchInternal(false)
        message = "Coupon saved privately to your account."
    }
    fun delete(coupon: Coupon) = action { app.api.delete(coupon.id); load(); message = "Coupon deleted." }
    fun publish(coupon: Coupon) = action { app.api.publish(coupon.id); message = "Coupon published to the shared catalog." }
    fun track(coupon: Coupon, kind: String) = action { app.api.track(coupon.id, CouponEvent(kind)); message = if(kind == "redeem") "Marked as used by you; merchant redemption is not verified." else "Copied." }
    fun preferences(selected: List<String>) = action { app.api.preferences(Preferences(selected)); account = app.api.me(); message = "Preferences saved." }
    fun createBrand(name: String) = action { app.api.createBrand(CreateBrand(name)); brands = app.api.brands(); message = "Brand added." }
    fun enableReminders() {
        val id = app.session.session?.id ?: return
        val request = PeriodicWorkRequestBuilder<ExpiryWorker>(12, TimeUnit.HOURS)
            .setConstraints(Constraints.Builder().setRequiredNetworkType(NetworkType.CONNECTED).build())
            .setInputData(workDataOf("accountId" to id)).build()
        WorkManager.getInstance(app).enqueueUniquePeriodicWork("coupon-expiry", ExistingPeriodicWorkPolicy.UPDATE, request)
        message = "Expiry reminders enabled. Android schedules checks approximately every 12 hours."
    }
    fun disableReminders() { WorkManager.getInstance(app).cancelUniqueWork("coupon-expiry"); message = "Expiry reminders disabled." }
    fun logout() = action { try { app.api.logout() } finally { clearSession() } }
    private suspend fun clearSession() {
        app.session.clear(); WorkManager.getInstance(app).cancelUniqueWork("coupon-expiry")
        app.getSystemService(NotificationManager::class.java).cancelAll()
        app.db.cache().clear(); app.getSharedPreferences("expiry-notices", 0).edit().clear().apply()
        loggedIn = false; account = null; coupons = emptyList(); saved = emptyList(); brands = emptyList()
        recommendations = null; draft = null; importText = ""
    }
}
