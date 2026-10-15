# راهنمای کامل جای‌گذاری Assetهای مسیر جدید

این فایل مربوط به صحنه `Assets/Project/Scene/Gameplay.unity` و مسیر جدید بازی است.

## نکته بسیار مهم درباره ساختار فعلی

در حال حاضر `StoryRouteController` بیشتر اشیای مسیر جدید را هنگام زدن Play می‌سازد. این اشیا زیر آبجکت زیر دیده می‌شوند:

`Reworked Story Route - Replace Placeholders`

این آبجکت و فرزندانش Runtime هستند. یعنی:

- فقط در Play Mode ظاهر می‌شوند.
- تغییر Sprite آن‌ها در Play Mode بعد از Stop ذخیره نمی‌شود.
- فعلاً نباید Assetهای اصلی را با Drag & Drop روی این آبجکت‌های Runtime قرار داد.
- برای اتصال دائمی بک‌گراندها و Props باید ابتدا برایشان فیلد Sprite یا Prefab داخل `StoryRouteController` اضافه شود.

تنها Assetهایی که همین حالا جای اتصال دائمی دارند، شش تصویر کات‌سین هستند. روش اتصال آن‌ها در بخش بعد آمده است.

همچنین اشیای مسیر قدیمی، مثل معمای چرخ‌دستی، هنگام شروع بازی توسط کنترلر جدید غیرفعال می‌شوند. بنابراین Asset جدید را روی اشیای قدیمی وصل نکن.

---

## تنظیم Import تصاویر

تمام تصاویر گیم‌پلی باید این تنظیمات را داشته باشند:

- `Texture Type`: Sprite (2D and UI)
- `Sprite Mode`: Multiple برای Sheetها و Single برای تصاویر تکی
- `Alpha Is Transparency`: روشن
- `Generate Mip Maps`: خاموش
- `Filter Mode`: Bilinear، مگر اینکه سبک نهایی Pixel Art باشد
- `Compression`: Normal یا None برای UI و تصاویر کات‌سین

فایل‌های پوشه `final-assets-v2` در حال حاضر به شکل Multiple وارد شده‌اند. برای Drag & Drop باید فلش کنار فایل را باز کنی و Sprite داخلی مثل `_0` را بگیری، نه خود فایل PNG را.

---

## تصاویر کات‌سین — همین حالا قابل اتصال

### روش اتصال مشترک

1. Play Mode خاموش باشد.
2. صحنه `Gameplay` را باز کن.
3. در Hierarchy آبجکت `Game Progress` را انتخاب کن.
4. در Inspector کامپوننت `Story Route Controller` را پیدا کن.
5. فلش کنار PNG را در Project باز کن.
6. Sprite داخلی تصویر را داخل فیلد مشخص‌شده Drag کن.

### 1. برگه آموزش تعامل

- فایل:
  `Assets/Project/Art/final-assets-v2/CUT-01-tutorial-paper-overlay-v2.png`
- فیلد مقصد:
  `Story Route Controller > Tutorial Paper Image`
- زمان نمایش:
  بعد از برداشتن برگه آموزشی از زمین در سکشن اول.
- نوع استفاده:
  UI Image بزرگ در وسط صفحه.

### 2. اولین سایه ترسناک

- فایل:
  `Assets/Project/Art/final-assets-v2/CUT-02-first-shadow-on-curtain-v2.png`
- فیلد مقصد:
  `Story Route Controller > First Shadow Image`
- زمان نمایش:
  اولین بار که کاراکتر روی جعبه می‌رود تا کلید را بردارد.
- نوع استفاده:
  تصویر کات‌سین تمام‌صفحه.

### 3. کناررفتن پرده

- فایل:
  `Assets/Project/Art/final-assets-v2/CUT-03-curtain-reveal-real-set-pieces-v2.png`
- فیلد مقصد:
  `Story Route Controller > Curtain Opening Image`
- زمان نمایش:
  وقتی بازیکن در طبقه بالا وزنه را به طناب وصل می‌کند.
- نوع استفاده:
  تصویر موقت کات‌سین بازشدن پرده.

### 4. مشخص‌شدن دکور واقعی

- فایل:
  `Assets/Project/Art/final-assets-v2/CUT-04-shadow-was-decoration-v2.png`
- فیلد مقصد:
  `Story Route Controller > Decoration Reveal Image`
- زمان نمایش:
  بعد از برگشت بازیکن از طبقه بالا به پشت صحنه اصلی.
- نوع استفاده:
  کات‌سین مشخص‌شدن عامل سایه روی پرده اصلی.

### 5. فهمیدن اینکه سایه متعلق به خود کاراکتر است

- فایل:
  `Assets/Project/Art/final-assets-v2/CUT-05-child-own-shadow-realization-v2.png`
- فیلد مقصد:
  `Story Route Controller > Own Shadow Image`
- زمان نمایش:
  یک بار هنگام ترسیدن در اتاق فیوز و یک بار هنگام برگشت.
- نوع استفاده:
  کات‌سین تمام‌صفحه.

### 6. مشخص‌شدن سایه لباس‌ها

- فایل:
  `Assets/Project/Art/final-assets-v2/CUT-06-final-costume-shadow-revelation-v2.png`
- فیلد مقصد:
  `Story Route Controller > Costume Reveal Image`
- زمان نمایش:
  در مسیر برگشت، وقتی بازیکن دوباره روی جعبه می‌رود.
- نوع استفاده:
  کات‌سین نهایی معمای سایه اتاق کلید.

---

## بک‌گراندهای سکشن‌ها — آماده هستند ولی هنوز فیلد اتصال ندارند

برای این موارد باید در مرحله جایگزینی آرت، داخل `StoryRouteController` فیلد Sprite ساخته شود. هر تصویر باید روی یک `SpriteRenderer` با Sorting Order پایین قرار بگیرد.

### 7. بک‌گراند سکشن شروع

- فایل:
  `BG-01-starting-backstage-two-doors-v2.png`
- سکشن مقصد:
  سکشن 0، مرکز دوربین `X = 0`
- آبجکت مقصد پیشنهادی:
  `Room 01 Background`
- Sorting Order پیشنهادی:
  `-20`
- کاربرد:
  محل شروع بازی و نمایش دو در چپ و راست.

### 8. بک‌گراند اتاق کلید و جعبه

- فایل:
  `BG-02-locked-corridor-box-puzzle-v2.png`
- سکشن مقصد:
  سکشن 1، مرکز دوربین `X = 18`
- آبجکت مقصد پیشنهادی:
  `Room 02 Background`
- Sorting Order پیشنهادی:
  `-20`
- کاربرد:
  در قفل‌شده، محل هل‌دادن جعبه و کلید دیواری.

### 9. بک‌گراند اتاق جعبه‌فیوز

- فایل:
  `BG-03-dark-fuse-box-room-v2.png`
- سکشن مقصد:
  سکشن 2، مرکز دوربین `X = 36`
- آبجکت مقصد پیشنهادی:
  `Room 03 Background`
- Sorting Order پیشنهادی:
  `-20`
- کاربرد:
  محیط تاریک، جعبه‌فیوز، جعبه‌ابزار و چراغ انتهای مسیر.

### 10. بک‌گراند پشت صحنه اصلی

- فایل:
  `BG-04-main-backstage-curtain-room-v2.png`
- سکشن مقصد:
  سکشن 3، مرکز دوربین `X = 54`
- آبجکت مقصد پیشنهادی:
  `Room 04 Background`
- Sorting Order پیشنهادی:
  `-20`
- کاربرد:
  پرده اصلی، سایه دکور، وزنه، در بسته روی صحنه و راه‌پله.

### 11. بک‌گراند طبقه بالای پشت صحنه

- فایل:
  `BG-05-upper-rope-area-v2.png`
- سکشن مقصد:
  سکشن 4، مرکز دوربین `X = 72`
- Placeholder فعلی:
  `Upper Backstage Background (Replace Art)`
- Sorting Order پیشنهادی:
  `-20`
- کاربرد:
  بخش بالایی طناب پرده و مسیر برگشت از راه‌پله.

### 12. نمای حیاط

- فایل:
  `BG-06-courtyard-glimpse-v2.png`
- مقصد:
  پایان بازی هنگام حرکت کاراکتر به سمت در حیاط.
- وضعیت فعلی:
  پایان فعلی قبل از ورود کامل به حیاط Fade To Black می‌شود؛ این تصویر هنوز استفاده نمی‌شود.
- استفاده پیشنهادی:
  یا قبل از سیاه‌شدن برای یک لحظه نمایش داده شود، یا به‌عنوان تصویر پایانی استفاده شود.

---

## Props سکشن شروع — آماده هستند ولی هنوز فیلد اتصال ندارند

### 13. دو در ورودی

- فایل:
  `PROP-01-two-entrance-doors-v2.png`
- از Sheet چه چیزهایی لازم است:
  - در بسته راهرو
  - در باز راهرو، اگر بعداً انیمیشن بازشدن اضافه شد
  - در بسته حیاط
  - در باز حیاط، فقط در صورت نمایش بازشدن
- Placeholderهای فعلی:
  - `Right Hallway Door (Replace Art)`
  - `Left Courtyard Door (Replace Art)`
- Sorting Order پیشنهادی:
  `6`
- Collider:
  باید روی آبجکت اصلی Placeholder باقی بماند؛ فقط SpriteRenderer عوض شود.

### 14. برگه روی زمین

- فایل:
  `PROP-03-tutorial-floor-item-paper-v2.png`
- Placeholder فعلی:
  `Tutorial Item (Replace Art)`
- محل:
  سکشن شروع، حدود `X = 0`
- Sorting Order پیشنهادی:
  `6`
- Collider:
  Collider تعاملی موجود حفظ شود.

---

## Props اتاق کلید — آماده هستند ولی هنوز فیلد اتصال ندارند

### 15. در قفل‌شده، کلید و جاکلیدی

- فایل:
  `PROP-02-locked-corridor-door-key-holder-v2.png`
- Spriteهای موردنیاز از Sheet:
  - در قفل‌شده
  - جاکلیدی دیواری
  - کلید
- Placeholderهای فعلی:
  - `Locked Key Room Door (Replace Art)`
  - `High Key (Replace Art)`
- نکته:
  بهتر است جاکلیدی یک SpriteRenderer جدا و غیرتعاملی باشد. کلید باید همان آبجکتی باشد که پس از برداشتن غیرفعال می‌شود.

### 16. جعبه قابل هل‌دادن

- فایل:
  `PROP-04-pushable-box-kit-v2.png`
- Sprite پیشنهادی:
  یکی از جعبه‌های سالم Sheet؛ جعبه‌ای که فرم ساده‌تری دارد برای Collider مناسب‌تر است.
- Placeholder فعلی:
  `Push Box (Replace Art)`
- نکته مهم:
  فقط SpriteRenderer جایگزین شود. کامپوننت `StoryPushBoxInteractable` و Collider نباید حذف شوند.

### 17. منبع واقعی سایه لباس‌ها

- فایل‌های مرتبط:
  - `DEC-02-backstage-utility-props-sheet-v2.png`
  - `SHADOW-01-costume-branch-source-masks-v2.png`
- مقصد:
  دیوار یا پشت پرده نزدیک محل کلید.
- وضعیت فعلی:
  در جهان بازی نمایش داده نمی‌شود و فقط کات‌سین `CUT-06` حقیقت را نشان می‌دهد.
- استفاده پیشنهادی:
  لباس یا رگال از `DEC-02` برای جسم واقعی و ماسک لباس از `SHADOW-01` برای سایه استفاده شود.

---

## Props اتاق جعبه‌فیوز — آماده هستند ولی هنوز فیلد اتصال ندارند

### 18. جعبه‌فیوز

- فایل:
  `PROP-05-fuse-box-red-indicator-v2.png`
- Spriteهای موردنیاز از Sheet:
  - جعبه بسته با چراغ قرمز برای حالت خراب
  - حالت باز، در صورت نمایش انیمیشن تعمیر
  - چراغ سبز/زرد یا Indicator جدا برای حالت تعمیرشده
- Placeholder فعلی:
  `Fuse Box (Replace Art)`
- رفتار:
  قبل از تعمیر حالت قرمز و بعد از تعمیر حالت روشن استفاده شود.
- Collider:
  Collider تعاملی فعلی حفظ شود.

### 19. جعبه‌ابزار و قطعه فیوز

- فایل:
  `PROP-06-toolbox-fuse-part-v2.png`
- Spriteهای موردنیاز:
  - جعبه‌ابزار بسته
  - جعبه‌ابزار باز، برای لحظه جست‌وجو
  - قطعه فیوز
- Placeholder فعلی:
  `Toolbox (Replace Art)`
- رفتار فعلی:
  بعد از جست‌وجو آبجکت جعبه‌ابزار غیرفعال می‌شود.
- نکته:
  قطعه فیوز فعلاً به‌صورت Inventory تصویری نمایش داده نمی‌شود؛ فقط وضعیت منطقی آن ثبت می‌شود.

### 20. چراغ انتهای مسیر و در خروجی

- فایل:
  `PROP-07-distant-lamp-exit-door-v2.png`
- Spriteهای موردنیاز:
  - چراغ انتهای راهرو
  - در خروجی کنار چراغ
- آبجکت‌های فعلی مرتبط:
  - `Far Light (Replace Art)` فقط نور 2D دارد و Sprite ندارد.
  - در خروجی فعلاً به‌صورت آبجکت تصویری جدا ساخته نشده و ورود با رسیدن خودکار کاراکتر انجام می‌شود.
- استفاده لازم:
  یک SpriteRenderer برای چراغ و یک SpriteRenderer برای در کنار منبع نور اضافه شود.

### 21. بافت نور

- فایل:
  `LIGHT-01-illustrated-light-textures-v2.png`
- مقصد:
  کنار `Far Light (Replace Art)` در اتاق فیوز.
- نوع استفاده:
  SpriteRenderer با حالت شفاف، پشت سایه و جلوی بک‌گراند.
- Sorting Order پیشنهادی:
  `1` یا `2`
- نکته:
  Light2D واقعی موجود باید حفظ شود؛ این تصویر فقط ظاهر مخروط یا هاله نور است.

### 22. سایه بازیکن روی دیوار

- فایل‌های مرتبط:
  - `SHADOW-02-receiver-soft-edge-resources-v2.png`
  - در صورت نیاز یکی از ماسک‌های `SHADOW-01-costume-branch-source-masks-v2.png`
- Placeholder فعلی:
  `Player Shadow (Replace Art)`
- کاربرد:
  سایه‌ای که بعد از تعمیر جعبه‌فیوز روی دیوار ظاهر می‌شود.
- نکته:
  این Sprite باید تیره، نیمه‌شفاف و بدون Collider باشد.

---

## Props پشت صحنه اصلی — آماده هستند ولی هنوز فیلد اتصال ندارند

### 23. پرده، ریل و طناب

- فایل:
  `PROP-08-curtain-rail-rope-kit-v2.png`
- Spriteهای موردنیاز:
  - ریل پرده
  - طناب آویزان
  - اجزای اتصال پرده
- Placeholder فعلی طناب:
  `Upper Curtain Rope (Replace Art)`
- مقصد:
  پرده در سکشن 3 و طناب قابل تعامل در سکشن 4.

### 24. وزنه و اتصال آن به طناب

- فایل:
  `PROP-09-weight-rope-connection-v2.png`
- Spriteهای موردنیاز:
  - وزنه روی زمین
  - وزنه متصل به طناب
- Placeholder فعلی:
  `Heavy Weight (Replace Art)`
- رفتار:
  حالت روی زمین پس از برداشتن غیرفعال می‌شود. حالت متصل به طناب باید هنگام اجرای کات‌سین یا پس از آن نمایش داده شود.

### 25. درِ بسته روی صحنه و راه‌پله

- فایل:
  `PROP-10-locked-stage-door-staircase-kit-v2.png`
- Spriteهای موردنیاز:
  - در بسته مسیر روی صحنه
  - راه‌پله پایین
  - راه‌پله بالا
- Placeholderهای فعلی:
  - `Permanently Locked Stage Door (Replace Art)`
  - `Stairs To Upper Backstage (Replace Art)`
  - `Stairs Down (Replace Art)`
- نکته:
  Collider Trigger راه‌پله‌ها باید حفظ شود؛ Sprite پله فقط ظاهر است.

### 26. دکور واقعی نمایش

- فایل:
  `DEC-01-handmade-theater-decoration-sheet-v2.png`
- Placeholder فعلی:
  `Revealed Stage Decoration (Replace Art)`
- زمان نمایش:
  بعد از بازشدن پرده و برگشت بازیکن به سکشن اصلی پشت صحنه.
- Sorting Order پیشنهادی:
  `3`

### 27. سایه ناشناس روی پرده

- فایل:
  `SHADOW-01-costume-branch-source-masks-v2.png`
- Placeholder فعلی:
  `Unknown Curtain Shadow (Replace Art)`
- زمان نمایش:
  از اولین ورود به پشت صحنه تا مشخص‌شدن دکور واقعی.
- تنظیم پیشنهادی:
  رنگ مشکی با Alpha حدود `0.65` تا `0.8`.

### 28. کوله روی زمین

- فایل موجود در پوشه نسل قبلی:
  `Assets/Project/Art/final-assets/PROP-04-school-bag-floor.png`
- Placeholder فعلی:
  `Backpack (Replace Art)`
- رفتار:
  بعد از برداشتن غیرفعال می‌شود.
- Collider:
  Collider تعاملی فعلی حفظ شود.

### 29. کوله روی کاراکتر

- فایل موجود:
  `Assets/Project/Art/final-assets/PROP-05-wearable-side-view-school-bag.png`
- مقصد:
  یک SpriteRenderer فرزند Player.
- وضعیت فعلی:
  هنوز به کاراکتر متصل نشده است.
- رفتار لازم:
  در شروع غیرفعال باشد و بعد از ثبت `BagCollected` فعال شود.

---

## کاراکتر و انیمیشن‌ها

### 30. طراحی مرجع کاراکتر

- فایل:
  `CHAR-01-final-protagonist-design-v2.png`
- کاربرد:
  فقط Reference طراحی است و مستقیم داخل Animator استفاده نمی‌شود.

### 31. Idle

- فایل:
  `CHAR-02-idle-sprite-sheet-v2.png`
- مقصد:
  Animation Clip حالت Idle در Animator بازیکن.
- Import:
  Multiple و Slice بر اساس تعداد فریم‌ها.

### 32. Walk

- فایل:
  `CHAR-02-walk-sprite-sheet-v2.png`
- مقصد:
  Animation Clip حالت Walk.
- استفاده:
  حرکت عادی و راه‌رفتن پایانی به سمت حیاط.

### 33. Run

- فایل:
  `CHAR-02-run-sprite-sheet-v2.png`
- مقصد:
  Animation Clip حالت Run.
- استفاده:
  دویدن عادی و فرار خودکار به سمت منبع نور.

### 34. تعامل‌ها و واکنش‌ها

- فایل:
  `CHAR-03-interaction-reaction-reference-v2.png`
- وضعیت فایل:
  Reference تصویری است، نه Sprite Sheet کامل انیمیشن.
- انیمیشن‌هایی که هنوز باید به‌صورت Sheet یا فریم‌های جدا آماده شوند:
  - برداشتن برگه
  - شروع هل‌دادن
  - Loop هل‌دادن
  - پایان هل‌دادن
  - بالا رفتن از جعبه
  - برداشتن کلید
  - پایین‌آمدن از جعبه
  - جست‌وجوی جعبه‌ابزار
  - قراردادن قطعه داخل جعبه‌فیوز
  - واکنش ترس
  - برداشتن وزنه
  - حمل وزنه در Idle و Walk
  - بستن وزنه به طناب
  - برداشتن کوله

این انیمیشن‌ها هنوز در منطق فعلی Animator Trigger ندارند. ابتدا باید Assetهای آن‌ها تکمیل و سپس Triggerهای ساده به `PlayerAnimator` اضافه شوند.

---

## UI موجود

### 35. ظاهر باکس دیالوگ

- فایل:
  `UI-01-interaction-dialogue-paper-overlay-kit-v2.png`
- مقصد:
  `Gameplay Canvas > HUD > Text Box`
- روش استفاده:
  Sprite مربوط به باکس دیالوگ را روی کامپوننت Image آبجکت Text Box قرار بده.
- نکته:
  TMP و `DialogueHUD` موجود حفظ شوند.
- بخش آیکن E این Sheet نباید استفاده شود، چون پرامپت عمومی تعامل حذف شده است.

### 36. تصویر پایان

- فایل:
  `UI-02-ending-fade-background-v2.png`
- وضعیت فعلی:
  پایان بازی فقط با Image مشکی Runtime انجام می‌شود.
- استفاده پیشنهادی:
  اگر پایان تصویری لازم بود، این Sprite به لایه Fade پایانی اضافه شود. در حال حاضر الزامی نیست.

### 37. منوی شروع، Pause و Credits

- فایل‌ها:
  - `UI-02-start-menu-background-v2.png`
  - `UI-02-pause-overlay-background-v2.png`
  - `UI-02-credits-background-v2.png`
- مقصدها:
  - Background پنل `Menu`
  - Background پنل `Pause Panel`
  - Background پنل Credits، در صورت ساخت پنل Credits
- ارتباط با مسیر جدید:
  این موارد مستقل از Gameplay Route هستند.

---

## Assetهایی که در مسیر جدید استفاده نمی‌شوند

- تمام Assetهای مربوط به Cart یا چرخ‌دستی معمای قبلی
- `UI-01-interaction-keycap-E.png`
- پرامپت E بالای اشیا
- بک‌گراندهای قدیمی `BG-02-cart-puzzle-background.png` و موارد مشابه مسیر قبلی
- چراغ یا کنترل‌پنل معمای قدیمی، مگر اینکه بخشی از ظاهر آن برای جعبه‌فیوز جدید دوباره استفاده شود

---

## ترتیب درست مرحله جایگزینی آرت

1. ابتدا شش تصویر CUT را در `Game Progress > Story Route Controller` وصل کن. این بخش همین حالا آماده است.
2. بک‌گراندهای BG-01 تا BG-05 را به کنترلر اضافه و Placeholderهای پس‌زمینه را جایگزین کن.
3. Props هر اتاق را به‌ترتیب جریان بازی وصل کن.
4. بعد از اطمینان از اندازه Props، Colliderهای موجود را با ظاهر جدید هماهنگ کن.
5. Idle، Walk و Run را در Animator جایگزین کن.
6. انیمیشن‌های تعامل را بعد از آماده‌شدن Sheetهایشان اضافه کن.
7. در آخر نورها، سایه‌ها و UI را تنظیم کن.

برای جلوگیری از خراب‌شدن منطق، هنگام جایگزینی آرت کامپوننت‌های `StoryInteractable`، `StoryPushBoxInteractable`، `StoryAreaTrigger` و Colliderهای موجود نباید حذف شوند؛ فقط SpriteRenderer و اندازه Collider اصلاح می‌شوند.
