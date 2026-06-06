// AutoCoupon Popup Script
// Menaxhon ndërfaqen e përdoruesit dhe komunikimin me background script

let currentCoupons = [];
let currentDomain = '';
let isLoading = false;

// ==================== INITIALIZIM ====================

document.addEventListener('DOMContentLoaded', async () => {
  // Merr elementet DOM
  const elements = {
    searchInput: document.getElementById('searchInput'),
    searchClear: document.getElementById('searchClear'),
    scanBtn: document.getElementById('scanBtn'),
    autoApplyBtn: document.getElementById('autoApplyBtn'),
    settingsBtn: document.getElementById('settingsBtn'),
    historyBtn: document.getElementById('historyBtn'),
    helpBtn: document.getElementById('helpBtn'),
    statusBar: document.getElementById('statusBar'),
    statusText: document.getElementById('statusText'),
    couponList: document.getElementById('couponList'),
    emptyState: document.getElementById('emptyState'),
    loadingState: document.getElementById('loadingState'),
    countBadge: document.getElementById('countBadge'),
    historyPanel: document.getElementById('historyPanel'),
    historyBack: document.getElementById('historyBack'),
    historyList: document.getElementById('historyList'),
    historyEmpty: document.getElementById('historyEmpty'),
    clearHistory: document.getElementById('clearHistory'),
    helpPanel: document.getElementById('helpPanel'),
    helpBack: document.getElementById('helpBack')
  };

  // Lidh eventet
  bindEvents(elements);

  // Merr domain-in aktual
  await getCurrentDomain();

  // Ngarko settings
  await loadSettings(elements);

  // Shfaq gjendjen fillestare
  updateUI(elements);
});

// ==================== EVENT BINDING ====================

function bindEvents(elements) {
  // Kërko kupon
  elements.scanBtn.addEventListener('click', () => scanCoupons(elements));
  
  // Auto-apply
  elements.autoApplyBtn.addEventListener('click', () => autoApplyCoupon(elements));
  
  // Search
  elements.searchInput.addEventListener('input', (e) => filterCoupons(e.target.value, elements));
  elements.searchInput.addEventListener('focus', () => {
    if (elements.searchInput.value) {
      elements.searchClear.classList.remove('hidden');
    }
  });
  elements.searchInput.addEventListener('blur', () => {
    setTimeout(() => elements.searchClear.classList.add('hidden'), 200);
  });
  
  // Search clear
  elements.searchClear.addEventListener('click', () => {
    elements.searchInput.value = '';
    elements.searchClear.classList.add('hidden');
    filterCoupons('', elements);
    elements.searchInput.focus();
  });
  
  // Settings
  elements.settingsBtn.addEventListener('click', () => {
    chrome.runtime.openOptionsPage();
  });
  
  // History
  elements.historyBtn.addEventListener('click', () => showHistory(elements));
  elements.historyBack.addEventListener('click', () => hidePanel(elements.historyPanel));
  elements.clearHistory.addEventListener('click', () => clearHistory(elements));
  
  // Help
  elements.helpBtn.addEventListener('click', () => showPanel(elements.helpPanel));
  elements.helpBack.addEventListener('click', () => hidePanel(elements.helpPanel));
  
  // Earnings
  const earningsBtn = document.getElementById('earningsBtn');
  const earningsBack = document.getElementById('earningsBack');
  const earningsPanel = document.getElementById('earningsPanel');
  
  if (earningsBtn) {
    earningsBtn.addEventListener('click', () => {
      showPanel(earningsPanel);
      loadEarnings();
    });
  }
  if (earningsBack) {
    earningsBack.addEventListener('click', () => hidePanel(earningsPanel));
  }
  
  // Keyboard shortcuts
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      hideAllPanels(elements);
    }
    if (e.ctrlKey && e.key === 'k') {
      e.preventDefault();
      elements.searchInput.focus();
    }
  });
}

// ==================== DOMAIN DHE KONTEKSTI ====================

async function getCurrentDomain() {
  try {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    if (tab && tab.url) {
      const url = new URL(tab.url);
      currentDomain = url.hostname.replace('www.', '');
    }
  } catch (err) {
    console.error('Gabim gjatë marrjes së domain-it:', err);
  }
}

// ==================== SKANIMI I KUPONAVE ====================

async function scanCoupons(elements) {
  if (isLoading) return;
  
  setLoading(elements, true, 'Duke kërkuar kupon...');
  
  try {
    // Kërko kuponat nga background
    const response = await chrome.runtime.sendMessage({
      action: 'fetchCoupons',
      domain: currentDomain
    });
    
    if (response && response.ok) {
      currentCoupons = response.coupons || [];
      
      // Skano faqen për kuponat shtesë
      try {
        const pageScan = await chrome.tabs.sendMessage(
          (await chrome.tabs.query({ active: true, currentWindow: true }))[0].id,
          { action: 'scanPage' }
        );
        
        if (pageScan && pageScan.ok && pageScan.coupons) {
          // Shto kuponat e faqes (pa dublikime)
          const seenCodes = new Set(currentCoupons.map(c => c.code));
          for (const coupon of pageScan.coupons) {
            if (!seenCodes.has(coupon.code)) {
              currentCoupons.push(coupon);
              seenCodes.add(coupon.code);
            }
          }
        }
      } catch (err) {
        // Nëse skanimi i faqes dështon, vazhdo me kuponat e background
      }
      
      if (currentCoupons.length > 0) {
        setStatus(elements, 'success', `${currentCoupons.length} kupon u gjetën`);
      } else {
        setStatus(elements, 'idle', 'Asnjë kupon nuk u gjet');
      }
    } else {
      setStatus(elements, 'error', response?.error || 'Gabim gjatë kërkimit');
    }
  } catch (err) {
    console.error('Gabim:', err);
    setStatus(elements, 'error', 'Gabim gjatë kërkimit');
  } finally {
    setLoading(elements, false);
    renderCoupons(elements);
  }
}

// ==================== AUTO-APPLY ====================

async function autoApplyCoupon(elements) {
  if (isLoading) return;
  
  setLoading(elements, true, 'Duke aplikuar kupon...');
  
  try {
    const response = await chrome.runtime.sendMessage({
      action: 'applyBestCoupon',
      domain: currentDomain,
      tabId: (await chrome.tabs.query({ active: true, currentWindow: true }))[0].id
    });
    
    if (response && response.ok) {
      setStatus(elements, 'success', `Kodi ${response.coupon.code} u aplikua!`);
      
      // Shto në histori
      await addToHistory(response.coupon);
      
      // Shfaq njoftim
      showNotification(`Kodi ${response.coupon.code} u aplikua me sukses!`);
    } else {
      setStatus(elements, 'error', response?.error || 'Nuk u gjet kupon për auto-apply');
    }
  } catch (err) {
    console.error('Gabim:', err);
    setStatus(elements, 'error', 'Gabim gjatë aplikimit');
  } finally {
    setLoading(elements, false);
  }
}

// ==================== RENDERIMI I KUPONAVE ====================

function renderCoupons(elements) {
  const { couponList, emptyState, countBadge } = elements;
  
  if (currentCoupons.length === 0) {
    couponList.innerHTML = '';
    emptyState.classList.remove('hidden');
    countBadge.textContent = '0';
    return;
  }
  
  emptyState.classList.add('hidden');
  countBadge.textContent = currentCoupons.length;
  
  couponList.innerHTML = currentCoupons.map((coupon, index) => `
    <div class="coupon-item" data-index="${index}" data-code="${coupon.code}">
      <div class="coupon-icon">${getCouponIcon(coupon.type)}</div>
      <div class="coupon-info">
        <div class="coupon-code">${escapeHtml(coupon.code)}</div>
        <div class="coupon-discount">${escapeHtml(coupon.discount || 'Zbritje')}</div>
      </div>
      <span class="coupon-type ${coupon.type || 'percentage'}">${getTypeLabel(coupon.type)}</span>
      <div class="coupon-action">
        <span>Kliko për të kopjuar</span>
      </div>
    </div>
  `).join('');
  
  // Lidh eventet e kuponave
  couponList.querySelectorAll('.coupon-item').forEach(item => {
    item.addEventListener('click', () => copyCoupon(item, elements));
  });
}

function getCouponIcon(type) {
  switch (type) {
    case 'percentage': return '💸';
    case 'fixed': return '💰';
    case 'shipping': return '🚚';
    default: return '🏷️';
  }
}

function getTypeLabel(type) {
  switch (type) {
    case 'percentage': return '%';
    case 'fixed': return '$';
    case 'shipping': return 'FALAS';
    default: return 'KUPON';
  }
}

// ==================== KOPJIMI I KUPONIT ====================

async function copyCoupon(item, elements) {
  const code = item.dataset.code;
  
  try {
    await navigator.clipboard.writeText(code);
    
    // Shfaq efektin e kopjimit
    item.classList.add('copied');
    const originalText = item.querySelector('.coupon-action span');
    originalText.textContent = '✓ U kopjua!';
    
    setTimeout(() => {
      item.classList.remove('copied');
      originalText.textContent = 'Kliko për të kopjuar';
    }, 1500);
    
    // Dërgo mesazh për të aplikuar kuponin
    const coupon = currentCoupons.find(c => c.code === code);
    if (coupon) {
      await addToHistory(coupon);
    }
    
  } catch (err) {
    console.error('Gabim gjatë kopjimit:', err);
    setStatus(elements, 'error', 'Nuk u kopjua');
  }
}

// ==================== FILTERI I KUPONAVE ====================

function filterCoupons(query, elements) {
  const { couponList, emptyState, countBadge, searchClear } = elements;
  
  if (query) {
    searchClear.classList.remove('hidden');
  } else {
    searchClear.classList.add('hidden');
  }
  
  const filtered = currentCoupons.filter(coupon => {
    const searchStr = `${coupon.code} ${coupon.discount}`.toLowerCase();
    return searchStr.includes(query.toLowerCase());
  });
  
  if (filtered.length === 0) {
    couponList.innerHTML = '';
    emptyState.classList.remove('hidden');
    emptyState.querySelector('.empty-title').textContent = 'Asnjë rezultat';
    emptyState.querySelector('.empty-sub').textContent = `Provoni një kërkim tjetër`;
    countBadge.textContent = '0';
  } else {
    emptyState.classList.add('hidden');
    countBadge.textContent = filtered.length;
    
    couponList.innerHTML = filtered.map((coupon, index) => `
      <div class="coupon-item" data-code="${coupon.code}">
        <div class="coupon-icon">${getCouponIcon(coupon.type)}</div>
        <div class="coupon-info">
          <div class="coupon-code">${escapeHtml(coupon.code)}</div>
          <div class="coupon-discount">${escapeHtml(coupon.discount || 'Zbritje')}</div>
        </div>
        <span class="coupon-type ${coupon.type || 'percentage'}">${getTypeLabel(coupon.type)}</span>
        <div class="coupon-action">
          <span>Kliko për të kopjuar</span>
        </div>
      </div>
    `).join('');
    
    // Lidh eventet
    couponList.querySelectorAll('.coupon-item').forEach(item => {
      item.addEventListener('click', () => copyCoupon(item, elements));
    });
  }
}

// ==================== HISTORIKU ====================

async function showHistory(elements) {
  const { historyList, historyEmpty } = elements;
  
  try {
    const result = await chrome.storage.local.get('history');
    const history = result.history || [];
    
    if (history.length === 0) {
      historyList.innerHTML = '';
      historyEmpty.classList.remove('hidden');
    } else {
      historyEmpty.classList.add('hidden');
      
      historyList.innerHTML = history.slice(0, 50).map(item => `
        <div class="history-item">
          <div class="history-icon">${getCouponIcon(item.type)}</div>
          <div class="history-info">
            <div class="history-domain">${escapeHtml(item.domain)}</div>
            <div class="history-code">${escapeHtml(item.code)}</div>
            ${item.discount ? `<div class="history-time">${escapeHtml(item.discount)}</div>` : ''}
            <div class="history-time">${formatTime(item.timestamp)}</div>
          </div>
        </div>
      `).join('');
    }
    
    showPanel(elements.historyPanel);
  } catch (err) {
    console.error('Gabim gjatë ngarkimit të historisë:', err);
  }
}

async function addToHistory(coupon) {
  try {
    await chrome.runtime.sendMessage({
      action: 'addToHistory',
      domain: currentDomain,
      coupon: coupon
    });
  } catch (err) {
    console.error('Gabim gjatë shtimit në histori:', err);
  }
}

async function clearHistory(elements) {
  if (confirm('Dëshiron të fshish të gjithë historinë?')) {
    try {
      await chrome.storage.local.set({ history: [] });
      elements.historyList.innerHTML = '';
      elements.historyEmpty.classList.remove('hidden');
    } catch (err) {
      console.error('Gabim gjatë pastrimit të historisë:', err);
    }
  }
}

// ==================== PANELS ====================

function showPanel(panel) {
  panel.classList.remove('hidden');
}

function hidePanel(panel) {
  panel.classList.add('hidden');
}

function hideAllPanels(elements) {
  hidePanel(elements.historyPanel);
  hidePanel(elements.helpPanel);
}

// ==================== UI HELPERS ====================

function setStatus(elements, type, text) {
  elements.statusBar.className = `status ${type}`;
  elements.statusText.textContent = text;
}

function setLoading(elements, loading, text) {
  isLoading = loading;
  
  if (loading) {
    elements.scanBtn.disabled = true;
    elements.autoApplyBtn.disabled = true;
    elements.loadingState.classList.remove('hidden');
    elements.emptyState.classList.add('hidden');
    
    if (text) {
      setStatus(elements, 'loading', text);
    }
  } else {
    elements.scanBtn.disabled = false;
    elements.autoApplyBtn.disabled = false;
    elements.loadingState.classList.add('hidden');
  }
}

function updateUI(elements) {
  if (currentDomain) {
    elements.statusText.textContent = `Gati për të kërkuar në ${currentDomain}`;
  } else {
    elements.statusText.textContent = 'Gati për të kërkuar';
  }
}

// ==================== SETTINGS ====================

async function loadSettings(elements) {
  try {
    const result = await chrome.storage.sync.get('settings');
    const settings = result.settings || {};
    
    // Përditëso UI bazuar në settings
    if (settings.autoApply) {
      elements.autoApplyBtn.classList.add('active');
    } else {
      elements.autoApplyBtn.classList.remove('active');
    }
  } catch (err) {
    console.error('Gabim gjatë ngarkimit të settings:', err);
  }
}

// ==================== NOTIFIKIME ====================

function showNotification(message) {
  // Krijo njoftim të personalizuar
  const notification = document.createElement('div');
  notification.className = 'autocoupon-notification';
  notification.innerHTML = `
    <div class="notification-icon">✓</div>
    <div class="notification-text">${escapeHtml(message)}</div>
  `;
  
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    left: 50%;
    transform: translateX(-50%);
    background: #10b981;
    color: white;
    padding: 12px 20px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 14px;
    font-weight: 600;
    box-shadow: 0 4px 20px rgba(16, 185, 129, 0.4);
    z-index: 1000;
    animation: slideDown 0.3s ease;
  `;
  
  document.body.appendChild(notification);
  
  setTimeout(() => {
    notification.style.animation = 'slideUp 0.3s ease';
    setTimeout(() => notification.remove(), 300);
  }, 2000);
}

// ==================== UTILITY FUNCTIONS ====================

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

function formatTime(timestamp) {
  const date = new Date(timestamp);
  const now = new Date();
  const diff = now - date;
  
  if (diff < 60000) return 'Tani';
  if (diff < 3600000) return `${Math.floor(diff / 60000)} min më parë`;
  if (diff < 86400000) return `${Math.floor(diff / 3600000)} orë më parë`;
  if (diff < 604800000) return `${Math.floor(diff / 86400000)} ditë më parë`;
  
  return date.toLocaleDateString('sq-AL');
}

// Shto animacionet CSS
const style = document.createElement('style');
style.textContent = `
  @keyframes slideDown {
    from { opacity: 0; transform: translate(-50%, -20px); }
    to { opacity: 1; transform: translate(-50%, 0); }
  }
  @keyframes slideUp {
    from { opacity: 1; transform: translate(-50%, 0); }
    to { opacity: 0; transform: translate(-50%, -20px); }
  }
`;
document.head.appendChild(style);

// ==================== EARNINGS ====================

async function loadEarnings() {
  try {
    // Merr statistikat nga background
    const response = await chrome.runtime.sendMessage({ action: 'getAffiliateStats' });
    
    if (response && response.ok) {
      const stats = response.stats;
      
      // Përditëso UI
      document.getElementById('totalEarnings').textContent = `$${stats.totalEarnings.toFixed(2)}`;
      document.getElementById('monthEarnings').textContent = `$${stats.thisMonthEarnings.toFixed(2)}`;
      document.getElementById('totalClicks').textContent = stats.totalClicks;
      document.getElementById('totalConversions').textContent = stats.totalConversions;
      document.getElementById('conversionRate').textContent = `${stats.conversionRate}%`;
    }
    
    // Shfaq partnerët
    showPartners();
  } catch (err) {
    console.error('Gabim gjatë ngarkimit të të ardhurave:', err);
  }
}

function showPartners() {
  const partners = [
    { name: 'Amazon', commission: '5%' },
    { name: 'eBay', commission: '4%' },
    { name: 'Etsy', commission: '4%' },
    { name: 'Walmart', commission: '3%' },
    { name: 'Target', commission: '4%' },
    { name: 'Best Buy', commission: '3%' },
    { name: 'Nike', commission: '5%' },
    { name: 'AliExpress', commission: '6%' }
  ];
  
  const container = document.getElementById('partnersList');
  if (container) {
    container.innerHTML = partners.map(p => `
      <div class="partner-item">
        <span class="partner-name">${p.name}</span>
        <span class="partner-commission">${p.commission}</span>
      </div>
    `).join('');
  }
}
