// AutoCoupon - Service Worker
// Menaxhon kuponat, alarmet, storage dhe komunikimin me content scripts

// Import affiliate module
importScripts('../lib/affiliate.js');

// ==================== INSTALIM DHE INITIALIZIM ====================

chrome.runtime.onInstalled.addListener(async (details) => {
  console.log('AutoCoupon u instalua:', details.reason);
  
  // Inicializo settings
  const { settings } = await chrome.storage.sync.get('settings');
  if (!settings) {
    await chrome.storage.sync.set({
      settings: {
        autoApply: true,
        notifications: true,
        theme: 'light',
        source: 'auto',
        language: 'sq'
      }
    });
  }

  // Inicializo domainet e besuara
  const { trustedDomains } = await chrome.storage.sync.get('trustedDomains');
  if (!trustedDomains) {
    await chrome.storage.sync.set({
      trustedDomains: [
        'amazon.com', 'ebay.com', 'etsy.com',
        'walmart.com', 'target.com', 'bestbuy.com',
        'shopify.com', 'nike.com', 'macys.com',
        'aliexpress.com', 'bestbuy.com', 'costco.com'
      ]
    });
  }

  // Inicializo listën e zezë
  const { blacklist } = await chrome.storage.sync.get('blacklist');
  if (!blacklist) {
    await chrome.storage.sync.set({ blacklist: [] });
  }

  // Inicializo statistikat
  const { stats } = await chrome.storage.local.get('stats');
  if (!stats) {
    await chrome.storage.local.set({
      stats: {
        totalApplied: 0,
        successfulApplied: 0,
        failedApplied: 0,
        lastUsed: null,
        firstUsed: Date.now()
      }
    });
  }

  // Inicializo historinë
  const { history } = await chrome.storage.local.get('history');
  if (!history) {
    await chrome.storage.local.set({ history: [] });
  }

  // Krijo alarme për pastrim
  chrome.alarms.create('cleanup', { periodInMinutes: 1440 }); // Çdo 24 orë
  chrome.alarms.create('cacheCleanup', { periodInMinutes: 360 }); // Çdo 6 orë

  // Shfaq njoftim instalimi
  if (details.reason === 'install') {
    chrome.notifications.create({
      type: 'basic',
      iconUrl: '/icons/icon128.png',
      title: 'AutoCoupon u instalua!',
      message: 'Kliko ikonën e extension për të filluar përdorimin.'
    });
  }
});

// ==================== ALARME ====================

chrome.alarms.onAlarm.addListener(async (alarm) => {
  console.log('Alarm u aktivizua:', alarm.name);
  
  switch (alarm.name) {
    case 'cleanup':
      await cleanupHistory();
      break;
    case 'cacheCleanup':
      await cleanupCache();
      break;
  }
});

// Pastrim histori (mbaj vetëm 30 ditët e fundit)
async function cleanupHistory() {
  try {
    const { history } = await chrome.storage.local.get('history');
    if (history && Array.isArray(history)) {
      const thirtyDaysAgo = Date.now() - (30 * 24 * 60 * 60 * 1000);
      const filtered = history.filter(h => h.timestamp > thirtyDaysAgo);
      await chrome.storage.local.set({ history: filtered });
      console.log(`Historia u pastrua: ${history.length - filtered.length} elementë u hoqën`);
    }
  } catch (err) {
    console.error('Gabim gjatë pastrimit të historisë:', err);
  }
}

// Pastrim cache i skaduar
async function cleanupCache() {
  try {
    const { couponCache } = await chrome.storage.local.get('couponCache');
    if (couponCache) {
      const now = Date.now();
      const cleaned = {};
      
      for (const [domain, data] of Object.entries(couponCache)) {
        if (data.expires > now) {
          cleaned[domain] = data;
        }
      }
      
      await chrome.storage.local.set({ couponCache: cleaned });
      console.log('Cache u pastrua');
    }
  } catch (err) {
    console.error('Gabim gjatë pastrimit të cache:', err);
  }
}

// ==================== MESAZHE DHE KOMUNIKIMI ====================

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  console.log('Mesazh i pranuar:', message.action);
  
  // Menaxho përgjigjen async
  handleMessage(message, sender)
    .then(response => sendResponse(response))
    .catch(err => sendResponse({ ok: false, error: err.message }));
  
  // Kthe true për të mbajtur lidhjen e hapur për async
  return true;
});

async function handleMessage(message, sender) {
  switch (message.action) {
    case 'fetchCoupons':
      return await fetchCoupons(message.domain);
    
    case 'applyCoupon':
      return await applyCoupon(message.code, message.tabId);
    
    case 'applyBestCoupon':
      return await applyBestCoupon(message.domain, message.tabId);
    
    case 'scanPage':
      return await scanPageForCoupons(message.tabId);
    
    case 'notify':
      return await sendNotification(message.type, message.text);
    
    case 'updateStats':
      return await updateStats(message.success);
    
    case 'getStats':
      return await getStats();
    
    case 'getSettings':
      return await getSettings();
    
    case 'updateSettings':
      return await updateSettings(message.settings);
    
    case 'addToBlacklist':
      return await addToBlacklist(message.domain);
    
    case 'removeFromBlacklist':
      return await removeFromBlacklist(message.domain);
    
    case 'getBlacklist':
      return await getBlacklist();
    
    case 'ping':
      return { ok: true, domain: sender.tab?.url ? new URL(sender.tab.url).hostname : 'unknown' };
    
    case 'trackAffiliateClick':
      return await trackAffiliateClick(message.domain, message.couponCode);
    
    case 'trackAffiliateConversion':
      return await trackAffiliateConversion(message.domain, message.couponCode, message.purchaseAmount);
    
    case 'getAffiliateStats':
      return await getAffiliateStats();
    
    case 'getAffiliateEarnings':
      return await getAffiliateEarnings();
    
    default:
      return { ok: false, error: 'Aksion i panjohur' };
  }
}

// ==================== MENAXHIMI I KUPONAVE ====================

// Baza e të dhënave të kuponave (përdoret si fallback)
const COUPON_DB = {
  'amazon.com': [
    { code: 'SAVE10', discount: '10% zbritje', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' },
    { code: 'AMZ5OFF', discount: '$5 zbritje', type: 'fixed' },
    { code: 'PRIME20', discount: '20% për Prime', type: 'percentage' },
    { code: 'DEAL15', discount: '15% zbritje', type: 'percentage' }
  ],
  'ebay.com': [
    { code: 'EBAY20', discount: '20% zbritje', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' },
    { code: 'SAVE15', discount: '15% zbritje', type: 'percentage' },
    { code: 'PLUS10', discount: '10% për eBay Plus', type: 'percentage' }
  ],
  'etsy.com': [
    { code: 'ETSY10', discount: '10% zbritje', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas mbi $35', type: 'shipping' },
    { code: 'WELCOME5', discount: '$5 zbritje për porosi të parë', type: 'fixed' },
    { code: 'SPRING20', discount: '20% zbritje', type: 'percentage' }
  ],
  'walmart.com': [
    { code: 'WALMART5', discount: '$5 zbritje', type: 'fixed' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' },
    { code: 'SAVEMORE', discount: '10% zbritje', type: 'percentage' },
    { code: 'SAVE20', discount: '20% zbritje', type: 'percentage' }
  ],
  'target.com': [
    { code: 'TARGET10', discount: '10% zbritje', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' },
    { code: 'CIRCLE5', discount: '$5 zbritje me Target Circle', type: 'fixed' },
    { code: 'BULLSEYE', discount: '15% zbritje', type: 'percentage' }
  ],
  'bestbuy.com': [
    { code: 'BBYSAVE', discount: '10% zbritje në aksesorë', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' },
    { code: 'TECH20', discount: '20% zbritje', type: 'percentage' }
  ],
  'nike.com': [
    { code: 'NIKE10', discount: '10% zbritje', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' },
    { code: 'MEMBER15', discount: '15% për anëtarë', type: 'percentage' },
    { code: 'JUSTDOIT', discount: '20% zbritje', type: 'percentage' }
  ],
  'macys.com': [
    { code: 'MACYS20', discount: '20% zbritje', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas mbi $49', type: 'shipping' },
    { code: 'SAVE25', discount: '25% zbritje në shtëpi', type: 'percentage' },
    { code: 'VIP30', discount: '30% zbritje', type: 'percentage' }
  ],
  'aliexpress.com': [
    { code: 'NEWUSER', discount: '$4 zbritje për përdorues të rinj', type: 'fixed' },
    { code: 'SALE10', discount: '10% zbritje', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' }
  ],
  'costco.com': [
    { code: 'COSTCO10', discount: '10% zbritje', type: 'percentage' },
    { code: 'MEMBER', discount: '15% për anëtarë', type: 'percentage' }
  ],
  'bestbuy.com': [
    { code: 'BBYSAVE', discount: '10% zbritje', type: 'percentage' },
    { code: 'ELECTRONICS', discount: '20% në elektronikë', type: 'percentage' },
    { code: 'FREESHIP', discount: 'Transport falas', type: 'shipping' }
  ]
};

// Kërko kuponat për një domain
async function fetchCoupons(domain) {
  try {
    const cleanDomain = domain.replace(/^www\./, '').split('/')[0];
    
    // Kontrollo cache-in
    const { couponCache } = await chrome.storage.local.get('couponCache');
    const cache = couponCache || {};
    const cached = cache[cleanDomain];
    
    if (cached && cached.expires > Date.now()) {
      console.log(`Kupona nga cache për ${cleanDomain}`);
      return { ok: true, coupons: cached.codes, fromCache: true };
    }
    
    // Kërko kuponat nga burimet
    const coupons = await scrapeCouponSources(cleanDomain);
    
    // Ruaj në cache
    if (coupons && coupons.length > 0) {
      cache[cleanDomain] = {
        codes: coupons,
        expires: Date.now() + (6 * 60 * 60 * 1000) // 6 orë
      };
      await chrome.storage.local.set({ couponCache: cache });
    }
    
    console.log(`U gjetën ${coupons?.length || 0} kupon për ${cleanDomain}`);
    return { ok: true, coupons: coupons || [], fromCache: false };
    
  } catch (err) {
    console.error('Gabim gjatë kërkimit të kuponave:', err);
    return { ok: false, error: err.message };
  }
}

// Kërko kuponat nga burimet e ndryshme
async function scrapeCouponSources(domain) {
  // Pastro domain-in
  const cleanDomain = domain.replace(/^www\./, '').split('/')[0];
  
  // Kërko në DB-në e brendshme
  let coupons = COUPON_DB[cleanDomain];
  
  // Nëse nuk gjendet direkt, provo me pjesë të domain-it
  if (!coupons) {
    const matchingKey = Object.keys(COUPON_DB).find(key => 
      cleanDomain.includes(key) || key.includes(cleanDomain)
    );
    if (matchingKey) {
      coupons = COUPON_DB[matchingKey];
    }
  }
  
  // Kthe kopje të kuponave
  return coupons ? [...coupons] : [];
}

// Apliko kuponin më të mirë
async function applyBestCoupon(domain, tabId) {
  try {
    const result = await fetchCoupons(domain);
    
    if (!result.ok || !result.coupons || result.coupons.length === 0) {
      return { ok: false, error: 'Asnjë kupon nuk u gjet' };
    }
    
    // Rreshto kuponat sipas tipit (përqindja më e lartë e parë)
    const sorted = result.coupons.sort((a, b) => {
      if (a.type === 'percentage' && b.type !== 'percentage') return -1;
      if (a.type !== 'percentage' && b.type === 'percentage') return 1;
      return 0;
    });
    
    // Provo kuponin e parë
    const bestCoupon = sorted[0];
    const applyResult = await applyCoupon(bestCoupon.code, tabId);
    
    if (applyResult.ok) {
      await updateStats(true);
      await addToHistory(domain, bestCoupon);
      return { ok: true, coupon: bestCoupon };
    } else {
      await updateStats(false);
      return applyResult;
    }
    
  } catch (err) {
    console.error('Gabim gjatë aplikimit të kuponit më të mirë:', err);
    return { ok: false, error: err.message };
  }
}

// Apliko kuponin
async function applyCoupon(code, tabId) {
  try {
    await chrome.tabs.sendMessage(tabId, { 
      action: 'applyCoupon', 
      code: code 
    });
    return { ok: true };
  } catch (err) {
    console.error('Gabim gjatë aplikimit të kuponit:', err);
    return { ok: false, error: err.message };
  }
}

// Skano faqen për kuponat
async function scanPageForCoupons(tabId) {
  try {
    const response = await chrome.tabs.sendMessage(tabId, { action: 'scanPage' });
    return response || { ok: true, coupons: [] };
  } catch (err) {
    console.error('Gabim gjatë skanimit të faqes:', err);
    return { ok: false, error: err.message };
  }
}

// ==================== MENAXHIMI I STATISTIKAVE ====================

async function updateStats(success) {
  try {
    const { stats } = await chrome.storage.local.get('stats');
    const currentStats = stats || {
      totalApplied: 0,
      successfulApplied: 0,
      failedApplied: 0,
      lastUsed: null,
      firstUsed: Date.now()
    };
    
    currentStats.totalApplied++;
    if (success) {
      currentStats.successfulApplied++;
    } else {
      currentStats.failedApplied++;
    }
    currentStats.lastUsed = Date.now();
    
    await chrome.storage.local.set({ stats: currentStats });
    return { ok: true, stats: currentStats };
    
  } catch (err) {
    console.error('Gabim gjatë përditësimit të statistikave:', err);
    return { ok: false, error: err.message };
  }
}

async function getStats() {
  try {
    const { stats } = await chrome.storage.local.get('stats');
    return { ok: true, stats: stats || {} };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}

// ==================== MENAXHIMI I HISTORISË ====================

async function addToHistory(domain, coupon) {
  try {
    const { history } = await chrome.storage.local.get('history');
    const list = history || [];
    
    list.unshift({
      domain: domain,
      code: coupon.code,
      discount: coupon.discount || '',
      type: coupon.type || '',
      timestamp: Date.now(),
      id: Date.now().toString(36) + Math.random().toString(36).slice(2, 6)
    });
    
    // Mbaj vetëm 200 elementët e fundit
    if (list.length > 200) {
      list.length = 200;
    }
    
    await chrome.storage.local.set({ history: list });
    return { ok: true };
    
  } catch (err) {
    console.error('Gabim gjatë shtimit në histori:', err);
    return { ok: false, error: err.message };
  }
}

// ==================== MENAXHIMI I SETTINGS ====================

async function getSettings() {
  try {
    const { settings } = await chrome.storage.sync.get('settings');
    return { ok: true, settings: settings || {} };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}

async function updateSettings(newSettings) {
  try {
    const { settings } = await chrome.storage.sync.get('settings');
    const updated = { ...settings, ...newSettings };
    await chrome.storage.sync.set({ settings: updated });
    return { ok: true, settings: updated };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}

// ==================== MENAXHIMI I BLACKLIST ====================

async function getBlacklist() {
  try {
    const { blacklist } = await chrome.storage.sync.get('blacklist');
    return { ok: true, blacklist: blacklist || [] };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}

async function addToBlacklist(domain) {
  try {
    const { blacklist } = await chrome.storage.sync.get('blacklist');
    const list = blacklist || [];
    
    if (!list.includes(domain)) {
      list.push(domain);
      await chrome.storage.sync.set({ blacklist: list });
    }
    
    return { ok: true, blacklist: list };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}

async function removeFromBlacklist(domain) {
  try {
    const { blacklist } = await chrome.storage.sync.get('blacklist');
    const list = (blacklist || []).filter(d => d !== domain);
    await chrome.storage.sync.set({ blacklist: list });
    return { ok: true, blacklist: list };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}

// ==================== NJOFTIME ====================

async function sendNotification(type, text) {
  try {
    const { settings } = await chrome.storage.sync.get('settings');
    
    if (!settings?.notifications) {
      return { ok: true, skipped: true };
    }
    
    chrome.notifications.create({
      type: 'basic',
      iconUrl: '/icons/icon128.png',
      title: 'AutoCoupon',
      message: text,
      priority: type === 'success' ? 1 : 0
    });
    
    return { ok: true };
  } catch (err) {
    console.error('Gabim gjatë dërgimit të njoftimit:', err);
    return { ok: false, error: err.message };
  }
}

// ==================== LIDHJA ME TABET ====================

// Kur një tab përditësohet
chrome.tabs.onUpdated.addListener(async (tabId, changeInfo, tab) => {
  if (changeInfo.status === 'complete' && tab.url) {
    try {
      const { settings } = await chrome.storage.sync.get('settings');
      
      if (settings?.autoApply) {
        const domain = new URL(tab.url).hostname.replace('www.', '');
        const { blacklist } = await chrome.storage.sync.get('blacklist');
        
        // Kontrollo nëse domain-i është në listën e zezë
        if (!(blacklist || []).some(d => domain.includes(d))) {
          // Dërgo mesazh për të kërkuar dhe aplikuar kuponin
          setTimeout(() => {
            chrome.tabs.sendMessage(tabId, { 
              action: 'autoApply', 
              domain: domain 
            }).catch(() => {});
          }, 1500);
        }
      }
    } catch (err) {
      // Ignoro gabimet e URL-së
    }
  }
});

// Kur extension-i klikohet
chrome.action.onClicked.addListener((tab) => {
  // Popup-i hapet automatikisht
});

console.log('AutoCoupon service worker u ngarkua');

// ==================== AFFILIATE TRACKING ====================

async function trackAffiliateClick(domain, couponCode) {
  try {
    // Merr partnerin
    const partner = AutoCouponAffiliate.getPartner(domain);
    if (!partner) {
      return { ok: false, error: 'Partner nuk u gjet' };
    }

    // Ruaj klikimin
    const { affiliateClicks } = await chrome.storage.local.get('affiliateClicks');
    const clicks = affiliateClicks || [];
    
    clicks.push({
      domain: domain,
      partner: partner.name,
      couponCode: couponCode,
      commission: partner.commission,
      timestamp: Date.now(),
      id: Date.now().toString(36) + Math.random().toString(36).slice(2, 8)
    });

    // Mbaj vetëm 1000 klikimet e fundit
    if (clicks.length > 1000) {
      clicks.length = 1000;
    }

    await chrome.storage.local.set({ affiliateClicks: clicks });
    
    console.log(`Affiliate: Klikim i regjistruar për ${partner.name}`);
    return { ok: true, partner: partner.name };
  } catch (err) {
    console.error('Affiliate: Gabim gjatë tracking:', err);
    return { ok: false, error: err.message };
  }
}

async function trackAffiliateConversion(domain, couponCode, purchaseAmount) {
  try {
    // Merr partnerin
    const partner = AutoCouponAffiliate.getPartner(domain);
    if (!partner) {
      return { ok: false, error: 'Partner nuk u gjet' };
    }

    // Llogarit komisionin
    const commission = purchaseAmount * partner.commission;

    // Ruaj konvertimin
    const { affiliateConversions } = await chrome.storage.local.get('affiliateConversions');
    const conversions = affiliateConversions || [];
    
    conversions.push({
      domain: domain,
      partner: partner.name,
      couponCode: couponCode,
      purchaseAmount: purchaseAmount,
      commission: commission,
      timestamp: Date.now(),
      id: Date.now().toString(36) + Math.random().toString(36).slice(2, 8)
    });

    await chrome.storage.local.set({ affiliateConversions: conversions });
    
    // Përditëso të ardhurat
    await updateAffiliateEarnings(commission);
    
    console.log(`Affiliate: Konvertim i regjistruar - $${commission.toFixed(2)} komision`);
    return { ok: true, commission: commission };
  } catch (err) {
    console.error('Affiliate: Gabim gjatë tracking:', err);
    return { ok: false, error: err.message };
  }
}

async function updateAffiliateEarnings(amount) {
  try {
    const { affiliateEarnings } = await chrome.storage.local.get('affiliateEarnings');
    const earnings = affiliateEarnings || {
      total: 0,
      thisMonth: 0,
      thisMonthStart: new Date().toISOString().slice(0, 7),
      history: []
    };

    earnings.total += amount;
    
    // Kontrollo nëse jemi në muajin e ri
    const currentMonth = new Date().toISOString().slice(0, 7);
    if (earnings.thisMonthStart !== currentMonth) {
      earnings.thisMonth = 0;
      earnings.thisMonthStart = currentMonth;
    }
    
    earnings.thisMonth += amount;
    
    // Shto në histori
    earnings.history.push({
      amount: amount,
      timestamp: Date.now()
    });

    // Mbaj vetëm 100 transaksionet e fundit
    if (earnings.history.length > 100) {
      earnings.history.length = 100;
    }

    await chrome.storage.local.set({ affiliateEarnings: earnings });
    return { ok: true };
  } catch (err) {
    console.error('Affiliate: Gabim gjatë përditësimit:', err);
    return { ok: false, error: err.message };
  }
}

async function getAffiliateStats() {
  try {
    const { affiliateClicks } = await chrome.storage.local.get('affiliateClicks');
    const { affiliateConversions } = await chrome.storage.local.get('affiliateConversions');
    const { affiliateEarnings } = await chrome.storage.local.get('affiliateEarnings');

    const clicks = affiliateClicks || [];
    const conversions = affiliateConversions || [];
    const earnings = affiliateEarnings || { total: 0, thisMonth: 0 };

    // Statistikat e këtij muaji
    const currentMonth = new Date().toISOString().slice(0, 7);
    const thisMonthClicks = clicks.filter(c => 
      new Date(c.timestamp).toISOString().slice(0, 7) === currentMonth
    );
    const thisMonthConversions = conversions.filter(c => 
      new Date(c.timestamp).toISOString().slice(0, 7) === currentMonth
    );

    return {
      ok: true,
      stats: {
        totalClicks: clicks.length,
        thisMonthClicks: thisMonthClicks.length,
        totalConversions: conversions.length,
        thisMonthConversions: thisMonthConversions.length,
        conversionRate: clicks.length > 0 ? 
          ((conversions.length / clicks.length) * 100).toFixed(1) : '0.0',
        totalEarnings: earnings.total,
        thisMonthEarnings: earnings.thisMonth
      }
    };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}

async function getAffiliateEarnings() {
  try {
    const { affiliateEarnings } = await chrome.storage.local.get('affiliateEarnings');
    return {
      ok: true,
      earnings: affiliateEarnings || {
        total: 0,
        thisMonth: 0,
        thisMonthStart: new Date().toISOString().slice(0, 7),
        history: []
      }
    };
  } catch (err) {
    return { ok: false, error: err.message };
  }
}