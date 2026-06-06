// AutoCoupon Storage Module
// Menaxhon të dhënat me Chrome Storage API

const AutoCouponStorage = {
  // ==================== SETTINGS ====================

  async getSettings() {
    try {
      const { settings } = await chrome.storage.sync.get('settings');
      return {
        autoApply: true,
        notifications: true,
        theme: 'light',
        source: 'auto',
        language: 'sq',
        minCodeLength: 4,
        maxCodeLength: 20,
        autoScan: true,
        ...settings
      };
    } catch (err) {
      console.error('Storage: Gabim gjatë marrjes së settings:', err);
      return this.getDefaultSettings();
    }
  },

  getDefaultSettings() {
    return {
      autoApply: true,
      notifications: true,
      theme: 'light',
      source: 'auto',
      language: 'sq',
      minCodeLength: 4,
      maxCodeLength: 20,
      autoScan: true
    };
  },

  async saveSettings(settings) {
    try {
      const current = await this.getSettings();
      const updated = { ...current, ...settings };
      await chrome.storage.sync.set({ settings: updated });
      return { success: true, settings: updated };
    } catch (err) {
      console.error('Storage: Gabim gjatë ruajtjes së settings:', err);
      return { success: false, error: err.message };
    }
  },

  async updateSetting(key, value) {
    try {
      const settings = await this.getSettings();
      settings[key] = value;
      await chrome.storage.sync.set({ settings });
      return { success: true };
    } catch (err) {
      console.error('Storage: Gabim gjatë përditësimit:', err);
      return { success: false, error: err.message };
    }
  },

  // ==================== TRUSTED DOMAINS ====================

  async getTrustedDomains() {
    try {
      const { trustedDomains } = await chrome.storage.sync.get('trustedDomains');
      return trustedDomains || this.getDefaultTrustedDomains();
    } catch (err) {
      console.error('Storage: Gabim gjatë marrjes së domain-eve:', err);
      return this.getDefaultTrustedDomains();
    }
  },

  getDefaultTrustedDomains() {
    return [
      'amazon.com', 'ebay.com', 'etsy.com',
      'walmart.com', 'target.com', 'bestbuy.com',
      'shopify.com', 'nike.com', 'macys.com',
      'aliexpress.com', 'costco.com', 'bestbuy.com'
    ];
  },

  async addTrustedDomain(domain) {
    try {
      const domains = await this.getTrustedDomains();
      const cleanDomain = this.cleanDomain(domain);
      
      if (!domains.includes(cleanDomain)) {
        domains.push(cleanDomain);
        await chrome.storage.sync.set({ trustedDomains: domains });
      }
      return { success: true, domains };
    } catch (err) {
      console.error('Storage: Gabim gjatë shtimit:', err);
      return { success: false, error: err.message };
    }
  },

  async removeTrustedDomain(domain) {
    try {
      let domains = await this.getTrustedDomains();
      domains = domains.filter(d => d !== this.cleanDomain(domain));
      await chrome.storage.sync.set({ trustedDomains: domains });
      return { success: true, domains };
    } catch (err) {
      console.error('Storage: Gabim gjatë heqjes:', err);
      return { success: false, error: err.message };
    }
  },

  async isTrustedDomain(domain) {
    const domains = await this.getTrustedDomains();
    const cleanDomain = this.cleanDomain(domain);
    return domains.some(d => cleanDomain.includes(d) || d.includes(cleanDomain));
  },

  // ==================== BLACKLIST ====================

  async getBlacklist() {
    try {
      const { blacklist } = await chrome.storage.sync.get('blacklist');
      return blacklist || [];
    } catch (err) {
      console.error('Storage: Gabim gjatë marrjes së listës së zezë:', err);
      return [];
    }
  },

  async addToBlacklist(domain) {
    try {
      const blacklist = await this.getBlacklist();
      const cleanDomain = this.cleanDomain(domain);
      
      if (!blacklist.includes(cleanDomain)) {
        blacklist.push(cleanDomain);
        await chrome.storage.sync.set({ blacklist });
      }
      return { success: true, blacklist };
    } catch (err) {
      console.error('Storage: Gabim gjatë shtimit:', err);
      return { success: false, error: err.message };
    }
  },

  async removeFromBlacklist(domain) {
    try {
      let blacklist = await this.getBlacklist();
      blacklist = blacklist.filter(d => d !== this.cleanDomain(domain));
      await chrome.storage.sync.set({ blacklist });
      return { success: true, blacklist };
    } catch (err) {
      console.error('Storage: Gabim gjatë heqjes:', err);
      return { success: false, error: err.message };
    }
  },

  async isBlacklisted(domain) {
    const blacklist = await this.getBlacklist();
    const cleanDomain = this.cleanDomain(domain);
    return blacklist.some(d => cleanDomain.includes(d) || d.includes(cleanDomain));
  },

  // ==================== HISTORY ====================

  async getHistory(limit = 100) {
    try {
      const { history } = await chrome.storage.local.get('history');
      const list = history || [];
      return list.slice(0, limit);
    } catch (err) {
      console.error('Storage: Gabim gjatë marrjes së historisë:', err);
      return [];
    }
  },

  async getHistoryCount() {
    try {
      const { history } = await chrome.storage.local.get('history');
      return (history || []).length;
    } catch (err) {
      return 0;
    }
  },

  async addToHistory(entry) {
    try {
      const { history } = await chrome.storage.local.get('history');
      const list = history || [];
      
      list.unshift({
        domain: entry.domain || '',
        code: entry.code || '',
        discount: entry.discount || '',
        type: entry.type || 'unknown',
        source: entry.source || 'auto',
        success: entry.success !== false,
        timestamp: Date.now(),
        id: this.generateId()
      });

      // Mbaj vetëm 500 elementët e fundit
      if (list.length > 500) {
        list.length = 500;
      }

      await chrome.storage.local.set({ history: list });
      return { success: true, count: list.length };
    } catch (err) {
      console.error('Storage: Gabim gjatë shtimit:', err);
      return { success: false, error: err.message };
    }
  },

  async clearHistory() {
    try {
      await chrome.storage.local.set({ history: [] });
      return { success: true };
    } catch (err) {
      console.error('Storage: Gabim gjatë pastrimit:', err);
      return { success: false, error: err.message };
    }
  },

  async getHistoryByDomain(domain) {
    try {
      const history = await this.getHistory(1000);
      return history.filter(h => h.domain === this.cleanDomain(domain));
    } catch (err) {
      return [];
    }
  },

  async getSuccessfulCoupons() {
    try {
      const history = await this.getHistory(1000);
      return history.filter(h => h.success);
    } catch (err) {
      return [];
    }
  },

  // ==================== COUPON CACHE ====================

  async getCouponCache() {
    try {
      const { couponCache } = await chrome.storage.local.get('couponCache');
      return couponCache || {};
    } catch (err) {
      console.error('Storage: Gabim gjatë marrjes së cache:', err);
      return {};
    }
  },

  async getCachedCoupons(domain) {
    try {
      const cache = await this.getCouponCache();
      const cleanDomain = this.cleanDomain(domain);
      const cached = cache[cleanDomain];
      
      if (cached && cached.expires > Date.now()) {
        return { success: true, coupons: cached.codes, fromCache: true };
      }
      
      return { success: true, coupons: [], fromCache: false };
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  async setCachedCoupons(domain, coupons, ttlHours = 6) {
    try {
      const cache = await this.getCouponCache();
      const cleanDomain = this.cleanDomain(domain);
      
      cache[cleanDomain] = {
        codes: coupons,
        expires: Date.now() + (ttlHours * 60 * 60 * 1000),
        timestamp: Date.now()
      };

      await chrome.storage.local.set({ couponCache: cache });
      return { success: true };
    } catch (err) {
      console.error('Storage: Gabim gjatë ruajtjes:', err);
      return { success: false, error: err.message };
    }
  },

  async clearExpiredCache() {
    try {
      const cache = await this.getCouponCache();
      const now = Date.now();
      const cleaned = {};
      
      for (const [domain, data] of Object.entries(cache)) {
        if (data.expires > now) {
          cleaned[domain] = data;
        }
      }

      await chrome.storage.local.set({ couponCache: cleaned });
      return { success: true, removed: Object.keys(cache).length - Object.keys(cleaned).length };
    } catch (err) {
      console.error('Storage: Gabim gjatë pastrimit:', err);
      return { success: false, error: err.message };
    }
  },

  async getCacheStats() {
    try {
      const cache = await this.getCouponCache();
      const now = Date.now();
      let valid = 0;
      let expired = 0;
      
      for (const data of Object.values(cache)) {
        if (data.expires > now) {
          valid++;
        } else {
          expired++;
        }
      }

      return {
        total: Object.keys(cache).length,
        valid,
        expired
      };
    } catch (err) {
      return { total: 0, valid: 0, expired: 0 };
    }
  },

  // ==================== STATISTICS ====================

  async getStats() {
    try {
      const { stats } = await chrome.storage.local.get('stats');
      return {
        totalScanned: 0,
        totalApplied: 0,
        successfulApplied: 0,
        failedApplied: 0,
        couponsFound: 0,
        domainsVisited: 0,
        lastUsed: null,
        firstUsed: Date.now(),
        ...stats
      };
    } catch (err) {
      return this.getDefaultStats();
    }
  },

  getDefaultStats() {
    return {
      totalScanned: 0,
      totalApplied: 0,
      successfulApplied: 0,
      failedApplied: 0,
      couponsFound: 0,
      domainsVisited: 0,
      lastUsed: null,
      firstUsed: Date.now()
    };
  },

  async updateStats(update) {
    try {
      const stats = await this.getStats();
      
      if (update.scanned) stats.totalScanned++;
      if (update.applied) stats.totalApplied++;
      if (update.success) stats.successfulApplied++;
      if (update.failed) stats.failedApplied++;
      if (update.couponsFound) stats.couponsFound += update.couponsFound;
      if (update.domainVisited) stats.domainsVisited++;
      
      stats.lastUsed = Date.now();
      
      await chrome.storage.local.set({ stats });
      return { success: true, stats };
    } catch (err) {
      console.error('Storage: Gabim gjatë përditësimit:', err);
      return { success: false, error: err.message };
    }
  },

  async getSuccessRate() {
    try {
      const stats = await this.getStats();
      if (stats.totalApplied === 0) return 0;
      return Math.round((stats.successfulApplied / stats.totalApplied) * 100);
    } catch (err) {
      return 0;
    }
  },

  // ==================== FAVORITES ====================

  async getFavorites() {
    try {
      const { favorites } = await chrome.storage.local.get('favorites');
      return favorites || [];
    } catch (err) {
      return [];
    }
  },

  async addToFavorites(coupon) {
    try {
      const favorites = await this.getFavorites();
      const exists = favorites.some(f => f.code === coupon.code && f.domain === coupon.domain);
      
      if (!exists) {
        favorites.unshift({
          ...coupon,
          addedAt: Date.now(),
          id: this.generateId()
        });
        
        // Mbaj vetëm 100 favoritet
        if (favorites.length > 100) {
          favorites.length = 100;
        }
        
        await chrome.storage.local.set({ favorites });
      }
      
      return { success: true, favorites };
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  async removeFromFavorites(code, domain) {
    try {
      let favorites = await this.getFavorites();
      favorites = favorites.filter(f => !(f.code === code && f.domain === domain));
      await chrome.storage.local.set({ favorites });
      return { success: true, favorites };
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  async isFavorite(code, domain) {
    const favorites = await this.getFavorites();
    return favorites.some(f => f.code === code && f.domain === domain);
  },

  // ==================== EXPORT / IMPORT ====================

  async exportData() {
    try {
      const sync = await chrome.storage.sync.get(null);
      const local = await chrome.storage.local.get(null);
      
      return {
        success: true,
        data: {
          version: '1.0.0',
          exportedAt: new Date().toISOString(),
          sync,
          local
        }
      };
    } catch (err) {
      console.error('Storage: Gabim gjatë export-it:', err);
      return { success: false, error: err.message };
    }
  },

  async exportDataAsJSON() {
    try {
      const result = await this.exportData();
      if (result.success) {
        return {
          success: true,
          json: JSON.stringify(result.data, null, 2)
        };
      }
      return result;
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  async importData(data) {
    try {
      if (typeof data === 'string') {
        data = JSON.parse(data);
      }

      // Validimi bazë
      if (!data || typeof data !== 'object') {
        return { success: false, error: 'Format i pavlefshëm' };
      }

      // Importo të dhënat
      if (data.sync) {
        await chrome.storage.sync.set(data.sync);
      }
      if (data.local) {
        await chrome.storage.local.set(data.local);
      }

      return { success: true };
    } catch (err) {
      console.error('Storage: Gabim gjatë import-it:', err);
      return { success: false, error: err.message };
    }
  },

  async validateImportData(data) {
    try {
      if (typeof data === 'string') {
        data = JSON.parse(data);
      }

      if (!data || typeof data !== 'object') {
        return { valid: false, error: 'Format i pavlefshëm' };
      }

      // Kontrollo strukturën
      if (data.sync && typeof data.sync !== 'object') {
        return { valid: false, error: 'Sync data e pavlefshme' };
      }

      if (data.local && typeof data.local !== 'object') {
        return { valid: false, error: 'Local data e pavlefshme' };
      }

      return { valid: true };
    } catch (err) {
      return { valid: false, error: 'JSON i pavlefshëm' };
    }
  },

  // ==================== BACKUP & RESTORE ====================

  async createBackup() {
    try {
      const result = await this.exportData();
      if (!result.success) return result;

      const backup = {
        ...result.data,
        backupId: this.generateId(),
        createdAt: new Date().toISOString()
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
      return { success: true, backupId: backup.backupId };
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  async getBackups() {
    try {
      const { backups } = await chrome.storage.local.get('backups');
      return (backups || []).map(b => ({
        id: b.backupId,
        date: b.createdAt
      }));
    } catch (err) {
      return [];
    }
  },

  async restoreBackup(backupId) {
    try {
      const { backups } = await chrome.storage.local.get('backups');
      const backup = (backups || []).find(b => b.backupId === backupId);
      
      if (!backup) {
        return { success: false, error: 'Backup nuk u gjet' };
      }

      return await this.importData(backup);
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  // ==================== RESET ====================

  async resetAll() {
    try {
      await chrome.storage.sync.clear();
      await chrome.storage.local.clear();
      
      // Rivendos defaults
      await this.saveSettings(this.getDefaultSettings());
      await chrome.storage.sync.set({ trustedDomains: this.getDefaultTrustedDomains() });
      
      return { success: true };
    } catch (err) {
      console.error('Storage: Gabim gjatë rivendosjes:', err);
      return { success: false, error: err.message };
    }
  },

  async resetSettings() {
    try {
      await this.saveSettings(this.getDefaultSettings());
      return { success: true };
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  async resetHistory() {
    try {
      await chrome.storage.local.set({ history: [] });
      return { success: true };
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  async resetCache() {
    try {
      await chrome.storage.local.set({ couponCache: {} });
      return { success: true };
    } catch (err) {
      return { success: false, error: err.message };
    }
  },

  // ==================== STORAGE INFO ====================

  async getStorageInfo() {
    try {
      const sync = await chrome.storage.sync.get(null);
      const local = await chrome.storage.local.get(null);
      
      const syncSize = new Blob([JSON.stringify(sync)]).size;
      const localSize = new Blob([JSON.stringify(local)]).size;
      
      return {
        sync: {
          bytes: syncSize,
          items: Object.keys(sync).length
        },
        local: {
          bytes: localSize,
          items: Object.keys(local).length
        },
        total: {
          bytes: syncSize + localSize,
          items: Object.keys(sync).length + Object.keys(local).length
        }
      };
    } catch (err) {
      return { sync: { bytes: 0, items: 0 }, local: { bytes: 0, items: 0 }, total: { bytes: 0, items: 0 } };
    }
  },

  // ==================== UTILITY FUNCTIONS ====================

  cleanDomain(domain) {
    if (!domain) return '';
    return domain
      .replace(/^https?:\/\//, '')
      .replace(/^www\./, '')
      .split('/')[0]
      .toLowerCase()
      .trim();
  },

  generateId() {
    return Date.now().toString(36) + Math.random().toString(36).slice(2, 8);
  },

  async isInitialized() {
    try {
      const { settings } = await chrome.storage.sync.get('settings');
      return !!settings;
    } catch (err) {
      return false;
    }
  },

  async initialize() {
    try {
      if (!(await this.isInitialized())) {
        await this.saveSettings(this.getDefaultSettings());
        await chrome.storage.sync.set({ 
          trustedDomains: this.getDefaultTrustedDomains(),
          blacklist: []
        });
        await chrome.storage.local.set({
          history: [],
          couponCache: {},
          stats: this.getDefaultStats(),
          favorites: []
        });
      }
      return { success: true };
    } catch (err) {
      return { success: false, error: err.message };
    }
  }
};

// Eksporto nëse është modul
if (typeof module !== 'undefined' && module.exports) {
  module.exports = AutoCouponStorage;
}
