# راهنمای جایگزینی آرت در سین Menu

سین اصلی پروژه از این به بعد این فایل است:

`Assets/Project/Scene/Menu.unity`

تمام اتاق‌ها، آیتم‌ها، درها، سایه‌ها، نقاط تعامل و UI داخل خود سین ذخیره شده‌اند. هیچ تصویر یا آبجکت گیم‌پلی هنگام اجرای بازی توسط کد ساخته نمی‌شود.

## قانون ساده جایگزینی آرت

برای عوض‌کردن ظاهر هر چیز:

1. Play Mode خاموش باشد.
2. سین `Menu` را باز کن.
3. آبجکت موردنظر را در Hierarchy پیدا کن.
4. در `Sprite Renderer` فقط فیلد `Sprite` را عوض کن.
5. اگر اندازه تصویر متفاوت بود، `Transform > Scale` و بعد اندازه `Collider` همان آبجکت را تنظیم کن.
6. کامپوننت‌های `StoryInteractable`، `StoryPushBoxInteractable`، `StoryAreaTrigger` و Colliderها را پاک نکن.

## محل تمام اتاق‌ها

در Hierarchy این ریشه را باز کن:

`GAMEPLAY WORLD - EDIT ART HERE`

زیر آن پنج بخش بازی قرار دارد:

- `ROOM 01 - START`
- `ROOM 02 - KEY AND BOX`
- `ROOM 03 - FUSE`
- `ROOM 04 - MAIN BACKSTAGE`
- `ROOM 05 - UPPER BACKSTAGE`

هر اتاق یک آبجکت `Background` دارد. برای جایگزینی بک‌گراند، همان آبجکت را انتخاب کن و Sprite آن را تغییر بده.

## اتاق اول: شروع بازی

داخل `ROOM 01 - START`:

- `Background`: پس‌زمینه اتاق اول.
- `Tutorial Paper - Replace Sprite Here`: برگه‌ای که از زمین برداشته می‌شود.
- `Right Hallway Door - OPEN`: در سمت راست که وارد مسیر اصلی بازی می‌شود.
- `Left Courtyard Door - LOCKED`: در سمت چپ که تا قبل از برداشتن کوله قابل استفاده نیست.

برای این سه آبجکت فقط Sprite Renderer را عوض کن. کامپوننت تعامل و Collider روی خودشان باید باقی بماند.

تصویر بزرگ آموزش که بعد از برداشتن برگه روی صفحه می‌آید، از این مسیر تنظیم می‌شود:

`Game Progress > Story Route Controller > Tutorial Paper Image`

## اتاق دوم: کلید و جعبه

داخل `ROOM 02 - KEY AND BOX`:

- `Background`: پس‌زمینه راهرو.
- `Locked Door`: در قفل‌شده خروجی.
- `High Key`: کلید روی دیوار.
- `Push Box`: جعبه قابل هل‌دادن.
- آبجکت‌های سایه و لباس: ظاهر سایه اول و حقیقت آن در برگشت.

روی `Push Box` فقط Sprite و اندازه Collider را تنظیم کن؛ اسکریپت هل‌دادن نباید حذف شود.

## اتاق سوم: جعبه‌فیوز

داخل `ROOM 03 - FUSE`:

- `Background`: پس‌زمینه تاریک.
- `Fuse Box`: جعبه‌فیوز.
- `Toolbox`: جعبه‌ابزار روی زمین.
- `Exit Door`: دری که کاراکتر هنگام فرار از آن عبور می‌کند.
- `Far Light`: چراغ انتهای مسیر.
- `Player Shadow`: سایه کاراکتر روی دیوار.

نور واقعی و منطق روشن‌شدن از قبل متصل است. Spriteها را عوض کن، ولی Light و اسکریپت‌ها را نگه دار.

## اتاق چهارم: پشت صحنه اصلی

داخل `ROOM 04 - MAIN BACKSTAGE`:

- `Background`: پس‌زمینه پشت صحنه.
- `Unknown Curtain Shadow`: سایه ناشناس روی پرده.
- `Revealed Stage Decoration`: دکوری که بعداً حقیقت سایه را نشان می‌دهد.
- `Heavy Weight`: وزنه قابل برداشتن.
- `Permanently Locked Stage Door`: در همیشه‌بسته مسیر روی صحنه.
- `Stairs To Upper Backstage`: راه‌پله رفتن به بالا.
- `Backpack`: کوله روی زمین.

## اتاق پنجم: بالای پشت صحنه

داخل `ROOM 05 - UPPER BACKSTAGE`:

- `Background`: پس‌زمینه بخش بالا.
- `Upper Curtain Rope`: طنابی که وزنه به آن متصل می‌شود.
- `Stairs Down`: راه برگشت به پایین.

## تصاویر کات‌سین

آبجکت `Game Progress` را انتخاب کن و در کامپوننت `Story Route Controller` این شش Sprite را جایگزین کن:

- `Tutorial Paper Image`: برگه آموزش.
- `First Shadow Image`: سایه ترسناک هنگام بالا رفتن روی جعبه.
- `Curtain Opening Image`: کناررفتن پرده.
- `Decoration Reveal Image`: مشخص‌شدن دکور واقعی.
- `Own Shadow Image`: فهمیدن اینکه سایه متعلق به خود کاراکتر است.
- `Costume Reveal Image`: مشخص‌شدن سایه لباس‌ها.

خود قاب نمایش کات‌سین داخل این مسیر است:

`Gameplay Canvas > Story Cutscene Overlay`

این قاب و Image آن داخل سین هستند؛ کد فقط آن‌ها را روشن و خاموش می‌کند.

## UI بازی

تمام UI زیر `Gameplay Canvas` قرار دارد:

- `HUD > Text Box`: باکس دیالوگ و TMP فارسی.
- `Pause Panel`: پنل Pause.
- `Menu`: منوی شروع.
- `Setting`: تنظیمات.
- `Story Cutscene Overlay`: نمایش تصاویر روایی.
- `Story Fade`: فید بین اتاق‌ها و پایان بازی.

برای تغییر ظاهر UI فقط Sprite کامپوننت Image، فونت TMP، رنگ و اندازه RectTransform را تغییر بده. آبجکت یا اسکریپت مدیریت UI را حذف نکن.

## کاراکتر و انیمیشن

برای ظاهر کاراکتر، آبجکت `Player` و فرزند تصویری آن را ویرایش کن. کلیپ‌های Idle، Walk و Run را در Animator فعلی جایگزین کن. بعداً برای انیمیشن‌های اختصاصی تعامل می‌توان کلیپ‌های جدید را به همان Animator اضافه کرد؛ منطق اصلی بازی برای اجرای مسیر به ساخت آبجکت جدید در کد وابسته نیست.

## چیزهایی که نباید دست‌کاری شوند

- اسم یا رفرنس آبجکت‌های تعاملی را بدون نیاز عوض نکن.
- `StoryRouteController` روی `Game Progress` را حذف نکن.
- Colliderهای Trigger درها و راه‌پله‌ها را حذف نکن.
- `StoryInteractable` و `StoryPushBoxInteractable` را حذف نکن.
- برای جایگزینی آرت لازم نیست هیچ کدی تغییر کند.

در حالت عادی تنها کار باقی‌مانده برای آرت نهایی این است: Sprite را جایگزین کن، Scale را تنظیم کن و در صورت نیاز Collider همان آبجکت را با تصویر هماهنگ کن.

## تنظیم دستی مسیر، Spawn و دوربین

داخل هر اتاق آبجکت زیر وجود دارد:

`LEVEL DESIGN POINTS - MOVE THESE`

تمام فرزندان این آبجکت Pointهای قابل جابه‌جایی هستند و از روی Scene View می‌توانی آن‌ها را حرکت بدهی:

- `PLAYER LIMIT - LEFT` و `PLAYER LIMIT - RIGHT`: محدوده‌ای که بازیکن اجازه راه‌رفتن دارد.
- `CAMERA ANCHOR`: ارتفاع و Z دوربین آن سکشن.
- `CAMERA LIMIT - LEFT` و `CAMERA LIMIT - RIGHT`: محدوده حرکت افقی دوربین.
- Pointهای `SPAWN`: محل دقیق ورود بازیکن از سکشن‌های مختلف، شامل ارتفاع ورود.
- Pointهای `AUTO RUN` و `AUTO WALK`: مقصد حرکت‌های خودکار داستانی.
- `PLAYER - CLIMB GROUND` و `PLAYER - CLIMB TOP`: محل ایستادن پایین و بالای جعبه.
- `BOX PLACEMENT TARGET`: مقصد نهایی جعبه. با جابه‌جایی این Point مسیر هل‌دادن کوتاه‌تر یا بلندتر می‌شود.

برای طویل‌کردن سکشن چهار:

1. بک‌گراند و اشیای اتاق را در عرض گسترش بده.
2. `PLAYER LIMIT - LEFT/RIGHT` را روی دو انتهای مسیر قرار بده.
3. `CAMERA LIMIT - LEFT/RIGHT` را به اندازه‌ای باز کن که دوربین در طول اتاق بازیکن را دنبال کند.
4. ارتفاع Pointهای `SPAWN` را روی سطح زمین جدید تنظیم کن.

اگر دو Camera Limit روی یک X باشند، دوربین ثابت می‌ماند. اگر از هم فاصله داشته باشند، دوربین بازیکن را بین آن دو Point دنبال می‌کند.

## ترتیب نمایش Cutscene و UI

این آبجکت‌ها داخل سین Canvas مستقل با `Override Sorting` دارند و مقدار `Order in Layer` آن‌ها مستقیماً از Inspector قابل تغییر است:

- `Story Fade`: مقدار پیش‌فرض 50
- `Story Cutscene Overlay`: مقدار پیش‌فرض 100؛ تمام تصاویر Cutscene از همین مقدار استفاده می‌کنند.
- `HUD > Text Box`: مقدار پیش‌فرض 200 تا دیالوگ روی Cutscene دیده شود.
- `Menu`، `Setting` و `Pause Panel`: مقدار پیش‌فرض 300 تا همیشه بالاتر از Cutscene باشند.

برای تغییر ترتیب تمام Cutsceneها فقط `Story Cutscene Overlay > Canvas > Order in Layer` را تغییر بده؛ لازم نیست کد را دست‌کاری کنی.
