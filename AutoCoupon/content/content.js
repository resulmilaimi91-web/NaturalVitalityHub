// AutoCoupon Content Script
// Injektohet në faqet e shitjeve dhe menaxhon gjetjen/aplikimin e kuponave

(function() {
  'use strict';

  // ==================== VARIABLA GLOBALE ====================
  
  const DOMAIN = window.location.hostname.replace('www.', '');
  const appliedCodes = new Set();
  let settings = {};
  let badge = null;
  let isInitialized = false;

  // ==================== INICIALIZIM ====================

  async function init() {
    if (isInitialized) return;
    isInitialized = true;

    try {
      // Merr settings
      const result = await chrome.storage.sync.get(['blacklist', 'settings']);
      const blacklist = result.blacklist || [];
      
      // Kontrollo nëse domain-i është në listën e zezë
      if (blacklist.some(d => DOMAIN.includes(d))) {
        console.log('AutoCoupon: Domain në listën e zezë, anashkaloj');
        return;
      }

      settings = result.settings || { autoApply: true, notifications: true };
      window.__autoCouponSettings = settings;

      // Krijo badge
      createBadge();

      // Dërgo ping për të treguar se jemi aktiv
      chrome.runtime.sendMessage({ action: 'ping' }).catch(() => {});

      // Auto-apply pas ngarkimit të faqes
      if (settings.autoApply) {
        // Prit ngarkimin e plotë të faqes
        if (document.readyState === 'complete') {
          setTimeout(autoApplyCoupon, 1500);
        } else {
          window.addEventListener('load', () => setTimeout(autoApplyCoupon, 1500));
        }
      }

      // Vëzhgjo për ndryshime në DOM
      observeDOMChanges();

    } catch (err) {
      console.error('AutoCoupon: Gabim gjatë inicializimit:', err);
    }
  }

  // ==================== DETEKTIMI I FUSHAVE TË KUPONAVE ====================

  // Seletorët për fushat e kuponave
  const COUPON_FIELD_SELECTORS = [
    // Input fields
    'input[placeholder*="coupon" i]',
    'input[placeholder*="promo" i]',
    'input[placeholder*="discount" i]',
    'input[placeholder*="code" i]',
    'input[placeholder*="kupon" i]',
    'input[placeholder*="zbritje" i]',
    'input[aria-label*="coupon" i]',
    'input[aria-label*="promo" i]',
    'input[name*="coupon" i]',
    'input[name*="promo" i]',
    'input[id*="coupon" i]',
    'input[id*="promo" i]',
    
    // Container classes
    '.promo-code input',
    '.coupon-code input',
    '.discount-code input',
    '.promo-code-field input',
    '.coupon-input',
    '.promo-input',
    
    // Data attributes
    '[data-testid*="coupon" i] input',
    '[data-testid*="promo" i] input',
    '[data-coupon*="input"]',
    '[data-promo*="input"]',
    
    // Class patterns
    '[class*="coupon" i] input[type="text"]',
    '[class*="promo" i] input[type="text"]',
    '[class*="discount" i] input[type="text"]',
    
    // ID patterns
    '#coupon-input',
    '#promo-input',
    '#discount-code',
    '#couponCode',
    '#promoCode',
    
    // Form fields
    'form input[placeholder*="code" i]',
    'form input[placeholder*="enter" i]',
    
    // Generic text inputs near coupon keywords
    'input[type="text"]:not([type="hidden"])',
    'input:not([type]):not([number]):not([email]):not([password])'
  ];

  function findCouponField() {
    // Provo secilin selector
    for (const selector of COUPON_FIELD_SELECTORS) {
      try {
        const fields = document.querySelectorAll(selector);
        for (const field of fields) {
          if (isValidCouponField(field)) {
            return field;
          }
        }
      } catch (err) {
        // Ignoro selector-t e pavlefshëm
      }
    }

    // Kërko me logikë shtesë
    return findCouponFieldByContext();
  }

  function isValidCouponField(field) {
    // Kontrollo nëse fusha është e dukshme dhe e zbrazët
    if (!field || field.offsetParent === null) return false;
    if (field.disabled || field.readOnly) return false;
    
    const type = field.type?.toLowerCase();
    if (['hidden', 'submit', 'button', 'checkbox', 'radio'].includes(type)) {
      return false;
    }

    // Kontrollo përmbajtjen
    const value = field.value?.trim();
    if (value && value.length > 20) return false; // Fusha me përmbajtje të gjatë nuk është kupon

    // Kontrollo placeholder dhe label
    const placeholder = field.placeholder?.toLowerCase() || '';
    const ariaLabel = field.getAttribute('aria-label')?.toLowerCase() || '';
    const name = field.name?.toLowerCase() || '';
    const id = field.id?.toLowerCase() || '';

    const couponKeywords = ['coupon', 'promo', 'discount', 'code', 'kupon', 'zbritje', 'redeem'];
    const hasKeyword = couponKeywords.some(kw => 
      placeholder.includes(kw) || ariaLabel.includes(kw) || 
      name.includes(kw) || id.includes(kw)
    );

    return hasKeyword;
  }

  function findCouponFieldByContext() {
    // Kërko fusha pranë fjalëve kyçe
    const keywords = ['coupon', 'promo', 'discount', 'code', 'kupon', 'zbritje', 'redeem', 'apply'];
    
    const walker = document.createTreeWalker(
      document.body,
      NodeFilter.SHOW_TEXT,
      null,
      false
    );

    let node;
    while (node = walker.nextNode()) {
      const text = node.textContent.toLowerCase();
      if (keywords.some(kw => text.includes(kw))) {
        // Gjej fushën më të afërt
        const parent = node.parentElement;
        if (parent) {
          const field = parent.querySelector('input[type="text"], input:not([type])');
          if (field && isValidCouponField(field)) {
            return field;
          }
        }
      }
    }

    return null;
  }

  // ==================== DETEKTIMI I BUTONAVE ====================

  const APPLY_BUTTON_SELECTORS = [
    // Butonat me tekst
    'button[type="submit"]',
    'button[class*="apply" i]',
    'button[data-testid*="apply" i]',
    
    // Butonat me data attributes
    '[data-action*="apply" i] button',
    '[data-coupon*="apply" i]',
    
    // Butonat e përgjithshëm
    '.apply-btn',
    '.promo-apply',
    '.coupon-apply',
    '#apply-btn',
    '#promo-apply',
    '#coupon-apply'
  ];

  const APPLY_KEYWORDS = [
    'apply', 'applied', 'aplikoj', 'applikoj', 'përdor', 'use',
    'redeem', 'submit', 'vazhdo', 'continue', 'checkout',
    'add', 'shto', 'konfirmo', 'confirm'
  ];

  function findApplyButton() {
    // Provo selektorët e drejtpërdrejtë
    for (const selector of APPLY_BUTTON_SELECTORS) {
      try {
        const buttons = document.querySelectorAll(selector);
        for (const btn of buttons) {
          if (isValidApplyButton(btn)) {
            return btn;
          }
        }
      } catch (err) {
        // Ignoro
      }
    }

    // Kërko me tekst
    return findApplyButtonByText();
  }

  function findApplyButtonByText() {
    const buttons = document.querySelectorAll('button, input[type="submit"], a.btn');
    
    for (const btn of buttons) {
      if (!isValidApplyButton(btn)) continue;
      
      const text = (btn.textContent || btn.value || '').toLowerCase().trim();
      if (APPLY_KEYWORDS.some(kw => text.includes(kw))) {
        return btn;
      }
    }

    return null;
  }

  function isValidApplyButton(btn) {
    if (!btn || btn.offsetParent === null) return false;
    if (btn.disabled) return false;
    
    const style = window.getComputedStyle(btn);
    if (style.display === 'none' || style.visibility === 'hidden') {
      return false;
    }

    return true;
  }

  // ==================== APLIKIMI I KUPONAVE ====================

  async function autoApplyCoupon() {
    const field = findCouponField();
    if (!field) {
      console.log('AutoCoupon: Nuk u gjet fushë kuponi');
      return;
    }

    try {
      // Kërko kuponat
      const response = await chrome.runtime.sendMessage({
        action: 'fetchCoupons',
        domain: DOMAIN
      });

      if (!response || !response.ok || !response.coupons || response.coupons.length === 0) {
        console.log('AutoCoupon: Asnjë kupon nuk u gjet');
        return;
      }

      // Provo çdo kupon
      for (const coupon of response.coupons) {
        if (!coupon.code || appliedCodes.has(coupon.code)) continue;

        // Vendos kodin në fushë
        setFieldValue(field, coupon.code);
        
        // Prit për përditësim
        await delay(300);

        // Gjej dhe kliko butonin e aplikimit
        const applyBtn = findApplyButton();
        if (applyBtn) {
          applyBtn.click();
          appliedCodes.add(coupon.code);
          
          // Shfaq notifikim
          if (settings.notifications) {
            showNotification(coupon, 'success');
          }

          // Ruaj në histori
          logSuccess(coupon);
          
          console.log(`AutoCoupon: Kodi ${coupon.code} u aplikua me sukses`);
          break;
        }
      }

    } catch (err) {
      console.error('AutoCoupon: Gabim gjatë aplikimit:', err);
    }
  }

  function setFieldValue(field, value) {
    // Përdor React/nëse është e nevojshme
    const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
      window.HTMLInputElement.prototype, 'value'
    ).set;

    nativeInputValueSetter.call(field, value);

    // Dërgo eventet
    field.dispatchEvent(new Event('input', { bubbles: true }));
    field.dispatchEvent(new Event('change', { bubbles: true }));
    field.dispatchEvent(new KeyboardEvent('keyup', { bubbles: true }));
    field.dispatchEvent(new KeyboardEvent('keydown', { bubbles: true }));
  }

  // ==================== SKANIMI I FAQUES ====================

  function scanPageForCoupons() {
    const found = [];
    const seen = new Set();

    //模式匹配 për kuponat
    const patterns = [
      /[A-Z0-9]{4,20}/g,  // Kodet standarde
      /[A-Z]{2,10}-\d{2,8}/g,  // Kodet me presje
      /[A-Z]{3,15}\d{2,6}/g   // Kodet e përziera
    ];

    // Fjalët kyçe për kuponat
    const couponKeywords = [
      'coupon', 'kupon', 'promo', 'promotion', 'discount', 'zbritje',
      'code', 'kod', 'save', 'kursen', 'offer', 'ofertë', 'deal',
      'sale', ' shitje', 'percent', 'përqindje'
    ];

    // Skano DOM
    const walker = document.createTreeWalker(
      document.body,
      NodeFilter.SHOW_TEXT,
      null,
      false
    );

    let node;
    while (node = walker.nextNode()) {
      const text = node.textContent.trim();
      if (text.length < 4 || text.length > 30) continue;

      // Kontrollo nëse teksti përmban fjalë kyçe
      const parentText = node.parentElement?.textContent?.toLowerCase() || '';
      const hasKeyword = couponKeywords.some(kw => parentText.includes(kw));
      
      if (!hasKeyword) continue;

      // Gjej kode
      for (const pattern of patterns) {
        const matches = text.match(pattern);
        if (matches) {
          for (const match of matches) {
            if (match.length >= 4 && !seen.has(match)) {
              seen.add(match);
              
              // Merr zbritjen
              const discount = extractDiscount(parentText);
              
              found.push({
                code: match,
                discount: discount,
                type: detectCouponType(discount),
                source: 'page'
              });
            }
          }
        }
      }
    }

    // Skano elementet HTML
    const elements = document.querySelectorAll('[data-coupon], [data-promo], .coupon-code, .promo-code');
    elements.forEach(el => {
      const code = el.textContent?.trim() || el.getAttribute('data-coupon') || el.getAttribute('data-promo');
      if (code && code.length >= 4 && code.length <= 20 && !seen.has(code)) {
        seen.add(code);
        found.push({
          code: code,
          discount: '',
          type: 'unknown',
          source: 'element'
        });
      }
    });

    return found;
  }

  function extractDiscount(text) {
    const patterns = [
      /(\d+)\s*%/,
      /%\s*(\d+)/,
      /\$\s*(\d+)/,
      /(\d+)\s*dollar/,
      /(\d+)\s*€/,
      /(\d+)\s*euro/,
      /free\s*shipping/i,
      /transport\s*falas/i
    ];

    for (const pattern of patterns) {
      const match = text.match(pattern);
      if (match) {
        return match[0];
      }
    }

    return '';
  }

  function detectCouponType(discount) {
    if (!discount) return 'unknown';
    
    if (discount.includes('%')) return 'percentage';
    if (discount.includes('$') || discount.includes('€')) return 'fixed';
    if (discount.toLowerCase().includes('free') || discount.includes('falas')) return 'shipping';
    
    return 'unknown';
  }

  // ==================== NOTIFIKIME ====================

  function showNotification(coupon, type = 'info') {
    // Fshi notifikimet e mëparshme
    const existing = document.querySelector('.autocoupon-notification');
    if (existing) existing.remove();

    // Krijo notifikim të ri
    const notification = document.createElement('div');
    notification.className = 'autocoupon-notification';
    
    // Përcakto ngjyrën bazuar në tipin
    const colors = {
      success: { bg: '#10b981', icon: '✓' },
      error: { bg: '#ef4444', icon: '✕' },
      info: { bg: '#3b82f6', icon: 'ℹ' },
      warning: { bg: '#f59e0b', icon: '!' }
    };

    const color = colors[type] || colors.info;

    notification.innerHTML = `
      <div class="notification-icon" style="background: ${color.bg}">${color.icon}</div>
      <div class="notification-content">
        <div class="notification-title">AutoCoupon</div>
        <div class="notification-message">
          Kodi <strong>${escapeHtml(coupon.code)}</strong> u aplikua!
          ${coupon.discount ? `<span class="notification-discount">${escapeHtml(coupon.discount)}</span>` : ''}
        </div>
      </div>
      <button class="notification-close">&times;</button>
    `;

    // Shto CSS
    const style = document.createElement('style');
    style.textContent = `
      .autocoupon-notification {
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 2147483647;
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 14px 18px;
        background: white;
        border-radius: 12px;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        font-size: 14px;
        color: #1e293b;
        max-width: 380px;
        animation: autocoupon-slide-in 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        cursor: default;
      }
      .autocoupon-notification:hover {
        box-shadow: 0 12px 40px rgba(0, 0, 0, 0.2);
      }
      .notification-icon {
        width: 32px;
        height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        color: white;
        font-weight: bold;
        font-size: 16px;
        flex-shrink: 0;
      }
      .notification-content {
        flex: 1;
      }
      .notification-title {
        font-weight: 700;
        font-size: 13px;
        color: #64748b;
        margin-bottom: 2px;
      }
      .notification-message {
        font-size: 14px;
        color: #1e293b;
      }
      .notification-discount {
        display: block;
        font-size: 12px;
        color: #10b981;
        margin-top: 2px;
      }
      .notification-close {
        background: none;
        border: none;
        font-size: 20px;
        color: #94a3b8;
        cursor: pointer;
        padding: 4px 8px;
        border-radius: 6px;
        transition: all 0.2s;
      }
      .notification-close:hover {
        background: #f1f5f9;
        color: #64748b;
      }
      @keyframes autocoupon-slide-in {
        from {
          opacity: 0;
          transform: translateX(40px);
        }
        to {
          opacity: 1;
          transform: translateX(0);
        }
      }
    `;

    document.head.appendChild(style);
    document.body.appendChild(notification);

    // Eventet
    notification.querySelector('.notification-close').addEventListener('click', () => {
      notification.remove();
    });

    notification.addEventListener('click', (e) => {
      if (e.target === notification) {
        notification.remove();
      }
    });

    // Hiq automatikisht pas 5 sekondave
    setTimeout(() => {
      if (notification.parentNode) {
        notification.style.animation = 'autocoupon-slide-out 0.3s ease forwards';
        setTimeout(() => notification.remove(), 300);
      }
    }, 5000);
  }

  // ==================== BADGE ====================

  function createBadge() {
    if (badge) return;

    badge = document.createElement('div');
    badge.className = 'autocoupon-badge';
    badge.innerHTML = `
      <span class="badge-icon">🏷️</span>
      <span class="badge-text">AutoCoupon</span>
    `;

    const style = document.createElement('style');
    style.textContent = `
      .autocoupon-badge {
        position: fixed;
        bottom: 20px;
        right: 20px;
        z-index: 2147483646;
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 16px;
        background: linear-gradient(135deg, #10b981 0%, #059669 100%);
        color: white;
        border-radius: 24px;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        font-size: 13px;
        font-weight: 600;
        box-shadow: 0 4px 20px rgba(16, 185, 129, 0.4);
        cursor: pointer;
        transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
        opacity: 0;
        transform: translateY(20px);
        animation: autocoupon-badge-in 0.5s ease 2s forwards;
      }
      .autocoupon-badge:hover {
        transform: translateY(-2px) scale(1.05);
        box-shadow: 0 6px 24px rgba(16, 185, 129, 0.5);
      }
      .badge-icon {
        font-size: 16px;
      }
      .badge-text {
        letter-spacing: 0.3px;
      }
      @keyframes autocoupon-badge-in {
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
    `;

    document.head.appendChild(style);
    document.body.appendChild(badge);

    // Kliko për të hapur popup
    badge.addEventListener('click', () => {
      chrome.runtime.sendMessage({ action: 'openPopup' }).catch(() => {});
    });
  }

  function removeBadge() {
    if (badge) {
      badge.remove();
      badge = null;
    }
  }

  // ==================== VËZHGIMI I DOM ====================

  function observeDOMChanges() {
    const observer = new MutationObserver((mutations) => {
      for (const mutation of mutations) {
        if (mutation.type === 'childList') {
          // Kontrollo për fusha të reja kuponash
          for (const node of mutation.addedNodes) {
            if (node.nodeType === Node.ELEMENT_NODE) {
              const field = node.querySelector?.('input') || 
                           (node.matches?.('input') ? node : null);
              
              if (field && isValidCouponField(field)) {
                console.log('AutoCoupon: U gjet fushë e re kuponash');
                // Mund të shtojmë logikë shtesë këtu
              }
            }
          }
        }
      }
    });

    observer.observe(document.body, {
      childList: true,
      subtree: true
    });
  }

  // ==================== HISTORIKU ====================

  function logSuccess(coupon) {
    chrome.storage.local.get('history', (result) => {
      const history = result.history || [];
      history.unshift({
        domain: DOMAIN,
        code: coupon.code,
        discount: coupon.discount || '',
        type: coupon.type || '',
        timestamp: Date.now(),
        id: Date.now().toString(36) + Math.random().toString(36).slice(2, 6)
      });
      
      // Mbaj vetëm 200 elementët e fundit
      if (history.length > 200) {
        history.length = 200;
      }
      
      chrome.storage.local.set({ history });
    });
  }

  // ==================== MESAZHE ====================

  chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
    console.log('AutoCoupon: Mesazh i pranuar:', message.action);

    switch (message.action) {
      case 'applyCoupon':
        handleApplyCoupon(message.code, sendResponse);
        return true;

      case 'scanPage':
        const coupons = scanPageForCoupons();
        sendResponse({ ok: true, coupons: coupons });
        return true;

      case 'autoApply':
        autoApplyCoupon().then(() => {
          sendResponse({ ok: true });
        }).catch(err => {
          sendResponse({ ok: false, error: err.message });
        });
        return true;

      case 'ping':
        sendResponse({ ok: true, domain: DOMAIN });
        return true;

      case 'getDomain':
        sendResponse({ ok: true, domain: DOMAIN });
        return true;

      case 'showNotification':
        showNotification(message.coupon, message.type);
        sendResponse({ ok: true });
        return true;

      case 'removeBadge':
        removeBadge();
        sendResponse({ ok: true });
        return true;

      default:
        sendResponse({ ok: false, error: 'Aksion i panjohur' });
        return true;
    }
  });

  function handleApplyCoupon(code, sendResponse) {
    const field = findCouponField();
    if (!field) {
      sendResponse({ ok: false, error: 'Nuk u gjet fushë kuponi' });
      return;
    }

    // Vendos kodin
    setFieldValue(field, code);

    // Kliko butonin pas një vonese
    setTimeout(() => {
      const applyBtn = findApplyButton();
      if (applyBtn) {
        applyBtn.click();
        appliedCodes.add(code);
        sendResponse({ ok: true });
      } else {
        sendResponse({ ok: false, error: 'Nuk u gjet butoni i aplikimit' });
      }
    }, 300);
  }

  // ==================== UTILITY ====================

  function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }

  // ==================== START ====================

  // Fillo inicializimin
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }

})();
