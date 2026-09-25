package com.couponhub.ai

import android.Manifest
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.app.NotificationCompat
import androidx.core.content.ContextCompat
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import retrofit2.HttpException

class ExpiryWorker(context: Context, params: WorkerParameters) : CoroutineWorker(context, params) {
    override suspend fun doWork(): Result {
        val app = applicationContext as CouponHubApp
        val accountId = inputData.getString("accountId") ?: return Result.success()
        if (app.session.session?.id != accountId) return Result.success()
        if (Build.VERSION.SDK_INT >= 33 && ContextCompat.checkSelfPermission(app, Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED)
            return Result.success()
        return try {
            val notices = app.api.notifications()
            if (app.session.session?.id != accountId) return Result.success()
            val manager = app.getSystemService(NotificationManager::class.java)
            manager.createNotificationChannel(NotificationChannel("expiry", "Coupon expiry", NotificationManager.IMPORTANCE_DEFAULT))
            val prefs = app.getSharedPreferences("expiry-notices", Context.MODE_PRIVATE)
            val intent = PendingIntent.getActivity(app, 0, Intent(app, MainActivity::class.java), PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT)
            notices.forEach {
                val key = "$accountId:${it.id}:${it.expiryDate}"
                if (!prefs.getBoolean(key, false)) {
                    manager.notify(it.id.hashCode(), NotificationCompat.Builder(app, "expiry")
                        .setSmallIcon(android.R.drawable.ic_dialog_info).setContentTitle("A saved coupon expires soon")
                        .setContentText("Open CouponHub to review your saved offers.").setContentIntent(intent).setAutoCancel(true).build())
                    prefs.edit().putBoolean(key, true).apply()
                }
            }
            Result.success()
        } catch (ex: HttpException) {
            if (ex.code() == 401) Result.success() else Result.retry()
        } catch (ex: java.io.IOException) { Result.retry() }
    }
}
