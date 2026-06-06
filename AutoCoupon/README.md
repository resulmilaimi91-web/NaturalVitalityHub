# AutoCoupon - Chrome Extension

Shtesë për Chrome që gjen dhe aplikon automatikisht kode kuponi në faqet e shitjeve online.

## 📦 Instalimi

1. **Hapni Chrome** dhe shkoni në `chrome://extensions/`
2. **Aktivizo** "Developer mode" (sipër djathtas)
3. **Kliko** "Load unpacked"
4. **Zgjidhni** dosjen `autocoupon`
5. **Extension-i** do të shfaqet në toolbar

## 🧪 Testimi

1. **Hapni** dosjen `test/test-shop.html` në Chrome
2. **Klikoni** ikonën e AutoCoupon në toolbar
3. **Klikoni** "Kërko Kupona" për të gjetur kuponat
4. **Provoni** "Auto-Apply" për të aplikuar kuponin

## 📁 Struktura

```
autocoupon/
├── manifest.json          # Konfigurimi i extension
├── background/
│   └── background.js      # Service worker
├── content/
│   ├── content.js         # Content script
│   └── content.css        # Stilet e content script
├── popup/
│   ├── popup.html         # Popup UI
│   ├── popup.js           # Popup logic
│   └── popup.css          # Popup styles
├── options/
│   ├── options.html       # Faqja e opsioneve
│   └── options.js         # Options logic
├── lib/
│   └── storage.js         # Storage module
├── icons/
│   ├── icon16.png         # Ikona 16x16
│   ├── icon48.png         # Ikona 48x48
│   └── icon128.png        # Ikona 128x128
└── test/
    └── test-shop.html     # Faqja test
```

## ⚡ Features

- **Auto-Apply** - Aplikon kuponat automatikisht
- **Kërkim** - Gjen kuponat nga burime të ndryshme
- **Kopjim** - Kliko për të kopjuar kodin
- **Historiku** - Ruaj historinë e kuponave
- **Backup** - Eksporto/Importo të dhënat
- **Dark Mode** - Tema e errët

## 🔧 Teknologji

- JavaScript vanilla (asnjë library)
- Chrome Extension Manifest V3
- Chrome Storage API
- CSS Flexbox/Grid

## 📝 License

MIT License
