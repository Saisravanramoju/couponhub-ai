package com.couponhub.ai

import android.Manifest
import android.os.Build
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalClipboardManager
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel
import java.time.Instant

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme(colorScheme = lightColorScheme(primary = Color(0xFF185C44), secondary = Color(0xFFAD622E),
                background = Color(0xFFFAF8F2), surface = Color(0xFFFAF8F2))) {
                val vm: CouponViewModel = viewModel(factory = object : ViewModelProvider.Factory {
                    @Suppress("UNCHECKED_CAST") override fun <T : ViewModel> create(modelClass: Class<T>): T = CouponViewModel(application as CouponHubApp) as T
                })
                CouponHub(vm)
            }
        }
    }
}

@Composable fun CouponHub(vm: CouponViewModel) {
    var tab by remember { mutableStateOf("Explore") }
    Column(Modifier.fillMaxSize().background(MaterialTheme.colorScheme.background).statusBarsPadding().navigationBarsPadding().padding(horizontal = 20.dp)) {
        Row(Modifier.fillMaxWidth().padding(vertical = 20.dp), horizontalArrangement = Arrangement.SpaceBetween) {
            Column { Text("CouponHub", style = MaterialTheme.typography.headlineMedium, fontWeight = FontWeight.Bold)
                Text("Make room for more savings.", style = MaterialTheme.typography.bodySmall) }
            if(vm.loggedIn) TextButton(onClick = { vm.refresh() }, enabled = !vm.busy) { Text("Refresh") }
        }
        if(vm.busy) LinearProgressIndicator(Modifier.fillMaxWidth())
        if(vm.message.isNotBlank()) Text(vm.message, Modifier.padding(vertical = 8.dp), style = MaterialTheme.typography.bodyMedium)
        if(!vm.loggedIn) LoginScreen(vm)
        else {
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                listOf("Explore", "For you", "Saved", "Import", "You").forEach { name ->
                    TextButton(onClick = { tab = name }, contentPadding = PaddingValues(4.dp)) {
                        Text(name, fontWeight = if(tab == name) FontWeight.Bold else FontWeight.Normal)
                    }
                }
            }
            Box(Modifier.weight(1f)) {
                when(tab) {
                    "Explore" -> ExploreScreen(vm)
                    "For you" -> RecommendationScreen(vm)
                    "Saved" -> CouponList(vm, vm.saved)
                    "Import" -> ImportScreen(vm)
                    "You" -> ProfileScreen(vm)
                }
            }
        }
    }
}

@Composable fun LoginScreen(vm: CouponViewModel) {
    var email by remember { mutableStateOf("") }; var password by remember { mutableStateOf("") }
    var register by remember { mutableStateOf(false) }
    Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        Spacer(Modifier.height(36.dp))
        Text("Your offers.\nAll together.", style = MaterialTheme.typography.displaySmall, fontWeight = FontWeight.Bold)
        Text("Keep coupons in one place, save favorites, and find offers for your next purchase.")
        OutlinedTextField(email, { email = it }, label = { Text("Email") }, singleLine = true, modifier = Modifier.fillMaxWidth())
        OutlinedTextField(password, { password = it }, label = { Text("Password (12–128 characters)") }, visualTransformation = PasswordVisualTransformation(), singleLine = true, modifier = Modifier.fillMaxWidth())
        Button(onClick = { vm.login(email.trim(), password, register) }, enabled = !vm.busy && email.contains('@') && password.length in 12..128, modifier = Modifier.fillMaxWidth()) { Text(if(register) "Create account" else "Sign in") }
        TextButton(onClick = { register = !register }) { Text(if(register) "Already have an account? Sign in" else "New here? Create an account") }
        Text("Imported coupons are private. AI features send only the text or offer details you choose to the provider.", style = MaterialTheme.typography.bodySmall)
    }
}

@Composable fun SelectField(label: String, selected: String, choices: List<String>, onSelect: (String) -> Unit) {
    var expanded by remember { mutableStateOf(false) }
    Box {
        OutlinedButton(onClick = { expanded = true }, modifier = Modifier.fillMaxWidth()) { Text("$label: $selected") }
        DropdownMenu(expanded, onDismissRequest = { expanded = false }) {
            choices.forEach { choice -> DropdownMenuItem(text = { Text(choice) }, onClick = { onSelect(choice); expanded = false }) }
        }
    }
}

@Composable fun ExploreScreen(vm: CouponViewModel) {
    Column {
        OutlinedTextField(vm.query, { vm.query = it.take(200) }, label = { Text("Search brands, codes or offers") }, modifier = Modifier.fillMaxWidth(), singleLine = true)
        SelectField("Category", vm.category ?: "All", listOf("All") + categories) { vm.category = it.takeUnless { it == "All" } }
        Button(onClick = { vm.search() }, enabled = !vm.busy) { Text("Find coupons") }
        Text("${vm.total} offers", style = MaterialTheme.typography.labelMedium, modifier = Modifier.padding(vertical = 8.dp))
        CouponList(vm, vm.coupons, hasMore = vm.coupons.size < vm.total, loadMore = { vm.search(true) })
    }
}

@Composable fun CouponList(vm: CouponViewModel, coupons: List<Coupon>, hasMore: Boolean = false, loadMore: () -> Unit = {}) {
    if(coupons.isEmpty()) Text("No coupons here yet. Import an offer or try another search.", Modifier.padding(vertical = 24.dp))
    LazyColumn(verticalArrangement = Arrangement.spacedBy(12.dp), contentPadding = PaddingValues(bottom = 24.dp)) {
        items(coupons, key = { it.id }) { CouponCard(vm, it) }
        if(hasMore) item { OutlinedButton(onClick = loadMore, enabled = !vm.busy) { Text("Load more") } }
    }
}

@Composable fun CouponCard(vm: CouponViewModel, coupon: Coupon, detail: String? = null) {
    val clipboard = LocalClipboardManager.current
    var expanded by remember { mutableStateOf(false) }
    var editing by remember { mutableStateOf(false) }
    var confirmDelete by remember { mutableStateOf(false) }
    val expired = coupon.expiryDate?.let { runCatching { Instant.parse(it).isBefore(Instant.now()) }.getOrDefault(false) } ?: false
    Card(colors = CardDefaults.cardColors(containerColor = Color.White), modifier = Modifier.fillMaxWidth()) {
        Column(Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            Text(coupon.brandName.uppercase(), style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.primary)
            Text(when(coupon.discountType) {
                "Percentage" -> "${coupon.discountValue}% off"
                "Flat" -> "₹${coupon.discountValue} off"
                "Cashback" -> if(coupon.maximumDiscount != null) "${coupon.discountValue}% cashback" else "₹${coupon.discountValue} cashback"
                "FreeDelivery" -> "Free delivery"
                "BuyOneGetOne" -> "Buy one, get one"
                else -> "Special offer"
            }, style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold)
            Text(coupon.description)
            if(detail != null) Text(detail, color = MaterialTheme.colorScheme.primary)
            Text(if(expired || !coupon.isActive) "Expired or inactive" else "Expires: ${coupon.expiryDate?.take(10) ?: "Not specified"}", style = MaterialTheme.typography.bodySmall)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedButton(onClick = { clipboard.setText(AnnotatedString(coupon.couponCode)); vm.track(coupon, "copy") }, enabled = !vm.busy && !expired && coupon.isActive) { Text(coupon.couponCode) }
                TextButton(onClick = { vm.toggleSave(coupon) }, enabled = !vm.busy) { Text(if(vm.saved.any { it.id == coupon.id }) "Unsave" else "Save") }
            }
            TextButton(onClick = { expanded = !expanded }) { Text(if(expanded) "Hide details" else "Terms & actions") }
            if(expanded) {
                Text("Minimum order: ${coupon.minimumOrderAmount?.let { "₹$it" } ?: "Not specified"}\nMaximum benefit: ${coupon.maximumDiscount?.let { "₹$it" } ?: "Not specified"}\nSource: ${coupon.couponSource}")
                Text("Check merchant eligibility, payment and account restrictions. CouponHub does not verify redemption.", style = MaterialTheme.typography.bodySmall)
                Row { TextButton(onClick = { vm.track(coupon, "redeem") }, enabled = !vm.busy) { Text("Mark used") }
                    TextButton(onClick = { editing = true }) { Text("Edit") }
                    TextButton(onClick = { confirmDelete = true }) { Text("Delete") } }
                if(vm.account?.isAdmin == true) TextButton(onClick = { vm.publish(coupon) }, enabled = !vm.busy) { Text("Publish to shared catalog") }
            }
        }
    }
    if(confirmDelete) AlertDialog(onDismissRequest = { confirmDelete = false }, title = { Text("Delete coupon?") }, text = { Text("This removes the coupon and its saved references.") }, confirmButton = { TextButton(onClick = { confirmDelete = false; vm.delete(coupon) }) { Text("Delete") } }, dismissButton = { TextButton(onClick = { confirmDelete = false }) { Text("Cancel") } })
    if(editing) AlertDialog(onDismissRequest = { editing = false }, title = { Text("Edit coupon") }, text = {
        Column(Modifier.heightIn(max = 500.dp).verticalScroll(rememberScrollState())) {
            CouponForm(vm, Draft(coupon.brandName, coupon.couponCode, coupon.description, coupon.category, coupon.discountType, coupon.discountValue, coupon.minimumOrderAmount, coupon.maximumDiscount, coupon.expiryDate), coupon) { editing = false }
        }
    }, confirmButton = { TextButton(onClick = { editing = false }) { Text("Close") } })
}

@Composable fun RecommendationScreen(vm: CouponViewModel) {
    var intent by remember { mutableStateOf("") }; var amount by remember { mutableStateOf("") }
    var useAi by remember { mutableStateOf(false) }
    LazyColumn(verticalArrangement = Arrangement.spacedBy(12.dp)) {
        item { Text("A little smarter.\nA little more saved.", style = MaterialTheme.typography.headlineMedium) }
        item { OutlinedTextField(intent, { intent = it.take(500) }, label = { Text("What are you shopping for?") }, modifier = Modifier.fillMaxWidth()) }
        item { OutlinedTextField(amount, { amount = it }, label = { Text("Order amount in ₹ (optional)") }, modifier = Modifier.fillMaxWidth(), singleLine = true) }
        item { Row { Checkbox(useAi, { useAi = it }); Text("Use AI ranking. Sends shopping intent and shortlisted offer details to OpenAI.", Modifier.padding(top = 8.dp)) } }
        item { Button(onClick = { vm.recommend(intent, amount, useAi) }, enabled = !vm.busy) { Text("Find my offers") } }
        vm.recommendations?.let { result ->
            item { Text(when(result.mode) { "ai" -> "Ranked with AI"; "rules-fallback" -> "AI unavailable — ranked using your preferences"; else -> "Ranked using your preferences" }) }
            if(result.items.isEmpty()) item { Text("No eligible coupons found. Try another category or amount.") }
            items(result.items, key = { it.coupon.id }) { CouponCard(vm, it.coupon, "${it.reason}${it.estimatedSavings?.let { saving -> " · Estimated benefit ₹$saving" } ?: ""}") }
        }
        item { Spacer(Modifier.height(20.dp)) }
    }
}

@Composable fun ImportScreen(vm: CouponViewModel) {
    var consent by remember { mutableStateOf(false) }
    val picker = rememberLauncherForActivityResult(ActivityResultContracts.GetContent()) { uri -> uri?.let(vm::ocr) }
    Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(12.dp)) {
        Text("Bring your offers together", style = MaterialTheme.typography.headlineSmall)
        Text("Paste an offer from an email or notification, or choose a screenshot. Remove personal information before AI extraction.")
        OutlinedButton(onClick = { picker.launch("image/*") }, enabled = !vm.busy) { Text("Read a screenshot on this device") }
        SelectField("Source", vm.importSource, listOf("Manual", "Email", "Notification", "OCR", "Screenshot")) { vm.importSource = it }
        OutlinedTextField(vm.importText, { vm.importText = it.take(12000) }, label = { Text("Offer text") }, modifier = Modifier.fillMaxWidth(), minLines = 3)
        Row { Checkbox(consent, { consent = it }); Text("I agree to send this text to OpenAI for extraction.", Modifier.padding(top = 8.dp)) }
        Button(onClick = { vm.extract(consent) }, enabled = !vm.busy && consent && vm.importText.length >= 5) { Text("Extract with AI") }
        HorizontalDivider()
        Text("Review or enter manually", style = MaterialTheme.typography.titleLarge)
        key(vm.draft) { CouponForm(vm, vm.draft) {} }
        Spacer(Modifier.height(20.dp))
    }
}

@Composable fun CouponForm(vm: CouponViewModel, draft: Draft?, editing: Coupon? = null, onSubmit: () -> Unit) {
    var brandId by remember { mutableStateOf(editing?.brandId ?: vm.brands.firstOrNull { it.name.equals(draft?.brandName, true) }?.id ?: "") }
    var code by remember { mutableStateOf(draft?.couponCode ?: "") }; var description by remember { mutableStateOf(draft?.description ?: "") }
    var category by remember { mutableStateOf(draft?.category?.takeIf { it in categories } ?: "Other") }
    var type by remember { mutableStateOf(draft?.discountType?.takeIf { it in discountTypes } ?: "Flat") }
    var value by remember { mutableStateOf(draft?.discountValue?.toString() ?: "") }
    var minimum by remember { mutableStateOf(draft?.minimumOrderAmount?.toString() ?: "") }
    var maximum by remember { mutableStateOf(draft?.maximumDiscount?.toString() ?: "") }
    var expiry by remember { mutableStateOf(draft?.expiryDate ?: "") }; var error by remember { mutableStateOf("") }
    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
        if(editing == null) SelectField("Brand", vm.brands.firstOrNull { it.id == brandId }?.name ?: "Select brand", vm.brands.map { it.name }) { name -> brandId = vm.brands.first { it.name == name }.id }
        if(vm.brands.isEmpty()) Text("No brands yet. An administrator must add a brand first.")
        OutlinedTextField(code, { code = it.take(100) }, label = { Text("Coupon code") }, modifier = Modifier.fillMaxWidth())
        OutlinedTextField(description, { description = it.take(500) }, label = { Text("Description and restrictions") }, modifier = Modifier.fillMaxWidth())
        SelectField("Category", category, categories) { category = it }
        SelectField("Discount", type, discountTypes) { type = it }
        OutlinedTextField(value, { value = it }, label = { Text("Value (% or ₹; 0 for delivery / BOGO)") }, modifier = Modifier.fillMaxWidth())
        OutlinedTextField(minimum, { minimum = it }, label = { Text("Minimum order ₹ (optional)") }, modifier = Modifier.fillMaxWidth())
        OutlinedTextField(maximum, { maximum = it }, label = { Text("Maximum discount ₹ (required for %)") }, modifier = Modifier.fillMaxWidth())
        OutlinedTextField(expiry, { expiry = it }, label = { Text("Expiry UTC: 2026-12-31T18:29:59Z (optional)") }, modifier = Modifier.fillMaxWidth())
        if(error.isNotBlank()) Text(error, color = MaterialTheme.colorScheme.error)
        Button(enabled = !vm.busy, onClick = {
            try {
                require(brandId.isNotBlank() && code.isNotBlank() && description.isNotBlank())
                fun number(text: String): Double? = if(text.isBlank()) null else text.toDouble().also { require(it.isFinite() && it in 0.0..999999.0) }
                val amount = number(value) ?: throw IllegalArgumentException()
                val date = expiry.takeIf { it.isNotBlank() }?.let { Instant.parse(it).also { date -> require(date.isAfter(Instant.now())) }.toString() }
                vm.saveCoupon(CreateCoupon(brandId, code.trim(), description.trim(), category, type, amount, number(minimum), number(maximum), date, editing?.couponSource ?: vm.importSource), editing?.id)
                error = ""; onSubmit()
            } catch(ex: Exception) { error = "Select a brand, enter valid values, and use a future UTC expiry or leave it blank." }
        }) { Text(if(editing == null) "Save private coupon" else "Save changes") }
    }
}

@Composable fun ProfileScreen(vm: CouponViewModel) {
    var selected by remember(vm.account) { mutableStateOf(vm.account?.categories ?: emptyList()) }
    var brand by remember { mutableStateOf("") }
    val permission = rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()) { granted -> if(granted) vm.enableReminders() }
    Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(8.dp)) {
        Text(vm.account?.email ?: "Your account", style = MaterialTheme.typography.titleLarge)
        Text("Favorite categories", style = MaterialTheme.typography.titleMedium)
        categories.forEach { category -> Row {
            Checkbox(category in selected, { checked -> selected = if(checked) selected + category else selected - category })
            Text(category, Modifier.padding(top = 12.dp))
        } }
        Button(onClick = { vm.preferences(selected) }, enabled = !vm.busy) { Text("Save preferences") }
        OutlinedButton(onClick = { if(Build.VERSION.SDK_INT >= 33) permission.launch(Manifest.permission.POST_NOTIFICATIONS) else vm.enableReminders() }) { Text("Enable expiry reminders") }
        TextButton(onClick = { vm.disableReminders() }) { Text("Disable reminders") }
        if(vm.account?.isAdmin == true) {
            HorizontalDivider(); Text("Manage brands", style = MaterialTheme.typography.titleLarge)
            OutlinedTextField(brand, { brand = it.take(100) }, label = { Text("Brand name") })
            Button(onClick = { vm.createBrand(brand.trim()) }, enabled = !vm.busy && brand.isNotBlank()) { Text("Add brand") }
        }
        TextButton(onClick = { vm.logout() }, enabled = !vm.busy) { Text("Sign out and clear local data") }
        Spacer(Modifier.height(20.dp))
    }
}
