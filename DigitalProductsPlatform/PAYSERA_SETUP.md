# UDHËZUESI - SI TË MERRNI PARA NË PAYSERA

## HAPI 1: KRIJO LLOGARI PAYSERA

1. Shko tek: https://www.paysera.com/en/business-registration
2. Plotëso formularin:
   - Business Name: Emri i biznesit tënd
   - Country: Shqipëri / Kosovë
   - Email: Email-i yt
   - Phone: Numri i telefonit
3. Verifiko email-in
4. Hyn në dashboard

## HAPI 2: MERR CREDENTIALS

1. Hyn në Paysera Dashboard
2. Shko tek: **Projects** → **My Projects**
3. Kliko **Create Project** ose përdor ekzistuesin
4. Shko tek: **API Keys** ose **Integration**
5. Merr këto të dhëna:
   ```
   Client ID:     ??????
   Client Secret: ??????
   Project ID:    ??????
   Webhook Secret: ??????
   ```

## HAPI 3: VENDOSI NË .env

Hap file-n `backend/.env` dhe vendosi credentials:

```
PAYSERA_CLIENT_ID=123456
PAYSERA_CLIENT_SECRET=abcdef123456
PAYSERA_PROJECT_ID=9999
PAYSERA_WEBHOOK_SECRET=xyz789
```

## HAPI 4: TESTO (SANDBOX)

Para se të përdorësh para reale, testo me kartë test:
- Card Number: 4111 1111 1111 1111
- Expiry: 12/25
- CVV: 123

## HAPI 5: AKTIVIZO LIVE MODE

Kur të jesh gati:
1. Hyn në Paysera Dashboard
2. Shko tek **Settings** → **Live Mode**
3. Aktivizo **Live Mode**
4. Tani do të marrësh para reale!

## SI TË TË VIJNË PARATË

```
Blejuesi blen produktin → Paysera mban paratë →
Pas 1-3 ditësh → Paratë në llogarinë tënde bankare
```

## MINIMUM PAGESA
- Minimumi: €1
- Komisioni: ~1.5% + €0.25

## KUJDES!
- MOS i shfaq credentials publikisht
- MOS i ndaj me askënd
- Ruani në `.env` që nuk është në GitHub
