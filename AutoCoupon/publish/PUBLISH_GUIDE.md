# Udhëzime për Publikim në Chrome Web Store

## Para se të fillosh

### Kërkesat
1. **Chrome Web Store Developer Account** - Regjistrohu në https://chrome.google.com/webstore/devconsole
2. **Tarifë regjistrimi** - $5 (një herë)
3. **Screenshot-et** - 3 screenshots (1280x800)

### Skedarët e nevojshëm
- `autocoupon.zip` - Dosja e extension-it
- Screenshot-et (3)
- Përshkrimi i extension-it

## Hapat për publikim

### Hapi 1: Hap Chrome Web Store Developer Dashboard
1. Shko në https://chrome.google.com/webstore/devconsole
2. Hy me llogarinë tënde Google
3. Kliko "Add a new item"

### Hapi 2: Ngarko ZIP
1. Kliko "Choose file"
2. Zgjidh `autocoupon.zip`
3. Prisni derisa të ngarkohet

### Hapi 3: Plotëso informacionin
1. **Title**: AutoCoupon - Automatic Coupon Finder
2. **Description**: Shiko store-listing/listing.md për përshkrimin e plotë
3. **Category**: Shopping
4. **Language**: Shqip

### Hapi 4: Shto screenshots
1. Kliko "Add screenshot"
2. Ngarko 3 screenshots:
   - popup-screenshot.png
   - options-screenshot.png
   - autoapply-screenshot.png

### Hapi 5: Konfiguro privacy
1. **Permissions**: Shiko manifest.json për permissions
2. **Host permissions**: *://*/*
3. **Privacy policy**: Krijo një faqe privacy policy

### Hapi 6: Dërgo për review
1. Kontrollo të gjitha informacionet
2. Kliko "Submit for review"
3. Prisni derisa të miratohet (zakonisht 1-3 ditë)

## Informacione shtesë

### Permissions të kërkuara
- `storage` - Për ruajtjen e të dhënave
- `scripting` - Për injektimin e content script
- `activeTab` - Për aksesin në tab-in aktiv
- `alarms` - Për alarmet e pastrimit

### Host Permissions
- `*://*/*` - Për të gjitha faqet web

### Privacy Policy
Krijo një faqe privacy policy që tregon:
- Nuk dërgon të dhëna të përdoruesve
- Të dhënat ruajtën vetëm lokalisht
- Asnjë tracking ose analytics

## Pas publikimit

### Monitoro review status
1. Shko në Developer Dashboard
2. Kontrollo statusin e review
3. Përgjigju komenteve të reviewer

### Përditëso extension
1. Bëj ndryshimet e nevojshme
2. Krijo ZIP të ri
3. Ngarko versionin e ri
4. Dërgo për review përsëri

## Ndihmë
- Chrome Web Store Developer Documentation: https://developer.chrome.com/docs/webstore/
- Chrome Extension API: https://developer.chrome.com/docs/extensions/
