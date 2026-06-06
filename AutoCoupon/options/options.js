// AutoCoupon Options Script
// Menaxhon faqen e opsioneve dhe configuration

let settings = {};
let blacklist = [];
let trustedDomains = [];

// ==================== INITIALIZIM ====================

document.addEventListener('DOMContentLoaded', async () => {
  await loadAll();
  bindEvents();
  loadStats();
});

// ==================== NGARKIMI I TË DHËNAVE ====================

async function loadAll() {
  try {
    const sync = await chrome.storage.sync.get(['settings', 'blacklist', 'trustedDomains']);
    
    settings = sync.settings || {
      autoApply: true,
      notifications: true,
      autoScan: true,
      theme: 'light',
      source: 'auto',
      language: 'sq'
    };
    
    blacklist = sync.blacklist || [];
    trustedDomains = sync.trustedDomains || [];
    
    updateUI();
  } catch (err) {
    console.error('Gabim gjatë ngarkimit:', err);
    showToast('Gabim gjatë ngarkimit të të dhënave', 'error');
  }
}

function updateUI() {
  // Settings
  document.getElementById('autoApplySwitch').classList.toggle('on', settings.autoApply);
  document.getElementById('notifSwitch').classList.toggle('on', settings.notifications);
  document.getElementById('autoScanSwitch').classList.toggle('on', settings.autoScan);
  document.getElementById('themeSelect').value = settings.theme || 'light';
  document.getElementById('sourceSelect').value = settings.source || 'auto';
  document.getElementById('languageSelect').value = settings.language || 'sq';
  
  // Tags
  renderTags('blacklistTags', blacklist, 'blacklist');
  renderTags('trustedTags', trustedDomains, 'trusted');
}

async function loadStats() {
  try {
    // Merr statistikat
    const { stats } = await chrome.storage.local.get('stats');
    const currentStats = stats || {};
    
    // Merr numrin e historisë
    const { history } = await chrome.storage.local.get('history');
    const historyCount = (history || []).length;
    
    // Përditëso UI
    document.getElementById('statApplied').textContent = currentStats.totalApplied || 0;
    document.getElementById('statSuccess').textContent = 
      currentStats.totalApplied ? 
        Math.round(((currentStats.successfulApplied || 0) / currentStats.totalApplied) * 100) + '%' : 
        '0%';
    document.getElementById('statDomains').textContent = currentStats.domainsVisited || 0;
    document.getElementById('statHistory').textContent = historyCount;
  } catch (err) {
    console.error('Gabim gjatë ngarkimit të statistikave:', err);
  }
}

// ==================== EVENT BINDING ====================

function bindEvents() {
  // Switches
  document.querySelectorAll('.switch').forEach(switchEl => {
    switchEl.addEventListener('click', function() {
      this.classList.toggle('on');
      const setting = this.dataset.setting;
      if (setting) {
        settings[setting] = this.classList.contains('on');
      }
    });
  });

  // Selects
  document.querySelectorAll('select').forEach(selectEl => {
    selectEl.addEventListener('change', function() {
      const setting = this.dataset.setting;
      if (setting) {
        settings[setting] = this.value;
      }
    });
  });

  // Blacklist
  document.getElementById('addBlacklistBtn').addEventListener('click', () => addToList('blacklist'));
  document.getElementById('blacklistInput').addEventListener('keydown', (e) => {
    if (e.key === 'Enter') addToList('blacklist');
  });

  // Trusted
  document.getElementById('addTrustedBtn').addEventListener('click', () => addToList('trusted'));
  document.getElementById('trustedInput').addEventListener('keydown', (e) => {
    if (e.key === 'Enter') addToList('trusted');
  });

  // Save
  document.getElementById('saveBtn')?.addEventListener('click', saveSettings);

  // Export
  document.getElementById('exportBtn').addEventListener('click', exportData);

  // Import
  document.getElementById('importBtn').addEventListener('click', importData);

  // Backup
  document.getElementById('backupBtn').addEventListener('click', createBackup);

  // Reset
  document.getElementById('resetAllBtn').addEventListener('click', resetAll);
}

// ==================== MENAXHIMI I LISTAVE ====================

function addToList(type) {
  const input = document.getElementById(type === 'blacklist' ? 'blacklistInput' : 'trustedInput');
  const domain = input.value.trim().toLowerCase()
    .replace(/^https?:\/\//, '')
    .replace(/\/.*$/, '');
  
  if (!domain) {
    showToast('Ju lutem shkruani një domain', 'error');
    return;
  }

  // Validimi
  if (!domain.includes('.')) {
    showToast('Domaini duhet të përmbajë "."', 'error');
    return;
  }

  const list = type === 'blacklist' ? blacklist : trustedDomains;
  
  if (list.includes(domain)) {
    showToast('Domaini ekziston tashmë', 'error');
    return;
  }

  list.push(domain);
  input.value = '';
  
  renderTags(
    type === 'blacklist' ? 'blacklistTags' : 'trustedTags',
    list,
    type
  );
  
  showToast(`Domaini ${domain} u shtua`);
}

function removeFromList(type, domain) {
  const list = type === 'blacklist' ? blacklist : trustedDomains;
  const index = list.indexOf(domain);
  
  if (index > -1) {
    list.splice(index, 1);
    renderTags(
      type === 'blacklist' ? 'blacklistTags' : 'trustedTags',
      list,
      type
    );
    showToast(`Domaini ${domain} u hoq`);
  }
}

function renderTags(containerId, items, type) {
  const container = document.getElementById(containerId);
  
  if (items.length === 0) {
    container.innerHTML = `<span class="tag-empty">Asnjë domain</span>`;
    return;
  }
  
  container.innerHTML = items.map(item => `
    <span class="tag">
      ${escapeHtml(item)}
      <span class="tag-remove" data-type="${type}" data-domain="${item}">&times;</span>
    </span>
  `).join('');
  
  // Lidh eventet
  container.querySelectorAll('.tag-remove').forEach(el => {
    el.addEventListener('click', () => {
      removeFromList(el.dataset.type, el.dataset.domain);
    });
  });
}

// ==================== RUAJTJA ====================

async function saveSettings() {
  try {
    await chrome.storage.sync.set({
      settings,
      blacklist,
      trustedDomains
    });
    
    showToast('Opsionet u ruajtën me sukses!');
  } catch (err) {
    console.error('Gabim gjatë ruajtjes:', err);
    showToast('Gabim gjatë ruajtjes', 'error');
  }
}

// ==================== EXPORT ====================

async function exportData() {
  try {
    const sync = await chrome.storage.sync.get(null);
    const local = await chrome.storage.local.get(null);
    
    const exportData = {
      version: '1.0.0',
      exportedAt: new Date().toISOString(),
      sync,
      local
    };
    
    const json = JSON.stringify(exportData, null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = url;
    a.download = `autocoupon-backup-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    showToast('Të dhënat u eksportuan me sukses!');
  } catch (err) {
    console.error('Gabim gjatë export-it:', err);
    showToast('Gabim gjatë export-it', 'error');
  }
}

// ==================== IMPORT ====================

async function importData() {
  const text = document.getElementById('importText').value.trim();
  
  if (!text) {
    showToast('Ju lutem ngjitni JSON për import', 'error');
    return;
  }
  
  try {
    const data = JSON.parse(text);
    
    // Validimi bazë
    if (!data || typeof data !== 'object') {
      showToast('Format i pavlefshëm', 'error');
      return;
    }
    
    // Importo të dhënat
    if (data.sync) {
      await chrome.storage.sync.set(data.sync);
    }
    if (data.local) {
      await chrome.storage.local.set(data.local);
    }
    
    // Përditëso UI
    await loadAll();
    
    document.getElementById('importText').value = '';
    showToast('Importimi u krye me sukses!');
  } catch (err) {
    console.error('Gabim gjatë import-it:', err);
    showToast('JSON i pavlefshëm. Kontrollo formatin.', 'error');
  }
}

// ==================== BACKUP ====================

async function createBackup() {
  try {
    const sync = await chrome.storage.sync.get(null);
    const local = await chrome.storage.local.get(null);
    
    const backup = {
      version: '1.0.0',
      type: 'backup',
      createdAt: new Date().toISOString(),
      sync,
      local
    };
    
    // Ruaj backup-in
    const { backups } = await chrome.storage.local.get('backups');
    const backupList = backups || [];
    backupList.unshift(backup);
    
    // Mbaj vetëm 5 backup-et e fundit
    if (backupList.length > 5) {
      backupList.length = 5;
    }
    
    await chrome.storage.local.set({ backups: backupList });
    
    // Eksporto backup-in
    const json = JSON.stringify(backup, null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = url;
    a.download = `autocoupon-backup-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    showToast('Backup u krijua me sukses!');
  } catch (err) {
    console.error('Gabim gjatë krijimit të backup:', err);
    showToast('Gabim gjatë krijimit të backup', 'error');
  }
}

// ==================== RESET ====================

async function resetAll() {
  if (!confirm('Kjo do të fshijë të gjitha të dhënat e AutoCoupon. Vazhdo?')) {
    return;
  }
  
  if (!confirm('Je i sigurt? Nuk mund të kthehet mbrapa.')) {
    return;
  }
  
  try {
    // Fshi të dhënat
    await chrome.storage.sync.clear();
    await chrome.storage.local.clear();
    
    // Rivendos defaults
    settings = {
      autoApply: true,
      notifications: true,
      autoScan: true,
      theme: 'light',
      source: 'auto',
      language: 'sq'
    };
    
    blacklist = [];
    trustedDomains = [
      'amazon.com', 'ebay.com', 'etsy.com',
      'walmart.com', 'target.com', 'bestbuy.com',
      'shopify.com', 'nike.com', 'macys.com'
    ];
    
    await chrome.storage.sync.set({
      settings,
      blacklist,
      trustedDomains
    });
    
    // Përditëso UI
    updateUI();
    loadStats();
    
    showToast('Të dhënat u rivendosën!');
  } catch (err) {
    console.error('Gabim gjatë rivendosjes:', err);
    showToast('Gabim gjatë rivendosjes', 'error');
  }
}

// ==================== TOAST ====================

function showToast(message, type = 'success') {
  const toast = document.getElementById('toast');
  toast.textContent = message;
  toast.className = 'toast show' + (type === 'error' ? ' error' : '');
  
  setTimeout(() => {
    toast.className = 'toast';
  }, 3000);
}

// ==================== UTILITY ====================

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}
