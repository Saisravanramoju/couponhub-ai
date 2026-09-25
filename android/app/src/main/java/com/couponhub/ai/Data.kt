package com.couponhub.ai

import android.app.Application
import android.content.Context
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import androidx.room.*
import com.google.gson.Gson
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.security.KeyStore
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec
import java.util.concurrent.TimeUnit

class SessionStore(context: Context) {
    private val prefs = context.getSharedPreferences("session", Context.MODE_PRIVATE)
    private val gson = Gson()
    private fun key(): SecretKey {
        val store = KeyStore.getInstance("AndroidKeyStore").apply { load(null) }
        return (store.getKey("couponhub-session", null) as? SecretKey) ?: KeyGenerator.getInstance(
            KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore").apply {
            init(KeyGenParameterSpec.Builder("couponhub-session", KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE).build())
        }.generateKey()
    }
    @Volatile var session: LoginResult? = runCatching {
        val encoded = prefs.getString("encrypted", null) ?: return@runCatching null
        val bytes = Base64.decode(encoded, Base64.NO_WRAP)
        val cipher = Cipher.getInstance("AES/GCM/NoPadding")
        cipher.init(Cipher.DECRYPT_MODE, key(), GCMParameterSpec(128, bytes.copyOfRange(0, 12)))
        gson.fromJson(String(cipher.doFinal(bytes.copyOfRange(12, bytes.size)), Charsets.UTF_8), LoginResult::class.java)
    }.getOrNull()
        private set
    @Synchronized fun save(value: LoginResult) {
        val cipher = Cipher.getInstance("AES/GCM/NoPadding").apply { init(Cipher.ENCRYPT_MODE, key()) }
        prefs.edit().putString("encrypted", Base64.encodeToString(cipher.iv + cipher.doFinal(gson.toJson(value).toByteArray(Charsets.UTF_8)), Base64.NO_WRAP)).commit()
        session = value
    }
    @Synchronized fun clear() { session = null; prefs.edit().clear().commit() }
}
@Entity(tableName = "coupon_cache")
data class CacheEntry(@PrimaryKey val key: String, val json: String, val updatedAt: Long)
@Dao interface CacheDao {
    @Query("SELECT * FROM coupon_cache WHERE `key` = :key") suspend fun get(key: String): CacheEntry?
    @Insert(onConflict = OnConflictStrategy.REPLACE) suspend fun put(entry: CacheEntry)
    @Query("DELETE FROM coupon_cache") suspend fun clear()
}
@Database(entities = [CacheEntry::class], version = 1, exportSchema = false)
abstract class CouponDatabase : RoomDatabase() { abstract fun cache(): CacheDao }
class CouponHubApp : Application() {
    lateinit var session: SessionStore; private set
    lateinit var db: CouponDatabase; private set
    lateinit var api: CouponApi; private set
    override fun onCreate() {
        super.onCreate()
        session = SessionStore(this)
        db = Room.databaseBuilder(this, CouponDatabase::class.java, "couponhub.db").build()
        val client = OkHttpClient.Builder().callTimeout(30, TimeUnit.SECONDS).addInterceptor { chain ->
            val request = chain.request().newBuilder()
            session.session?.let { request.header("Authorization", "Bearer ${it.accessToken}") }
            chain.proceed(request.build())
        }.build()
        api = Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL).client(client)
            .addConverterFactory(GsonConverterFactory.create()).build().create(CouponApi::class.java)
    }
}
